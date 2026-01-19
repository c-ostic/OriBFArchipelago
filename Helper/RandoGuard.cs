using OriBFArchipelago.Core;
using OriBFArchipelago.MapTracker.Core;
using System;

namespace OriBFArchipelago.Helper
{
    internal static class RandoGuard
    {
        internal static bool IsFalseWithMessage(bool obj, string message = null)
        {
            if (obj == true)
                return false;

            if (message == null)
                message = $"{obj} should be set to true";


            ModLogger.Info(message);
            RandomizerMessager.instance.AddMessage(message);
            return true;
        }
        internal static bool IsTrueWithMessage(bool obj, string message = null)
        {
            if (obj == false)
                return false;

            if (message == null)
                message = $"{obj} should be set to true";

            ModLogger.Info(message);
            RandomizerMessager.instance.AddMessage(message);
            return true;
        }

        /// <summary>
        /// Checks whether property is null and if so will return a message that the player can see using RandomizerMessenger.
        /// </summary>
        /// <param name="obj">Object to check for null</param>
        /// <param name="message">The message that the user will see</param>
        /// <returns></returns>
        internal static bool IsNullWithMessage(object obj, string message = null)
        {
            if (obj != null)
                return false;

            if (string.IsNullOrEmpty(message))
                message = $"{obj} can not be null";

            ModLogger.Info(message);
            RandomizerMessager.instance.AddMessage(message);
            return true;
        }

        /// <summary>
        /// Checks whether property is null and if so will send message to log file
        /// </summary>
        /// <param name="obj">Object to check for null value</param>
        /// <param name="message">The mesage to log to the log files</param>
        /// <returns></returns>
        internal static bool IsNull(object obj, string message = null)
        {
            if (obj != null)
                return false;

            if (string.IsNullOrEmpty(message))
                message = $"{obj} can not be null";

            ModLogger.Info(message);
            return true;
        }
    }
}
