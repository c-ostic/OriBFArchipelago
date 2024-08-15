using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    internal class LocationLookup
    {
        internal struct Position
        {
            public double x;
            public double y;

            public Position()
            {
                x = 0; y = 0;
            }

            public Position(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public double DistanceSquared(Position other)
            {
                double diffX = other.x - x, diffY = other.y - y;
                return diffX * diffX + diffY * diffY;
            }

            public override bool Equals(object obj)
            {
                return obj is Position position &&
                       x == position.x &&
                       y == position.y;
            }

            public override int GetHashCode()
            {
                int hashCode = 1502939027;
                hashCode = hashCode * -1521134295 + x.GetHashCode();
                hashCode = hashCode * -1521134295 + y.GetHashCode();
                return hashCode;
            }

            public override string ToString()
            {
                return "(" + x + ", " + y + ")";
            }
        }

        private const double THRESHOLD = 400;

        public static string GetLocationName(GameObject g)
        {
            Position p = new Position();
            p.x = Mathf.Round(g.transform.position.x * 10) / 10.0;
            p.y = Mathf.Round(g.transform.position.y * 10) / 10.0;

            if (locations.ContainsKey(p))
            {
                return locations[p];
            }
            else
            {
                // unfortunately, the location may not be exact, so go through the list and find the closest location
                Position closest = new Position();
                double minDistance = 999;
                foreach (var location in locations)
                {
                    double distance = p.DistanceSquared(location.Key);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = location.Key;
                    }
                }

                if (locations.ContainsKey(closest) && minDistance < THRESHOLD)
                {
                    return locations[closest];
                }
                else
                {
                    Console.WriteLine("No location at " + p.ToString());
                    return null;
                }
            }
        }

        private static readonly Dictionary<Position, string> locations = new Dictionary<Position, string>
        {
            { new Position(92.0, -227.5), "FirstPickup" },
            { new Position(-27.4, -256.3), "FirstEnergyCell" },
            { new Position(-154.5, -271.8), "FronkeyFight" },
            { new Position(83.1, -222.3), "GladesKeystone1" },
            { new Position(-11.1, -206.4), "GladesKeystone2" },
            { new Position(59.0, -280.9), "GladesGrenadePool" },
            { new Position(83.0, -196.8), "GladesGrenadeTree" },
            { new Position(5.7, -241.8), "GladesMainPool" },
            { new Position(-40.1, -239.8), "GladesMainPoolDeep" },
            { new Position(257.1, -199.7), "FronkeyWalkRoof" },
            { new Position(-80.5, -189.0), "FourthHealthCell" },
            { new Position(-59.4, -244.3), "GladesMapKeystone" },
            { new Position(-316.0, -308.0), "WallJumpSkillTree" },
            { new Position(-283.4, -236.4), "LeftGladesHiddenExp" },
            { new Position(303.4, -190.5), "DeathGauntletExp" },
            { new Position(423.8, -169.2), "DeathGauntletEnergyCell" },
            { new Position(-81.0, -248.0), "GladesMap" },
            { new Position(-48.3, -166.0), "AboveFourthHealth" },
            { new Position(-245.7, -277.0), "WallJumpAreaExp" },
            { new Position(-336.8, -288.2), "WallJumpAreaEnergyCell" },
            { new Position(-247.1, -207.1), "LeftGladesExp" },
            { new Position(-238.3, -212.4), "LeftGladesKeystone" },
            { new Position(-184.9, -227.6), "LeftGladesMapstone" },
            { new Position(-182.1, -194.0), "SpiritCavernsKeystone1" },
            { new Position(-217.4, -183.9), "SpiritCavernsKeystone2" },
            { new Position(-177.9, -154.5), "SpiritCavernsTopRightKeystone" },
            { new Position(-217.5, -146.3), "SpiritCavernsTopLeftKeystone" },
            { new Position(-216.4, -176.4), "SpiritCavernsAbilityCell" },
            { new Position(-155.0, -186.0), "GladesLaser" },
            { new Position(-165.4, -140.4), "GladesLaserGrenade" },
            { new Position(-56.0, -160.0), "ChargeFlameSkillTree" },
            { new Position(43.0, -156.0), "ChargeFlameAreaPlant" },
            { new Position(5.0, -193.2), "ChargeFlameAreaExp" },
            { new Position(-14.2, -95.7), "AboveChargeFlameTreeExp" },
            { new Position(64.0, -109.4), "SpiderSacEnergyDoor" },
            { new Position(151.8, -117.8), "SpiderSacHealthCell" },
            { new Position(60.7, -155.8), "SpiderSacEnergyCell" },
            { new Position(93.4, -92.5), "SpiderSacGrenadeDoor" },
            { new Position(154.0, -291.5), "DashAreaOrbRoomExp" },
            { new Position(183.8, -291.5), "DashAreaAbilityCell" },
            { new Position(197.3, -229.1), "DashAreaRoofExp" },
            { new Position(292.0, -256.0), "DashSkillTree" },
            { new Position(313.0, -232.0), "DashAreaPlant" },
            { new Position(304.4, -303.2), "RazielNo" },
            { new Position(346.0, -255.0), "DashAreaMapstone" },
            { new Position(395.0, -309.1), "BlackrootTeleporterHealthCell" },
            { new Position(418.0, -291.0), "BlackrootMap" },
            { new Position(432.0, -324.0), "BlackrootBoulderExp" },
            { new Position(72.0, -380.0), "GrenadeSkillTree" },
            { new Position(224.0, -359.1), "GrenadeAreaExp" },
            { new Position(252.4, -331.9), "GrenadeAreaAbilityCell" },
            { new Position(279.0, -375.0), "LowerBlackrootAbilityCell" },
            { new Position(391.5, -423.0), "LowerBlackrootLaserAbilityCell" },
            { new Position(339.0, -418.8), "LowerBlackrootLaserExp" },
            { new Position(208.5, -431.5), "LowerBlackrootGrenadeThrow" },
            { new Position(459.5, -506.8), "LostGroveAbilityCell" },
            { new Position(462.4, -489.5), "LostGroveHiddenExp" },
            { new Position(307.3, -525.2), "LostGroveTeleporter" },
            { new Position(527.0, -544.0), "LostGroveLongSwim" },
            { new Position(300.0, -94.0), "HollowGroveMapstone" },
            { new Position(703.2, -82.3), "OuterSwampAbilityCell" },
            { new Position(618.5, -98.0), "OuterSwampStompExp" },
            { new Position(581.5, -67.0), "OuterSwampHealthCell" },
            { new Position(351.0, -119.0), "HollowGroveMap" },
            { new Position(333.1, -61.9), "HollowGroveTreeAbilityCell" },
            { new Position(365.0, -119.0), "HollowGroveMapPlant" },
            { new Position(330.0, -78.0), "HollowGroveTreePlant" },
            { new Position(628.0, -120.0), "SwampEntrancePlant" },
            { new Position(435.0, -140.0), "MoonGrottoStompPlant" },
            { new Position(515.0, -100.0), "OuterSwampMortarPlant" },
            { new Position(354.0, -178.5), "GroveWaterStompAbilityCell" },
            { new Position(666.0, -48.0), "OuterSwampGrenadeExp" },
            { new Position(409.5, -34.5), "SwampTeleporterAbilityCell" },
            { new Position(174.2, -105.6), "GroveAboveSpiderWaterExp" },
            { new Position(261.4, -117.7), "GroveAboveSpiderWaterHealthCell" },
            { new Position(272.5, -97.5), "GroveAboveSpiderWaterEnergyCell" },
            { new Position(187.3, -163.9), "GroveSpiderWaterSwim" },
            { new Position(339.0, -216.0), "DeathGauntletSwimEnergyDoor" },
            { new Position(356.8, -207.1), "DeathGauntletStompSwim" },
            { new Position(477.0, -140.0), "AboveGrottoTeleporterExp" },
            { new Position(432.0, -108.0), "GrottoLasersRoofExp" },
            { new Position(365.2, -109.1), "IcelessExp" },
            { new Position(540.0, -220.0), "BelowGrottoTeleporterPlant" },
            { new Position(450.0, -166.8), "LeftGrottoTeleporterExp" },
            { new Position(502.0, -108.0), "OuterSwampMortarAbilityCell" },
            { new Position(595.0, -136.0), "SwampEntranceSwim" },
            { new Position(543.6, -189.4), "BelowGrottoTeleporterHealthCell" },
            { new Position(423.0, -274.0), "GrottoEnergyDoorSwim" },
            { new Position(424.0, -220.0), "GrottoEnergyDoorHealthCell" },
            { new Position(552.0, -141.5), "GrottoSwampDrainAccessExp" },
            { new Position(537.0, -176.0), "GrottoSwampDrainAccessPlant" },
            { new Position(451.0, -296.0), "GrottoHideoutFallAbilityCell" },
            { new Position(513.0, -413.0), "GumoHideoutMapstone" },
            { new Position(620.0, -404.0), "GumoHideoutMiniboss" },
            { new Position(572.5, -378.5), "GumoHideoutCrusherExp" },
            { new Position(590.0, -384.0), "GumoHideoutCrusherKeystone" },
            { new Position(496.0, -369.0), "GumoHideoutRightHangingExp" },
            { new Position(467.5, -369.0), "GumoHideoutLeftHangingExp" },
            { new Position(449.0, -430.0), "GumoHideoutRedirectAbilityCell" },
            { new Position(477.0, -389.0), "GumoHideoutMap" }, //No location at (477, -388.2)
            { new Position(784.0, -412.0), "DoubleJumpSkillTree" },
            { new Position(759.0, -398.0), "DoubleJumpAreaExp" },
            { new Position(545.8, -357.9), "GumoHideoutEnergyCell" },
            { new Position(567.6, -246.2), "GumoHideoutRockfallExp" },
            { new Position(500.0, -248.0), "WaterVein" },
            { new Position(406.9, -386.2), "LeftGumoHideoutExp" },
            { new Position(393.8, -375.6), "LeftGumoHideoutHealthCell" },
            { new Position(447.0, -368.0), "LeftGumoHideoutLowerPlant" },
            { new Position(439.0, -344.0), "LeftGumoHideoutUpperPlant" },
            { new Position(492.0, -400.0), "GumoHideoutRedirectPlant" },
            { new Position(397.0, -411.5), "LeftGumoHideoutSwim" },
            { new Position(515.4, -441.9), "GumoHideoutRedirectEnergyCell" },
            { new Position(505.3, -439.4), "GumoHideoutRedirectExp" },
            { new Position(328.9, -353.7), "FarLeftGumoHideoutExp" },
            { new Position(643.5, -127.5), "SwampEntranceAbilityCell" },
            { new Position(321.7, -179.1), "DeathGauntletRoofHealthCell" },
            { new Position(342.0, -179.0), "DeathGauntletRoofPlant" },
            { new Position(523.0, 142.0), "LowerGinsoHiddenExp" },
            { new Position(531.0, 267.0), "LowerGinsoKeystone1" },
            { new Position(540.0, 277.0), "LowerGinsoKeystone2" },
            { new Position(508.0, 304.0), "LowerGinsoKeystone3" },
            { new Position(529.0, 297.0), "LowerGinsoKeystone4" },
            { new Position(540.0, 101.0), "LowerGinsoPlant" },
            { new Position(532.0, 328.0), "BashSkillTree" },
            { new Position(518.4, 339.8), "BashAreaExp" },
            { new Position(507.0, 476.0), "UpperGinsoLowerKeystone" },
            { new Position(535.0, 488.0), "UpperGinsoRightKeystone" },
            { new Position(531.0, 502.0), "UpperGinsoUpperRightKeystone" },
            { new Position(508.0, 498.0), "UpperGinsoUpperLeftKeystone" },
            { new Position(517.1, 384.4), "UpperGinsoRedirectLowerExp" },
            { new Position(530.5, 407.0), "UpperGinsoRedirectUpperExp" },
            { new Position(536.6, 434.7), "UpperGinsoEnergyCell" },
            { new Position(456.9, 566.1), "TopGinsoLeftLowerExp" },
            { new Position(471.2, 614.8), "TopGinsoLeftUpperExp" },
            { new Position(610.0, 611.0), "TopGinsoRightPlant" },
            { new Position(534.5, 661.5), "GinsoEscapeSpiderExp" },
            { new Position(537.5, 733.6), "GinsoEscapeJumpPadExp" },
            { new Position(533.5, 827.3), "GinsoEscapeProjectileExp" },
            { new Position(519.3, 867.6), "GinsoEscapeHangingExp" },
            { new Position(548.0, 952.0), "GinsoEscapeExit" },
            { new Position(677.0, -129.0), "SwampMap" },
            { new Position(637.0, -162.2), "InnerSwampDrainExp" },
            { new Position(761.9, -173.5), "InnerSwampHiddenSwimExp" },
            { new Position(684.0, -205.0), "InnerSwampSwimLeftKeystone" },
            { new Position(766.0, -183.0), "InnerSwampSwimRightKeystone" },
            { new Position(796.0, -210.0), "InnerSwampSwimMapstone" },
            { new Position(770.9, -148.0), "InnerSwampStompExp" },
            { new Position(722.3, -95.5), "InnerSwampEnergyCell" },
            { new Position(860.0, -96.0), "StompSkillTree" },
            { new Position(914.9, -71.3), "StompAreaRoofExp" },
            { new Position(884.0, -98.3), "StompAreaExp" },
            { new Position(874.2, -143.6), "StompAreaGrenadeExp" },
            { new Position(97.5, -37.9), "HoruFieldsHiddenExp" },
            { new Position(175.9, 1.0), "HoruFieldsEnergyCell" },
            { new Position(124.0, 21.0), "HoruFieldsPlant" },
            { new Position(176.8, -34.6), "HoruFieldsAbilityCell" },
            { new Position(160.3, -78.4), "HoruFieldsHealthCell" },
            { new Position(56.0, 343.0), "HoruMap" },
            { new Position(-29.8, 148.9), "HoruL4LowerExp" },
            { new Position(-191.7, 194.4), "HoruL4ChaseExp" },
            { new Position(13.5, 164.3), "HoruLavaDrainedLeftExp" },
            { new Position(193.8, 384.9), "HoruR1HangingExp" },
            { new Position(148.0, 363.0), "HoruR1Mapstone" },
            { new Position(249.4, 403.0), "HoruR1EnergyCell" },
            { new Position(318.0, 245.0), "HoruR3Plant" },
            { new Position(191.7, 165.2), "HoruR4StompExp" },
            { new Position(253.5, 194.6), "HoruR4LaserExp" },
            { new Position(163.4, 136.4), "HoruR4DrainedExp" },
            { new Position(129.8, 165.6), "HoruLavaDrainedRightExp" },
            { new Position(106.5, 112.0), "DoorWarpExp" },
            { new Position(98.0, 130.5), "HoruTeleporterExp" },
            { new Position(264.0, 380.0), "HoruR1" },
            { new Position(172.0, 288.0), "HoruR2" },
            { new Position(304.0, 304.0), "HoruR3" },
            { new Position(216.0, 192.0), "HoruR4" },
            { new Position(-92.0, 376.0), "HoruL1" },
            { new Position(-20.0, 276.0), "HoruL2" },
            { new Position(-164.0, 336.0), "HoruL3" },
            { new Position(-96.0, 152.0), "HoruL4" },
            { new Position(-206.0, -113.3), "ValleyEntryAbilityCell" },
            { new Position(-221.0, -84.0), "ValleyEntryTreeExp" },
            { new Position(-179.0, -88.0), "ValleyEntryTreePlant" },
            { new Position(-320.0, -162.0), "ValleyEntryGrenadeLongSwim" },
            { new Position(-355.4, 65.6), "ValleyRightFastStomplessCell" },
            { new Position(-418.8, 67.5), "ValleyRightExp" },
            { new Position(-292.0, 20.7), "ValleyRightBirdStompCell" },
            { new Position(-460.0, -13.0), "GlideSkillFeather" },
            { new Position(-546.2, 54.4), "KuroPerchExp" },
            { new Position(-408.0, -170.0), "ValleyMap" },
            { new Position(-468.0, -67.0), "ValleyMainPlant" },
            { new Position(-572.0, 157.0), "WilhelmExp" },
            { new Position(-359.0, -87.5), "ValleyRightSwimExp" },
            { new Position(-415.5, -80.0), "ValleyMainFACS" },
            { new Position(-460.0, -187.0), "ValleyForlornApproachGrenade" },
            { new Position(-350.9, -98.7), "ValleyThreeBirdAbilityCell" },
            { new Position(-561.0, -89.0), "LowerValleyMapstone" },
            { new Position(-538.0, -104.0), "LowerValleyExp" },
            { new Position(-460.0, -255.0), "OutsideForlornTreeExp" },
            { new Position(-514.5, -277.3), "OutsideForlornWaterExp" },
            { new Position(-538.7, -234.7), "OutsideForlornCliffExp" },
            { new Position(-443.0, -152.0), "ValleyForlornApproachMapstone" },
            { new Position(-703.9, -390.0), "ForlornEntranceExp" },
            { new Position(-841.3, -350.9), "ForlornHiddenSpiderExp" },
            { new Position(-858.0, -353.0), "ForlornKeystone1" },
            { new Position(-892.0, -328.0), "ForlornKeystone2" },
            { new Position(-888.0, -251.0), "ForlornKeystone3" },
            { new Position(-869.0, -255.0), "ForlornKeystone4" },
            { new Position(-843.0, -308.0), "ForlornMap" },
            { new Position(-815.0, -266.0), "ForlornPlant" },
            { new Position(-625.5, -315.1), "RightForlornHealthCell" },
            { new Position(-732.0, -236.0), "ForlornEscape" },
            { new Position(-607.0, -314.0), "RightForlornPlant" },
            { new Position(-510.3, 204.3), "SorrowEntranceAbilityCell" },
            { new Position(-485.0, 323.0), "SorrowMainShaftKeystone" },
            { new Position(-503.0, 274.0), "SorrowSpikeKeystone" },
            { new Position(-514.0, 303.0), "SorrowHiddenKeystone" },
            { new Position(-596.0, 229.0), "SorrowLowerLeftKeystone" },
            { new Position(-451.0, 284.0), "SorrowMap" },
            { new Position(-435.0, 322.0), "SorrowMapstone" },
            { new Position(-609.0, 299.0), "SorrowHealthCell" },
            { new Position(-671.1, 290.0), "LeftSorrowAbilityCell" },
            { new Position(-608.0, 329.0), "LeftSorrowKeystone1" },
            { new Position(-612.0, 347.0), "LeftSorrowKeystone2" },
            { new Position(-604.0, 361.0), "LeftSorrowKeystone3" },
            { new Position(-613.1, 371.7), "LeftSorrowKeystone4" },
            { new Position(-627.1, 394.0), "LeftSorrowEnergyCell" },
            { new Position(-630.0, 249.0), "LeftSorrowPlant" },
            { new Position(-677.0, 269.9), "LeftSorrowGrenade" },
            { new Position(-456.0, 419.0), "UpperSorrowRightKeystone" },
            { new Position(-414.0, 429.0), "UpperSorrowFarRightKeystone" },
            { new Position(-514.0, 427.0), "UpperSorrowLeftKeystone" },
            { new Position(-545.0, 409.5), "UpperSorrowSpikeExp" },
            { new Position(-592.0, 445.0), "UpperSorrowFarLeftKeystone" },
            { new Position(-696.0, 408.0), "ChargeJumpSkillTree" },
            { new Position(-646.9, 473.1), "AboveChargeJumpAbilityCell" },
            { new Position(-560.0, 600.0), "Sunstone" },
            { new Position(-478.0, 586.0), "SunstonePlant" },
            { new Position(-678.1, -30.0), "MistyEntranceStompExp" },
            { new Position(-822.3, -9.7), "MistyEntranceTreeExp" },
            { new Position(-979.3, 23.6), "MistyFrogNookExp" },
            { new Position(-1076.0, 32.0), "MistyKeystone1" },
            { new Position(-1083.0, 8.3), "MistyMortarCorridorUpperExp" },
            { new Position(-1009.0, -35.0), "MistyMortarCorridorHiddenExp" },
            { new Position(-1102.0, -67.0), "MistyPlant" },
            { new Position(-1188.0, -100.0), "ClimbSkillTree" },
            { new Position(-912.0, -36.0), "MistyKeystone3" },
            { new Position(-837.7, -123.5), "MistyPostClimbSpikeCave" },
            { new Position(-796.0, -144.0), "MistyPostClimbAboveSpikePit" },
            { new Position(-768.0, -144.0), "MistyKeystone4" },
            { new Position(-671.9, -39.4), "MistyGrenade" },
            { new Position(-1043.0, -8.0), "MistyKeystone2" },
            { new Position(-1075.7, -2.2), "MistyAbilityCell" },
            { new Position(-720.0, -24.0), "GumonSeal" },
            { new Position(-168.1, -103.0), "SpiritTreeExp" }
        };


    }
}
