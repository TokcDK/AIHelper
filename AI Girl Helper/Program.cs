using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AI_Girl_Helper
{
    static class Program
    {
        //предотвратить повторный запуск и показать окно, если свернуто
        //https://stackoverflow.com/questions/184084/how-to-force-c-sharp-net-app-to-run-only-one-instance-in-windows
        //https://stackoverflow.com/questions/4566632/maximize-another-process-window-in-net
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool SetForegroundWindow(IntPtr hWnd);
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Pinvoke declaration for ShowWindow
        private const int SW_RESTORE = 9;

        //appname
        private static readonly string AppName = Application.ProductName;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, AppName, out createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new AIGirlHelper());
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            ShowWindow(process.MainWindowHandle, SW_RESTORE);
                            //SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }
    }
}
