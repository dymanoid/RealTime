// <copyright file="RealTimeResidentAI.Common.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private string GetCitizenDesc(uint citizenId, ref TCitizen citizen)
        {
            string employment = citizenProxy.GetWorkBuilding(ref citizen) == 0 ? "unempl." : "empl.";
            return $"Citizen {citizenId} ({employment}, {citizenProxy.GetAge(ref citizen)})";
        }

        private void ProcessCitizenDead(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort currentBuilding = citizenProxy.GetCurrentBuilding(ref citizen);
            Citizen.Location currentLocation = citizenProxy.GetLocation(ref citizen);

            if (currentBuilding == 0 || (currentLocation == Citizen.Location.Moving && citizenProxy.GetVehicle(ref citizen) == 0))
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is released");
                citizenManager.ReleaseCitizen(citizenId);
                return;
            }

            if (currentLocation != Citizen.Location.Home && citizenProxy.GetHomeBuilding(ref citizen) != 0)
            {
                citizenProxy.SetHome(ref citizen, citizenId, 0);
            }

            if (currentLocation != Citizen.Location.Work && citizenProxy.GetWorkBuilding(ref citizen) != 0)
            {
                citizenProxy.SetWorkplace(ref citizen, citizenId, 0);
            }

            if (currentLocation != Citizen.Location.Visit && citizenProxy.GetVisitBuilding(ref citizen) != 0)
            {
                citizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
            }

            if (currentLocation == Citizen.Location.Moving || citizenProxy.GetVehicle(ref citizen) != 0)
            {
                return;
            }

            if (currentLocation == Citizen.Location.Visit
                && buildingManager.GetBuildingService(citizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.HealthCare)
            {
                return;
            }

            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Dead);
            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is dead, body should get serviced");
        }

        private bool ProcessCitizenArrested(ref TCitizen citizen)
        {
            switch (citizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Moving:
                    return false;
                case Citizen.Location.Visit
                    when buildingManager.GetBuildingService(citizenProxy.GetVisitBuilding(ref citizen)) == ItemClass.Service.PoliceDepartment:
                    return true;
            }

            citizenProxy.SetArrested(ref citizen, false);
            return false;
        }

        private bool ProcessCitizenSick(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            Citizen.Location currentLocation = citizenProxy.GetLocation(ref citizen);
            if (currentLocation == Citizen.Location.Moving)
            {
                return false;
            }

            ushort currentBuilding = citizenProxy.GetCurrentBuilding(ref citizen);

            if (currentLocation != Citizen.Location.Home && currentBuilding == 0)
            {
                Log.Warning($"Teleporting {GetCitizenDesc(citizenId, ref citizen)} back home because they are sick but no building is specified");
                citizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return true;
            }

            if (currentLocation != Citizen.Location.Home && citizenProxy.GetVehicle(ref citizen) != 0)
            {
                return true;
            }

            if (currentLocation == Citizen.Location.Visit)
            {
                ItemClass.Service service = buildingManager.GetBuildingService(citizenProxy.GetVisitBuilding(ref citizen));
                if (service == ItemClass.Service.HealthCare || service == ItemClass.Service.Disaster)
                {
                    return true;
                }
            }

            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is sick, trying to get to a hospital");
            residentAI.FindHospital(instance, citizenId, currentBuilding, TransferManager.TransferReason.Sick);
            return true;
        }

        private void ProcessCitizenEvacuation(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort building = citizenProxy.GetCurrentBuilding(ref citizen);
            if (building != 0)
            {
                Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is trying to find an evacuation place");
                residentAI.FindEvacuationPlace(instance, citizenId, building, residentAI.GetEvacuationReason(instance, building));
            }
        }

        private CitizenState GetCitizenState(ref TCitizen citizen)
        {
            ushort currentBuilding = citizenProxy.GetCurrentBuilding(ref citizen);
            if ((buildingManager.GetBuildingFlags(currentBuilding) & Building.Flags.Evacuating) != 0)
            {
                return CitizenState.Evacuating;
            }

            switch (citizenProxy.GetLocation(ref citizen))
            {
                case Citizen.Location.Home:
                    if ((citizenProxy.GetFlags(ref citizen) & Citizen.Flags.MovingIn) != 0)
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
                            if (citizenProxy.GetWorkBuilding(ref citizen) != 0 && IsWorkDay
                                && timeInfo.CurrentHour > config.LunchBegin && !ShouldReturnFromSchoolOrWork(citizenProxy.GetAge(ref citizen)))
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
                    ushort homeBuilding = citizenProxy.GetHomeBuilding(ref citizen);
                    return homeBuilding != 0 && citizenManager.GetTargetBuilding(citizenProxy.GetInstance(ref citizen)) == homeBuilding
                        ? CitizenState.MovingHome
                        : CitizenState.MovingToTarget;

                default:
                    return CitizenState.Unknown;
            }
        }
    }
}
