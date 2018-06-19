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
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            if (workBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: at school/work with no work building. Teleporting home.");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                return;
            }

            if (ShouldGoToLunch(CitizenProxy.GetAge(ref citizen)))
            {
                ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);
                Citizen.Location currentLocation = CitizenProxy.GetLocation(ref citizen);

                ushort lunchPlace = MoveToCommercialBuilding(instance, citizenId, ref citizen, LocalSearchDistance);
                if (lunchPlace != 0)
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going for lunch from {currentBuilding} ({currentLocation}) to {lunchPlace}");
                }
                else
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} wanted to go for lunch from {currentBuilding} ({currentLocation}), but there were no buildings close enough");
                }

                return;
            }

            if (!ShouldReturnFromSchoolOrWork(CitizenProxy.GetAge(ref citizen)))
            {
                return;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} leaves their workplace");

            if (!CitizenGoesShopping(instance, citizenId, ref citizen) && !CitizenGoesRelaxing(instance, citizenId, ref citizen))
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, workBuilding, CitizenProxy.GetHomeBuilding(ref citizen));
            }
        }

        private bool CitizenGoesWorking(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
            ushort workBuilding = CitizenProxy.GetWorkBuilding(ref citizen);
            ushort currentBuilding = CitizenProxy.GetCurrentBuilding(ref citizen);

            if (!ShouldMoveToSchoolOrWork(workBuilding, currentBuilding, CitizenProxy.GetAge(ref citizen)))
            {
                return false;
            }

            Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {currentBuilding} to school/work {workBuilding}");

            residentAI.StartMoving(instance, citizenId, ref citizen, homeBuilding, workBuilding);
            return true;
        }
    }
}
