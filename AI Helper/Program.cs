using AIHelper.Manage;
using AIHelper.Manage.Update.SelfUpdate;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace AIHelper
{
    internal static class Program
    {
        //appname
        internal static readonly string AppName = ManageSettings.ApplicationProductName = Application.ProductName;

        /// <summary>
        /// How this instance of the application was started, read once at the start.
        /// </summary>
        /// <remarks>
        /// Set before anything else runs, so that whatever reads it later - the start of the main
        /// window in particular - sees the command line rather than the empty default.
        /// </remarks>
        internal static StartupOptions Startup { get; private set; } = StartupOptions.Empty;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Startup = StartupOptions.Parse(args);

            // Which folder the application runs from is part of what it is, and everything that
            // builds a path out of it - the resources, the tools, the update folders - is wrong
            // without it. It is settled here, at the entry point, and not while the main window is
            // being constructed: the hand-off below runs before that, and so would anything else
            // added to the start later.
            ManageSettings.ApplicationStartupPath = Application.StartupPath;

            using (Mutex mutex = new Mutex(true, AppName, out bool createdNew))
            {
                if (createdNew)
                {
                    // An update downloaded by an earlier session is applied before the window is
                    // built. The tool that applies it has to be started while this process is
                    // still running - it waits for this process to end - and the less of the
                    // session has happened by then, the less of it is thrown away by the restart
                    // the tool performs when it is done. This is the whole of the hand-off: the
                    // tool does everything else, and this process ends right here.
                    if (SelfUpdateRunner.TryApplyPendingUpdate())
                    {
                        return;
                    }

                    //load dll from selected subdir, need to move all related  dlls to the subdir in post build event
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSubFolder);

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    //A second copy was started, so bring the window of the running one back
                    //instead of opening a second one.
                    //https://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
                    //https://stackoverflow.com/questions/4566632/maximize-another-process-window-in-net
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            NativeMethods.ShowWindow(process.MainWindowHandle, NativeMethods.ShowRestore);
                            break;
                        }
                    }
                }
            }
        }

        static Assembly LoadFromSubFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RES", "lib");
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
    }
}
