using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Manage.Functions
{
    internal class ThemesLoader
    {
        internal static List<IUITheme> GetList()
        {
            return GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IUITheme>().ToList();
        }
        internal static void ApplyDefaultTheme() { ApplyTheme(new DefaultTheme1(), ManageSettings.MainForm); }
        internal static void ApplyTheme(IUITheme theme, Control control)
        {
            if (control.Controls == null) return;

            foreach (var childControl in control.Controls)
            {
                if (!(childControl is Control c)) continue;

                if (c is Panel p && p.Name.EndsWith("BackgroundPanel"))
                    c.BackColor = theme.BackColorPanel;// Color.FromArgb(255, 19, 54, 55); //Color.FromArgb(255, 231, 216, 192);

                if (c is Button b)
                {
                    b.BackColor = theme.BackColorButton;// Color.FromArgb(255, 160, 107, 123); //Color.FromArgb(255, 143, 107, 79);
                    b.ForeColor = theme.ForeColorButton;// Color.White;
                }

                ApplyTheme(theme, c);
            }
        }
    }
}
