using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ElysiumModMenu
{
    /// <summary>
    /// Filters verbose BepInEx output before it reaches diagnostics, console and
    /// disk. Warnings and errors always bypass the verbose-log switch.
    /// </summary>
    internal sealed class RepeatedLogFilter : ILogListener
    {
        private static readonly object InstallLock = new object();
        private static RepeatedLogFilter instance;

        private readonly object sync = new object();
        private readonly List<ILogListener> listeners;
        private bool disposed;

        private RepeatedLogFilter(IEnumerable<ILogListener> originalListeners)
        {
            listeners = originalListeners.Where(listener => listener != null).ToList();
        }

        public LogLevel LogLevelFilter
        {
            get
            {
                LogLevel levels = LogLevel.None;
                foreach (ILogListener listener in listeners)
                    levels |= listener.LogLevelFilter;
                return levels;
            }
        }

        public static void Install()
        {
            lock (InstallLock)
            {
                if (instance != null) return;

                PropertyInfo listenersProperty = typeof(Logger).GetProperty(
                    "Listeners",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                object listenersObject = listenersProperty?.GetValue(null);
                if (!(listenersObject is ICollection<ILogListener> globalListeners)) return;

                List<ILogListener> originalListeners = globalListeners
                    .Where(listener => listener != null && !(listener is RepeatedLogFilter))
                    .ToList();
                if (originalListeners.Count == 0) return;

                var filter = new RepeatedLogFilter(originalListeners);
                foreach (ILogListener listener in originalListeners)
                    globalListeners.Remove(listener);
                globalListeners.Add(filter);
                instance = filter;
            }
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if (eventArgs == null) return;

            bool isImportant = (eventArgs.Level & (LogLevel.Warning | LogLevel.Error | LogLevel.Fatal)) != 0;
            if (!ElysiumModMenuGUI.detailedLogsEnabled &&
                (!isImportant || IsKnownVerboseNoise(eventArgs)))
                return;

            ElysiumModMenuGUI.ObserveRawDiagnosticLog(eventArgs);
            Forward(sender, eventArgs);
        }

        private static bool IsKnownVerboseNoise(LogEventArgs eventArgs)
        {
            if ((eventArgs.Level & LogLevel.Warning) == 0) return false;

            string message = eventArgs.Data as string ?? eventArgs.Data?.ToString();
            if (message == null) return false;

            return message.StartsWith("Delay spawn for unowned ", System.StringComparison.Ordinal) ||
                message.StartsWith("Stored data for ", System.StringComparison.Ordinal) ||
                (message.StartsWith("[Server] > ", System.StringComparison.Ordinal) &&
                 message.Contains(" has SendMode set to Everything"));
        }

        private void Forward(object sender, LogEventArgs eventArgs)
        {
            foreach (ILogListener listener in listeners)
            {
                try
                {
                    if ((listener.LogLevelFilter & eventArgs.Level) != 0)
                        listener.LogEvent(sender, eventArgs);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed) return;
                disposed = true;
            }

            foreach (ILogListener listener in listeners)
            {
                try { listener.Dispose(); }
                catch { }
            }
        }
    }
}
