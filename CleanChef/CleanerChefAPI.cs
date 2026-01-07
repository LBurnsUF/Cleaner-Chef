using System;

namespace CleanerChef
{
    public static class CleanerChefAPI
    {
        /// <summary>Raised whenever HaltCorruption changes.</summary>
        public static event Action<bool> HaltCorruptionChanged;

        /// <summary>Current value of HaltCorruption (false if not initialized).</summary>
        public static bool HaltCorruptionEnabled { get; internal set; }

        internal static void RaiseHaltCorruptionChanged(bool enabled)
        {
            HaltCorruptionEnabled = enabled;
            HaltCorruptionChanged?.Invoke(enabled);
        }
    }
}
