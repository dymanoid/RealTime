// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using RealTime.Tools;

    internal static partial class RealTimeResidentAI
    {
        private const float LocalSearchDistance = 1000f;

        private static void ProcessCitizenAtHome(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen)
        {
            if (citizen.m_homeBuilding == 0)
            {
                Log.Debug($"WARNING: {CitizenInfo(citizenId, ref citizen)} is in corrupt state: at home with no home building. Releasing the poor citizen.");
                refs.CitizenMgr.ReleaseCitizen(citizenId);
                return;
            }

            if (citizen.m_vehicle != 0)
            {
                Log.Debug(
                    refs.SimMgr.m_currentGameTime,
                    $"WARNING: {CitizenInfo(citizenId, ref citizen)} is at home but vehicle = {citizen.m_vehicle}");
                return;
            }

            if (CitizenGoesWorking(instance, refs, citizenId, ref citizen))
            {
                return;
            }

            if (!DoRandomMove(instance))
            {
                return;
            }

            if (!CitizenGoesShopping(instance, refs, citizenId, ref citizen))
            {
                CitizenGoesRelaxing(instance, refs, citizenId, ref citizen);
            }
        }
    }
}
