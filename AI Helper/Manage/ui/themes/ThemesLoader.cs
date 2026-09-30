using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// Paints a window with a <see cref="IUITheme"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The loader walks a control tree once and gives every control the colour of the
    /// role it plays. Roles come from the control type (a button is a button), from
    /// where the control sits (anything inside a control whose name carries the title
    /// bar mark is drawn as title bar chrome) and from the name mark a control
    /// carries (muted, accent, danger, or the opt out mark which leaves a control
    /// alone).
    /// </para>
    /// <para>
    /// Applying a theme is idempotent and safe to repeat. That matters because the
    /// window builds some of its controls while it runs - the tools and folders tab
    /// buttons and the extra settings panels are created on the first visit to their
    /// tab - and those have to be themed after they exist, not before.
    /// </para>
    /// </remarks>
    internal static class ThemesLoader
    {
        /// <summary>
        /// Name of the theme used when nothing is stored yet or the stored name is
        /// no longer a known theme.
        /// </summary>
        private const string DefaultThemeName = "Default";

        private const string DarkThemeName = "Dark";
        private const string CrazyThemeName = "Crazy";

        /// <summary>
        /// Height of the accent strip drawn on top of the selected tab header.
        /// </summary>
        private const int TabAccentStripHeight = 3;

        /// <summary>
        /// Space added around the caption inside a tab header, on top of the padding the
        /// control applies to it itself.
        /// </summary>
        private const int TabExtraWidth = 6;
        private const int TabExtraHeight = 2;

        /// <summary>
        /// Narrowest tab header, so a short caption does not produce a tiny tab.
        /// </summary>
        private const int MinTabWidth = 64;

        /// <summary>
        /// Space the tab strip leaves beside its headers. The window has to be wide
        /// enough for this as well, or the last header is cut off.
        /// </summary>
        private const int TabStripMargin = 6;

        /// <summary>
        /// Used to draw a control while no theme has been applied yet.
        /// </summary>
        private static readonly ThemePalette FallbackPalette = new DefaultTheme().Palette;

        /// <summary>
        /// Set once the theme list has been put into the selector of the main window.
        /// </summary>
        private static bool _isSelectorBound;

        /// <summary>
        /// Set while the selector is being moved to match the current theme, so that
        /// does not look like the user picking a theme.
        /// </summary>
        private static bool _isSyncingSelector;

        /// <summary>
        /// The theme being edited, or <c>null</c> while no editor is open.
        /// </summary>
        /// <remarks>
        /// It is not stored and not in the theme list: a theme of the user's own only exists
        /// once they save it. This is what is on screen in the meantime, which is how an
        /// edit is visible while it is being made.
        /// </remarks>
        private static CustomTheme _colourDraft;

        /// <summary>
        /// The theme which was in use when the edit began, so throwing the edit away has
        /// somewhere to go back to.
        /// </summary>
        private static IUITheme _themeBeforeEdit;

        /// <summary>
        /// Raised after a theme has been applied to the main window.
        /// </summary>
        /// <remarks>
        /// Some parts of the window build their own controls while it runs, and those
        /// controls sometimes have to be rebuilt rather than recoloured - the function
        /// row on the launch page adapts its own colours to the theme, which it can
        /// only do while it is being built. Whoever owns such a part subscribes here
        /// and rebuilds it.
        /// </remarks>
        internal static event EventHandler ThemeApplied;

        /// <summary>
        /// The palette currently in use, or <c>null</c> before the first theme is
        /// applied.
        /// </summary>
        internal static ThemePalette CurrentPalette
        {
            get
            {
                var theme = ManageSettings.CurrentTheme;
                return theme == null ? null : theme.Palette;
            }
        }

        /// <summary>
        /// Every theme the application knows: the compiled ones, then the ones the user
        /// saved.
        /// </summary>
        /// <remarks>
        /// <see cref="CustomTheme"/> is the shape a saved theme is read into, not a theme
        /// of its own, so the discovery is told to leave it out. Without that it would
        /// hand back an empty instance of it and the list would show a nameless entry.
        /// </remarks>
        internal static List<IUITheme> GetList()
        {
            var themes = GetListOfSubClasses.Inherited.GetInterfaceImplimentations<IUITheme>()
                .Where(theme => !(theme is CustomTheme))
                .ToList();

            themes.AddRange(CustomThemeStore.LoadAll());

            return themes;
        }

        /// <summary>
        /// Tells whether a theme is one the user saved, which is what decides whether it
        /// can be renamed and deleted.
        /// </summary>
        internal static bool IsCustomTheme(IUITheme theme)
        {
            return theme is CustomTheme;
        }

        /// <summary>
        /// Keeps the colours the window is showing as a theme of the user's own, and
        /// switches to it.
        /// </summary>
        /// <remarks>
        /// The colours are taken from what is on screen rather than from any theme by
        /// name, which is the point of the feature: the random theme draws a new set every
        /// time it is applied, and this is how a set worth keeping is kept.
        /// </remarks>
        internal static CustomTheme SaveCurrentAsCustomTheme(string name)
        {
            var saved = CustomThemeStore.Save(name, CurrentPalette ?? FallbackPalette);
            if (saved == null) return null;

            // the list is rebuilt from disk, so the instance to use from now on is the
            // one the list holds rather than the one just written
            var themes = ReloadThemeList();

            var stored = themes.FirstOrDefault(theme =>
                theme is CustomTheme && string.Equals(theme.Name, saved.Name, StringComparison.CurrentCultureIgnoreCase));

            var current = stored as CustomTheme ?? saved;

            SwitchToTheme(current);
            RememberThemeName(current);

            return current;
        }

        /// <summary>
        /// Tells whether the colours on screen are an edit which has not been saved.
        /// </summary>
        internal static bool IsEditingColours
        {
            get { return _colourDraft != null; }
        }

        /// <summary>
        /// Starts editing the colours of the theme in use, and returns the palette to edit.
        /// </summary>
        /// <remarks>
        /// The palette handed back is a copy, for two reasons. The theme being edited is
        /// left as it was, so the edit can be thrown away; and when that theme is one of the
        /// compiled ones, changing it would change it for the rest of the run and for
        /// anything else which asks for it.
        /// <para>
        /// The copy is applied under the name of the theme it came from, so the theme list
        /// goes on showing the theme being edited rather than jumping to an entry which does
        /// not exist yet. Nothing is written anywhere until the user saves it as a theme of
        /// their own.
        /// </para>
        /// </remarks>
        internal static ThemePalette BeginColourEdit()
        {
            var current = ManageSettings.CurrentTheme;

            var draft = new CustomTheme
            {
                Name = current == null ? DefaultThemeName : current.Name,
                Palette = CopyPalette(CurrentPalette ?? FallbackPalette),
                FilePath = null
            };

            _themeBeforeEdit = current;
            _colourDraft = draft;

            SwitchToTheme(draft);

            return draft.Palette;
        }

        /// <summary>
        /// Repaints the window from the palette being edited.
        /// </summary>
        /// <remarks>
        /// Called for every change the editor makes, which is what makes the edit live. The
        /// current theme is not moved and the selector is not synced: it already is the
        /// draft, and doing either again for every keystroke would be work with no effect.
        /// </remarks>
        internal static void ApplyColourEdit()
        {
            if (_colourDraft == null) return;

            SetTheme(_colourDraft);
        }

        /// <summary>
        /// Ends an edit, either keeping the colours which are on screen or going back to the
        /// theme which was in use.
        /// </summary>
        /// <param name="keep">
        /// <c>true</c> to leave the edited colours in place, <c>false</c> to go back.
        /// </param>
        /// <remarks>
        /// Keeping them does not save them. They stay on screen, and the theme they came
        /// from is left alone, until the user saves them as a theme of their own; that is
        /// the step which writes a file and gives them a name.
        /// </remarks>
        internal static void EndColourEdit(bool keep)
        {
            var draft = _colourDraft;
            var previous = _themeBeforeEdit;

            _colourDraft = null;
            _themeBeforeEdit = null;

            if (draft == null || keep) return;

            SwitchToTheme(previous ?? FindTheme(DefaultThemeName));
        }

        /// <summary>
        /// A palette which can be changed without changing the one it came from.
        /// </summary>
        /// <remarks>
        /// Copied through the role list rather than property by property, so a role added to
        /// the palette is copied without this being remembered.
        /// </remarks>
        private static ThemePalette CopyPalette(ThemePalette source)
        {
            var copy = new ThemePalette();

            if (source == null) return copy;

            foreach (var role in ThemePalette.ColourRoles())
            {
                role.SetValue(copy, role.GetValue(source, null), null);
            }

            copy.IsDark = source.IsDark;

            return copy;
        }

        /// <summary>
        /// Tells whether a name is already used by a theme, stored or compiled in.
        /// </summary>
        /// <remarks>
        /// The compiled themes count as well as the saved ones. The list is the only way to
        /// choose a theme and the settings file remembers the choice by name, so a saved
        /// theme named after a compiled one would make both the list and the remembered name
        /// ambiguous - and saving one would look like it had done nothing.
        /// </remarks>
        /// <param name="except">
        /// The theme which is allowed to keep the name, which is the one being renamed.
        /// </param>
        internal static bool IsThemeNameTaken(string name, CustomTheme except)
        {
            // the store answers for the saved themes, and turns an empty name down
            if (CustomThemeStore.IsNameTaken(name, except)) return true;

            var candidate = name.Trim();

            return GetList().Any(theme =>
                !(theme is CustomTheme)
                && string.Equals(theme.Name, candidate, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Renames a theme the user saved.
        /// </summary>
        /// <returns><c>false</c> when the theme could not be written.</returns>
        internal static bool RenameCustomTheme(CustomTheme theme, string newName)
        {
            if (theme == null) return false;

            if (!CustomThemeStore.Rename(theme, newName)) return false;

            ReloadThemeList();

            // the settings file remembers the theme by name, so a rename of the theme in
            // use has to be written there too or the next start would fall back
            if (IsCurrent(theme)) RememberThemeName(theme);

            SyncThemeSelector();

            return true;
        }

        /// <summary>
        /// Removes a theme the user saved, falling back to the default one when it was
        /// the theme in use.
        /// </summary>
        internal static bool DeleteCustomTheme(CustomTheme theme)
        {
            if (theme == null) return false;

            var wasCurrent = IsCurrent(theme);

            if (!CustomThemeStore.Delete(theme)) return false;

            ReloadThemeList();

            if (wasCurrent)
            {
                SetDefaultTheme();
                RememberThemeName(ManageSettings.CurrentTheme);
            }
            else
            {
                SyncThemeSelector();
            }

            return true;
        }

        /// <summary>
        /// Tells whether the given theme is the one being shown.
        /// </summary>
        /// <remarks>
        /// Compared by name rather than by instance, because a theme read from disk is a
        /// new instance each time the list is rebuilt.
        /// </remarks>
        private static bool IsCurrent(IUITheme theme)
        {
            var current = ManageSettings.CurrentTheme;

            return current != null
                && theme != null
                && string.Equals(current.Name, theme.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Rebuilds the theme list and puts it back into the selector.
        /// </summary>
        /// <returns>The themes which exist now.</returns>
        private static List<IUITheme> ReloadThemeList()
        {
            var themes = GetList();

            var selector = GetThemeSelector();
            if (selector != null && _isSelectorBound)
            {
                BindThemeSelector(selector, themes);
            }

            return themes;
        }

        /// <summary>
        /// Applies a theme to the main window.
        /// </summary>
        /// <remarks>
        /// A theme which draws its own colours is asked for a new set here, before the
        /// window is walked, so that every control of one pass is painted from the same
        /// palette. Doing it per control instead would leave the window a patchwork of
        /// several draws.
        /// </remarks>
        internal static void SetTheme(IUITheme theme)
        {
            if (theme == null) return;

            var rollable = theme as IRollableTheme;
            if (rollable != null) rollable.Roll();

            SetTheme(theme, ManageSettings.MainForm);

            var handler = ThemeApplied;
            if (handler != null) handler(null, EventArgs.Empty);
        }

        /// <summary>
        /// Applies a theme to a control and everything below it.
        /// </summary>
        internal static void SetTheme(IUITheme theme, Control control)
        {
            if (theme == null || control == null) return;

            var palette = theme.Palette;
            if (palette == null) return;

            ApplyControlTheme(palette, control);

            foreach (Control child in control.Controls)
            {
                SetTheme(theme, child);
            }
        }

        /// <summary>
        /// Applies the theme which is currently selected, if there is one.
        /// </summary>
        /// <remarks>
        /// Meant for a control which was created after the window was themed, such as
        /// a panel the user only sees on the second visit to a tab.
        /// </remarks>
        internal static void ApplyTheme(Control control)
        {
            SetTheme(ManageSettings.CurrentTheme, control);
        }

        /// <summary>
        /// Picks a theme at random from the known ones.
        /// </summary>
        internal static void SetRandomTheme()
        {
            var themes = GetList();
            if (themes.Count == 0) return;

            //Random.Next(min, max) is exclusive of max, so cnt - 1 excluded the last theme
            SwitchToTheme(themes[new Random().Next(0, themes.Count)]);
        }

        /// <summary>
        /// Switches back to the default theme.
        /// </summary>
        internal static void SetDefaultTheme()
        {
            SwitchToTheme(FindTheme(DefaultThemeName));
        }

        /// <summary>
        /// Switches to the dark theme.
        /// </summary>
        internal static void SetDarkTheme()
        {
            SwitchToTheme(FindTheme(DarkThemeName));
        }

        /// <summary>
        /// Switches to the random colour theme.
        /// </summary>
        /// <remarks>
        /// Asking for the theme is enough to get new colours: it draws them every time
        /// it is applied, so every pass over the window - picking it in the list, and
        /// every game change after that - comes out different.
        /// </remarks>
        internal static void SetRandomColors()
        {
            SwitchToTheme(FindTheme(CrazyThemeName));
        }

        /// <summary>
        /// Makes <paramref name="theme"/> the current one, shows it in the selector
        /// and applies it.
        /// </summary>
        private static void SwitchToTheme(IUITheme theme)
        {
            if (theme == null) return;

            ManageSettings.CurrentTheme = theme;

            SyncThemeSelector();

            SetTheme(theme);
        }

        /// <summary>
        /// The known theme with the given name, or <c>null</c> when there is none.
        /// </summary>
        private static IUITheme FindTheme(string name)
        {
            return GetList().FirstOrDefault(t => t.Name == name);
        }

        /// <summary>
        /// Applies the theme stored in the ini file, or the default one, and puts the
        /// theme list into the selector of the main window.
        /// </summary>
        internal static void SetTheme()
        {
            var themes = GetList();
            if (themes.Count == 0) return;

            if (!_isSelectorBound)
            {
                _isSelectorBound = true;

                var selector = GetThemeSelector();
                if (selector != null)
                {
                    BindThemeSelector(selector, themes);
                    ReadStoredTheme(themes);
                }
            }

            // applied unconditionally: the theme may already be the right one, but the
            // controls it has to reach may have been created since the last pass
            SetTheme(ManageSettings.CurrentTheme ?? GetDefaultTheme(themes));

            // after the theme, not before: theming the selector makes it flat and owner
            // drawn, which recreates its handle the first time, and a data bound drop
            // down which has its handle recreated loses the selection it was showing
            SyncThemeSelector();
        }

        /// <summary>
        /// The theme selector of the main window, or <c>null</c> while the main window
        /// is not there yet.
        /// </summary>
        private static ComboBox GetThemeSelector()
        {
            var mainForm = ManageSettings.MainForm;
            return mainForm == null ? null : mainForm.SelectThemeComboBox;
        }

        private static IUITheme GetDefaultTheme(List<IUITheme> themes)
        {
            var fallback = themes.FirstOrDefault(t => t.Name == DefaultThemeName);
            return fallback ?? themes[0];
        }

        /// <summary>
        /// Fills the selector with the known themes and wires it to apply the chosen
        /// one.
        /// </summary>
        /// <remarks>
        /// Safe to call again after the list has changed. The handler is detached before
        /// the data source is replaced and attached again afterwards, because replacing
        /// the source moves the selection and would otherwise look like the user choosing
        /// a theme, and because subscribing twice would apply every theme twice.
        /// </remarks>
        private static void BindThemeSelector(ComboBox selector, List<IUITheme> themes)
        {
            selector.SelectedIndexChanged -= OnThemeSelected;
            selector.DataSource = null;
            selector.DataSource = themes;
            selector.DisplayMember = "Name";
            selector.ValueMember = "Name";
            selector.SelectedIndexChanged += OnThemeSelected;
        }

        /// <summary>
        /// Takes the theme the user picked, remembers it and applies it.
        /// </summary>
        private static void OnThemeSelected(object sender, EventArgs e)
        {
            if (_isSyncingSelector) return;

            var selector = sender as ComboBox;
            var theme = selector == null ? null : selector.SelectedItem as IUITheme;
            if (theme == null) return;

            ManageSettings.CurrentTheme = theme;

            RememberThemeName(theme);

            SetTheme(theme);
        }

        /// <summary>
        /// Writes the name of a theme into the settings file, so the same one is applied
        /// the next time the application starts.
        /// </summary>
        private static void RememberThemeName(IUITheme theme)
        {
            if (theme == null) return;

            ManageIni.GetINIFile(ManageSettings.AiHelperIniPath)
                .SetKey(ManageSettings.IniSettingsSectionName, ManageSettings.IniThemeKeyName, theme.Name);
        }

        /// <summary>
        /// Restores the theme the user picked last time, falling back to the default
        /// one when that theme no longer exists.
        /// </summary>
        private static void ReadStoredTheme(List<IUITheme> themes)
        {
            ManageSettings.CurrentTheme = GetDefaultTheme(themes);

            var ini = ManageIni.GetINIFile(ManageSettings.AiHelperIniPath);
            if (!ini.KeyExists(ManageSettings.IniThemeKeyName, ManageSettings.IniSettingsSectionName)) return;

            var storedName = ini.GetKey(ManageSettings.IniSettingsSectionName, ManageSettings.IniThemeKeyName);
            var stored = themes.FirstOrDefault(t => t.Name == storedName);

            if (stored != null) ManageSettings.CurrentTheme = stored;
        }

        /// <summary>
        /// Shows the current theme in the selector.
        /// </summary>
        /// <remarks>
        /// The current theme is not always the very instance the selector was filled
        /// with - the random theme builds a new one every time it is used - so the
        /// entry is looked up by name. The selection event is muted while this runs,
        /// because moving the selector is not the user choosing a theme and would
        /// otherwise apply a different instance of it.
        /// </remarks>
        private static void SyncThemeSelector()
        {
            var selector = GetThemeSelector();
            var current = ManageSettings.CurrentTheme;
            if (selector == null || current == null) return;

            for (var i = 0; i < selector.Items.Count; i++)
            {
                var candidate = selector.Items[i] as IUITheme;
                if (candidate == null || candidate.Name != current.Name) continue;

                if (selector.SelectedIndex == i) return;

                _isSyncingSelector = true;
                try
                {
                    selector.SelectedIndex = i;
                }
                finally
                {
                    _isSyncingSelector = false;
                }

                return;
            }
        }

        /// <summary>
        /// Gives one control the colours of its role.
        /// </summary>
        private static void ApplyControlTheme(ThemePalette palette, Control control)
        {
            if (control.Name.IndexOf(ManageSettings.ThemeLabelColorSetIgnoreNameMark, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // the control keeps the colour it was given on purpose
                return;
            }

            var isOnTitleBar = IsOnTitleBar(control);
            var isMuted = HasMark(control, ManageSettings.ThemeMutedNameMark);
            var isAccent = HasMark(control, ManageSettings.ThemeAccentNameMark);
            var isDanger = HasMark(control, ManageSettings.ThemeDangerNameMark);
            var isAppTitle = HasMark(control, ManageSettings.ThemeAppTitleNameMark);
            var isGameTitle = HasMark(control, ManageSettings.ThemeGameTitleNameMark);

            // a scroll bar is drawn by Windows and ignores the colours it is given, so
            // it has to be asked for a visual style of its own instead
            if (CanHaveScrollBars(control))
            {
                ThemedScrollBars.Apply(control, palette.IsDark);
            }

            // the colour text takes on the surface it sits on. Each family of control
            // has a role of its own for it, and a mark on the control overrides that.
            var textColor = MarkedText(palette, isOnTitleBar, isMuted, isAccent, isAppTitle, isGameTitle,
                isOnTitleBar ? palette.TitleBarText : palette.Text);
            var groupText = MarkedText(palette, isOnTitleBar, isMuted, isAccent, isAppTitle, isGameTitle, palette.GroupText);
            var checkText = MarkedText(palette, isOnTitleBar, isMuted, isAccent, isAppTitle, isGameTitle, palette.CheckText);

            var form = control as Form;
            if (form != null)
            {
                ApplyForm(palette, form);
                return;
            }

            var tabControl = control as TabControl;
            if (tabControl != null)
            {
                ApplyTabControl(palette, tabControl);
                return;
            }

            var tabPage = control as TabPage;
            if (tabPage != null)
            {
                ApplyTabPage(palette, tabPage);
                return;
            }

            var groupBox = control as GroupBox;
            if (groupBox != null)
            {
                ApplyGroupBox(palette, groupBox, groupText);
                return;
            }

            var button = control as Button;
            if (button != null)
            {
                ApplyButton(palette, button, isOnTitleBar, isAccent, isDanger, IsPrimaryButton(button));
                return;
            }

            var checkBox = control as CheckBox;
            if (checkBox != null)
            {
                ApplyCheckBox(palette, checkBox, checkText);
                return;
            }

            var radioButton = control as RadioButton;
            if (radioButton != null)
            {
                radioButton.ForeColor = textColor;
                return;
            }

            var linkLabel = control as LinkLabel;
            if (linkLabel != null)
            {
                ApplyLinkLabel(palette, linkLabel, textColor, isAccent);
                return;
            }

            var label = control as Label;
            if (label != null)
            {
                label.ForeColor = textColor;
                return;
            }

            var comboBox = control as ComboBox;
            if (comboBox != null)
            {
                ApplyComboBox(palette, comboBox);
                return;
            }

            var listBox = control as ListBox;
            if (listBox != null)
            {
                ApplyListBox(palette, listBox);
                return;
            }

            var listView = control as ListView;
            if (listView != null)
            {
                listView.BackColor = palette.InputFace;
                listView.ForeColor = palette.InputText;
                listView.BorderStyle = BorderStyle.FixedSingle;
                return;
            }

            var textBox = control as TextBoxBase;
            if (textBox != null)
            {
                ApplyTextBox(palette, textBox);
                return;
            }

            var progressBar = control as ProgressBar;
            if (progressBar != null)
            {
                ApplyProgressBar(palette, progressBar);
                return;
            }

            var pictureBox = control as PictureBox;
            if (pictureBox != null)
            {
                // a picture is drawn over whatever its parent painted, so it must not put
                // a face of its own behind an image which has transparent parts
                pictureBox.BackColor = Color.Transparent;
                return;
            }

            var toolStrip = control as ToolStrip;
            if (toolStrip != null)
            {
                ThemedToolStrip.Apply(toolStrip, palette);
                return;
            }

            var panel = control as Panel;
            if (panel != null)
            {
                ApplyContainer(palette, panel, isOnTitleBar, textColor);
                return;
            }

            var userControl = control as UserControl;
            if (userControl != null)
            {
                ApplyContainer(palette, userControl, isOnTitleBar, textColor);
                return;
            }
        }

        /// <summary>
        /// The colour text takes on a surface: the colour a mark on the control asks
        /// for, and otherwise the role that control family gives its text.
        /// </summary>
        /// <remarks>
        /// The marks are tested most specific first. A control can carry more than one
        /// of them by accident - the launch page title is named for the accent it used
        /// to be drawn in as well as for what it is - and the one that says what the
        /// control <em>is</em> has to win over the one that only said how it looked.
        /// </remarks>
        private static Color MarkedText(ThemePalette palette, bool isOnTitleBar, bool isMuted, bool isAccent,
            bool isAppTitle, bool isGameTitle, Color role)
        {
            if (isAppTitle) return palette.LaunchTitleText;
            if (isGameTitle) return palette.LaunchGameText;
            if (isAccent) return palette.Accent;
            if (isMuted) return isOnTitleBar ? palette.TitleBarMuted : palette.TextMuted;

            return role;
        }

        /// <summary>
        /// Colours a group box.
        /// </summary>
        /// <remarks>
        /// A group box draws its own frame, in a system colour which no property
        /// reaches, so the frame is drawn by the control instead - see
        /// <see cref="ThemedGroupBox"/>. Its face and caption are set here either way,
        /// so a group box nobody re-registered still follows the theme, only without a
        /// frame colour of its own.
        /// </remarks>
        private static void ApplyGroupBox(ThemePalette palette, GroupBox groupBox, Color textColor)
        {
            groupBox.BackColor = palette.GroupFace;
            groupBox.ForeColor = textColor;

            var themed = groupBox as ThemedGroupBox;
            if (themed != null)
            {
                themed.BorderColour = palette.GroupBorder;
            }
        }

        /// <summary>
        /// Colours a check box.
        /// </summary>
        /// <remarks>
        /// A check box draws the box beside its caption with the system colours and
        /// ignores the background and the border colour it is offered, so no theme could
        /// reach it. A <see cref="ThemedCheckBox"/> draws the box itself and is told the
        /// three colours here. A plain check box is still drawn flat, and given its
        /// caption colour, so a control nobody re-registered is not left unthemed.
        /// </remarks>
        private static void ApplyCheckBox(ThemePalette palette, CheckBox checkBox, Color textColor)
        {
            // flat is what makes the box a shape rather than a picture of the system
            // theme, and it is the shape the themed box is drawn over
            checkBox.FlatStyle = FlatStyle.Flat;
            checkBox.ForeColor = textColor;

            var themed = checkBox as ThemedCheckBox;
            if (themed == null) return;

            themed.GlyphFace = palette.CheckFace;
            themed.GlyphBorder = palette.CheckBorder;
            themed.GlyphMark = palette.CheckMark;
        }

        /// <summary>
        /// Colours a container.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A container which is the body of a window or of a tab paints the surface
        /// colour. Every container inside one is left transparent instead, so it shows
        /// what its parent painted. That is what keeps a table of settings inside a
        /// group box from punching a differently coloured hole in it.
        /// </para>
        /// <para>
        /// A user control is a container as well and is treated the same way. It used to
        /// be missing from the type switch altogether, so a composite control kept the
        /// face its designer gave it while the labels inside it were themed. That is what
        /// left the cards of the updater window white in a dark theme, with their captions
        /// drawn in the theme's light text colour on top of that white - which is why the
        /// current version and the name of the target could not be read.
        /// </para>
        /// </remarks>
        private static void ApplyContainer(ThemePalette palette, Control container, bool isOnTitleBar, Color textColor)
        {
            container.ForeColor = textColor;

            if (isOnTitleBar)
            {
                container.BackColor = palette.TitleBar;
                return;
            }

            var isWindowBody = container.Parent is Form || container.Parent is TabPage;
            container.BackColor = isWindowBody ? palette.Surface : Color.Transparent;
        }

        /// <summary>
        /// Colours the window itself.
        /// </summary>
        /// <remarks>
        /// A window without a system border draws its own one by leaving a one pixel
        /// margin around its content, so its own background has to be the border
        /// colour. A window with a system border has no such margin and gets the
        /// normal window colour instead.
        /// </remarks>
        private static void ApplyForm(ThemePalette palette, Form form)
        {
            form.BackColor = form.FormBorderStyle == FormBorderStyle.None ? palette.Border : palette.Window;
            form.ForeColor = palette.Text;

            // fonts are inherited, so setting it here reaches every control below
            if (form.Font.Name != ThemeFonts.Family || form.Font.Size != ThemeFonts.BodySize)
            {
                form.Font = ThemeFonts.Body;
            }
        }

        private static void ApplyTabPage(ThemePalette palette, TabPage tabPage)
        {
            // a tab page which keeps using the system visual style ignores its own
            // background colour, which would leave the theme half applied
            tabPage.UseVisualStyleBackColor = false;

            tabPage.BackColor = palette.Surface;
            tabPage.ForeColor = palette.Text;
        }

        /// <summary>
        /// Colours a tab strip and switches it to owner drawing.
        /// </summary>
        /// <remarks>
        /// A tab strip drawn by the system ignores the theme completely, so the headers
        /// and the strip behind them are drawn by <see cref="ThemedTabControl"/> instead:
        /// flat, with a frame around each header and an accent strip on the selected one.
        /// The drawing reads the palette when it paints rather than capturing it, so
        /// switching themes repaints the strip correctly.
        /// </remarks>
        private static void ApplyTabControl(ThemePalette palette, TabControl tabControl)
        {
            tabControl.BackColor = palette.TabFace;
            tabControl.ForeColor = palette.TabText;

            // owner drawing is kept because that is what fixes the size of a header;
            // the headers themselves are painted by the strip, which needs the whole
            // header rather than the rectangle an owner draw is handed
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.ItemSize = MeasureTabHeader(tabControl);

            // the area behind the headers is painted by the control itself and ignores
            // BackColor, so a control which can paint it is told which colour to use
            var themed = tabControl as ThemedTabControl;
            if (themed != null)
            {
                themed.StripColor = palette.TabFace;
            }

            // the event is a member of the control, so it survives re-applying the
            // theme; removing it first keeps a single subscription
            tabControl.DrawItem -= DrawTabHeader;
            tabControl.DrawItem += DrawTabHeader;
        }

        /// <summary>
        /// Width the tab strip needs for its headers.
        /// </summary>
        /// <remarks>
        /// Every header is as wide as the longest caption, so the strip is as wide as
        /// all of them put together. The window uses this to stay wide enough: a longer
        /// caption - a different language, for instance - would otherwise push the last
        /// header past the edge of the window, and a tab strip does not scroll.
        /// </remarks>
        internal static int MeasureTabStripWidth(TabControl tabControl)
        {
            if (tabControl == null || tabControl.TabCount == 0) return 0;

            return tabControl.ItemSize.Width * tabControl.TabCount
                 + tabControl.Padding.X * 2
                 + TabStripMargin;
        }

        /// <summary>
        /// Size of one tab header, wide enough for the longest caption of the strip.
        /// </summary>
        /// <remarks>
        /// The control pads the caption inside its header itself, and clips the caption
        /// rather than growing the header when that padding does not fit. The header is
        /// therefore measured from the padding the control reports, not from a constant
        /// here: the two have to agree, and the one which draws is the control.
        /// </remarks>
        private static Size MeasureTabHeader(TabControl tabControl)
        {
            var font = tabControl.Font;
            var padding = tabControl.Padding;

            var width = MinTabWidth;
            foreach (TabPage page in tabControl.TabPages)
            {
                var captionWidth = TextRenderer.MeasureText(page.Text, font).Width
                                 + padding.X * 2
                                 + TabExtraWidth;

                if (captionWidth > width) width = captionWidth;
            }

            return new Size(width, font.Height + padding.Y * 2 + TabExtraHeight);
        }

        /// <summary>
        /// Paints one tab header: its face, its frame, its caption and, on the selected
        /// one, the accent bar which marks it as selected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The whole header is painted, including the two pixel frame the control draws
        /// around it. That frame is a system colour with no property behind it, which is
        /// what the light outline around a tab was; painting the header in full is what
        /// makes it the theme's.
        /// </para>
        /// <para>
        /// The rectangle comes from the control rather than from the bounds an owner draw
        /// is handed. The two differ - the owner draw rectangle is inset by that same
        /// frame - and the strip needs the whole header, so it asks for it and the owner
        /// drawing uses the same answer, which is also what keeps the caption where it
        /// was.
        /// </para>
        /// </remarks>
        internal static void PaintTabHeader(Graphics graphics, TabControl tabControl, int index)
        {
            if (graphics == null || tabControl == null) return;
            if (index < 0 || index >= tabControl.TabPages.Count) return;

            var palette = CurrentPalette ?? FallbackPalette;
            var isSelected = tabControl.SelectedIndex == index;
            var bounds = tabControl.GetTabRect(index);

            using (var background = new SolidBrush(isSelected ? palette.TabSelectedFace : palette.TabFace))
            {
                graphics.FillRectangle(background, bounds);
            }

            using (var border = new Pen(palette.TabBorder))
            {
                DrawTabFrame(graphics, border, bounds, isSelected);
            }

            if (isSelected)
            {
                using (var accent = new SolidBrush(palette.Accent))
                {
                    graphics.FillRectangle(accent, new Rectangle(bounds.X, bounds.Y, bounds.Width, TabAccentStripHeight));
                }
            }

            TextRenderer.DrawText(
                graphics,
                tabControl.TabPages[index].Text,
                tabControl.Font,
                bounds,
                isSelected ? palette.TabSelectedText : palette.TabText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// Draws the outline of one header.
        /// </summary>
        /// <remarks>
        /// The selected header is left open at the bottom, so it joins the page under it
        /// instead of being cut off from it - which is how a tab strip shows which page is
        /// the one on screen. Its top edge is drawn as well even though the accent bar
        /// covers it, so the outline of a header does not depend on whether it is
        /// selected.
        /// </remarks>
        private static void DrawTabFrame(Graphics graphics, Pen border, Rectangle bounds, bool isSelected)
        {
            var left = bounds.Left;
            var top = bounds.Top;
            var right = bounds.Right - 1;
            var bottom = bounds.Bottom - 1;

            graphics.DrawLine(border, left, top, right, top);
            graphics.DrawLine(border, left, top, left, bottom);
            graphics.DrawLine(border, right, top, right, bottom);

            if (!isSelected) graphics.DrawLine(border, left, bottom, right, bottom);
        }

        /// <summary>
        /// Paints one header for the control's own owner drawing.
        /// </summary>
        /// <remarks>
        /// The strip paints its own headers, so this is only reached if something draws
        /// the control through the native path. It stays because the control is kept in
        /// owner draw mode - that is what fixes the header size - and a mode with no
        /// handler would leave those headers blank.
        /// </remarks>
        private static void DrawTabHeader(object sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null) return;

            PaintTabHeader(e.Graphics, tabControl, e.Index);
        }

        /// <summary>
        /// Colours a button, including the shades it uses under the pointer and while
        /// it is pressed.
        /// </summary>
        /// <remarks>
        /// The shades go through <see cref="FlatAppearance"/> rather than through
        /// event handlers, so a themed button needs no code of its own and keeps
        /// working when the theme changes.
        /// </remarks>
        private static void ApplyButton(ThemePalette palette, Button button, bool isOnTitleBar, bool isAccent, bool isDanger, bool isPrimary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;

            var appearance = button.FlatAppearance;

            if (isOnTitleBar)
            {
                // window chrome: no outline, and the close button warns in red
                button.BackColor = palette.TitleBar;
                button.ForeColor = palette.TitleBarText;

                appearance.BorderSize = 0;

                var hoverColor = isDanger || IsCloseButton(button)
                    ? palette.Danger
                    : ThemePalette.Blend(palette.TitleBar, palette.IsDark ? Color.White : Color.Black, 0.22f);

                appearance.MouseOverBackColor = hoverColor;
                appearance.MouseDownBackColor = ThemePalette.Blend(hoverColor, Color.Black, 0.18f);
                return;
            }

            if (isAccent || isDanger || isPrimary)
            {
                var face = isDanger ? palette.Danger : palette.Accent;

                button.BackColor = face;
                button.ForeColor = isDanger ? palette.DangerText : palette.AccentText;

                appearance.BorderSize = 0;
                appearance.MouseOverBackColor = isDanger ? palette.DangerHover : palette.AccentHover;
                appearance.MouseDownBackColor = ThemePalette.Blend(face, Color.Black, 0.18f);
                return;
            }

            button.BackColor = palette.ButtonFace;
            button.ForeColor = palette.ButtonText;

            appearance.BorderSize = 1;
            appearance.BorderColor = palette.ButtonBorder;
            appearance.MouseOverBackColor = palette.ButtonHover;
            appearance.MouseDownBackColor = palette.ButtonPressed;
        }

        /// <summary>
        /// Tells whether a button is the action the user is meant to end a dialog
        /// with, which is the button drawn in the accent colour.
        /// </summary>
        /// <remarks>
        /// A dialog says which button that is: the one it accepts on, or the one
        /// which ends it with <see cref="DialogResult.OK"/>. A window without either
        /// - the main window and the install progress window - has no primary action,
        /// and every button in it keeps the normal face.
        /// </remarks>
        private static bool IsPrimaryButton(Button button)
        {
            if (button.DialogResult == DialogResult.OK) return true;

            var form = button.FindForm();
            return form != null && ReferenceEquals(form.AcceptButton, button);
        }

        /// <summary>
        /// Tells whether a button on the title bar is the one which closes the window.
        /// </summary>
        /// <remarks>
        /// The window chrome is the one place where the meaning of a button comes from
        /// its name rather than from a role, because the close button is created by the
        /// designer before a theme exists to mark it.
        /// </remarks>
        private static bool IsCloseButton(Button button)
        {
            return button.Name.IndexOf("Close", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ApplyLinkLabel(ThemePalette palette, LinkLabel linkLabel, Color textColor, bool isAccent)
        {
            linkLabel.ForeColor = textColor;
            linkLabel.LinkColor = isAccent ? palette.Accent : palette.Link;
            linkLabel.ActiveLinkColor = palette.LinkHover;
            linkLabel.VisitedLinkColor = palette.LinkVisited;
            linkLabel.DisabledLinkColor = palette.TextMuted;
        }

        /// <summary>
        /// Colours a text box as an editor, or as a read only display.
        /// </summary>
        /// <remarks>
        /// A read only box is showing text rather than accepting it, so it is drawn
        /// on the surface colour instead of the input colour. That keeps the log
        /// window and the message boxes from looking editable. Every box keeps a
        /// single line border, because a read only box sits on the same colour as the
        /// panel around it in the light theme and would otherwise have no edge - and
        /// that border is the one an edit control draws in its own colour, so a box
        /// which can paint it is told which colour to use.
        /// </remarks>
        private static void ApplyTextBox(ThemePalette palette, TextBoxBase textBox)
        {
            if (textBox.ReadOnly)
            {
                textBox.BackColor = palette.SurfaceAlt;
                textBox.ForeColor = palette.Text;
            }
            else
            {
                textBox.BackColor = palette.InputFace;
                textBox.ForeColor = palette.InputText;
            }

            var bordered = textBox as TextBox;
            if (bordered != null)
            {
                bordered.BorderStyle = BorderStyle.FixedSingle;
            }

            var themed = textBox as ThemedTextBox;
            if (themed != null)
            {
                themed.BorderColour = palette.InputBorder;
            }
        }

        /// <summary>
        /// Colours a drop down.
        /// </summary>
        /// <remarks>
        /// A drop down list is drawn by Windows, which ignores both the background and
        /// the text colour it is given, so the closed drop down and the entries it drops
        /// stayed on the system colours in every theme. Drawing it flat makes WinForms
        /// paint the closed drop down itself, and owner drawing the entries does the
        /// same for the list. The one pixel outline a flat drop down draws for itself is
        /// still the system colour, because a drop down does not expose the outline the
        /// way a button does; a <see cref="ThemedComboBox"/> paints over it, and a plain
        /// drop down keeps it.
        /// </remarks>
        private static void ApplyComboBox(ThemePalette palette, ComboBox comboBox)
        {
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = palette.InputFace;
            comboBox.ForeColor = palette.InputText;

            comboBox.DrawMode = DrawMode.OwnerDrawFixed;

            var themed = comboBox as ThemedComboBox;
            if (themed != null)
            {
                themed.BorderColour = palette.InputBorder;
            }

            // as with the tab strip: the handler reads the palette when it paints, and
            // removing it first keeps a single subscription
            comboBox.DrawItem -= DrawComboBoxItem;
            comboBox.DrawItem += DrawComboBoxItem;
        }

        /// <summary>
        /// Draws one entry of a drop down list.
        /// </summary>
        private static void DrawComboBoxItem(object sender, DrawItemEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            var palette = CurrentPalette ?? FallbackPalette;
            var bounds = e.Bounds;

            var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var background = new SolidBrush(isSelected ? palette.Selection : palette.InputFace))
            {
                e.Graphics.FillRectangle(background, bounds);
            }

            if (e.Index < 0 || e.Index >= comboBox.Items.Count) return;

            TextRenderer.DrawText(
                e.Graphics,
                comboBox.GetItemText(comboBox.Items[e.Index]),
                comboBox.Font,
                bounds,
                isSelected ? palette.SelectionText : palette.InputText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>
        /// Colours a list.
        /// </summary>
        /// <remarks>
        /// The rows are drawn here rather than by Windows. A list takes the highlight it
        /// puts behind the selected row from the system and offers no property to change
        /// it, so the selection colour of the theme never appeared: the background
        /// followed the theme and the selected row stayed the system blue. It is the same
        /// thing that happens to a drop down, and it has the same answer.
        /// </remarks>
        private static void ApplyListBox(ThemePalette palette, ListBox listBox)
        {
            listBox.BackColor = palette.InputFace;
            listBox.ForeColor = palette.InputText;
            listBox.BorderStyle = BorderStyle.FixedSingle;

            // a checked list draws its own check boxes, and taking its rows over would
            // mean drawing those as well. Its colours are still worth setting, so it
            // stops here rather than being skipped.
            if (listBox is CheckedListBox) return;

            // owner drawing replaces the row height with the fixed one, so the height the
            // list already used is kept and the rows do not change size
            var itemHeight = listBox.ItemHeight;

            listBox.DrawMode = DrawMode.OwnerDrawFixed;

            if (itemHeight > 0 && listBox.ItemHeight != itemHeight)
            {
                listBox.ItemHeight = itemHeight;
            }

            // as with the tab strip and the drop down: the handler reads the palette when
            // it paints, and removing it first keeps a single subscription
            listBox.DrawItem -= DrawListBoxItem;
            listBox.DrawItem += DrawListBoxItem;
        }

        /// <summary>
        /// Draws one row of a list.
        /// </summary>
        private static void DrawListBoxItem(object sender, DrawItemEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null) return;

            var palette = CurrentPalette ?? FallbackPalette;
            var bounds = e.Bounds;

            var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var isFocused = (e.State & DrawItemState.Focus) == DrawItemState.Focus;

            var background = isSelected ? palette.Selection : palette.InputFace;
            var foreground = isSelected ? palette.SelectionText : palette.InputText;

            using (var brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            if (e.Index < 0 || e.Index >= listBox.Items.Count)
            {
                // an empty row is still filled, so the list does not show the system
                // colour in the gap below the last entry
                return;
            }

            TextRenderer.DrawText(
                e.Graphics,
                listBox.GetItemText(listBox.Items[e.Index]),
                listBox.Font,
                bounds,
                foreground,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // the keyboard still has to show where it is, so the focus mark is kept and
            // drawn in the theme's own colours rather than the system ones
            if (isSelected && isFocused)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, bounds, foreground, background);
            }
        }

        /// <summary>
        /// Tells whether a control can show a scroll bar of its own.
        /// </summary>
        /// <remarks>
        /// Only the controls which can are asked for a themed scroll bar: the call
        /// changes how the whole window is drawn, so handing it to a control which never
        /// shows one would be asking for a change nobody can see.
        /// </remarks>
        private static bool CanHaveScrollBars(Control control)
        {
            return control is ScrollableControl
                || control is TextBoxBase
                || control is ListBox
                || control is ListView
                || control is TreeView
                || control is ComboBox
                || control is ScrollBar;
        }

        /// <summary>
        /// Colours a progress bar.
        /// </summary>
        /// <remarks>
        /// A progress bar with visual styles on is drawn by Windows and ignores the
        /// colours it is given, so the bar stayed the system green in every theme.
        /// Turning the visual style off for this one control makes Windows draw it the
        /// classic way instead, and the classic way is the one which honours a bar
        /// colour and a trough colour.
        /// </remarks>
        private static void ApplyProgressBar(ThemePalette palette, ProgressBar progressBar)
        {
            progressBar.ForeColor = palette.ProgressBar;
            progressBar.BackColor = palette.ProgressBarTrack;

            // the colours only take effect once the visual style is off, and the style can
            // only be turned off once the control has a handle
            progressBar.HandleCreated -= OnProgressBarHandleCreated;
            progressBar.HandleCreated += OnProgressBarHandleCreated;

            if (progressBar.IsHandleCreated)
            {
                DisableVisualStyle(progressBar);
            }
        }

        private static void OnProgressBarHandleCreated(object sender, EventArgs e)
        {
            var progressBar = sender as ProgressBar;
            if (progressBar == null) return;

            DisableVisualStyle(progressBar);
        }

        /// <summary>
        /// Asks Windows to draw a control the classic way rather than with its visual
        /// style. An empty style name is how a style is removed.
        /// </summary>
        private static void DisableVisualStyle(Control control)
        {
            NativeMethods.SetWindowTheme(control.Handle, string.Empty, string.Empty);
        }

        /// <summary>
        /// Tells whether a control sits inside the title bar of its window, or is the
        /// title bar itself.
        /// </summary>
        /// <remarks>
        /// The title bar is a container of its own, so it carries the mark and its
        /// children find the mark on it. The control itself has to be tested too,
        /// otherwise the bar would be drawn as an ordinary surface with chrome
        /// coloured controls sitting on it.
        /// </remarks>
        private static bool IsOnTitleBar(Control control)
        {
            if (HasMark(control, ManageSettings.ThemeTitleBarNameMark)) return true;

            for (var parent = control.Parent; parent != null; parent = parent.Parent)
            {
                if (HasMark(parent, ManageSettings.ThemeTitleBarNameMark)) return true;
            }

            return false;
        }

        private static bool HasMark(Control control, string mark)
        {
            return control.Name.IndexOf(mark, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
