// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<T>
    {
        private void ProcessCitizenAtHome(T instance, uint citizenId, ref Citizen citizen)
        {
            if (citizen.m_homeBuilding == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: at home with no home building. Releasing the poor citizen.");
                citizenManager.ReleaseCitizen(citizenId);
                return;
            }

            if (citizen.m_vehicle != 0)
            {
                Log.Debug(timeInfo.Now, $"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is at home but vehicle = {citizen.m_vehicle}");
                return;
            }

            if (CitizenGoesWorking(instance, citizenId, ref citizen))
            {
                return;
            }

            if (!residentAI.DoRandomMove(instance))
            {
                return;
            }

            if (!CitizenGoesShopping(instance, citizenId, ref citizen))
            {
                CitizenGoesRelaxing(instance, citizenId, ref citizen);
            }
        }
    }
}
