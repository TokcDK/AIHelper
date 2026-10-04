using AIHelper.Manage.FoldersTab;
using AIHelper.Manage.Functions;
using AIHelper.Manage.ToolsTab;

namespace AIHelper.Manage
{
    /// <summary>
    /// Builds the content of the three tabs which are filled from what the assembly
    /// provides rather than from the designer.
    /// </summary>
    /// <remarks>
    /// Only the tab being shown is rebuilt, and the two others are emptied: a tab which is
    /// no longer visible must not keep controls around, because the loaders below add to
    /// whatever is already there.
    /// </remarks>
    internal class ManageTabs
    {
        internal static void LoadContent()
        {
            // load dinamic elements
            if (ManageSettings.MainForm.AIGirlHelperTabControl.SelectedTab == ManageSettings.MainForm.LaunchTabPage)
            {
                ManageSettings.MainForm.ToolsTabPageBackgroundPanel.Controls.Clear();
                ManageSettings.MainForm.FoldersTabPageBackgroundPanel.Controls.Clear();
                FunctionsForFlpLoader.Load();
            }
            else if (ManageSettings.MainForm.AIGirlHelperTabControl.SelectedTab == ManageSettings.MainForm.ToolsTabPage)
            {
                ManageSettings.MainForm.FunctionsForFLPTableLayoutPanel.Controls.Clear();
                ManageSettings.MainForm.FoldersTabPageBackgroundPanel.Controls.Clear();
                ToolsTabButtonsLoader.Load();
            }
            else if (ManageSettings.MainForm.AIGirlHelperTabControl.SelectedTab == ManageSettings.MainForm.FoldersTabPage)
            {
                ManageSettings.MainForm.FunctionsForFLPTableLayoutPanel.Controls.Clear();
                ManageSettings.MainForm.ToolsTabPageBackgroundPanel.Controls.Clear();
                FoldersTabButtonsLoader.Load();
            }
            else
            {
                ManageSettings.MainForm.FunctionsForFLPTableLayoutPanel.Controls.Clear();
                ManageSettings.MainForm.ToolsTabPageBackgroundPanel.Controls.Clear();
                ManageSettings.MainForm.FoldersTabPageBackgroundPanel.Controls.Clear();
            }
        }
    }
}
