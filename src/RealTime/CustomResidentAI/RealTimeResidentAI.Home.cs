// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.CustomResidentAI
{
    using RealTime.Tools;

    internal sealed partial class RealTimeResidentAI<TAI, TCitizen>
    {
        private void ProcessCitizenAtHome(TAI instance, uint citizenId, ref TCitizen citizen)
        {
            if (citizenProxy.GetHomeBuilding(ref citizen) == 0)
            {
                Log.Debug($"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is in corrupt state: at home with no home building. Releasing the poor citizen.");
                citizenManager.ReleaseCitizen(citizenId);
                return;
            }

            ushort vehicle = citizenProxy.GetVehicle(ref citizen);
            if (vehicle != 0)
            {
                Log.Debug(timeInfo.Now, $"WARNING: {GetCitizenDesc(citizenId, ref citizen)} is at home but vehicle = {vehicle}");
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
