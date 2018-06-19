// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenMoving(TAI instance, uint citizenId, ref TCitizen citizen, bool mayCancel)
        {
            ushort instanceId = CitizenProxy.GetInstance(ref citizen);
            CitizenInstance.Flags flags = CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour;
            if (CitizenProxy.GetVehicle(ref citizen) == 0 && instanceId == 0)
            {
                if (CitizenProxy.GetVisitBuilding(ref citizen) != 0)
                {
                    CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                }

                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                CitizenProxy.SetArrested(ref citizen, false);
            }
            else if ((CitizenManager.GetInstanceFlags(instanceId) & flags) == flags)
            {
                ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
                if (IsChance(AbandonTourChance) && homeBuilding != 0)
                {
                    CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
                    residentAI.StartMoving(instance, citizenId, ref citizen, 0, homeBuilding);
                }
            }
        }
    }
}
