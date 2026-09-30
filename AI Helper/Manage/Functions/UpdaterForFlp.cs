using System;
using System.Drawing;
using NLog;

namespace AIHelper.Manage.Functions
{
    internal class UpdaterForFlp : FunctionForFlpBase
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public override string Symbol => T._("🌐");

        public override string Description => T._("Check for updates");

        public override void OnClick(object o, EventArgs e)
        {
            RunUpdate();
        }

        public override Color? ForeColor => Color.LightBlue;

        /// <summary>
        /// Runs the update pass and keeps the failure it may end with.
        /// </summary>
        /// <remarks>
        /// Nothing awaits the pass, so a task which is started and dropped takes its
        /// exception with it: the update would look as though it had done nothing, with
        /// nothing written down about why. This is the only place that failure can be
        /// caught, because the pass runs on the thread which draws the window.
        /// </remarks>
        private static async void RunUpdate()
        {
            try
            {
                await ManageUpdateMods.UpdateMods().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _log.Error("Failed to update mods. Error:\r\n" + ex);
            }
        }
    }
}
