using MonoMod.Utils;
using OriBFArchipelago.ArchipelagoUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OriBFArchipelago.Core
{
    internal class RandomizerSettings : MonoBehaviour
    {
        private static RandomizerSettings instance;

        public static bool EnableDebug => false; //Enables debug settings; use only for developers.

        private void Awake()
        {
            instance = this;
            ShowSettings = false;
        }

        public static bool SkipCutscenes => ArchipelagoOptionsScreen.SkipCutscenes;
        public static bool ShowSettings { get; set; }
        public static bool InSaveSelect { get; set; }
        public static bool InGame { get; set; }
        public static int ActiveSaveSlot { get; set; }
        public static bool SeenInfoMessage => MapTrackerOptionsScreen.SeenTrackerInfoPopup;
        public static bool DoubleBashAssist => ArchipelagoOptionsScreen.DoubleBashAssist;
        public static bool DoubleBashTap => ArchipelagoOptionsScreen.DoubleBashTap;
        public static bool GrenadeJumpAssist => ArchipelagoOptionsScreen.GrenadeJumpAssist;
        public static RandomizerMessager.MessagerState MessageState => ArchipelagoOptionsScreen.MessageState;
        public static float MessageDuration => ArchipelagoOptionsScreen.MessageDuration;
    }
}
