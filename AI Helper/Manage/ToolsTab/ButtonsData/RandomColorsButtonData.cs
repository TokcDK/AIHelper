using System;
using System.Collections.Generic;
using System.IO;
using AIHelper.Manage.Functions;
using INIFileMan;

namespace AIHelper.Manage.ToolsTab.ButtonsData
{
    internal class RandomColorsButtonData : IToolsTabButtonData, IButtonDataDev
    {
        public string Text => T._("Random UI colors");

        public string Description => T._("Random colors for elements");

        public bool IsVisible => File.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "dev.txt"));

        public void OnClick(object o, EventArgs e)
        {
            ThemesLoader.SetRandomColors();
        }
    }
}
