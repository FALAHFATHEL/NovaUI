using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [Description("مؤشر تحميل دوّار احترافي مع ألوان وحجم قابلة للتخصيص.")]
    public class NovaLoadingSpinner : Control
    {
        private readonly Timer _timer;

        private Color _spinnerColor = Color.FromArgb(99, 102, 241);
        private Color _trackColor = Color.FromArgb(226, 232, 240);
        private int _thickness = 3;
        private int _speed = 20; // ms / degree
        private float _angle;
        private bool _isSpinning = true;
        private int _arcLength = 90; 

        public NovaLoadingSpinner()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(32, 32);

            _timer = new Timer { Interval = _speed };
            _timer.Tick += (s, e) =>
            {
                _angle = (_angle + 6f) % 360f;
                Invalidate();
            };
        }

        #region Properties

        [Category("NovaLoadingSpinner")]
        public Color SpinnerColor
        {
            get { return _spinnerColor; }
            set { _spinnerColor = value; Invalidate(); }
        }

        [Category("NovaLoadingSpinner")]
        public Color TrackColor
        {
            get { return _trackColor; }
            set { _trackColor = value; Invalidate(); }
        }

        [Category("NovaLoadingSpinner"), DefaultValue(3)]
        public int Thickness
        {
            get { return _thickness; }
            set { _thickness = Math.Max(1, value); Invalidate(); }
        }

        [Category("NovaLoadingSpinner"), DefaultValue(20)]
        [Description("سرعة الدوران بالمللي ثانية بين كل إطار (أقل = أسرع).")]
        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = Math.Max(5, value);
                if (_timer != null) _timer.Interval = _speed;
            }
        }

        [Category("NovaLoadingSpinner"), DefaultValue(90)]
        [Description("طول القوس المتحرك بالدرجات (0 = دائرة كاملة).")]
        public int ArcLength
        {
            get { return _arcLength; }
            set { _arcLength = Math.Max(10, Math.Min(360, value)); Invalidate(); }
        }

        [Category("NovaLoadingSpinner"), DefaultValue(true)]
        public bool IsSpinning
        {
            get { return _isSpinning; }
            set
            {
                _isSpinning = value;
                if (value) Start(); else Stop();
                Invalidate();
            }
        }

        #endregion

        #region Public API

        public void Start()
        {
            _isSpinning = true;
            if (_timer != null && !_timer.Enabled) _timer.Start();
            Invalidate();
        }

        public void Stop()
        {
            _isSpinning = false;
            if (_timer != null) _timer.Stop();
            Invalidate();
        }

        #endregion

        #region Visibility Hooks

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && _isSpinning) Start();
            else Stop();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Stop();
            base.OnHandleDestroyed(e);
        }

        #endregion

        #region Paint

        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void DrawParentBackground(Graphics g)
        {
            if (Parent == null) return;
            try
            {
                var state = g.Save();
                g.TranslateTransform(-this.Left, -this.Top);
                using (var pe = new PaintEventArgs(g, Parent.ClientRectangle))
                {
                    InvokePaintBackground(Parent, pe);
                    InvokePaint(Parent, pe);
                }
                g.Restore(state);
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            DrawParentBackground(g);

            int size = Math.Min(Width, Height);
            if (size < 8) return;

            int inset = _thickness;
            var rect = new Rectangle(
                (Width - size) / 2 + inset / 2,
                (Height - size) / 2 + inset / 2,
                size - inset,
                size - inset);

            using (var p = new Pen(_trackColor, _thickness))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawArc(p, rect, 0, 360);
            }

            using (var p = new Pen(_spinnerColor, _thickness))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawArc(p, rect, _angle, _arcLength);
            }
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}