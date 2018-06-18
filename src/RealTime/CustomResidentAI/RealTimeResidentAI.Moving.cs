// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenMoving(TAI instance, uint citizenId, ref TCitizen citizen, bool mayCancel)
        {
            ushort instanceId = citizenProxy.GetInstance(ref citizen);
            CitizenInstance.Flags flags = CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour;
            if (citizenProxy.GetVehicle(ref citizen) == 0 && instanceId == 0)
            {
                if (citizenProxy.GetVisitBuilding(ref citizen) != 0)
                {
                    citizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                }

                citizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                citizenProxy.SetArrested(ref citizen, false);
            }
            else if ((citizenManager.GetInstanceFlags(instanceId) & flags) == flags)
            {
                ushort homeBuilding = citizenProxy.GetHomeBuilding(ref citizen);
                if (IsChance(AbandonTourChance) && homeBuilding != 0)
                {
                    citizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
                    residentAI.StartMoving(instance, citizenId, ref citizen, 0, homeBuilding);
                }
            }
        }
    }
}
