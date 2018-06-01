// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    internal static partial class RealTimeResidentAI
    {
        private static bool ProcessCitizenMoving(Arguments args, uint citizenID, ref Citizen data)
        {
            if (data.Dead)
            {
                if (data.m_vehicle == 0)
                {
                    args.CitizenMgr.ReleaseCitizen(citizenID);
                    return true;
                }

                if (data.m_homeBuilding != 0)
                {
                    data.SetHome(citizenID, 0, 0u);
                }

                if (data.m_workBuilding != 0)
                {
                    data.SetWorkplace(citizenID, 0, 0u);
                }

                if (data.m_visitBuilding != 0)
                {
                    data.SetVisitplace(citizenID, 0, 0u);
                }
            }
            else if (data.m_vehicle == 0 && data.m_instance == 0)
            {
                if (data.m_visitBuilding != 0)
                {
                    data.SetVisitplace(citizenID, 0, 0u);
                }

                data.CurrentLocation = Citizen.Location.Home;
                data.Arrested = false;
            }
            else if (data.m_instance != 0 && (args.CitizenMgr.m_instances.m_buffer[data.m_instance].m_flags & (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour)) == (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour))
            {
                int num = args.SimMgr.m_randomizer.Int32(40u);
                if (num < 10 && data.m_homeBuilding != 0)
                {
                    data.m_flags &= ~Citizen.Flags.Evacuating;
                    args.ResidentAI.StartMoving(citizenID, ref data, 0, data.m_homeBuilding);
                }
            }

            return false;
        }
    }
}
