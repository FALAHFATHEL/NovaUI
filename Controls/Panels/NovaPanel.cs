using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("Click")]
    [Description("لوحة احترافية: زوايا دائرية ناعمة بدون أطراف مكسرة، تدرج لوني، حدود.")]
    public class NovaPanel : Panel
    {
        private int _borderRadius = 16;
        private int _borderSize = 0;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _fillColor = Color.White;

        private bool _gradientEnabled = false;
        private Color _gradientColor1 = Color.FromArgb(99, 102, 241);
        private Color _gradientColor2 = Color.FromArgb(139, 92, 246);
        private float _gradientAngle = 90f;

        public NovaPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(320, 200);
            Font = new Font("Segoe UI", 9.5F);
        }

        #region الخصائص

        [Category("NovaPanel"), DefaultValue(16)]
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                _borderRadius = value;
                Invalidate();
                NotifyChildrenToRedraw();
            }
        }

        [Category("NovaPanel"), DefaultValue(0)]
        public int BorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaPanel")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("NovaPanel")]
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                Invalidate();
                NotifyChildrenToRedraw();
            }
        }

        [Category("NovaPanel"), DefaultValue(false)]
        public bool GradientEnabled
        {
            get => _gradientEnabled;
            set
            {
                _gradientEnabled = value;
                Invalidate();
                NotifyChildrenToRedraw();
            }
        }

        [Category("NovaPanel")]
        public Color GradientColor1
        {
            get => _gradientColor1;
            set
            {
                _gradientColor1 = value;
                Invalidate();
                NotifyChildrenToRedraw();
            }
        }

        [Category("NovaPanel")]
        public Color GradientColor2
        {
            get => _gradientColor2;
            set
            {
                _gradientColor2 = value;
                Invalidate();
                NotifyChildrenToRedraw();
            }
        }

        [Category("NovaPanel"), DefaultValue(90f)]
        public float GradientAngle
        {
            get => _gradientAngle;
            set { _gradientAngle = value; Invalidate(); }
        }

        #endregion

        #region إدارة إعادة الرسم للأدوات الأبناء

        private void NotifyChildrenToRedraw()
        {
            foreach (Control child in Controls)
            {
                child.Invalidate();
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.Invalidate();
        }

        #endregion

        #region Path

        private int GetEffectiveRadius()
        {
            if (Width <= 0 || Height <= 0) return 0;
            int maxRadius = Math.Min(Width, Height) / 2;
            if (_borderRadius < 0) return maxRadius;
            return Math.Min(_borderRadius, maxRadius);
        }

        private static GraphicsPath BuildPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region الرسم

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

            // رسم خلفية الأب الحاوية خلف الزوايا لضمان الشفافية وتنعيم الحواف (Anti-aliasing)
            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = GetEffectiveRadius();

            using (var path = BuildPath(rect, radius))
            {
                if (_gradientEnabled)
                {
                    using (var brush = new LinearGradientBrush(
                        rect, _gradientColor1, _gradientColor2, _gradientAngle))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    using (var b = new SolidBrush(_fillColor))
                        g.FillPath(b, path);
                }

                if (_borderSize > 0)
                {
                    using (var p = new Pen(_borderColor, _borderSize))
                    {
                        p.Alignment = PenAlignment.Inset;
                        g.DrawPath(p, path);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        #endregion

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }
    }
}

