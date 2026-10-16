using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("CheckedChanged")]
    [Description("زر اختيار دائري احترافي مع حركة سلسة ودعم كامل لـ RTL و LTR.")]
    public class NovaRadioButton : Control
    {
        private bool _checked;
        private float _dotProgress;
        private bool _hover;

        private int _circleSize = 22;
        private int _spacing = 10;

        private Color _uncheckedFill = Color.White;
        private Color _uncheckedBorder = Color.FromArgb(203, 213, 225);
        private Color _checkedFill = Color.White;
        private Color _checkedBorder = Color.FromArgb(99, 102, 241);
        private Color _hoverBorder = Color.FromArgb(99, 102, 241);
        private Color _dotColor = Color.FromArgb(99, 102, 241);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _disabledColor = Color.FromArgb(203, 213, 225);

        public event EventHandler CheckedChanged;

        public NovaRadioButton()
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
            Cursor = Cursors.Hand;
            Text = "خيار";
            AutoSize = true;  
        }

        [Category("NovaRadioButton"), DefaultValue(false)]
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                AnimateDot();
                if (value) UncheckSiblings();
                var h = CheckedChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        [Category("NovaRadioButton"), DefaultValue(22)]
        public int CircleSize { get => _circleSize; set { _circleSize = Math.Max(12, value); PerformLayout(); Invalidate(); } }

        [Category("NovaRadioButton"), DefaultValue(10)]
        public int Spacing { get => _spacing; set { _spacing = Math.Max(0, value); PerformLayout(); Invalidate(); } }

        [Category("NovaRadioButton")] public Color UncheckedFill { get => _uncheckedFill; set { _uncheckedFill = value; Invalidate(); } }
        [Category("NovaRadioButton")] public Color UncheckedBorder { get => _uncheckedBorder; set { _uncheckedBorder = value; Invalidate(); } }
        [Category("NovaRadioButton")] public Color CheckedFill { get => _checkedFill; set { _checkedFill = value; Invalidate(); } }
        [Category("NovaRadioButton")] public Color CheckedBorder { get => _checkedBorder; set { _checkedBorder = value; Invalidate(); } }
        [Category("NovaRadioButton")] public Color HoverBorder { get => _hoverBorder; set { _hoverBorder = value; Invalidate(); } }
        [Category("NovaRadioButton")] public Color DotColor { get => _dotColor; set { _dotColor = value; Invalidate(); } }
        [Category("NovaRadioButton")]
        public Color TextColor { get => _textColor; set { _textColor = value; ForeColor = value; Invalidate(); } }
        [Category("NovaRadioButton")] public Color DisabledColor { get => _disabledColor; set { _disabledColor = value; Invalidate(); } }

        // ————— AutoSize / Layout —————
        public override Size GetPreferredSize(Size proposedSize)
        {
            int h = Math.Max(_circleSize, Font.Height) + Padding.Vertical;

            if (string.IsNullOrEmpty(Text))
                return new Size(_circleSize + Padding.Horizontal, h);

            var flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
            if (IsRtlEffective()) flags |= TextFormatFlags.RightToLeft;

            Size textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, h), flags);

            int w = _circleSize + _spacing + textSize.Width + Padding.Horizontal;
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

        private void AnimateDot()
        {
            Animator.Animate(this, _dotProgress, _checked ? 1f : 0f, 200,
                v => { _dotProgress = v; Invalidate(); },
                null, Animator.EaseOutBack);
        }

        private void UncheckSiblings()
        {
            if (Parent == null) return;
            foreach (Control c in Parent.Controls)
            {
                var rb = c as NovaRadioButton;
                if (rb != null && rb != this && rb.Checked) rb.Checked = false;
            }
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
            int cy = Padding.Top + (Height - Padding.Vertical - _circleSize) / 2;
            int circleX = isRtl ? contentRight - _circleSize : contentLeft;
            var circleRect = new Rectangle(circleX, cy, _circleSize, _circleSize);

            Color fill = Enabled ? _uncheckedFill : Color.FromArgb(248, 250, 252);
            Color border = !Enabled ? _disabledColor
                         : _checked ? _checkedBorder
                         : _hover ? _hoverBorder
                                    : _uncheckedBorder;

            using (var b = new SolidBrush(fill))
            using (var p = new Pen(border, 1.5f))
            {
                g.FillEllipse(b, circleRect);
                g.DrawEllipse(p, circleRect);
            }

            if (_dotProgress > 0.05f && Enabled)
            {
                float maxDot = _circleSize * 0.5f;
                float dotSize = maxDot * _dotProgress;
                float dotX = circleRect.X + (_circleSize - dotSize) / 2f;
                float dotY = circleRect.Y + (_circleSize - dotSize) / 2f;

                using (var b = new SolidBrush(_dotColor))
                    g.FillEllipse(b, dotX, dotY, dotSize, dotSize);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                int textX = isRtl ? contentLeft : contentLeft + _circleSize + _spacing;
                int textW = Width - Padding.Horizontal - _circleSize - _spacing;
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

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Enabled) Checked = true;
            base.OnMouseDown(e);
        }
    }
}