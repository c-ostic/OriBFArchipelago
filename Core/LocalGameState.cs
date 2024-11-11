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
        public static void Reset()
        {
            IsGinsoExit = false;
            IsPendingCheckpoint = false;
        }
    }
}
