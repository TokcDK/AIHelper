using System;
using System.Drawing;
using System.Globalization;
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

        /// <summary>
        /// The ini keys the window size is stored under. These are the names older
        /// versions wrote, so a size stored by one of those is still read.
        /// </summary>
        private const string IniWidthKey = "Width";
        private const string IniHeightKey = "Height";

        /// <summary>
        /// How long the window has to stay at one size before it is written to the ini.
        /// </summary>
        /// <remarks>
        /// Dragging the edge of a window raises a size change for every step of the drag,
        /// and writing the file once per step would be a lot of writes for one resize.
        /// The write waits for the drag to stop instead.
        /// </remarks>
        private const int SizeSaveDelayMilliseconds = 500;

        internal static void CalcSizeDependOnDesktop(Form f)
        {
            ApplyInitialFormSize(f);

            TrackFormSizeChanges(f);

            WireCustomFrameButtons(f);
        }

        /// <summary>
        /// Sets the form size from the ini file, or from the desktop resolution when
        /// there is no usable size stored.
        /// </summary>
        private static void ApplyInitialFormSize(Form f)
        {
            if (TryApplyStoredSize(f)) return;

            ApplyDefaultSize(f);
        }

        /// <summary>
        /// Applies the size the window was left at last time.
        /// </summary>
        /// <returns>
        /// <c>false</c> when nothing usable was stored, so the caller falls back to a
        /// size calculated from the desktop.
        /// </returns>
        private static bool TryApplyStoredSize(Form f)
        {
            try
            {
                var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
                var section = ManageSettings.IniSettingsSectionName;

                if (!ini.KeyExists(IniWidthKey, section) || !ini.KeyExists(IniHeightKey, section)) return false;

                if (!TryParseSize(ini.GetKey(section, IniWidthKey), out var width)) return false;
                if (!TryParseSize(ini.GetKey(section, IniHeightKey), out var height)) return false;

                var size = ClampToScreen(f, new Size(width, height));
                if (size.Width <= 0 || size.Height <= 0) return false;

                f.Size = size;
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to read the stored main form size. Error:\r\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// Size the window gets when it has never been resized: a fraction of the
        /// desktop, but never smaller than the window allows.
        /// </summary>
        private static void ApplyDefaultSize(Form f)
        {
            var desktop = Screen.PrimaryScreen.WorkingArea;

            var width = Math.Max(f.MinimumSize.Width, (int)(desktop.Width / 3.2));
            var height = Math.Max(f.MinimumSize.Height, (int)(width * 0.6));

            f.Size = ClampToScreen(f, new Size(width, height));
        }

        /// <summary>
        /// Keeps a size within what the window and the screen allow.
        /// </summary>
        /// <remarks>
        /// A size stored on a larger monitor would otherwise open the window larger than
        /// the screen it is opened on, with its window buttons out of reach.
        /// </remarks>
        private static Size ClampToScreen(Form f, Size size)
        {
            var desktop = Screen.PrimaryScreen.WorkingArea;

            var width = Math.Min(Math.Max(size.Width, f.MinimumSize.Width), desktop.Width);
            var height = Math.Min(Math.Max(size.Height, f.MinimumSize.Height), desktop.Height);

            return new Size(width, height);
        }

        /// <summary>
        /// Reads a size which was written in the invariant format, so the value does not
        /// depend on the language the application happens to be running in.
        /// </summary>
        private static bool TryParseSize(string value, out int size)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out size);
        }

        /// <summary>
        /// Saves the form size in the ini file once the user has stopped resizing it.
        /// </summary>
        /// <remarks>
        /// The timer belongs to the window, so the size is read on the thread which owns
        /// the window rather than from a worker thread, and it is read after the drag has
        /// finished rather than while the window is still moving.
        /// </remarks>
        private static void TrackFormSizeChanges(Form f)
        {
            var saveTimer = new Timer { Interval = SizeSaveDelayMilliseconds };

            saveTimer.Tick += (o, e) =>
            {
                saveTimer.Stop();
                SaveSize(f);
            };

            f.SizeChanged += (o, e) =>
            {
                saveTimer.Stop();
                saveTimer.Start();
            };

            f.FormClosed += (o, e) => saveTimer.Dispose();
        }

        /// <summary>
        /// Writes the current size of the form to the ini file.
        /// </summary>
        /// <remarks>
        /// Called whenever the user stops resizing the window, and once more while it
        /// closes in case it was closed before that write happened.
        /// </remarks>
        internal static void SaveSize(Form f)
        {
            try
            {
                // a minimized window reports the size it will return to
                var size = f.WindowState == FormWindowState.Normal ? f.Size : f.RestoreBounds.Size;
                if (size.Width <= 0 || size.Height <= 0) return;

                var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);

                ini.SetKey(ManageSettings.IniSettingsSectionName, IniWidthKey, size.Width.ToString(CultureInfo.InvariantCulture));
                ini.SetKey(ManageSettings.IniSettingsSectionName, IniHeightKey, size.Height.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                _log.Error("Failed to save the main form size. Error:\r\n" + ex);
            }
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
