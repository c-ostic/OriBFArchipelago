using HarmonyLib;
using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using OriBFArchipelago.MapTracker.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OriBFArchipelago.Patches.RemoveAnimations
{
    [HarmonyPatch(typeof(ActionSequence), nameof(ActionSequence.Perform))]
    internal static class ActionSequencePatch
    {
        private static bool enableLogging = true;
        private static bool eableHandlingLogging = true;
        public static List<MoonGuid> loggedGuids { get; set; } = [];


        private static IEnumerable<MoonGuid> IgnoredCutscenes = new List<MoonGuid>()
        {
            new MoonGuid("35249383 1283924914 2133692343 -729656019") //Tree of whispers
        };
        private static IEnumerable<string> IgnoredActionSequenceTypes = new List<string>()
        {
            "*damageSequence",
            "*deathSequence",
            "*hurtSequence",
            "*deadSequence",
            "onDeathAction",
            "noPointsActiveAction", //Not sure what this is
            "allPointsActiveAction", //Not sure what this is
            "actionSein", //Jumping on jump pad,
            "emptySlotPressed",
            "startGameSequence",
            "pressed",
            "saveGameSequence",
            "checkpointSequence",
            "enterAction",
            "exitAction",
            "landSequence",
            "doorFailSequence",
            "shootProjectile",
        };


        private static IEnumerable<MoonGuid> IgnoredActionSequences = new List<MoonGuid>
        {
            new MoonGuid("-933034724 1235203200 1527549604 -1462972182"),
            new MoonGuid("-87923899 1143790382 1365678474 126259305"),
            new MoonGuid("-982116190 1144303431 -1000890704 947257701"),
            new MoonGuid("-525337297 1201000768 1873908633 -1107931824"),
            new MoonGuid("895140080 1268873989 -1755951226 -1711992871"),
            new MoonGuid("-1604512648 1221335156 686798758 -513905122"),
            new MoonGuid("-744906621 1320888060 119868550 1452593774"),
            new MoonGuid("-1429415529 1268195290 -1966204791 -1505244940"),
            new MoonGuid("-1405682094 1341136820 1302676867 688360457"),
            new MoonGuid("-1706014048 1097871675 -1735743594 1338903756"),
            new MoonGuid("1421435139 1291248485 1102137270 -783837366"),
            new MoonGuid("158170661 1197238018 -792257131 62749936"),
            //All above related to damaging and killing enemies
            new MoonGuid("1267809327 1276225883 -1283782994 1796795750"),
            new MoonGuid("-1603862557 1089955631 -1481416054 -603316832"),            
            //All above are unknown what they do

            new MoonGuid("1263386797 1221647652 1846010499 -578854057"), //Keystone fly to user
            new MoonGuid("-865516688 1270747745 1166912703 -1549797483"), //Inventory change sound
            new MoonGuid("-1414350952 1227565761 176465076 -136605616"), //Inventory change sound
            new MoonGuid("-2055588002 1167681587 547357861 1780117491"), //Inventory select 
            new MoonGuid("-921666438 1102527873 1495434641 -742626407"), //Inventory select sound
            new MoonGuid("-1618349656 1269615211 -949111642 -503599033"), //Level up sequence
            new MoonGuid("1179896597 1123440810 1723487126 -1482280298"), //Blackroot Burrows title card
            new MoonGuid("-367309834 1111995082 -1919647859 1668331845"), //Sunken glades title card
            new MoonGuid("-867372507 1217277839 -1194013807 1668773622"), //Jump pad bounce
            
            new MoonGuid("-1486703868 1095031711 -624702302 641094419"), //Spirit tree text at sein

            //new MoonGuid("-232182932 1209203051 -2122766693 -1152089597"), //Blackroot bottom spirit torch 1
            //new MoonGuid("1941836092 1228817173 -574556000 1650436805"), //Blackroot bottom spirit torch 2
            //new MoonGuid("-2103903354 1266690147 1792051084 1872092184"), //Blackroot bottom spirit torch 3
            //new MoonGuid("2003114056 1236350661 -157534318 171094914"), //Blackroot bottom spirit torch 4
            //new MoonGuid(" 770734279 1301085176 713134479 -643508546"), //Blackroot bottom spirit torch 5
            //new MoonGuid("-974529720 1254621206 -488955245 -891109339"), //Blackroot bottom spirit torch 6
            //new MoonGuid("-46614877 1166294262 -699399495 400774248"), //Blackroot bottom spirit torch 6

            new MoonGuid("1351495039 1324533952 -118993999 1037842765"), //Start menu hint
            new MoonGuid("-2109804156 1131730672 -2009056885 -729374555"), //Deactivating hint zone

            new MoonGuid("-1130500164 1203472173 -2060408173 -103327691"), //Charging enemy at start
            new MoonGuid("-184465967 1160855336 1380848257 1425202086"), //Actually charging

            new MoonGuid("780498513 1100677661 1944628894 40893783"), //Pause menu
            new MoonGuid("-1517606521 1235598051 -746186569 248023522"), //Return to main menu
            new MoonGuid("-366063036 1142508438 -267824255 2129018456"), //Return to main menu confirm question
            new MoonGuid("425753223 1264549420 667090096 -566696812"), //Return to main menu confirm 

            new MoonGuid("1502331497 1172407104 1870144679 -1566526065"), //Collection of keystone


            //new MoonGuid(""),
        };


        private static IEnumerable<ActionSequenceExtension> _actionSequenceExtensions = new List<ActionSequenceExtension>()
        {
            new("FirstHealthOrb", "Animation for collecting first health orb", [05], new MoonGuid("-1640494324 1332054917 -484692311 -1370069449")),
            new("FirstEnergyOrb", "Animation for collecting first energy orb", [05], new MoonGuid("-19356878 1117716508 -1253827710 -315997777")),
            new("FirstXPOrb", "Animation for collecting first xp orb", [05], new MoonGuid("-147680714 1089154972 67598721 862472043")),
            //new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("1990366120 1332263646 -2040573797 -1880444431")),
            //SavePedestals
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("-1966853268 1195618287 -590473547 -1494253540")), //Sunken Glades 
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("-751058081 1271097205 1101442470 238108912")), //Moon Grotto
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("863615873 1243313958 -1075371851 739952073")), //Forlorn Ruins
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("-1154652467 1286431676 -1128360514 1797572823")), //Thornfelt Swamp
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("-1602522028 1238140958 153759634 -534094651")), //Horu Field
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("695661863 1323206629 1326601601 -256484684")), //Mount Huro             
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("1852895079 1212385171 -1161000781 2140306541")), //Sorrow Pass
            new("SavePedestalAction","Animation for when saving at a save pedestal", [01, 04, 06, 08, 10, 11,12], new MoonGuid("1990366120 1332263646 -2040573797 -1880444431")), //Blackroot Burrows
            
            //SunkenGlades
            new("InitialSpawn","Cutscene that explains what sunwells do", [1, 4, 5, 10, 11, 14, 15, 16, 17], new MoonGuid("-847281790 1241793475 1080917670 -1990575197")),
            new("Sein","Collection of sein",[01, 02, 04, 05, 06, 18, 21, 22, 30, 39, 44, 46, 50, 51,52], new MoonGuid("-264884829 1192335403 997860227 440544885")),
            new("FronkeyFight", "Killing the 3 fronkeys", [21,23], new MoonGuid("777438106 1287194490 -1646540149 -148368935")),
            new("SunkenGladesTeleporter","Cutscene that explains what sunwells do", [11], new MoonGuid("-1965591914 1122371942 1831952010 -327674226")),
            new("FirstKeystoneDoor", "Cutscene that triggers when you approach sunken glades keystone door", [14], new MoonGuid("-1395114778 1278874370 -350057552 1114507062")),
            new("DoorOpeningSequence", "Runs when opening a door", [01, 02, 03, 04, 05, 07, 09, 10, 12, 13,14,15,16,18], new MoonGuid("-1922669055 1254794419 202462358 1800757594")),
            new("SunkenGladesGrenadeAtWater", "Cutscene that shows the opening of the underwater door in sunken glades", [03, 04, 09, 17],new MoonGuid("1865222289 1321724071 -1742292036 -281605782")),
            new("SunkenGladesMapStone", "Cutscene that triggers when approaching the mapstone in sunken glades", [15], new MoonGuid("2113839868 1240904231 -2113682503 -2123470106")),
            new("WallJumpSkillTree", "Cutscene that explains what the trees are", [12, 14], new MoonGuid("-1930755400 1341760787 -1205985134 742195902")),
            new("EnergyGate","Cutscene that explains energy gates",[14], new MoonGuid("-1546192206 1296366634 -822821188 -2033193275")),

            //Blackroot
            new("BeforeBlackrootStatue", "Cutscene before the statue from sunken glades to blackroot", [11], new MoonGuid("969124513 1142725948 -1552980856 552605659")),
            new("BlackrootStatue", "Cutscene at statue from sunken glades to blackroot", [44], new MoonGuid("746320762 1081963650 1132673183 -2101072877")),

            new("BlackrootLight", "Cutscene at blackroot light bulb", [13], new MoonGuid("-1143820929 1234045024 1654993341 -1159008444")),
            new("BlackrootLightOpeningDoor", "Opening of the door in blackroot darkness", [01, 03, 04, 08, 10, 17], new MoonGuid("-334997307 1327210186 1493744052 -1761866442")),
            new("BlackrootLightStatueLever", "Pulling the lever to open the area where the light goes in", [05, 06, 07, 10, 11], new MoonGuid("1130616771 1092097200 1055730091 -1144946263")),
            new("BlackrootLightStatue", "Putting in the light orb in the statue", [03, 04, 06, 10, 11, 15, 17, 24, 25, 26], new MoonGuid("-10669251 1162529949 803405450 -1997983234")),

            new("BeforeDashSkillTree", "Cutscene when walking upto dash skill tree", [17], new MoonGuid("-335956874 1086831074 156163220 -1920124430")),
            new("BlackrootTeleporter", "Cutscene that shows dash area in blackroot", [30], new MoonGuid("-1368061152 1323579809 -958908746 -954948757")),

            new("OpenGateToGrotto", "Opens the gate to moon grotto",[05,06,07,08, 09], new MoonGuid("811457255 1092625385 1848673415 1904400117")),

            //Hollow Grove
            new("ChargeFlameSkillTree","Cutscne that explains about ano", [14,15], new MoonGuid("26462499 1079965410 1822047116 -798669916")),
            new("1. SpiritTree", "First cutscene at spirit tree", [04, 13, 16, 17, 18], new MoonGuid("-711136679 1126088874 -679292744 1071704955")),
            new("2. SpiritTreeStartCutscene", "Starting the unskipable cutscene", [02, 03, 05, 06, 07, 08, 09], new MoonGuid("469605998 1261573223 -1353132142 -1554447430")), //Is this really unskipable?
            new("3. AfterSpiritTreeCutscene", "Cutscene after skipable cutscene", [03, 21,22], new MoonGuid("-2025079432 1110082339 -1920359013 -28269904")),
            new("4. ShowElements", "Shows all elements on the map", [27,28], new MoonGuid("-1364532790 1112407316 1844745097 1026494537")),
            new("5. EndOfSpiritTreeCutscene", "The finishing moments of the spirit tree cutscene", [01, 02, 04, 08, 10, 13, 16, 17, 19, 20], new MoonGuid("213370189 1226433519 -937546573 1776663918")),
            new("FronkeyAboveChargeFlame", "Cutscene that tells you about the broken bridge", [09], new MoonGuid("-1041337981 1188119947 -796771185 673416302")),
            new("SpiderEgg","Cutscene that explains what to do at the spider egg", [20], new MoonGuid("1585702376 1210128595 1687967394 1229935533")),
            new("PedestalStomp","Cutscene that shows the opening of the gate to the save pedestal", [06,07,11], new MoonGuid("-770082635 1314070652 1932291261 -1598448927")),
            new("LeverForPlatform", "Pulling the lever to lower the platform to the right of the tree", [05, 06], new MoonGuid("-878651643 1210790854 -1588004431 -1015433105")),

            //Horu fields
            new("1. DroppingStone","Shows the dropping of the stone", [12], new MoonGuid("-2137911009 1339882321 529298075 336350830")),
            new("2. DroppingStone","Shows the dropping of the stone", [01,02,03,05], new MoonGuid("185196820 1122042374 110970291 -5029481")),
            new("MountHoruDoorOpen", "Opening of mount horu's door", [03, 04, 05, 06, 07, 08, 13, 14],new MoonGuid("-1832697925 1245659008 1759198354 -126852414")),
             
            //Swamp
            new("GumoAtTree", "First encounter with Gumo", [01, 02, 03, 05, 06, 09, 16, 18, 19 ], new MoonGuid("1950260161 1273538601 -1538437959 1280125770")),
            new("GumoAtSpitters", "Gumo enabling all the spitters",[06, 08, 11, 16, 17],new MoonGuid("792606551 1290633918 -1717577576 -2067970926")),
            new("GumoJumpingDown", "Gumo jumping down and activating lasers",[01, 03, 05, 12, 13, 15], new MoonGuid("-1958716129 1091670628 1187537581 -244205506")),
            new("FirstKuroAppearance","Kuro showing his face for the first time", [02, 03],new MoonGuid("1778853495 1203245955 -1472097121 977949109")),
            new("OpenGinsoTree", "Opening of Ginso Tree", [05,06,09,10],new MoonGuid("395817233 1340225072 267074435 558261298")),
            new("StompWaterPole", "Opening of the gate in the water", [06,07,08,13],new MoonGuid("-756314413 1313504610 494822539 -1328747187")),
            new("StompTreePole", "Opening of the gate underneath the small tree", [06, 08, 12], new MoonGuid("1432024516 1307203918 1808270737 2070758549")),
            new("RightSideLightBulb", "Opening of the water door by grenading the lightbulb", [02,03,04,05,06,09,10,11,12,14,17,18], new MoonGuid("1098451239 1219833295 -1808300371 -866556387")),
            new("StompTree","Exposition on the stomp skilltree",[12,14], new MoonGuid("337967039 1290330560 -102101579 -35473256")),

            //Moon
            new("GumoJumpingFurtherDown","Gumo jumping down after moon TP",[05, 06],new MoonGuid("-217466045 1309290050 -477348435 1521107282")),
            new("GumoGettingAttacked","Gumo getting attacked by the first miniboss",[01, 02, 05, 06, 07, 08, 22, 23], new MoonGuid("1644266034 1159348567 1319740331 -549778202")),
            new("OpenDoorAfterGrottoShark", "Opening the door after killing the grotto shark", [01, 02, 03, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 27],new MoonGuid("-931246602 1172798988 -954463614 -1275889756")),
            new("GumoEscapeThroughSpikes","Gumo's path through the spike maze", [01, 02], new MoonGuid("-700209528 1331170895 65121928 1522345109")),
            new("GumoWaitingForAmbush", "Show gumo waiting ontop to ambush Ori",[08, 09, 10, 15, 19, 20, 21],new MoonGuid("1236217866 1298028289 2052795783 -1289478889")),
            new("RescueGump","Rescue gumo from the stone on top of him",[02, 05, 06, 07, 08, 10, 12, 14, 15, 16],new MoonGuid("53584786 1168928934 -777751624 1508091491")),
            new("GumoGiveWaterThing", "Gumo giving the Ginso Key to Ori", [01,02, 03,04,05, 07, 08, 10, 11, 17, 18, 19, 20, 21, 25], new MoonGuid("1376551603 1121813286 -104614488 1790121249")),
            new("GumoRunOverGap", "Activates when running to the left of the gap at bottom grotto",[04, 08, 09],new MoonGuid("636825529 1114295791 195746239 1758731857")),
            new("DoubleJumpSkillTree", "Skips the exposition before collecting double jump", [12], new MoonGuid("261338335 1184241805 1860098225 -2077367194")),

            //Valley of the Wind
            new("WaterLever", "Lever at the hollow grove side", [03, 04, 05, 09, 11, 18],new MoonGuid("-535165738 1133285013 -695290185 -1054012665")),
            new("OpenValleyGate", "Opens the gate to valley of the wind",[07, 08, 09, 13], new MoonGuid("901903725 1185454079 -233910905 1236736341")),
            new("ShowKuroLurking", "Shows Kuro lurking to kill Ori when going into the open", [13,14, 22, 23], new MoonGuid("554625759 1196984167 -1411865209 368717326")),
            new("KuroAtCliff", "Kuro overlooking the cliff, showing how to remove him",[05,06,16,17], new MoonGuid("-457224259 1076850475 402546573 -1836316074")),
            //new("KuroDropping", "Stomping kuro and making him leave", [01,02,03,04,05,06,07,08,09,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,32,33,34,35,36], new MoonGuid("1424557570 1329884842 -385751934 1745320476")), //Haven't figured out how to properly skip this. Skipping results in not being able to pick up feather
            //new("KuroFeather", "Collecting kuro's feather", [05,06,07,08,09,10,12,16,17], new MoonGuid("-756839669 1238273858 363643021 -1763422804")), //Needs to stay on to actually collect the feather properly
            new("TreeOfWhispersDoor", "Opens the door for lower left valley to the tree of whispers", [05,06,07], new MoonGuid("1519946723 1246680883 939576731 1472509622")),
            new("LightTreeDoor","Use grendae to light the bulb to open door to item", [01,03,04,05,06,07,10,11,12,13,15,18,19], new MoonGuid("712475150 1153485569 -222104651 386379640")),
            new("BottomDoor","Open the door to forlorn area", [05,06,07], new MoonGuid("1505975971 1099708282 -762174588 -1455573852")),

            //Lost Grove
            new("DashGate", "Opening gate at dash area", [01,02,03,04,09,10,11,12], new MoonGuid("458381796 1082263303 581967003 -1486563515")),
            new("BigCutscene", "The cutscene that shows about the friendship and parents intervening",[33, 38, 42, 48, 49], new MoonGuid("563632359 1185974542 1520397737 -258754801")),
            new("GrenadeSkillTree", "Shows when nearing the grenade skill tree", [14,15], new MoonGuid("-622973999 1263458193 41168301 -95283387")),
            new("GrendaeGate","Opening gate at grenade skill tree", [01,02,03,04,10,11,12,14,15,16,17], new MoonGuid("-978772582 1143642823 -1177073764 -284972556")),
            new("PressureGrenadePlateDoor","Opening of gate that requires a pressure plate to be held and throwing a grenade",[01,02,03,04,10,11,12,13], new MoonGuid("34615599 1179073922 33697180 451012224")),
            new("FirstLever", "Lever to open door to dash out sequence", [05,06,07,08], new MoonGuid("-1262154005 1161379472 408777355 1091836632")),
            new("SecondLever", "Lever before the dash out sequence", [01, 06, 07, 08, 09], new MoonGuid("-751033906 1080928688 -645894784 2092556967")),
            new("LosingDad", "Cutscene where Naru's dad dies", [43,45], new MoonGuid("-147850866 1235485514 2127604624 619107832")),

            //Ginso Tree
            new("GinsoTreeEntrance", "First entry of Ginso Tree", [01,05,21,22,23], new MoonGuid("1797081604 1288163207 -1681566832 -1939855093")),
            new("FightMiniBoss","Closing door behind you on start of miniboss fight", [03,04,05,06,08,12,13], new MoonGuid("1183926712 1201180019 -328380759 -2018562755")),
            new("GinsoAfterMiniBoss","Exposition on what to do after killing the miniboss",[20,21,22,23], new MoonGuid("-2129976299 1224686485 -470376798 304795154")),
            new("GinsoEscapeLeftSide","Explosions on the left side before ginso escape", [03,05,06,07,08,09,10,11,12,13,14,15,16,17,18,19,10,20,21,22,23,24,25,26,27,28,29,30,31,33,35,36], new MoonGuid("-1236483305 1284618203 -1974049373 -575868861")),
            new("GinsoEscapeRightSide","Explosions on the right side before ginso escape", [04,06,07,08,09,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,37,40], new MoonGuid("1191654416 1302123663 151424932 840401249")),
            new("GinsoWaterSequence", "Sequence to start the water escape of ginso tree", [02,03,05,06,07,08,09,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32], new MoonGuid("-1679049206 1134072076 -1860355956 -1662641344")),
            new("EndOfGinsoEscape", "Exposition at the end of ginso escape and getting saved by gumon", [01, 02, 03, 10, 11, 12, 13, 14, 20, 29, 33, 37, 38, 40, 43, 44, 45, 46], new MoonGuid("-847281791 1241793475 1080917670 -1990575197")),
            new("SkipSkillTree", "Skips the exposition for the skilltree", [12], new MoonGuid("-314577372 1103920600 1985791414 -2110544796")),
            //Forlorn Cavern
            new("GumoSeal", "Insert gumo seal", [02,03,05,06,07,08,09,10,11,12,14,16], new MoonGuid("1987292132 1080256411 1972704413 111953066")),
            new("ForlornIntroduction", "First entry to forlorn cavern", [04,05,25,26], new MoonGuid("-849820852 1250504405 886572168 -1183457540")),
            new("GettingNightBerry", "Collection of the freezing orb", [10, 11,12,13,15], new MoonGuid("-468484755 1271818938 1374739346 -2106151641")),
            
            //Sorrow Pass
            new("SomeLever", "The lever that opens the door on the far left", [04,06], new MoonGuid("-1388084155 1237989621 -1066325088 -867368922")),
            new("ChargedJumpSkillTree", "Exposition at the skill tree for ChargedJump", [11,12,14], new MoonGuid("-1410197724 1128707013 -1950746477 1812865810")),
            new("SunStoneExposition", "Exposition at the top of sorrow pass before collecting sunstone", [15], new MoonGuid("-696092440 1113700240 -171193210 -1201469275")),            
            //Mount Horu
            new("HoruIntroduction", "First entry of Mount Horu", [01, 05, 07, 22, 23], new MoonGuid("-182315221 1105231312 1943840650 1813785295")),
            new("HuroL1Stomp", "Stomp on the platform at door 1", [02,03,4,055,06,07,08,09,10,11,12,13,17,18], new MoonGuid("-1589734316 1113886190 -437311593 899774608")),
            new("HoruL2RockDrop", "Drop the rock into the lava to stop the flow",[01, 02, 04, 05, 06, 07, 08, 11, 12, 13, 14], new MoonGuid("-885091187 1141805217 1126505132 -234469124")),
            new("HoruR2RockFall", "Stomp on the switch to drop the block at the end",[02, 03, 08, 09], new MoonGuid("2031988133 1253051438 -1048575855 -1243562809")),
            new("HoruR1DoorEntrance", "Exposition on story when entering door",[04,08,09], new MoonGuid("-1038430429 1234410252 2139859126 -490676512")),
            new("HoruR1DoorEntrance2", "More exposition on story when entering door",[25], new MoonGuid("-91325239 1253403492 1476455557 764231789")),
            new("HoruR1LightRockExposition", "Exposition on the light rocks",[14, 15], new MoonGuid("-303476169 1101394032 20567448 -553215510")),
            new("HoruR1LavaDrain", "Shine light on the rock at the 1rd (from the top) entrance of the left to lower the lava",[02,03,04,05,06,07,08], new MoonGuid("-944003312 1224624697 -191465038 -1871897436")),
            new("HoruR1LavaDrainFinish", "Exposition after dropping the first rock",[02, 04, 07, 08, 09, 10], new MoonGuid("199020531 1138092816 193035928 1030074132")),
            new("HoruR2LavaDrain", "Lowering of the lava due to actions at the 2nd (from the top) entrance of the right",[02,04,08,09,10,11,13,15,18], new MoonGuid("-1664229988 1269302847 -122889574 398995567")),
            new("HoruR3LavaDrain", "Shine light on the rock at the 3rd (from the top) entrance of the right to lower the lava",[01, 02, 03, 04, 07, 09, 10, 12, 1, 17], new MoonGuid("-496260207 1303555699 578011313 -1827639718")),
            new("HoruR4LavaDrain", "Shine light on the rock at the 4rd (from the top) entrance of the right to lower the lava",[02, 03, 04, 05, 06, 07, 08], new MoonGuid("-237534521 1117446743 1449463690 -1967567174")),
            new("HoruL2LavaDrain", "Lowering of the lava due to actions at the 2nd (from the top) entrance of the left",[02,03,04,08,09,10,11,13,15,18], new MoonGuid("-229872336 1122348114 -600436304 657168478")),
            new("HoruL3LavaDrain", "Shine light on the rock at the 3rd (from the top) entrance of the left to lower the lava",[02, 03, 04, 05, 06, 07, 08], new MoonGuid("-182458660 1303160168 1079892404 1715488948")),

            new("door2lavadrain", "Drains the lava?", [02,03,7,08,09,10,11,12,13,14,15,20], new MoonGuid("-1235656039 1086313701 1258576023 -104497747")),
            new("door8lavadrain", "Drains the lava?", [02,03,07,08,09,10,11,12,17], new MoonGuid("1446986356 1324984179 1323541149 1364584793")),

            //Doors from top to bottom            
            //Door left 3: 
            //new("1. door3LavaDrain", "ASD",[02, 03, 04, 05, 06, 08, 09, 10],new MoonGuid("-182458660 1303160168 1079892404 1715488948")),
            //new("2. door3LavaDrain", "ASD",[02, 03, 08, 09, 10, 11],new MoonGuid("-2124952831 1181382434 35333286 829377599")),             
            //new("3. door3LavaDrain", "ASD",[06, 07, 08],new MoonGuid("889927369 1310717430 -1483327331 -122332287")),
            //new("4. door3LavaDrain", "ASD",[02, 03, 05, 06, 10, 11, 12 ,13 ,15 ,17, 20],new MoonGuid("406127258 1339914959 423600281 -1062944213")),
             
            //Door left 4: Issue - There might be a small freeze when stomping for the third time due to the "if get world event condition" in this animation.
            new("1. door4LavaDrain", "ASD",[02, 04, 05, 06, 07, 14],new MoonGuid("-920418859 1183339757 90418109 -930667380")),
            new("2. door4LavaDrain", "Drains the lava in the lower left door", [02,03,04,08,13,14,15,16], new MoonGuid("1621954031 1114090025 1789235899 -1461548696")),

            //Misty Woods
            new("AboveLantern","Skips the exposition above the lantern", [03,05,08,15,17], new MoonGuid("-145071351 1205762473 -1338429310 33294940")),
            new("MistyWoodsSkillTree","Skips exposition on the skilltree in misty woods", [06,08], new MoonGuid("1565883492 1308263384 -1752567420 -1888351303")),
            new("LanternToOpenDoor","Latern hit with grenade to open stump for item", [03,04,05,06,07,09,10,11,12,13,15,18], new MoonGuid("716813929 1280992173 724076716 2101543441")),
            new("OrbInPlatform", "Puts the orb into the smokey platform", [07, 08, 09, 11, 12, 13, 14, 15,16 ,17, 18, 19, 21, 22, 23, 26, 27, 28, 29, 30, 31, 32, 33, 34], new MoonGuid("134252146 1207943286 -699389268 -654615683")),
            new("CollectGumonSeal", "Exposition on collection of gumon's seal", [03, 04, 05, 09, 11, 14, 22, 23, 24, 25], new MoonGuid("774264447 1144716649 -1525033835 -306857486"))
            /*todo: 
             * Horu L3
             * Horu L2
             * Horu L1
             * Horu R4
             * Horu R3
             * Horu R2
             * Horu R1
             * Switch in sorrow pass
             * Top sorrow pass
             * Misty woods above lantern
             * Misty woods pre pedestal
             */
            


        };

        private static Dictionary<MoonGuid, ActionSequenceExtension> actionSequenceExtensions;
        private static Dictionary<MoonGuid, ActionSequenceExtension> ActionSequenceExtensions
        {
            get
            {
                return actionSequenceExtensions ?? (actionSequenceExtensions = _actionSequenceExtensions.ToDictionary(d => d.Guid, d => d));
            }
        }


        internal static bool Prefix(ActionSequence __instance)
        {
            if (!RandomizerSettings.SkipCutscenes)
                return true;

            if (IgnoredActionSequences.Contains(__instance.MoonGuid))
                return true;

            if (IgnoredActionSequenceTypes.Contains(__instance.name))
                return true;

            if (!ActionSequenceExtensions.ContainsKey(__instance.MoonGuid))
            {
                LogUnknownActionSequence(__instance);
                return true;
            }
            var actionSequenceExt = ActionSequenceExtensions[__instance.MoonGuid];

            if (eableHandlingLogging)
            {
                ModLogger.Debug("==========================================================================================================================================");
                ModLogger.Debug($"Handeling action removal for trigger: {actionSequenceExt.Name} ({actionSequenceExt.Guid})");
                ModLogger.Debug("==== Before ====");
                LogActionSequenceActions(__instance);
            }
            __instance.Actions = __instance.Actions
                .Where(d => actionSequenceExt.ActionsToKeep.Any(c => d.name.StartsWith($"{c:00}."))) //First remove all actions we do not wish to take
                 .Select(action =>
                 {
                     if (action is BaseAnimatorAction)
                     {
                         if (__instance.MoonGuid == new MoonGuid("1621954031 1114090025 1789235899 -1461548696"))
                             return action;

                         var baseAnimatorAction = (action as BaseAnimatorAction);
                         baseAnimatorAction.Command = BaseAnimatorAction.PlayMode.StopAtEnd;
                         ModLogger.Debug($"{action.name}: Skipping to end");

                         //Todo: For some reason animatoractions will rerun after a save by the actionsequence, this can cause softlocks like at sein when you walk off the platform and it will attempt to walk you to a specific location.
                         //Figure out why some animations are rerun after a save and stop it from happening.
                         //Possible solution: Look for triggers that deactivate things and keep them enabled
                     }
                     return action; //Then always put a sequence at the end of its loop
                 }).ToList();

            if (eableHandlingLogging)
            {
                ModLogger.Debug("==== After ====");
                LogActionSequenceActions(__instance);
                ModLogger.Debug("==========================================================================================================================================");
            }
            return true;
        }

        internal static void Postfix(ActionSequence __instance)
        {
            if (IgnoredCutscenes.Contains(__instance.MoonGuid))
            {
                __instance.StartCoroutine(PostfixCoroutine(__instance));
            }
        }

        internal static IEnumerator PostfixCoroutine(ActionSequence __instance)
        {
            ModLogger.Debug("Attempting to skip cutscene");
            yield return new WaitForSeconds(0.3f);
            SkipCutsceneController.Instance.SkipCutscene();
        }

        private static void LogUnknownActionSequence(ActionSequence actionSequence)
        {
            if (!enableLogging)
                return;

            if (loggedGuids.Contains(actionSequence.MoonGuid))
                return;

            loggedGuids.Add(actionSequence.MoonGuid);

            ModLogger.Debug("==========================================================================================================================================");
            if (actionSequence == null)
            {
                ModLogger.Debug($"Unknown action sequence is null");
            }
            else
            {
                ModLogger.Debug($"Unknown action sequence: {actionSequence.name} [{actionSequence.GetType().Name}] - {actionSequence.MoonGuid}");
                LogActionSequenceActions(actionSequence);
            }
            ModLogger.Debug("==========================================================================================================================================");
        }

        private static void LogActionSequenceActions(ActionSequence actionSequence)
        {
            if (!enableLogging)
                return;
            try
            {
                if (actionSequence == null)
                    ModLogger.Debug($"Action sequence is null");
                else if (actionSequence.Actions == null || actionSequence.Actions.Count == 0)
                    ModLogger.Debug("No actions");
                else
                    foreach (ActionMethod action in actionSequence.Actions)
                        ModLogger.Debug($"{action.name} - {action.ToString()} - {action.GetType().Name}");
            }
            catch (System.Exception ex)
            {
                ModLogger.Error($"{ex}");
            }
        }
    }
}