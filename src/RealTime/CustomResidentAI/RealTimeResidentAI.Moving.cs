// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    internal sealed partial class RealTimeResidentAI<T>
    {
        private void ProcessCitizenMoving(T instance, uint citizenId, ref Citizen citizen, bool mayCancel)
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
            else if ((citizenManager.GetInstanceFlags(citizen.m_instance) & flags) == flags)
            {
                if (IsChance(AbandonTourChance) && citizen.m_homeBuilding != 0)
                {
                    citizen.m_flags &= ~Citizen.Flags.Evacuating;
                    residentAI.StartMoving(instance, citizenId, ref citizen, 0, citizen.m_homeBuilding);
                }
            }
        }
    }
}
