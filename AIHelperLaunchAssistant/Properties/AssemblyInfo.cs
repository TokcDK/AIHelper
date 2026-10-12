using System.Reflection;
using System.Runtime.InteropServices;

// General information about this assembly.
[assembly: AssemblyTitle("AI Helper Launch Assistant")]
[assembly: AssemblyDescription("Starts a program while AI Helper is closed and starts AI Helper again when it ends")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AI Helper Launch Assistant")]
[assembly: AssemblyCopyright("Denis K<TokcDK> 2019-2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// COM stays out of this: nothing here is meant to be reached from outside the process.
[assembly: ComVisible(false)]
[assembly: Guid("3f7c1e64-9a2b-4d5e-8c71-0b4d6a9e2f13")]

// Version of the tool. Unlike the application, this is written by hand: the tool is a few hundred
// lines that change far less often than the application around it, and the date-based scheme the
// application uses would need its whole ledger machinery copied in for no benefit.
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// The only libraries this tool loads are the Windows ones, and they are all in System32. Saying so
// keeps a library planted next to the executable from being picked up instead.
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
