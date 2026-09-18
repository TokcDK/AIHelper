using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIHelper.Utils;
using NLog;

namespace AIHelper.Manage
{
    /// <summary>
    /// Custom window frame support of the main form: initial size, size persistence and window buttons
    /// </summary>
    internal class ManageMainFormService
    {
        static readonly Logger _log = LogManager.GetCurrentClassLogger();

#if !DEBUG
        /// <summary>
        /// Guards against re-entering the size-changed handler while the size is being written to ini
        /// </summary>
        static bool _isWritingSize;
#endif

        internal static void CalcSizeDependOnDesktop(Form f)
        {
            ApplyInitialFormSize(f);

            TrackFormSizeChanges(f);

            WireCustomFrameButtons(f);
        }

        /// <summary>
        /// set form size from ini or calculate default size from the desktop resolution
        /// </summary>
        /// <param name="f"></param>
        private static void ApplyInitialFormSize(Form f)
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

            if (isSizeGotFromIni) return;

            // set default size depend on desktop
            var resolution = Screen.PrimaryScreen.Bounds;
            int w = (int)(resolution.Width / 3.2);
            f.Size = new Size(w, (int)(w * 0.6));
        }

        /// <summary>
        /// save form size in ini when the form size was changed
        /// </summary>
        /// <param name="f"></param>
        private static void TrackFormSizeChanges(Form f)
        {
#if !DEBUG
            // register size changed to save window size
            f.SizeChanged += new EventHandler((o, e) =>
            {
                if (_isWritingSize) return;

                _isWritingSize = true;

                Task.Delay(1000).ContinueWith(t =>
                {
                    try
                    {
                        // save window size
                        var ini1 = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

                        ini1.SetKey(ManageSettings.IniSettingsSectionName, nameof(f.Width), f.Width + "");
                        ini1.SetKey(ManageSettings.IniSettingsSectionName, nameof(f.Height), f.Height + "");
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Failed to save the main form size. Error:\r\n" + ex);
                    }
                    finally
                    {
                        _isWritingSize = false;
                    }
                });
            });
#endif
        }

        /// <summary>
        /// wire close and minimize buttons of the custom window frame
        /// </summary>
        /// <param name="f"></param>
        private static void WireCustomFrameButtons(Form f)
        {
            var mainForm = f as MainForm;
            if (mainForm == null) return;

            mainForm.FormCloseButton.Click += new EventHandler((o, e) =>
            {
                mainForm.Close();
                Application.Exit();
            });
            mainForm.FormMinimizeButton.Click += new EventHandler((o, e) =>
            {
                mainForm.WindowState = FormWindowState.Minimized;
            });

            new MouseDragger(mainForm);
        }

        private const int ResizeHandleSize = 10;
        internal static void Resize(ref Message m, Form f)
        {
            if (m.Msg != 0x0084/*NCHITTEST*/) return;

            if ((int)m.Result != 0x01/*HTCLIENT*/) return;

            Point screenPoint = new Point(m.LParam.ToInt32());
            Point clientPoint = f.PointToClient(screenPoint);
            if (clientPoint.Y <= ResizeHandleSize)
            {
                if (clientPoint.X <= ResizeHandleSize)
                    m.Result = (IntPtr)13/*HTTOPLEFT*/ ;
                else if (clientPoint.X < (f.Width - ResizeHandleSize))
                    m.Result = (IntPtr)12/*HTTOP*/ ;
                else
                    m.Result = (IntPtr)14/*HTTOPRIGHT*/ ;
            }
            else if (clientPoint.Y <= (f.Height - ResizeHandleSize))
            {
                if (clientPoint.X <= ResizeHandleSize)
                    m.Result = (IntPtr)10/*HTLEFT*/ ;
                else if (clientPoint.X < (f.Width - ResizeHandleSize))
                    m.Result = (IntPtr)2/*HTCAPTION*/ ;
                else
                    m.Result = (IntPtr)11/*HTRIGHT*/ ;
            }
            else
            {
                if (clientPoint.X <= ResizeHandleSize)
                    m.Result = (IntPtr)16/*HTBOTTOMLEFT*/ ;
                else if (clientPoint.X < (f.Width - ResizeHandleSize))
                    m.Result = (IntPtr)15/*HTBOTTOM*/ ;
                else
                    m.Result = (IntPtr)17/*HTBOTTOMRIGHT*/ ;
            }
        }
    }
}
