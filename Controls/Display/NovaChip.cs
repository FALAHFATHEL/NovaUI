using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("Removed")]
    [Description("رقاقة قابلة للحذف (Chip / Tag) — مع أيقونة اختيارية وزر حذف.")]
    public class NovaChip : Control
    {
        private string _chipText = "وسم";
        private string _iconGlyph = "";
        private bool _removable = true;
        private bool _hover, _closeHover;

        private Color _fillColor = Color.FromArgb(241, 245, 249);
        private Color _hoverFillColor = Color.FromArgb(226, 232, 240);
        private Color _textColor = Color.FromArgb(51, 65, 85);
        private Color _iconColor = Color.FromArgb(100, 116, 139);
        private Color _closeColor = Color.FromArgb(100, 116, 139);
        private Color _closeHoverColor = Color.FromArgb(239, 68, 68);
        private Color _borderColor = Color.Transparent;
        private int _borderSize = 0;
        private int _borderRadius = -1;
        private int _paddingLeft = 14;
        private int _paddingRight = 4;
        private int _iconSpacing = 6;
        private int _iconSize = 12;
        private Font _chipFont = new Font("Segoe UI", 9.5F);

        private Rectangle _closeRect;
        private Rectangle _iconRect;

        public event EventHandler Removed;
        public event EventHandler ChipClicked;

        public NovaChip()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(90, 30);
            Font = _chipFont;
            Cursor = Cursors.Hand;
        }

        #region Properties

        [Category("NovaChip"), DefaultValue("وسم")]
        public string ChipText
        {
            get => _chipText;
            set { _chipText = value ?? ""; AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue("")]
        [Description("رمز أيقونة اختيارية قبل النص.")]
        public string IconGlyph
        {
            get => _iconGlyph;
            set { _iconGlyph = value ?? ""; AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue(12)]
        public int IconSize
        {
            get => _iconSize;
            set { _iconSize = Math.Max(6, value); AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue(6)]
        public int IconSpacing
        {
            get => _iconSpacing;
            set { _iconSpacing = Math.Max(0, value); AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue(true)]
        public bool Removable
        {
            get => _removable;
            set { _removable = value; AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaChip")]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        [Category("NovaChip")]
        public Color HoverFillColor
        {
            get => _hoverFillColor;
            set { _hoverFillColor = value; Invalidate(); }
        }

        [Category("NovaChip")]
        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [Category("NovaChip")]
        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; Invalidate(); }
        }

        [Category("NovaChip")]
        public Color CloseColor
        {
            get => _closeColor;
            set { _closeColor = value; Invalidate(); }
        }

        [Category("NovaChip")]
        public Color CloseHoverColor
        {
            get => _closeHoverColor;
            set { _closeHoverColor = value; Invalidate(); }
        }

        [Category("NovaChip")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue(0)]
        public int BorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue(-1)]
        [Description("استدارة الزوايا. -1 = كبسولة تلقائية.")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue(14)]
        public int PaddingLeft
        {
            get => _paddingLeft;
            set { _paddingLeft = Math.Max(2, value); AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaChip"), DefaultValue(4)]
        public int PaddingRight
        {
            get => _paddingRight;
            set { _paddingRight = Math.Max(2, value); AutoSizeControl(); Invalidate(); }
        }

        [Category("NovaChip")]
        public Font ChipFont
        {
            get => _chipFont;
            set { _chipFont = value ?? new Font("Segoe UI", 9.5F); Font = _chipFont; AutoSizeControl(); Invalidate(); }
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
            var textSize = TextRenderer.MeasureText(_chipText ?? "", _chipFont);
            int iconW = string.IsNullOrEmpty(_iconGlyph) ? 0 : _iconSize + _iconSpacing;
            int closeW = _removable ? 24 : 0;

            Height = textSize.Height + 12;
            Width = _paddingLeft + iconW + textSize.Width + closeW + _paddingRight;
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

            bool rtl = RightToLeft == RightToLeft.Yes;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = _borderRadius < 0
                ? Height / 2
                : Math.Min(_borderRadius, Height / 2);

            // الخلفية + الحدود
            Color fill = _hover ? _hoverFillColor : _fillColor;
            using (var path = Round(rect, radius))
            {
                using (var b = new SolidBrush(fill))
                    g.FillPath(b, path);

                if (_borderSize > 0)
                    using (var p = new Pen(_borderColor, _borderSize))
                    {
                        p.Alignment = PenAlignment.Inset;
                        g.DrawPath(p, path);
                    }
            }

            // حساب مناطق المحتوى
            int contentLeft = _paddingLeft;
            int contentRight = Width - _paddingRight;

            Rectangle iconRect = Rectangle.Empty;
            Rectangle closeRect = Rectangle.Empty;
            Rectangle textRect;

            if (rtl)
            {
                // في RTL: الأيقونة يمين، زر الإغلاق يسار
                if (_removable)
                {
                    closeRect = new Rectangle(_paddingRight, (Height - 20) / 2, 20, 20);
                    contentLeft += 24;
                }
                if (!string.IsNullOrEmpty(_iconGlyph))
                {
                    iconRect = new Rectangle(contentRight - _iconSize - 4, (Height - _iconSize) / 2 - 2, _iconSize + 4, _iconSize + 4);
                    contentRight -= _iconSize + _iconSpacing;
                }
                textRect = new Rectangle(contentLeft, 0, Math.Max(10, contentRight - contentLeft), Height);
            }
            else
            {
                // في LTR: الأيقونة يسار، زر الإغلاق يمين
                if (!string.IsNullOrEmpty(_iconGlyph))
                {
                    iconRect = new Rectangle(contentLeft, (Height - _iconSize) / 2 - 2, _iconSize + 4, _iconSize + 4);
                    contentLeft += _iconSize + _iconSpacing;
                }
                if (_removable)
                {
                    closeRect = new Rectangle(Width - _paddingRight - 20, (Height - 20) / 2, 20, 20);
                    contentRight -= 24;
                }
                textRect = new Rectangle(contentLeft, 0, Math.Max(10, contentRight - contentLeft), Height);
            }

            // الأيقونة
            if (!string.IsNullOrEmpty(_iconGlyph) && !iconRect.IsEmpty)
            {
                using (var f = new Font("Segoe MDL2 Assets", _iconSize))
                using (var b = new SolidBrush(_iconColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                    g.DrawString(_iconGlyph, f, b, iconRect, sf);
            }

            // النص
            using (var b = new SolidBrush(_textColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = rtl ? StringFormatFlags.DirectionRightToLeft : 0
            })
                g.DrawString(_chipText ?? "", _chipFont, b, textRect, sf);

            // زر الحذف
            if (_removable && !closeRect.IsEmpty)
            {
                _closeRect = closeRect;

                if (_closeHover)
                {
                    using (var b = new SolidBrush(Color.FromArgb(40, _closeHoverColor)))
                    using (var path = Round(closeRect, closeRect.Width / 2))
                        g.FillPath(b, path);
                }

                Color cc = _closeHover ? _closeHoverColor : _closeColor;
                using (var pen = new Pen(cc, 1.7f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    int cx = closeRect.X + closeRect.Width / 2;
                    int cy = closeRect.Y + closeRect.Height / 2;
                    int s = 4;
                    g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
                    g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
                }
            }
            else
            {
                _closeRect = Rectangle.Empty;
            }
        }

        #endregion

        #region Mouse

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _closeHover = false;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_removable) return;

            bool h = _closeRect.Contains(e.Location);
            if (h != _closeHover)
            {
                _closeHover = h;
                Invalidate(_closeRect);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;

            if (_removable && _closeRect.Contains(e.Location))
            {
                Removed?.Invoke(this, EventArgs.Empty);
                return;
            }

            ChipClicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            Invalidate();
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