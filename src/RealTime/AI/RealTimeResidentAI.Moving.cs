// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    internal static partial class RealTimeResidentAI
    {
        private static void ProcessCitizenMoving(ResidentAI instance, References refs, uint citizenId, ref Citizen citizen, bool mayCancel)
        {
            CitizenInstance.Flags flags = CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour;
            if (citizen.m_vehicle == 0 && citizen.m_instance == 0)
            {
                if (citizen.m_visitBuilding != 0)
                {
                    citizen.SetVisitplace(citizenId, 0, 0u);
                }

                citizen.CurrentLocation = Citizen.Location.Home;
                citizen.Arrested = false;
            }
            else if (citizen.m_instance != 0 && (refs.CitizenMgr.m_instances.m_buffer[citizen.m_instance].m_flags & flags) == flags)
            {
                if (refs.SimMgr.m_randomizer.Int32(40u) < 10 && citizen.m_homeBuilding != 0)
                {
                    citizen.m_flags &= ~Citizen.Flags.Evacuating;
                    instance.StartMoving(citizenId, ref citizen, 0, citizen.m_homeBuilding);
                }
            }
        }
    }
}
