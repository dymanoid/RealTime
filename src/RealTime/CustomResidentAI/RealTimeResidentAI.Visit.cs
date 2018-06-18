// <copyright file="RealTimeResidentAI.Visit.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<T>
    {
        private void ProcessCitizenVisit(T instance, CitizenState citizenState, uint citizenId, ref Citizen citizen)
        {
            if (citizen.m_visitBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: visiting with no visit building. Teleporting home.");
                citizen.CurrentLocation = Citizen.Location.Home;
                return;
            }

            switch (citizenState)
            {
                case CitizenState.AtLunch:
                    CitizenReturnsFromLunch(instance, citizenId, ref citizen);

                    return;

                case CitizenState.AtLeisureArea:
                case CitizenState.Visiting:
                    if (!CitizenGoesWorking(instance, citizenId, ref citizen))
                    {
                        CitizenReturnsHomeFromVisit(instance, citizenId, ref citizen);
                    }

                    return;

                case CitizenState.Shopping:
                    if (CitizenGoesWorking(instance, citizenId, ref citizen))
                    {
                        return;
                    }

                    if (IsChance(ReturnFromShoppingChance) || IsWorkDayMorning())
                    {
                        Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from shopping back home");
                        ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                        return;
                    }

                    break;

                default:
                    return;
            }

            if ((citizen.m_flags & Citizen.Flags.NeedGoods) != 0)
            {
                buildingManager.ModifyMaterialBuffer(citizen.m_visitBuilding, TransferManager.TransferReason.Shopping, -ShoppingGoodsAmount);
                citizen.m_flags &= ~Citizen.Flags.NeedGoods;
            }
        }

        private bool CitizenReturnsFromLunch(T instance, uint citizenId, ref Citizen citizen)
        {
            if (IsLunchHour)
            {
                return false;
            }

            if (citizen.m_workBuilding != 0)
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from lunch to {citizen.m_workBuilding}");
                ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_workBuilding);
            }
            else
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is at lunch but no work building. Teleporting home.");
                citizen.CurrentLocation = Citizen.Location.Home;
            }

            return true;
        }

        private bool CitizenReturnsHomeFromVisit(T instance, uint citizenId, ref Citizen citizen)
        {
            if (citizen.m_homeBuilding == 0 || citizen.m_vehicle != 0)
            {
                return false;
            }

            if (buildingManager.GetBuildingService(citizen.m_visitBuilding) == ItemClass.Service.Disaster)
            {
                if ((buildingManager.GetBuildingFlags(citizen.m_visitBuilding) & Building.Flags.Downgrading) != 0)
                {
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from evacuation place back home");
                    ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                    return true;
                }

                return false;
            }

            ushort eventId = buildingManager.GetEvent(citizen.m_visitBuilding);
            if (eventId != 0)
            {
                if ((eventManager.GetEventFlags(eventId) & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == 0)
                {
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from an event back home");
                    ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                }

                return true;
            }

            if (IsChance(ReturnFromVisitChance))
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} returning from visit back home");
                ReturnFromVisit(instance, citizenId, ref citizen, citizen.m_homeBuilding);
                return true;
            }

            return false;
        }

        private void ReturnFromVisit(T instance, uint citizenId, ref Citizen citizen, ushort targetBuilding)
        {
            if (targetBuilding != 0 && citizen.m_vehicle == 0)
            {
                citizen.m_flags &= ~Citizen.Flags.Evacuating;
                residentAI.StartMoving(instance, citizenId, ref citizen, citizen.m_visitBuilding, targetBuilding);
                citizen.SetVisitplace(citizenId, 0, 0u);
            }
        }

        private bool CitizenGoesShopping(T instance, uint citizenId, ref Citizen citizen)
        {
            if ((citizen.m_flags & Citizen.Flags.NeedGoods) == 0)
            {
                return false;
            }

            if (timeInfo.IsNightTime)
            {
                if (IsChance(GetGoOutChance(Citizen.GetAgeGroup(citizen.Age), false)))
                {
                    ushort localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping at night, trying local shop '{localVisitPlace}'");
                    return localVisitPlace > 0;
                }

                return false;
            }

            if (IsChance(GoShoppingChance))
            {
                bool localOnly = citizen.m_workBuilding != 0 && IsWorkDayMorning();
                ushort localVisitPlace = 0;

                if (IsChance(config.LocalBuildingSearchQuota))
                {
                    localVisitPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, tries a local shop '{localVisitPlace}'");
                }

                if (localVisitPlace == 0)
                {
                    if (localOnly)
                    {
                        Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, but didn't found a local shop");
                        return false;
                    }

                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna go shopping, heading to a random shop");
                    residentAI.FindVisitPlace(instance, citizenId, citizen.m_homeBuilding, residentAI.GetShoppingReason(instance));
                }

                return true;
            }

            return false;
        }

        private bool CitizenGoesRelaxing(T instance, uint citizenId, ref Citizen citizen)
        {
            // TODO: add events here
            if (!ShouldFindEntertainment(Citizen.GetAgeGroup(citizen.Age)))
            {
                return false;
            }

            ushort buildingId = citizen.GetBuildingByLocation();
            if (buildingId == 0)
            {
                return false;
            }

            if (timeInfo.IsNightTime)
            {
                ushort leisure = MoveToLeisure(instance, citizenId, ref citizen, buildingId);
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna relax at night, trying leisure area '{leisure}'");
            }
            else
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanna relax, heading to an entertainment place");
                residentAI.FindVisitPlace(instance, citizenId, buildingId, residentAI.GetEntertainmentReason(instance));
            }

            return true;
        }

        private ushort MoveToCommercialBuilding(T instance, uint citizenId, ref Citizen citizen, float distance)
        {
            ushort buildingId = citizen.GetBuildingByLocation();
            if (buildingId == 0)
            {
                return 0;
            }

            ushort foundBuilding = buildingManager.FindActiveBuilding(buildingId, distance, ItemClass.Service.Commercial);

            if (foundBuilding != 0)
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, buildingId, foundBuilding);
                citizen.SetVisitplace(citizenId, foundBuilding, 0U);
                citizen.m_visitBuilding = foundBuilding;
            }

            return foundBuilding;
        }

        private ushort MoveToLeisure(T instance, uint citizenId, ref Citizen citizen, ushort buildingId)
        {
            ushort leisureBuilding = buildingManager.FindActiveBuilding(
                buildingId,
                FullSearchDistance,
                ItemClass.Service.Commercial,
                ItemClass.SubService.CommercialLeisure);

            if (leisureBuilding != 0)
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, buildingId, leisureBuilding);
                citizen.SetVisitplace(citizenId, leisureBuilding, 0U);
                citizen.m_visitBuilding = leisureBuilding;
            }

            return leisureBuilding;
        }
    }
}
