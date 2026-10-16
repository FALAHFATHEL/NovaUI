using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("ValueChanged")]
    public class NovaSlider : Control
    {
        private int _minimum = 0, _maximum = 100, _value = 50;
        private bool _dragging, _hover;
        private int _trackHeight = 6, _thumbSize = 20;
        private Color _trackColor = Color.FromArgb(226, 232, 240);
        private Color _progressColor = Color.FromArgb(99, 102, 241);
        private Color _thumbColor = Color.White;
        private Color _thumbBorderColor = Color.FromArgb(99, 102, 241);
        private Color _hoverThumbColor = Color.FromArgb(238, 242, 255);
        private bool _showTooltip;
        private Color _tooltipBack = Color.FromArgb(15, 23, 42);
        private Color _tooltipText = Color.White;

        public event EventHandler ValueChanged;

        public NovaSlider()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(280, 40);
            Cursor = Cursors.Hand;
        }

        [Category("NovaSlider"), DefaultValue(0)] public int Minimum { get => _minimum; set { _minimum = value; if (_maximum < _minimum) _maximum = _minimum; Clamp(); Invalidate(); } }
        [Category("NovaSlider"), DefaultValue(100)] public int Maximum { get => _maximum; set { _maximum = Math.Max(value, _minimum); Clamp(); Invalidate(); } }
        [Category("NovaSlider"), DefaultValue(50)]
        public int Value
        {
            get => _value;
            set { int v = Math.Max(_minimum, Math.Min(_maximum, value)); if (_value == v) return; _value = v; Invalidate(); ValueChanged?.Invoke(this, EventArgs.Empty); }
        }
        [Category("NovaSlider"), DefaultValue(6)] public int TrackHeight { get => _trackHeight; set { _trackHeight = Math.Max(2, value); Invalidate(); } }
        [Category("NovaSlider"), DefaultValue(20)] public int ThumbSize { get => _thumbSize; set { _thumbSize = Math.Max(8, value); Invalidate(); } }
        [Category("NovaSlider")] public Color TrackColor { get => _trackColor; set { _trackColor = value; Invalidate(); } }
        [Category("NovaSlider")] public Color ProgressColor { get => _progressColor; set { _progressColor = value; Invalidate(); } }
        [Category("NovaSlider")] public Color ThumbColor { get => _thumbColor; set { _thumbColor = value; Invalidate(); } }
        [Category("NovaSlider")] public Color ThumbBorderColor { get => _thumbBorderColor; set { _thumbBorderColor = value; Invalidate(); } }
        [Category("NovaSlider")] public Color HoverThumbColor { get => _hoverThumbColor; set { _hoverThumbColor = value; Invalidate(); } }
        [Category("NovaSlider"), DefaultValue(false)] public bool ShowTooltip { get => _showTooltip; set { _showTooltip = value; Invalidate(); } }
        [Category("NovaSlider")] public Color TooltipBackColor { get => _tooltipBack; set { _tooltipBack = value; Invalidate(); } }
        [Category("NovaSlider")] public Color TooltipTextColor { get => _tooltipText; set { _tooltipText = value; Invalidate(); } }

        private void Clamp() { if (_value < _minimum) _value = _minimum; if (_value > _maximum) _value = _maximum; }
        private float Pct() { if (_maximum == _minimum) return 0f; return (float)(_value - _minimum) / (_maximum - _minimum); }
        private int PctToValue(int x) { int u = Width - _thumbSize; if (u <= 0) return _minimum; float p = (float)(x - _thumbSize / 2) / u; p = Math.Max(0f, Math.Min(1f, p)); return _minimum + (int)Math.Round(p * (_maximum - _minimum)); }

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

            int cy = Height / 2, trackX = _thumbSize / 2, trackW = Width - _thumbSize;
            int trackY = cy - _trackHeight / 2;

            var trackRect = new Rectangle(trackX, trackY, trackW, _trackHeight);
            using (var path = Round(trackRect, _trackHeight / 2))
            using (var b = new SolidBrush(_trackColor)) g.FillPath(b, path);

            float pct = Pct();
            int progW = (int)(trackW * pct);
            if (progW > 0)
            {
                var progRect = new Rectangle(trackX, trackY, progW, _trackHeight);
                using (var path = Round(progRect, _trackHeight / 2))
                using (var b = new SolidBrush(_progressColor)) g.FillPath(b, path);
            }

            int thumbX = trackX + progW - _thumbSize / 2;
            int thumbY = cy - _thumbSize / 2;
            var thumbRect = new Rectangle(thumbX, thumbY, _thumbSize, _thumbSize);

            using (var b = new SolidBrush(_hover || _dragging ? _hoverThumbColor : _thumbColor))
                g.FillEllipse(b, thumbRect);

            using (var p = new Pen(_thumbBorderColor, 2f))
                g.DrawEllipse(p, thumbRect);

            int dotSize = _thumbSize / 3;
            using (var b = new SolidBrush(_thumbBorderColor))
                g.FillEllipse(b, thumbRect.X + (_thumbSize - dotSize) / 2, thumbRect.Y + (_thumbSize - dotSize) / 2, dotSize, dotSize);

            if (_showTooltip && (_hover || _dragging))
            {
                string text = _value.ToString();
                var ts = TextRenderer.MeasureText(text, new Font("Segoe UI", 9F, FontStyle.Bold));
                int tw = ts.Width + 16, th = 26;
                int tx = thumbRect.X + _thumbSize / 2 - tw / 2;
                int ty = thumbRect.Y - th - 8;
                if (tx < 0) tx = 0;
                if (tx + tw > Width) tx = Width - tw;
                var tr = new Rectangle(tx, ty, tw, th);
                using (var path = Round(tr, 6))
                using (var b = new SolidBrush(_tooltipBack)) g.FillPath(b, path);
                using (var b = new SolidBrush(_tooltipText))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(text, new Font("Segoe UI", 9F, FontStyle.Bold), b, tr, sf);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { _dragging = true; Value = PctToValue(e.X); } base.OnMouseDown(e); }
        protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); if (_dragging) Value = PctToValue(e.X); }
        protected override void OnMouseUp(MouseEventArgs e) { _dragging = false; Invalidate(); base.OnMouseUp(e); }

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
