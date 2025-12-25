using OriBFArchipelago.ArchipelagoUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OriBFArchipelago.Helper
{
    internal class RandomizerInformationManager
    {
        internal static void ShowWelcomeMessage()
        {

            {
                var messageText = @"Welcome to the archipelago randomizer!
There is an ingame tracker available but turned off by default.
Please note that the ingame tracker is in its early stages and can display incorrectly.
";
                var confirmText = "How do I enable tracker";
                var cancelText = "I've read all I want to";
                var trackerInformationBox = new RandomizerMessageBox(messageText, ShowTrackerInstructions, confirmText, SetSeenTrackerInfoPopup, cancelText);
                trackerInformationBox.Show();
            }
        }

        internal static void SetSeenTrackerInfoPopup()
        {
            MapTrackerOptionsScreen.SetSeenTrackerInfoPopup(true);
        }

        private static void ShowTrackerInstructions()
        {
            var messageText = @"You can turn on the ingame tracker by going into options > item tracker options and setting icon and map visibility

You can double check validity of checks against Universal Tracker of the Poptracker version which are more reliable at the moment.";
            var confirmText = "Ok";
            var trackerInformationBox = new RandomizerMessageBox(messageText, SetSeenTrackerInfoPopup, confirmText);
            trackerInformationBox.Show();
        }
    }
}
