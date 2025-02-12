using Core;
using Game;
using HarmonyLib;
using OriModding.BF.l10n;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    public class RandomizerController : MonoBehaviour, ISuspendable
    {
        public bool IsSuspended { get; set; }

        public static RandomizerController Instance { get; private set; }

        private static List<string> tips;

        private void Awake()
        {
            SuspensionManager.Register(this);
            Instance = this;
        }

        private void Start()
        {
            // populate tips in start to make sure Keybinder has loaded the keybinds
            tips = new List<string>()
            {
                $"Tip! Press ({Keybinder.ToString(KeybindAction.OpenTeleport)}) to teleport.",
                $"Tip! Press ({Keybinder.ToString(KeybindAction.Help)}) to see all keybinds.",
                $"Tip! Press ({Keybinder.ToString(KeybindAction.GoalProgress)}) to see your goal progress.",
                $"Tip! Press ({Keybinder.ToString(KeybindAction.ListStones)}) to see your keystones and mapstones.",
                $"Disconnected from the server? Press ({Keybinder.ToString(KeybindAction.Reconnect)}) to reconnect.",
                "Tip! You can adjust some randomizer settings in the bottom left corner of the pause menu."
            };
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OpenTeleportMenu()
        {
            if (Characters.Sein.Active && !Characters.Sein.IsSuspended && Characters.Sein.Controller.CanMove && !UI.MainMenuVisible)
            {
                if (TeleporterController.CanTeleport(null))
                {
                    string defaultTeleporter = "sunkenGlades";
                    float closestTeleporter = Mathf.Infinity;

                    bool isInGlades = false;
                    bool isInGrotto = false;

                    if (Scenes.Manager.CurrentScene.Scene.StartsWith("sunkenGlades"))
                        isInGlades = true;
                    else if (Scenes.Manager.CurrentScene.Scene.StartsWith("moonGrotto"))
                        isInGrotto = true;

                    foreach (GameMapTeleporter teleporter in TeleporterController.Instance.Teleporters)
                    {
                        if (teleporter.Activated)
                        {
                            if (isInGlades && teleporter.Identifier == "sunkenGlades")
                            {
                                defaultTeleporter = teleporter.Identifier;
                                break;
                            }
                            else if (isInGrotto && teleporter.Identifier == "moonGrotto")
                            {
                                defaultTeleporter = teleporter.Identifier;
                                break;
                            }

                            Vector3 distanceVector = teleporter.WorldPosition - Characters.Sein.Position;
                            if (distanceVector.sqrMagnitude < closestTeleporter)
                            {
                                defaultTeleporter = teleporter.Identifier;
                                closestTeleporter = distanceVector.sqrMagnitude;
                            }
                        }
                    }

                    TeleporterController.Show(defaultTeleporter);
                }
            }
        }

        private void ListStones()
        {
            if (RandomizerManager.Options.KeyStoneLogic == KeyStoneOptions.Anywhere)
            {
                int keyStonesRemaining = RandomizerManager.Receiver.GetItemCount(InventoryItem.KeyStone) -
                                         RandomizerManager.Receiver.GetItemCount(InventoryItem.KeyStoneUsed);
                RandomizerMessager.instance.AddMessage("KeyStones remaining: " + keyStonesRemaining);
            }
            else if (RandomizerManager.Options.KeyStoneLogic == KeyStoneOptions.AreaSpecific)
            {
                int gladesKeyStones = RandomizerManager.Receiver.GetItemCount(InventoryItem.GladesKeyStone) -
                                      RandomizerManager.Receiver.GetItemCount(InventoryItem.GladesKeyStoneUsed);
                int grottoKeyStones = RandomizerManager.Receiver.GetItemCount(InventoryItem.GrottoKeyStone) -
                                      RandomizerManager.Receiver.GetItemCount(InventoryItem.GrottoKeyStoneUsed);
                int ginsoKeyStones = RandomizerManager.Receiver.GetItemCount(InventoryItem.GinsoKeyStone) -
                                     RandomizerManager.Receiver.GetItemCount(InventoryItem.GinsoKeyStoneUsed);
                int swampKeyStones = RandomizerManager.Receiver.GetItemCount(InventoryItem.SwampKeyStone) -
                                     RandomizerManager.Receiver.GetItemCount(InventoryItem.SwampKeyStoneUsed);
                int mistyKeyStones = RandomizerManager.Receiver.GetItemCount(InventoryItem.MistyKeyStone) -
                                     RandomizerManager.Receiver.GetItemCount(InventoryItem.MistyKeyStoneUsed);
                int forlornKeyStones = RandomizerManager.Receiver.GetItemCount(InventoryItem.ForlornKeyStone) -
                                       RandomizerManager.Receiver.GetItemCount(InventoryItem.ForlornKeyStoneUsed);
                int sorrowKeyStones = RandomizerManager.Receiver.GetItemCount(InventoryItem.SorrowKeyStone) -
                                      RandomizerManager.Receiver.GetItemCount(InventoryItem.SorrowKeyStoneUsed);

                RandomizerMessager.instance.AddMessage("Glades KeyStones remaining: " + gladesKeyStones);
                RandomizerMessager.instance.AddMessage("Grotto KeyStones remaining: " + grottoKeyStones);
                RandomizerMessager.instance.AddMessage("Ginso KeyStones remaining: " + ginsoKeyStones);
                RandomizerMessager.instance.AddMessage("Swamp KeyStones remaining: " + swampKeyStones);
                RandomizerMessager.instance.AddMessage("Misty KeyStones remaining: " + mistyKeyStones);
                RandomizerMessager.instance.AddMessage("Forlorn KeyStones remaining: " + forlornKeyStones);
                RandomizerMessager.instance.AddMessage("Sorrow KeyStones remaining: " + sorrowKeyStones);
            }

            if (RandomizerManager.Options.MapStoneLogic == MapStoneOptions.Anywhere)
            {
                int mapStonesRemaining = RandomizerManager.Receiver.GetItemCount(InventoryItem.MapStone) -
                                         RandomizerManager.Receiver.GetItemCount(InventoryItem.MapStoneUsed);
                RandomizerMessager.instance.AddMessage("MapStones remaining: " + mapStonesRemaining);
            }
            else if (RandomizerManager.Options.MapStoneLogic == MapStoneOptions.AreaSpecific)
            {
                List<InventoryItem> mapStoneList = new List<InventoryItem>
                    {
                        InventoryItem.GladesMapStone,
                        InventoryItem.GroveMapStone,
                        InventoryItem.GrottoMapStone,
                        InventoryItem.SwampMapStone,
                        InventoryItem.ValleyMapStone,
                        InventoryItem.ForlornMapStone,
                        InventoryItem.SorrowMapStone,
                        InventoryItem.HoruMapStone,
                        InventoryItem.BlackrootMapStone
                    };

                string mapStoneString = mapStoneList.Where(x => RandomizerManager.Receiver.HasItem(x) && 
                                                                !RandomizerManager.Receiver.HasItem(x+1))
                                                    .Select(y => y.ToString())
                                                    .Join(z => z.ToString(), ", ");

                RandomizerMessager.instance.AddMessage("Current MapStones: " + mapStoneString);
            }
        }

        public static bool PlayerHasControl => Characters.Sein && Characters.Sein.Controller.CanMove && Characters.Sein.Active;

        private void FixedUpdate()
        {
            if (Keybinder.OnPressed(KeybindAction.OpenTeleport) && PlayerHasControl)
            {
                OpenTeleportMenu();
            }

            if (Keybinder.OnPressed(KeybindAction.ListStones))
            {
                ListStones();
            }

            if (Keybinder.OnPressed(KeybindAction.GoalProgress))
            {
                RandomizerManager.Connection.IsGoalComplete();
            }

            if (Keybinder.OnPressed(KeybindAction.Reconnect) && PlayerHasControl)
            {
                RandomizerMessager.instance.AddMessage("Attempting reconnection...");
                RandomizerManager.Receiver.OnReconnect();
                RandomizerManager.Connection.Reconnect();
            }

            if (Keybinder.OnPressed(KeybindAction.Help))
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (KeybindAction action in Enum.GetValues(typeof(KeybindAction)))
                {
                    stringBuilder.AppendLine($"{action}: {Keybinder.ToString(action)}");
                }
                stringBuilder.AppendLine("Check Keybinds.md on the GitHub for more info");
                RandomizerMessager.instance.AddMessage(stringBuilder.ToString());
            }

            if (LocalGameState.TeleportNightberry && PlayerHasControl && Items.NightBerry != null)
            {
                // teleport the nightberry to the location of the forlorn teleporter
                Items.NightBerry.transform.position = new Vector3(-910f, -300f);
                LocalGameState.TeleportNightberry = false;
            }
        }

        public void ShowRandomTip()
        {
            System.Random random = new System.Random();
            int index = random.Next(0, tips.Count);
            RandomizerMessager.instance.AddMessage($"{tips[index]}");
        }
    }
}
