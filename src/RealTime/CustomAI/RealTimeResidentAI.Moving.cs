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

                if (CitizenProxy.HasFlags(ref citizen, Citizen.Flags.MovingIn))
                {
                    CitizenMgr.ReleaseCitizen(citizenId);
                }
                else
                {
                    // TODO: check whether this makes sense and maybe remove/replace this logic
                    // Don't know why the original game does this...
                    CitizenProxy.SetLocation(ref citizen, Citizen.Location.Home);
                    CitizenProxy.SetArrested(ref citizen, false);
                }

                return;
            }

            if (vehicleId == 0 && CitizenMgr.IsAreaEvacuating(instanceId) && !CitizenProxy.HasFlags(ref citizen, Citizen.Flags.Evacuating))
            {
                Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} was on the way, but the area evacuates. Finding an evacuation place.");
                TransferMgr.AddOutgoingOfferFromCurrentPosition(citizenId, residentAI.GetEvacuationReason(instance, 0));
                return;
            }

            bool returnHome = false;
            ushort targetBuilding = CitizenMgr.GetTargetBuilding(instanceId);
            if (targetBuilding != CitizenProxy.GetWorkBuilding(ref citizen))
            {
                ItemClass.Service targetService = BuildingMgr.GetBuildingService(targetBuilding);
                if (targetService == ItemClass.Service.Beautification && IsBadWeather(citizenId))
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} cancels the trip to a park due to bad weather");
                    returnHome = true;
                }
            }

            if (!returnHome && CitizenMgr.InstanceHasFlags(instanceId, CitizenInstance.Flags.WaitingTransport | CitizenInstance.Flags.WaitingTaxi))
            {
                if (mayCancel && CitizenMgr.GetInstanceWaitCounter(instanceId) == 255 && Random.ShouldOccur(AbandonTransportWaitChance))
                {
                    Log.Debug(TimeInfo.Now, $"{GetCitizenDesc(citizenId, ref citizen, false)} goes back home");
                    returnHome = true;
                }
            }

            if (returnHome)
            {
                ushort home = CitizenProxy.GetHomeBuilding(ref citizen);
                if (home == 0)
                {
                    return;
                }

                residentAI.StartMoving(instance, citizenId, ref citizen, 0, home);
            }
        }
    }
}
