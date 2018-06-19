// <copyright file="RealTimeResidentAI.SchoolWork.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenAtSchoolOrWork(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort workBuilding = citizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: at school/work with no work building. Teleporting home.");
                citizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            if (ShouldGoToLunch(citizenProxy.GetAge(ref citizen)))
            {
                ushort currentBuilding = citizenProxy.GetCurrentBuilding(ref citizen);
                Citizen.Location currentLocation = citizenProxy.GetLocation(ref citizen);

                ushort lunchPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                if (lunchPlace != 0)
                {
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going for lunch from {currentBuilding} ({currentLocation}) to {lunchPlace}");
                }
                else
                {
                    Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted to go for lunch from {currentBuilding} ({currentLocation}), but there were no buildings close enough");
                }

                return;
            }

            if (!ShouldReturnFromSchoolOrWork(citizenProxy.GetAge(ref citizen)))
            {
                return;
            }

            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} leaves their workplace");

            if (!CitizenGoesShopping(instance, citizenId, ref citizen) && !CitizenGoesRelaxing(instance, citizenId, ref citizen))
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, workBuilding, citizenProxy.GetHomeBuilding(ref citizen));
            }
        }

        private bool CitizenGoesWorking(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = citizenProxy.GetHomeBuilding(ref citizen);
            ushort workBuilding = citizenProxy.GetWorkBuilding(ref citizen);
            ushort currentBuilding = citizenProxy.GetCurrentBuilding(ref citizen);

            if (!ShouldMoveToSchoolOrWork(workBuilding, currentBuilding, citizenProxy.GetAge(ref citizen)))
            {
                return false;
            }

            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} to school/work {workBuilding}");

            residentAI.StartMoving(instance, citizenId, ref citizen, homeBuilding, workBuilding);
            return true;
        }
    }
}
