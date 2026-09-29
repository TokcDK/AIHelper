using AIHelper.Manage.ModeSwitch;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    /// <summary>
    /// Switches the current game between Mod Organizer mode and normal mode.
    /// </summary>
    class ManageMOModeSwitch
    {
        /// <summary>
        /// Switches the current game to the other mode.
        /// </summary>
        /// <remarks>
        /// Both modes are about Mod Organizer: the game is either managed by it or worked
        /// with directly, and only Mod Organizer knows where the files of a mod belong.
        /// Without an installation there is nothing to switch to, so the request is refused
        /// here rather than left to look like it did something.
        /// </remarks>
        internal static void SwitchBetweenMoAndStandartModes()
        {
            if (!ManageModOrganizer.IsInstalled)
            {
                MessageBox.Show(T._("Mod Organizer is not installed.") + " "
                    + T._("The game is worked with directly and the mode cannot be switched."));

                return;
            }

            if (ManageSettings.IsMoMode)
            {
                new ToCommonMode().Switch();
            }
            else
            {
                new ToMOMode().Switch();
            }
        }
    }
}
