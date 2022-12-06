﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    internal class ManageMainFormService
    {
        static bool _isWritingSize = false;

        internal static void CalcSizeDependOnDesktop(Form f)
        {
            // get size from ini
            bool isSizeGotFromIni = false;
#if !DEBUG
            var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
            if(ini.KeyExists(nameof(f.Width), ManageSettings.IniSettingsSectionName)
                && ini.KeyExists(nameof(f.Height), ManageSettings.IniSettingsSectionName))
            {
                var ww = ini.GetKey(ManageSettings.IniSettingsSectionName, nameof(f.Width));
                var wh = ini.GetKey(ManageSettings.IniSettingsSectionName, nameof(f.Height));
                if (int.TryParse(ww, out int wwi) && int.TryParse(wh, out int whi))
                {
                    f.Size = new Size(wwi, whi);
                    isSizeGotFromIni = true;
                }
            }
#endif

            if (!isSizeGotFromIni)
            {
                // set default size depend on desktop
                var resolution = Screen.PrimaryScreen.Bounds;
                int w = (int)(resolution.Width / 3.2);
                f.Size = new Size(w, (int)(w * 0.6));
            }
#if !DEBUG
            // register size changed to save window size
            f.SizeChanged += new EventHandler((o, e) =>
            {
                if (_isWritingSize) return;

                _isWritingSize = true;

                Task.Delay(1000).ContinueWith(t =>
                {
                    // save window size
                    var ini1 = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

                    ini1.SetKey(ManageSettings.IniSettingsSectionName, nameof(f.Width), f.Width + "");
                    ini1.SetKey(ManageSettings.IniSettingsSectionName, nameof(f.Height), f.Height + "");
                });
            });
#endif
        }
    }
}
