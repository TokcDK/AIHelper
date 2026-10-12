using System;
using System.IO;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// Works out which executable the assistant has to start again.
    /// </summary>
    /// <remarks>
    /// The tool is deployed inside the application it belongs to, at
    /// &lt;application&gt;\RES\tools\aihelper-launch-assistant, so the application's own executable can
    /// be found from the assistant's location and does not have to be passed in. The name is the one
    /// thing that cannot be derived, and it is the assembly name of the application.
    /// </remarks>
    internal static class HostApplicationLocator
    {
        /// <summary>Executable of the application the tool ships with.</summary>
        internal const string HostExecutableName = "AI Helper.exe";

        /// <summary>
        /// Process name of that application, without the ".exe" suffix, which is the form
        /// <see cref="System.Diagnostics.Process"/> works with. Derived here so the executable and the
        /// process name cannot drift apart.
        /// </summary>
        internal static string HostProcessName
        {
            get { return ProcessNames.Normalize(HostExecutableName); }
        }

        /// <summary>How far up from the tool's own folder to look for the host executable.</summary>
        private const int MaxSearchDepth = 4;

        /// <summary>
        /// Finds the host executable by walking up from the assistant's own location, or returns
        /// <c>null</c> when it is not there.
        /// </summary>
        internal static string Find(string assistantExecutablePath)
        {
            if (string.IsNullOrWhiteSpace(assistantExecutablePath))
            {
                return null;
            }

            string directory = Path.GetDirectoryName(assistantExecutablePath);

            for (int depth = 0; depth < MaxSearchDepth && !string.IsNullOrEmpty(directory); depth++)
            {
                string candidate = Path.Combine(directory, HostExecutableName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                // null at a drive root, "" for a bare name: both end the walk.
                directory = Path.GetDirectoryName(directory);
            }

            return null;
        }
    }
}
