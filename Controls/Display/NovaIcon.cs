using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultProperty("IconGlyph")]
    [Description("أيقونة احترافية من معرض NovaUI مع محاذاة كاملة ودعم RTL.")]
    public class NovaIcon : Control
    {
        private string _iconGlyph = "\uE77B";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";
        private int _iconSize = 24;
        private Color _iconColor = Color.FromArgb(99, 102, 241);
        private ContentAlignment _iconAlign = ContentAlignment.MiddleCenter;

        public NovaIcon()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(32, 32);
            TabStop = false;
        }

        [Category("NovaIcon")]
        [DefaultValue("\uE77B")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        public string IconGlyph { get { return _iconGlyph; } set { _iconGlyph = value ?? ""; Invalidate(); } }

        [Category("NovaIcon"), DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName { get { return _iconFontName; } set { _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); } }

        [Category("NovaIcon"), DefaultValue("")]
        public string IconPackName { get { return _iconPackName; } set { _iconPackName = value ?? ""; Invalidate(); } }

        [Category("NovaIcon"), DefaultValue(24)]
        public int IconSize { get { return _iconSize; } set { _iconSize = Math.Max(6, value); Invalidate(); } }

        [Category("NovaIcon")]
        public Color IconColor { get { return _iconColor; } set { _iconColor = value; Invalidate(); } }

        [Category("NovaIcon"), DefaultValue(ContentAlignment.MiddleCenter)]
        public ContentAlignment IconAlign { get { return _iconAlign; } set { _iconAlign = value; Invalidate(); } }

        private bool IsRtlEffective()
        {
            if (RightToLeft == RightToLeft.Yes) return true;
            if (RightToLeft == RightToLeft.No) return false;
            Control p = Parent;
            while (p != null)
            {
                if (p.RightToLeft == RightToLeft.Yes) return true;
                if (p.RightToLeft == RightToLeft.No) return false;
                p = p.Parent;
            }
            return false;
        }

        private string GetIconFontKey()
        {
            if (!string.IsNullOrEmpty(_iconPackName)) return _iconPackName;
            if (!string.IsNullOrEmpty(_iconFontName)) return _iconFontName;
            return "Segoe MDL2 Assets";
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

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawParentBackground(g);

            if (Width <= 0 || Height <= 0) return;
            if (string.IsNullOrEmpty(_iconGlyph)) return;

            var align = _iconAlign;
            if (IsRtlEffective())
            {
                switch (align)
                {
                    case ContentAlignment.TopLeft: align = ContentAlignment.TopRight; break;
                    case ContentAlignment.MiddleLeft: align = ContentAlignment.MiddleRight; break;
                    case ContentAlignment.BottomLeft: align = ContentAlignment.BottomRight; break;
                    case ContentAlignment.TopRight: align = ContentAlignment.TopLeft; break;
                    case ContentAlignment.MiddleRight: align = ContentAlignment.MiddleLeft; break;
                    case ContentAlignment.BottomRight: align = ContentAlignment.BottomLeft; break;
                }
            }

            int x;
            switch (align)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft: x = 0; break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter: x = (Width - _iconSize) / 2; break;
                default: x = Width - _iconSize; break;
            }

            int y;
            switch (align)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight: y = 0; break;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight: y = (Height - _iconSize) / 2; break;
                default: y = Height - _iconSize; break;
            }

            var iconRect = new Rectangle(x, y, _iconSize, _iconSize);

            try
            {
                using (var f = FontManager.GetIconFont(GetIconFontKey(), _iconSize))
                using (var b = new SolidBrush(_iconColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoClip
                })
                {
                    g.DrawString(_iconGlyph, f, b, iconRect, sf);
                }
            }
            catch { }
        }

        protected override void OnRightToLeftChanged(EventArgs e) { base.OnRightToLeftChanged(e); Invalidate(); }
        protected override void OnParentChanged(EventArgs e) { base.OnParentChanged(e); Invalidate(); }
    }
}