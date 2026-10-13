using System.Reflection;
using System.Runtime.InteropServices;

// The version of the tool is deliberately not the application's version. The tool is replaced
// together with the application - AI Helper refreshes RES\tools\aihelper-updater from the
// downloaded payload before it starts it - so what it is a version *of* is the update protocol,
// not the release. That protocol has not changed since the first one.
[assembly: AssemblyTitle("aihelper-updater")]
[assembly: AssemblyDescription("Applies a downloaded AI Helper update and starts the application again.")]
[assembly: AssemblyProduct("aihelper-updater")]
[assembly: AssemblyCompany("AI Helper")]
[assembly: ComVisible(false)]
[assembly: Guid("8f3c1a64-2b7e-4d91-9a5c-6e0b4f7d1c82")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
