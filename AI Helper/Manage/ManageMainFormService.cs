using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Utils;

namespace AIHelper.Manage
{
    internal class ManageMainFormService
    {
        static bool _isWritingSize;

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

                    _isWritingSize = false;
                });
            });
#endif

            ManageSettings.MainForm.FormCloseButton.Click += new EventHandler((o, e) =>
            {
                ManageSettings.MainForm.Close();
                Application.Exit();
            });
            ManageSettings.MainForm.FormMinimizeButton.Click += new EventHandler((o, e) =>
            {
                ManageSettings.MainForm.WindowState= FormWindowState.Minimized;
            });

            new MouseDragger(ManageSettings.MainForm);
        }

        const int RESIZE_HANDLE_SIZE = 10;
        internal static void Resize(ref Message m, Form f)
        {
            if (m.Msg != 0x0084/*NCHITTEST*/) return;

            if ((int)m.Result != 0x01/*HTCLIENT*/) return;

            Point screenPoint = new Point(m.LParam.ToInt32());
            Point clientPoint = f.PointToClient(screenPoint);
            if (clientPoint.Y <= RESIZE_HANDLE_SIZE)
            {
                if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)13/*HTTOPLEFT*/ ;
                else if (clientPoint.X < (f.Width - RESIZE_HANDLE_SIZE))
                    m.Result = (IntPtr)12/*HTTOP*/ ;
                else
                    m.Result = (IntPtr)14/*HTTOPRIGHT*/ ;
            }
            else if (clientPoint.Y <= (f.Height - RESIZE_HANDLE_SIZE))
            {
                if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)10/*HTLEFT*/ ;
                else if (clientPoint.X < (f.Width - RESIZE_HANDLE_SIZE))
                    m.Result = (IntPtr)2/*HTCAPTION*/ ;
                else
                    m.Result = (IntPtr)11/*HTRIGHT*/ ;
            }
            else
            {
                if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)16/*HTBOTTOMLEFT*/ ;
                else if (clientPoint.X < (f.Width - RESIZE_HANDLE_SIZE))
                    m.Result = (IntPtr)15/*HTBOTTOM*/ ;
                else
                    m.Result = (IntPtr)17/*HTBOTTOMRIGHT*/ ;
            }
        }
    }
}
