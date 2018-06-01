// <copyright file="RealTimeResidentAI.Work.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace RealTime.AI
{
    using ColossalFramework;
    using UnityEngine;

    internal static partial class RealTimeResidentAI
    {
        private static bool ProcessCitizenAtWork(Arguments args, uint citizenID, ref Citizen data)
        {
            if (data.Dead)
            {
                if (data.m_workBuilding == 0)
                {
                    args.CitizenMgr.ReleaseCitizen(citizenID);
                }
                else
                {
                    if (data.m_homeBuilding != 0)
                    {
                        data.SetHome(citizenID, 0, 0u);
                    }

                    if (data.m_visitBuilding != 0)
                    {
                        data.SetVisitplace(citizenID, 0, 0u);
                    }

                    if (data.m_vehicle != 0)
                    {
                        return false;
                    }

                    if (FindHospital(args.ResidentAI, citizenID, data.m_workBuilding, TransferManager.TransferReason.Dead))
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
            else
            {
                if (data.Sick)
                {
                    if (data.m_workBuilding == 0)
                    {
                        data.CurrentLocation = Citizen.Location.Home;
                        return false;
                    }

                    if (data.m_vehicle != 0)
                    {
                        return false;
                    }

                    if (FindHospital(args.ResidentAI, citizenID, data.m_workBuilding, TransferManager.TransferReason.Sick))
                    {
                        return false;
                    }

                    return true;
                }

                if (data.m_workBuilding == 0)
                {
                    data.CurrentLocation = Citizen.Location.Home;
                }
                else
                {
                    ushort eventIndex = args.BuildingMgr.m_buildings.m_buffer[data.m_workBuilding].m_eventIndex;
                    if ((args.BuildingMgr.m_buildings.m_buffer[data.m_workBuilding].m_flags & Building.Flags.Evacuating) != 0)
                    {
                        if (data.m_vehicle == 0)
                        {
                            FindEvacuationPlace(args.ResidentAI, citizenID, data.m_workBuilding, GetEvacuationReason(args.ResidentAI, data.m_workBuilding));
                        }
                    }
                    else if (eventIndex == 0 || (Singleton<EventManager>.instance.m_events.m_buffer[eventIndex].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == EventData.Flags.None)
                    {
                        if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                        {
                            if (data.m_vehicle == 0)
                            {
                                FindVisitPlace(args.ResidentAI, citizenID, data.m_workBuilding, GetShoppingReason(args.ResidentAI));
                            }
                        }
                        else
                        {
                            if (data.m_instance == 0 && !DoRandomMove(args.ResidentAI))
                            {
                                return false;
                            }

                            int dayTimeFrame = (int)args.SimMgr.m_dayTimeFrame;
                            int dAYTIME_FRAMES = (int)SimulationManager.DAYTIME_FRAMES;
                            int num2 = dAYTIME_FRAMES / 40;
                            int num3 = (int)(SimulationManager.DAYTIME_FRAMES * 16) / 24;
                            int num4 = dayTimeFrame - num3 & dAYTIME_FRAMES - 1;
                            int num5 = Mathf.Abs(num4 - (dAYTIME_FRAMES >> 1));
                            num5 = num5 * num5 / (dAYTIME_FRAMES >> 1);
                            int num6 = args.SimMgr.m_randomizer.Int32((uint)dAYTIME_FRAMES);
                            if (num6 < num2)
                            {
                                if (data.m_vehicle == 0)
                                {
                                    FindVisitPlace(args.ResidentAI, citizenID, data.m_workBuilding, GetEntertainmentReason(args.ResidentAI));
                                }
                            }
                            else if (num6 < num2 + num5 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                            {
                                data.m_flags &= ~Citizen.Flags.Evacuating;
                                args.ResidentAI.StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
