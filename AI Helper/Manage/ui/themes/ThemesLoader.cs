using System;
using System.Collections.Generic;
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
        internal static void SetTheme(IUITheme theme)
        {
            SetTheme(theme, ManageSettings.MainForm);
        }
        internal static void SetRandomTheme()
        {
            var list = GetList();
            var cnt = list.Count;
            if (cnt == 0) return;
            var rnd = new Random();
            var theme = list[rnd.Next(0, cnt - 1)];
            SetTheme(theme);
        }

        internal static void SetTheme(IUITheme theme, Control control)
        {
            if (control.Controls == null) return;

            foreach (var childControl in control.Controls)
            {
                if (!(childControl is Control c)) continue;

                foreach (var el in theme.Elements)
                {
                    if (childControl.GetType() != el.Type) continue;

                    if (el.ForeColor != default) c.ForeColor = el.ForeColor;
                    if (el.BackColor != default) c.BackColor = el.BackColor;
                    if (el.Font != default) c.Font = el.Font;
                    if (el.BackgroundImage != default) c.BackgroundImage = el.BackgroundImage;

                    break;
                }

                SetTheme(theme, c);
            }
        }
        internal static void SetDefaultTheme() 
        {
            ManageSettings.CurrentTheme = new DefaultTheme1();
            SetTheme(ManageSettings.CurrentTheme, ManageSettings.MainForm); 
        }
        internal static void SetDarkTheme() { SetTheme(new DarkThemeVS(), ManageSettings.MainForm); }

        internal static void SetRandomColors()
        {
            SetTheme(new RandomColorsTheme(), ManageSettings.MainForm);
        }
    }
}
