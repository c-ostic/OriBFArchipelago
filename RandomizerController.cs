using Core;
using Game;
using HarmonyLib;
using OriModding.BF.l10n;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OriBFArchipelago
{
    public class RandomizerController : MonoBehaviour, ISuspendable
    {
        public bool IsSuspended { get; set; }

        public static RandomizerController Instance { get; private set; }

        private void Awake()
        {
            SuspensionManager.Register(this);
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void OpenTeleportMenu()
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

        public static bool PlayerHasControl => Characters.Sein && Characters.Sein.Controller.CanMove && Characters.Sein.Active;

        private void Update()
        {
            if (IsSuspended)
                return;

            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt) && UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                if (PlayerHasControl)
                {
                    OpenTeleportMenu();
                }
            }
        }
    }
}
