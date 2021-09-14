﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AIHelper.Manage
{
    static class ManageProcess
    {
        public static void RunProgram(string programPath, string arguments = "")
        {
            if (!File.Exists(programPath))
            {
                return;
            }

            GC.Collect();//reduce memory usage before run a program

            //fix mo profile name missing quotes when profile name with spaces
            //if (!string.IsNullOrWhiteSpace(arguments) && arguments.Contains("moshortcut://:") && !arguments.Contains("moshortcut://:\""))
            //{
            //    arguments = arguments.Replace("moshortcut://:", "moshortcut://:\"") + "\"";
            //}

            //if (Path.GetFileNameWithoutExtension(programPath) == "ModOrganizer" && arguments.Length > 0)
            //{
            //    Task.Run(() => RunFreezedMoKiller(arguments.Replace("moshortcut://:", string.Empty))).ConfigureAwait(false);
            //}

            Process program = new Process();

            //MessageBox.Show("outdir=" + outdir);
            program.StartInfo.FileName = programPath;

            if (arguments.Length > 0)
            {
                program.StartInfo.Arguments = arguments;
            }

            if (!ManageSettings.IsMoMode() || string.IsNullOrWhiteSpace(program.StartInfo.Arguments))
            {
                program.StartInfo.WorkingDirectory = Path.GetDirectoryName(programPath);
            }

            // свернуть
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize());
            //WindowState = FormWindowState.Minimized;
            //if (LinksForm == null || LinksForm.IsDisposed)
            //{
            //}
            //else
            //{
            //    LinksForm.WindowState = FormWindowState.Minimized;
            //}
            //if (extraSettingsForm == null || extraSettingsForm.IsDisposed)
            //{
            //}
            //else
            //{
            //    extraSettingsForm.WindowState = FormWindowState.Minimized;
            //}

            _ = program.Start();
            program.WaitForExit();

            // Показать
            ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize());
            //WindowState = FormWindowState.Normal;
            //if (LinksForm == null || LinksForm.IsDisposed)
            //{
            //}
            //else
            //{
            //    LinksForm.WindowState = FormWindowState.Normal;
            //}
            //if (extraSettingsForm == null || extraSettingsForm.IsDisposed)
            //{
            //}
            //else
            //{
            //    extraSettingsForm.WindowState = FormWindowState.Normal;
            //}

            program.Dispose();
        }

        internal static IEnumerable<Process> GetProcesses(this string processName)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                yield return process;
            }
        }

        internal static Process GetIfExists()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    return process;
                    //SetForegroundWindow(process.MainWindowHandle);
                    //break;
                }
            }

            return null;
        }

        /// <summary>
        /// If Mod Organizer Will freeze in memory after main process will be closed then it will try to kill MO exe
        /// </summary>
        /// <param name="processName"></param>
        private static void RunFreezedMoKiller(string processName)
        {
            //if (string.IsNullOrEmpty(processName))
            //{
            //    return;
            //}

            ////https://stackoverflow.com/questions/262280/how-can-i-know-if-a-process-is-running

            //Thread.Sleep(5000);
            //while (Process.GetProcessesByName(processName).Length != 0)
            //{
            //    Thread.Sleep(5000);
            //}

            //try
            //{
            //    if (Process.GetProcessesByName("ModOrganizer").Length != 0)
            //    {
            //        foreach (var process in Process.GetProcessesByName("ModOrganizer"))
            //        {
            //            process.Kill();
            //        }
            //    }
            //}
            //catch
            //{
            //    MessageBox.Show("AIHelper: Failed to kill one of freezed ModOrganizer.exe. Try to kill it from Task Manager.");
            //}

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
                    ManageLogs.Log("Cant kill process \"" + process.ProcessName + "\" with id" + process.Id + ". Error:\r\n" + ex);
                }
            }
        }
    }
}