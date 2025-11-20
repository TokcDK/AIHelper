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
            foreach (Control c in control.Controls)
            {
                foreach (var el in theme.Elements)
                {
                    if (!el.Type.IsAssignableFrom(c.GetType())) continue;

                    if (!el.ForeColor.IsEmpty && !c.Name.Contains(ManageSettings.ThemeLabelColorSetIgnoreNameMark))
                        c.ForeColor = el.ForeColor;
                    if (!el.BackColor.IsEmpty && !c.Name.Contains(ManageSettings.ThemeLabelColorSetIgnoreNameMark))
                        c.BackColor = el.BackColor;
                    if (el.Font != null)
                        c.Font = el.Font;
                    if (el.BackgroundImage != null)
                        c.BackgroundImage = el.BackgroundImage;

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
            if (ManageSettings.CurrentTheme != default) return;

            var list = GetList();
            var cnt = list.Count;

            ManageSettings.CurrentTheme = list.FirstOrDefault(t => t.Name == new DefaultTheme().Name);

            // bind combobox to themes list
            var bs = new BindingSource
            {
                DataSource = list
            };
            ManageSettings.MainForm.SelectThemeComboBox.DataSource = bs.DataSource;
            ManageSettings.MainForm.SelectThemeComboBox.DisplayMember = "Name";
            ManageSettings.MainForm.SelectThemeComboBox.ValueMember = "Name";

            // add event to set current theme and write value in ini
            ManageSettings.MainForm.SelectThemeComboBox.SelectedIndexChanged += new EventHandler((o, e) =>
            {
                ManageSettings.CurrentTheme = ManageSettings.MainForm.SelectThemeComboBox.SelectedItem as IUITheme;
                
                var i = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
                i.SetKey(ManageSettings.IniSettingsSectionName, ManageSettings.IniThemeKeyName, ManageSettings.CurrentTheme.Name);

                SetTheme(ManageSettings.CurrentTheme);
            });

            // try read last selected them name
            var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
            if (ini.KeyExists(ManageSettings.IniThemeKeyName, ManageSettings.IniSettingsSectionName))
            {
                // set theme if found
                var themeName = ini.GetKey(ManageSettings.IniSettingsSectionName, ManageSettings.IniThemeKeyName);

                var t = list.FirstOrDefault(n => n.Name == themeName);
                ManageSettings.CurrentTheme = t == default ? ManageSettings.CurrentTheme : t;
            }

            // save selected index to check if combobox event fired
            var sel = ManageSettings.MainForm.SelectThemeComboBox.SelectedIndex;

            // set them as current in combobox
            ManageSettings.MainForm.SelectThemeComboBox.SelectedItem = ManageSettings.CurrentTheme;

            // set theme manually when index of them is not changed and event did not fired
            if(sel== ManageSettings.MainForm.SelectThemeComboBox.SelectedIndex) SetTheme(ManageSettings.CurrentTheme);
        }
    }
}
