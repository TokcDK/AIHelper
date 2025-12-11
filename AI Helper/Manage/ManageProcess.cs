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
        public static void RunProgram(string programPath, string arguments = "")
        {
            if (!File.Exists(programPath))
            {
                return;
            }

            GC.Collect();//reduce memory usage before run a program

            Process program = new Process();

            //MessageBox.Show("outdir=" + outdir);
            program.StartInfo.FileName = programPath;

            if (arguments.Length > 0)
            {
                program.StartInfo.Arguments = arguments;
            }

            if (!ManageSettings.IsMoMode|| string.IsNullOrWhiteSpace(program.StartInfo.Arguments))
            {
                program.StartInfo.WorkingDirectory = Path.GetDirectoryName(programPath);
            }

            // свернуть
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize);

            _ = program.Start();
            program.WaitForExit();

            // Показать
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize);

            program.Dispose();
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
