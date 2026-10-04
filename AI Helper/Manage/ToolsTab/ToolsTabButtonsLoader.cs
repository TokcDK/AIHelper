using AIHelper.Manage.ui;

namespace AIHelper.Manage.ToolsTab
{
    /// <summary>
    /// Fills the tools tab with the buttons the assembly provides.
    /// </summary>
    internal static class ToolsTabButtonsLoader
    {
        internal static void Load()
        {
            ButtonListLoader.Load(
                ManageSettings.MainForm.ToolsTabPageBackgroundPanel,
                GetListOfSubClasses.Inherited.GetInterfaceImplementations<IToolsTabButtonData>());
        }
    }
}
