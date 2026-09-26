namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// One selectable look of the application.
    /// </summary>
    /// <remarks>
    /// A theme is only a <see cref="Palette"/>. It used to carry a second list of
    /// per control type colours as well, which meant every theme described its look
    /// twice and the two descriptions could disagree. The palette is now the single
    /// source of truth, and <see cref="ThemesLoader"/> decides which control asks
    /// for which role.
    /// </remarks>
    public interface IUITheme
    {
        /// <summary>
        /// Name shown in the theme list and stored in the ini file. Renaming a theme
        /// makes an existing setting fall back to the default one.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The colours of this theme, named after the role a control plays.
        /// </summary>
        ThemePalette Palette { get; }
    }
}
