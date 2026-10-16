using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("ColorChanged")]
    [Description("منتقي ألوان احترافي: SV Area + Hue + Alpha + HEX + معاينة.")]
    public class NovaColorPicker : UserControl
    {
        #region Fileds

        private Color _selectedColor = Color.FromArgb(99, 102, 241);
        private float _hue;
        private float _saturation;
        private float _value;
        private int _alpha = 255;

        private Bitmap _svBitmap;
        private int _svBitmapHue = -1;

        private Rectangle _svRect;
        private Rectangle _hueRect;
        private Rectangle _alphaRect;
        private Rectangle _previewRect;

        private bool _draggingSV, _draggingHue, _draggingAlpha;
        private bool _hoverHue, _hoverAlpha;

        private readonly NovaTextBox _hexBox;

        private const int HSliderH = 20;
        private const int SliderGap = 12;
        private const int PreviewH = 50;
        private const int Radius = 8;
        private const int Padding_ = 12;

        public event EventHandler ColorChanged;

        #endregion

        public NovaColorPicker()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.White;
            Size = new Size(300, 380);
            Font = new Font("Segoe UI", 9.5F);

            var hsv = ColorToHSV(_selectedColor);
            _hue = hsv.h;
            _saturation = hsv.s;
            _value = hsv.v;
            _alpha = _selectedColor.A;

            _hexBox = new NovaTextBox
            {
                Size = new Size(120, 36),
                PlaceholderText = "#6366F1",
                IconGlyph = "",
                Radius = 8,
                Font = new Font("Consolas", 10F)
            };
            _hexBox.InnerTextBox.MaxLength = 7;
            _hexBox.InnerTextBox.TextChanged += HexBox_TextChanged;
            _hexBox.InnerTextBox.Leave += (s, e) => SyncHexBox();
            Controls.Add(_hexBox);
        }

        #region Properties

        [Category("NovaColorPicker")]
        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor == value) return;
                _selectedColor = value;
                var hsv = ColorToHSV(value);
                _hue = hsv.h;
                _saturation = hsv.s;
                _value = hsv.v;
                _alpha = value.A;
                SyncHexBox();
                Invalidate();
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("NovaColorPicker"), DefaultValue(255)]
        [Description("قناة الشفافية 0-255.")]
        public int Alpha
        {
            get => _alpha;
            set
            {
                _alpha = Math.Max(0, Math.Min(255, value));
                UpdateColorFromHSV();
                Invalidate();
            }
        }

        [Category("NovaColorPicker"), DefaultValue(8)]
        public int BorderRadius { get; set; } = 8;

        #endregion

        #region Layout

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            LayoutControls();
        }

        private void LayoutControls()
        {
            int w = ClientSize.Width - Padding_ * 2;

            int svH = Math.Min(w, 180);
            _svRect = new Rectangle(Padding_, Padding_, w, svH);

            int y = _svRect.Bottom + SliderGap;
            _hueRect = new Rectangle(Padding_, y, w, HSliderH);

            y = _hueRect.Bottom + SliderGap;
            _alphaRect = new Rectangle(Padding_, y, w, HSliderH);

            y = _alphaRect.Bottom + SliderGap;
            _previewRect = new Rectangle(Padding_, y, w - 140, PreviewH);

            if (_hexBox != null)
                _hexBox.Location = new Point(Padding_ + _previewRect.Width + 12, y + 7);

            Height = _previewRect.Bottom + Padding_;
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), BorderRadius))
            using (var b = new SolidBrush(BackColor))
                g.FillPath(b, path);

            DrawSVArea(g);
            DrawHueSlider(g);
            DrawAlphaSlider(g);
            DrawPreview(g);

            using (var p = new Pen(Color.FromArgb(226, 232, 240), 1))
            using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), BorderRadius))
                g.DrawPath(p, path);
        }

        private void DrawSVArea(Graphics g)
        {
            int hueInt = (int)Math.Round(_hue);
            if (_svBitmap == null || _svBitmapHue != hueInt ||
                _svBitmap.Width != _svRect.Width || _svBitmap.Height != _svRect.Height)
            {
                BuildSVBitmap(_svRect.Width, _svRect.Height, _hue);
                _svBitmapHue = hueInt;
            }

            g.DrawImage(_svBitmap, _svRect);

            int cx = _svRect.X + (int)(_saturation * _svRect.Width);
            int cy = _svRect.Y + (int)((1 - _value) * _svRect.Height);

            using (var p = new Pen(Color.White, 2))
                g.DrawEllipse(p, cx - 6, cy - 6, 12, 12);
            using (var p = new Pen(Color.FromArgb(80, 0, 0, 0), 1))
                g.DrawEllipse(p, cx - 7, cy - 7, 14, 14);
        }

        private void BuildSVBitmap(int w, int h, float hue)
        {
            if (w <= 0 || h <= 0) return;
            _svBitmap?.Dispose();
            _svBitmap = new Bitmap(w, h);

            for (int y = 0; y < h; y++)
            {
                float v = 1f - (float)y / (h - 1);
                for (int x = 0; x < w; x++)
                {
                    float s = (float)x / (w - 1);
                    var rgb = HSVToColor(hue, s, v, 255);
                    _svBitmap.SetPixel(x, y, rgb);
                }
            }
        }

        private void DrawHueSlider(Graphics g)
        {
            using (var path = Round(_hueRect, _hueRect.Height / 2))
            {
                using (var brush = new LinearGradientBrush(_hueRect, Color.Red, Color.Red, LinearGradientMode.Horizontal))
                {
                    brush.InterpolationColors = new ColorBlend
                    {
                        Positions = new[] { 0f, 1f / 6, 2f / 6, 3f / 6, 4f / 6, 5f / 6, 1f },
                        Colors = new[] { Color.Red, Color.Yellow, Color.Lime, Color.Cyan, Color.Blue, Color.Magenta, Color.Red }
                    };
                    g.FillPath(brush, path);
                }
            }

            int cx = _hueRect.X + (int)(_hue / 360f * _hueRect.Width);
            int cy = _hueRect.Y + _hueRect.Height / 2;
            DrawSliderThumb(g, cx, cy);
        }

        private void DrawAlphaSlider(Graphics g)
        {
            DrawCheckerboard(g, _alphaRect);

            using (var path = Round(_alphaRect, _alphaRect.Height / 2))
            using (var brush = new LinearGradientBrush(_alphaRect,
                Color.FromArgb(0, _selectedColor.R, _selectedColor.G, _selectedColor.B),
                Color.FromArgb(255, _selectedColor.R, _selectedColor.G, _selectedColor.B),
                LinearGradientMode.Horizontal))
            {
                g.FillPath(brush, path);
            }

            int cx = _alphaRect.X + (int)((_alpha / 255f) * _alphaRect.Width);
            int cy = _alphaRect.Y + _alphaRect.Height / 2;
            DrawSliderThumb(g, cx, cy);
        }

        private void DrawSliderThumb(Graphics g, int cx, int cy)
        {
            using (var b = new SolidBrush(Color.White))
                g.FillEllipse(b, cx - 9, cy - 9, 18, 18);

            using (var p = new Pen(Color.FromArgb(120, 120, 140), 2))
                g.DrawEllipse(p, cx - 9, cy - 9, 18, 18);

            using (var p = new Pen(Color.FromArgb(60, 60, 80), 1))
                g.DrawEllipse(p, cx - 6, cy - 6, 12, 12);
        }

        private void DrawCheckerboard(Graphics g, Rectangle rect)
        {
            int size = 8;
            using (var path = Round(rect, rect.Height / 2))
            {
                var old = g.Clip;
                g.SetClip(path);

                for (int y = rect.Y; y < rect.Bottom; y += size)
                    for (int x = rect.X; x < rect.Right; x += size)
                    {
                        bool dark = ((x / size) + (y / size)) % 2 == 0;
                        using (var b = new SolidBrush(dark ? Color.FromArgb(220, 220, 220) : Color.White))
                            g.FillRectangle(b, x, y, size, size);
                    }

                g.Clip = old;
            }
        }

        private void DrawPreview(Graphics g)
        {
            if (_alpha < 255) DrawCheckerboard(g, _previewRect);

            using (var path = Round(_previewRect, 8))
            using (var b = new SolidBrush(_selectedColor))
                g.FillPath(b, path);

            using (var p = new Pen(Color.FromArgb(226, 232, 240), 1))
            using (var path = Round(_previewRect, 8))
                g.DrawPath(p, path);
        }

        #endregion

        #region Mouse

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            if (_svRect.Contains(e.Location)) { _draggingSV = true; UpdateSV(e.Location); }
            else if (_hueRect.Contains(e.Location)) { _draggingHue = true; UpdateHue(e.Location); }
            else if (_alphaRect.Contains(e.Location)) { _draggingAlpha = true; UpdateAlpha(e.Location); }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_draggingSV) UpdateSV(e.Location);
            else if (_draggingHue) UpdateHue(e.Location);
            else if (_draggingAlpha) UpdateAlpha(e.Location);
            else
            {
                bool hh = _hueRect.Contains(e.Location);
                bool ha = _alphaRect.Contains(e.Location);
                if (hh != _hoverHue || ha != _hoverAlpha)
                {
                    _hoverHue = hh;
                    _hoverAlpha = ha;
                    Cursor = (hh || ha) ? Cursors.Hand : Cursors.Default;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _draggingSV = _draggingHue = _draggingAlpha = false;
        }

        private void UpdateSV(Point p)
        {
            _saturation = Math.Max(0f, Math.Min(1f, (float)(p.X - _svRect.X) / _svRect.Width));
            _value = 1f - Math.Max(0f, Math.Min(1f, (float)(p.Y - _svRect.Y) / _svRect.Height));
            UpdateColorFromHSV();
            Invalidate();
        }

        private void UpdateHue(Point p)
        {
            _hue = Math.Max(0f, Math.Min(360f, ((float)(p.X - _hueRect.X) / _hueRect.Width) * 360f));
            UpdateColorFromHSV();
            Invalidate();
        }

        private void UpdateAlpha(Point p)
        {
            _alpha = Math.Max(0, Math.Min(255, (int)(((float)(p.X - _alphaRect.X) / _alphaRect.Width) * 255f)));
            UpdateColorFromHSV();
            Invalidate();
        }

        private void UpdateColorFromHSV()
        {
            _selectedColor = HSVToColor(_hue, _saturation, _value, _alpha);
            SyncHexBox();
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region HEX

        private bool _syncingHex;

        private void SyncHexBox()
        {
            if (_hexBox == null) return;
            _syncingHex = true;
            _hexBox.Text = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
            _syncingHex = false;
        }

        private void HexBox_TextChanged(object sender, EventArgs e)
        {
            if (_syncingHex) return;
            var text = _hexBox.Text.Trim();
            if (!text.StartsWith("#")) text = "#" + text;
            if (text.Length != 7) return;

            try
            {
                var c = ColorTranslator.FromHtml(text);
                _selectedColor = Color.FromArgb(_alpha, c.R, c.G, c.B);
                var hsv = ColorToHSV(_selectedColor);
                _hue = hsv.h;
                _saturation = hsv.s;
                _value = hsv.v;
                Invalidate();
                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        #endregion

        #region Helpers

        private static Color HSVToColor(float h, float s, float v, int alpha)
        {
            h = h % 360f;
            if (h < 0) h += 360;

            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
            float m = v - c;

            float r = 0, g = 0, b = 0;
            if (h < 60) { r = c; g = x; }
            else if (h < 120) { r = x; g = c; }
            else if (h < 180) { g = c; b = x; }
            else if (h < 240) { g = x; b = c; }
            else if (h < 300) { r = x; b = c; }
            else { r = c; b = x; }

            return Color.FromArgb(
                Math.Max(0, Math.Min(255, alpha)),
                Math.Max(0, Math.Min(255, (int)((r + m) * 255))),
                Math.Max(0, Math.Min(255, (int)((g + m) * 255))),
                Math.Max(0, Math.Min(255, (int)((b + m) * 255))));
        }

        private static (float h, float s, float v) ColorToHSV(Color color)
        {
            float r = color.R / 255f, g = color.G / 255f, b = color.B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0f;
            if (delta > 0)
            {
                if (max == r) h = 60 * (((g - b) / delta) % 6);
                else if (max == g) h = 60 * (((b - r) / delta) + 2);
                else h = 60 * (((r - g) / delta) + 4);
            }
            if (h < 0) h += 360;

            float s = max == 0 ? 0 : delta / max;
            return (h, s, max);
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _svBitmap?.Dispose();
                _hexBox?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}