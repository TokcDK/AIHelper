using System;
using AIHelper.Manage;
using AIHelper.Manage.Functions;

namespace AIHelper
{
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