using NovaUI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    public enum ProgressStyle { Solid, Striped, Indeterminate, Gradient }

    [ToolboxItem(true)]
    [DefaultEvent("ValueChanged")]
    public class NovaProgressBar : Control
    {
        private int _minimum, _maximum = 100, _value;
        private ProgressStyle _style = ProgressStyle.Solid;
        private int _radius = 8, _thickness = 0;
        private Color _trackColor = Color.FromArgb(226, 232, 240);
        private Color _fillColor = Color.FromArgb(99, 102, 241);
        private Color _fillColor2 = Color.FromArgb(139, 92, 246);
        private Color _stripColor = Color.FromArgb(220, 255, 255, 255);
        private int _stripWidth = 12;
        private bool _showPercentage;
        private Color _percentageColor = Color.White;
        private Timer _animTimer;
        private int _animOffset;

        public event EventHandler ValueChanged;

        public NovaProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(300, 14);
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _animTimer = new Timer { Interval = 30 };
            _animTimer.Tick += (s, e) => { _animOffset = (_animOffset + 2) % _stripWidth; Invalidate(); };
        }

        [Category("NovaProgressBar"), DefaultValue(0)] public int Minimum { get => _minimum; set { _minimum = value; if (_maximum < _minimum) _maximum = _minimum; Value = Clamp(_value); } }
        [Category("NovaProgressBar"), DefaultValue(100)] public int Maximum { get => _maximum; set { _maximum = Math.Max(value, _minimum + 1); Value = Clamp(_value); } }
        [Category("NovaProgressBar"), DefaultValue(0)] public int Value { get => _value; set { int v = Clamp(value); if (_value == v) return; _value = v; Invalidate(); ValueChanged?.Invoke(this, EventArgs.Empty); } }
        [Category("NovaProgressBar"), DefaultValue(ProgressStyle.Solid)]
        public ProgressStyle Style { get => _style; set { _style = value; UpdateTimerState(); Invalidate(); } }
        [Category("NovaProgressBar"), DefaultValue(8)] public int BorderRadius { get => _radius; set { _radius = Math.Max(0, value); Invalidate(); } }
        [Category("NovaProgressBar"), DefaultValue(0)] public int Thickness { get => _thickness; set { _thickness = Math.Max(0, value); Invalidate(); } }
        [Category("NovaProgressBar")] public Color TrackColor { get => _trackColor; set { _trackColor = value; Invalidate(); } }
        [Category("NovaProgressBar")] public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        [Category("NovaProgressBar")] public Color FillColor2 { get => _fillColor2; set { _fillColor2 = value; Invalidate(); } }
        [Category("NovaProgressBar")] public Color StripColor { get => _stripColor; set { _stripColor = value; Invalidate(); } }
        [Category("NovaProgressBar"), DefaultValue(12)] public int StripWidth { get => _stripWidth; set { _stripWidth = Math.Max(4, value); Invalidate(); } }
        [Category("NovaProgressBar"), DefaultValue(false)] public bool ShowPercentage { get => _showPercentage; set { _showPercentage = value; Invalidate(); } }
        [Category("NovaProgressBar")] public Color PercentageColor { get => _percentageColor; set { _percentageColor = value; Invalidate(); } }

        private int Clamp(int v) { if (v < _minimum) return _minimum; if (v > _maximum) return _maximum; return v; }

        public void SetValueAnimated(int target, int durationMs = 500)
        {
            int start = _value, t = Clamp(target);
            Animator.Animate(this, start, t, durationMs, v => { _value = (int)Math.Round(v); Invalidate(); },
                () => { _value = t; Invalidate(); ValueChanged?.Invoke(this, EventArgs.Empty); }, Animator.EaseOutCubic);
        }

        private void UpdateTimerState()
        {
            if (_style == ProgressStyle.Indeterminate && Visible && !DesignMode) _animTimer.Start();
            else _animTimer.Stop();
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

            // رسم خلفية الحاوية بدقة لتنعيم حواف شريط التقدم وإزالة الحدود البيضاء
            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            int barH = _thickness > 0 ? Math.Min(_thickness, Height) : Height;
            int y = (Height - barH) / 2;
            var trackRect = new Rectangle(0, y, Width, barH);
            int r = Math.Min(_radius, barH / 2);

            using (var path = Round(trackRect, r))
            using (var b = new SolidBrush(_trackColor)) g.FillPath(b, path);

            if (_style == ProgressStyle.Indeterminate) DrawIndeterminate(g, trackRect, r);
            else if (_maximum > _minimum && _value > _minimum)
            {
                float pct = (float)(_value - _minimum) / (_maximum - _minimum);
                int fillW = (int)(trackRect.Width * pct);
                if (fillW > 2)
                {
                    var fillRect = new Rectangle(trackRect.X, trackRect.Y, fillW, barH);
                    using (var clipPath = Round(trackRect, r))
                    {
                        var oldClip = g.Clip; g.SetClip(clipPath);
                        if (_style == ProgressStyle.Gradient)
                            using (var brush = new LinearGradientBrush(fillRect, _fillColor, _fillColor2, 0f)) g.FillRectangle(brush, fillRect);
                        else
                        {
                            using (var b = new SolidBrush(_fillColor)) g.FillRectangle(b, fillRect);
                            if (_style == ProgressStyle.Striped) DrawStripes(g, fillRect);
                        }
                        g.Clip = oldClip;
                    }
                }
            }

            if (_showPercentage && _style != ProgressStyle.Indeterminate)
            {
                int pct = _maximum > _minimum ? (int)Math.Round(100.0 * (_value - _minimum) / (_maximum - _minimum)) : 0;
                using (var b = new SolidBrush(_percentageColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(pct + "%", Font, b, new RectangleF(trackRect.X, trackRect.Y, trackRect.Width, barH), sf);
            }
        }

        private void DrawStripes(Graphics g, Rectangle fillRect)
        {
            using (var b = new SolidBrush(_stripColor))
                for (int x = fillRect.X - _stripWidth + _animOffset; x < fillRect.Right; x += _stripWidth)
                {
                    var stripe = new Point[] { new Point(x, fillRect.Bottom), new Point(x + _stripWidth / 2, fillRect.Bottom), new Point(x + _stripWidth, fillRect.Top), new Point(x + _stripWidth / 2, fillRect.Top) };
                    g.FillPolygon(b, stripe);
                }
        }

        private void DrawIndeterminate(Graphics g, Rectangle trackRect, int radius)
        {
            int blockW = Math.Max(60, trackRect.Width / 4);
            int travel = trackRect.Width + blockW;
            int pos = (_animOffset * 4) % travel;
            int blockX = pos - blockW;
            using (var clipPath = Round(trackRect, radius))
            {
                var oldClip = g.Clip; g.SetClip(clipPath);
                using (var brush = new LinearGradientBrush(new Rectangle(blockX, trackRect.Y, blockW, trackRect.Height), Color.FromArgb(0, _fillColor), _fillColor, LinearGradientMode.Horizontal))
                {
                    brush.InterpolationColors = new ColorBlend { Positions = new[] { 0f, 0.5f, 1f }, Colors = new[] { Color.FromArgb(0, _fillColor), _fillColor, Color.FromArgb(0, _fillColor) } };
                    g.FillRectangle(brush, new Rectangle(blockX, trackRect.Y, blockW, trackRect.Height));
                }
                g.Clip = oldClip;
            }
        }

        protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); UpdateTimerState(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); UpdateTimerState(); }
        protected override void OnHandleDestroyed(EventArgs e) { _animTimer?.Stop(); base.OnHandleDestroyed(e); }
        protected override void Dispose(bool disposing) { if (disposing) { _animTimer?.Stop(); _animTimer?.Dispose(); } base.Dispose(disposing); }

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
