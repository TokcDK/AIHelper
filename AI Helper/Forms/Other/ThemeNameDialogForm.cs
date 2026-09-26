using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using System;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Asks for the name of a theme.
    /// </summary>
    /// <remarks>
    /// The same dialog names a theme which is being saved and renames one which already
    /// exists, because both ask the same question; only the title and the text it starts
    /// with differ.
    /// </remarks>
    public partial class ThemeNameDialogForm : Form
    {
        /// <summary>
        /// Returns why a name cannot be used, or <c>null</c> when it can.
        /// </summary>
        /// <remarks>
        /// The check is handed to the dialog rather than done by the caller afterwards, so
        /// a name which is already taken is refused while the dialog is still open, instead
        /// of closing it and having to ask again.
        /// </remarks>
        private Func<string, string> _validate;

        private string _title;

        public ThemeNameDialogForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The name the user entered.
        /// </summary>
        internal string ThemeName
        {
            get { return txtThemeName.Text.Trim(); }
        }

        /// <summary>
        /// Asks for a name, and returns it or <c>null</c> when the user cancelled.
        /// </summary>
        internal static string Ask(IWin32Window owner, string title, string initialName, Func<string, string> validate)
        {
            using (var dialog = new ThemeNameDialogForm())
            {
                dialog.Prepare(title, initialName, validate);

                return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.ThemeName : null;
            }
        }

        /// <summary>
        /// Sets the dialog up for one question.
        /// </summary>
        private void Prepare(string title, string initialName, Func<string, string> validate)
        {
            _title = title;
            _validate = validate;

            Text = title;

            // the longest name the store will keep, so the limit is enforced while typing
            // rather than by silently shortening what was typed
            txtThemeName.MaxLength = CustomThemeStore.MaxNameLength;
            txtThemeName.Text = initialName ?? string.Empty;
        }

        private void ThemeNameDialogForm_Load(object sender, EventArgs e)
        {
            SetTranslations();

            // the dialog is opened from the settings while the program runs, so it takes
            // the theme in use rather than the one which was current when it was created
            ThemesLoader.ApplyTheme(this);

            SetTooltips();

            // the name it starts with is a suggestion, so it is ready to be typed over
            txtThemeName.SelectAll();
            txtThemeName.Focus();
        }

        private void ThemeNameDialogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeTooltips();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var reason = _validate == null ? null : _validate(ThemeName);

            if (reason != null)
            {
                // the dialog stays open so the name can be corrected where it was typed
                MessageBox.Show(this, reason, _title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void SetTranslations()
        {
            lblThemeName.Text = T._("Theme name");
            btnStart.Text = T._("OK");
            btnCancel.Text = T._("Cancel");
        }

        private UiTooltip _toolTip;

        /// <summary>
        /// remove all tooltips and dispose resources
        /// </summary>
        internal void DisposeTooltips()
        {
            if (_toolTip == null) return;

            _toolTip.Dispose();
            _toolTip = null;
        }

        internal void SetTooltips()
        {
            DisposeTooltips();

            _toolTip = new UiTooltip();

            _toolTip.Set(txtThemeName, T._("The name this theme will be shown under in the theme list"));
        }
    }
}
