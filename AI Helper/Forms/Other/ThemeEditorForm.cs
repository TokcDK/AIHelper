using AIHelper.Manage.ui;
using AIHelper.Manage.ui.themes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Changes the colours of the theme in use, one element at a time, and shows the
    /// result while it is being changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The elements are the roles a palette is made of, in the same order the theme file
    /// lists them, so the window and the file read the same way round.
    /// </para>
    /// <para>
    /// Every change is applied to the window at once. What is being changed is a copy of
    /// the palette, so the theme the colours came from is left alone and the edit can be
    /// thrown away; nothing is written to disk until the user saves the colours as a theme
    /// of their own, which is what the Save button beside the theme list does.
    /// </para>
    /// </remarks>
    public partial class ThemeEditorForm : Form
    {
        /// <summary>
        /// The colour roles, in the order they are listed and edited.
        /// </summary>
        private readonly List<PropertyInfo> _roles = new List<PropertyInfo>();

        /// <summary>
        /// The colour each element had when the window opened, which is what resetting
        /// goes back to.
        /// </summary>
        private readonly Dictionary<string, Color> _original = new Dictionary<string, Color>();

        /// <summary>
        /// The palette being edited, which is a copy of the one which was on screen.
        /// </summary>
        private ThemePalette _palette;

        /// <summary>
        /// Set while the window is writing a value into a control itself, so that does not
        /// look like the user having changed it.
        /// </summary>
        private bool _isUpdating;

        public ThemeEditorForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens the editor and returns once it has been closed.
        /// </summary>
        /// <remarks>
        /// Modal, so the theme list underneath cannot be changed while an edit is in
        /// progress. Closing keeps the colours which are on screen; saving them is a
        /// separate step, taken from the theme list afterwards.
        /// </remarks>
        internal static void Edit(IWin32Window owner)
        {
            using (var editor = new ThemeEditorForm())
            {
                editor.ShowDialog(owner);
            }
        }

        /// <summary>
        /// The role the list is showing, or <c>null</c> while nothing is selected.
        /// </summary>
        private PropertyInfo SelectedRole
        {
            get
            {
                var index = RolesOwnColorListBox.SelectedIndex;
                return index < 0 || index >= _roles.Count ? null : _roles[index];
            }
        }

        private void ThemeEditorForm_Load(object sender, EventArgs e)
        {
            SetTranslations();

            _roles.AddRange(ThemePalette.ColourRoles());

            // a copy, so the theme these colours came from is not changed by the edit
            _palette = ThemesLoader.BeginColourEdit();

            foreach (var role in _roles)
            {
                _original[role.Name] = ColourOf(role);
                RolesOwnColorListBox.Items.Add(ElementLabel(role));
            }

            ThemesLoader.ApplyTheme(this);
            ApplyPaletteToSelf();
            SetTooltips();

            // from here on every change repaints the window, and this window follows the
            // palette it is changing
            ThemesLoader.ThemeApplied += ThemesLoader_ThemeApplied;

            if (RolesOwnColorListBox.Items.Count > 0) RolesOwnColorListBox.SelectedIndex = 0;
        }

        private void ThemeEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThemesLoader.ThemeApplied -= ThemesLoader_ThemeApplied;

            // the colours stay on screen, which is the point of the window. The theme they
            // came from is untouched and nothing has been written, so this is not the same
            // as having saved them.
            ThemesLoader.EndColourEdit(true);

            DisposeTooltips();
        }

        /// <summary>
        /// Follows the palette when it changes, so the window shows the colours it is
        /// setting rather than the ones it opened with.
        /// </summary>
        private void ThemesLoader_ThemeApplied(object sender, EventArgs e)
        {
            ThemesLoader.ApplyTheme(this);
            ApplyPaletteToSelf();
        }

        /// <summary>
        /// Colours the parts of this window the theme loader leaves alone.
        /// </summary>
        /// <remarks>
        /// The list of elements draws its own rows, so the loader is told to keep off it by
        /// the mark in its name - which also means its colours and its scroll bar have to be
        /// set here instead.
        /// </remarks>
        private void ApplyPaletteToSelf()
        {
            var palette = _palette ?? ThemesLoader.CurrentPalette;
            if (palette == null) return;

            RolesOwnColorListBox.BackColor = palette.InputFace;
            RolesOwnColorListBox.ForeColor = palette.InputText;
            ThemedScrollBars.Apply(RolesOwnColorListBox, palette.IsDark);

            // the grid draws its own cells, so it is off the loader's walk too and only
            // needs the colour of the lines between them
            ColourOwnColorSwatchGrid.BorderColour = palette.Border;

            RolesOwnColorListBox.Invalidate();
        }

        private void RolesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectedRole();
        }

        /// <summary>
        /// Shows the colour of the selected element, and which of the actions make sense
        /// for it.
        /// </summary>
        private void ShowSelectedRole()
        {
            var role = SelectedRole;

            txtColourHex.Enabled = role != null;
            btnPick.Enabled = role != null;
            btnResetElement.Enabled = role != null && IsChanged(role);

            if (role == null) return;

            var colour = ColourOf(role);

            SetHexText(colour);

            PreviewOwnColorPanel.BackColor = colour;
        }

        private void TxtColourHex_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdating) return;

            var role = SelectedRole;
            if (role == null) return;

            // a half typed value is left alone rather than reported: the field is being
            // typed into, and the colour simply does not change until it makes sense
            if (!CustomThemeStore.TryParseColour(txtColourHex.Text, out var colour)) return;

            // the text is not rewritten here: it is being typed into, and replacing it
            // would move the caret with every character
            SetRoleColour(role, colour, false);
        }

        private void BtnPick_Click(object sender, EventArgs e)
        {
            var role = SelectedRole;
            if (role == null) return;

            var current = ColourOf(role);

            using (var picker = new ColorDialog())
            {
                picker.FullOpen = true;
                picker.Color = current;

                if (picker.ShowDialog(this) != DialogResult.OK) return;

                // the system picker has no way to choose transparency, so the alpha already
                // there is kept rather than being dropped to opaque
                var picked = Color.FromArgb(current.A, picker.Color.R, picker.Color.G, picker.Color.B);

                SetRoleColour(role, picked, true);
            }
        }

        private void BtnResetElement_Click(object sender, EventArgs e)
        {
            var role = SelectedRole;
            if (role == null) return;

            SetRoleColour(role, _original[role.Name], true);
        }

        /// <summary>
        /// Takes the colour of a pixel somewhere else on the screen.
        /// </summary>
        /// <remarks>
        /// A theme is most often matched against something which is already on screen -
        /// another program, a picture, a game's own interface - and neither the system
        /// picker nor the grid beside it can take a colour from there.
        /// </remarks>
        private void BtnFromScreen_Click(object sender, EventArgs e)
        {
            var role = SelectedRole;
            if (role == null) return;

            // this window has to be out of the way, so that what it is covering can be
            // picked from as well. The messages are pumped once so the hiding has taken
            // effect before the screen is read.
            Hide();

            Color picked;
            try
            {
                Application.DoEvents();
                picked = ScreenColourPickerForm.Pick(this);
            }
            finally
            {
                Show();
                Activate();
            }

            if (picked.IsEmpty) return;

            SetRoleColour(role, picked, true);
        }

        /// <summary>
        /// Sets the selected element from one of the ready made colours.
        /// </summary>
        /// <remarks>
        /// The grid holds opaque colours, so the transparency already on the element is
        /// kept. What is being chosen here is the colour; the field above is where every
        /// part of it, transparency included, can be typed.
        /// </remarks>
        private void ColourSwatchGrid_ColourPicked(Color colour)
        {
            var role = SelectedRole;
            if (role == null) return;

            SetRoleColour(role, Color.FromArgb(ColourOf(role).A, colour.R, colour.G, colour.B), true);
        }

        private void BtnResetAll_Click(object sender, EventArgs e)
        {
            foreach (var role in _roles)
            {
                role.SetValue(_palette, _original[role.Name], null);
            }

            ShowSelectedRole();
            RepaintFromEdit();
        }

        /// <summary>
        /// Gives one element a colour and shows the result.
        /// </summary>
        /// <param name="updateText">
        /// Whether to write the value back into the field as well. Not done while the field
        /// is being typed into, because that would move the caret.
        /// </param>
        private void SetRoleColour(PropertyInfo role, Color colour, bool updateText)
        {
            role.SetValue(_palette, colour, null);

            if (updateText) SetHexText(colour);

            PreviewOwnColorPanel.BackColor = colour;
            btnResetElement.Enabled = IsChanged(role);

            RepaintFromEdit();
        }

        /// <summary>
        /// Redraws the list and puts the change on the window.
        /// </summary>
        private void RepaintFromEdit()
        {
            // the rows carry a swatch of the colour, so they are no longer right
            RolesOwnColorListBox.Invalidate();

            ThemesLoader.ApplyColourEdit();
        }

        private void SetHexText(Color colour)
        {
            _isUpdating = true;
            try
            {
                txtColourHex.Text = CustomThemeStore.HexColour(colour);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private Color ColourOf(PropertyInfo role)
        {
            return (Color)role.GetValue(_palette, null);
        }

        /// <summary>
        /// Tells whether an element no longer has the colour the theme gave it.
        /// </summary>
        private bool IsChanged(PropertyInfo role)
        {
            return ColourOf(role).ToArgb() != _original[role.Name].ToArgb();
        }

        /// <summary>
        /// The name of a role as words, so <c>ButtonHover</c> reads as "Button hover".
        /// </summary>
        /// <remarks>
        /// Split rather than listed, so a role added to the palette shows up here with a
        /// name without anything being written for it.
        /// </remarks>
        private static string ElementLabel(PropertyInfo role)
        {
            var name = role.Name;
            var text = new StringBuilder(name.Length + 4);

            for (var i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i])) text.Append(' ');
                text.Append(i == 0 ? char.ToUpperInvariant(name[i]) : name[i]);
            }

            return text.ToString();
        }

        /// <summary>
        /// Draws one row of the element list: the colour as a swatch, then the name.
        /// </summary>
        /// <remarks>
        /// Drawn here rather than by the theme loader, because a row shows a colour rather
        /// than only text. The selection colours still come from the palette, so the list
        /// looks like the rest of the window.
        /// </remarks>
        private void RolesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var palette = _palette ?? ThemesLoader.CurrentPalette;
            if (palette == null) return;

            var bounds = e.Bounds;
            var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            var background = isSelected ? palette.Selection : palette.InputFace;
            var foreground = isSelected ? palette.SelectionText : palette.InputText;

            using (var brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            if (e.Index < 0 || e.Index >= _roles.Count) return;

            var swatch = new Rectangle(bounds.X + 4, bounds.Y + 3, 30, Math.Max(4, bounds.Height - 7));

            using (var brush = new SolidBrush(ColourOf(_roles[e.Index])))
            {
                e.Graphics.FillRectangle(brush, swatch);
            }

            using (var pen = new Pen(palette.Border))
            {
                e.Graphics.DrawRectangle(pen, swatch);
            }

            var textBounds = new Rectangle(
                swatch.Right + 8,
                bounds.Y,
                Math.Max(0, bounds.Right - swatch.Right - 12),
                bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                RolesOwnColorListBox.Items[e.Index].ToString(),
                RolesOwnColorListBox.Font,
                textBounds,
                foreground,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (isSelected && (e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, bounds, foreground, background);
            }
        }

        private void SetTranslations()
        {
            Text = T._("Edit theme colours");

            lblElement.Text = T._("Element");
            lblColour.Text = T._("Colour");
            btnPick.Text = T._("Pick...");
            btnFromScreen.Text = T._("From screen");
            btnResetElement.Text = T._("Reset element");
            btnResetAll.Text = T._("Reset all");
            btnClose.Text = T._("Close");
            lblHintMutedLabel.Text = T._("Changes apply at once. Use Save to keep them as a theme.");
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

            _toolTip.Set(RolesOwnColorListBox, T._("The parts of the window which take a colour from the theme"));
            _toolTip.Set(txtColourHex, T._("The colour as #AARRGGBB: alpha, red, green and blue in hex"));
            _toolTip.Set(ColourOwnColorSwatchGrid, T._("A colour to set the element to with one click"));
            _toolTip.Set(btnPick, T._("Choose the colour with the system colour picker"));
            _toolTip.Set(btnFromScreen, T._("Take the colour of a pixel anywhere on the screen"));
            _toolTip.Set(btnResetElement, T._("Put this element back to the colour the theme gave it"));
            _toolTip.Set(btnResetAll, T._("Put every element back to the colours the theme gave them"));
            _toolTip.Set(lblHintMutedLabel, T._("Saving is a separate step: use Save beside the theme list to keep these colours as a theme"));
        }
    }
}
