// <copyright file="RealTimeResidentAI.Visit.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using ColossalFramework;
    using RealTime.Tools;

    internal static partial class RealTimeResidentAI
    {
        private static void ProcessCitizenVisit(CitizenState citizenState, ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            if (citizen.m_visitBuilding == 0)
            {
                Log.Debug($"WARNING: {CitizenInfo(citizenId, ref citizen)} is in corrupt state: visiting with no visit building. Teleporting home.");
                citizen.CurrentLocation = Citizen.Location.Home;
                return;
            }

            switch (citizenState)
            {
                case CitizenState.AtLunch:
                    CitizenReturnsFromLunch(instance, refs, citizenId, ref citizen);

                    return;

                case CitizenState.AtLeisureArea:
                case CitizenState.Visiting:
                    if (!CitizenGoesWorking(instance, refs, citizenId, ref citizen))
                    {
                        CitizenReturnsHomeFromVisit(instance, refs, citizenId, ref citizen);
                    }

                    return;

                case CitizenState.Shopping:
                    if (CitizenGoesWorking(instance, refs, citizenId, ref citizen))
                    {
                        return;
                    }

                    if (refs.SimMgr.m_randomizer.Int32(40) < 10)
                    {
                        Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} returning from shopping back home");
                        ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                        return;
                    }

                    break;

                default:
                    return;
            }

            ref Building visitBuilding = ref refs.BuildingMgr.m_buildings.m_buffer[citizen.m_visitBuilding];
            if ((citizen.m_flags & Citizen.Flags.NeedGoods) != 0)
            {
                int goodsDelta = -100;
                visitBuilding.Info.m_buildingAI.ModifyMaterialBuffer(
                    citizen.m_visitBuilding,
                    ref visitBuilding,
                    TransferManager.TransferReason.Shopping,
                    ref goodsDelta);

                citizen.m_flags &= ~Citizen.Flags.NeedGoods;
                return;
            }
        }

        private static bool CitizenReturnsFromLunch(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            if (Logic.IsLunchHour)
            {
                return false;
            }

            if (citizen.m_workBuilding != 0)
            {
                Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} returning from lunch to {citizen.m_workBuilding}");
                ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_workBuilding);
            }
            else
            {
                Log.Debug($"WARNING: {CitizenInfo(citizenId, ref citizen)} is at lunch but no work building. Teleporting home.");
                citizen.CurrentLocation = Citizen.Location.Home;
            }

            return true;
        }

        private static bool CitizenReturnsHomeFromVisit(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            ref Building visitBuilding = ref refs.BuildingMgr.m_buildings.m_buffer[citizen.m_visitBuilding];
            switch (visitBuilding.Info.m_class.m_service)
            {
                case ItemClass.Service.HealthCare:
                case ItemClass.Service.PoliceDepartment:
                    if (refs.SimMgr.m_randomizer.Int32(100) < 50)
                    {
                        Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} returning from visit back home");
                        ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                        return true;
                    }

                    break;

                case ItemClass.Service.Disaster:
                    if ((visitBuilding.m_flags & Building.Flags.Downgrading) != 0)
                    {
                        Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} returning from evacuation place back home");
                        ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                        return true;
                    }

                    break;
            }

            ushort eventId = visitBuilding.m_eventIndex;
            if (eventId != 0)
            {
                if ((Singleton<EventManager>.instance.m_events.m_buffer[eventId].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == 0)
                {
                    Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} returning from an event back home");
                    ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                }

                return true;
            }

            return false;
        }

        private static void ReturnFromVisit(ResidentAI residentAi, uint citizenId, ref Citizen citizen, ushort targetBuilding)
        {
            if (targetBuilding != 0 && citizen.m_vehicle == 0)
            {
                citizen.m_flags &= ~Citizen.Flags.Evacuating;
                residentAi.StartMoving(citizenId, ref citizen, citizen.m_visitBuilding, targetBuilding);
                citizen.SetVisitplace(citizenId, 0, 0u);
            }
        }

        private static bool CitizenGoesShopping(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            if ((citizen.m_flags & Citizen.Flags.NeedGoods) == 0)
            {
                return false;
            }

            int random = refs.SimMgr.m_randomizer.Int32(100);

            if (refs.SimMgr.m_isNightTime)
            {
                if (random < Logic.GetGoOutAtNightChance(citizen.Age)
                    && FindLocalCommercialBuilding(instance, refs, citizenId, ref citizen, LocalSearchDistance) > 0)
                {
                    Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} wanna go shopping at night, heading to a local shop");
                    return true;
                }
            }
            else if (random < 50)
            {
                ushort localVisitPlace = 0;

                if (Logic.CurrentConfig.AllowLocalBuildingSearch
                    && refs.SimMgr.m_randomizer.UInt32(100) < Logic.CurrentConfig.LocalBuildingSearchQuota)
                {
                    Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} wanna go shopping, tries to find a local shop");
                    localVisitPlace = FindLocalCommercialBuilding(instance, refs, citizenId, ref citizen, LocalSearchDistance);
                }

                if (localVisitPlace == 0)
                {
                    Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} wanna go shopping, heading to a random shop");
                    FindVisitPlace(instance, citizenId, citizen.m_homeBuilding, GetShoppingReason(instance));
                }

                return true;
            }

            return false;
        }

        private static bool CitizenGoesRelaxing(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            // TODO: add events here
            if (!Logic.ShouldFindEntertainment(ref citizen))
            {
                return false;
            }

            ushort buildingId = citizen.GetBuildingByLocation();
            if (buildingId == 0)
            {
                return false;
            }

            if (refs.SimMgr.m_isNightTime)
            {
                Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} wanna relax at night, heading to a leisure area");
                FindLeisure(instance, refs, citizenId, ref citizen, buildingId);
            }
            else
            {
                Log.Debug(refs.SimMgr.m_currentGameTime, $"{CitizenInfo(citizenId, ref citizen)} wanna relax, heading to an entertainment place");
                FindVisitPlace(instance, citizenId, buildingId, GetEntertainmentReason(instance));
            }

            return true;
        }

        private static ushort FindLocalCommercialBuilding(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen, float distance)
        {
            ushort buildingId = citizen.GetBuildingByLocation();
            if (buildingId == 0)
            {
                return 0;
            }

            ushort foundBuilding = 0;
            ref Building currentBuilding = ref refs.BuildingMgr.m_buildings.m_buffer[buildingId];

            foundBuilding = refs.BuildingMgr.FindBuilding(
                currentBuilding.m_position,
                distance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.None,
                Building.Flags.Created | Building.Flags.Active,
                Building.Flags.Deleted);

            if (foundBuilding != 0)
            {
                instance.StartMoving(citizenId, ref citizen, buildingId, foundBuilding);
                citizen.SetVisitplace(citizenId, foundBuilding, 0U);
                citizen.m_visitBuilding = foundBuilding;
            }

            return foundBuilding;
        }

        private static void FindLeisure(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen, ushort buildingId)
        {
            ref Building currentBuilding = ref refs.BuildingMgr.m_buildings.m_buffer[buildingId];

            ushort leisureBuilding = refs.BuildingMgr.FindBuilding(
                currentBuilding.m_position,
                float.MaxValue,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialLeisure,
                Building.Flags.Created | Building.Flags.Active,
                Building.Flags.Deleted);

            if (leisureBuilding != 0 && refs.SimMgr.m_randomizer.Int32(10) > 2)
            {
                instance.StartMoving(citizenId, ref citizen, buildingId, leisureBuilding);
                citizen.SetVisitplace(citizenId, leisureBuilding, 0U);
                citizen.m_visitBuilding = leisureBuilding;
            }
        }
    }
}
