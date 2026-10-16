using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("Click")]
    [Description("زر احترافي بزوايا دائرية ناعمة جداً وبدون أطراف مكسرة.")]
    public class NovaButton : Button
    {
        private bool _hover, _down;

        private int _borderRadius = 12;
        private int _borderSize = 0;
        private Color _borderColor = Color.FromArgb(99, 102, 241);

        private string _iconGlyph = "";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";
        private int _iconSize = 14;
        private int _iconSpacing = 8;
        private bool _iconOnRight = false;

        private Color _normalColor = Color.FromArgb(99, 102, 241);
        private Color _hoverColor = Color.FromArgb(79, 70, 229);
        private Color _pressColor = Color.FromArgb(67, 56, 202);
        private Color _disabledColor = Color.FromArgb(203, 213, 225);

        private Color _textColor = Color.White;
        private Color _hoverTextColor = Color.White;
        private Color _disabledTextColor = Color.FromArgb(148, 163, 184);

        private bool _gradientEnabled = false;
        private Color _gradientColor1 = Color.FromArgb(99, 102, 241);
        private Color _gradientColor2 = Color.FromArgb(139, 92, 246);
        private float _gradientAngle = 45f;

        public NovaButton()
        {
            try { IconPackRegistry.EnsureInitialized(); } catch { }

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            base.BackColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            Size = new Size(200, 50);
        }

        [Category("NovaButton - Shape"), DefaultValue(12)]
        public int BorderRadius { get { return _borderRadius; } set { _borderRadius = value; Invalidate(); } }

        [Category("NovaButton - Shape"), DefaultValue(0)]
        public int BorderSize { get { return _borderSize; } set { _borderSize = Math.Max(0, value); Invalidate(); } }

        [Category("NovaButton - Shape")]
        public Color BorderColor { get { return _borderColor; } set { _borderColor = value; Invalidate(); } }

        [Category("FControls")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        [DefaultValue("")]
        public string IconGlyph { get { return _iconGlyph; } set { _iconGlyph = value ?? ""; Invalidate(); } }

        [Category("FControls"), DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName { get { return _iconFontName; } set { _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); } }

        [Category("FControls"), DefaultValue("")]
        public string IconPackName { get { return _iconPackName; } set { _iconPackName = value ?? ""; Invalidate(); } }

        [Category("NovaButton - Icon"), DefaultValue(14)]
        public int IconSize { get { return _iconSize; } set { _iconSize = Math.Max(6, value); Invalidate(); } }

        [Category("NovaButton - Icon"), DefaultValue(8)]
        public int IconSpacing { get { return _iconSpacing; } set { _iconSpacing = Math.Max(0, value); Invalidate(); } }

        [Category("NovaButton - Icon"), DefaultValue(false)]
        public bool IconOnRight { get { return _iconOnRight; } set { _iconOnRight = value; Invalidate(); } }

        [Category("NovaButton - Colors")]
        public Color NormalColor { get { return _normalColor; } set { _normalColor = value; Invalidate(); } }
        [Category("NovaButton - Colors")]
        public Color HoverColor { get { return _hoverColor; } set { _hoverColor = value; Invalidate(); } }
        [Category("NovaButton - Colors")]
        public Color PressColor { get { return _pressColor; } set { _pressColor = value; Invalidate(); } }
        [Category("NovaButton - Colors")]
        public Color DisabledColor { get { return _disabledColor; } set { _disabledColor = value; Invalidate(); } }
        [Category("NovaButton - Colors")]
        public Color TextColor { get { return _textColor; } set { _textColor = value; ForeColor = value; Invalidate(); } }
        [Category("NovaButton - Colors")]
        public Color HoverTextColor { get { return _hoverTextColor; } set { _hoverTextColor = value; Invalidate(); } }
        [Category("NovaButton - Colors")]
        public Color DisabledTextColor { get { return _disabledTextColor; } set { _disabledTextColor = value; Invalidate(); } }

        [Category("NovaButton - Gradient"), DefaultValue(false)]
        public bool GradientEnabled { get { return _gradientEnabled; } set { _gradientEnabled = value; Invalidate(); } }
        [Category("NovaButton - Gradient")]
        public Color GradientColor1 { get { return _gradientColor1; } set { _gradientColor1 = value; Invalidate(); } }
        [Category("NovaButton - Gradient")]
        public Color GradientColor2 { get { return _gradientColor2; } set { _gradientColor2 = value; Invalidate(); } }
        [Category("NovaButton - Gradient"), DefaultValue(45f)]
        public float GradientAngle { get { return _gradientAngle; } set { _gradientAngle = value; Invalidate(); } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor { get { return base.BackColor; } set { base.BackColor = Color.Transparent; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Cursor Cursor { get { return base.Cursor; } set { base.Cursor = value; } }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void DrawParentBackground(Graphics g)
        {
            if (Parent == null) return;
            try
            {
                var state = g.Save();
                g.TranslateTransform(-this.Left, -this.Top);
                using (var pe = new PaintEventArgs(g, Parent.ClientRectangle))
                {
                    InvokePaintBackground(Parent, pe);
                    InvokePaint(Parent, pe);
                }
                g.Restore(state);
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            try
            {
                int radius = _borderRadius < 0 ? Height / 2 : Math.Min(_borderRadius, Height / 2);
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);

                using (var path = Round(rect, radius))
                {
                    if (Enabled && _gradientEnabled && !_down && !_hover)
                    {
                        using (var brush = new LinearGradientBrush(rect, _gradientColor1, _gradientColor2, _gradientAngle))
                            g.FillPath(brush, path);
                    }
                    else
                    {
                        Color fill = !Enabled ? _disabledColor : _down ? _pressColor : _hover ? _hoverColor : _normalColor;
                        using (var b = new SolidBrush(fill))
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

                Color contentColor = !Enabled ? _disabledTextColor : _hover ? _hoverTextColor : _textColor;
                bool hasText = !string.IsNullOrEmpty(Text);
                bool hasIcon = !string.IsNullOrEmpty(_iconGlyph);

                if (hasIcon && !hasText) DrawIconOnly(g, contentColor);
                else if (!hasIcon && hasText) DrawTextOnly(g, contentColor);
                else if (hasIcon && hasText) DrawIconWithText(g, contentColor);
            }
            catch { }
        }

        private Font GetIconFont()
        {
            string fontKey = !string.IsNullOrEmpty(_iconPackName) ? _iconPackName
                : (!string.IsNullOrEmpty(_iconFontName) ? _iconFontName : "Segoe MDL2 Assets");
            return FontManager.GetIconFont(fontKey, _iconSize);
        }

        private void DrawIconOnly(Graphics g, Color color)
        {
            try
            {
                using (var iconFont = GetIconFont())
                using (var path = new GraphicsPath())
                {
                    path.AddString(_iconGlyph, iconFont.FontFamily, (int)iconFont.Style, iconFont.Size, Point.Empty, StringFormat.GenericTypographic);
                    var bounds = path.GetBounds();
                    if (bounds.Width <= 0 || bounds.Height <= 0) return;

                    float offsetX = (Width - bounds.Width) / 2f - bounds.X;
                    float offsetY = (Height - bounds.Height) / 2f - bounds.Y;

                    using (var m = new Matrix()) { m.Translate(offsetX, offsetY); path.Transform(m); }
                    using (var b = new SolidBrush(color)) g.FillPath(b, path);
                }
            }
            catch { }
        }

        private void DrawTextOnly(Graphics g, Color color)
        {
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }

        private void DrawIconWithText(Graphics g, Color color)
        {
            try
            {
                using (var iconFont = GetIconFont())
                using (var iconPath = new GraphicsPath())
                {
                    iconPath.AddString(_iconGlyph, iconFont.FontFamily, (int)iconFont.Style, iconFont.Size, Point.Empty, StringFormat.GenericTypographic);
                    var iconBounds = iconPath.GetBounds();
                    float iconW = iconBounds.Width;
                    float iconH = iconBounds.Height;

                    using (var sfText = new StringFormat(StringFormat.GenericTypographic) { FormatFlags = StringFormatFlags.NoWrap })
                    {
                        var textSizeF = g.MeasureString(Text, Font, int.MaxValue, sfText);
                        float textW = textSizeF.Width;
                        float textH = textSizeF.Height;

                        float totalWidth = iconW + _iconSpacing + textW;
                        float startX = (Width - totalWidth) / 2f;
                        float midY = Height / 2f;

                        float iconX, textX;
                        if (!_iconOnRight) { iconX = startX; textX = startX + iconW + _iconSpacing; }
                        else { textX = startX; iconX = startX + textW + _iconSpacing; }

                        using (var m = new Matrix()) { m.Translate(iconX - iconBounds.X, midY - iconH / 2f - iconBounds.Y); iconPath.Transform(m); }
                        using (var b = new SolidBrush(color)) g.FillPath(b, iconPath);
                        using (var b = new SolidBrush(color)) g.DrawString(Text, Font, b, new PointF(textX, midY - textH / 2f), sfText);
                    }
                }
            }
            catch { }
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

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }
        protected override void OnParentBackColorChanged(EventArgs e) { base.OnParentBackColorChanged(e); Invalidate(); }
    }
}