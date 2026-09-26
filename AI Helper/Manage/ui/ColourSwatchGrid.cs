using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.ui
{
    /// <summary>
    /// A grid of ready made colours, for setting one without going through a picker.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A theme is mostly made of greys, whites and one accent, so most of the work is
    /// picking something close rather than choosing an exact value. A grid of the colours
    /// which come up again and again does that in one click, and the hex field beside it is
    /// there for the times when it has to be exact.
    /// </para>
    /// <para>
    /// A single control rather than a table of small ones: the colours are a list, and one
    /// control paints them and works out which was clicked. A table would put forty
    /// controls into the designer and into the theme loader's walk for no gain.
    /// </para>
    /// </remarks>
    internal sealed class ColourSwatchGrid : Control
    {
        /// <summary>
        /// How many colours go on a row.
        /// </summary>
        internal const int Columns = 6;

        private const int CellSize = 22;
        private const int CellGap = 3;

        /// <summary>
        /// The colours offered, in reading order: the greys a theme is built from, then the
        /// hues it is marked with.
        /// </summary>
        /// <remarks>
        /// Named colours rather than numbers, because a named colour is also something the
        /// theme file accepts, so what is picked here can be typed there.
        /// </remarks>
        private static readonly Color[] Colours =
        {
            Color.White, Color.Gainsboro, Color.Silver, Color.Gray, Color.DimGray, Color.Black,
            Color.WhiteSmoke, Color.LightGray, Color.DarkGray, Color.DarkSlateGray, Color.Teal, Color.MidnightBlue,
            Color.LightPink, Color.Salmon, Color.OrangeRed, Color.Red, Color.DarkRed, Color.Maroon,
            Color.Wheat, Color.Gold, Color.Yellow, Color.YellowGreen, Color.Olive, Color.DarkOliveGreen,
            Color.Honeydew, Color.LightGreen, Color.LimeGreen, Color.Green, Color.SeaGreen, Color.DarkGreen,
            Color.LightCyan, Color.PowderBlue, Color.DeepSkyBlue, Color.DodgerBlue, Color.SteelBlue, Color.Navy,
            Color.Lavender, Color.Plum, Color.MediumPurple, Color.BlueViolet, Color.Purple, Color.Indigo,
        };

        private Color _borderColour = Color.FromArgb(120, 120, 120);

        internal ColourSwatchGrid()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint
                | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor,
                true);

            // the surface behind the grid belongs to the window, and the grid only draws
            // its own cells on top of it
            BackColor = Color.Transparent;
        }

        /// <summary>
        /// Raised when one of the colours is clicked.
        /// </summary>
        internal event Action<Color> ColourPicked;

        /// <summary>
        /// Colour of the line around each cell. Set from the palette so the grid reads as
        /// part of the window rather than as a block pasted into it.
        /// </summary>
        internal Color BorderColour
        {
            get { return _borderColour; }
            set
            {
                _borderColour = value;
                Invalidate();
            }
        }

        /// <summary>
        /// How many rows the grid needs.
        /// </summary>
        internal static int RowCount
        {
            get { return (Colours.Length + Columns - 1) / Columns; }
        }

        /// <summary>
        /// The height at which every colour is on screen.
        /// </summary>
        internal static int PreferredGridHeight
        {
            get { return RowCount * (CellSize + CellGap) + CellGap; }
        }

        /// <summary>
        /// The area one cell occupies.
        /// </summary>
        internal static Rectangle CellBounds(int index)
        {
            var column = index % Columns;
            var row = index / Columns;

            return new Rectangle(
                CellGap + column * (CellSize + CellGap),
                CellGap + row * (CellSize + CellGap),
                CellSize,
                CellSize);
        }

        /// <summary>
        /// The colour at a point, or <c>-1</c> when the point is between the cells.
        /// </summary>
        /// <remarks>
        /// The whole of the work, with the mouse handling left as a thin wrapper over it, so
        /// what a click does can be checked without a mouse.
        /// </remarks>
        internal int IndexAt(Point location)
        {
            for (var index = 0; index < Colours.Length; index++)
            {
                if (CellBounds(index).Contains(location)) return index;
            }

            return -1;
        }

        /// <summary>
        /// Takes a click on the grid, and reports the colour which was hit.
        /// </summary>
        /// <returns><c>true</c> when a colour was picked.</returns>
        internal bool TryPick(Point location, out Color colour)
        {
            colour = Color.Empty;

            var index = IndexAt(location);
            if (index < 0) return false;

            colour = Colours[index];

            var handler = ColourPicked;
            if (handler != null) handler(colour);

            return true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) return;

            Color picked;
            if (TryPick(e.Location, out picked)) Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // a swatch of a light colour on a light surface and of a dark one on a dark
            // surface both need an outline, so every cell gets one
            using (var pen = new Pen(_borderColour))
            {
                for (var index = 0; index < Colours.Length; index++)
                {
                    var bounds = CellBounds(index);

                    using (var brush = new SolidBrush(Colours[index]))
                    {
                        e.Graphics.FillRectangle(brush, bounds);
                    }

                    // drawn inside the cell so neighbouring cells do not share an edge
                    e.Graphics.DrawRectangle(
                        pen,
                        bounds.X,
                        bounds.Y,
                        bounds.Width - 1,
                        bounds.Height - 1);
                }
            }
        }

        /// <summary>
        /// Draws the cells without the flicker a control of this size would otherwise show
        /// while the window is being resized.
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackColor == Color.Transparent)
            {
                base.OnPaintBackground(e);
                return;
            }

            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}
