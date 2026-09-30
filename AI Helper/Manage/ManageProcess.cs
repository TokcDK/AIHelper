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

        /// <summary>
        /// Exit code reported for a program which was not run at all.
        /// </summary>
        internal const int ProgramNotRunExitCode = -1;

        public static void RunProgramAndWaitHidden(string fileName, string arguments = "")
        {
            RunProgramAndWaitHidden(fileName, arguments, out _);
        }

        /// <summary>
        /// Runs a program hidden and waits for it to exit, reporting how it exited.
        /// </summary>
        /// <remarks>
        /// The exit code is the only trace left by a run which ended before it was done:
        /// a user who stopped the program from inside it closes a window, and nothing else
        /// about the run says so. Reporting it here is what lets a caller tell that apart
        /// from a run which finished.
        /// </remarks>
        /// <param name="fileName">Path of the program to run.</param>
        /// <param name="arguments">Arguments for the program, or an empty string for none.</param>
        /// <param name="exitCode">
        /// Exit code of the program, or <see cref="ProgramNotRunExitCode"/> when it was not
        /// run at all.
        /// </param>
        public static void RunProgramAndWaitHidden(string fileName, string arguments, out int exitCode)
        {
            exitCode = ProgramNotRunExitCode;

            if (!File.Exists(fileName))
            {
                return;
            }

            GC.Collect();//reduce memory usage before run a program

            using (var program = new Process())
            {
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

                try
                {
                    _ = program.Start();
                    program.WaitForExit();

                    exitCode = program.ExitCode;
                }
                finally
                {
                    // Показать
                    ManageOther.SwitchFormMinimizedNormalAll(ManageSettings.ListOfFormsForMinimize);
                }
            }
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
                // dispose the process handle after use
                using (process)
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

        /// <summary>
        /// Get exe path and arguments to launch the main game or the studio
        /// </summary>
        /// <param name="isGame">Indicates whether to run the main game (true) or the studio (false).</param>
        /// <param name="isVr">true when the game must be launched in vr mode</param>
        /// <returns>exe path, arguments and old mo profile name which must be restored after the launch</returns>
        internal static (string, string, string) GetGameStudioLaunchExecutionParameters(bool isGame, bool isVr)
        {
            string exePath;
            string arguments = string.Empty;
            string oldMOProfileName = string.Empty;

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

            bool isVr = ManageSettings.CurrentGameIsHaveVr && ManageSettings.MainForm.VRGameCheckBox.Checked;

            var (exePath, arguments, oldMOProfileName) = ManageProcess.GetGameStudioLaunchExecutionParameters(isGame, isVr);

            ManageProcess.KillProcessesByName(Path.GetFileNameWithoutExtension(exePath));
            ManageProcess.RunProgramAndWaitHidden(exePath, arguments);
            ManageProcess.OnAfterRunMainGameStudioExe(oldMOProfileName);

            ManageSettings.MainForm.OnOffButtons();
        }
    }
}
