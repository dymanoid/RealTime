using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using ColossalFramework.Threading;
using System;
using UnityEngine;

public class ResidentAI1 : HumanAI
{
    public const int UNIVERSITY_DURATION = 15;

    public const int BREED_INTERVAL = 12;

    public const int GAY_PROBABILITY = 5;

    public const int CAR_PROBABILITY_CHILD = 0;

    public const int CAR_PROBABILITY_TEEN = 5;

    public const int CAR_PROBABILITY_YOUNG = 15;

    public const int CAR_PROBABILITY_ADULT = 20;

    public const int CAR_PROBABILITY_SENIOR = 10;

    public const int BIKE_PROBABILITY_CHILD = 40;

    public const int BIKE_PROBABILITY_TEEN = 30;

    public const int BIKE_PROBABILITY_YOUNG = 20;

    public const int BIKE_PROBABILITY_ADULT = 10;

    public const int BIKE_PROBABILITY_SENIOR = 0;

    public const int TAXI_PROBABILITY_CHILD = 0;

    public const int TAXI_PROBABILITY_TEEN = 2;

    public const int TAXI_PROBABILITY_YOUNG = 2;

    public const int TAXI_PROBABILITY_ADULT = 4;

    public const int TAXI_PROBABILITY_SENIOR = 6;

    public override Color GetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode)
    {
        switch (infoMode)
        {
        case InfoManager.InfoMode.Health:
        {
            int health2 = Singleton<CitizenManager>.instance.m_citizens.m_buffer[data.m_citizen].m_health;
            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, (float)Citizen.GetHealthLevel(health2) * 0.2f);
        }
        case InfoManager.InfoMode.Happiness:
        {
            int health = Singleton<CitizenManager>.instance.m_citizens.m_buffer[data.m_citizen].m_health;
            int wellbeing = Singleton<CitizenManager>.instance.m_citizens.m_buffer[data.m_citizen].m_wellbeing;
            int happiness = Citizen.GetHappiness(health, wellbeing);
            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, (float)Citizen.GetHappinessLevel(happiness) * 0.25f);
        }
        default:
            return base.GetColor(instanceID, ref data, infoMode);
        }
    }

    public override void SetRenderParameters(RenderManager.CameraInfo cameraInfo, ushort instanceID, ref CitizenInstance data, Vector3 position, Quaternion rotation, Vector3 velocity, Color color, bool underground)
    {
        if ((data.m_flags & CitizenInstance.Flags.AtTarget) != 0)
        {
            if ((data.m_flags & CitizenInstance.Flags.SittingDown) != 0)
            {
                base.m_info.SetRenderParameters(position, rotation, velocity, color, 2, underground);
                return;
            }
            if ((data.m_flags & (CitizenInstance.Flags.Panicking | CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) == CitizenInstance.Flags.Panicking)
            {
                base.m_info.SetRenderParameters(position, rotation, velocity, color, 1, underground);
                return;
            }
            if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating | CitizenInstance.Flags.Cheering)) == CitizenInstance.Flags.Cheering)
            {
                base.m_info.SetRenderParameters(position, rotation, velocity, color, 5, underground);
                return;
            }
        }
        if ((data.m_flags & CitizenInstance.Flags.RidingBicycle) != 0)
        {
            base.m_info.SetRenderParameters(position, rotation, velocity, color, 3, underground);
        }
        else if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != 0)
        {
            base.m_info.SetRenderParameters(position, rotation, Vector3.zero, color, 1, underground);
        }
        else
        {
            base.m_info.SetRenderParameters(position, rotation, velocity, color, instanceID & 4, underground);
        }
    }

    public override string GetLocalizedStatus(ushort instanceID, ref CitizenInstance data, out InstanceID target)
    {
        if ((data.m_flags & (CitizenInstance.Flags.Blown | CitizenInstance.Flags.Floating)) != 0)
        {
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
        }
        CitizenManager instance = Singleton<CitizenManager>.instance;
        uint citizen = data.m_citizen;
        bool flag = false;
        ushort num = 0;
        ushort num2 = 0;
        ushort num3 = 0;
        if (citizen != 0)
        {
            num = instance.m_citizens.m_buffer[citizen].m_homeBuilding;
            num2 = instance.m_citizens.m_buffer[citizen].m_workBuilding;
            num3 = instance.m_citizens.m_buffer[citizen].m_vehicle;
            flag = ((instance.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.Student) != Citizen.Flags.None);
        }
        ushort targetBuilding = data.m_targetBuilding;
        bool flag2;
        bool flag3;
        if (targetBuilding != 0)
        {
            if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
            {
                if (num3 != 0)
                {
                    VehicleManager instance2 = Singleton<VehicleManager>.instance;
                    VehicleInfo info = instance2.m_vehicles.m_buffer[num3].Info;
                    if (info.m_class.m_service == ItemClass.Service.Residential && info.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
                    {
                        if (info.m_vehicleAI.GetOwnerID(num3, ref instance2.m_vehicles.m_buffer[num3]).Citizen == citizen)
                        {
                            target = InstanceID.Empty;
                            target.NetNode = targetBuilding;
                            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO");
                        }
                    }
                    else if (info.m_class.m_service == ItemClass.Service.PublicTransport || info.m_class.m_service == ItemClass.Service.Disaster)
                    {
                        ushort transportLine = Singleton<NetManager>.instance.m_nodes.m_buffer[targetBuilding].m_transportLine;
                        if ((data.m_flags & CitizenInstance.Flags.WaitingTaxi) != 0)
                        {
                            target = InstanceID.Empty;
                            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_WAITING_TAXI");
                        }
                        if (instance2.m_vehicles.m_buffer[num3].m_transportLine != transportLine)
                        {
                            target = InstanceID.Empty;
                            target.NetNode = targetBuilding;
                            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
                        }
                    }
                }
                if ((data.m_flags & CitizenInstance.Flags.OnTour) != 0)
                {
                    target = InstanceID.Empty;
                    target.NetNode = targetBuilding;
                    return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_VISITING");
                }
                target = InstanceID.Empty;
                target.NetNode = targetBuilding;
                return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO");
            }
            flag2 = ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetBuilding].m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None);
            flag3 = (data.m_path == 0 && (data.m_flags & CitizenInstance.Flags.HangAround) != CitizenInstance.Flags.None);
            if (num3 != 0)
            {
                VehicleManager instance3 = Singleton<VehicleManager>.instance;
                VehicleInfo info2 = instance3.m_vehicles.m_buffer[num3].Info;
                if (info2.m_class.m_service == ItemClass.Service.Residential && info2.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
                {
                    if (info2.m_vehicleAI.GetOwnerID(num3, ref instance3.m_vehicles.m_buffer[num3]).Citizen == citizen)
                    {
                        if (flag2)
                        {
                            target = InstanceID.Empty;
                            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO_OUTSIDE");
                        }
                        if (targetBuilding == num)
                        {
                            target = InstanceID.Empty;
                            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO_HOME");
                        }
                        if (targetBuilding == num2)
                        {
                            target = InstanceID.Empty;
                            return ColossalFramework.Globalization.Locale.Get((!flag) ? "CITIZEN_STATUS_DRIVINGTO_WORK" : "CITIZEN_STATUS_DRIVINGTO_SCHOOL");
                        }
                        target = InstanceID.Empty;
                        target.Building = targetBuilding;
                        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_DRIVINGTO");
                    }
                    goto IL_0480;
                }
                if (info2.m_class.m_service != ItemClass.Service.PublicTransport && info2.m_class.m_service != ItemClass.Service.Disaster)
                {
                    goto IL_0480;
                }
                if ((data.m_flags & CitizenInstance.Flags.WaitingTaxi) != 0)
                {
                    target = InstanceID.Empty;
                    return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_WAITING_TAXI");
                }
                if (flag2)
                {
                    target = InstanceID.Empty;
                    return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_OUTSIDE");
                }
                if (targetBuilding == num)
                {
                    target = InstanceID.Empty;
                    return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO_HOME");
                }
                if (targetBuilding == num2)
                {
                    target = InstanceID.Empty;
                    return ColossalFramework.Globalization.Locale.Get((!flag) ? "CITIZEN_STATUS_TRAVELLINGTO_WORK" : "CITIZEN_STATUS_TRAVELLINGTO_SCHOOL");
                }
                target = InstanceID.Empty;
                target.Building = targetBuilding;
                return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_TRAVELLINGTO");
            }
            goto IL_0480;
        }
        target = InstanceID.Empty;
        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
        IL_0480:
        if (flag2)
        {
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO_OUTSIDE");
        }
        if (targetBuilding == num)
        {
            if (flag3)
            {
                target = InstanceID.Empty;
                return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_AT_HOME");
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO_HOME");
        }
        if (targetBuilding == num2)
        {
            if (flag3)
            {
                target = InstanceID.Empty;
                return ColossalFramework.Globalization.Locale.Get((!flag) ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get((!flag) ? "CITIZEN_STATUS_GOINGTO_WORK" : "CITIZEN_STATUS_GOINGTO_SCHOOL");
        }
        if (flag3)
        {
            target = InstanceID.Empty;
            target.Building = targetBuilding;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_VISITING");
        }
        target = InstanceID.Empty;
        target.Building = targetBuilding;
        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_GOINGTO");
    }

    public override string GetLocalizedStatus(uint citizenID, ref Citizen data, out InstanceID target)
    {
        CitizenManager instance = Singleton<CitizenManager>.instance;
        ushort instance2 = data.m_instance;
        if (instance2 != 0)
        {
            return GetLocalizedStatus(instance2, ref instance.m_instances.m_buffer[instance2], out target);
        }
        Citizen.Location currentLocation = data.CurrentLocation;
        ushort homeBuilding = data.m_homeBuilding;
        ushort workBuilding = data.m_workBuilding;
        ushort visitBuilding = data.m_visitBuilding;
        bool flag = (data.m_flags & Citizen.Flags.Student) != Citizen.Flags.None;
        switch (currentLocation)
        {
        case Citizen.Location.Home:
            if (homeBuilding == 0)
            {
                break;
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_AT_HOME");
        case Citizen.Location.Work:
            if (workBuilding == 0)
            {
                break;
            }
            target = InstanceID.Empty;
            return ColossalFramework.Globalization.Locale.Get((!flag) ? "CITIZEN_STATUS_AT_WORK" : "CITIZEN_STATUS_AT_SCHOOL");
        case Citizen.Location.Visit:
            if (visitBuilding == 0)
            {
                break;
            }
            target = InstanceID.Empty;
            target.Building = visitBuilding;
            return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_VISITING");
        }
        target = InstanceID.Empty;
        return ColossalFramework.Globalization.Locale.Get("CITIZEN_STATUS_CONFUSED");
    }

    public override void LoadInstance(ushort instanceID, ref CitizenInstance data)
    {
        base.LoadInstance(instanceID, ref data);
        if (data.m_sourceBuilding != 0)
        {
            Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddSourceCitizen(instanceID, ref data);
        }
        if (data.m_targetBuilding != 0)
        {
            if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
            {
                Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
            }
            else
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
            }
        }
    }

    public override void SimulationStep(ushort instanceID, ref CitizenInstance citizenData, ref CitizenInstance.Frame frameData, bool lodPhysics)
    {
        uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
        if ((currentFrameIndex >> 4 & 0x3F) == (instanceID & 0x3F))
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint citizen = citizenData.m_citizen;
            if (citizen != 0 && (instance.m_citizens.m_buffer[citizen].m_flags & Citizen.Flags.NeedGoods) != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                ushort homeBuilding = instance.m_citizens.m_buffer[citizen].m_homeBuilding;
                ushort num = instance2.FindBuilding(frameData.m_position, 32f, ItemClass.Service.Commercial, ItemClass.SubService.None, Building.Flags.Created | Building.Flags.Active, Building.Flags.Deleted);
                if (homeBuilding != 0 && num != 0)
                {
                    BuildingInfo info = instance2.m_buildings.m_buffer[num].Info;
                    int num2 = -100;
                    info.m_buildingAI.ModifyMaterialBuffer(num, ref instance2.m_buildings.m_buffer[num], TransferManager.TransferReason.Shopping, ref num2);
                    uint containingUnit = instance.m_citizens.m_buffer[citizen].GetContainingUnit(citizen, instance2.m_buildings.m_buffer[homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
                    if (containingUnit != 0)
                    {
                        instance.m_units.m_buffer[containingUnit].m_goods += (ushort)(-num2);
                    }
                    instance.m_citizens.m_buffer[citizen].m_flags &= ~Citizen.Flags.NeedGoods;
                }
            }
        }
        base.SimulationStep(instanceID, ref citizenData, ref frameData, lodPhysics);
    }

    public override void SimulationStep(uint citizenID, ref Citizen data)
    {
        if (!data.Dead && UpdateAge(citizenID, ref data))
        {
            return;
        }
        if (!data.Dead)
        {
            UpdateHome(citizenID, ref data);
        }
        if (!data.Sick && !data.Dead)
        {
            if (UpdateHealth(citizenID, ref data))
            {
                return;
            }
            UpdateWellbeing(citizenID, ref data);
            UpdateWorkplace(citizenID, ref data);
        }
        UpdateLocation(citizenID, ref data);
    }

    public override void SimulationStep(uint homeID, ref CitizenUnit data)
    {
        CitizenManager instance = Singleton<CitizenManager>.instance;
        ushort building = instance.m_units.m_buffer[homeID].m_building;
        if (data.m_citizen0 != 0 && data.m_citizen1 != 0 && (data.m_citizen2 == 0 || data.m_citizen3 == 0 || data.m_citizen4 == 0))
        {
            bool flag = CanMakeBabies(data.m_citizen0, ref instance.m_citizens.m_buffer[data.m_citizen0]);
            bool flag2 = CanMakeBabies(data.m_citizen1, ref instance.m_citizens.m_buffer[data.m_citizen1]);
            if (flag && flag2 && Singleton<SimulationManager>.instance.m_randomizer.Int32(12u) == 0)
            {
                int family = instance.m_citizens.m_buffer[data.m_citizen0].m_family;
                if (instance.CreateCitizen(out uint num, 0, family, ref Singleton<SimulationManager>.instance.m_randomizer))
                {
                    instance.m_citizens.m_buffer[num].SetHome(num, 0, homeID);
                    instance.m_citizens.m_buffer[num].m_flags |= Citizen.Flags.Original;
                    if (building != 0)
                    {
                        DistrictManager instance2 = Singleton<DistrictManager>.instance;
                        Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_position;
                        byte district = instance2.GetDistrict(position);
                        instance2.m_districts.m_buffer[district].m_birthData.m_tempCount += 1u;
                    }
                }
            }
        }
        if (data.m_citizen0 != 0 && data.m_citizen1 == 0)
        {
            TryFindPartner(data.m_citizen0, ref instance.m_citizens.m_buffer[data.m_citizen0]);
        }
        else if (data.m_citizen1 != 0 && data.m_citizen0 == 0)
        {
            TryFindPartner(data.m_citizen1, ref instance.m_citizens.m_buffer[data.m_citizen1]);
        }
        if (data.m_citizen2 != 0)
        {
            TryMoveAwayFromHome(data.m_citizen2, ref instance.m_citizens.m_buffer[data.m_citizen2]);
        }
        if (data.m_citizen3 != 0)
        {
            TryMoveAwayFromHome(data.m_citizen3, ref instance.m_citizens.m_buffer[data.m_citizen3]);
        }
        if (data.m_citizen4 != 0)
        {
            TryMoveAwayFromHome(data.m_citizen4, ref instance.m_citizens.m_buffer[data.m_citizen4]);
        }
        data.m_goods = (ushort)Mathf.Max(0, data.m_goods - 20);
        if (data.m_goods < 200)
        {
            int num2 = Singleton<SimulationManager>.instance.m_randomizer.Int32(5u);
            for (int i = 0; i < 5; i++)
            {
                uint citizen = data.GetCitizen((num2 + i) % 5);
                if (citizen != 0)
                {
                    instance.m_citizens.m_buffer[citizen].m_flags |= Citizen.Flags.NeedGoods;
                    break;
                }
            }
        }
        if (building != 0 && ((long)Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_problems & -4611686018427387904L) != 0)
        {
            uint num3 = 0u;
            int num4 = 0;
            if (data.m_citizen4 != 0 && !instance.m_citizens.m_buffer[data.m_citizen4].Dead)
            {
                num4++;
                num3 = data.m_citizen4;
            }
            if (data.m_citizen3 != 0 && !instance.m_citizens.m_buffer[data.m_citizen3].Dead)
            {
                num4++;
                num3 = data.m_citizen3;
            }
            if (data.m_citizen2 != 0 && !instance.m_citizens.m_buffer[data.m_citizen2].Dead)
            {
                num4++;
                num3 = data.m_citizen2;
            }
            if (data.m_citizen1 != 0 && !instance.m_citizens.m_buffer[data.m_citizen1].Dead)
            {
                num4++;
                num3 = data.m_citizen1;
            }
            if (data.m_citizen0 != 0 && !instance.m_citizens.m_buffer[data.m_citizen0].Dead)
            {
                num4++;
                num3 = data.m_citizen0;
            }
            if (num3 != 0)
            {
                TryMoveFamily(num3, ref instance.m_citizens.m_buffer[num3], num4);
            }
        }
    }

    protected override void PathfindSuccess(ushort instanceID, ref CitizenInstance data)
    {
        uint citizen = data.m_citizen;
        if (citizen != 0)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            if ((instance.m_citizens.m_buffer[citizen].m_flags & (Citizen.Flags.MovingIn | Citizen.Flags.DummyTraffic)) == Citizen.Flags.MovingIn)
            {
                StatisticBase statisticBase = Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.MoveRate);
                statisticBase.Add(1);
            }
        }
        base.PathfindSuccess(instanceID, ref data);
    }

    protected override void Spawn(ushort instanceID, ref CitizenInstance data)
    {
        if ((data.m_flags & CitizenInstance.Flags.Character) == CitizenInstance.Flags.None)
        {
            data.Spawn(instanceID);
            uint citizen = data.m_citizen;
            ushort targetBuilding = data.m_targetBuilding;
            if (citizen != 0 && targetBuilding != 0)
            {
                Randomizer randomizer = new Randomizer(citizen);
                if (randomizer.Int32(20u) == 0)
                {
                    CitizenManager instance = Singleton<CitizenManager>.instance;
                    DistrictManager instance2 = Singleton<DistrictManager>.instance;
                    Vector3 position;
                    if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
                    {
                        NetManager instance3 = Singleton<NetManager>.instance;
                        position = instance3.m_nodes.m_buffer[targetBuilding].m_position;
                    }
                    else
                    {
                        BuildingManager instance4 = Singleton<BuildingManager>.instance;
                        position = instance4.m_buildings.m_buffer[targetBuilding].m_position;
                    }
                    byte district = instance2.GetDistrict(data.m_targetPos);
                    byte district2 = instance2.GetDistrict(position);
                    DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;
                    DistrictPolicies.Services servicePolicies2 = instance2.m_districts.m_buffer[district2].m_servicePolicies;
                    if (((servicePolicies | servicePolicies2) & DistrictPolicies.Services.PetBan) == DistrictPolicies.Services.None)
                    {
                        CitizenInfo groupAnimalInfo = instance.GetGroupAnimalInfo(ref randomizer, base.m_info.m_class.m_service, base.m_info.m_class.m_subService);
                        if ((object)groupAnimalInfo != null && instance.CreateCitizenInstance(out ushort num, ref randomizer, groupAnimalInfo, 0u))
                        {
                            groupAnimalInfo.m_citizenAI.SetSource(num, ref instance.m_instances.m_buffer[num], instanceID);
                            groupAnimalInfo.m_citizenAI.SetTarget(num, ref instance.m_instances.m_buffer[num], instanceID);
                        }
                    }
                }
            }
        }
    }

    private bool UpdateAge(uint citizenID, ref Citizen data)
    {
        int num = data.Age + 1;
        if (num <= 45)
        {
            if (num == 15 || num == 45)
            {
                FinishSchoolOrWork(citizenID, ref data);
            }
        }
        else if (num == 90 || num == 180)
        {
            FinishSchoolOrWork(citizenID, ref data);
        }
        else if ((data.m_flags & Citizen.Flags.Student) != 0 && num % 15 == 0)
        {
            FinishSchoolOrWork(citizenID, ref data);
        }
        if ((data.m_flags & Citizen.Flags.Original) != 0)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            if (instance.m_tempOldestOriginalResident < num)
            {
                instance.m_tempOldestOriginalResident = num;
            }
            if (num == 240)
            {
                Singleton<StatisticsManager>.instance.Acquire<StatisticInt32>(StatisticType.FullLifespans).Add(1);
            }
        }
        data.Age = num;
        if (num >= 240 && data.CurrentLocation != Citizen.Location.Moving && data.m_vehicle == 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(240, 255) <= num)
        {
            Die(citizenID, ref data);
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
            {
                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                return true;
            }
        }
        return false;
    }

    private void Die(uint citizenID, ref Citizen data)
    {
        data.Sick = false;
        data.Dead = true;
        data.SetParkedVehicle(citizenID, 0);
        if ((data.m_flags & Citizen.Flags.MovingIn) == Citizen.Flags.None)
        {
            ushort num = data.GetBuildingByLocation();
            if (num == 0)
            {
                num = data.m_homeBuilding;
            }
            if (num != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[num].m_position;
                byte district = instance.GetDistrict(position);
                instance.m_districts.m_buffer[district].m_deathData.m_tempCount += 1u;
            }
        }
    }

    private void UpdateHome(uint citizenID, ref Citizen data)
    {
        if (data.m_homeBuilding == 0 && (data.m_flags & Citizen.Flags.DummyTraffic) == Citizen.Flags.None)
        {
            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
            offer.Priority = 7;
            offer.Citizen = citizenID;
            offer.Amount = 1;
            offer.Active = true;
            if (data.m_workBuilding != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                offer.Position = instance.m_buildings.m_buffer[data.m_workBuilding].m_position;
            }
            else
            {
                offer.PositionX = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
                offer.PositionZ = Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
            }
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
            {
                switch (data.EducationLevel)
                {
                case Citizen.Education.Uneducated:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0, offer);
                    break;
                case Citizen.Education.OneSchool:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1, offer);
                    break;
                case Citizen.Education.TwoSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2, offer);
                    break;
                case Citizen.Education.ThreeSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3, offer);
                    break;
                }
            }
            else
            {
                switch (data.EducationLevel)
                {
                case Citizen.Education.Uneducated:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0B, offer);
                    break;
                case Citizen.Education.OneSchool:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1B, offer);
                    break;
                case Citizen.Education.TwoSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2B, offer);
                    break;
                case Citizen.Education.ThreeSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3B, offer);
                    break;
                }
            }
        }
    }

    private void UpdateWorkplace(uint citizenID, ref Citizen data)
    {
        if (data.m_workBuilding == 0 && data.m_homeBuilding != 0)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            Vector3 position = instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(position);
            DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;
            int age = data.Age;
            TransferManager.TransferReason transferReason = TransferManager.TransferReason.None;
            switch (Citizen.GetAgeGroup(age))
            {
            case Citizen.AgeGroup.Child:
                if (!data.Education1)
                {
                    transferReason = TransferManager.TransferReason.Student1;
                }
                break;
            case Citizen.AgeGroup.Teen:
                if (!data.Education2)
                {
                    transferReason = TransferManager.TransferReason.Student2;
                }
                break;
            case Citizen.AgeGroup.Young:
            case Citizen.AgeGroup.Adult:
                if (!data.Education3)
                {
                    transferReason = TransferManager.TransferReason.Student3;
                }
                break;
            }
            if (data.Unemployed != 0 && ((servicePolicies & DistrictPolicies.Services.EducationBoost) == DistrictPolicies.Services.None || transferReason != TransferManager.TransferReason.Student3 || age % 5 > 2))
            {
                TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
                offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u);
                offer.Citizen = citizenID;
                offer.Position = position;
                offer.Amount = 1;
                offer.Active = true;
                switch (data.EducationLevel)
                {
                case Citizen.Education.Uneducated:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker0, offer);
                    break;
                case Citizen.Education.OneSchool:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker1, offer);
                    break;
                case Citizen.Education.TwoSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker2, offer);
                    break;
                case Citizen.Education.ThreeSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Worker3, offer);
                    break;
                }
            }
            switch (transferReason)
            {
            case TransferManager.TransferReason.None:
                return;
            case TransferManager.TransferReason.Student3:
                if ((servicePolicies & DistrictPolicies.Services.SchoolsOut) != 0 && age % 5 <= 1)
                {
                    return;
                }
                break;
            }
            TransferManager.TransferOffer offer2 = default(TransferManager.TransferOffer);
            offer2.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u);
            offer2.Citizen = citizenID;
            offer2.Position = position;
            offer2.Amount = 1;
            offer2.Active = true;
            Singleton<TransferManager>.instance.AddOutgoingOffer(transferReason, offer2);
        }
    }

    private bool UpdateHealth(uint citizenID, ref Citizen data)
    {
        if (data.m_homeBuilding == 0)
        {
            return false;
        }
        int num = 20;
        BuildingManager instance = Singleton<BuildingManager>.instance;
        BuildingInfo info = instance.m_buildings.m_buffer[data.m_homeBuilding].Info;
        Vector3 position = instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
        DistrictManager instance2 = Singleton<DistrictManager>.instance;
        byte district = instance2.GetDistrict(position);
        DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;
        DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[district].m_cityPlanningPolicies;
        if ((servicePolicies & DistrictPolicies.Services.SmokingBan) != 0)
        {
            num += 10;
        }
        if (data.Age >= 180 && (cityPlanningPolicies & DistrictPolicies.CityPlanning.AntiSlip) != 0)
        {
            num += 10;
        }
        info.m_buildingAI.GetMaterialAmount(data.m_homeBuilding, ref instance.m_buildings.m_buffer[data.m_homeBuilding], TransferManager.TransferReason.Garbage, out int num2, out int _);
        num2 /= 1000;
        if (num2 <= 2)
        {
            num += 12;
        }
        else if (num2 >= 4)
        {
            num -= num2 - 3;
        }
        int healthCareRequirement = Citizen.GetHealthCareRequirement(Citizen.GetAgePhase(data.EducationLevel, data.Age));
        Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.HealthCare, position, out int num4, out int num5);
        if (healthCareRequirement != 0)
        {
            if (num4 != 0)
            {
                num += ImmaterialResourceManager.CalculateResourceEffect(num4, healthCareRequirement, 500, 20, 40);
            }
            if (num5 != 0)
            {
                num += ImmaterialResourceManager.CalculateResourceEffect(num5, healthCareRequirement >> 1, 250, 5, 20);
            }
        }
        Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.NoisePollution, position, out int num6);
        if (num6 != 0)
        {
            num = ((info.m_class.m_subService != ItemClass.SubService.ResidentialLowEco && info.m_class.m_subService != ItemClass.SubService.ResidentialHighEco) ? (num - num6 * 100 / 255) : (num - num6 * 150 / 255));
        }
        Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.CrimeRate, position, out int num7);
        if (num7 > 3)
        {
            num = ((num7 > 30) ? ((num7 > 70) ? (num - 15) : (num - 5)) : (num - 2));
        }
        Singleton<WaterManager>.instance.CheckWater(position, out bool flag, out bool flag2, out byte b);
        if (flag)
        {
            num += 12;
            data.NoWater = 0;
        }
        else
        {
            int noWater = data.NoWater;
            if (noWater < 2)
            {
                data.NoWater = noWater + 1;
            }
            else
            {
                num -= 5;
            }
        }
        if (flag2)
        {
            num += 12;
            data.NoSewage = 0;
        }
        else
        {
            int noSewage = data.NoSewage;
            if (noSewage < 2)
            {
                data.NoSewage = noSewage + 1;
            }
            else
            {
                num -= 5;
            }
        }
        num = ((b >= 35) ? (num - (b * 2 - 35)) : (num - b));
        Singleton<NaturalResourceManager>.instance.CheckPollution(position, out byte b2);
        if (b2 != 0)
        {
            num = ((info.m_class.m_subService != ItemClass.SubService.ResidentialLowEco && info.m_class.m_subService != ItemClass.SubService.ResidentialHighEco) ? (num - b2 * 100 / 255) : (num - b2 * 200 / 255));
        }
        num = Mathf.Clamp(num, 0, 100);
        data.m_health = (byte)num;
        int num8 = 0;
        if (num <= 10)
        {
            int badHealth = data.BadHealth;
            if (badHealth < 3)
            {
                num8 = 15;
                data.BadHealth = badHealth + 1;
            }
            else
            {
                num8 = ((num5 != 0) ? 50 : 75);
            }
        }
        else if (num <= 25)
        {
            data.BadHealth = 0;
            num8 += 10;
        }
        else if (num <= 50)
        {
            data.BadHealth = 0;
            num8 += 3;
        }
        else
        {
            data.BadHealth = 0;
        }
        Citizen.Location currentLocation = data.CurrentLocation;
        if (currentLocation != Citizen.Location.Moving && data.m_vehicle == 0 && num8 != 0 && Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) < num8)
        {
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(3u) == 0)
            {
                Die(citizenID, ref data);
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                {
                    Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    return true;
                }
            }
            else
            {
                data.Sick = true;
            }
        }
        return false;
    }

    private void UpdateWellbeing(uint citizenID, ref Citizen data)
    {
        if (data.m_homeBuilding != 0)
        {
            int num = 0;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[data.m_homeBuilding].Info;
            Vector3 position = instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            ItemClass @class = info.m_class;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(position);
            DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;
            DistrictPolicies.Taxation taxationPolicies = instance2.m_districts.m_buffer[district].m_taxationPolicies;
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[district].m_cityPlanningPolicies;
            int health = data.m_health;
            if (health > 80)
            {
                num += 10;
            }
            else if (health > 60)
            {
                num += 5;
            }
            num -= Mathf.Clamp(50 - health, 0, 30);
            if ((servicePolicies & DistrictPolicies.Services.PetBan) != 0)
            {
                num -= 5;
            }
            if ((servicePolicies & DistrictPolicies.Services.SmokingBan) != 0)
            {
                num -= 15;
            }
            Building.Frame lastFrameData = instance.m_buildings.m_buffer[data.m_homeBuilding].GetLastFrameData();
            if (lastFrameData.m_fireDamage != 0)
            {
                num -= 15;
            }
            Citizen.Wealth wealthLevel = data.WealthLevel;
            Citizen.AgePhase agePhase = Citizen.GetAgePhase(data.EducationLevel, data.Age);
            int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(@class, taxationPolicies);
            int num2 = (int)(8 - wealthLevel);
            int num3 = (int)(11 - wealthLevel);
            if (@class.m_subService == ItemClass.SubService.ResidentialHigh)
            {
                num2++;
                num3++;
            }
            if (taxRate < num2)
            {
                num += num2 - taxRate;
            }
            if (taxRate > num3)
            {
                num -= taxRate - num3;
            }
            int policeDepartmentRequirement = Citizen.GetPoliceDepartmentRequirement(agePhase);
            if (policeDepartmentRequirement != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.PoliceDepartment, position, out int num4, out int num5);
                if (num4 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num4, policeDepartmentRequirement, 500, 20, 40);
                }
                if (num5 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num5, policeDepartmentRequirement >> 1, 250, 5, 20);
                }
            }
            int fireDepartmentRequirement = Citizen.GetFireDepartmentRequirement(agePhase);
            if (fireDepartmentRequirement != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.FireDepartment, position, out int num6, out int num7);
                if (num6 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num6, fireDepartmentRequirement, 500, 20, 40);
                }
                if (num7 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num7, fireDepartmentRequirement >> 1, 250, 5, 20);
                }
            }
            int educationRequirement = Citizen.GetEducationRequirement(agePhase);
            if (educationRequirement != 0)
            {
                int num8;
                int num9;
                if (agePhase < Citizen.AgePhase.Teen0)
                {
                    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationElementary, position, out num8, out num9);
                    if (num8 > 1000 && !data.Education1 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000u) < num8 - 1000)
                    {
                        data.Education1 = true;
                    }
                }
                else if (agePhase < Citizen.AgePhase.Young0)
                {
                    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationHighSchool, position, out num8, out num9);
                    if (num8 > 1000 && !data.Education2 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000u) < num8 - 1000)
                    {
                        data.Education2 = true;
                    }
                }
                else
                {
                    Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.EducationUniversity, position, out num8, out num9);
                    if (num8 > 1000 && !data.Education3 && Singleton<SimulationManager>.instance.m_randomizer.Int32(9000u) < num8 - 1000)
                    {
                        data.Education3 = true;
                    }
                }
                if (num8 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num8, educationRequirement, 500, 20, 40);
                }
                if (num9 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num9, educationRequirement >> 1, 250, 5, 20);
                }
            }
            int entertainmentRequirement = Citizen.GetEntertainmentRequirement(agePhase);
            if (entertainmentRequirement != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.Entertainment, position, out int num10, out int num11);
                if (num10 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num10, entertainmentRequirement, 500, 30, 60);
                }
                if (num11 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num11, entertainmentRequirement >> 1, 250, 10, 40);
                }
            }
            int transportRequirement = Citizen.GetTransportRequirement(agePhase);
            if (transportRequirement != 0)
            {
                Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.PublicTransport, position, out int num12, out int num13);
                if (num12 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num12, transportRequirement, 500, 20, 40);
                }
                if (num13 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num13, transportRequirement >> 1, 250, 5, 20);
                }
            }
            int deathCareRequirement = Citizen.GetDeathCareRequirement(agePhase);
            Singleton<ImmaterialResourceManager>.instance.CheckResource(ImmaterialResourceManager.Resource.DeathCare, position, out int num14, out int num15);
            if (deathCareRequirement != 0)
            {
                if (num14 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num14, deathCareRequirement, 500, 10, 20);
                }
                if (num15 != 0)
                {
                    num += ImmaterialResourceManager.CalculateResourceEffect(num15, deathCareRequirement >> 1, 250, 3, 10);
                }
            }
            Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.RadioCoverage, position, out int num16);
            if (num16 != 0)
            {
                num += ImmaterialResourceManager.CalculateResourceEffect(num16, 50, 100, 2, 3);
            }
            Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.DisasterCoverage, position, out int num17);
            if (num17 != 0)
            {
                num += ImmaterialResourceManager.CalculateResourceEffect(num17, 50, 100, 3, 4);
            }
            Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.FirewatchCoverage, position, out int num18);
            if (num18 != 0)
            {
                num += ImmaterialResourceManager.CalculateResourceEffect(num18, 100, 1000, 0, 3);
            }
            Singleton<ElectricityManager>.instance.CheckElectricity(position, out bool flag);
            if (flag)
            {
                num += 12;
                data.NoElectricity = 0;
            }
            else
            {
                int noElectricity = data.NoElectricity;
                if (noElectricity < 2)
                {
                    data.NoElectricity = noElectricity + 1;
                }
                else
                {
                    num -= 5;
                }
            }
            Singleton<WaterManager>.instance.CheckHeating(position, out bool flag2);
            if (flag2)
            {
                num += 5;
            }
            else if ((servicePolicies & DistrictPolicies.Services.NoElectricity) != 0)
            {
                num -= 10;
            }
            if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.ElectricCars) != 0)
            {
                int carProbability = GetCarProbability(Citizen.GetAgeGroup(data.Age));
                if (new Randomizer(citizenID).Int32(100u) < carProbability)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.PolicyCost, 200, @class);
                }
            }
            bool flag3 = Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment);
            int workRequirement = Citizen.GetWorkRequirement(agePhase);
            if (workRequirement != 0)
            {
                if (data.m_workBuilding == 0)
                {
                    int unemployed = data.Unemployed;
                    num -= unemployed * workRequirement / 100;
                    if (flag3)
                    {
                        data.Unemployed = unemployed + 1;
                    }
                    else
                    {
                        data.Unemployed = Mathf.Min(1, unemployed + 1);
                    }
                }
                else
                {
                    data.Unemployed = 0;
                }
            }
            else
            {
                data.Unemployed = 0;
            }
            num = Mathf.Clamp(num, 0, 100);
            data.m_wellbeing = (byte)num;
            if (flag3)
            {
                Randomizer randomizer = new Randomizer(citizenID * 7931 + 123);
                int maxCrimeRate = Citizen.GetMaxCrimeRate(Citizen.GetWellbeingLevel(data.EducationLevel, num));
                int num19 = Mathf.Min(maxCrimeRate, Citizen.GetCrimeRate(data.Unemployed));
                data.Criminal = (randomizer.Int32(500u) < num19);
            }
            else
            {
                data.Criminal = false;
            }
        }
    }

    private void FinishSchoolOrWork(uint citizenID, ref Citizen data)
    {
        if (data.m_workBuilding != 0)
        {
            if (data.CurrentLocation == Citizen.Location.Work && data.m_homeBuilding != 0)
            {
                data.m_flags &= ~Citizen.Flags.Evacuating;
                base.StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            CitizenManager instance2 = Singleton<CitizenManager>.instance;
            uint num = instance.m_buildings.m_buffer[data.m_workBuilding].m_citizenUnits;
            int num2 = 0;
            do
            {
                if (num == 0)
                {
                    return;
                }
                uint nextUnit = instance2.m_units.m_buffer[num].m_nextUnit;
                CitizenUnit.Flags flags = instance2.m_units.m_buffer[num].m_flags;
                if ((flags & (CitizenUnit.Flags.Work | CitizenUnit.Flags.Student)) != 0)
                {
                    if ((flags & CitizenUnit.Flags.Student) != 0)
                    {
                        if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[num]))
                        {
                            BuildingInfo info = instance.m_buildings.m_buffer[data.m_workBuilding].Info;
                            if (info.m_buildingAI.GetEducationLevel1())
                            {
                                data.Education1 = true;
                            }
                            if (info.m_buildingAI.GetEducationLevel2())
                            {
                                data.Education2 = true;
                            }
                            if (info.m_buildingAI.GetEducationLevel3())
                            {
                                data.Education3 = true;
                            }
                            data.m_workBuilding = 0;
                            data.m_flags &= ~Citizen.Flags.Student;
                            if ((data.m_flags & Citizen.Flags.Original) != 0 && data.EducationLevel == Citizen.Education.ThreeSchools && instance2.m_fullyEducatedOriginalResidents++ == 0 && Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements != SimulationMetaData.MetaBool.True)
                            {
                                ThreadHelper.dispatcher.Dispatch(delegate
                                {
                                    if (!PlatformService.achievements["ClimbingTheSocialLadder"].achieved)
                                    {
                                        PlatformService.achievements["ClimbingTheSocialLadder"].Unlock();
                                    }
                                });
                            }
                            return;
                        }
                    }
                    else if (data.RemoveFromUnit(citizenID, ref instance2.m_units.m_buffer[num]))
                    {
                        data.m_workBuilding = 0;
                        data.m_flags &= ~Citizen.Flags.Student;
                        return;
                    }
                }
                num = nextUnit;
            }
            while (++num2 <= 524288);
            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
        }
    }

    private bool DoRandomMove()
    {
        uint vehicleCount = (uint)Singleton<VehicleManager>.instance.m_vehicleCount;
        uint instanceCount = (uint)Singleton<CitizenManager>.instance.m_instanceCount;
        if (vehicleCount * 65536 > instanceCount * 16384)
        {
            return Singleton<SimulationManager>.instance.m_randomizer.UInt32(16384u) > vehicleCount;
        }
        return Singleton<SimulationManager>.instance.m_randomizer.UInt32(65536u) > instanceCount;
    }

    private TransferManager.TransferReason GetShoppingReason()
    {
        switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(8u))
        {
        case 0:
            return TransferManager.TransferReason.Shopping;
        case 1:
            return TransferManager.TransferReason.ShoppingB;
        case 2:
            return TransferManager.TransferReason.ShoppingC;
        case 3:
            return TransferManager.TransferReason.ShoppingD;
        case 4:
            return TransferManager.TransferReason.ShoppingE;
        case 5:
            return TransferManager.TransferReason.ShoppingF;
        case 6:
            return TransferManager.TransferReason.ShoppingG;
        case 7:
            return TransferManager.TransferReason.ShoppingH;
        default:
            return TransferManager.TransferReason.Shopping;
        }
    }

    private TransferManager.TransferReason GetEntertainmentReason()
    {
        switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(4u))
        {
        case 0:
            return TransferManager.TransferReason.Entertainment;
        case 1:
            return TransferManager.TransferReason.EntertainmentB;
        case 2:
            return TransferManager.TransferReason.EntertainmentC;
        case 3:
            return TransferManager.TransferReason.EntertainmentD;
        default:
            return TransferManager.TransferReason.Entertainment;
        }
    }

    private TransferManager.TransferReason GetEvacuationReason(ushort sourceBuilding)
    {
        if (sourceBuilding != 0)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(instance.m_buildings.m_buffer[sourceBuilding].m_position);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[district].m_cityPlanningPolicies;
            if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.VIPArea) != 0)
            {
                switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(4u))
                {
                case 0:
                    return TransferManager.TransferReason.EvacuateVipA;
                case 1:
                    return TransferManager.TransferReason.EvacuateVipB;
                case 2:
                    return TransferManager.TransferReason.EvacuateVipC;
                case 3:
                    return TransferManager.TransferReason.EvacuateVipD;
                default:
                    return TransferManager.TransferReason.EvacuateVipA;
                }
            }
        }
        switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(4u))
        {
        case 0:
            return TransferManager.TransferReason.EvacuateA;
        case 1:
            return TransferManager.TransferReason.EvacuateB;
        case 2:
            return TransferManager.TransferReason.EvacuateC;
        case 3:
            return TransferManager.TransferReason.EvacuateD;
        default:
            return TransferManager.TransferReason.EvacuateA;
        }
    }

    private void UpdateLocation(uint citizenID, ref Citizen data)
    {
        if (data.m_homeBuilding == 0 && data.m_workBuilding == 0 && data.m_visitBuilding == 0 && data.m_instance == 0 && data.m_vehicle == 0)
        {
            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
        }
        else
        {
            switch (data.CurrentLocation)
            {
            case Citizen.Location.Home:
                if ((data.m_flags & Citizen.Flags.MovingIn) != 0)
                {
                    Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    return;
                }
                if (data.Dead)
                {
                    if (data.m_homeBuilding == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
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
                            break;
                        }
                        if (FindHospital(citizenID, data.m_homeBuilding, TransferManager.TransferReason.Dead))
                        {
                            break;
                        }
                    }
                    return;
                }
                if (data.Arrested)
                {
                    data.Arrested = false;
                }
                else if (data.m_homeBuilding != 0 && data.m_vehicle == 0)
                {
                    if (data.Sick)
                    {
                        if (FindHospital(citizenID, data.m_homeBuilding, TransferManager.TransferReason.Sick))
                        {
                            break;
                        }
                        return;
                    }
                    if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].m_flags & Building.Flags.Evacuating) != 0)
                    {
                        base.FindEvacuationPlace(citizenID, data.m_homeBuilding, GetEvacuationReason(data.m_homeBuilding));
                    }
                    else if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                    {
                        base.FindVisitPlace(citizenID, data.m_homeBuilding, GetShoppingReason());
                    }
                    else
                    {
                        if (data.m_instance == 0 && !DoRandomMove())
                        {
                            break;
                        }
                        int dayTimeFrame2 = (int)Singleton<SimulationManager>.instance.m_dayTimeFrame;
                        int dAYTIME_FRAMES2 = (int)SimulationManager.DAYTIME_FRAMES;
                        int num9 = dAYTIME_FRAMES2 / 40;
                        int num10 = (int)(SimulationManager.DAYTIME_FRAMES * 8) / 24;
                        int num11 = dayTimeFrame2 - num10 & dAYTIME_FRAMES2 - 1;
                        int num12 = Mathf.Abs(num11 - (dAYTIME_FRAMES2 >> 1));
                        num12 = num12 * num12 / (dAYTIME_FRAMES2 >> 1);
                        int num13 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)dAYTIME_FRAMES2);
                        if (num13 < num9)
                        {
                            base.FindVisitPlace(citizenID, data.m_homeBuilding, GetEntertainmentReason());
                        }
                        else if (num13 < num9 + num12 && data.m_workBuilding != 0)
                        {
                            data.m_flags &= ~Citizen.Flags.Evacuating;
                            base.StartMoving(citizenID, ref data, data.m_homeBuilding, data.m_workBuilding);
                        }
                    }
                }
                break;
            case Citizen.Location.Work:
                if (data.Dead)
                {
                    if (data.m_workBuilding == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
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
                            break;
                        }
                        if (FindHospital(citizenID, data.m_workBuilding, TransferManager.TransferReason.Dead))
                        {
                            break;
                        }
                    }
                    return;
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
                            break;
                        }
                        if (data.m_vehicle != 0)
                        {
                            break;
                        }
                        if (FindHospital(citizenID, data.m_workBuilding, TransferManager.TransferReason.Sick))
                        {
                            break;
                        }
                        return;
                    }
                    if (data.m_workBuilding == 0)
                    {
                        data.CurrentLocation = Citizen.Location.Home;
                    }
                    else
                    {
                        BuildingManager instance = Singleton<BuildingManager>.instance;
                        ushort eventIndex = instance.m_buildings.m_buffer[data.m_workBuilding].m_eventIndex;
                        if ((instance.m_buildings.m_buffer[data.m_workBuilding].m_flags & Building.Flags.Evacuating) != 0)
                        {
                            if (data.m_vehicle == 0)
                            {
                                base.FindEvacuationPlace(citizenID, data.m_workBuilding, GetEvacuationReason(data.m_workBuilding));
                            }
                        }
                        else if (eventIndex == 0 || (Singleton<EventManager>.instance.m_events.m_buffer[eventIndex].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == EventData.Flags.None)
                        {
                            if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                            {
                                if (data.m_vehicle == 0)
                                {
                                    base.FindVisitPlace(citizenID, data.m_workBuilding, GetShoppingReason());
                                }
                            }
                            else
                            {
                                if (data.m_instance == 0 && !DoRandomMove())
                                {
                                    break;
                                }
                                int dayTimeFrame = (int)Singleton<SimulationManager>.instance.m_dayTimeFrame;
                                int dAYTIME_FRAMES = (int)SimulationManager.DAYTIME_FRAMES;
                                int num2 = dAYTIME_FRAMES / 40;
                                int num3 = (int)(SimulationManager.DAYTIME_FRAMES * 16) / 24;
                                int num4 = dayTimeFrame - num3 & dAYTIME_FRAMES - 1;
                                int num5 = Mathf.Abs(num4 - (dAYTIME_FRAMES >> 1));
                                num5 = num5 * num5 / (dAYTIME_FRAMES >> 1);
                                int num6 = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)dAYTIME_FRAMES);
                                if (num6 < num2)
                                {
                                    if (data.m_vehicle == 0)
                                    {
                                        base.FindVisitPlace(citizenID, data.m_workBuilding, GetEntertainmentReason());
                                    }
                                }
                                else if (num6 < num2 + num5 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                {
                                    data.m_flags &= ~Citizen.Flags.Evacuating;
                                    base.StartMoving(citizenID, ref data, data.m_workBuilding, data.m_homeBuilding);
                                }
                            }
                        }
                    }
                }
                break;
            case Citizen.Location.Visit:
                if (data.Dead)
                {
                    if (data.m_visitBuilding == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    }
                    else
                    {
                        if (data.m_homeBuilding != 0)
                        {
                            data.SetHome(citizenID, 0, 0u);
                        }
                        if (data.m_workBuilding != 0)
                        {
                            data.SetWorkplace(citizenID, 0, 0u);
                        }
                        if (data.m_vehicle != 0)
                        {
                            break;
                        }
                        if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service == ItemClass.Service.HealthCare)
                        {
                            break;
                        }
                        if (FindHospital(citizenID, data.m_visitBuilding, TransferManager.TransferReason.Dead))
                        {
                            break;
                        }
                    }
                    return;
                }
                if (data.Arrested)
                {
                    if (data.m_visitBuilding == 0)
                    {
                        data.Arrested = false;
                    }
                }
                else if (!data.Collapsed)
                {
                    if (data.Sick)
                    {
                        if (data.m_visitBuilding == 0)
                        {
                            data.CurrentLocation = Citizen.Location.Home;
                            break;
                        }
                        if (data.m_vehicle != 0)
                        {
                            break;
                        }
                        BuildingManager instance2 = Singleton<BuildingManager>.instance;
                        ItemClass.Service service = instance2.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service;
                        if (service == ItemClass.Service.HealthCare)
                        {
                            break;
                        }
                        if (service == ItemClass.Service.Disaster)
                        {
                            break;
                        }
                        if (FindHospital(citizenID, data.m_visitBuilding, TransferManager.TransferReason.Sick))
                        {
                            break;
                        }
                        return;
                    }
                    BuildingManager instance3 = Singleton<BuildingManager>.instance;
                    ItemClass.Service service2 = ItemClass.Service.None;
                    if (data.m_visitBuilding != 0)
                    {
                        service2 = instance3.m_buildings.m_buffer[data.m_visitBuilding].Info.m_class.m_service;
                    }
                    switch (service2)
                    {
                    case ItemClass.Service.HealthCare:
                    case ItemClass.Service.PoliceDepartment:
                        if (data.m_homeBuilding != 0 && data.m_vehicle == 0)
                        {
                            data.m_flags &= ~Citizen.Flags.Evacuating;
                            base.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                            data.SetVisitplace(citizenID, 0, 0u);
                        }
                        break;
                    case ItemClass.Service.Disaster:
                        if ((instance3.m_buildings.m_buffer[data.m_visitBuilding].m_flags & Building.Flags.Downgrading) != 0 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                        {
                            data.m_flags &= ~Citizen.Flags.Evacuating;
                            base.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                            data.SetVisitplace(citizenID, 0, 0u);
                        }
                        break;
                    default:
                        if (data.m_visitBuilding == 0)
                        {
                            data.CurrentLocation = Citizen.Location.Home;
                        }
                        else if ((instance3.m_buildings.m_buffer[data.m_visitBuilding].m_flags & Building.Flags.Evacuating) != 0)
                        {
                            if (data.m_vehicle == 0)
                            {
                                base.FindEvacuationPlace(citizenID, data.m_visitBuilding, GetEvacuationReason(data.m_visitBuilding));
                            }
                        }
                        else if ((data.m_flags & Citizen.Flags.NeedGoods) != 0)
                        {
                            BuildingInfo info = instance3.m_buildings.m_buffer[data.m_visitBuilding].Info;
                            int num7 = -100;
                            info.m_buildingAI.ModifyMaterialBuffer(data.m_visitBuilding, ref instance3.m_buildings.m_buffer[data.m_visitBuilding], TransferManager.TransferReason.Shopping, ref num7);
                        }
                        else
                        {
                            ushort eventIndex2 = instance3.m_buildings.m_buffer[data.m_visitBuilding].m_eventIndex;
                            if (eventIndex2 != 0)
                            {
                                if ((Singleton<EventManager>.instance.m_events.m_buffer[eventIndex2].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active | EventData.Flags.Ready)) == EventData.Flags.None && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                {
                                    data.m_flags &= ~Citizen.Flags.Evacuating;
                                    base.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                    data.SetVisitplace(citizenID, 0, 0u);
                                }
                            }
                            else
                            {
                                if (data.m_instance == 0 && !DoRandomMove())
                                {
                                    break;
                                }
                                int num8 = Singleton<SimulationManager>.instance.m_randomizer.Int32(40u);
                                if (num8 < 10 && data.m_homeBuilding != 0 && data.m_vehicle == 0)
                                {
                                    data.m_flags &= ~Citizen.Flags.Evacuating;
                                    base.StartMoving(citizenID, ref data, data.m_visitBuilding, data.m_homeBuilding);
                                    data.SetVisitplace(citizenID, 0, 0u);
                                }
                            }
                        }
                        break;
                    }
                }
                break;
            case Citizen.Location.Moving:
                if (data.Dead)
                {
                    if (data.m_vehicle == 0)
                    {
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        return;
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
                else if (data.m_instance != 0 && (Singleton<CitizenManager>.instance.m_instances.m_buffer[data.m_instance].m_flags & (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour)) == (CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour))
                {
                    int num = Singleton<SimulationManager>.instance.m_randomizer.Int32(40u);
                    if (num < 10 && data.m_homeBuilding != 0)
                    {
                        data.m_flags &= ~Citizen.Flags.Evacuating;
                        base.StartMoving(citizenID, ref data, 0, data.m_homeBuilding);
                    }
                }
                break;
            }
            data.m_flags &= ~Citizen.Flags.NeedGoods;
        }
    }

    public bool CanMakeBabies(uint citizenID, ref Citizen data)
    {
        if (data.Dead)
        {
            return false;
        }
        if (Citizen.GetAgeGroup(data.Age) != Citizen.AgeGroup.Adult)
        {
            return false;
        }
        if ((data.m_flags & Citizen.Flags.MovingIn) != 0)
        {
            return false;
        }
        return true;
    }

    public void TryMoveAwayFromHome(uint citizenID, ref Citizen data)
    {
        if (!data.Dead && data.m_homeBuilding != 0)
        {
            Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(data.Age);
            if (ageGroup != Citizen.AgeGroup.Young && ageGroup != Citizen.AgeGroup.Adult)
            {
                return;
            }
            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
            if (ageGroup == Citizen.AgeGroup.Young)
            {
                offer.Priority = 1;
            }
            else
            {
                offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(2, 4);
            }
            offer.Citizen = citizenID;
            offer.Position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            offer.Amount = 1;
            offer.Active = true;
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
            {
                switch (data.EducationLevel)
                {
                case Citizen.Education.Uneducated:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0, offer);
                    break;
                case Citizen.Education.OneSchool:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1, offer);
                    break;
                case Citizen.Education.TwoSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2, offer);
                    break;
                case Citizen.Education.ThreeSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3, offer);
                    break;
                }
            }
            else
            {
                switch (data.EducationLevel)
                {
                case Citizen.Education.Uneducated:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0B, offer);
                    break;
                case Citizen.Education.OneSchool:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1B, offer);
                    break;
                case Citizen.Education.TwoSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2B, offer);
                    break;
                case Citizen.Education.ThreeSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3B, offer);
                    break;
                }
            }
        }
    }

    public void TryMoveFamily(uint citizenID, ref Citizen data, int familySize)
    {
        if (!data.Dead && data.m_homeBuilding != 0)
        {
            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
            offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, 7);
            offer.Citizen = citizenID;
            offer.Position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            offer.Amount = 1;
            offer.Active = true;
            if (familySize == 1)
            {
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0)
                {
                    switch (data.EducationLevel)
                    {
                    case Citizen.Education.Uneducated:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0, offer);
                        break;
                    case Citizen.Education.OneSchool:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1, offer);
                        break;
                    case Citizen.Education.TwoSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2, offer);
                        break;
                    case Citizen.Education.ThreeSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3, offer);
                        break;
                    }
                }
                else
                {
                    switch (data.EducationLevel)
                    {
                    case Citizen.Education.Uneducated:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single0B, offer);
                        break;
                    case Citizen.Education.OneSchool:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single1B, offer);
                        break;
                    case Citizen.Education.TwoSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single2B, offer);
                        break;
                    case Citizen.Education.ThreeSchools:
                        Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Single3B, offer);
                        break;
                    }
                }
            }
            else
            {
                switch (data.EducationLevel)
                {
                case Citizen.Education.Uneducated:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family0, offer);
                    break;
                case Citizen.Education.OneSchool:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family1, offer);
                    break;
                case Citizen.Education.TwoSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family2, offer);
                    break;
                case Citizen.Education.ThreeSchools:
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Family3, offer);
                    break;
                }
            }
        }
    }

    public void TryFindPartner(uint citizenID, ref Citizen data)
    {
        if (!data.Dead && data.m_homeBuilding != 0)
        {
            Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(data.Age);
            TransferManager.TransferReason material = TransferManager.TransferReason.None;
            switch (ageGroup)
            {
            case Citizen.AgeGroup.Young:
                material = TransferManager.TransferReason.PartnerYoung;
                break;
            case Citizen.AgeGroup.Adult:
                material = TransferManager.TransferReason.PartnerAdult;
                break;
            }
            if (ageGroup != Citizen.AgeGroup.Young && ageGroup != Citizen.AgeGroup.Adult)
            {
                return;
            }
            Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].m_position;
            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
            offer.Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8u);
            offer.Citizen = citizenID;
            offer.Position = position;
            offer.Amount = 1;
            offer.Active = (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0);
            bool flag = Singleton<SimulationManager>.instance.m_randomizer.Int32(100u) < 5;
            if (Citizen.GetGender(citizenID) == Citizen.Gender.Female != flag)
            {
                Singleton<TransferManager>.instance.AddIncomingOffer(material, offer);
            }
            else
            {
                Singleton<TransferManager>.instance.AddOutgoingOffer(material, offer);
            }
        }
    }

    private bool FindHospital(uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
    {
        if (reason == TransferManager.TransferReason.Dead)
        {
            if (Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.DeathCare))
            {
                return true;
            }
            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
            return false;
        }
        if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            Vector3 position = instance.m_buildings.m_buffer[sourceBuilding].m_position;
            byte district = instance2.GetDistrict(position);
            DistrictPolicies.Services servicePolicies = instance2.m_districts.m_buffer[district].m_servicePolicies;
            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
            offer.Priority = 6;
            offer.Citizen = citizenID;
            offer.Position = position;
            offer.Amount = 1;
            if ((servicePolicies & DistrictPolicies.Services.HelicopterPriority) != 0)
            {
                instance2.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.HelicopterPriority;
                offer.Active = false;
                Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick2, offer);
            }
            else if ((instance.m_buildings.m_buffer[sourceBuilding].m_flags & Building.Flags.RoadAccessFailed) != 0 || Singleton<SimulationManager>.instance.m_randomizer.Int32(20u) == 0)
            {
                offer.Active = false;
                Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Sick2, offer);
            }
            else
            {
                offer.Active = (Singleton<SimulationManager>.instance.m_randomizer.Int32(2u) == 0);
                Singleton<TransferManager>.instance.AddOutgoingOffer(reason, offer);
            }
            return true;
        }
        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
        return false;
    }

    public override void StartTransfer(uint citizenID, ref Citizen data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
    {
        if (data.m_flags != 0)
        {
            if (data.Dead && reason != TransferManager.TransferReason.Dead)
            {
                return;
            }
            switch (reason)
            {
            case TransferManager.TransferReason.Sick:
                if (data.Sick)
                {
                    data.m_flags &= ~Citizen.Flags.Evacuating;
                    if (base.StartMoving(citizenID, ref data, 0, offer))
                    {
                        data.SetVisitplace(citizenID, offer.Building, 0u);
                    }
                }
                break;
            case TransferManager.TransferReason.Dead:
                if (data.Dead)
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                    if (data.m_visitBuilding != 0)
                    {
                        data.CurrentLocation = Citizen.Location.Visit;
                    }
                }
                break;
            case TransferManager.TransferReason.Worker0:
            case TransferManager.TransferReason.Worker1:
            case TransferManager.TransferReason.Worker2:
            case TransferManager.TransferReason.Worker3:
                if (data.m_workBuilding == 0)
                {
                    data.SetWorkplace(citizenID, offer.Building, 0u);
                }
                break;
            case TransferManager.TransferReason.Student1:
            case TransferManager.TransferReason.Student2:
            case TransferManager.TransferReason.Student3:
                if (data.m_workBuilding == 0)
                {
                    data.SetStudentplace(citizenID, offer.Building, 0u);
                }
                break;
            case TransferManager.TransferReason.Shopping:
            case TransferManager.TransferReason.ShoppingB:
            case TransferManager.TransferReason.ShoppingC:
            case TransferManager.TransferReason.ShoppingD:
            case TransferManager.TransferReason.ShoppingE:
            case TransferManager.TransferReason.ShoppingF:
            case TransferManager.TransferReason.ShoppingG:
            case TransferManager.TransferReason.ShoppingH:
                if (data.m_homeBuilding != 0 && !data.Sick)
                {
                    data.m_flags &= ~Citizen.Flags.Evacuating;
                    if (base.StartMoving(citizenID, ref data, 0, offer))
                    {
                        data.SetVisitplace(citizenID, offer.Building, 0u);
                        CitizenManager instance3 = Singleton<CitizenManager>.instance;
                        BuildingManager instance4 = Singleton<BuildingManager>.instance;
                        uint containingUnit = data.GetContainingUnit(citizenID, instance4.m_buildings.m_buffer[data.m_homeBuilding].m_citizenUnits, CitizenUnit.Flags.Home);
                        if (containingUnit != 0)
                        {
                            instance3.m_units.m_buffer[containingUnit].m_goods += 100;
                        }
                    }
                }
                break;
            case TransferManager.TransferReason.Entertainment:
            case TransferManager.TransferReason.EntertainmentB:
            case TransferManager.TransferReason.EntertainmentC:
            case TransferManager.TransferReason.EntertainmentD:
                if (data.m_homeBuilding != 0 && !data.Sick)
                {
                    data.m_flags &= ~Citizen.Flags.Evacuating;
                    if (base.StartMoving(citizenID, ref data, 0, offer))
                    {
                        data.SetVisitplace(citizenID, offer.Building, 0u);
                    }
                }
                break;
            case TransferManager.TransferReason.Single0:
            case TransferManager.TransferReason.Single1:
            case TransferManager.TransferReason.Single2:
            case TransferManager.TransferReason.Single3:
            case TransferManager.TransferReason.Single0B:
            case TransferManager.TransferReason.Single1B:
            case TransferManager.TransferReason.Single2B:
            case TransferManager.TransferReason.Single3B:
                data.SetHome(citizenID, offer.Building, 0u);
                if (data.m_homeBuilding == 0)
                {
                    if (data.CurrentLocation == Citizen.Location.Visit && (data.m_flags & Citizen.Flags.Evacuating) != 0)
                    {
                        break;
                    }
                    Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                }
                break;
            case TransferManager.TransferReason.Family0:
            case TransferManager.TransferReason.Family1:
            case TransferManager.TransferReason.Family2:
            case TransferManager.TransferReason.Family3:
                if (data.m_homeBuilding != 0 && offer.Building != 0)
                {
                    uint num = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_homeBuilding].FindCitizenUnit(CitizenUnit.Flags.Home, citizenID);
                    if (num != 0)
                    {
                        MoveFamily(num, ref Singleton<CitizenManager>.instance.m_units.m_buffer[num], offer.Building);
                    }
                }
                break;
            case TransferManager.TransferReason.PartnerYoung:
            case TransferManager.TransferReason.PartnerAdult:
            {
                uint citizen = offer.Citizen;
                if (citizen != 0)
                {
                    CitizenManager instance = Singleton<CitizenManager>.instance;
                    BuildingManager instance2 = Singleton<BuildingManager>.instance;
                    ushort homeBuilding = instance.m_citizens.m_buffer[citizen].m_homeBuilding;
                    if (homeBuilding != 0 && !instance.m_citizens.m_buffer[citizen].Dead)
                    {
                        uint num2 = instance2.m_buildings.m_buffer[homeBuilding].FindCitizenUnit(CitizenUnit.Flags.Home, citizen);
                        if (num2 != 0)
                        {
                            data.SetHome(citizenID, 0, num2);
                            data.m_family = instance.m_citizens.m_buffer[citizen].m_family;
                        }
                    }
                }
                break;
            }
            case TransferManager.TransferReason.EvacuateA:
            case TransferManager.TransferReason.EvacuateB:
            case TransferManager.TransferReason.EvacuateC:
            case TransferManager.TransferReason.EvacuateD:
            case TransferManager.TransferReason.EvacuateVipA:
            case TransferManager.TransferReason.EvacuateVipB:
            case TransferManager.TransferReason.EvacuateVipC:
            case TransferManager.TransferReason.EvacuateVipD:
                data.m_flags |= Citizen.Flags.Evacuating;
                if (base.StartMoving(citizenID, ref data, 0, offer))
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                }
                else
                {
                    data.SetVisitplace(citizenID, offer.Building, 0u);
                    if (data.m_visitBuilding != 0 && data.m_visitBuilding == offer.Building)
                    {
                        data.CurrentLocation = Citizen.Location.Visit;
                    }
                }
                break;
            }
        }
    }

    private void MoveFamily(uint homeID, ref CitizenUnit data, ushort targetBuilding)
    {
        BuildingManager instance = Singleton<BuildingManager>.instance;
        CitizenManager instance2 = Singleton<CitizenManager>.instance;
        uint unitID = 0u;
        if (targetBuilding != 0)
        {
            unitID = instance.m_buildings.m_buffer[targetBuilding].GetEmptyCitizenUnit(CitizenUnit.Flags.Home);
        }
        for (int i = 0; i < 5; i++)
        {
            uint citizen = data.GetCitizen(i);
            if (citizen != 0 && !instance2.m_citizens.m_buffer[citizen].Dead)
            {
                instance2.m_citizens.m_buffer[citizen].SetHome(citizen, 0, unitID);
                if (instance2.m_citizens.m_buffer[citizen].m_homeBuilding == 0)
                {
                    instance2.ReleaseCitizen(citizen);
                }
            }
        }
    }

    public override void SetSource(ushort instanceID, ref CitizenInstance data, ushort sourceBuilding)
    {
        if (sourceBuilding != data.m_sourceBuilding)
        {
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].RemoveSourceCitizen(instanceID, ref data);
            }
            data.m_sourceBuilding = sourceBuilding;
            if (data.m_sourceBuilding != 0)
            {
                Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding].AddSourceCitizen(instanceID, ref data);
            }
        }
        if (sourceBuilding != 0)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[sourceBuilding].Info;
            data.Unspawn(instanceID);
            Randomizer randomizer = new Randomizer(instanceID);
            info.m_buildingAI.CalculateSpawnPosition(sourceBuilding, ref instance.m_buildings.m_buffer[sourceBuilding], ref randomizer, base.m_info, out Vector3 vector, out Vector3 a);
            Quaternion rotation = Quaternion.identity;
            Vector3 forward = a - vector;
            if (forward.sqrMagnitude > 0.01f)
            {
                rotation = Quaternion.LookRotation(forward);
            }
            data.m_frame0.m_velocity = Vector3.zero;
            data.m_frame0.m_position = vector;
            data.m_frame0.m_rotation = rotation;
            data.m_frame1 = data.m_frame0;
            data.m_frame2 = data.m_frame0;
            data.m_frame3 = data.m_frame0;
            data.m_targetPos = new Vector4(a.x, a.y, a.z, 1f);
            ushort eventIndex = 0;
            if (data.m_citizen != 0 && Singleton<CitizenManager>.instance.m_citizens.m_buffer[data.m_citizen].m_workBuilding != sourceBuilding)
            {
                eventIndex = instance.m_buildings.m_buffer[sourceBuilding].m_eventIndex;
            }
            Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(eventIndex, data.m_citizen);
            if (eventCitizenColor.a == 255)
            {
                data.m_color = eventCitizenColor;
                data.m_flags |= CitizenInstance.Flags.CustomColor;
            }
        }
    }

    public override void SetTarget(ushort instanceID, ref CitizenInstance data, ushort targetIndex, bool targetIsNode)
    {
        int dayTimeFrame = (int)Singleton<SimulationManager>.instance.m_dayTimeFrame;
        int dAYTIME_FRAMES = (int)SimulationManager.DAYTIME_FRAMES;
        int num = Mathf.Max(dAYTIME_FRAMES >> 2, Mathf.Abs(dayTimeFrame - (dAYTIME_FRAMES >> 1)));
        if (Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)dAYTIME_FRAMES >> 1) < num)
        {
            data.m_flags &= ~CitizenInstance.Flags.CannotUseTaxi;
        }
        else
        {
            data.m_flags |= CitizenInstance.Flags.CannotUseTaxi;
        }
        data.m_flags &= ~CitizenInstance.Flags.CannotUseTransport;
        if (targetIndex != data.m_targetBuilding || targetIsNode != ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None))
        {
            if (data.m_targetBuilding != 0)
            {
                if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
                {
                    Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                    ushort num2 = 0;
                    if (targetIsNode)
                    {
                        num2 = Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].m_transportLine;
                    }
                    if ((data.m_flags & CitizenInstance.Flags.OnTour) != 0)
                    {
                        ushort transportLine = Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].m_transportLine;
                        uint citizen = data.m_citizen;
                        if (transportLine != 0 && transportLine != num2 && citizen != 0)
                        {
                            TransportManager instance = Singleton<TransportManager>.instance;
                            TransportInfo info = instance.m_lines.m_buffer[transportLine].Info;
                            if ((object)info != null && info.m_vehicleType == VehicleInfo.VehicleType.None)
                            {
                                data.m_flags &= ~CitizenInstance.Flags.OnTour;
                            }
                        }
                    }
                    if (!targetIsNode)
                    {
                        data.m_flags &= ~CitizenInstance.Flags.TargetIsNode;
                    }
                }
                else
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                }
            }
            data.m_targetBuilding = targetIndex;
            if (targetIsNode)
            {
                data.m_flags |= CitizenInstance.Flags.TargetIsNode;
            }
            if (data.m_targetBuilding != 0)
            {
                if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
                {
                    Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                }
                else
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                }
                data.m_targetSeed = (byte)Singleton<SimulationManager>.instance.m_randomizer.Int32(256u);
            }
        }
        if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.None && IsRoadConnection(targetIndex))
        {
            goto IL_02a3;
        }
        if (IsRoadConnection(data.m_sourceBuilding))
        {
            goto IL_02a3;
        }
        data.m_flags &= ~CitizenInstance.Flags.BorrowCar;
        goto IL_02c6;
        IL_02a3:
        data.m_flags |= CitizenInstance.Flags.BorrowCar;
        goto IL_02c6;
        IL_02c6:
        if (targetIndex != 0 && (data.m_flags & (CitizenInstance.Flags.Character | CitizenInstance.Flags.TargetIsNode)) == CitizenInstance.Flags.None)
        {
            ushort eventIndex = 0;
            if (data.m_citizen != 0 && Singleton<CitizenManager>.instance.m_citizens.m_buffer[data.m_citizen].m_workBuilding != targetIndex)
            {
                eventIndex = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetIndex].m_eventIndex;
            }
            Color32 eventCitizenColor = Singleton<EventManager>.instance.GetEventCitizenColor(eventIndex, data.m_citizen);
            if (eventCitizenColor.a == 255)
            {
                data.m_color = eventCitizenColor;
                data.m_flags |= CitizenInstance.Flags.CustomColor;
            }
        }
        if (!StartPathFind(instanceID, ref data))
        {
            data.Unspawn(instanceID);
        }
    }

    public override void BuildingRelocated(ushort instanceID, ref CitizenInstance data, ushort building)
    {
        base.BuildingRelocated(instanceID, ref data, building);
        if (building == data.m_targetBuilding && (data.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.None)
        {
            base.InvalidPath(instanceID, ref data);
        }
    }

    public override void JoinTarget(ushort instanceID, ref CitizenInstance data, ushort otherInstance)
    {
        ushort num = 0;
        bool flag = false;
        bool flag2 = false;
        if (otherInstance != 0)
        {
            num = Singleton<CitizenManager>.instance.m_instances.m_buffer[otherInstance].m_targetBuilding;
            flag = ((Singleton<CitizenManager>.instance.m_instances.m_buffer[otherInstance].m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None);
            flag2 = ((Singleton<CitizenManager>.instance.m_instances.m_buffer[otherInstance].m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None);
        }
        if (num != data.m_targetBuilding || flag != ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != CitizenInstance.Flags.None))
        {
            if (data.m_targetBuilding != 0)
            {
                if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
                {
                    Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                    data.m_flags &= ~(CitizenInstance.Flags.TargetIsNode | CitizenInstance.Flags.OnTour);
                }
                else
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].RemoveTargetCitizen(instanceID, ref data);
                }
            }
            data.m_targetBuilding = num;
            if (flag)
            {
                data.m_flags |= CitizenInstance.Flags.TargetIsNode;
            }
            if (flag2)
            {
                data.m_flags |= CitizenInstance.Flags.OnTour;
            }
            if (data.m_targetBuilding != 0)
            {
                if ((data.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
                {
                    Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                }
                else
                {
                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].AddTargetCitizen(instanceID, ref data);
                }
            }
        }
        if (otherInstance != 0)
        {
            PathManager instance = Singleton<PathManager>.instance;
            CitizenManager instance2 = Singleton<CitizenManager>.instance;
            data.Unspawn(instanceID);
            data.m_frame3 = (data.m_frame2 = (data.m_frame1 = (data.m_frame0 = instance2.m_instances.m_buffer[otherInstance].GetLastFrameData())));
            data.m_targetPos = instance2.m_instances.m_buffer[otherInstance].m_targetPos;
            uint path = instance2.m_instances.m_buffer[otherInstance].m_path;
            if (instance.AddPathReference(path))
            {
                if (data.m_path != 0)
                {
                    instance.ReleasePath(data.m_path);
                }
                data.m_path = path;
                if ((instance2.m_instances.m_buffer[otherInstance].m_flags & CitizenInstance.Flags.WaitingPath) != 0)
                {
                    data.m_flags |= CitizenInstance.Flags.WaitingPath;
                }
                else
                {
                    data.Spawn(instanceID);
                }
            }
        }
    }

    private bool IsRoadConnection(ushort building)
    {
        if (building != 0)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if ((instance.m_buildings.m_buffer[building].m_flags & Building.Flags.IncomingOutgoing) != 0 && instance.m_buildings.m_buffer[building].Info.m_class.m_service == ItemClass.Service.Road)
            {
                return true;
            }
        }
        return false;
    }

    protected override bool SpawnVehicle(ushort instanceID, ref CitizenInstance citizenData, PathUnit.Position pathPos)
    {
        VehicleManager instance = Singleton<VehicleManager>.instance;
        float num = 20f;
        int num2 = Mathf.Max((int)((citizenData.m_targetPos.x - num) / 32f + 270f), 0);
        int num3 = Mathf.Max((int)((citizenData.m_targetPos.z - num) / 32f + 270f), 0);
        int num4 = Mathf.Min((int)((citizenData.m_targetPos.x + num) / 32f + 270f), 539);
        int num5 = Mathf.Min((int)((citizenData.m_targetPos.z + num) / 32f + 270f), 539);
        for (int i = num3; i <= num5; i++)
        {
            for (int j = num2; j <= num4; j++)
            {
                ushort num6 = instance.m_vehicleGrid[i * 540 + j];
                int num7 = 0;
                while (num6 != 0)
                {
                    if (TryJoinVehicle(instanceID, ref citizenData, num6, ref instance.m_vehicles.m_buffer[num6]))
                    {
                        citizenData.m_flags |= CitizenInstance.Flags.EnteringVehicle;
                        citizenData.m_flags &= ~CitizenInstance.Flags.TryingSpawnVehicle;
                        citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                        citizenData.m_waitCounter = 0;
                        return true;
                    }
                    num6 = instance.m_vehicles.m_buffer[num6].m_nextGridVehicle;
                    if (++num7 > 16384)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }
        NetManager instance2 = Singleton<NetManager>.instance;
        CitizenManager instance3 = Singleton<CitizenManager>.instance;
        Vector3 vector = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        ushort num8 = instance3.m_citizens.m_buffer[citizenData.m_citizen].m_parkedVehicle;
        if (num8 != 0)
        {
            vector = instance.m_parkedVehicles.m_buffer[num8].m_position;
            rotation = instance.m_parkedVehicles.m_buffer[num8].m_rotation;
        }
        VehicleInfo vehicleInfo;
        VehicleInfo vehicleInfo2 = GetVehicleInfo(instanceID, ref citizenData, false, out vehicleInfo);
        if ((object)vehicleInfo2 != null && vehicleInfo2.m_vehicleType != VehicleInfo.VehicleType.Bicycle)
        {
            if (vehicleInfo2.m_class.m_subService == ItemClass.SubService.PublicTransportTaxi)
            {
                instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
                if ((citizenData.m_flags & CitizenInstance.Flags.WaitingTaxi) == CitizenInstance.Flags.None && instance2.m_segments.m_buffer[pathPos.m_segment].Info.m_hasPedestrianLanes)
                {
                    citizenData.m_flags |= CitizenInstance.Flags.WaitingTaxi;
                    citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                    citizenData.m_waitCounter = 0;
                }
                return true;
            }
            uint laneID = PathManager.GetLaneID(pathPos);
            Vector3 vector2 = citizenData.m_targetPos;
            if (num8 != 0 && Vector3.SqrMagnitude(vector - vector2) < 1024f)
            {
                vector2 = vector;
            }
            else
            {
                num8 = 0;
            }
            instance2.m_lanes.m_buffer[laneID].GetClosestPosition(vector2, out Vector3 vector3, out float num9);
            byte lastPathOffset = (byte)Mathf.Clamp(Mathf.RoundToInt(num9 * 255f), 0, 255);
            vector3 = vector2 + Vector3.ClampMagnitude(vector3 - vector2, 5f);
            if (instance.CreateVehicle(out ushort num10, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo2, vector2, TransferManager.TransferReason.None, false, false))
            {
                Vehicle.Frame frame = instance.m_vehicles.m_buffer[num10].m_frame0;
                if (num8 != 0)
                {
                    frame.m_rotation = rotation;
                }
                else
                {
                    Vector3 a = vector3;
                    CitizenInstance.Frame lastFrameData = citizenData.GetLastFrameData();
                    Vector3 forward = a - lastFrameData.m_position;
                    if (forward.sqrMagnitude > 0.01f)
                    {
                        frame.m_rotation = Quaternion.LookRotation(forward);
                    }
                }
                instance.m_vehicles.m_buffer[num10].m_frame0 = frame;
                instance.m_vehicles.m_buffer[num10].m_frame1 = frame;
                instance.m_vehicles.m_buffer[num10].m_frame2 = frame;
                instance.m_vehicles.m_buffer[num10].m_frame3 = frame;
                vehicleInfo2.m_vehicleAI.FrameDataUpdated(num10, ref instance.m_vehicles.m_buffer[num10], ref frame);
                instance.m_vehicles.m_buffer[num10].m_targetPos0 = new Vector4(vector3.x, vector3.y, vector3.z, 2f);
                instance.m_vehicles.m_buffer[num10].m_flags |= Vehicle.Flags.Stopped;
                instance.m_vehicles.m_buffer[num10].m_path = citizenData.m_path;
                instance.m_vehicles.m_buffer[num10].m_pathPositionIndex = citizenData.m_pathPositionIndex;
                instance.m_vehicles.m_buffer[num10].m_lastPathOffset = lastPathOffset;
                instance.m_vehicles.m_buffer[num10].m_transferSize = (ushort)(citizenData.m_citizen & 0xFFFF);
                if ((object)vehicleInfo != null)
                {
                    instance.m_vehicles.m_buffer[num10].CreateTrailer(num10, vehicleInfo, false);
                }
                vehicleInfo2.m_vehicleAI.TrySpawn(num10, ref instance.m_vehicles.m_buffer[num10]);
                if (num8 != 0)
                {
                    InstanceID empty = InstanceID.Empty;
                    empty.ParkedVehicle = num8;
                    InstanceID empty2 = InstanceID.Empty;
                    empty2.Vehicle = num10;
                    Singleton<InstanceManager>.instance.ChangeInstance(empty, empty2);
                }
                citizenData.m_path = 0u;
                instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
                instance3.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, num10, 0u);
                citizenData.m_flags |= CitizenInstance.Flags.EnteringVehicle;
                citizenData.m_flags &= ~CitizenInstance.Flags.TryingSpawnVehicle;
                citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                citizenData.m_waitCounter = 0;
                return true;
            }
            instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
            if ((citizenData.m_flags & CitizenInstance.Flags.TryingSpawnVehicle) == CitizenInstance.Flags.None)
            {
                citizenData.m_flags |= CitizenInstance.Flags.TryingSpawnVehicle;
                citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
                citizenData.m_waitCounter = 0;
            }
            return true;
        }
        instance3.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
        if ((citizenData.m_flags & CitizenInstance.Flags.TryingSpawnVehicle) == CitizenInstance.Flags.None)
        {
            citizenData.m_flags |= CitizenInstance.Flags.TryingSpawnVehicle;
            citizenData.m_flags &= ~CitizenInstance.Flags.BoredOfWaiting;
            citizenData.m_waitCounter = 0;
        }
        return true;
    }

    protected override bool SpawnBicycle(ushort instanceID, ref CitizenInstance citizenData, PathUnit.Position pathPos)
    {
        VehicleInfo vehicleInfo;
        VehicleInfo vehicleInfo2 = GetVehicleInfo(instanceID, ref citizenData, false, out vehicleInfo);
        if ((object)vehicleInfo2 != null && vehicleInfo2.m_vehicleType == VehicleInfo.VehicleType.Bicycle)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            CitizenInstance.Frame lastFrameData = citizenData.GetLastFrameData();
            if (instance2.CreateVehicle(out ushort num, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo2, lastFrameData.m_position, TransferManager.TransferReason.None, false, false))
            {
                Vehicle.Frame frame = instance2.m_vehicles.m_buffer[num].m_frame0;
                frame.m_rotation = lastFrameData.m_rotation;
                instance2.m_vehicles.m_buffer[num].m_frame0 = frame;
                instance2.m_vehicles.m_buffer[num].m_frame1 = frame;
                instance2.m_vehicles.m_buffer[num].m_frame2 = frame;
                instance2.m_vehicles.m_buffer[num].m_frame3 = frame;
                vehicleInfo2.m_vehicleAI.FrameDataUpdated(num, ref instance2.m_vehicles.m_buffer[num], ref frame);
                if ((object)vehicleInfo != null)
                {
                    instance2.m_vehicles.m_buffer[num].CreateTrailer(num, vehicleInfo, false);
                }
                vehicleInfo2.m_vehicleAI.TrySpawn(num, ref instance2.m_vehicles.m_buffer[num]);
                instance.m_citizens.m_buffer[citizenData.m_citizen].SetParkedVehicle(citizenData.m_citizen, 0);
                instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, num, 0u);
                citizenData.m_flags |= CitizenInstance.Flags.RidingBicycle;
                return true;
            }
        }
        return false;
    }

    private bool TryJoinVehicle(ushort instanceID, ref CitizenInstance citizenData, ushort vehicleID, ref Vehicle vehicleData)
    {
        if ((vehicleData.m_flags & Vehicle.Flags.Stopped) == (Vehicle.Flags)0)
        {
            return false;
        }
        CitizenManager instance = Singleton<CitizenManager>.instance;
        uint num = vehicleData.m_citizenUnits;
        int num2 = 0;
        while (num != 0)
        {
            uint nextUnit = instance.m_units.m_buffer[num].m_nextUnit;
            for (int i = 0; i < 5; i++)
            {
                uint citizen = instance.m_units.m_buffer[num].GetCitizen(i);
                if (citizen != 0)
                {
                    ushort instance2 = instance.m_citizens.m_buffer[citizen].m_instance;
                    if (instance2 == 0)
                    {
                        break;
                    }
                    if (instance.m_instances.m_buffer[instance2].m_targetBuilding != citizenData.m_targetBuilding)
                    {
                        break;
                    }
                    if ((instance.m_instances.m_buffer[instance2].m_flags & CitizenInstance.Flags.TargetIsNode) != (citizenData.m_flags & CitizenInstance.Flags.TargetIsNode))
                    {
                        break;
                    }
                    instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, vehicleID, 0u);
                    if (instance.m_citizens.m_buffer[citizenData.m_citizen].m_vehicle != vehicleID)
                    {
                        break;
                    }
                    if (citizenData.m_path != 0)
                    {
                        Singleton<PathManager>.instance.ReleasePath(citizenData.m_path);
                        citizenData.m_path = 0u;
                    }
                    return true;
                }
            }
            num = nextUnit;
            if (++num2 > 524288)
            {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                break;
            }
        }
        return false;
    }

    protected override void SwitchBuildingTargetPos(ushort instanceID, ref CitizenInstance citizenData)
    {
        if (citizenData.m_path == 0 && citizenData.m_targetBuilding != 0 && (citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.None)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[citizenData.m_targetBuilding].Info;
            if (info.m_hasPedestrianPaths)
            {
                Randomizer randomizer = new Randomizer(instanceID << 8 | citizenData.m_targetSeed);
                info.m_buildingAI.CalculateUnspawnPosition(citizenData.m_targetBuilding, ref instance.m_buildings.m_buffer[citizenData.m_targetBuilding], ref randomizer, base.m_info, instanceID, out Vector3 _, out Vector3 vector2, out Vector2 _, out CitizenInstance.Flags _);
                float num = Vector3.Distance(citizenData.m_targetPos, vector2);
                if (num > 10f)
                {
                    base.StartPathFind(instanceID, ref citizenData, citizenData.m_targetPos, vector2, null, true, false);
                }
            }
        }
    }

    public override void EnterParkArea(ushort instanceID, ref CitizenInstance citizenData, byte park, ushort gateID)
    {
        if (gateID != 0)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            instance.m_parks.m_buffer[park].m_tempResidentCount++;
        }
        base.EnterParkArea(instanceID, ref citizenData, park, gateID);
    }

    protected override bool StartPathFind(ushort instanceID, ref CitizenInstance citizenData)
    {
        if (citizenData.m_citizen != 0)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            ushort vehicle = instance.m_citizens.m_buffer[citizenData.m_citizen].m_vehicle;
            if (vehicle != 0)
            {
                VehicleInfo info = instance2.m_vehicles.m_buffer[vehicle].Info;
                if ((object)info != null)
                {
                    uint citizen = info.m_vehicleAI.GetOwnerID(vehicle, ref instance2.m_vehicles.m_buffer[vehicle]).Citizen;
                    if (citizen == citizenData.m_citizen)
                    {
                        info.m_vehicleAI.SetTarget(vehicle, ref instance2.m_vehicles.m_buffer[vehicle], 0);
                        return false;
                    }
                }
                bool flag = false;
                if (instance2.m_vehicles.m_buffer[vehicle].m_transportLine != 0)
                {
                    NetManager instance3 = Singleton<NetManager>.instance;
                    ushort targetBuilding = instance2.m_vehicles.m_buffer[vehicle].m_targetBuilding;
                    if (targetBuilding != 0)
                    {
                        uint lane = instance3.m_nodes.m_buffer[targetBuilding].m_lane;
                        int laneOffset = instance3.m_nodes.m_buffer[targetBuilding].m_laneOffset;
                        if (lane != 0)
                        {
                            ushort segment = instance3.m_lanes.m_buffer[lane].m_segment;
                            if (instance3.m_segments.m_buffer[segment].GetClosestLane(lane, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, out lane, out NetInfo.Lane _))
                            {
                                citizenData.m_targetPos = instance3.m_lanes.m_buffer[lane].CalculatePosition((float)laneOffset * 0.003921569f);
                                flag = true;
                            }
                        }
                    }
                }
                if (!flag)
                {
                    instance.m_citizens.m_buffer[citizenData.m_citizen].SetVehicle(citizenData.m_citizen, 0, 0u);
                    return false;
                }
            }
        }
        if (citizenData.m_targetBuilding != 0)
        {
            VehicleInfo vehicleInfo;
            VehicleInfo vehicleInfo2 = GetVehicleInfo(instanceID, ref citizenData, false, out vehicleInfo);
            if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) != 0)
            {
                NetManager instance4 = Singleton<NetManager>.instance;
                Vector3 endPos = instance4.m_nodes.m_buffer[citizenData.m_targetBuilding].m_position;
                uint lane3 = instance4.m_nodes.m_buffer[citizenData.m_targetBuilding].m_lane;
                if (lane3 != 0)
                {
                    ushort segment2 = instance4.m_lanes.m_buffer[lane3].m_segment;
                    if (instance4.m_segments.m_buffer[segment2].GetClosestLane(lane3, NetInfo.LaneType.Pedestrian, VehicleInfo.VehicleType.None, out lane3, out NetInfo.Lane _))
                    {
                        int laneOffset2 = instance4.m_nodes.m_buffer[citizenData.m_targetBuilding].m_laneOffset;
                        endPos = instance4.m_lanes.m_buffer[lane3].CalculatePosition((float)laneOffset2 * 0.003921569f);
                    }
                }
                return base.StartPathFind(instanceID, ref citizenData, citizenData.m_targetPos, endPos, vehicleInfo2, true, false);
            }
            BuildingManager instance5 = Singleton<BuildingManager>.instance;
            BuildingInfo info2 = instance5.m_buildings.m_buffer[citizenData.m_targetBuilding].Info;
            Randomizer randomizer = new Randomizer(instanceID << 8 | citizenData.m_targetSeed);
            info2.m_buildingAI.CalculateUnspawnPosition(citizenData.m_targetBuilding, ref instance5.m_buildings.m_buffer[citizenData.m_targetBuilding], ref randomizer, base.m_info, instanceID, out Vector3 _, out Vector3 endPos2, out Vector2 _, out CitizenInstance.Flags _);
            return base.StartPathFind(instanceID, ref citizenData, citizenData.m_targetPos, endPos2, vehicleInfo2, true, false);
        }
        return false;
    }

    protected override VehicleInfo GetVehicleInfo(ushort instanceID, ref CitizenInstance citizenData, bool forceProbability, out VehicleInfo trailer)
    {
        trailer = null;
        if (citizenData.m_citizen != 0)
        {
            Citizen.AgeGroup ageGroup;
            switch (base.m_info.m_agePhase)
            {
            case Citizen.AgePhase.Child:
                ageGroup = Citizen.AgeGroup.Child;
                break;
            case Citizen.AgePhase.Teen0:
            case Citizen.AgePhase.Teen1:
                ageGroup = Citizen.AgeGroup.Teen;
                break;
            case Citizen.AgePhase.Young0:
            case Citizen.AgePhase.Young1:
            case Citizen.AgePhase.Young2:
                ageGroup = Citizen.AgeGroup.Young;
                break;
            case Citizen.AgePhase.Adult0:
            case Citizen.AgePhase.Adult1:
            case Citizen.AgePhase.Adult2:
            case Citizen.AgePhase.Adult3:
                ageGroup = Citizen.AgeGroup.Adult;
                break;
            case Citizen.AgePhase.Senior0:
            case Citizen.AgePhase.Senior1:
            case Citizen.AgePhase.Senior2:
            case Citizen.AgePhase.Senior3:
                ageGroup = Citizen.AgeGroup.Senior;
                break;
            default:
                ageGroup = Citizen.AgeGroup.Adult;
                break;
            }
            int num;
            int num2;
            if (forceProbability || (citizenData.m_flags & CitizenInstance.Flags.BorrowCar) != 0)
            {
                num = 100;
                num2 = 0;
            }
            else
            {
                num = GetCarProbability(instanceID, ref citizenData, ageGroup);
                num2 = GetBikeProbability(instanceID, ref citizenData, ageGroup);
            }
            Randomizer randomizer = new Randomizer(citizenData.m_citizen);
            bool flag = randomizer.Int32(100u) < num;
            bool flag2 = randomizer.Int32(100u) < num2;
            bool flag3;
            bool flag4;
            if (flag)
            {
                int electricCarProbability = GetElectricCarProbability(instanceID, ref citizenData, base.m_info.m_agePhase);
                flag3 = false;
                flag4 = (randomizer.Int32(100u) < electricCarProbability);
            }
            else
            {
                int taxiProbability = GetTaxiProbability(instanceID, ref citizenData, ageGroup);
                flag3 = (randomizer.Int32(100u) < taxiProbability);
                flag4 = false;
            }
            ItemClass.Service service = ItemClass.Service.Residential;
            ItemClass.SubService subService = (!flag4) ? ItemClass.SubService.ResidentialLow : ItemClass.SubService.ResidentialLowEco;
            if (!flag && flag3)
            {
                service = ItemClass.Service.PublicTransport;
                subService = ItemClass.SubService.PublicTransportTaxi;
            }
            VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref randomizer, service, subService, ItemClass.Level.Level1);
            VehicleInfo randomVehicleInfo2 = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref randomizer, ItemClass.Service.Residential, ItemClass.SubService.ResidentialHigh, (ageGroup != 0) ? ItemClass.Level.Level2 : ItemClass.Level.Level1);
            if (flag2 && (object)randomVehicleInfo2 != null)
            {
                return randomVehicleInfo2;
            }
            if ((flag || flag3) && (object)randomVehicleInfo != null)
            {
                return randomVehicleInfo;
            }
            return null;
        }
        return null;
    }

    private int GetCarProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
    {
        return GetCarProbability(ageGroup);
    }

    private int GetCarProbability(Citizen.AgeGroup ageGroup)
    {
        switch (ageGroup)
        {
        case Citizen.AgeGroup.Child:
            return 0;
        case Citizen.AgeGroup.Teen:
            return 5;
        case Citizen.AgeGroup.Young:
            return 15;
        case Citizen.AgeGroup.Adult:
            return 20;
        case Citizen.AgeGroup.Senior:
            return 10;
        default:
            return 0;
        }
    }

    private int GetBikeProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
    {
        CitizenManager instance = Singleton<CitizenManager>.instance;
        uint citizen = citizenData.m_citizen;
        ushort homeBuilding = instance.m_citizens.m_buffer[citizen].m_homeBuilding;
        int num = 0;
        if (homeBuilding != 0)
        {
            Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuilding].m_position;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(position);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[district].m_cityPlanningPolicies;
            if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.EncourageBiking) != 0)
            {
                num = 10;
            }
        }
        switch (ageGroup)
        {
        case Citizen.AgeGroup.Child:
            return 40 + num;
        case Citizen.AgeGroup.Teen:
            return 30 + num;
        case Citizen.AgeGroup.Young:
            return 20 + num;
        case Citizen.AgeGroup.Adult:
            return 10 + num;
        case Citizen.AgeGroup.Senior:
            return num;
        default:
            return 0;
        }
    }

    private int GetTaxiProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgeGroup ageGroup)
    {
        switch (ageGroup)
        {
        case Citizen.AgeGroup.Child:
            return 0;
        case Citizen.AgeGroup.Teen:
            return 2;
        case Citizen.AgeGroup.Young:
            return 2;
        case Citizen.AgeGroup.Adult:
            return 4;
        case Citizen.AgeGroup.Senior:
            return 6;
        default:
            return 0;
        }
    }

    private int GetElectricCarProbability(ushort instanceID, ref CitizenInstance citizenData, Citizen.AgePhase agePhase)
    {
        CitizenManager instance = Singleton<CitizenManager>.instance;
        uint citizen = citizenData.m_citizen;
        ushort homeBuilding = instance.m_citizens.m_buffer[citizen].m_homeBuilding;
        if (homeBuilding != 0)
        {
            Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuilding].m_position;
            DistrictManager instance2 = Singleton<DistrictManager>.instance;
            byte district = instance2.GetDistrict(position);
            DistrictPolicies.CityPlanning cityPlanningPolicies = instance2.m_districts.m_buffer[district].m_cityPlanningPolicies;
            if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.ElectricCars) != 0)
            {
                return 100;
            }
        }
        switch (agePhase)
        {
        case Citizen.AgePhase.Child:
        case Citizen.AgePhase.Teen0:
        case Citizen.AgePhase.Young0:
        case Citizen.AgePhase.Adult0:
        case Citizen.AgePhase.Senior0:
            return 5;
        case Citizen.AgePhase.Teen1:
        case Citizen.AgePhase.Young1:
        case Citizen.AgePhase.Adult1:
        case Citizen.AgePhase.Senior1:
            return 10;
        case Citizen.AgePhase.Young2:
        case Citizen.AgePhase.Adult2:
        case Citizen.AgePhase.Senior2:
            return 15;
        case Citizen.AgePhase.Adult3:
        case Citizen.AgePhase.Senior3:
            return 20;
        default:
            return 0;
        }
    }
}
