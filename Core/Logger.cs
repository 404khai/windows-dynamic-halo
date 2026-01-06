using System;
using System.IO;
using System.Diagnostics;

namespace WindowsDynamicHalo.Core
{
    public static class Logger
    {
        // Write to Desktop to ensure visibility and permissions
        private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "halo_debug_log.txt");

        public static void Log(string message)
        {
            string logEntry = $"{DateTime.Now:HH:mm:ss.fff}: {message}";
            
            // 1. Write to File
            try
            {
                File.AppendAllText(LogPath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Fallback if file write fails
                Debug.WriteLine($"FAILED TO WRITE TO LOG FILE: {ex.Message}");
            }

            // 2. Write to Debug Output (VS Output window)
            Debug.WriteLine(logEntry);

            // 3. Write to Console (Terminal)
            Console.WriteLine(logEntry);
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(LogPath))
                    File.Delete(LogPath);
            }
            catch { }
        }
    }
}