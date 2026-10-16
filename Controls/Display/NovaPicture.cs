using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NovaUI
{
    public enum NovaPictureSizeMode { Normal, Stretch, Zoom, Cover }

    [ToolboxItem(true)]
    [DefaultProperty("Image")]
    [Description("صورة احترافية بزوايا دائرية ناعمة مع دعم أيقونات NovaUI ووضع Cover/Zoom وحالات تفاعلية.")]
    public class NovaPicture : Control
    {
        private Image _image;
        private int _borderRadius = 12;
        private bool _circle = false;
        private int _borderSize = 0;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private NovaPictureSizeMode _sizeMode = NovaPictureSizeMode.Cover;
        private Padding _imagePadding = new Padding(0);

        private string _iconGlyph = "\uEB9F";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";
        private int _iconSize = 48;
        private Color _iconColor = Color.FromArgb(148, 163, 184);
        private Color _placeholderBackColor = Color.FromArgb(241, 245, 249);
        private string _placeholderText = "";
        private Color _placeholderTextColor = Color.FromArgb(148, 163, 184);

        private bool _interactive = false;
        private bool _hover = false;
        private bool _showHoverOverlay = false;
        private Color _hoverOverlayColor = Color.FromArgb(60, 15, 23, 42);

        private bool _showShadow = false;
        private int _shadowDepth = 4;
        private Color _shadowColor = Color.FromArgb(50, 15, 23, 42);

        public event EventHandler ImageChanged;

        public NovaPicture()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(200, 200);
            Cursor = Cursors.Default;
        }

        [Category("NovaPicture"), DefaultValue(null)]
        public Image Image
        {
            get { return _image; }
            set { if (_image == value) return; _image = value; OnImageChanged(); }
        }

        [Category("NovaPicture"), DefaultValue(12)]
        public int BorderRadius { get { return _borderRadius; } set { _borderRadius = value; Invalidate(); } }

        [Category("NovaPicture"), DefaultValue(false)]
        public bool Circle { get { return _circle; } set { _circle = value; Invalidate(); } }

        [Category("NovaPicture"), DefaultValue(0)]
        public int BorderSize { get { return _borderSize; } set { _borderSize = Math.Max(0, value); Invalidate(); } }

        [Category("NovaPicture")]
        public Color BorderColor { get { return _borderColor; } set { _borderColor = value; Invalidate(); } }

        [Category("NovaPicture"), DefaultValue(NovaPictureSizeMode.Cover)]
        public NovaPictureSizeMode SizeMode { get { return _sizeMode; } set { _sizeMode = value; Invalidate(); } }

        [Category("NovaPicture")]
        public Padding ImagePadding { get { return _imagePadding; } set { _imagePadding = value; Invalidate(); } }

        [Category("NovaPicture - Icon"), DefaultValue("\uEB9F")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        public string IconGlyph { get { return _iconGlyph; } set { _iconGlyph = value ?? ""; Invalidate(); } }

        [Category("NovaPicture - Icon"), DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName { get { return _iconFontName; } set { _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); } }

        [Category("NovaPicture - Icon"), DefaultValue("")]
        public string IconPackName { get { return _iconPackName; } set { _iconPackName = value ?? ""; Invalidate(); } }

        [Category("NovaPicture - Icon"), DefaultValue(48)]
        public int IconSize { get { return _iconSize; } set { _iconSize = Math.Max(8, value); Invalidate(); } }

        [Category("NovaPicture - Icon")]
        public Color IconColor { get { return _iconColor; } set { _iconColor = value; Invalidate(); } }

        [Category("NovaPicture - Icon")]
        public Color PlaceholderBackColor { get { return _placeholderBackColor; } set { _placeholderBackColor = value; Invalidate(); } }

        [Category("NovaPicture - Icon"), DefaultValue("")]
        public string PlaceholderText { get { return _placeholderText; } set { _placeholderText = value ?? ""; Invalidate(); } }

        [Category("NovaPicture - Icon")]
        public Color PlaceholderTextColor { get { return _placeholderTextColor; } set { _placeholderTextColor = value; Invalidate(); } }

        [Category("NovaPicture - Behavior"), DefaultValue(false)]
        public bool Interactive
        {
            get { return _interactive; }
            set { _interactive = value; Cursor = value ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }

        [Category("NovaPicture - Behavior"), DefaultValue(false)]
        public bool ShowHoverOverlay { get { return _showHoverOverlay; } set { _showHoverOverlay = value; Invalidate(); } }

        [Category("NovaPicture - Behavior")]
        public Color HoverOverlayColor { get { return _hoverOverlayColor; } set { _hoverOverlayColor = value; Invalidate(); } }

        [Category("NovaPicture - Behavior"), DefaultValue(false)]
        public bool ShowShadow { get { return _showShadow; } set { _showShadow = value; Invalidate(); } }

        [Category("NovaPicture - Behavior"), DefaultValue(4)]
        public int ShadowDepth { get { return _shadowDepth; } set { _shadowDepth = Math.Max(1, value); Invalidate(); } }

        [Category("NovaPicture - Behavior")]
        public Color ShadowColor { get { return _shadowColor; } set { _shadowColor = value; Invalidate(); } }

        public void SetImage(Image img, bool disposeOld = false)
        {
            var old = _image;
            _image = img;
            if (disposeOld && old != null && !ReferenceEquals(old, img)) { try { old.Dispose(); } catch { } }
            OnImageChanged();
        }

        public void ClearImage(bool dispose = false)
        {
            var old = _image;
            _image = null;
            if (dispose && old != null) { try { old.Dispose(); } catch { } }
            OnImageChanged();
        }

        protected virtual void OnImageChanged()
        {
            Invalidate();
            var h = ImageChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void DrawParentBackground(Graphics g)
        {
            if (Parent == null) return;
            try
            {
                var state = g.Save();
                g.TranslateTransform(-Left, -Top);
                using (var pe = new PaintEventArgs(g, Parent.ClientRectangle))
                {
                    InvokePaintBackground(Parent, pe);
                    InvokePaint(Parent, pe);
                }
                g.Restore(state);
            }
            catch
            {
                try { using (var b = new SolidBrush(Parent.BackColor)) g.FillRectangle(b, ClientRectangle); } catch { }
            }
        }

        private int GetEffectiveRadius(Rectangle bounds)
        {
            int maxR = Math.Min(bounds.Width, bounds.Height) / 2;
            if (_circle || _borderRadius < 0) return maxR;
            return Math.Min(_borderRadius, maxR);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            var bounds = new Rectangle(
                _imagePadding.Left, _imagePadding.Top,
                Math.Max(1, Width - _imagePadding.Horizontal),
                Math.Max(1, Height - _imagePadding.Vertical));

            int radius = GetEffectiveRadius(bounds);

            if (_showShadow) DrawShadow(g, bounds, radius);

            using (var path = RoundedPath(bounds, radius))
            {
                using (var b = new SolidBrush(_placeholderBackColor))
                    g.FillPath(b, path);

                if (_image != null) DrawImageContent(g, path, bounds);
                else if (!string.IsNullOrEmpty(_iconGlyph)) DrawIconPlaceholder(g, bounds);
                else if (!string.IsNullOrEmpty(_placeholderText)) DrawTextPlaceholder(g, bounds);

                if (_interactive && _showHoverOverlay && _hover)
                {
                    using (var b = new SolidBrush(_hoverOverlayColor))
                        g.FillPath(b, path);
                }

                if (_borderSize > 0)
                {
                    using (var p = new Pen(_borderColor, _borderSize))
                    {
                        p.Alignment = PenAlignment.Inset;
                        g.DrawPath(p, path);
                    }
                }
            }
        }

        private void DrawImageContent(Graphics g, GraphicsPath path, Rectangle bounds)
        {
            var imgRect = ComputeImageRect(bounds);

            int w = Math.Max(1, (int)Math.Round(imgRect.Width));
            int h = Math.Max(1, (int)Math.Round(imgRect.Height));

            using (var scaled = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (var gs = Graphics.FromImage(scaled))
                {
                    gs.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gs.SmoothingMode = SmoothingMode.HighQuality;
                    gs.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gs.CompositingQuality = CompositingQuality.HighQuality;
                    gs.Clear(Color.Transparent);
                    gs.DrawImage(_image, new Rectangle(0, 0, w, h));
                }

                using (var brush = new TextureBrush(scaled) { WrapMode = WrapMode.Clamp })
                {
                    var matrix = new Matrix();
                    matrix.Translate(imgRect.X, imgRect.Y);
                    brush.Transform = matrix;
                    g.FillPath(brush, path);
                }
            }
        }

        private RectangleF ComputeImageRect(Rectangle bounds)
        {
            if (_image == null) return bounds;

            float iw = _image.Width;
            float ih = _image.Height;
            if (iw <= 0 || ih <= 0) return bounds;

            float bw = bounds.Width;
            float bh = bounds.Height;

            switch (_sizeMode)
            {
                case NovaPictureSizeMode.Normal:
                    return new RectangleF(
                        bounds.X + (bw - iw) / 2f,
                        bounds.Y + (bh - ih) / 2f, iw, ih);
                case NovaPictureSizeMode.Stretch:
                    return bounds;
                case NovaPictureSizeMode.Zoom:
                    {
                        float scale = Math.Min(bw / iw, bh / ih);
                        float nw = iw * scale;
                        float nh = ih * scale;
                        return new RectangleF(bounds.X + (bw - nw) / 2f, bounds.Y + (bh - nh) / 2f, nw, nh);
                    }
                case NovaPictureSizeMode.Cover:
                    {
                        float scale = Math.Max(bw / iw, bh / ih);
                        float nw = iw * scale;
                        float nh = ih * scale;
                        return new RectangleF(bounds.X + (bw - nw) / 2f, bounds.Y + (bh - nh) / 2f, nw, nh);
                    }
            }
            return bounds;
        }

        private void DrawIconPlaceholder(Graphics g, Rectangle bounds)
        {
            try
            {
                string key = !string.IsNullOrEmpty(_iconPackName) ? _iconPackName
                           : (!string.IsNullOrEmpty(_iconFontName) ? _iconFontName : "Segoe MDL2 Assets");

                Rectangle iconArea = bounds;
                if (!string.IsNullOrEmpty(_placeholderText))
                {
                    int textH = 24;
                    iconArea = new Rectangle(bounds.X, bounds.Y, bounds.Width, Math.Max(1, bounds.Height - textH));
                }

                using (var f = FontManager.GetIconFont(key, _iconSize))
                using (var b = new SolidBrush(_iconColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoClip
                })
                {
                    g.DrawString(_iconGlyph, f, b, iconArea, sf);
                }

                if (!string.IsNullOrEmpty(_placeholderText))
                {
                    var textRect = new Rectangle(bounds.X, iconArea.Bottom, bounds.Width, 24);
                    using (var b = new SolidBrush(_placeholderTextColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    })
                    {
                        g.DrawString(_placeholderText, Font, b, textRect, sf);
                    }
                }
            }
            catch { }
        }

        private void DrawTextPlaceholder(Graphics g, Rectangle bounds)
        {
            using (var b = new SolidBrush(_placeholderTextColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                g.DrawString(_placeholderText, Font, b, bounds, sf);
            }
        }

        private void DrawShadow(Graphics g, Rectangle bounds, int radius)
        {
            int layers = 4;
            for (int i = layers; i >= 1; i--)
            {
                int expand = i;
                int alpha = Math.Max(2, _shadowColor.A / (i + 1));

                var shadowRect = new Rectangle(
                    bounds.X - expand,
                    bounds.Y + _shadowDepth - expand / 2,
                    bounds.Width + expand * 2,
                    bounds.Height + expand * 2);

                using (var path = RoundedPath(shadowRect, radius + expand))
                using (var b = new SolidBrush(Color.FromArgb(alpha, _shadowColor.R, _shadowColor.G, _shadowColor.B)))
                {
                    g.FillPath(b, path);
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (_interactive) { _hover = true; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_interactive) { _hover = false; Invalidate(); }
        }

        private static GraphicsPath RoundedPath(Rectangle r, int radius)
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
    }
}