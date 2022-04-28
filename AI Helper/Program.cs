using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AIHelper
{
    internal static class Program
    {
        //предотвратить повторный запуск и показать окно, если свернуто
        //https://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
        //https://stackoverflow.com/questions/4566632/maximize-another-process-window-in-net
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool SetForegroundWindow(IntPtr hWnd);
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Pinvoke declaration for ShowWindow
        private const int SwRestore = 9;

        //appname
        internal static readonly string AppName = Properties.Settings.Default.ApplicationProductName = Application.ProductName;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(true, AppName, out bool createdNew))
            {
                if (createdNew)
                {
                    //load dll from selected subdir, need to move all related  dlls to the subdir in post build event
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromSubFolder);

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            ShowWindow(process.MainWindowHandle, SwRestore);
                            //SetForegroundWindow(process.MainWindowHandle);
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
