using System;
using System.Diagnostics;
using System.Reflection;

namespace AIHelperLaunchAssistant
{
    /// <summary>
    /// Names, version and location of the tool, in the one place the usage text, the log and the icon
    /// all read them from.
    /// </summary>
    internal static class ApplicationInfo
    {
        internal const string Name = "AI Helper Launch Assistant";

        internal const string ExecutableName = "aihelper-launch-assistant.exe";

        /// <summary>Version of the assembly, so there is only one place a version is written down.</summary>
        internal static string Version
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return version == null ? "unknown" : version.ToString(3);
            }
        }

        /// <summary>
        /// Full path of the running executable. Everything the assistant works out from its own
        /// location - the host application, the log file, its own icon - starts here.
        /// </summary>
        internal static string ExecutablePath
        {
            get
            {
                try
                {
                    using (Process process = Process.GetCurrentProcess())
                    {
                        ProcessModule module = process.MainModule;
                        if (module != null && !string.IsNullOrEmpty(module.FileName))
                        {
                            return module.FileName;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Falls back to the assembly location below.
                }
                catch (NotSupportedException)
                {
                    // Falls back to the assembly location below.
                }

                return Assembly.GetExecutingAssembly().Location;
            }
        }
    }
}
