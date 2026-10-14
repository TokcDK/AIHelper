using System;
using AIHelper.Manage.Functions;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class MOCommonModeSwitchButtonData : IToolsTabButtonData, IButtonDataAvailability
    {
        readonly MOModeSwitchForFlp data = new MOModeSwitchForFlp();

        public string Text => ManageSettings.IsMoMode ? T._("MO->Common") : T._("Common->MO");

        public string Description => data.Description;

        /// <summary>
        /// Switching modes moves files between the game folder and Mod Organizer, so it
        /// needs Mod Organizer to be installed.
        /// </summary>
        public bool IsAvailable => ManageModOrganizer.IsInstalled;

        public void OnClick(object o, EventArgs e)
        {
            data.OnClick(o, e);
        }
    }
}
