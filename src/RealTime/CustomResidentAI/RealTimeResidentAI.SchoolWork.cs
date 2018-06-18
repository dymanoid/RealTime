// <copyright file="RealTimeResidentAI.SchoolWork.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<T>
    {
        private void ProcessCitizenAtSchoolOrWork(T instance, uint citizenId, ref Citizen citizen)
        {
            if (citizen.m_workBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: at school/work with no work building. Teleporting home.");
                citizen.CurrentLocation = Citizen.Location.Home;
                return;
            }

            if (ShouldGoToLunch(Citizen.GetAgeGroup(citizen.Age)))
            {
                ushort currentBuilding = citizen.GetBuildingByLocation();
                Citizen.Location currentLocation = citizen.CurrentLocation;

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

            if (!ShouldReturnFromSchoolOrWork(Citizen.GetAgeGroup(citizen.Age)))
            {
                return;
            }

            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} leaves their workplace");

            if (!CitizenGoesShopping(instance, citizenId, ref citizen) && !CitizenGoesRelaxing(instance, citizenId, ref citizen))
            {
                residentAI.StartMoving(instance, citizenId, ref citizen, citizen.m_workBuilding, citizen.m_homeBuilding);
            }
        }

        private bool CitizenGoesWorking(T instance, uint citizenId, ref Citizen citizen)
        {
            if (!ShouldMoveToSchoolOrWork(citizen.m_workBuilding, citizen.GetBuildingByLocation(), Citizen.GetAgeGroup(citizen.Age)))
            {
                return false;
            }

            Log.Debug(timeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} is going from {citizen.GetBuildingByLocation()} ({citizen.CurrentLocation}) to school/work {citizen.m_workBuilding}");

            residentAI.StartMoving(instance, citizenId, ref citizen, citizen.m_homeBuilding, citizen.m_workBuilding);
            return true;
        }
    }
}
