// <copyright file="RealTimeResidentAI.Common.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<T>
    {
        private static string GetCitizenDesc(uint citizenId, ref Citizen citizen)
        {
            string employment = citizen.m_workBuilding == 0 ? "unempl." : "empl.";
            return $"Citizen {citizenId} ({employment}, {Citizen.GetAgeGroup(citizen.m_age)})";
        }

        private void ProcessCitizenDead(T instance, uint citizenId, ref Citizen citizen)
        {
            ushort currentBuilding = citizen.GetBuildingByLocation();

            if (currentBuilding == 0 || (citizen.CurrentLocation == Citizen.Location.Moving && citizen.m_vehicle == 0))
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is released");
                citizenManager.ReleaseCitizen(citizenId);
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
                && buildingManager.GetBuildingService(citizen.m_visitBuilding) == ItemClass.Service.HealthCare)
            {
                return;
            }

            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Dead);
            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is dead, body should get serviced");
        }

        private bool ProcessCitizenArrested(ref Citizen citizen)
        {
            switch (citizen.CurrentLocation)
            {
                case Citizen.Location.Moving:
                    return false;
                case Citizen.Location.Visit when buildingManager.GetBuildingService(citizen.m_visitBuilding) == ItemClass.Service.PoliceDepartment:
                    return true;
            }

            citizen.Arrested = false;
            return false;
        }

        private bool ProcessCitizenSick(T instance, uint citizenId, ref Citizen citizen)
        {
            if (citizen.CurrentLocation == Citizen.Location.Moving)
            {
                return false;
            }

            ushort currentBuilding = citizen.GetBuildingByLocation();

            if (citizen.CurrentLocation != Citizen.Location.Home && currentBuilding == 0)
            {
                Log.Warning($"Teleporting {GetCitizenDesc(citizenId, ref citizen)} back home because they are sick but no building is specified");
                citizen.CurrentLocation = Citizen.Location.Home;
                return true;
            }

            if (citizen.CurrentLocation != Citizen.Location.Home && citizen.m_vehicle != 0)
            {
                return true;
            }

            if (citizen.CurrentLocation == Citizen.Location.Visit)
            {
                ItemClass.Service service = buildingManager.GetBuildingService(citizen.m_visitBuilding);
                if (service == ItemClass.Service.HealthCare || service == ItemClass.Service.Disaster)
                {
                    return true;
                }
            }

            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is sick, trying to get to a hospital");
            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Sick);
            return true;
        }

        private void ProcessCitizenEvacuation(T instance, uint citizenId, ref Citizen citizen)
        {
            ushort building = citizen.GetBuildingByLocation();
            if (building != 0)
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is trying to find an evacuation place");
                residentAI.FindEvacuationPlace(instance, citizenId, building, residentAI.GetEvacuationReason(instance, building));
            }
        }

        private CitizenState GetCitizenState(ref Citizen citizen)
        {
            ushort currentBuilding = citizen.GetBuildingByLocation();
            if ((buildingManager.GetBuildingFlags(currentBuilding) & Building.Flags.Evacuating) != 0)
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

                    switch (buildingManager.GetBuildingService(currentBuilding))
                    {
                        case ItemClass.Service.Commercial:
                            if (citizen.m_workBuilding != 0 && IsWorkDay
                                && timeInfo.CurrentHour > config.LunchBegin && !ShouldReturnFromSchoolOrWork(Citizen.GetAgeGroup(citizen.Age)))
                            {
                                return CitizenState.AtLunch;
                            }

                            if (buildingManager.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure)
                            {
                                return CitizenState.AtLeisureArea;
                            }

                            return CitizenState.Shopping;

                        case ItemClass.Service.Beautification:
                            return CitizenState.AtLeisureArea;
                    }

                    return CitizenState.Visiting;

                case Citizen.Location.Moving:
                    return citizen.m_homeBuilding != 0 && citizenManager.GetTargetBuilding(citizen.m_instance) == citizen.m_homeBuilding
                        ? CitizenState.MovingHome
                        : CitizenState.MovingToTarget;

                default:
                    return CitizenState.Unknown;
            }
        }
    }
}
