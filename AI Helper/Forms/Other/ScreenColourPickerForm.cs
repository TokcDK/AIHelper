using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using NLog;

namespace AIHelper.Forms.Other
{
    /// <summary>
    /// Takes a colour from anywhere on the screen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The colour picker Windows provides has no way to take a colour which is already on
    /// screen, which is the one thing a theme is most often matched against: another
    /// program, a picture, a game's own interface. This shows the screen as it was and lets
    /// a single pixel of it be clicked.
    /// </para>
    /// <para>
    /// Built in code rather than from a designer because it has no child controls at all -
    /// it paints a picture of the screen and a magnifier beside the pointer. The screen is
    /// captured <em>before</em> the window appears, so the window is never part of what is
    /// being picked from.
    /// </para>
    /// </remarks>
    internal sealed class ScreenColourPickerForm : Form
    {
        /// <summary>
        /// How much bigger than life the pixels around the pointer are drawn.
        /// </summary>
        private const int Zoom = 8;

        /// <summary>
        /// How many pixels either side of the pointer the magnifier shows.
        /// </summary>
        private const int SampleRadius = 6;

        private const int StripHeight = 22;

        /// <summary>
        /// Half the width of the cross drawn at the pointer.
        /// </summary>
        private const int CrosshairArm = 12;

        static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly Bitmap _screen;
        private readonly Rectangle _screenBounds;

        private Point _pointer;
        private bool _hasPointer;
        private Rectangle _previousArea = Rectangle.Empty;

        private ScreenColourPickerForm(Bitmap screen, Rectangle screenBounds)
        {
            _screen = screen;
            _screenBounds = screenBounds;

            // no border, no title, exactly over the screen which was captured
            AutoScaleMode = AutoScaleMode.None;
            Bounds = screenBounds;
            Cursor = Cursors.Cross;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// The colour which was picked, or <see cref="Color.Empty"/> when the user gave up.
        /// </summary>
        internal Color PickedColour { get; private set; }

        /// <summary>
        /// Shows the screen and waits for a pixel to be clicked.
        /// </summary>
        /// <remarks>
        /// The caller is expected to have hidden whatever window the request came from, so
        /// that what is behind it can be picked from as well. Everything on the screen is
        /// in the capture, which is why the capture is taken here rather than after the
        /// window is up.
        /// </remarks>
        internal static Color Pick(IWin32Window owner)
        {
            var bounds = SystemInformation.VirtualScreen;

            Bitmap screen;
            try
            {
                screen = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

                using (var graphics = Graphics.FromImage(screen))
                {
                    graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                }
            }
            catch (Exception ex)
            {
                // a locked session or a protected surface can refuse this, and there is
                // nothing to show if it does
                _log.Error("Failed to capture the screen for the colour picker. Error:\r\n" + ex);
                return Color.Empty;
            }

            using (var picker = new ScreenColourPickerForm(screen, bounds))
            {
                return picker.ShowDialog(owner) == DialogResult.OK ? picker.PickedColour : Color.Empty;
            }
        }

        /// <summary>
        /// The colour of the screen at a point of this window.
        /// </summary>
        /// <remarks>
        /// The window sits exactly over the area which was captured, so a point of it is
        /// the same point of the picture once the origin of that area is taken off - which
        /// is not zero when the screen to the left of the main one is in use.
        /// </remarks>
        private Color ColourAt(Point point)
        {
            var screenPoint = PointToScreen(point);

            var x = screenPoint.X - _screenBounds.Left;
            var y = screenPoint.Y - _screenBounds.Top;

            if (x < 0 || y < 0 || x >= _screen.Width || y >= _screen.Height) return Color.Empty;

            return _screen.GetPixel(x, y);
        }

        /// <summary>
        /// The colour of the screen at an offset from the pointer, for the magnifier.
        /// </summary>
        private Color ColourAtOffset(int offsetX, int offsetY)
        {
            var screenPoint = PointToScreen(_pointer);

            var x = screenPoint.X + offsetX - _screenBounds.Left;
            var y = screenPoint.Y + offsetY - _screenBounds.Top;

            if (x < 0 || y < 0 || x >= _screen.Width || y >= _screen.Height) return Color.Empty;

            return _screen.GetPixel(x, y);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _pointer = e.Location;
            _hasPointer = true;

            // only what the magnifier and the cross covered is asked for again, so the
            // picture underneath is not copied to the window on every move of the mouse
            var area = Union(_previousArea, Union(MagnifierBounds(), CrosshairBounds()));

            _previousArea = Union(MagnifierBounds(), CrosshairBounds());

            Invalidate(area);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            _pointer = e.Location;

            var colour = ColourAt(_pointer);
            if (colour.IsEmpty) return;

            PickedColour = colour;
            DialogResult = DialogResult.OK;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode != Keys.Escape) return;

            DialogResult = DialogResult.Cancel;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // the screen as it was, which is also what makes the window look transparent
            e.Graphics.DrawImageUnscaled(_screen, 0, 0);

            if (!_hasPointer) return;

            DrawCrosshair(e.Graphics);
            DrawMagnifier(e.Graphics);
        }

        /// <summary>
        /// Draws a cross at the pointer, so the pixel about to be taken is easy to aim at.
        /// </summary>
        private void DrawCrosshair(Graphics graphics)
        {
            var bounds = CrosshairBounds();

            // drawn twice, dark then light, so it stays visible over any picture
            using (var dark = new Pen(Color.FromArgb(160, 0, 0, 0)))
            using (var light = new Pen(Color.FromArgb(200, 255, 255, 255)))
            {
                graphics.DrawLine(dark, bounds.Left, _pointer.Y, bounds.Right, _pointer.Y);
                graphics.DrawLine(dark, _pointer.X, bounds.Top, _pointer.X, bounds.Bottom);
                graphics.DrawLine(light, bounds.Left, _pointer.Y - 1, bounds.Right, _pointer.Y - 1);
                graphics.DrawLine(light, _pointer.X - 1, bounds.Top, _pointer.X - 1, bounds.Bottom);
            }
        }

        /// <summary>
        /// Draws the pixels around the pointer, larger than life, with the colour they add
        /// up to underneath.
        /// </summary>
        private void DrawMagnifier(Graphics graphics)
        {
            var area = MagnifierBounds();
            var side = (SampleRadius * 2 + 1) * Zoom;

            for (var offsetY = -SampleRadius; offsetY <= SampleRadius; offsetY++)
            {
                for (var offsetX = -SampleRadius; offsetX <= SampleRadius; offsetX++)
                {
                    var pixelColour = ColourAtOffset(offsetX, offsetY);
                    if (pixelColour.IsEmpty) continue;

                    using (var brush = new SolidBrush(pixelColour))
                    {
                        graphics.FillRectangle(
                            brush,
                            area.X + (offsetX + SampleRadius) * Zoom,
                            area.Y + (offsetY + SampleRadius) * Zoom,
                            Zoom,
                            Zoom);
                    }
                }
            }

            // the middle cell is the pixel which will be taken, so it is marked off from
            // the ones around it
            using (var marker = new Pen(Color.FromArgb(220, 255, 255, 255), 2))
            {
                graphics.DrawRectangle(marker, area.X + SampleRadius * Zoom - 1, area.Y + SampleRadius * Zoom - 1, Zoom + 1, Zoom + 1);
            }

            var strip = new Rectangle(area.X, area.Y + side, area.Width, StripHeight);

            using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            {
                graphics.FillRectangle(brush, strip);
            }

            var colour = ColourAt(_pointer);

            TextRenderer.DrawText(
                graphics,
                CustomThemeStoreHex(colour) + "   " + ArgbText(colour),
                Font,
                strip,
                Color.Black,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private Rectangle CrosshairBounds()
        {
            return new Rectangle(
                _pointer.X - CrosshairArm - 1,
                _pointer.Y - CrosshairArm - 1,
                CrosshairArm * 2 + 3,
                CrosshairArm * 2 + 3);
        }

        /// <summary>
        /// Where the magnifier goes: beside the pointer, kept inside the screen.
        /// </summary>
        private Rectangle MagnifierBounds()
        {
            var side = (SampleRadius * 2 + 1) * Zoom;
            var width = side;
            var height = side + StripHeight;

            var x = _pointer.X + 20;
            var y = _pointer.Y + 20;

            // flipped to the other side of the pointer rather than pushed away from it,
            // so the magnifier stays next to what it is magnifying
            if (x + width > ClientSize.Width) x = _pointer.X - 20 - width;
            if (y + height > ClientSize.Height) y = _pointer.Y - 20 - height;

            x = Math.Max(0, Math.Min(x, Math.Max(0, ClientSize.Width - width)));
            y = Math.Max(0, Math.Min(y, Math.Max(0, ClientSize.Height - height)));

            return new Rectangle(x, y, width, height);
        }

        private static Rectangle Union(Rectangle a, Rectangle b)
        {
            if (a.IsEmpty) return b;
            if (b.IsEmpty) return a;

            return Rectangle.Union(a, b);
        }

        /// <summary>
        /// The hex form of a colour, as the theme file spells it.
        /// </summary>
        /// <remarks>
        /// Written through the theme store so the value shown here is exactly the value
        /// which would be saved, rather than one which looks like it.
        /// </remarks>
        private static string CustomThemeStoreHex(Color colour)
        {
            return colour.IsEmpty ? "#00000000" : AIHelper.Manage.ui.themes.CustomThemeStore.HexColour(colour);
        }

        private static string ArgbText(Color colour)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}, {1}, {2}, {3}",
                colour.A,
                colour.R,
                colour.G,
                colour.B);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _screen != null)
            {
                _screen.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
