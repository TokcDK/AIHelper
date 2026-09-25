using AIHelper.Manage;
using System.Collections.Generic;

namespace AIHelper.Games.Settings
{
    /// <summary>
    /// The screen resolutions a game settings tab offers.
    /// </summary>
    /// <remarks>
    /// One instance per settings control. This used to be a static list that every
    /// control appended to, so a resolution found in one game leaked into the list
    /// of the next one and the shared list kept growing for the whole lifetime of
    /// the process.
    /// </remarks>
    internal sealed class ScreenResolutions
    {
        /// <summary>
        /// Format the games expect in their own settings file, for example
        /// "1280 x 720 (16 : 9)".
        /// </summary>
        private const string LabelFormat = "{0} x {1} ({2})";

        private static readonly (string Label, int Width, int Height)[] Defaults =
        {
            ("854 x 480 (16 : 9)",    854,  480),
            ("800 x 600 (4 : 3)",     800,  600),
            ("1024 x 576 (16 : 9)",  1024,  576),
            ("1024 x 768 (4 : 3)",   1024,  768),
            ("1280 x 720 (16 : 9)",  1280,  720),
            ("1366 x 768 (16 : 9)",  1366,  768),
            ("1536 x 864 (16 : 9)",  1536,  864),
            ("1600 x 900 (16 : 9)",  1600,  900),
            ("1920 x 1080 (16 : 9)", 1920, 1080),
            ("2560 x 1440 (16 : 9)", 2560, 1440),
            ("3840 x 2160 (16 : 9)", 3840, 2160),
        };

        private readonly List<(string Label, int Width, int Height)> _items =
            new List<(string Label, int Width, int Height)>(Defaults);

        /// <summary>
        /// Number of known resolutions.
        /// </summary>
        internal int Count => _items.Count;

        internal (string Label, int Width, int Height) this[int index] => _items[index];

        /// <summary>
        /// Labels of all known resolutions, in the order they are offered.
        /// </summary>
        internal string[] Labels()
        {
            var labels = new string[_items.Count];

            for (var i = 0; i < _items.Count; i++)
            {
                labels[i] = _items[i].Label;
            }

            return labels;
        }

        /// <summary>
        /// Finds a resolution by its pixel size.
        /// </summary>
        /// <returns>The index, or -1 when the size is unknown.</returns>
        internal int IndexOf(int width, int height) => _items.FindIndex(r => r.Width == width && r.Height == height);

        /// <summary>
        /// Finds a resolution by the label the game stored.
        /// </summary>
        /// <returns>The index, or -1 when the label is unknown.</returns>
        internal int IndexOfLabel(string label) => _items.FindIndex(r => r.Label == label);

        /// <summary>
        /// Adds a resolution the game uses but the list does not know yet.
        /// </summary>
        /// <returns>The index of the added entry.</returns>
        internal int Add(int width, int height)
        {
            _items.Add((BuildLabel(width, height), width, height));
            return _items.Count - 1;
        }

        /// <summary>
        /// Builds the label of a resolution in the format the games use.
        /// </summary>
        internal static string BuildLabel(int width, int height) =>
            string.Format(LabelFormat, width, height, ManageMath.GetAspectRatio(width, height));
    }
}
