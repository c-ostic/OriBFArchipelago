using BepInEx;
using BepInEx.Logging;
using System;
using System.IO;

namespace OriBFArchipelago.MapTracker.Core
{
    public static class ModLogger
    {
        private static string logFile;
        private static readonly object lockObject = new object();
        private static ManualLogSource _logger;
        public static void Initialize(string modName, ManualLogSource logger)
        {
            _logger = logger;
            // Create logs directory if it doesn't exist
            string logsDirectory = Path.Combine(Paths.GameRootPath, "Logs");
            Directory.CreateDirectory(logsDirectory);

            // Create a log file with timestamp in name to avoid conflicts            
            logFile = Path.Combine(logsDirectory, $"{modName}.log");

            File.WriteAllText(logFile, string.Empty);
            // Create the file and write initial timestamp
            ModLogger.Info($"=== {modName} Log Started ===");
        }

        private static void Log(string text, string type)
        {
            if (_logger == null)
                return;

            lock (lockObject)
            {
                try
                {
                    File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss} {type}] {text}\n");
                }
                catch (Exception e)
                {
                    // Fallback to BepInEx logger if file writing fails
                    _logger.LogError($"Failed to write to mod log file: {e.Message}");
                }
            }
        }

        public static void Debug(string text)
        {
            Log(text, "Debug");
        }
        public static void Info(string text)
        {
            Log(text, "Info");
        }

        public static void Error(string text)
        {
            Log(text, "Error");
        }

        internal static void Warning(string text)
        {
            Log(text, "Warning");
        }
    }
}
