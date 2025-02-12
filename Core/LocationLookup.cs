using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    internal enum LocationType
    {
        EnergyCell,
        HealthCell,
        AbilityCell,
        Skill,
        Keystone,
        Mapstone,
        Map,
        Plant,
        Event,
        Cutscene,
        ExpSmall,
        ExpMedium,
        ExpLarge,
        ProgressiveMap
    }

    internal class Location
    {
        public MoonGuid MoonGuid { get; private set; }
        public string Name { get; private set; }
        public WorldArea Area { get; private set; }
        public LocationType Type { get; private set; }
        public Vector2 WorldPosition { get; private set; }

        public Location(MoonGuid moonGuid, string name, WorldArea area, LocationType type, Vector2 worldPosition)
        {
            MoonGuid = moonGuid;
            Name = name;
            Area = area;
            Type = type;
            WorldPosition = worldPosition;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return MoonGuid.Equals((obj as Location)?.MoonGuid);
        }

        public override int GetHashCode()
        {
            return MoonGuid.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    internal class LocationLookup
    {
        public static Location Get(MoonGuid moonGuid)
        {
            // If this is the first time this is called, populate the locationByGuid dictionary
            if (locationByGuid is null)
            {
                locationByGuid = new Dictionary<MoonGuid, Location>();

                foreach (Location location in locations)
                {
                    locationByGuid.Add(location.MoonGuid, location);
                }
            }

            if (moonGuid == null) return null;

            locationByGuid.TryGetValue(moonGuid, out Location target);

            if (target == null)
            {
                Console.WriteLine("Invalid location: " + moonGuid.ToString());
            }

            return target;
        }

        public static Location Get(string name)
        {
            // If this is the first time this is called, populate the locationByName dictionary
            if (locationByName is null)
            {
                locationByName = new Dictionary<string, Location>();

                foreach (Location location in locations)
                {
                    locationByName.Add(location.Name, location);
                }
            }

            if (name == null) return null;

            locationByName.TryGetValue(name, out Location target);

            if (target == null)
            {
                Console.WriteLine("Invalid location: " + name);
            }

            return target;
        }

        private static Dictionary<string, Location> locationByName;

        private static Dictionary<MoonGuid, Location> locationByGuid;

        private static readonly List<Location> locations = new List<Location>()
        {
            new Location(new MoonGuid(new Guid("a63780b0-42b1-4774-9b29-e09860be1909")), "FirstPickup", WorldArea.Glades, LocationType.ExpSmall, new Vector2(92.0f, -227.5f)),
            new Location(new MoonGuid(new Guid("946296aa-9449-4a2b-8d71-9d8dc9354e04")), "FirstEnergyCell", WorldArea.Glades, LocationType.EnergyCell, new Vector2(-27.4f, -256.3f)),
            new Location(new MoonGuid(new Guid("6bb0cafc-6dd1-47f2-85dd-8567b3a60e15")), "FronkeyFight", WorldArea.Glades, LocationType.ExpSmall, new Vector2(-154.5f, -271.8f)),
            new Location(new MoonGuid(new Guid("df30b319-f085-4e36-9ad7-04f14f8087ee")), "Sein", WorldArea.Glades, LocationType.Skill, new Vector2(-164.1f, -261.4f)),
            new Location(new MoonGuid(new Guid("bc6b3f8d-e278-460e-86b6-7ece93e236ce")), "GladesKeystone1", WorldArea.Glades, LocationType.Keystone, new Vector2(83.1f, -222.3f)),
            new Location(new MoonGuid(new Guid("e06e9d7a-e566-4594-aba9-87b8e660a89c")), "GladesKeystone2", WorldArea.Glades, LocationType.Keystone, new Vector2(-11.1f, -206.4f)),
            new Location(new MoonGuid(new Guid("8da3969b-7ae7-4894-8801-49727a1634e7")), "GladesGrenadePool", WorldArea.Glades, LocationType.ExpLarge, new Vector2(59.0f, -280.9f)),
            new Location(new MoonGuid(new Guid("096fb36f-de47-477e-85a2-7bb9f4a8920a")), "GladesGrenadeTree", WorldArea.Glades, LocationType.AbilityCell, new Vector2(83.0f, -196.8f)),
            new Location(new MoonGuid(new Guid("f648eef0-1c8b-449b-bf87-8260ba65ffab")), "GladesMainPool", WorldArea.Glades, LocationType.ExpMedium, new Vector2(5.7f, -241.8f)),
            new Location(new MoonGuid(new Guid("5c65dca8-1dab-410f-a03a-a6cdacb3a273")), "GladesMainPoolDeep", WorldArea.Glades, LocationType.EnergyCell, new Vector2(-40.1f, -239.8f)),
            new Location(new MoonGuid(new Guid("aec98a28-a213-4a32-9149-9189ad294a82")), "FronkeyWalkRoof", WorldArea.Glades, LocationType.ExpLarge, new Vector2(257.1f, -199.7f)),
            new Location(new MoonGuid(new Guid("5d7a6107-ca33-4820-a7cf-0a9db233d16c")), "FourthHealthCell", WorldArea.Glades, LocationType.HealthCell, new Vector2(-80.5f, -189.0f)),
            new Location(new MoonGuid(new Guid("b4d6891b-6985-44b7-a38a-7207a2c0eb9a")), "GladesMapKeystone", WorldArea.Glades, LocationType.Keystone, new Vector2(-59.4f, -244.3f)),
            new Location(new MoonGuid(new Guid("9be6ea7c-ea0e-4127-bf98-7d1ab19d73dd")), "WallJumpSkillTree", WorldArea.Glades, LocationType.Skill, new Vector2(-316.0f, -308.0f)),
            new Location(new MoonGuid(new Guid("32cc3987-9ef6-461d-ae6a-f38f3288bb5d")), "LeftGladesHiddenExp", WorldArea.Glades, LocationType.ExpSmall, new Vector2(-283.4f, -236.4f)),
            new Location(new MoonGuid(new Guid("3cb3ac42-c9ca-47be-a6dd-b5415220b07b")), "DeathGauntletExp", WorldArea.Grove, LocationType.ExpMedium, new Vector2(303.4f, -190.5f)),
            new Location(new MoonGuid(new Guid("227e146b-13d0-4043-b0b7-f3a2ceacaa6e")), "DeathGauntletEnergyCell", WorldArea.Grotto, LocationType.EnergyCell, new Vector2(423.8f, -169.2f)),
            new Location(new MoonGuid(new Guid("df93a803-a0b8-4c29-a46f-894babb371c9")), "GladesMap", WorldArea.Glades, LocationType.Map, new Vector2(-81.0f, -248.0f)),
            new Location(new MoonGuid(new Guid("febeb52a-4b83-4385-b0f2-c8c3a863f646")), "AboveFourthHealth", WorldArea.Glades, LocationType.AbilityCell, new Vector2(-48.3f, -166.0f)),
            new Location(new MoonGuid(new Guid("20aecfe4-bb54-42ef-b108-365f1bd2d2d2")), "WallJumpAreaExp", WorldArea.Glades, LocationType.ExpLarge, new Vector2(-245.7f, -277.0f)),
            new Location(new MoonGuid(new Guid("15e6e704-70cf-4410-ae45-00ee9e9e74e1")), "WallJumpAreaEnergyCell", WorldArea.Glades, LocationType.EnergyCell, new Vector2(-336.8f, -288.2f)),
            new Location(new MoonGuid(new Guid("3923d025-375d-4e4a-ab65-b4be0b95b47e")), "LeftGladesExp", WorldArea.Glades, LocationType.ExpSmall, new Vector2(-247.1f, -207.1f)),
            new Location(new MoonGuid(new Guid("0e87e1d6-3fb6-47c1-a996-e54429ddf5b0")), "LeftGladesKeystone", WorldArea.Glades, LocationType.Keystone, new Vector2(-238.3f, -212.4f)),
            new Location(new MoonGuid(new Guid("39f08f0c-9eb5-468b-a132-a1f325df358b")), "LeftGladesMapstone", WorldArea.Glades, LocationType.Mapstone, new Vector2(-184.9f, -227.6f)),
            new Location(new MoonGuid(new Guid("9135c75e-d9c0-45ca-9b05-e4ac4bbe75fb")), "SpiritCavernsKeystone1", WorldArea.Glades, LocationType.Keystone, new Vector2(-182.1f, -194.0f)),
            new Location(new MoonGuid(new Guid("927cd88f-8daf-462f-8e0f-5f5886fc96c4")), "SpiritCavernsKeystone2", WorldArea.Glades, LocationType.Keystone, new Vector2(-217.4f, -183.9f)),
            new Location(new MoonGuid(new Guid("9f4f6208-7d94-42b3-a9c0-7fa64ff186f3")), "SpiritCavernsTopRightKeystone", WorldArea.Glades, LocationType.Keystone, new Vector2(-177.9f, -154.5f)),
            new Location(new MoonGuid(new Guid("d61613b4-d469-40ab-afe7-d5ffd438ce90")), "SpiritCavernsTopLeftKeystone", WorldArea.Glades, LocationType.Keystone, new Vector2(-217.5f, -146.3f)),
            new Location(new MoonGuid(new Guid("3d70c596-68de-4606-8f2e-c132ac4021f6")), "SpiritCavernsAbilityCell", WorldArea.Glades, LocationType.AbilityCell, new Vector2(-216.4f, -176.4f)),
            new Location(new MoonGuid(new Guid("b905425d-a894-48ae-9c27-0b21e2b11b53")), "GladesLaser", WorldArea.Glades, LocationType.EnergyCell, new Vector2(-155.0f, -186.0f)),
            new Location(new MoonGuid(new Guid("4319a75b-01d6-49c0-9bac-1d10f5c59a82")), "GladesLaserGrenade", WorldArea.Glades, LocationType.AbilityCell, new Vector2(-165.4f, -140.4f)),
            new Location(new MoonGuid(new Guid("9887f949-0a02-4ca0-974d-fe400569ece9")), "SpiritTreeExp", WorldArea.Grove, LocationType.ExpMedium, new Vector2(-168.1f, -103.0f)),
            new Location(new MoonGuid(new Guid("db35e94b-0392-48ff-990e-c24befa00a2e")), "ChargeFlameSkillTree", WorldArea.Grove, LocationType.Skill, new Vector2(-56.0f, -160.0f)),
            new Location(new MoonGuid(new Guid("50e3ac87-6cb0-46a5-9b6d-61fc512bf491")), "ChargeFlameAreaPlant", WorldArea.Grove, LocationType.Plant, new Vector2(43.0f, -156.0f)),
            new Location(new MoonGuid(new Guid("31ce9625-ba55-4bb3-87c0-9d4350a7e512")), "ChargeFlameAreaExp", WorldArea.Glades, LocationType.ExpMedium, new Vector2(5.0f, -193.2f)),
            new Location(new MoonGuid(new Guid("7bbd835d-c1cb-4144-be49-5df3d7e76835")), "AboveChargeFlameTreeExp", WorldArea.Grove, LocationType.ExpMedium, new Vector2(-14.2f, -95.7f)),
            new Location(new MoonGuid(new Guid("9a078c11-d254-4bef-823f-9b8c9a224936")), "SpiderSacEnergyDoor", WorldArea.Grove, LocationType.AbilityCell, new Vector2(64.0f, -109.4f)),
            new Location(new MoonGuid(new Guid("41d64105-b524-4a62-85fd-a44e0c458ff8")), "SpiderSacHealthCell", WorldArea.Grove, LocationType.HealthCell, new Vector2(151.8f, -117.8f)),
            new Location(new MoonGuid(new Guid("e03c5ccc-09ed-4f74-9aa2-82335dd6f538")), "SpiderSacEnergyCell", WorldArea.Grove, LocationType.EnergyCell, new Vector2(60.7f, -155.8f)),
            new Location(new MoonGuid(new Guid("82e91575-ec36-4bfa-b050-e87fc3684cb5")), "SpiderSacGrenadeDoor", WorldArea.Grove, LocationType.AbilityCell, new Vector2(93.4f, -92.5f)),
            new Location(new MoonGuid(new Guid("fac86702-fb8e-4f69-9c10-875ca4c054d3")), "DashAreaOrbRoomExp", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(154.0f, -291.5f)),
            new Location(new MoonGuid(new Guid("a0e20125-fd8d-403e-8f05-18bc816178fb")), "DashAreaAbilityCell", WorldArea.Blackroot, LocationType.AbilityCell, new Vector2(183.8f, -291.5f)),
            new Location(new MoonGuid(new Guid("a8959482-6024-4de2-9c4a-85904321498a")), "DashAreaRoofExp", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(197.3f, -229.1f)),
            new Location(new MoonGuid(new Guid("bb0847e0-a88b-4449-9bf6-f9944e5024df")), "DashSkillTree", WorldArea.Blackroot, LocationType.Skill, new Vector2(292.0f, -256.0f)),
            new Location(new MoonGuid(new Guid("a334b21b-691d-4b79-b149-4027b2aed3e5")), "DashAreaPlant", WorldArea.Blackroot, LocationType.Plant, new Vector2(313.0f, -232.0f)),
            new Location(new MoonGuid(new Guid("821112d5-830d-4dd0-a16c-6f574a4122aa")), "RazielNo", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(304.4f, -303.2f)),
            new Location(new MoonGuid(new Guid("c09a7bd5-d703-40ec-92b9-c3a840478aa8")), "DashAreaMapstone", WorldArea.Blackroot, LocationType.Mapstone, new Vector2(346.0f, -255.0f)),
            new Location(new MoonGuid(new Guid("b73d5139-430f-4076-8357-91638db8aa4c")), "BlackrootTeleporterHealthCell", WorldArea.Blackroot, LocationType.HealthCell, new Vector2(395.0f, -309.1f)),
            new Location(new MoonGuid(new Guid("6b12d651-6eae-46b3-9949-94fa73404a45")), "BlackrootMap", WorldArea.Blackroot, LocationType.Map, new Vector2(418.0f, -291.0f)),
            new Location(new MoonGuid(new Guid("965ca68c-a41d-47a4-ac63-64a932c04177")), "BlackrootBoulderExp", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(432.0f, -324.0f)),
            new Location(new MoonGuid(new Guid("a90db45e-56b4-405b-80d6-83bc4607b921")), "GrenadeSkillTree", WorldArea.Blackroot, LocationType.Skill, new Vector2(72.0f, -380.0f)),
            new Location(new MoonGuid(new Guid("f9f705a1-e2c4-4856-a6b3-ce08a0b15807")), "GrenadeAreaExp", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(224.0f, -359.1f)),
            new Location(new MoonGuid(new Guid("bcd84bf8-2b02-442a-bd2d-798687ead2aa")), "GrenadeAreaAbilityCell", WorldArea.Blackroot, LocationType.AbilityCell, new Vector2(252.4f, -331.9f)),
            new Location(new MoonGuid(new Guid("07983731-614c-4123-8bec-1eb036fbdceb")), "LowerBlackrootAbilityCell", WorldArea.Blackroot, LocationType.AbilityCell, new Vector2(279.0f, -375.0f)),
            new Location(new MoonGuid(new Guid("66dabcc6-dafa-47a4-9c2d-0be6f915198c")), "LowerBlackrootLaserAbilityCell", WorldArea.Blackroot, LocationType.AbilityCell, new Vector2(391.5f, -423.0f)),
            new Location(new MoonGuid(new Guid("e9cec978-351e-4ab4-b37b-c3d90b11e17e")), "LowerBlackrootLaserExp", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(339.0f, -418.8f)),
            new Location(new MoonGuid(new Guid("d8e6c0cd-f194-4913-8776-f23882161070")), "LowerBlackrootGrenadeThrow", WorldArea.Blackroot, LocationType.AbilityCell, new Vector2(208.5f, -431.5f)),
            new Location(new MoonGuid(new Guid("b4f9a336-4147-49e4-8ea6-83d46493c154")), "LostGroveAbilityCell", WorldArea.Blackroot, LocationType.AbilityCell, new Vector2(459.5f, -506.8f)),
            new Location(new MoonGuid(new Guid("c06b2949-b01a-4c67-a44f-ae2c9476ebb8")), "LostGroveHiddenExp", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(462.4f, -489.5f)),
            new Location(new MoonGuid(new Guid("aa4ee768-a4ab-456f-8c38-4d7433ca6c81")), "LostGroveTeleporter", WorldArea.Blackroot, LocationType.ExpMedium, new Vector2(307.3f, -525.2f)),
            new Location(new MoonGuid(new Guid("1a3b17d1-5331-4315-885c-c4a012e527f4")), "LostGroveLongSwim", WorldArea.Blackroot, LocationType.AbilityCell, new Vector2(527.0f, -544.0f)),
            new Location(new MoonGuid(new Guid("f1d27bac-0911-4a0e-8901-626982bb909e")), "HollowGroveMapstone", WorldArea.Grove, LocationType.Mapstone, new Vector2(300.0f, -94.0f)),
            new Location(new MoonGuid(new Guid("6e75862e-6c9f-488a-b4cf-760dcaa22f9e")), "OuterSwampAbilityCell", WorldArea.Swamp, LocationType.AbilityCell, new Vector2(703.2f, -82.3f)),
            new Location(new MoonGuid(new Guid("5e0ff4fa-86c1-4ac6-afd3-94734b0ec95b")), "OuterSwampStompExp", WorldArea.Swamp, LocationType.ExpMedium, new Vector2(618.5f, -98.0f)),
            new Location(new MoonGuid(new Guid("c7978ab3-94f4-4040-b4a6-0d254d03017a")), "OuterSwampHealthCell", WorldArea.Swamp, LocationType.HealthCell, new Vector2(581.5f, -67.0f)),
            new Location(new MoonGuid(new Guid("fd4e2fed-e523-4327-9db6-667088becf80")), "HollowGroveMap", WorldArea.Grove, LocationType.Map, new Vector2(351.0f, -119.0f)),
            new Location(new MoonGuid(new Guid("ffb8ac3d-8be4-4dbf-aa37-c7cec997d207")), "HollowGroveTreeAbilityCell", WorldArea.Grove, LocationType.AbilityCell, new Vector2(333.1f, -61.9f)),
            new Location(new MoonGuid(new Guid("8f928efd-0550-4c86-835a-d813bd8aacd3")), "HollowGroveMapPlant", WorldArea.Grove, LocationType.Plant, new Vector2(365.0f, -119.0f)),
            new Location(new MoonGuid(new Guid("5a507a02-ede0-4720-8db8-d1a0e3e99929")), "HollowGroveTreePlant", WorldArea.Grove, LocationType.Plant, new Vector2(330.0f, -78.0f)),
            new Location(new MoonGuid(new Guid("216d4b2e-843f-462d-a04a-155da8b2938f")), "SwampEntrancePlant", WorldArea.Swamp, LocationType.Plant, new Vector2(628.0f, -120.0f)),
            new Location(new MoonGuid(new Guid("f647b2e0-1032-4cea-9fa9-f13e0ffa660f")), "MoonGrottoStompPlant", WorldArea.Grotto, LocationType.Plant, new Vector2(435.0f, -140.0f)),
            new Location(new MoonGuid(new Guid("07e6b6ae-01ba-42cf-b468-b32d2347feb3")), "OuterSwampMortarPlant", WorldArea.Swamp, LocationType.Plant, new Vector2(515.0f, -100.0f)),
            new Location(new MoonGuid(new Guid("5db1c38e-62da-4839-b5dd-1b28db826a1c")), "GroveWaterStompAbilityCell", WorldArea.Grove, LocationType.AbilityCell, new Vector2(354.0f, -178.5f)),
            new Location(new MoonGuid(new Guid("308487d6-451c-40f4-86a2-631ea7a4ff93")), "OuterSwampGrenadeExp", WorldArea.Swamp, LocationType.ExpLarge, new Vector2(666.0f, -48.0f)),
            new Location(new MoonGuid(new Guid("3dc121cd-1460-4387-aad8-88c700d97dab")), "SwampTeleporterAbilityCell", WorldArea.Swamp, LocationType.AbilityCell, new Vector2(409.5f, -34.5f)),
            new Location(new MoonGuid(new Guid("a36910b8-8ab6-4097-8f18-3548d4bc2ebc")), "GroveAboveSpiderWaterExp", WorldArea.Grove, LocationType.ExpLarge, new Vector2(174.2f, -105.6f)),
            new Location(new MoonGuid(new Guid("4570a95a-58c8-41bd-b5b3-13948d1cecf1")), "GroveAboveSpiderWaterHealthCell", WorldArea.Grove, LocationType.HealthCell, new Vector2(261.4f, -117.7f)),
            new Location(new MoonGuid(new Guid("01216f37-d036-4d76-ab97-5036b92121b0")), "GroveAboveSpiderWaterEnergyCell", WorldArea.Grove, LocationType.EnergyCell, new Vector2(272.5f, -97.5f)),
            new Location(new MoonGuid(new Guid("111c6aa1-9b11-49d6-b20e-80af97f0c8c7")), "GroveSpiderWaterSwim", WorldArea.Grove, LocationType.ExpMedium, new Vector2(187.3f, -163.9f)),
            new Location(new MoonGuid(new Guid("cf38993c-f94f-45b8-b0f9-119e257c02d7")), "DeathGauntletSwimEnergyDoor", WorldArea.Grove, LocationType.AbilityCell, new Vector2(339.0f, -216.0f)),
            new Location(new MoonGuid(new Guid("c563573f-ebfc-4b8f-8e2d-16aebd0a8698")), "DeathGauntletStompSwim", WorldArea.Grotto, LocationType.ExpLarge, new Vector2(356.8f, -207.1f)),
            new Location(new MoonGuid(new Guid("9eeb188b-99e3-4783-bbe6-e5440fe8faca")), "AboveGrottoTeleporterExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(477.0f, -140.0f)),
            new Location(new MoonGuid(new Guid("93918644-d27f-48fa-a1ae-4587dfc1af39")), "GrottoLasersRoofExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(432.0f, -108.0f)),
            new Location(new MoonGuid(new Guid("45d1f68e-1e7b-4bad-8f20-185f34fe8c8a")), "IcelessExp", WorldArea.Grove, LocationType.ExpMedium, new Vector2(365.2f, -109.1f)),
            new Location(new MoonGuid(new Guid("7774dc60-7d2c-43cf-8be8-75a2eb8ad89e")), "BelowGrottoTeleporterPlant", WorldArea.Grotto, LocationType.Plant, new Vector2(540.0f, -220.0f)),
            new Location(new MoonGuid(new Guid("fe6f5315-4db5-42e9-b240-d25ae5e4aff2")), "LeftGrottoTeleporterExp", WorldArea.Grotto, LocationType.ExpLarge, new Vector2(450.0f, -166.8f)),
            new Location(new MoonGuid(new Guid("cb8e6952-0852-4337-84ac-4adbca2e2b73")), "OuterSwampMortarAbilityCell", WorldArea.Swamp, LocationType.AbilityCell, new Vector2(502.0f, -108.0f)),
            new Location(new MoonGuid(new Guid("56a4b631-7d43-4fda-9d5f-ac1d454bcab9")), "SwampEntranceSwim", WorldArea.Swamp, LocationType.ExpLarge, new Vector2(595.0f, -136.0f)),
            new Location(new MoonGuid(new Guid("818751c2-f958-4afa-b71d-d39f9853461d")), "BelowGrottoTeleporterHealthCell", WorldArea.Grotto, LocationType.HealthCell, new Vector2(543.6f, -189.4f)),
            new Location(new MoonGuid(new Guid("dbc518b6-ec15-4d73-88c0-0aee01e1614c")), "GrottoEnergyDoorSwim", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(423.0f, -274.0f)),
            new Location(new MoonGuid(new Guid("80ba61d0-281f-4fe8-883a-0b04320a5200")), "GrottoEnergyDoorHealthCell", WorldArea.Grotto, LocationType.HealthCell, new Vector2(424.0f, -220.0f)),
            new Location(new MoonGuid(new Guid("a3e8287b-7f53-497f-9dd2-92e6807d9f35")), "GrottoSwampDrainAccessExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(552.0f, -141.5f)),
            new Location(new MoonGuid(new Guid("97361c03-6a9c-4e7e-afc2-bbd1fff37ef8")), "GrottoSwampDrainAccessPlant", WorldArea.Grotto, LocationType.Plant, new Vector2(537.0f, -176.0f)),
            new Location(new MoonGuid(new Guid("ac4c2a10-b3c3-4805-bc4d-d8dd51b2fe34")), "GrottoHideoutFallAbilityCell", WorldArea.Grotto, LocationType.AbilityCell, new Vector2(451.0f, -296.0f)),
            new Location(new MoonGuid(new Guid("eb9be108-9834-4192-a14b-cc6ca5c5eb7a")), "GumoHideoutMapstone", WorldArea.Grotto, LocationType.Mapstone, new Vector2(513.0f, -413.0f)),
            new Location(new MoonGuid(new Guid("e2a87ba7-6179-439e-b610-0f424d600fef")), "GumoHideoutMiniboss", WorldArea.Grotto, LocationType.Keystone, new Vector2(620.0f, -404.0f)),
            new Location(new MoonGuid(new Guid("ce03775a-859f-4117-b88c-4e5cdcd63abe")), "GumoHideoutCrusherExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(572.5f, -378.5f)),
            new Location(new MoonGuid(new Guid("4cd7b2de-aa6d-4ea6-99f0-ed4748765194")), "GumoHideoutCrusherKeystone", WorldArea.Grotto, LocationType.Keystone, new Vector2(590.0f, -384.0f)),
            new Location(new MoonGuid(new Guid("e9324310-3a82-49ec-8823-1b79e828b65f")), "GumoHideoutRightHangingExp", WorldArea.Grotto, LocationType.ExpSmall, new Vector2(496.0f, -369.0f)),
            new Location(new MoonGuid(new Guid("c54266b0-44eb-4ef9-86cd-8efa8f0e3377")), "GumoHideoutLeftHangingExp", WorldArea.Grotto, LocationType.ExpSmall, new Vector2(467.5f, -369.0f)),
            new Location(new MoonGuid(new Guid("f8c47a8b-74d8-495f-8920-a2fcb9c83a0a")), "GumoHideoutRedirectAbilityCell", WorldArea.Grotto, LocationType.AbilityCell, new Vector2(449.0f, -430.0f)),
            new Location(new MoonGuid(new Guid("5051e7e8-3361-46fc-9edf-8470b28bf7df")), "GumoHideoutMap", WorldArea.Grotto, LocationType.Map, new Vector2(477.0f, -389.0f)),
            new Location(new MoonGuid(new Guid("83a91676-9217-4d5f-8ae5-ef6189d6ebfe")), "DoubleJumpSkillTree", WorldArea.Grotto, LocationType.Skill, new Vector2(784.0f, -412.0f)),
            new Location(new MoonGuid(new Guid("5601a577-4fe0-4baa-aa82-24baa01ebcd8")), "DoubleJumpAreaExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(759.0f, -398.0f)),
            new Location(new MoonGuid(new Guid("1a34da08-b071-434c-bb16-5061d90dbb23")), "GumoHideoutEnergyCell", WorldArea.Grotto, LocationType.EnergyCell, new Vector2(545.8f, -357.9f)),
            new Location(new MoonGuid(new Guid("76b64647-1a59-4fa5-8dbc-9a9db47afe6e")), "GumoHideoutRockfallExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(567.6f, -246.2f)),
            new Location(new MoonGuid(new Guid("ed6776f6-fe53-4fdc-b6c3-82ae8863a0a7")), "WaterVein", WorldArea.Grotto, LocationType.Event, new Vector2(500.0f, -248.0f)),
            new Location(new MoonGuid(new Guid("99d20fdb-93ae-4a56-8099-50db65fcc038")), "LeftGumoHideoutExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(406.9f, -386.2f)),
            new Location(new MoonGuid(new Guid("055b2efc-5c0d-48df-b459-c0597a6157a0")), "LeftGumoHideoutHealthCell", WorldArea.Grotto, LocationType.HealthCell, new Vector2(393.8f, -375.6f)),
            new Location(new MoonGuid(new Guid("f26afebd-eb6e-4632-b221-92d933a9ce71")), "LeftGumoHideoutLowerPlant", WorldArea.Grotto, LocationType.Plant, new Vector2(447.0f, -368.0f)),
            new Location(new MoonGuid(new Guid("1e5ffc6a-4955-442e-8e55-83cc6dc98bae")), "LeftGumoHideoutUpperPlant", WorldArea.Grotto, LocationType.Plant, new Vector2(439.0f, -344.0f)),
            new Location(new MoonGuid(new Guid("aae4f932-9adf-493b-a7c2-610f7155800b")), "GumoHideoutRedirectPlant", WorldArea.Grotto, LocationType.Plant, new Vector2(492.0f, -400.0f)),
            new Location(new MoonGuid(new Guid("3848b58f-6b4d-458c-82c5-d12da775c239")), "LeftGumoHideoutSwim", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(397.0f, -411.5f)),
            new Location(new MoonGuid(new Guid("191a7af3-09ce-400a-b46f-c42e3795a91a")), "GumoHideoutRedirectEnergyCell", WorldArea.Grotto, LocationType.EnergyCell, new Vector2(515.4f, -441.9f)),
            new Location(new MoonGuid(new Guid("effd752c-50ad-4ac0-95fd-fae40eee581c")), "GumoHideoutRedirectExp", WorldArea.Grotto, LocationType.ExpLarge, new Vector2(505.3f, -439.4f)),
            new Location(new MoonGuid(new Guid("5525f151-36cf-44bf-924b-8a0ccc9f7854")), "FarLeftGumoHideoutExp", WorldArea.Grotto, LocationType.ExpMedium, new Vector2(328.9f, -353.7f)),
            new Location(new MoonGuid(new Guid("d903d308-ba71-4ed4-a5a8-27d1c01250e3")), "SwampEntranceAbilityCell", WorldArea.Swamp, LocationType.AbilityCell, new Vector2(643.5f, -127.5f)),
            new Location(new MoonGuid(new Guid("981f27e8-b14d-474a-a2f0-99eb418d45a1")), "DeathGauntletRoofHealthCell", WorldArea.Grove, LocationType.HealthCell, new Vector2(321.7f, -179.1f)),
            new Location(new MoonGuid(new Guid("9b46304b-0b29-4d72-bb28-c6068ef3304b")), "DeathGauntletRoofPlant", WorldArea.Grove, LocationType.Plant, new Vector2(342.0f, -179.0f)),
            new Location(new MoonGuid(new Guid("0f0951ec-975e-446d-b434-259a40dde5cc")), "LowerGinsoHiddenExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(523.0f, 142.0f)),
            new Location(new MoonGuid(new Guid("930cc8c6-d894-4be9-8b68-5ddb33e2cc9a")), "LowerGinsoKeystone1", WorldArea.Ginso, LocationType.Keystone, new Vector2(531.0f, 267.0f)),
            new Location(new MoonGuid(new Guid("10d7d1cd-05a1-4cda-8358-ffb70309ada4")), "LowerGinsoKeystone2", WorldArea.Ginso, LocationType.Keystone, new Vector2(540.0f, 277.0f)),
            new Location(new MoonGuid(new Guid("35b2689e-8ecb-4f4d-9434-10b9bfdc778e")), "LowerGinsoKeystone3", WorldArea.Ginso, LocationType.Keystone, new Vector2(508.0f, 304.0f)),
            new Location(new MoonGuid(new Guid("dd7b36d5-af5a-44dc-8f2f-071d2729c35a")), "LowerGinsoKeystone4", WorldArea.Ginso, LocationType.Keystone, new Vector2(529.0f, 297.0f)),
            new Location(new MoonGuid(new Guid("d15bed0b-b493-4931-8822-4f8c91a782d3")), "LowerGinsoPlant", WorldArea.Ginso, LocationType.Plant, new Vector2(540.0f, 101.0f)),
            new Location(new MoonGuid(new Guid("d094cce4-3b65-454f-b5c9-7e6ed7dcc436")), "BashSkillTree", WorldArea.Ginso, LocationType.Skill, new Vector2(532.0f, 328.0f)),
            new Location(new MoonGuid(new Guid("32419a38-e045-48dc-b9af-84f192f34b68")), "BashAreaExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(518.4f, 339.8f)),
            new Location(new MoonGuid(new Guid("e5d4683c-2909-4d5a-b5a2-6184af83f66f")), "UpperGinsoLowerKeystone", WorldArea.Ginso, LocationType.Keystone, new Vector2(507.0f, 476.0f)),
            new Location(new MoonGuid(new Guid("94a7bf35-e1f1-4be6-ab6d-276d18e36fe6")), "UpperGinsoRightKeystone", WorldArea.Ginso, LocationType.Keystone, new Vector2(535.0f, 488.0f)),
            new Location(new MoonGuid(new Guid("dff38fe5-bc28-4709-875e-4cc7870484b1")), "UpperGinsoUpperRightKeystone", WorldArea.Ginso, LocationType.Keystone, new Vector2(531.0f, 502.0f)),
            new Location(new MoonGuid(new Guid("b3e67ad6-4be0-4fba-b3a9-84dca30eb78e")), "UpperGinsoUpperLeftKeystone", WorldArea.Ginso, LocationType.Keystone, new Vector2(508.0f, 498.0f)),
            new Location(new MoonGuid(new Guid("290b4d70-e069-4bab-948a-473bdb723377")), "UpperGinsoRedirectLowerExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(517.1f, 384.4f)),
            new Location(new MoonGuid(new Guid("dad100fb-2cd5-4796-9eb3-5833e513a583")), "UpperGinsoRedirectUpperExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(530.5f, 407.0f)),
            new Location(new MoonGuid(new Guid("86516ca1-f515-4d4c-a01a-48370ad334d6")), "UpperGinsoEnergyCell", WorldArea.Ginso, LocationType.EnergyCell, new Vector2(536.6f, 434.7f)),
            new Location(new MoonGuid(new Guid("a72abfba-4389-4714-a5f4-1fd606ac7dd8")), "TopGinsoLeftLowerExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(456.9f, 566.1f)),
            new Location(new MoonGuid(new Guid("abaaf3cb-a9f3-45f0-8755-c09dc03dfdfa")), "TopGinsoLeftUpperExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(471.2f, 614.8f)),
            new Location(new MoonGuid(new Guid("008f4c83-8e59-4640-9bdd-ac60a7c121c0")), "TopGinsoRightPlant", WorldArea.Ginso, LocationType.Plant, new Vector2(610.0f, 611.0f)),
            new Location(new MoonGuid(new Guid("41f363b4-1d7b-4788-9bf0-66d56fd82b9c")), "GinsoEscapeSpiderExp", WorldArea.Ginso, LocationType.ExpLarge, new Vector2(534.5f, 661.5f)),
            new Location(new MoonGuid(new Guid("38f99d93-1c86-4ca5-a782-4c79a8f30311")), "GinsoEscapeJumpPadExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(537.5f, 733.6f)),
            new Location(new MoonGuid(new Guid("3b098597-20a4-4471-80bd-d0bf1d84ed14")), "GinsoEscapeProjectileExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(533.5f, 827.3f)),
            new Location(new MoonGuid(new Guid("fb37b950-223e-4e1e-9261-11fa135a4bde")), "GinsoEscapeHangingExp", WorldArea.Ginso, LocationType.ExpMedium, new Vector2(519.3f, 867.6f)),
            new Location(new MoonGuid(new Guid("12728fe9-7224-46aa-b315-7790e2bd8efc")), "GinsoEscapeExit", WorldArea.Ginso, LocationType.Event, new Vector2(548.0f, 952.0f)),
            new Location(new MoonGuid(new Guid("d5e3a6dc-2cd8-46da-9120-6863dfce00bd")), "SwampMap", WorldArea.Swamp, LocationType.Map, new Vector2(677.0f, -129.0f)),
            new Location(new MoonGuid(new Guid("d1306ce2-990b-4d51-bce3-2e5557927670")), "InnerSwampDrainExp", WorldArea.Swamp, LocationType.ExpMedium, new Vector2(637.0f, -162.2f)),
            new Location(new MoonGuid(new Guid("d1f5b054-1050-44dd-bd77-53cc3d430dd3")), "InnerSwampHiddenSwimExp", WorldArea.Swamp, LocationType.ExpMedium, new Vector2(761.9f, -173.5f)),
            new Location(new MoonGuid(new Guid("f7c585e3-3b73-42ab-8664-56a17a85a238")), "InnerSwampSwimLeftKeystone", WorldArea.Swamp, LocationType.Keystone, new Vector2(684.0f, -205.0f)),
            new Location(new MoonGuid(new Guid("0cc53366-fc0f-4163-9021-ece2ae0751bc")), "InnerSwampSwimRightKeystone", WorldArea.Swamp, LocationType.Keystone, new Vector2(766.0f, -183.0f)),
            new Location(new MoonGuid(new Guid("a976fb6d-0db9-4ff3-becb-fd8cb98e7e98")), "InnerSwampSwimMapstone", WorldArea.Swamp, LocationType.Mapstone, new Vector2(796.0f, -210.0f)),
            new Location(new MoonGuid(new Guid("ebe89d18-678d-4654-84a2-ca94fe1831a2")), "InnerSwampStompExp", WorldArea.Swamp, LocationType.ExpMedium, new Vector2(770.9f, -148.0f)),
            new Location(new MoonGuid(new Guid("2da844e9-4e55-4ec0-a71a-b0c6abd6e1b0")), "InnerSwampEnergyCell", WorldArea.Swamp, LocationType.EnergyCell, new Vector2(722.3f, -95.5f)),
            new Location(new MoonGuid(new Guid("d02c2919-1426-44e9-a72b-f48a6c7c68e1")), "StompSkillTree", WorldArea.Swamp, LocationType.Skill, new Vector2(860.0f, -96.0f)),
            new Location(new MoonGuid(new Guid("08edaf56-f649-4e8c-8ee6-0fab727507d7")), "StompAreaRoofExp", WorldArea.Swamp, LocationType.ExpLarge, new Vector2(914.9f, -71.3f)),
            new Location(new MoonGuid(new Guid("9f0907c3-fc53-4943-910e-79cef197c9bf")), "StompAreaExp", WorldArea.Swamp, LocationType.ExpMedium, new Vector2(884.0f, -98.3f)),
            new Location(new MoonGuid(new Guid("195e0dd8-4871-40db-9c6a-a8912684e944")), "StompAreaGrenadeExp", WorldArea.Swamp, LocationType.ExpLarge, new Vector2(874.2f, -143.6f)),
            new Location(new MoonGuid(new Guid("dbd78938-437d-4bad-bac3-6f9f3471fd62")), "HoruFieldsHiddenExp", WorldArea.Grove, LocationType.ExpLarge, new Vector2(97.5f, -37.9f)),
            new Location(new MoonGuid(new Guid("82db7562-639e-4a64-ab5e-f042096c5d9b")), "HoruFieldsEnergyCell", WorldArea.Grove, LocationType.EnergyCell, new Vector2(175.9f, 1.0f)),
            new Location(new MoonGuid(new Guid("1b390cdd-e654-4a83-8aa6-d5235333a1c4")), "HoruFieldsPlant", WorldArea.Grove, LocationType.Plant, new Vector2(124.0f, 21.0f)),
            new Location(new MoonGuid(new Guid("83819798-2573-4af5-8bf9-70c5d9d0c68d")), "HoruFieldsAbilityCell", WorldArea.Grove, LocationType.AbilityCell, new Vector2(176.8f, -34.6f)),
            new Location(new MoonGuid(new Guid("79cf7fce-01e9-44a3-8568-f2b243baeb7d")), "HoruFieldsHealthCell", WorldArea.Grove, LocationType.HealthCell, new Vector2(160.3f, -78.4f)),
            new Location(new MoonGuid(new Guid("91da766a-938d-49d4-bae1-41c015618da6")), "HoruMap", WorldArea.Horu, LocationType.Map, new Vector2(56.0f, 343.0f)),
            new Location(new MoonGuid(new Guid("88d96765-325c-4dfc-9167-25020be791d9")), "HoruL4LowerExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(-29.8f, 148.9f)),
            new Location(new MoonGuid(new Guid("22e628e7-af91-4630-8e1c-b9d6ce0a7fcd")), "HoruL4ChaseExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(-191.7f, 194.4f)),
            new Location(new MoonGuid(new Guid("7523b755-e9e4-407e-83ef-1c1f7210d310")), "HoruLavaDrainedLeftExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(13.5f, 164.3f)),
            new Location(new MoonGuid(new Guid("a2512573-18dc-46c4-81da-5e5a2ec38f10")), "HoruR1HangingExp", WorldArea.Horu, LocationType.ExpMedium, new Vector2(193.8f, 384.9f)),
            new Location(new MoonGuid(new Guid("d368611b-b65c-432d-b62b-2c15b33eccfd")), "HoruR1Mapstone", WorldArea.Horu, LocationType.Mapstone, new Vector2(148.0f, 363.0f)),
            new Location(new MoonGuid(new Guid("61414408-5868-442c-9abf-728f9fb0f4a7")), "HoruR1EnergyCell", WorldArea.Horu, LocationType.EnergyCell, new Vector2(249.4f, 403.0f)),
            new Location(new MoonGuid(new Guid("9c56d300-6891-4876-9756-2659b8887ad3")), "HoruR3Plant", WorldArea.Horu, LocationType.Plant, new Vector2(318.0f, 245.0f)),
            new Location(new MoonGuid(new Guid("5b154bb4-59ea-4135-96a8-d7cfb51870eb")), "HoruR4StompExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(191.7f, 165.2f)),
            new Location(new MoonGuid(new Guid("b9240859-43a5-428b-a81d-ba79a65cf253")), "HoruR4LaserExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(253.5f, 194.6f)),
            new Location(new MoonGuid(new Guid("01ea51a6-9586-4d16-ac13-847fec4637e0")), "HoruR4DrainedExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(163.4f, 136.4f)),
            new Location(new MoonGuid(new Guid("753b9eef-37af-45ae-8628-194312e1c752")), "HoruLavaDrainedRightExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(129.8f, 165.6f)),
            new Location(new MoonGuid(new Guid("1e459051-fb73-4ce9-82e3-43777566bf19")), "HoruL1", WorldArea.Horu, LocationType.Cutscene, new Vector2(-92.0f, 376.0f)),
            new Location(new MoonGuid(new Guid("1a7fd156-3c71-4618-99ee-454c9aab2f27")), "HoruL2", WorldArea.Horu, LocationType.Cutscene, new Vector2(-20.0f, 276.0f)),
            new Location(new MoonGuid(new Guid("14684bd4-ce7e-4c3d-ba2d-4673104d8e86")), "HoruL3", WorldArea.Horu, LocationType.Cutscene, new Vector2(-164.0f, 336.0f)),
            new Location(new MoonGuid(new Guid("f93ca13c-c7bc-4b3f-bcc0-3c7b30b2c4a9")), "HoruL4", WorldArea.Horu, LocationType.Cutscene, new Vector2(-96.0f, 152.0f)),
            new Location(new MoonGuid(new Guid("726bbbb9-339f-4b6f-baf0-05a02c215385")), "HoruR1", WorldArea.Horu, LocationType.Cutscene, new Vector2(264.0f, 380.0f)),
            new Location(new MoonGuid(new Guid("46555a18-aec8-4943-a12b-fa5782d65b42")), "HoruR2", WorldArea.Horu, LocationType.Cutscene, new Vector2(172.0f, 288.0f)),
            new Location(new MoonGuid(new Guid("7cdc17a4-5448-44c2-890a-ce1ba09f6e66")), "HoruR3", WorldArea.Horu, LocationType.Cutscene, new Vector2(304.0f, 304.0f)),
            new Location(new MoonGuid(new Guid("4dc498b4-9f8d-4779-ab12-4cce964d211d")), "HoruR4", WorldArea.Horu, LocationType.Cutscene, new Vector2(216.0f, 192.0f)),
            new Location(new MoonGuid(new Guid("09b053bd-87bb-4718-9419-e5689f618233")), "DoorWarpExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(106.5f, 112.0f)),
            new Location(new MoonGuid(new Guid("ca00fedb-6d8d-4091-95aa-4e49d9583a1c")), "HoruTeleporterExp", WorldArea.Horu, LocationType.ExpLarge, new Vector2(98.0f, 130.5f)),
            new Location(new MoonGuid(new Guid("956197d9-4af7-452c-931a-2c45f33435d2")), "ValleyEntryAbilityCell", WorldArea.Valley, LocationType.AbilityCell, new Vector2(-206.0f, -113.3f)),
            new Location(new MoonGuid(new Guid("f39ac231-0153-4a0a-b897-ebcb6dafb626")), "ValleyEntryTreeExp", WorldArea.Valley, LocationType.ExpMedium, new Vector2(-221.0f, -84.0f)),
            new Location(new MoonGuid(new Guid("3dac0d06-0ea7-466c-a959-9583f15f1d56")), "ValleyEntryTreePlant", WorldArea.Valley, LocationType.Plant, new Vector2(-179.0f, -88.0f)),
            new Location(new MoonGuid(new Guid("cd85689a-a50c-4923-8621-135a443a76f9")), "ValleyEntryGrenadeLongSwim", WorldArea.Valley, LocationType.EnergyCell, new Vector2(-320.0f, -162.0f)),
            new Location(new MoonGuid(new Guid("dcb47a2b-3a57-45ef-80c7-a58a2861972b")), "ValleyRightFastStomplessCell", WorldArea.Valley, LocationType.AbilityCell, new Vector2(-355.4f, 65.6f)),
            new Location(new MoonGuid(new Guid("4ca2a5dd-bfeb-4c25-95a2-61e6b0c37d07")), "ValleyRightExp", WorldArea.Valley, LocationType.ExpMedium, new Vector2(-418.8f, 67.5f)),
            new Location(new MoonGuid(new Guid("3c8775ce-69a3-402e-869f-e78529dab898")), "ValleyRightBirdStompCell", WorldArea.Valley, LocationType.AbilityCell, new Vector2(-292.0f, 20.7f)),
            new Location(new MoonGuid(new Guid("77ab5ca3-78e5-4c95-80bf-c829fe0db9cb")), "GlideSkillFeather", WorldArea.Valley, LocationType.Skill, new Vector2(-460.0f, -20.0f)),
            new Location(new MoonGuid(new Guid("a7ec42f9-1245-447f-b14c-2af1a282c12b")), "KuroPerchExp", WorldArea.Sorrow, LocationType.ExpLarge, new Vector2(-546.2f, 54.4f)),
            new Location(new MoonGuid(new Guid("c7529078-8db5-40a8-8564-c7d568dc9fef")), "ValleyMap", WorldArea.Valley, LocationType.Map, new Vector2(-408.0f, -170.0f)),
            new Location(new MoonGuid(new Guid("54116d93-15e8-43ce-b34a-4b01e795be11")), "ValleyMainPlant", WorldArea.Valley, LocationType.Plant, new Vector2(-468.0f, -67.0f)),
            new Location(new MoonGuid(new Guid("e8700000-f30c-4294-9a2d-51410549c601")), "WilhelmExp", WorldArea.Sorrow, LocationType.ExpLarge, new Vector2(-572.0f, 157.0f)),
            new Location(new MoonGuid(new Guid("a550b330-e931-42cc-b00c-50ca36143ee7")), "ValleyRightSwimExp", WorldArea.Valley, LocationType.ExpMedium, new Vector2(-359.0f, -87.5f)),
            new Location(new MoonGuid(new Guid("c3044298-0f52-44cc-a4e7-396db2822ef0")), "ValleyMainFACS", WorldArea.Valley, LocationType.AbilityCell, new Vector2(-415.5f, -80.0f)),
            new Location(new MoonGuid(new Guid("467cdd0a-d736-4d51-af19-eee884ee066f")), "ValleyForlornApproachGrenade", WorldArea.Valley, LocationType.AbilityCell, new Vector2(-460.0f, -187.0f)),
            new Location(new MoonGuid(new Guid("a306b326-2662-4035-9c97-2c4459f84a71")), "ValleyThreeBirdAbilityCell", WorldArea.Valley, LocationType.AbilityCell, new Vector2(-350.9f, -98.7f)),
            new Location(new MoonGuid(new Guid("ed79b633-818d-4a00-844e-998fd2745b41")), "LowerValleyMapstone", WorldArea.Valley, LocationType.Mapstone, new Vector2(-561.0f, -89.0f)),
            new Location(new MoonGuid(new Guid("09b3ac61-4972-4448-996b-93bc56222643")), "LowerValleyExp", WorldArea.Valley, LocationType.ExpMedium, new Vector2(-538.0f, -104.0f)),
            new Location(new MoonGuid(new Guid("c068bb67-aed3-470c-90d2-c37fadd4f294")), "OutsideForlornTreeExp", WorldArea.Valley, LocationType.ExpMedium, new Vector2(-460.0f, -255.0f)),
            new Location(new MoonGuid(new Guid("20d96007-fe1e-4469-b5fb-5c5f1f72e68f")), "OutsideForlornWaterExp", WorldArea.Valley, LocationType.ExpMedium, new Vector2(-514.5f, -277.3f)),
            new Location(new MoonGuid(new Guid("ce0c05d0-dc62-4a92-adf9-85440516b089")), "OutsideForlornCliffExp", WorldArea.Valley, LocationType.ExpLarge, new Vector2(-538.7f, -234.7f)),
            new Location(new MoonGuid(new Guid("3c59543f-9c36-43cc-9aab-69d859cae9ea")), "ValleyForlornApproachMapstone", WorldArea.Valley, LocationType.Mapstone, new Vector2(-443.0f, -152.0f)),
            new Location(new MoonGuid(new Guid("a067bd11-efc4-4b4f-8d82-9b51ddf209ce")), "ForlornEntranceExp", WorldArea.Forlorn, LocationType.ExpLarge, new Vector2(-703.9f, -390.0f)),
            new Location(new MoonGuid(new Guid("aec7c676-e2fe-4b4b-8af4-5c9674aac8b9")), "ForlornHiddenSpiderExp", WorldArea.Forlorn, LocationType.ExpMedium, new Vector2(-841.3f, -350.9f)),
            new Location(new MoonGuid(new Guid("117b3edb-dc08-4488-b9a0-391ab9d57960")), "ForlornKeystone1", WorldArea.Forlorn, LocationType.Keystone, new Vector2(-858.0f, -353.0f)),
            new Location(new MoonGuid(new Guid("77bcf456-5d29-4010-af33-bc913b951a93")), "ForlornKeystone2", WorldArea.Forlorn, LocationType.Keystone, new Vector2(-892.0f, -328.0f)),
            new Location(new MoonGuid(new Guid("10eb191f-dac8-45bb-9c86-dc3ed1c77cc0")), "ForlornKeystone3", WorldArea.Forlorn, LocationType.Keystone, new Vector2(-888.0f, -251.0f)),
            new Location(new MoonGuid(new Guid("5e483d15-dfef-408d-9545-d70633769e81")), "ForlornKeystone4", WorldArea.Forlorn, LocationType.Keystone, new Vector2(-869.0f, -255.0f)),
            new Location(new MoonGuid(new Guid("3c0113b8-d5f3-4428-abfa-b00321a780bb")), "ForlornMap", WorldArea.Forlorn, LocationType.Map, new Vector2(-843.0f, -308.0f)),
            new Location(new MoonGuid(new Guid("61b87fae-1434-409e-98c4-6c79598a9e73")), "ForlornPlant", WorldArea.Forlorn, LocationType.Plant, new Vector2(-815.0f, -266.0f)),
            new Location(new MoonGuid(new Guid("da60ec6f-c4ba-493b-b4bd-4f7d8218a73a")), "RightForlornHealthCell", WorldArea.Forlorn, LocationType.HealthCell, new Vector2(-625.5f, -315.1f)),
            new Location(new MoonGuid(new Guid("13f2bf4e-fa2c-46c8-bd24-8e3e4bb27e55")), "ForlornEscape", WorldArea.Forlorn, LocationType.Event, new Vector2(-732.0f, -236.0f)),
            new Location(new MoonGuid(new Guid("92a35133-5de9-4bc5-83c6-9b359d9a0fbf")), "RightForlornPlant", WorldArea.Forlorn, LocationType.Plant, new Vector2(-607.0f, -314.0f)),
            new Location(new MoonGuid(new Guid("e601dd92-c65c-4cf1-b442-accf961e6ab7")), "SorrowEntranceAbilityCell", WorldArea.Sorrow, LocationType.AbilityCell, new Vector2(-510.3f, 204.3f)),
            new Location(new MoonGuid(new Guid("910dce6f-b896-42d9-b764-e5ef2ba9e471")), "SorrowMainShaftKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-485.0f, 323.0f)),
            new Location(new MoonGuid(new Guid("10919728-d585-443e-8e2d-86af53161f7c")), "SorrowSpikeKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-503.0f, 274.0f)),
            new Location(new MoonGuid(new Guid("6c13aced-1bd4-447e-a57d-c373a50142fb")), "SorrowHiddenKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-514.0f, 303.0f)),
            new Location(new MoonGuid(new Guid("ff0edbb1-882e-4221-a5e0-bfe3136611e3")), "SorrowLowerLeftKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-596.0f, 229.0f)),
            new Location(new MoonGuid(new Guid("937c8eed-a6fa-4bb9-92c3-46ba0d50adf5")), "SorrowMap", WorldArea.Sorrow, LocationType.Map, new Vector2(-451.0f, 284.0f)),
            new Location(new MoonGuid(new Guid("917c444e-b83e-4bc9-b7e6-93e2d8ee695e")), "SorrowMapstone", WorldArea.Sorrow, LocationType.Mapstone, new Vector2(-435.0f, 322.0f)),
            new Location(new MoonGuid(new Guid("12525396-13fd-4658-a7c3-8df57bac078e")), "SorrowHealthCell", WorldArea.Sorrow, LocationType.HealthCell, new Vector2(-609.0f, 299.0f)),
            new Location(new MoonGuid(new Guid("eae7b2e1-351d-4b55-8b92-9fa04b307d36")), "LeftSorrowAbilityCell", WorldArea.Sorrow, LocationType.AbilityCell, new Vector2(-671.1f, 290.0f)),
            new Location(new MoonGuid(new Guid("69616e29-9a5d-4382-89fb-6d17a8748780")), "LeftSorrowKeystone1", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-608.0f, 329.0f)),
            new Location(new MoonGuid(new Guid("2c561421-dc69-4148-a65e-4a264c5305c7")), "LeftSorrowKeystone2", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-612.0f, 347.0f)),
            new Location(new MoonGuid(new Guid("0eadcdde-8f34-488a-b6ef-b12581a2b27a")), "LeftSorrowKeystone3", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-604.0f, 361.0f)),
            new Location(new MoonGuid(new Guid("a296680b-4a68-4a3e-b664-0a95f6c9893c")), "LeftSorrowKeystone4", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-613.1f, 371.7f)),
            new Location(new MoonGuid(new Guid("e9ca9858-3abd-43b2-8f5e-233529859842")), "LeftSorrowEnergyCell", WorldArea.Sorrow, LocationType.EnergyCell, new Vector2(-627.1f, 394.0f)),
            new Location(new MoonGuid(new Guid("8b7d005c-614e-42b3-9d38-9ee16313f668")), "LeftSorrowPlant", WorldArea.Sorrow, LocationType.Plant, new Vector2(-630.0f, 249.0f)),
            new Location(new MoonGuid(new Guid("b31bf78c-695a-4594-aad6-822f9b42f767")), "LeftSorrowGrenade", WorldArea.Sorrow, LocationType.ExpLarge, new Vector2(-677.0f, 269.9f)),
            new Location(new MoonGuid(new Guid("e8e62373-8d78-474c-bc2e-76244cfd5220")), "UpperSorrowRightKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-456.0f, 419.0f)),
            new Location(new MoonGuid(new Guid("de067109-811d-4138-9856-4836f217ece3")), "UpperSorrowFarRightKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-414.0f, 429.0f)),
            new Location(new MoonGuid(new Guid("46f35914-7b0b-4322-8c1d-3ffa1d9c8ab7")), "UpperSorrowLeftKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-514.0f, 427.0f)),
            new Location(new MoonGuid(new Guid("d5b98071-5721-4e15-b2a4-62fde346be0d")), "UpperSorrowSpikeExp", WorldArea.Sorrow, LocationType.ExpMedium, new Vector2(-545.0f, 409.5f)),
            new Location(new MoonGuid(new Guid("8395174a-d06d-461b-ba37-b9dd97ee322b")), "UpperSorrowFarLeftKeystone", WorldArea.Sorrow, LocationType.Keystone, new Vector2(-592.0f, 445.0f)),
            new Location(new MoonGuid(new Guid("24783b59-4196-4fcb-bd42-d1a925962e86")), "ChargeJumpSkillTree", WorldArea.Sorrow, LocationType.Skill, new Vector2(-696.0f, 408.0f)),
            new Location(new MoonGuid(new Guid("ce467c66-3616-4596-b052-3aa36b9daeab")), "AboveChargeJumpAbilityCell", WorldArea.Sorrow, LocationType.AbilityCell, new Vector2(-646.9f, 473.1f)),
            new Location(new MoonGuid(new Guid("50ffb37d-475f-48f3-911a-bb1c5ba6dd79")), "Sunstone", WorldArea.Sorrow, LocationType.Event, new Vector2(-560.0f, 600.0f)),
            new Location(new MoonGuid(new Guid("f8927d97-73bb-405d-bc66-a97d29b7de94")), "SunstonePlant", WorldArea.Sorrow, LocationType.Plant, new Vector2(-478.0f, 586.0f)),
            new Location(new MoonGuid(new Guid("2f9b8590-168f-4a66-9543-4fce8b1a1035")), "MistyEntranceStompExp", WorldArea.Misty, LocationType.ExpMedium, new Vector2(-678.1f, -30.0f)),
            new Location(new MoonGuid(new Guid("b91ec03d-b110-4d53-960f-724dc406a7f7")), "MistyEntranceTreeExp", WorldArea.Misty, LocationType.ExpMedium, new Vector2(-822.3f, -9.7f)),
            new Location(new MoonGuid(new Guid("47513f30-c883-46f0-9193-8f30c1d3b060")), "MistyFrogNookExp", WorldArea.Misty, LocationType.ExpMedium, new Vector2(-979.3f, 23.6f)),
            new Location(new MoonGuid(new Guid("d7491440-a5d3-40f0-a937-7b2e4cc1a95e")), "MistyKeystone1", WorldArea.Misty, LocationType.Keystone, new Vector2(-1076.0f, 32.0f)),
            new Location(new MoonGuid(new Guid("40675dbd-5f3c-4e1c-86ee-c7241c5fdd52")), "MistyMortarCorridorUpperExp", WorldArea.Misty, LocationType.ExpMedium, new Vector2(-1083.0f, 8.3f)),
            new Location(new MoonGuid(new Guid("958dfa0f-6ab3-422e-b1e1-98a71eef37ba")), "MistyMortarCorridorHiddenExp", WorldArea.Misty, LocationType.ExpMedium, new Vector2(-1009.0f, -35.0f)),
            new Location(new MoonGuid(new Guid("5e563952-174f-42be-ab22-17a901dd29e0")), "MistyPlant", WorldArea.Misty, LocationType.Plant, new Vector2(-1102.0f, -67.0f)),
            new Location(new MoonGuid(new Guid("a273c819-a2c7-44c9-8798-a6510fc25e0d")), "ClimbSkillTree", WorldArea.Misty, LocationType.Skill, new Vector2(-1188.0f, -100.0f)),
            new Location(new MoonGuid(new Guid("920137e5-c082-428b-93a1-bcb8f67a0748")), "MistyKeystone3", WorldArea.Misty, LocationType.Keystone, new Vector2(-912.0f, -36.0f)),
            new Location(new MoonGuid(new Guid("43c5043b-4b30-4cc3-84e3-baedb5e5a378")), "MistyPostClimbSpikeCave", WorldArea.Misty, LocationType.ExpMedium, new Vector2(-837.7f, -123.5f)),
            new Location(new MoonGuid(new Guid("8eae37fc-ca7a-4f9b-904b-47384ed8ec39")), "MistyPostClimbAboveSpikePit", WorldArea.Misty, LocationType.ExpLarge, new Vector2(-796.0f, -144.0f)),
            new Location(new MoonGuid(new Guid("654eae78-1b9e-4554-85a3-10523617bcf5")), "MistyKeystone4", WorldArea.Misty, LocationType.Keystone, new Vector2(-768.0f, -144.0f)),
            new Location(new MoonGuid(new Guid("817a54c5-92b6-47da-8891-59b51020b2c3")), "MistyGrenade", WorldArea.Misty, LocationType.ExpLarge, new Vector2(-671.9f, -39.4f)),
            new Location(new MoonGuid(new Guid("62b3451f-85cd-423e-941c-8fbe4589ee55")), "MistyKeystone2", WorldArea.Misty, LocationType.Keystone, new Vector2(-1043.0f, -8.0f)),
            new Location(new MoonGuid(new Guid("08b6537c-6090-4ff2-96ea-ee4123feb799")), "MistyAbilityCell", WorldArea.Misty, LocationType.AbilityCell, new Vector2(-1075.7f, -2.2f)),
            new Location(new MoonGuid(new Guid("40872a5d-1d65-46c9-bd05-39b991ebce58")), "GumonSeal", WorldArea.Misty, LocationType.Event, new Vector2(-720.0f, -24.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000001")), "ProgressiveMap1", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 24.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000002")), "ProgressiveMap2", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 28.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000003")), "ProgressiveMap3", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 32.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000004")), "ProgressiveMap4", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 36.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000005")), "ProgressiveMap5", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 40.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000006")), "ProgressiveMap6", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 44.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000007")), "ProgressiveMap7", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 48.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000008")), "ProgressiveMap8", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 52.0f)),
            new Location(new MoonGuid(new Guid("00000000-0000-0000-0000-100000000009")), "ProgressiveMap9", WorldArea.Void, LocationType.ProgressiveMap, new Vector2(0.0f, 56.0f))
        };
    }
}
