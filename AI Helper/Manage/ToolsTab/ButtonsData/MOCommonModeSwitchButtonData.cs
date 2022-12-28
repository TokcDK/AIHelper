using System;
using AIHelper.Manage.Functions;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class MOCommonModeSwitchButtonData : IToolsTabButtonData
    {
        readonly IFunctionForFlp data = new MOModeSwitchForFlp();

        public string Text => ManageSettings.IsMoMode ? T._("MO->Common") : T._("Common->MO");

        public string Description => data.Description;

        public void OnClick(object o, EventArgs e)
        {
            data.OnClick(o, e);
        }
    }
}
