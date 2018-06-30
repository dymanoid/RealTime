// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenAtHome(TAI instance, uint citizenId, ref TCitizen citizen, bool isVirtual)
        {
            if (CitizenProxy.GetHomeBuilding(ref citizen) == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen, isVirtual)} is in corrupt state: at home with no home building. Releasing the poor citizen.");
                CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            ushort vehicle = CitizenProxy.GetVehicle(ref citizen);
            if (vehicle != 0)
            {
                Log.Debug(TimeInfo.Now, $"WARNING: {GetCitizenDesc(citizenId, ref citizen, isVirtual)} is at home but vehicle = {vehicle}");
                return;
            }

            if (CitizenGoesWorking(instance, citizenId, ref citizen, isVirtual))
            {
                return;
            }

            if (IsBusyAtHomeInTheMorning(CitizenProxy.GetAge(ref citizen)))
            {
                return;
            }

            if (CitizenGoesShopping(instance, citizenId, ref citizen, isVirtual) || CitizenGoesToEvent(instance, citizenId, ref citizen, isVirtual))
            {
                return;
            }

            CitizenGoesRelaxing(instance, citizenId, ref citizen, isVirtual);
        }

        private bool IsBusyAtHomeInTheMorning(Citizen.AgeGroup citizenAge)
        {
            float offset = IsWeekend ? 2 : 0;
            switch (citizenAge)
            {
                case Citizen.AgeGroup.Child:
                    return IsBusyAtHomeInTheMorning(8 + offset);

                case Citizen.AgeGroup.Teen:
                case Citizen.AgeGroup.Young:
                    return IsBusyAtHomeInTheMorning(9 + offset);

                case Citizen.AgeGroup.Adult:
                    return IsBusyAtHomeInTheMorning(8 + (offset / 2f));

                case Citizen.AgeGroup.Senior:
                    return IsBusyAtHomeInTheMorning(7);

                default:
                    return true;
            }

            bool IsBusyAtHomeInTheMorning(float latestHour)
            {
                float currentHour = TimeInfo.CurrentHour;
                if (currentHour >= latestHour || currentHour < TimeInfo.SunriseHour)
                {
                    return false;
                }

                float sunriseHour = TimeInfo.SunriseHour;
                float dx = latestHour - sunriseHour;
                float x = currentHour - sunriseHour;

                // A cubic probability curve from sunrise (0%) to latestHour (100%)
                uint chance = (uint)((100f / dx * x) - ((dx - x) * (dx - x) * x));
                return !IsChance(chance);
            }
        }
    }
}
