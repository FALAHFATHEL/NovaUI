using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("CheckedChanged")]
    [Description("مربع اختيار احترافي بعلامة صح متحركة ودعم كامل لـ RTL و LTR وشفافية كاملة.")]
    public class NovaCheckBox : Control
    {
        private bool _checked;
        private float _checkProgress;
        private bool _hover;

        private int _boxSize = 22;
        private int _radius = 6;
        private int _boxSpacing = 10;

        private Color _uncheckedFill = Color.White;
        private Color _uncheckedBorder = Color.FromArgb(203, 213, 225);
        private Color _checkedFill = Color.FromArgb(99, 102, 241);
        private Color _checkedBorder = Color.FromArgb(99, 102, 241);
        private Color _hoverBorder = Color.FromArgb(99, 102, 241);
        private Color _checkMarkColor = Color.White;
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _disabledColor = Color.FromArgb(203, 213, 225);

        public event EventHandler CheckedChanged;

        public NovaCheckBox()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 10F);
            ForeColor = _textColor;
            Text = "Option";
            AutoSize = true;  
        }

        [Category("NovaCheckBox"), DefaultValue(false)]
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                AnimateCheck();
                var h = CheckedChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        [Category("NovaCheckBox"), DefaultValue(22)]
        public int BoxSize { get => _boxSize; set { _boxSize = Math.Max(12, value); PerformLayout(); Invalidate(); } }

        [Category("NovaCheckBox"), DefaultValue(6)]
        public int BoxRadius { get => _radius; set { _radius = Math.Max(0, value); Invalidate(); } }

        [Category("NovaCheckBox"), DefaultValue(10)]
        public int BoxSpacing { get => _boxSpacing; set { _boxSpacing = Math.Max(0, value); PerformLayout(); Invalidate(); } }

        [Category("NovaCheckBox")] public Color UncheckedFill { get => _uncheckedFill; set { _uncheckedFill = value; Invalidate(); } }
        [Category("NovaCheckBox")] public Color UncheckedBorder { get => _uncheckedBorder; set { _uncheckedBorder = value; Invalidate(); } }
        [Category("NovaCheckBox")] public Color CheckedFill { get => _checkedFill; set { _checkedFill = value; Invalidate(); } }
        [Category("NovaCheckBox")] public Color CheckedBorder { get => _checkedBorder; set { _checkedBorder = value; Invalidate(); } }
        [Category("NovaCheckBox")] public Color HoverBorder { get => _hoverBorder; set { _hoverBorder = value; Invalidate(); } }
        [Category("NovaCheckBox")] public Color CheckMarkColor { get => _checkMarkColor; set { _checkMarkColor = value; Invalidate(); } }
        [Category("NovaCheckBox")]
        public Color TextColor { get => _textColor; set { _textColor = value; ForeColor = value; Invalidate(); } }
        [Category("NovaCheckBox")] public Color DisabledColor { get => _disabledColor; set { _disabledColor = value; Invalidate(); } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Cursor Cursor { get { return base.Cursor; } set { base.Cursor = value; } }

        // ————— AutoSize / Layout —————
        public override Size GetPreferredSize(Size proposedSize)
        {
            int h = Math.Max(_boxSize, Font.Height) + Padding.Vertical;

            if (string.IsNullOrEmpty(Text))
                return new Size(_boxSize + Padding.Horizontal, h);

            var flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
            if (IsRtlEffective()) flags |= TextFormatFlags.RightToLeft;

            Size textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, h), flags);

            int w = _boxSize + _boxSpacing + textSize.Width + Padding.Horizontal;
            return new Size(w, h);
        }

        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); PerformLayout(); Invalidate(); }
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); PerformLayout(); Invalidate(); }
        protected override void OnRightToLeftChanged(EventArgs e) { base.OnRightToLeftChanged(e); PerformLayout(); Invalidate(); }
        protected override void OnParentChanged(EventArgs e) { base.OnParentChanged(e); PerformLayout(); Invalidate(); }
        protected override void OnParentBackColorChanged(EventArgs e) { Invalidate(); base.OnParentBackColorChanged(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        // ————— RTL —————
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

        private void AnimateCheck()
        {
            Animator.Animate(this, _checkProgress, _checked ? 1f : 0f, 180,
                v => { _checkProgress = v; Invalidate(); },
                null, Animator.EaseOutBack);
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
            g.CompositingQuality = CompositingQuality.HighQuality;

            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            bool isRtl = IsRtlEffective();

            int contentLeft = Padding.Left;
            int contentRight = Width - Padding.Right;
            int boxY = Padding.Top + (Height - Padding.Vertical - _boxSize) / 2;
            int boxX = isRtl ? contentRight - _boxSize : contentLeft;
            var boxRect = new Rectangle(boxX, boxY, _boxSize, _boxSize);

            Color fill = (_checked || _checkProgress > 0.5f)
                       ? (Enabled ? _checkedFill : _disabledColor)
                       : (Enabled ? _uncheckedFill : Color.FromArgb(248, 250, 252));

            Color border = !Enabled ? _disabledColor
                         : _checked ? _checkedBorder
                         : _hover ? _hoverBorder
                                    : _uncheckedBorder;

            using (var path = Round(boxRect, _radius))
            using (var b = new SolidBrush(fill))
            using (var p = new Pen(border, 1.5f))
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }

            if (_checkProgress > 0.05f && Enabled)
            {
                float cx = boxRect.X + boxRect.Width / 2f;
                float cy = boxRect.Y + boxRect.Height / 2f;
                float s = boxRect.Width * 0.28f;

                using (var pen = new Pen(_checkMarkColor, _boxSize * 0.14f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    var p1 = new PointF(cx - s, cy + s * 0.1f);
                    var p2 = new PointF(cx - s * 0.2f, cy + s);
                    var p3 = new PointF(cx + s, cy - s);

                    float t1 = Math.Min(1f, _checkProgress / 0.5f);
                    float t2 = Math.Max(0f, (_checkProgress - 0.5f) / 0.5f);

                    g.DrawLine(pen, p1, Lerp(p1, p2, t1));
                    if (t2 > 0) g.DrawLine(pen, p2, Lerp(p2, p3, t2));
                }
            }

            if (!string.IsNullOrEmpty(Text))
            {
                int textX = isRtl ? contentLeft : contentLeft + _boxSize + _boxSpacing;
                int textW = Width - Padding.Horizontal - _boxSize - _boxSpacing;
                if (textW < 1) textW = 1;
                var textRect = new Rectangle(textX, Padding.Top, textW, Height - Padding.Vertical);

                using (var b = new SolidBrush(Enabled ? _textColor : _disabledColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = isRtl ? StringFormatFlags.DirectionRightToLeft : 0
                })
                {
                    g.DrawString(Text, Font, b, textRect, sf);
                }
            }
        }

        private static PointF Lerp(PointF a, PointF b, float t)
            => new PointF(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Enabled) Checked = !Checked;
            base.OnMouseDown(e);
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
    }
}