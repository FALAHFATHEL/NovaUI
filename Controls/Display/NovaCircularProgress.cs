using NovaUI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    public class NovaCircularProgress : Control
    {
        private int _value, _maximum = 100, _thickness = 8;
        private Color _progressColor = Color.FromArgb(99, 102, 241);
        private Color _trackColor = Color.FromArgb(226, 232, 240);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private bool _showPercentage = true;
        private bool _roundedCaps = true;
        private int _animatedValue;
        private bool _animating;

        public NovaCircularProgress()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(120, 120);
            Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        }

        [Category("NovaCircularProgress"), DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                int v = Math.Max(0, Math.Min(_maximum, value));
                if (_value == v) return;
                int from = _animating ? _animatedValue : _value;
                _value = v; _animating = true;
                Animator.Animate(this, from, _value, 400, f => { _animatedValue = (int)f; Invalidate(); }, () => _animating = false, Animator.EaseOutCubic);
            }
        }

        [Category("NovaCircularProgress"), DefaultValue(100)] public int Maximum { get => _maximum; set { _maximum = Math.Max(1, value); Invalidate(); } }
        [Category("NovaCircularProgress"), DefaultValue(8)] public int Thickness { get => _thickness; set { _thickness = Math.Max(2, value); Invalidate(); } }
        [Category("NovaCircularProgress")] public Color ProgressColor { get => _progressColor; set { _progressColor = value; Invalidate(); } }
        [Category("NovaCircularProgress")] public Color TrackColor { get => _trackColor; set { _trackColor = value; Invalidate(); } }
        [Category("NovaCircularProgress")] public Color TextColor { get => _textColor; set { _textColor = value; Invalidate(); } }
        [Category("NovaCircularProgress"), DefaultValue(true)] public bool ShowPercentage { get => _showPercentage; set { _showPercentage = value; Invalidate(); } }
        [Category("NovaCircularProgress"), DefaultValue(true)] public bool RoundedCaps { get => _roundedCaps; set { _roundedCaps = value; Invalidate(); } }

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

            int size = Math.Min(Width, Height) - _thickness - 2;
            if (size <= 4) return;
            var rect = new Rectangle((Width - size) / 2, (Height - size) / 2, size, size);

            using (var track = new Pen(_trackColor, _thickness))
            {
                if (_roundedCaps) { track.StartCap = track.EndCap = LineCap.Round; }
                g.DrawArc(track, rect, 0, 360);
            }
            float sweep = 360f * _animatedValue / _maximum;
            if (sweep > 0.1f)
            {
                using (var prog = new Pen(_progressColor, _thickness))
                {
                    if (_roundedCaps) { prog.StartCap = prog.EndCap = LineCap.Round; }
                    g.DrawArc(prog, rect, -90, sweep);
                }
            }
            if (_showPercentage)
            {
                int percent = (int)Math.Round(100.0 * _animatedValue / _maximum);
                using (var brush = new SolidBrush(_textColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(percent + "%", Font, brush, ClientRectangle, sf);
            }
        }
    }
}
