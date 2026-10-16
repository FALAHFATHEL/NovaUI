using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    public enum BadgeStyle { Solid, Soft, Outline }

    [ToolboxItem(true)]
    [Description("شارة صغيرة لعرض الحالة أو العدّاد.")]
    public class NovaBadge : Control
    {
        private string _badgeText = "جديد";
        private BadgeStyle _style = BadgeStyle.Solid;
        private Color _accentColor = Color.FromArgb(99, 102, 241);
        private Color _textColor = Color.White;
        private Color _softTextColor = Color.Empty;
        private int _padding = 12;
        private int _borderRadius = -1;
        private int _borderSize = 1;
        private Font _badgeFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        public NovaBadge()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(60, 26);
            Font = _badgeFont;
        }

        #region Properties

        [Category("NovaBadge"), DefaultValue("جديد")]
        public string BadgeText
        {
            get => _badgeText;
            set { _badgeText = value ?? ""; AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaBadge"), DefaultValue(BadgeStyle.Solid)]
        public BadgeStyle Style
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        [Category("NovaBadge")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        [Category("NovaBadge")]
        [Description("لون النص في نمط Solid.")]
        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [Category("NovaBadge")]
        [Description("لون النص في Soft/Outline. اتركه فارغاً لاستخدام AccentColor.")]
        public Color SoftTextColor
        {
            get => _softTextColor;
            set { _softTextColor = value; Invalidate(); }
        }

        [Category("NovaBadge"), DefaultValue(12)]
        public int Padding_Horizontal
        {
            get => _padding;
            set { _padding = Math.Max(2, value); AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaBadge"), DefaultValue(-1)]
        [Description("استدارة الزوايا. -1 = كبسولة تلقائية.")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        [Category("NovaBadge"), DefaultValue(1)]
        [Description("سماكة الحدود (لنمط Outline فقط).")]
        public int BorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaBadge")]
        public Font BadgeFont
        {
            get => _badgeFont;
            set { _badgeFont = value ?? new Font("Segoe UI", 8.5F, FontStyle.Bold); Font = _badgeFont; AutoSizeControl(); Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = Color.Transparent;
        }

        #endregion

        #region AutoSize

        private void AutoSizeControl()
        {
            var size = TextRenderer.MeasureText(_badgeText ?? "", _badgeFont);
            Width = size.Width + _padding * 2;
            Height = size.Height + 8;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            AutoSizeControl();
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (Width <= 2 || Height <= 2) return;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = _borderRadius < 0
                ? Height / 2
                : Math.Min(_borderRadius, Height / 2);

            using (var path = Round(rect, radius))
            {
                switch (_style)
                {
                    case BadgeStyle.Solid:
                        using (var b = new SolidBrush(_accentColor))
                            g.FillPath(b, path);
                        break;

                    case BadgeStyle.Soft:
                        using (var b = new SolidBrush(Color.FromArgb(35, _accentColor)))
                            g.FillPath(b, path);
                        break;

                    case BadgeStyle.Outline:
                        using (var p = new Pen(_accentColor, _borderSize))
                        {
                            p.Alignment = PenAlignment.Inset;
                            g.DrawPath(p, path);
                        }
                        break;
                }
            }

            Color textCol = _style == BadgeStyle.Solid
                ? _textColor
                : (_softTextColor != Color.Empty ? _softTextColor : _accentColor);

            using (var b = new SolidBrush(textCol))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString(_badgeText ?? "", _badgeFont, b, ClientRectangle, sf);
            }
        }

        #endregion

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
    }
}