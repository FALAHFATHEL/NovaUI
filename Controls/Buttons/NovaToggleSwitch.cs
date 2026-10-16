using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("CheckedChanged")]
    public class NovaToggleSwitch : Control
    {
        private bool _checked;
        private float _thumbPos;
        private int _radius = -1;
        private Color _onColor = Color.FromArgb(34, 197, 94);
        private Color _offColor = Color.FromArgb(203, 213, 225);
        private Color _thumbColor = Color.White;
        private Color _disabledOn = Color.FromArgb(165, 180, 252);
        private Color _disabledOff = Color.FromArgb(226, 232, 240);
        private bool _hover;

        public event EventHandler CheckedChanged;

        public NovaToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(56, 28);
            Cursor = Cursors.Hand;
        }

        [Category("NovaToggleSwitch"), DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                AnimateThumb();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("NovaToggleSwitch"), DefaultValue(-1)] public int BorderRadius { get => _radius; set { _radius = value; Invalidate(); } }
        [Category("NovaToggleSwitch")] public Color OnColor { get => _onColor; set { _onColor = value; Invalidate(); } }
        [Category("NovaToggleSwitch")] public Color OffColor { get => _offColor; set { _offColor = value; Invalidate(); } }
        [Category("NovaToggleSwitch")] public Color ThumbColor { get => _thumbColor; set { _thumbColor = value; Invalidate(); } }
        [Category("NovaToggleSwitch")] public Color DisabledOnColor { get => _disabledOn; set { _disabledOn = value; Invalidate(); } }
        [Category("NovaToggleSwitch")] public Color DisabledOffColor { get => _disabledOff; set { _disabledOff = value; Invalidate(); } }

        private void AnimateThumb()
        {
            float from = _thumbPos;
            float to = _checked ? 1f : 0f;
            Animator.Animate(this, from, to, 180, v => { _thumbPos = v; Invalidate(); }, null, Animator.EaseOutCubic);
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

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = _radius < 0 ? Height / 2 : Math.Min(_radius, Height / 2);

            Color trackColor;
            if (!Enabled) trackColor = _checked ? _disabledOn : _disabledOff;
            else if (_checked) trackColor = _hover ? Darken(_onColor, 0.92f) : _onColor;
            else trackColor = _hover ? Darken(_offColor, 0.94f) : _offColor;

            using (var path = Round(rect, radius))
            using (var b = new SolidBrush(trackColor)) g.FillPath(b, path);

            int padding = 3;
            int thumbSize = Height - padding * 2;
            int travelRange = Width - thumbSize - padding * 2;
            int thumbX = padding + (int)(travelRange * _thumbPos);

            using (var b = new SolidBrush(_thumbColor))
            {
                g.FillEllipse(b, thumbX, padding, thumbSize, thumbSize);
                using (var shadow = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
                    g.DrawEllipse(shadow, thumbX, padding, thumbSize, thumbSize);
            }
        }

        private static Color Darken(Color c, float factor)
            => Color.FromArgb(c.A, (int)Math.Max(0, c.R * factor), (int)Math.Max(0, c.G * factor), (int)Math.Max(0, c.B * factor));

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left && Enabled) Checked = !Checked; base.OnMouseDown(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); if (Height > 0 && Width < Height * 2) Width = Height * 2; }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }
}
