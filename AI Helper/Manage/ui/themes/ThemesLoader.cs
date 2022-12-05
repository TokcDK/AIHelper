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
        internal static void ApplyDefaultTheme() { ApplyTheme(new DefaultTheme1(), ManageSettings.MainForm); }
        internal static void ApplyTheme(IUITheme theme, Control control)
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

                ApplyTheme(theme, c);
            }
        }

        internal static void ApplyRandomColors()
        {
            ApplyTheme(new RandomColorsTheme(), ManageSettings.MainForm);
        }
    }
}
