using AIHelper.Manage.ToolsTab;
using AIHelper.Manage.ui;

namespace AIHelper.Manage.Functions
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
                GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IToolsTabButtonData>());
        }
    }
}
