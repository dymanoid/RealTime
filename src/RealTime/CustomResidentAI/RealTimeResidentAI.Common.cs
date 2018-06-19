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
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is released");
                CitizenManager.ReleaseCitizen(citizenId);
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
                && BuildingManager.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.HealthCare)
            {
                return;
            }

            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Dead);
            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is dead, body should get serviced");
        }

        private bool ProcessCitizenArrested(ref TCitizen citizen)
        {
            switch (CitizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Moving:
                    return false;
                case Citizen.Location.Visit
                    when BuildingManager.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.PoliceDepartment:
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
                Log.Warning($"Teleporting {GetCitizenDesc(citizenId, ref citizen)} back home because they are sick but no building is specified");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return true;
            }

            if (currentLocation != Citizen.Location.Home && CitizenProxy.GetVehicle(ref citizen) != 0)
            {
                return true;
            }

            if (currentLocation == Citizen.Location.Visit)
            {
                ItemClass.Service service = BuildingManager.GetBuildingService(CitizenProxy.GetVisitBuilding(ref citizen));
                if (service == ItemClass.Service.HealthCare || service == ItemClass.Service.Disaster)
                {
                    return true;
                }
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is sick, trying to get to a hospital");
            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Sick);
            return true;
        }

        private void ProcessCitizenEvacuation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort building = CitizenProxy.GetCurrentBuilding(ref citizen);
            if (building != 0)
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is trying to find an evacuation place");
                residentAI.FindEvacuationPlace(instance, citizenId, building, residentAI.GetEvacuationReason(instance, building));
            }
        }

        private ResidentState GetCitizenState(ref TCitizen citizen)
        {
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
            if ((BuildingManager.GetBuildingFlags(currentBuilding) & Building.Flags.Evacuating) != 0)
            {
                return ResidentState.Evacuating;
            }

            switch (CitizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Home:
                    if ((CitizenProxy.GetFlags(ref citizen) & Citizen.Flags.MovingIn) != 0)
                    {
                        return ResidentState.LeftCity;
                    }

                    if (currentBuilding != 0)
                    {
                        return ResidentState.AtHome;
                    }

                    return ResidentState.Unknown;

                case Citizen.Location.Work:
                    return currentBuilding != 0
                        ? ResidentState.AtSchoolOrWork
                        : ResidentState.Unknown;

                case Citizen.Location.Visit:
                    if (currentBuilding == 0)
                    {
                        return ResidentState.Unknown;
                    }

                    switch (BuildingManager.GetBuildingService(currentBuilding))
                    {
                        case ItemClass.Service.Commercial:
                            if (CitizenProxy.GetWorkBuilding(ref citizen) != 0 && IsWorkDay
                                && TimeInfo.CurrentHour > Config.LunchBegin && !ShouldReturnFromSchoolOrWork(CitizenProxy.GetAge(ref citizen)))
                            {
                                return ResidentState.AtLunch;
                            }

                            if (BuildingManager.GetBuildingSubService(currentBuilding) == ItemClass.SubService.CommercialLeisure)
                            {
                                return ResidentState.AtLeisureArea;
                            }

                            return ResidentState.Shopping;

                        case ItemClass.Service.Beautification:
                            return ResidentState.AtLeisureArea;
                    }

                    return ResidentState.Visiting;

                case Citizen.Location.Moving:
                    ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
                    return homeBuilding != 0 && CitizenManager.GetTargetBuilding(CitizenProxy.GetInstance(ref citizen)) == homeBuilding
                        ? ResidentState.MovingHome
                        : ResidentState.MovingToTarget;

                default:
                    return ResidentState.Unknown;
            }
        }
    }
}
