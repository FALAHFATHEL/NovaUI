using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [Description("صورة رمزية دائرية — تعرض صورة أو الأحرف الأولى مع حلقة ونقطة حالة.")]
    public class NovaAvatar : Control
    {
        private Image _image;
        private string _initials = "";
        private int _ringThickness = 0;
        private Color _ringColor = Color.FromArgb(99, 102, 241);
        private Color _backColor1 = Color.FromArgb(99, 102, 241);
        private Color _backColor2 = Color.FromArgb(139, 92, 246);
        private bool _gradientBackground = true;
        private Color _initialsColor = Color.White;
        private Color _statusColor = Color.Empty;
        private int _statusSize = 14;

        public NovaAvatar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(60, 60);
            Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        }

        [Category("NovaAvatar")]
        public Image Image
        {
            get => _image;
            set { _image = value; Invalidate(); }
        }

        [Category("NovaAvatar"), DefaultValue("")]
        [Description("الأحرف الأولى (حرفان). مثال: 'أح'.")]
        public string Initials
        {
            get => _initials;
            set { _initials = value ?? ""; Invalidate(); }
        }

        [Category("NovaAvatar"), DefaultValue(0)]
        [Description("سماكة الحلقة الخارجية. 0 = بدون حلقة.")]
        public int RingThickness
        {
            get => _ringThickness;
            set { _ringThickness = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaAvatar")]
        public Color RingColor
        {
            get => _ringColor;
            set { _ringColor = value; Invalidate(); }
        }

        [Category("NovaAvatar"), DefaultValue(true)]
        public bool GradientBackground
        {
            get => _gradientBackground;
            set { _gradientBackground = value; Invalidate(); }
        }

        [Category("NovaAvatar")]
        public Color BackgroundColor1
        {
            get => _backColor1;
            set { _backColor1 = value; Invalidate(); }
        }

        [Category("NovaAvatar")]
        public Color BackgroundColor2
        {
            get => _backColor2;
            set { _backColor2 = value; Invalidate(); }
        }

        [Category("NovaAvatar")]
        public Color InitialsColor
        {
            get => _initialsColor;
            set { _initialsColor = value; Invalidate(); }
        }

        [Category("NovaAvatar")]
        [Description("لون نقطة الحالة (اتركه فارغاً لإخفائها).")]
        public Color StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; Invalidate(); }
        }

        [Category("NovaAvatar"), DefaultValue(14)]
        public int StatusSize
        {
            get => _statusSize;
            set { _statusSize = Math.Max(8, value); Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float ring = _ringThickness;
            float maxSize = Math.Min(Width, Height);
            float d = maxSize - ring * 2; 

            if (d <= 4) return;

            var circleRect = new RectangleF((Width - d) / 2f, (Height - d) / 2f, d, d);

            if (ring > 0)
            {
                float ringPathSize = maxSize - ring;
                var ringRect = new RectangleF(
                    (Width - ringPathSize) / 2f,
                    (Height - ringPathSize) / 2f,
                    ringPathSize,
                    ringPathSize);

                using (var p = new Pen(_ringColor, ring))
                    g.DrawEllipse(p, ringRect);
            }

            if (_gradientBackground)
            {
                using (var brush = new LinearGradientBrush(
                    circleRect, _backColor1, _backColor2, 45f))
                    g.FillEllipse(brush, circleRect);
            }
            else
            {
                using (var b = new SolidBrush(_backColor1))
                    g.FillEllipse(b, circleRect);
            }

            if (_image != null)
            {
                int imgW = _image.Width;
                int imgH = _image.Height;
                int minImg = Math.Min(imgW, imgH);

                RectangleF srcRect = new RectangleF(
                    (imgW - minImg) / 2f,
                    (imgH - minImg) / 2f,
                    minImg,
                    minImg);

                using (var clipPath = new GraphicsPath())
                {
                    clipPath.AddEllipse(circleRect);
                    var oldClip = g.Clip;
                    g.SetClip(clipPath);

                    g.DrawImage(_image, circleRect, srcRect, GraphicsUnit.Pixel);

                    g.Clip = oldClip;
                }
            }
            else if (!string.IsNullOrEmpty(_initials))
            {
                using (var b = new SolidBrush(_initialsColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                    g.DrawString(_initials, Font, b, circleRect, sf);
            }

            if (_statusColor != Color.Empty && _statusColor.A > 0)
            {
                int ss = _statusSize;
                int sx = Width - ss - 2;
                int sy = Height - ss - 2;

                Color parentBg = (Parent != null &&
                                  Parent.BackColor != Color.Transparent &&
                                  Parent.BackColor != Color.Empty)
                                 ? Parent.BackColor
                                 : Color.White;

                using (var b = new SolidBrush(parentBg))
                    g.FillEllipse(b, sx - 3, sy - 3, ss + 6, ss + 6);

                using (var b = new SolidBrush(_statusColor))
                    g.FillEllipse(b, sx, sy, ss, ss);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            int s = Math.Min(Width, Height);
            if (Width != s || Height != s)
                Size = new Size(s, s);
        }
    }
}