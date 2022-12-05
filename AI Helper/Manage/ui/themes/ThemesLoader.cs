using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AIHelper.Manage.ui.themes
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
            ManageSettings.CurrentTheme = new DefaultTheme();
            SetTheme(ManageSettings.CurrentTheme, ManageSettings.MainForm);
        }
        internal static void SetDarkTheme() { SetTheme(new DarkTheme(), ManageSettings.MainForm); }

        internal static void SetRandomColors()
        {
            SetTheme(new CrazyTheme(), ManageSettings.MainForm);
        }

        internal static void SetTheme()
        {
            var list = GetList();
            var cnt = list.Count;
            ManageSettings.CurrentTheme = list.FirstOrDefault(t => t.Name == new DefaultTheme().Name);
            
            var bs = new BindingSource();
            bs.DataSource = list;

            ManageSettings.MainForm.SelectThemeComboBox.DataSource = bs.DataSource;

            ManageSettings.MainForm.SelectThemeComboBox.DisplayMember = "Name";
            ManageSettings.MainForm.SelectThemeComboBox.ValueMember = "Name";

            ManageSettings.MainForm.SelectThemeComboBox.SelectedIndexChanged += new EventHandler((o, e) =>
            {
                var st = ManageSettings.MainForm.SelectThemeComboBox.SelectedItem as IUITheme;

                SetTheme(st);
            });

            var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

            if (ini.KeyExists(ManageSettings.IniThemeKeyName, ManageSettings.IniSettingsSectionName))
            {
                var themeName = ini.GetKey(ManageSettings.IniSettingsSectionName, ManageSettings.IniThemeKeyName);

                var t = list.FirstOrDefault(n => n.Name == themeName);
                ManageSettings.CurrentTheme = t == default ? ManageSettings.CurrentTheme : t;
            }

            ManageSettings.MainForm.SelectThemeComboBox.SelectedItem = ManageSettings.CurrentTheme;
        }
    }
}
