using System.Diagnostics;

namespace hbkpsvc
{
    class Logging
    {
        public static void Log(string message, EventLogEntryType messageType)
        {
            string source = "hbkpsvc";
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, "Application");
            }

            EventLog log = new EventLog();
            log.Source = source;
            log.WriteEntry(message, messageType);
        }
    }
}
