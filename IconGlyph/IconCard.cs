using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    /// <summary>بطاقة أيقونة قابلة لإعادة الاستخدام داخل IconPickerForm.</summary>
    internal sealed class IconCard : Panel
    {
        public string Glyph { get; private set; }
        public string IconName { get; private set; }

        /// <summary>★ اسم الحزمة (يُستخدم لجلب الخط بشكل آمن).</summary>
        public string PackName { get; set; }

        public bool IsSelected { get; set; }

        public event EventHandler Selected;
        public event EventHandler Confirmed;

        private static readonly Color PrimaryColor = Color.FromArgb(99, 102, 241);
        private static readonly Color PrimaryDark = Color.FromArgb(79, 70, 229);
        private static readonly Color CardNormalBg = Color.FromArgb(248, 250, 252);
        private static readonly Color CardHoverBg = Color.FromArgb(238, 242, 255);
        private static readonly Color CardSelectedBg = Color.FromArgb(224, 231, 255);
        private static readonly Color CardBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color CardSelectedBord = Color.FromArgb(99, 102, 241);
        private static readonly Color NameTextColor = Color.FromArgb(100, 116, 139);

        private bool _hover;
        private readonly int _iconSize;

        public IconCard(string name, string glyph, int iconSize)
        {
            IconName = name;
            Glyph = glyph;
            _iconSize = iconSize;

            Width = 104;
            Height = 104;
            Margin = new Padding(6);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            TabStop = true;
            DoubleBuffered = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            Color bg = IsSelected ? CardSelectedBg
                     : _hover ? CardHoverBg
                     : CardNormalBg;

            Color border = IsSelected ? CardSelectedBord : CardBorder;
            float thickness = IsSelected ? 1.6f : 1f;

            using (var path = Rounded(rect, 12))
            {
                using (var b = new SolidBrush(bg))
                    g.FillPath(b, path);
                using (var p = new Pen(border, thickness))
                    g.DrawPath(p, path);
            }

            // ★ Glyph — استخدام AddString لفرض الخط الخاص
            DrawGlyphPrecise(g, rect);

            // Name
            using (var f = new Font("Segoe UI", 7.5F))
            using (var b = new SolidBrush(NameTextColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                g.DrawString(IconName, f, b,
                    new RectangleF(4, Height - 28, Width - 8, 22), sf);
            }

            // Focus dotted rectangle
            if (Focused && !IsSelected)
            {
                using (var p = new Pen(Color.FromArgb(120, PrimaryColor), 1f)
                { DashStyle = DashStyle.Dot })
                using (var path = Rounded(new Rectangle(2, 2, Width - 5, Height - 5), 10))
                    g.DrawPath(p, path);
            }
        }

        /// <summary>
        /// ★ يرسم الرمز بدقة عبر GraphicsPath.AddString مع Scale-to-fit.
        /// </summary>
        private void DrawGlyphPrecise(Graphics g, Rectangle rect)
        {
            if (string.IsNullOrEmpty(Glyph)) return;

            string key = string.IsNullOrEmpty(PackName)
                ? "Segoe MDL2 Assets"
                : PackName;

            Font font = null;
            try
            {
                font = FontManager.GetIconFont(key, _iconSize);
                if (font == null || font.FontFamily == null) return;

                using (var path = new GraphicsPath())
                {
                    using (var sf = StringFormat.GenericTypographic)
                    {
                        sf.FormatFlags |= StringFormatFlags.NoWrap |
                                          StringFormatFlags.NoClip |
                                          StringFormatFlags.MeasureTrailingSpaces;

                        path.AddString(
                            Glyph,
                            font.FontFamily,
                            (int)font.Style,
                            font.Size,
                            PointF.Empty,
                            sf);
                    }

                    var bounds = path.GetBounds();
                    if (bounds.Width < 0.1f || bounds.Height < 0.1f) return;

                    float areaX = 4;
                    float areaY = 8;
                    float areaW = Width - 8;
                    float areaH = Height - 34;

                    // ★ Scale-to-fit
                    float scaleX = bounds.Width > areaW ? areaW / bounds.Width : 1f;
                    float scaleY = bounds.Height > areaH ? areaH / bounds.Height : 1f;
                    float scale = Math.Min(scaleX, scaleY);

                    if (scale < 1f)
                    {
                        using (var m = new Matrix())
                        {
                            m.Scale(scale, scale);
                            path.Transform(m);
                        }
                        bounds = path.GetBounds();
                    }

                    float dx = areaX + (areaW - bounds.Width) / 2f - bounds.X;
                    float dy = areaY + (areaH - bounds.Height) / 2f - bounds.Y;

                    using (var m = new Matrix())
                    {
                        m.Translate(dx, dy);
                        path.Transform(m);
                    }

                    using (var b = new SolidBrush(IsSelected ? PrimaryDark : PrimaryColor))
                        g.FillPath(b, path);
                }
            }
            catch { }
            finally
            {
                if (font != null) font.Dispose();
            }
        }

        private static GraphicsPath Rounded(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
            var h = Selected; if (h != null) h(this, EventArgs.Empty);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            var h = Confirmed; if (h != null) h(this, EventArgs.Empty);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Space) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                var h = Confirmed; if (h != null) h(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    }
}