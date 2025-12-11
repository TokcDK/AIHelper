using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Manage
{
    static class ManageProcess
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static void RunProgramAndWaitHidden(string fileName, string arguments = "")
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            GC.Collect();//reduce memory usage before run a program

            Process program = new Process();

            //MessageBox.Show("outdir=" + outdir);
            program.StartInfo.FileName = fileName;

            if (arguments.Length > 0)
            {
                program.StartInfo.Arguments = arguments;
            }

            if (!ManageSettings.IsMoMode|| string.IsNullOrWhiteSpace(program.StartInfo.Arguments))
            {
                program.StartInfo.WorkingDirectory = Path.GetDirectoryName(fileName);
            }

            // свернуть
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize);

            _ = program.Start();
            program.WaitForExit();

            // Показать
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize);

            program.Dispose();
        }

        internal static void SimpleRunProcess(string fileName, string arguments = "")
        {
            using (var process = new Process())
            {
                try
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Arguments = arguments;
                    process.Start();
                }
                catch (Exception e)
                {
                    _log.Error($"Failed run process. Error:{e.Message}");
                }
            }
        }

        internal static IEnumerable<Process> GetProcesses(this string processName)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                yield return process;
            }
        }

        internal static void KillProcessesByName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return;
            }

            foreach (var process in GetProcesses(processName))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    _log.Error("Cant kill process \"" + process.ProcessName + "\" with id" + process.Id + ". Error:\r\n" + ex);
                }
            }
        }
    }
}
