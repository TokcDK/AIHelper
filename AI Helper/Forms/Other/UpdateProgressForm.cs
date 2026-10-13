using System;
using System.Drawing;
using System.Windows.Forms;
using AIHelper.Manage.ui.themes;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Window which is shown while a pass that can take a while runs, so that the phase it has
    /// reached can be read.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pass blocks the thread it runs on - a separate program is started and waited for, or a
    /// release is downloaded - so the message loop does not run and the window can not be
    /// repainted by it. The phase is therefore painted at the moment it is set, by
    /// <see cref="SetStatus"/>, rather than left to the next pass of the loop.
    /// </para>
    /// <para>
    /// The window is not the user's to close. The pass behind it can not be stopped from here,
    /// and a window which could be dismissed would leave that pass running with nothing on screen
    /// to say so. It is closed by the pass itself, from its own cleanup, which is the only reason
    /// <see cref="CloseSafely"/> exists.
    /// </para>
    /// <para>
    /// The window knows nothing about the pass it belongs to: the title is given when it is
    /// opened and the phases come from the pass, so a new phase is a line in the pass and nothing
    /// here. That is what lets the zipmods pass and the application's own update share it.
    /// </para>
    /// <para>
    /// Built in code rather than from a designer because it holds a single label and wires no
    /// events of its own.
    /// </para>
    /// </remarks>
    internal sealed class UpdateProgressForm : Form
    {
        private readonly Label _statusLabel;

        /// <summary>
        /// Set once the pass is done with the window, so that a report which arrives late - from
        /// a worker, or from a phase which was already past - can not touch a form whose handle
        /// is gone.
        /// </summary>
        private bool _isClosed;

        private UpdateProgressForm(string windowTitle)
        {
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;

            // no close button, and OnFormClosing refuses the ways in which a window without one
            // can still be dismissed
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = true;
            Size = new Size(400, 60);
            StartPosition = FormStartPosition.CenterScreen;
            Text = windowTitle ?? string.Empty;

            _statusLabel = new Label
            {
                AutoEllipsis = true,

                // painted from what is behind it, so that it needs no colour of its own
                BackColor = Color.Transparent,
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 0, 8, 0),
                TextAlign = ContentAlignment.MiddleLeft,

                // a phase is a sentence, not a menu entry: an ampersand in it is a character and
                // not the start of a shortcut
                UseMnemonic = false
            };

            Controls.Add(_statusLabel);
        }

        /// <summary>
        /// Opens the window, themed like the rest of the application, and waits for it to have
        /// been painted before returning.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The painting has to happen here and not when the message loop next runs: the caller
        /// blocks the thread it calls this on, and a window which was still waiting to paint
        /// would show nothing at all for the whole pass.
        /// </para>
        /// <para>
        /// Not called <c>Show</c> because <see cref="Form"/> already has one which takes an
        /// owner, and it is the one used below.
        /// </para>
        /// </remarks>
        /// <param name="owner">Window the progress window belongs to.</param>
        /// <param name="windowTitle">Caption, which stays the same for the whole pass.</param>
        internal static UpdateProgressForm Open(IWin32Window owner, string windowTitle)
        {
            var form = new UpdateProgressForm(windowTitle);

            ThemesLoader.ApplyTheme(form);

            try
            {
                form.Show(owner);
                form.Refresh();
            }
            catch
            {
                // the caller only ever sees a window this method returned, so one which could not
                // be shown has to be got rid of here rather than left behind
                form.CloseSafely();
                throw;
            }

            return form;
        }

        /// <summary>
        /// Shows what the pass is doing now.
        /// </summary>
        /// <remarks>
        /// Safe to call from a worker thread, and safe to call once the pass is over: a report
        /// which can not be shown is dropped rather than thrown.
        /// </remarks>
        /// <param name="statusText">Phase the pass has reached, in the language of the interface.</param>
        internal void SetStatus(string statusText)
        {
            if (_isClosed || IsDisposed || Disposing) return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke((Action)(() => SetStatus(statusText)));
                }
                catch (InvalidOperationException)
                {
                    // the handle went between the check above and this call, so there is nothing
                    // left to show the phase on
                }

                return;
            }

            _statusLabel.Text = statusText ?? string.Empty;

            // painted now, because the caller is about to block this thread
            _statusLabel.Refresh();
        }

        /// <summary>
        /// Closes and disposes the window.
        /// </summary>
        /// <remarks>
        /// Meant for the cleanup of the pass, which calls it whether that pass succeeded or
        /// failed, and which is the only thing allowed to close the window. Calling it twice, or
        /// on a window which was never opened, does nothing.
        /// </remarks>
        internal void CloseSafely()
        {
            if (_isClosed) return;

            _isClosed = true;

            try
            {
                Close();
            }
            finally
            {
                // closing a form disposes it, this only covers the case where it could not, so
                // that the handle is released either way
                Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _statusLabel != null)
            {
                // the label would be disposed with the window either way, being one of its
                // children, but the field is owned here and released here
                _statusLabel.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // the application shutting down is not the user closing this window, so it is let
            // through - only a dismissal of the window itself is refused
            if (!_isClosed && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }
    }
}
