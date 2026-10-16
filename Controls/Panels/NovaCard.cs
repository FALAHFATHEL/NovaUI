using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    public class NovaCard : Panel
    {
        private int _borderRadius = 16;
        private int _borderSize = 1;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _fillColor = Color.White;
        private string _headerText = "";
        private Font _headerFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        private Color _headerTextColor = Color.FromArgb(15, 23, 42);
        private Color _headerBackColor = Color.Transparent;
        private int _headerHeight = 48;
        private bool _showHeaderSeparator = true;
        private Color _headerSeparatorColor = Color.FromArgb(241, 245, 249);
        private string _headerIcon = "";
        private Color _headerIconColor = Color.FromArgb(99, 102, 241);
        private int _headerIconSize = 14;

        public NovaCard()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            base.BackColor = Color.Transparent;
            Size = new Size(320, 220);
            Font = new Font("Segoe UI", 9.5F);
            Padding = new Padding(16);
        }

        [Category("NovaCard"), DefaultValue(16)] public int BorderRadius { get => _borderRadius; set { _borderRadius = Math.Max(0, value); Invalidate(); } }
        [Category("NovaCard"), DefaultValue(1)] public int BorderSize { get => _borderSize; set { _borderSize = Math.Max(0, value); Invalidate(); } }
        [Category("NovaCard")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("NovaCard")] public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        [Category("NovaCard"), DefaultValue("")] public string HeaderText { get => _headerText; set { _headerText = value ?? ""; Invalidate(); } }
        [Category("NovaCard")] public Font HeaderFont { get => _headerFont; set { _headerFont = value ?? new Font("Segoe UI", 12F, FontStyle.Bold); Invalidate(); } }
        [Category("NovaCard")] public Color HeaderTextColor { get => _headerTextColor; set { _headerTextColor = value; Invalidate(); } }
        [Category("NovaCard")] public Color HeaderBackColor { get => _headerBackColor; set { _headerBackColor = value; Invalidate(); } }
        [Category("NovaCard"), DefaultValue(48)] public int HeaderHeight { get => _headerHeight; set { _headerHeight = Math.Max(0, value); Invalidate(); } }
        [Category("NovaCard"), DefaultValue(true)] public bool ShowHeaderSeparator { get => _showHeaderSeparator; set { _showHeaderSeparator = value; Invalidate(); } }
        [Category("NovaCard")] public Color HeaderSeparatorColor { get => _headerSeparatorColor; set { _headerSeparatorColor = value; Invalidate(); } }
        [Category("NovaCard"), DefaultValue("")] public string HeaderIcon { get => _headerIcon; set { _headerIcon = value ?? ""; Invalidate(); } }
        [Category("NovaCard")] public Color HeaderIconColor { get => _headerIconColor; set { _headerIconColor = value; Invalidate(); } }
        [Category("NovaCard"), DefaultValue(14)] public int HeaderIconSize { get => _headerIconSize; set { _headerIconSize = Math.Max(6, value); Invalidate(); } }

        private int GetRadius()
        {
            if (Width <= 0 || Height <= 0) return 0;
            int max = Math.Min(Width, Height) / 2;
            return _borderRadius < 0 ? max : Math.Min(_borderRadius, max);
        }

        private void DrawParentBackground(Graphics g)
        {
            if (Parent == null) return;

            var state = g.Save();
            g.TranslateTransform(-this.Left, -this.Top);

            using (var pe = new PaintEventArgs(g, Parent.ClientRectangle))
            {
                InvokePaintBackground(Parent, pe);
                InvokePaint(Parent, pe);
            }

            g.Restore(state);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // رسم محتوى الحاوية الأب خلف الكارت لتفعيل الشفافية الناعمة بدون حدود بيضاء
            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = GetRadius();
            using (var path = Round(rect, radius))
            {
                using (var b = new SolidBrush(_fillColor)) g.FillPath(b, path);
                if (_borderSize > 0)
                    using (var p = new Pen(_borderColor, _borderSize)) { p.Alignment = PenAlignment.Inset; g.DrawPath(p, path); }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (_headerHeight <= 0) return;
            if (string.IsNullOrEmpty(_headerText) && string.IsNullOrEmpty(_headerIcon)) return;
            int headerH = Math.Min(_headerHeight, Height);
            bool rtl = RightToLeft == RightToLeft.Yes;
            int radius = GetRadius();

            if (_headerBackColor != Color.Transparent && _headerBackColor != Color.Empty)
            {
                var hr = new Rectangle(0, 0, Width - 1, headerH);
                using (var path = RoundTop(hr, radius))
                using (var b = new SolidBrush(_headerBackColor)) g.FillPath(b, path);
            }

            int iconSize = headerH;
            int textSide = 16;
            Rectangle iconRect = rtl ? new Rectangle(Width - iconSize, 0, iconSize, headerH) : new Rectangle(0, 0, iconSize, headerH);

            if (!string.IsNullOrEmpty(_headerIcon))
            {
                using (var f = new Font("Segoe MDL2 Assets", _headerIconSize))
                using (var b = new SolidBrush(_headerIconColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(_headerIcon, f, b, iconRect, sf);
                textSide = headerH;
            }

            if (!string.IsNullOrEmpty(_headerText))
            {
                Rectangle textRect = rtl ? new Rectangle(12, 0, Width - textSide - 12, headerH) : new Rectangle(textSide, 0, Width - textSide - 12, headerH);
                using (var b = new SolidBrush(_headerTextColor))
                using (var sf = new StringFormat { Alignment = rtl ? StringAlignment.Far : StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                {
                    if (rtl) sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    g.DrawString(_headerText, _headerFont, b, textRect, sf);
                }
            }

            if (_showHeaderSeparator)
                using (var p = new Pen(_headerSeparatorColor, 1))
                    g.DrawLine(p, radius / 2, headerH, Width - radius / 2, headerH);
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        private static GraphicsPath RoundTop(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom); p.CloseFigure(); return p;
        }
    }
}

