using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OriBFArchipelago.Core
{
    // Maintain game states that doesn't need to be saved
    internal class LocalGameState
    {
        public static bool IsGinsoExit = false;
        public static bool IsGrottoBridgeFalling = false;
        public static bool IsPendingCheckpoint = false;
        public static bool IsTeleporting { 
            get
            {
                Type teleporterType = typeof(TeleporterController);
                FieldInfo isTeleportingField = teleporterType.GetField("m_isTeleporting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var value = isTeleportingField.GetValue(TeleporterController.Instance);
                return (bool)value;
            }
        }

        public static bool QueueDoubleBash = false;
        public static bool WasDoubleBashQueued = false;
        public static bool QueueGrenadeJump = false;
        public static bool TeleportNightberry = false;

        public static void Reset()
        {
            IsGinsoExit = false;
            IsGrottoBridgeFalling = false;
            IsPendingCheckpoint = false;
            QueueDoubleBash = false;
            WasDoubleBashQueued = false;
            QueueGrenadeJump = false;
            TeleportNightberry = false;
        }
    }
}
