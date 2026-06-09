using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

        internal static (string, string, string) GetGameStudioLaunchExecutionParameters(bool isGame)
        {
            string exePath;
            string arguments = string.Empty;
            string oldMOProfileName = string.Empty;

            bool isVr = ManageSettings.CurrentGameIsHaveVr && ManageSettings.MainForm.VRGameCheckBox.Checked;

            if (ManageSettings.IsMoMode)
            {
                var customMOExeTitle = isGame ? ManageSettings.CurrentGameExemoProfileName + (isVr ? "VR" : "") 
                    : ManageModOrganizer.GetMOcustomExecutableTitleByExeName(ManageSettings.StudioExeName);
                exePath = ManageSettings.AppMOexePath; // set Mod organizer exe path

                if (ManageModOrganizer.TryGetMOProfileNameByExeTitle(customMOExeTitle, out string profileNameToRun))
                {
                    oldMOProfileName = ManageModOrganizer.SetCurrentProfileByName(profileNameToRun);
                }

                arguments = "moshortcut://:\"" + customMOExeTitle + "\"";
            }
            else
            {
                string exeName = isGame ? (isVr ? ManageSettings.CurrentGame.GameExeNameVr : ManageSettings.CurrentGameExeName) : ManageSettings.StudioExeName;
                exePath = Path.Combine(ManageSettings.CurrentGameDataDirPath, exeName + ".exe");
            }

            return (exePath, arguments, oldMOProfileName);
        }

        internal static void OnAfterRunMainGameStudioExe(string oldMOProfileName)
        {
            if (ManageSettings.IsMoMode && !string.IsNullOrEmpty(oldMOProfileName))
            {
                // return last profile
                ManageModOrganizer.SetCurrentProfileByName(oldMOProfileName);
            }
        }

        /// <summary>
        /// Execute the main game or studio exe, depending on the parameter. Before executing, it will wait if the game is changing, then kill all processes with the same name as the exe to prevent multiple instances, and after execution, it will return to the previous MO profile if in MO mode.
        /// </summary>
        /// <param name="isGame">Indicates whether to run the main game (true) or the studio (false).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async static Task RunMainGameStudioExe(bool isGame)
        {
            ManageSettings.MainForm.OnOffButtons(false);

            await Task.Run(() => ManageOther.WaitIfGameIsChanging()).ConfigureAwait(true);

            var (exePath, arguments, oldMOProfileName) = ManageProcess.GetGameStudioLaunchExecutionParameters(isGame);

            ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(exePath));
            ManageProcess.RunProgramAndWaitHidden(exePath, arguments);
            ManageProcess.OnAfterRunMainGameStudioExe(oldMOProfileName);

            ManageSettings.MainForm.OnOffButtons();
        }
    }
}
