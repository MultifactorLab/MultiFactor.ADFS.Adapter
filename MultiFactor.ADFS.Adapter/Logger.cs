using System;
using System.Diagnostics;

namespace MultiFactor.ADFS.Adapter
{
    /// <summary>
    /// EventLog
    /// </summary>
    public class Logger
    {
        private string _source;

        public Logger(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public static void Info(string message)
        {
            WriteEvent(message, EventLogEntryType.Information);
        }
        public static void Warn(string message)
        {
            WriteEvent(message, EventLogEntryType.Warning);
        }
        public static void Error(string message)
        {
            WriteEvent(message, EventLogEntryType.Error);
        }

        private static void WriteEvent(string message, EventLogEntryType type)
        {
            try
            {
                using (var log = new EventLog("Application"))
                {  
                    log.Source = "MultiFactor";
                    log.WriteEntry(message, type);
                }
            }
            catch
            {
            }
        }
    }
}
