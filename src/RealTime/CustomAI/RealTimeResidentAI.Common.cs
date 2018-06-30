// <copyright file="RealTimeResidentAI.Common.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenDead(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            Citizen.Location currentLocation = CitizenProxy.GetLocation(ref citizen);

            if (currentBuilding == 0 || (currentLocation == Citizen.Location.Moving && CitizenProxy.GetVehicle(ref citizen) == 0))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} is released");
                CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            if (currentLocation != Citizen.Location.Home && CitizenProxy.GetHomeBuilding(ref citizen) != 0)
            {
                CitizenProxy.SetHome(ref citizen, citizenId, 0);
            }

            if (currentLocation != Citizen.Location.Work && CitizenProxy.GetWorkBuilding(ref citizen) != 0)
            {
                CitizenProxy.SetWorkplace(ref citizen, citizenId, 0);
            }

            if (currentLocation != Citizen.Location.Visit && CitizenProxy.GetVisitBuilding(ref citizen) != 0)
            {
                CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
            }

            if (currentLocation == Citizen.Location.Moving || CitizenProxy.GetVehicle(ref citizen) != 0)
            {
                return;
            }

            if (currentLocation == Citizen.Location.Visit
                && BuildingMgr.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.HealthCare)
            {
                return;
            }

            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Dead);
            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} is dead, body should get serviced");
        }

        private bool ProcessCitizenArrested(ref TCitizen citizen)
        {
            switch (CitizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Moving:
                    return false;
                case Citizen.Location.Visit
                    when BuildingMgr.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.PoliceDepartment:
                    return true;
            }

            CitizenProxy.SetArrested(ref citizen, false);
            return false;
        }

        private bool ProcessCitizenSick(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            Citizen.Location currentLocation = CitizenProxy.GetLocation(ref citizen);
            if (currentLocation == Citizen.Location.Moving)
            {
                return false;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if (currentLocation != Citizen.Location.Home && currentBuilding == 0)
            {
                Log.Debug($"Teleporting {GetCitizenDesc(citizenId, ref citizen, false)} back home because they are sick but no building is specified");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return true;
            }

            if (currentLocation != Citizen.Location.Home && CitizenProxy.GetVehicle(ref citizen) != 0)
            {
                return true;
            }

            if (currentLocation == Citizen.Location.Visit)
            {
                ItemClass.Service service = BuildingMgr.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen));
                if (service == ItemClass.Service.HealthCare || service == ItemClass.Service.Disaster)
                {
                    return true;
                }
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} is sick, trying to get to a hospital");
            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Sick);
            return true;
        }

        private void ProcessCitizenEvacuation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort building = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (building != 0)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} is trying to find an evacuation place");
                residentAI.FindEvacuationPlace(instance, citizenId, building, residentAI.GetEvacuationReason(instance, building));
            }
        }

        private bool StartMovingToVisitBuilding(TAI instance, uint citizenId, ref TCitizen citizen, ushort visitBuilding, bool isVirtual)
        {
            if (visitBuilding == 0)
            {
                return false;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            CitizenProxy.SetVisitPlace(ref citizen, citizenId, visitBuilding);
            CitizenProxy.SetVisitBuilding(ref citizen, visitBuilding);
            if (isVirtual || currentBuilding == visitBuilding)
            {
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Visit);
            }
            else
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, currentBuilding, visitBuilding);
            }

            return true;
        }

        private ResidentState GetResidentState(ref TCitizen citizen)
        {
            if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.DummyTraffic))
            {
                return ResidentState.Ignored;
            }

            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            ItemClass.Service buildingService = BuildingMgr.GetBuildingService(currentBuilding);

            if (BuildingMgr.BuildingHasFlags(currentBuilding, Building.Flags.Evacuating)
                && buildingService != ItemClass.Service.Disaster)
            {
                return ResidentState.Evacuating;
            }

            switch (CitizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Home:
                    return currentBuilding != 0
                        ? ResidentState.AtHome
                        : ResidentState.Unknown;

                case Citizen.Location.Work:
                    if (buildingService == ItemClass.Service.Disaster && CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
                    {
                        return ResidentState.InShelter;
                    }

                    if (CitizenProxy.GetVisitBuilding(ref citizen) == currentBuilding)
                    {
                        // A citizen may visit their own work building (e.g. shopping)
                        goto case Citizen.Location.Visit;
                    }

                    return currentBuilding != 0
                        ? ResidentState.AtSchoolOrWork
                        : ResidentState.Unknown;

                case Citizen.Location.Visit:
                    if (currentBuilding == 0)
                    {
                        return ResidentState.Unknown;
                    }

                    switch (buildingService)
                    {
                        case ItemClass.Service.Commercial:
                            if (CitizenProxy.GetWorkBuilding(ref citizen) != 0 && IsWorkDay
                                && TimeInfo.CurrentHour > Config.LunchBegin && TimeInfo.CurrentHour < GetSpareTimeBeginHour(CitizenProxy.GetAge(ref citizen)))
                            {
                                return ResidentState.AtLunch;
                            }

                            if (BuildingMgr.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure)
                            {
                                return ResidentState.AtLeisureArea;
                            }

                            return ResidentState.Shopping;

                        case ItemClass.Service.Beautification:
                            return ResidentState.AtLeisureArea;

                        case ItemClass.Service.Disaster:
                            return ResidentState.InShelter;
                    }

                    return ResidentState.Visiting;

                case Citizen.Location.Moving:
                    ushort instanceId = CitizenProxy.GetInstance(ref citizen);
                    if (CitizenMgr.InstanceHasFlags(instanceId, CitizenInstance.Flags.OnTour | CitizenInstance.Flags.TargetIsNode, true))
                    {
                        return ResidentState.OnTour;
                    }

                    ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
                    return homeBuilding != 0 && CitizenMgr.GetTargetBuilding(instanceId) == homeBuilding
                        ? ResidentState.MovingHome
                        : ResidentState.MovingToTarget;

                default:
                    return ResidentState.Unknown;
            }
        }
    }
}
