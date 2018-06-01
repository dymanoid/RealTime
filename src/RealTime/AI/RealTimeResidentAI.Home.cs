// <copyright file="RealTimeResidentAI.Home.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using UnityEngine;

    internal static partial class RealTimeResidentAI
    {
        private static bool ProcessCitizenAtHome(Arguments args, uint citizenID, ref Citizen data)
        {
            if ((data.m_flags & Citizen.Flags.MovingIn) != 0)
            {
                args.CitizenMgr.ReleaseCitizen(citizenID);
                return true;
            }

            if (data.Dead)
            {
                if (data.m_homeBuilding == 0)
                {
                    args.CitizenMgr.ReleaseCitizen(citizenID);
                }
                else
                {
                    if (data.m_workBuilding != 0)
                    {
                        data.SetWorkplace(citizenID, 0, 0u);
                    }

                    if (data.m_visitBuilding != 0)
                    {
                        data.SetVisitplace(citizenID, 0, 0u);
                    }

                    if (data.m_vehicle != 0)
                    {
                        return false;
                    }

                    if (FindHospital(args.ResidentAI, citizenID, data.m_homeBuilding, TransferManager.TransferReason.Dead))
                    {
                        return false;
                    }
                }

                return true;
            }

            if (data.Arrested)
            {
                data.Arrested = false;
            }
            else if (data.m_homeBuilding != 0 && data.m_vehicle == 0)
            {
                if (data.Sick)
                {
                    if (FindHospital(args.ResidentAI, citizenID, data.m_homeBuilding, TransferManager.TransferReason.Sick))
                    {
                        return false;
                    }

                    return true;
                }

                if ((args.BuildingMgr.m_buildings.m_buffer[data.m_homeBuilding].m_flags & Building.Flags.Evacuating) != 0)
                {
                    FindEvacuationPlace(args.ResidentAI, citizenID, data.m_homeBuilding, GetEvacuationReason(args.ResidentAI, data.m_homeBuilding));
                }
                else if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                {
                    FindVisitPlace(args.ResidentAI, citizenID, data.m_homeBuilding, GetShoppingReason(args.ResidentAI));
                }
                else
                {
                    if (data.m_instance == 0 && !DoRandomMove(args.ResidentAI))
                    {
                        return false;
                    }

                    int dayTimeFrame2 = (int)args.SimMgr.m_dayTimeFrame;
                    int dAYTIME_FRAMES2 = (int)SimulationManager.DAYTIME_FRAMES;
                    int num9 = dAYTIME_FRAMES2 / 40;
                    int num10 = (int)(SimulationManager.DAYTIME_FRAMES * 8) / 24;
                    int num11 = dayTimeFrame2 - num10 & dAYTIME_FRAMES2 - 1;
                    int num12 = Mathf.Abs(num11 - (dAYTIME_FRAMES2 >> 1));
                    num12 = num12 * num12 / (dAYTIME_FRAMES2 >> 1);
                    int num13 = args.SimMgr.m_randomizer.Int32((uint)dAYTIME_FRAMES2);
                    if (num13 < num9)
                    {
                        FindVisitPlace(args.ResidentAI, citizenID, data.m_homeBuilding, GetEntertainmentReason(args.ResidentAI));
                    }
                    else if (num13 < num9 + num12 && data.m_workBuilding != 0)
                    {
                        data.m_flags &= ~Citizen.Flags.Evacuating;
                        args.ResidentAI.StartMoving(citizenID, ref data, data.m_homeBuilding, data.m_workBuilding);
                    }
                }
            }

            return false;
        }
    }
}
