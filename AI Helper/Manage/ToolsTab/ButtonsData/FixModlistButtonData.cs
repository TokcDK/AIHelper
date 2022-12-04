using System;
using System.Windows.Forms;
using AIHelper.Manage.Functions;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class FixModlistButtonData : IToolsTabButtonData
    {
        public string Text => T._("Fix modlist");

        public string Description => T._("Fix problems in current enabled mods list");

        public void OnClick(object o, EventArgs e)
        {
            FixModlist();
        }

        static void FixModlist()
        {
            ManageSettings.MainForm.OnOffButtons(false);
            //impossible to correctly update mods in common mode
            if (!ManageSettings.IsMoMode)
            {
                DialogResult result = MessageBox.Show(T._("Attention") + "\n\n" + T._("Correct modlist fixes possible only in MO mode") + "\n\n" + T._("Switch to MO mode?"), T._("Confirmation"), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    ManageMOModeSwitch.SwitchBetweenMoAndStandartModes();
                }
                else
                {
                    ManageSettings.MainForm.OnOffButtons();
                    return;
                }
            }

            new ManageRules.ModList().ModlistFixes();

            ManageSettings.MainForm.OnOffButtons();

            ManageSettings.MainForm.UpdateData();
        }
    }
}
