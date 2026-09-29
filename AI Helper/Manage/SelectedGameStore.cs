using System;
using NLog;

namespace AIHelper.Manage
{
    /// <summary>
    /// Remembers which game the application was working with, so the next start opens
    /// on it.
    /// </summary>
    /// <remarks>
    /// The game is remembered by folder name rather than by index, because the list of
    /// games is rebuilt from the disk on every start and its order is not something the
    /// application controls.
    /// </remarks>
    internal static class SelectedGameStore
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Folder name of the game which was selected the last time.
        /// </summary>
        /// <returns>the folder name, or an empty string when none was remembered</returns>
        /// <remarks>
        /// A file which cannot be read is reported and treated as "nothing remembered",
        /// which makes the caller fall back to the first game it found.
        /// </remarks>
        internal static string Read()
        {
            try
            {
                var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

                return ini.GetKey(ManageSettings.SettingsIniSectionName, ManageSettings.SelectedGameIniKeyName) ?? string.Empty;
            }
            catch (Exception ex)
            {
                _log.Warn("The remembered game could not be read. Error:\r\n" + ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Remembers <paramref name="gameDirName"/> as the game to open next time.
        /// </summary>
        /// <param name="gameDirName">folder name of the game</param>
        /// <returns>true when the value was written</returns>
        /// <remarks>
        /// A failure is logged rather than thrown. This is called while the game is being
        /// switched and while the window is closing, and neither of those may be
        /// interrupted because a settings file could not be written - the selection is
        /// also stored again on the next switch and on close.
        /// </remarks>
        internal static bool Save(string gameDirName)
        {
            try
            {
                var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

                ini.SetKey(ManageSettings.SettingsIniSectionName, ManageSettings.SelectedGameIniKeyName, gameDirName ?? string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                _log.Warn("The selected game could not be saved. Error:\r\n" + ex);
                return false;
            }
        }
    }
}
