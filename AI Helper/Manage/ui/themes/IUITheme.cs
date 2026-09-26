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

    /// <summary>
    /// A theme which draws new colours every time it is applied, instead of keeping the
    /// same palette for as long as the application runs.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="IUITheme"/> on purpose. Almost every theme is a fixed
    /// set of colours, and only the random one has anything to redraw, so asking all of
    /// them to implement a redraw they do not have would be noise. The loader only has
    /// to look for this interface to know which themes want a new draw.
    /// </remarks>
    internal interface IRollableTheme
    {
        /// <summary>
        /// Draws a new set of colours, which replaces the palette of this instance.
        /// </summary>
        void Roll();
    }
}
