using System;
using System.Globalization;

namespace AIHelper.Manage.ui.themes
{
    /// <summary>
    /// A theme the user kept from the colours they were looking at.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every other theme is a compiled class, so its colours are settled by the code
    /// which declares it. This one is built while the application runs and its colours
    /// come from a file, which is what lets a set of random colours the user liked be
    /// kept and chosen again later.
    /// </para>
    /// <para>
    /// <see cref="Name"/> is settable, unlike the one on <see cref="IUITheme"/>, because
    /// the name of a theme the user made can be changed after it has been made. The
    /// interface only promises that a name can be read.
    /// </para>
    /// </remarks>
    internal class CustomTheme : IUITheme
    {
        /// <summary>
        /// The file this theme is stored in, or <c>null</c> for one which has not been
        /// written yet.
        /// </summary>
        /// <remarks>
        /// Renaming writes the new name into the file rather than moving it, so this
        /// stays right for as long as the theme exists.
        /// </remarks>
        internal string FilePath { get; set; }

        public string Name { get; set; }

        public ThemePalette Palette { get; set; }

        /// <summary>
        /// A name for a theme the user has not named yet: the moment it was made.
        /// </summary>
        /// <remarks>
        /// Local time, and a format which sorts as text, because the name is the only
        /// thing distinguishing one saved theme from another in the list. Written with
        /// the invariant culture so the digits are the same in every language.
        /// </remarks>
        internal static string SuggestName()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture);
        }
    }
}
