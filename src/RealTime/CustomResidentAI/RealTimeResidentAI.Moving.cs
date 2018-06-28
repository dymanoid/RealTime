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
            ushort vehicleId = CitizenProxy.GetVehicle(ref citizen);

            if (vehicleId == 0 && instanceId == 0)
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

            if (vehicleId == 0 && CitizenMgr.IsAreaEvacuating(instanceId) && !CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen)} was on the way, but the area evacuates. Finding an evacuation place.");
                TransferMgr.AddOutgoingOfferFromCurrentPosition(citizenId, residentAI.GetEvacuationReason(instance, 0));
                return;
            }

            if (CitizenMgr.InstanceHasFlags(instanceId, CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour, true))
            {
                ushort homeBuilding = CitizenProxy.GetHomeBuilding(ref citizen);
                if (IsChance(AbandonTourChance) && homeBuilding != 0)
                {
                    CitizenProxy.RemoveFlags(ref citizen, Citizen.Flags.Evacuating);
                    residentAI.StartMoving(instance, citizenId, ref citizen, 0, homeBuilding);
                }
            }
            else if (CitizenMgr.InstanceHasFlags(instanceId, CitizenInstance.Flags.WaitingTransport | CitizenInstance.Flags.WaitingTaxi))
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
