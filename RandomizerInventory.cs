using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OriBFArchipelago
{
    internal class RandomizerInventory
    {
        public int abilityCells;
        public int healthCells;
        public int energyCells;
        public int keyStones;
        public int mapStones;
        public int exp;

        public bool ginsoKey;
        public bool forlornKey;
        public bool horuKey;
        public bool cleanWater;
        public bool wind;

        public Dictionary<AbilityType, bool> abilities;
        public Dictionary<string, bool> teleporters;

        public RandomizerInventory()
        {
            Reset();
        }

        public RandomizerInventory(string fileName)
        {
            Reset();
            try
            {
                StreamReader sr = new StreamReader($"BepInEx\\plugins\\OriBFArchipelago\\Files\\{fileName}.txt");

                abilityCells = int.Parse(sr.ReadLine());
                healthCells = int.Parse(sr.ReadLine());
                energyCells = int.Parse(sr.ReadLine());
                keyStones = int.Parse(sr.ReadLine());
                mapStones = int.Parse(sr.ReadLine());
                exp = int.Parse(sr.ReadLine());

                ginsoKey = bool.Parse(sr.ReadLine());
                forlornKey = bool.Parse(sr.ReadLine());
                horuKey = bool.Parse(sr.ReadLine());
                cleanWater = bool.Parse(sr.ReadLine());
                wind = bool.Parse(sr.ReadLine());

                foreach (AbilityType type in abilities.Keys.ToList())
                {
                    abilities[type] = bool.Parse(sr.ReadLine());
                }

                foreach (string teleporter in teleporters.Keys.ToList())
                {
                    teleporters[teleporter] = bool.Parse(sr.ReadLine());
                }

                //Close the file
                sr.Close();

                Console.WriteLine("Successfully read save file");

            }
            catch (IOException e)
            {
                Console.WriteLine("Save file does not exist");
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not read save file: " + e);
            }
        }

        public void Reset()
        {
            abilityCells = 0;
            healthCells = 0;
            energyCells = 0;
            keyStones = 0;
            mapStones = 0;
            exp = 0;

            ginsoKey = false;
            forlornKey = false;
            horuKey = false;
            cleanWater = false;
            wind = false;

            abilities = new Dictionary<AbilityType, bool>()
            {
                { AbilityType.WallJump, false },
                { AbilityType.ChargeFlame, false },
                { AbilityType.DoubleJump, false },
                { AbilityType.Bash, false },
                { AbilityType.Stomp, false },
                { AbilityType.Glide, false },
                { AbilityType.Climb, false },
                { AbilityType.ChargeJump, false },
                { AbilityType.Dash, false },
                { AbilityType.Grenade, false },
            };

            teleporters = new Dictionary<string, bool>()
            {
                { "TPGlades", false },
                { "TPGrove", false },
                { "TPSwamp", false },
                { "TPGrotto", false },
                { "TPGinso", false },
                { "TPValley", false },
                { "TPForlorn", false },
                { "TPSorrow", false },
                { "TPHoru", false },
                { "TPBlackroot", false },
            };
        }

        public void Save(string fileName)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter($"BepInEx\\plugins\\OriBFArchipelago\\Files\\{fileName}.txt");

                sw.WriteLine(abilityCells);
                sw.WriteLine(healthCells);
                sw.WriteLine(energyCells);
                sw.WriteLine(keyStones);
                sw.WriteLine(mapStones);
                sw.WriteLine(exp);

                sw.WriteLine(ginsoKey);
                sw.WriteLine(forlornKey);
                sw.WriteLine(horuKey);
                sw.WriteLine(cleanWater);
                sw.WriteLine(wind);

                foreach (AbilityType type in abilities.Keys.ToList())
                {
                    sw.WriteLine(abilities[type]);
                }

                foreach (string teleporter in teleporters.Keys.ToList())
                {
                    sw.WriteLine(teleporters[teleporter]);
                }

                //Close the file
                sw.Close();

                Console.WriteLine("Successfully saved to file");

            }
            catch (Exception e)
            {
                Console.WriteLine("Could not save to file: " + e);
            }
        }

        public void AddAll(RandomizerInventory addInventory)
        {
            abilityCells += addInventory.abilityCells;
            healthCells += addInventory.healthCells;
            energyCells += addInventory.energyCells;
            keyStones += addInventory.keyStones;
            mapStones += addInventory.mapStones;
            exp += addInventory.exp;

            ginsoKey |= addInventory.ginsoKey;
            forlornKey |= addInventory.forlornKey;
            horuKey |= addInventory.horuKey;
            cleanWater |= addInventory.cleanWater;
            wind |= addInventory.wind;

            foreach (AbilityType type in abilities.Keys.ToList())
            {
                abilities[type] |= addInventory.abilities[type];
            }

            foreach (string teleporter in teleporters.Keys.ToList())
            {
                teleporters[teleporter] |= addInventory.teleporters[teleporter];
            }
        }

        public void Add(string item)
        {
            switch (item)
            {
                case "AbilityCell":
                    abilityCells++; break;
                case "HealthCell":
                    healthCells++; break;
                case "EnergyCell":
                    energyCells++; break;
                case "KeyStone":
                    keyStones++; break;
                case "Plant":
                    exp += 50; break;
                case "MapStone":
                    mapStones++; break;
                case "GinsoKey":
                    ginsoKey = true; break;
                case "ForlornKey":
                    forlornKey = true; break;
                case "HoruKey":
                    horuKey = true; break;
                case "CleanWater":
                    cleanWater = true; break;
                case "Wind":
                    wind = true; break;
                case "WallJump":
                case "ChargeFlame":
                case "DoubleJump":
                case "Bash":
                case "Stomp":
                case "Glide":
                case "Climb":
                case "ChargeJump":
                case "Dash":
                case "Grenade":
                    var skill = (AbilityType)Enum.Parse(typeof(AbilityType), item);
                    abilities[skill] = true; break;
                case "TPGlades":
                case "TPGrove":
                case "TPSwamp":
                case "TPGrotto":
                case "TPGinso":
                case "TPValley":
                case "TPForlorn":
                case "TPSorrow":
                case "TPHoru":
                case "TPBlackroot":
                    teleporters[item] = true; break;
                case "EX15":
                case "EX100":
                case "EX200":
                case "EX50":
                    int amount = int.Parse(item.Substring(2));
                    exp += amount; break;
            }
        }

        // TODO: this is really messy, change this
        public int CompareOn(RandomizerInventory other, string item)
        {
            if (other == null) return 1;

            switch (item)
            {
                case "AbilityCell":
                    return abilityCells.CompareTo(other.abilityCells);
                case "HealthCell":
                    return healthCells.CompareTo(other.healthCells);
                case "EnergyCell":
                    return energyCells.CompareTo(other.energyCells); 
                case "KeyStone":
                    return keyStones.CompareTo(other.keyStones);
                case "MapStone":
                    return mapStones.CompareTo(other.mapStones);
                case "GinsoKey":
                    return ginsoKey.CompareTo(other.ginsoKey);
                case "ForlornKey":
                    return forlornKey.CompareTo(other.forlornKey);
                case "HoruKey":
                    return horuKey.CompareTo(other.horuKey);
                case "CleanWater":
                    return cleanWater.CompareTo(other.horuKey);
                case "Wind":
                    return wind.CompareTo(other.horuKey);
                case "WallJump":
                case "ChargeFlame":
                case "DoubleJump":
                case "Bash":
                case "Stomp":
                case "Glide":
                case "Climb":
                case "ChargeJump":
                case "Dash":
                case "Grenade":
                    var skill = (AbilityType)Enum.Parse(typeof(AbilityType), item);
                    return abilities[skill].CompareTo(other.abilities[skill]);
                case "TPGlades":
                case "TPGrove":
                case "TPSwamp":
                case "TPGrotto":
                case "TPGinso":
                case "TPValley":
                case "TPForlorn":
                case "TPSorrow":
                case "TPHoru":
                case "TPBlackroot":
                    return teleporters[item].CompareTo(other.teleporters[item]);
                case "EX15":
                case "EX100":
                case "EX200":
                case "EX50":
                case "Plant":
                    return exp.CompareTo(other.exp);
                default:
                    return 0;
            }
        }
    }
}
