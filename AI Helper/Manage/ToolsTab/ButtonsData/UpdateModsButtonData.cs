using System;
using AIHelper.Manage.Functions;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class UpdateModsButtonData : IToolsTabButtonData
    {
        readonly IFunctionForFlp data = new UpdaterForFlp();

        public string Text => T._("Update");

        public string Description => data.Description;

        public void OnClick(object o, EventArgs e)
        {
            data.OnClick(o, e);
        }
    }
}
