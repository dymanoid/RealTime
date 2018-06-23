// <copyright file="RealTimeResidentAI.Moving.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomAI
{
    using RealTime.Tools;
    using static Constants;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenMoving(TAI instance, uint citizenId, ref TCitizen citizen, bool mayCancel)
        {
            ushort instanceId = CitizenProxy.GetInstance(ref citizen);

            // TODO: implement bored of traffic jam trip abandon
            if (CitizenProxy.GetVehicle(ref citizen) == 0 && instanceId == 0)
            {
                if (CitizenProxy.GetVisitBuilding(ref citizen) != 0)
                {
                    CitizenProxy.SetVisitPlace(ref citizen, citizenId, 0);
                }

                Log.Debug($"Teleporting {GetCitizenDesc(citizenId, ref citizen)} back home because they are moving but no instance is specified");
                CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                CitizenProxy.SetArrested(ref citizen, false);

                return;
            }

            CitizenInstance.Flags instanceFlags = CitizenManager.GetInstanceFlags(instanceId);
            CitizenInstance.Flags onTourFlags = CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour;

            if ((instanceFlags & onTourFlags) == onTourFlags)
            {
                ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
                if (IsChance(AbandonTourChance) && homeBuilding != 0)
                {
                    CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
                    residentAI.StartMoving(instance, citizenId, ref citizen, 0, homeBuilding);
                }
            }
            else if ((instanceFlags & (CitizenInstance.Flags.WaitingTransport | CitizenInstance.Flags.WaitingTaxi)) != 0)
            {
                if (mayCancel && CitizenMgr.GetInstanceWaitCounter(instanceId) == 255 && IsChance(AbandonTransportWaitChance))
                {
                    ushort home = CitizenProxy.GetHomeBuilding(ref citizen);
                    if (home == 0)
                    {
                        return;
                    }

                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} doesn't want to wait for transport anymore, goes back home");
                    residentAI.StartMoving(instance, citizenId, ref citizen, 0, home);
                }
            }
        }
    }
}
