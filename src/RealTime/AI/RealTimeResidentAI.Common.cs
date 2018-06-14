// <copyright file="RealTimeResidentAI.Common.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using RealTime.Tools;

    internal static partial class RealTimeResidentAI
    {
        private static void ProcessCitizenDead(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            ushort currentBuilding = citizen.GetBuildingByLocation();

            if (currentBuilding == 0 || (citizen.CurrentLocation == Citizen.Location.Moving && citizen.m_vehicle == 0))
            {
                Log.Debug($"{CitizenInfo(citizenId, ref citizen)} is released");
                refs.CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            if (citizen.CurrentLocation != Citizen.Location.Home && citizen.m_homeBuilding != 0)
            {
                citizen.SetHome(citizenId, 0, 0u);
            }

            if (citizen.CurrentLocation != Citizen.Location.Work && citizen.m_workBuilding != 0)
            {
                citizen.SetWorkplace(citizenId, 0, 0u);
            }

            if (citizen.CurrentLocation != Citizen.Location.Visit && citizen.m_visitBuilding != 0)
            {
                citizen.SetVisitplace(citizenId, 0, 0u);
            }

            if (citizen.CurrentLocation == Citizen.Location.Moving || citizen.m_vehicle != 0)
            {
                return;
            }

            if (citizen.CurrentLocation == Citizen.Location.Visit
                &&
                refs.BuildingMgr.m_buildings.m_buffer[citizen.m_visitBuilding].Info.m_class.m_service == ItemClass.Service.HealthCare)
            {
                return;
            }

            FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Dead);
            Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} is dead, body should go to a cemetery");
        }

        private static bool ProcessCitizenArrested(References refs, ref Citizen citizen)
        {
            if (citizen.CurrentLocation == Citizen.Location.Moving)
            {
                return false;
            }

            if (citizen.CurrentLocation == Citizen.Location.Visit && citizen.m_visitBuilding != 0)
            {
                ref Building building = ref refs.BuildingMgr.m_buildings.m_buffer[citizen.m_visitBuilding];
                if (building.Info.m_class.m_service == ItemClass.Service.PoliceDepartment)
                {
                    return true;
                }
            }

            citizen.Arrested = false;
            return false;
        }

        private static bool ProcessCitizenSick(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            if (citizen.CurrentLocation == Citizen.Location.Moving)
            {
                return false;
            }

            ushort currentBuilding = citizen.GetBuildingByLocation();

            if (citizen.CurrentLocation != Citizen.Location.Home && currentBuilding == 0)
            {
                Log.Warning($"Teleporting {CitizenInfo(citizenId, ref citizen)} back home because they are sick but no building is specified");
                citizen.CurrentLocation = Citizen.Location.Home;
                return true;
            }

            if (citizen.CurrentLocation != Citizen.Location.Home && citizen.m_vehicle != 0)
            {
                return true;
            }

            if (citizen.CurrentLocation == Citizen.Location.Visit)
            {
                ItemClass.Service service = refs.BuildingMgr.m_buildings.m_buffer[citizen.m_visitBuilding].Info.m_class.m_service;
                if (service == ItemClass.Service.HealthCare || service == ItemClass.Service.Disaster)
                {
                    return true;
                }
            }

            Log.Debug($"{CitizenInfo(citizenId, ref citizen)} is sick, trying to get to a hospital");
            FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Sick);
            return true;
        }

        private static void ProcessCitizenEvacuation(ResidentAI residentAi, uint citizenId, ref Citizen citizen)
        {
            ushort building = citizen.GetBuildingByLocation();
            if (building != 0)
            {
                Log.Debug($"{CitizenInfo(citizenId, ref citizen)} is trying to find an evacuation place");
                FindEvacuationPlace(residentAi, citizenId, building, GetEvacuationReason(residentAi, building));
            }
        }

        private static CitizenState GetCitizenState(References refs, ref Citizen citizen)
        {
            ushort currentBuilding = citizen.GetBuildingByLocation();
            if (currentBuilding != 0 && (refs.BuildingMgr.m_buildings.m_buffer[currentBuilding].m_flags & Building.Flags.Evacuating) != 0)
            {
                return CitizenState.Evacuating;
            }

            switch (citizen.CurrentLocation)
            {
                case Citizen.Location.Home:
                    if ((citizen.m_flags & Citizen.Flags.MovingIn) != 0)
                    {
                        return CitizenState.LeftCity;
                    }

                    if (currentBuilding != 0)
                    {
                        return CitizenState.AtHome;
                    }

                    return CitizenState.Unknown;

                case Citizen.Location.Work:
                    return currentBuilding != 0
                        ? CitizenState.AtSchoolOrWork
                        : CitizenState.Unknown;

                case Citizen.Location.Visit:
                    if (currentBuilding == 0)
                    {
                        return CitizenState.Unknown;
                    }

                    ref Building building = ref refs.BuildingMgr.m_buildings.m_buffer[currentBuilding];
                    switch (building.Info.GetService())
                    {
                        case ItemClass.Service.Commercial:
                            if (citizen.m_workBuilding != 0
                                && Logic.IsWorkDayAndBetweenHours(Logic.CurrentConfig.LunchBegin, Logic.CurrentConfig.WorkEnd))
                            {
                                return CitizenState.AtLunch;
                            }

                            if (building.Info.GetSubService() == ItemClass.SubService.CommercialLeisure)
                            {
                                return CitizenState.AtLeisureArea;
                            }

                            return CitizenState.Shopping;

                        case ItemClass.Service.Beautification:
                            return CitizenState.AtLeisureArea;
                    }

                    return CitizenState.Visiting;

                case Citizen.Location.Moving:
                    if (citizen.m_instance != 0)
                    {
                        ref CitizenInstance instance = ref refs.CitizenMgr.m_instances.m_buffer[citizen.m_instance];
                        if ((instance.m_flags & CitizenInstance.Flags.TargetIsNode) != 0
                            && instance.m_targetBuilding == citizen.m_homeBuilding)
                        {
                            return CitizenState.MovingHome;
                        }
                    }

                    return CitizenState.MovingToTarget;

                default:
                    return CitizenState.Unknown;
            }
        }

        private static string CitizenInfo(uint id, ref Citizen citizen)
        {
            string employment = citizen.m_workBuilding == 0 ? "free" : "emp.";
            return $"Citizen {id} ({employment}, {Citizen.GetAgeGroup(citizen.m_age)})";
        }

        private sealed class References
        {
            public References(CitizenManager citizenMgr, BuildingManager buildingMgr, SimulationManager simMgr)
            {
                CitizenMgr = citizenMgr;
                BuildingMgr = buildingMgr;
                SimMgr = simMgr;
            }

            public CitizenManager CitizenMgr { get; }

            public BuildingManager BuildingMgr { get; }

            public SimulationManager SimMgr { get; }
        }
    }
}
