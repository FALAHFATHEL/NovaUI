using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    public enum AlertType { Info, Success, Warning, Danger, Neutral }

    [ToolboxItem(true)]
    [DefaultEvent("AlertClosed")]
    [Description("تنبيه مضمّن داخل الصفحة — بـ 5 أنواع وأزرار إجراء وتجاوب كامل مع RTL و LTR والألوان المخصصة.")]
    public class NovaAlert : Control
    {
        private AlertType _type = AlertType.Info;
        private string _title = "";
        private string _message = "نص التنبيه";
        private bool _dismissible = true;
        private bool _showIcon = true;
        private string _actionText = "";
        private bool _actionHover, _closeHover;
        private Rectangle _closeRect, _actionRect;

        private string _iconGlyph = "";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";

        private int _radius = 10;
        private int _padding = 14;
        private int _iconSize = 22;

        private Color _customForeColor = Color.Empty;
        private Color _customBackColor = Color.Empty;
        private Color _customBorderColor = Color.Empty;
        private Color _iconColor = Color.Empty;
        private Color _titleColor = Color.Empty;
        private Color _messageColor = Color.Empty;
        private Color _closeButtonColor = Color.Empty;
        private Color _actionButtonColor = Color.Empty;
        private Color _actionButtonTextColor = Color.Empty;

        private Color _typeColor;
        private Color _typeBgColor;
        private string _typeIcon;

        public event EventHandler AlertClosed;
        public event EventHandler ActionClicked;

        public NovaAlert()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9.5F);
            Size = new Size(500, 70);
            ApplyType();
        }

        #region Properties (Base)

        [Category("NovaAlert"), DefaultValue(AlertType.Info)]
        public AlertType Type
        {
            get => _type;
            set { _type = value; ApplyType(); UpdateHeight(); Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue("")]
        public string Title
        {
            get => _title;
            set { _title = value ?? ""; UpdateHeight(); Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue("نص التنبيه")]
        public string Message
        {
            get => _message;
            set { _message = value ?? ""; UpdateHeight(); Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue(true)]
        public bool Dismissible
        {
            get => _dismissible;
            set { _dismissible = value; UpdateHeight(); Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue(true)]
        public bool ShowIcon
        {
            get => _showIcon;
            set { _showIcon = value; UpdateHeight(); Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue("")]
        public string ActionText
        {
            get => _actionText;
            set { _actionText = value ?? ""; UpdateHeight(); Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue("")]
        [Description("رمز أيقونة مخصّص.")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        public string IconGlyph
        {
            get => _iconGlyph;
            set { _iconGlyph = value ?? ""; Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName
        {
            get => _iconFontName;
            set { _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue("")]
        public string IconPackName
        {
            get => _iconPackName;
            set { _iconPackName = value ?? ""; Invalidate(); }
        }

        [Category("NovaAlert"), DefaultValue(10)]
        public int BorderRadius
        {
            get => _radius;
            set { _radius = Math.Max(0, value); Invalidate(); }
        }

        #endregion

        #region Properties (Colors)

        [Category("NovaAlert Colors"), Description("لون عام للنص والأيقونة (يُستخدم كمرجع إذا لم تُحدَّد ألوان فرعية).")]
        public Color CustomForeColor
        {
            get => _customForeColor;
            set { _customForeColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون خلفية التنبيه المخصصة.")]
        public Color CustomBackColor
        {
            get => _customBackColor;
            set { _customBackColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون إطار التنبيه والخط الجانبي المخصص.")]
        public Color CustomBorderColor
        {
            get => _customBorderColor;
            set { _customBorderColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون الأيقونة. إذا تُرك فارغاً يُستخدم CustomForeColor أو لون النوع.")]
        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون نص العنوان. إذا تُرك فارغاً يُستخدم CustomForeColor أو لون النوع.")]
        public Color TitleColor
        {
            get => _titleColor;
            set { _titleColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون نص الرسالة. إذا تُرك فارغاً يُستخدم اللون الافتراضي الرمادي.")]
        public Color MessageColor
        {
            get => _messageColor;
            set { _messageColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون زر الإغلاق (X) وحلقة التحويم.")]
        public Color CloseButtonColor
        {
            get => _closeButtonColor;
            set { _closeButtonColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون خلفية زر الإجراء.")]
        public Color ActionButtonColor
        {
            get => _actionButtonColor;
            set { _actionButtonColor = value; Invalidate(); }
        }

        [Category("NovaAlert Colors"), Description("لون نص زر الإجراء.")]
        public Color ActionButtonTextColor
        {
            get => _actionButtonTextColor;
            set { _actionButtonTextColor = value; Invalidate(); }
        }

        #endregion

        private Color ActiveForeColor => (!_customForeColor.IsEmpty) ? _customForeColor : _typeColor;
        private Color ActiveBackColor => (!_customBackColor.IsEmpty) ? _customBackColor : _typeBgColor;
        private Color ActiveBorderColor => (!_customBorderColor.IsEmpty) ? _customBorderColor : _typeColor;
        private Color ActiveIconColor => (!_iconColor.IsEmpty) ? _iconColor : ActiveForeColor;
        private Color ActiveTitleColor => (!_titleColor.IsEmpty) ? _titleColor : ActiveForeColor;
        private Color ActiveMessageColor => (!_messageColor.IsEmpty) ? _messageColor : Color.FromArgb(51, 65, 85);
        private Color ActiveCloseColor => (!_closeButtonColor.IsEmpty) ? _closeButtonColor : ActiveForeColor;
        private Color ActiveActionBg => (!_actionButtonColor.IsEmpty) ? _actionButtonColor : ActiveForeColor;
        private Color ActiveActionText => (!_actionButtonTextColor.IsEmpty) ? _actionButtonTextColor : Color.White;

        private void ApplyType()
        {
            switch (_type)
            {
                case AlertType.Success:
                    _typeColor = Color.FromArgb(22, 163, 74);
                    _typeBgColor = Color.FromArgb(232, 248, 240);
                    _typeIcon = "\uE73E";
                    break;
                case AlertType.Warning:
                    _typeColor = Color.FromArgb(217, 119, 6);
                    _typeBgColor = Color.FromArgb(254, 249, 224);
                    _typeIcon = "\uE7BA";
                    break;
                case AlertType.Danger:
                    _typeColor = Color.FromArgb(220, 38, 38);
                    _typeBgColor = Color.FromArgb(254, 242, 242);
                    _typeIcon = "\uEA39";
                    break;
                case AlertType.Neutral:
                    _typeColor = Color.FromArgb(71, 85, 105);
                    _typeBgColor = Color.FromArgb(241, 245, 249);
                    _typeIcon = "\uE946";
                    break;
                default:
                    _typeColor = Color.FromArgb(37, 99, 235);
                    _typeBgColor = Color.FromArgb(239, 246, 255);
                    _typeIcon = "\uE946";
                    break;
            }
        }

        private void UpdateHeight()
        {
            if (Width <= 0) return;

            int innerW = Width - (_padding * 2);
            if (_showIcon) innerW -= _iconSize + 12;
            if (_dismissible) innerW -= 32;

            if (!string.IsNullOrEmpty(_actionText))
            {
                try
                {
                    using (var actionFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                    {
                        var actionSize = TextRenderer.MeasureText(_actionText, actionFont);
                        int bw = actionSize.Width + 20;
                        innerW -= (bw + 12);
                    }
                }
                catch { innerW -= 100; }
            }

            innerW = Math.Max(50, innerW);

            Size textSize;
            try
            {
                textSize = TextRenderer.MeasureText(
                    _message ?? "", Font, new Size(innerW, int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            }
            catch { textSize = new Size(innerW, 20); }

            int h = (_padding * 2) + textSize.Height;
            if (!string.IsNullOrEmpty(_title)) h += 24;

            int minH = _padding * 2 + 20;
            Height = Math.Max(minH, h);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateHeight();
        }

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
            catch
            {
                using (var b = new SolidBrush(Parent.BackColor))
                    g.FillRectangle(b, ClientRectangle);
            }
        }

        private string GetIconFontKey()
        {
            if (!string.IsNullOrEmpty(_iconPackName)) return _iconPackName;
            if (!string.IsNullOrEmpty(_iconFontName)) return _iconFontName;
            return "Segoe MDL2 Assets";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            bool isRtl = RightToLeft == RightToLeft.Yes;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            try
            {
                using (var path = Round(rect, _radius))
                {
                    using (var b = new SolidBrush(ActiveBackColor))
                        g.FillPath(b, path);
                    using (var p = new Pen(ActiveBorderColor, 1))
                        g.DrawPath(p, path);
                }
            }
            catch { }

            try
            {
                const int barWidth = 4;

                using (var clipPath = Round(rect, _radius))
                {
                    var state = g.Save();
                    g.SetClip(clipPath, CombineMode.Replace);

                    Rectangle barRect = isRtl
                        ? new Rectangle(rect.Right - barWidth + 1, rect.Y, barWidth, rect.Height + 1)
                        : new Rectangle(rect.X, rect.Y, barWidth, rect.Height + 1);

                    using (var b = new SolidBrush(ActiveBorderColor))
                        g.FillRectangle(b, barRect);

                    g.Restore(state);
                }
            }
            catch { }

            int actionWidth = 0;
            if (!string.IsNullOrEmpty(_actionText))
            {
                try
                {
                    using (var actionFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                    {
                        var actionSize = TextRenderer.MeasureText(_actionText, actionFont);
                        int bw = actionSize.Width + 20;
                        int bh = 30;
                        int bx = isRtl
                            ? _padding + (_dismissible ? 32 : 0)
                            : Width - _padding - (_dismissible ? 32 : 0) - bw;
                        int by = (Height - bh) / 2;
                        _actionRect = new Rectangle(bx, by, bw, bh);
                        actionWidth = bw + 12;
                    }
                }
                catch { _actionRect = Rectangle.Empty; }
            }
            else _actionRect = Rectangle.Empty;

            if (_dismissible)
            {
                int size = 24;
                int closeX = isRtl ? _padding : Width - _padding - size + 4;
                _closeRect = new Rectangle(closeX, (Height - size) / 2, size, size);
            }
            else _closeRect = Rectangle.Empty;

            string glyph = !string.IsNullOrEmpty(_iconGlyph) ? _iconGlyph : _typeIcon;

            if (_showIcon && !string.IsNullOrEmpty(glyph))
            {
                int iconX = isRtl ? Width - _padding - _iconSize - 8 : _padding + 8;
                int iconY = _padding;

                try
                {
                    using (var f = FontManager.GetIconFont(GetIconFontKey(), _iconSize * 0.65f))
                    using (var b = new SolidBrush(ActiveIconColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    })
                    {
                        g.DrawString(glyph, f, b,
                            new RectangleF(iconX - 2, iconY - 2, _iconSize + 4, _iconSize + 4), sf);
                    }
                }
                catch { }
            }

            int textW = Width - (_padding * 2);
            if (_showIcon) textW -= _iconSize + 12;
            if (_dismissible) textW -= 32;
            if (!string.IsNullOrEmpty(_actionText)) textW -= actionWidth;
            textW = Math.Max(40, textW);

            int textDrawX = isRtl
                ? _padding + (_dismissible ? 32 : 0) + (!string.IsNullOrEmpty(_actionText) ? actionWidth : 0)
                : (_padding + 8 + (_showIcon ? _iconSize + 12 : 0));

            int textY = _padding;

            if (!string.IsNullOrEmpty(_title))
            {
                try
                {
                    using (var f = new Font("Segoe UI", 10F, FontStyle.Bold))
                    using (var b = new SolidBrush(ActiveTitleColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near,
                        Trimming = StringTrimming.EllipsisCharacter
                    })
                    {
                        if (isRtl) sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                        g.DrawString(_title, f, b, new RectangleF(textDrawX, textY, textW, 22), sf);
                    }
                }
                catch { }
                textY += 22;
            }

            if (!string.IsNullOrEmpty(_message))
            {
                try
                {
                    using (var b = new SolidBrush(ActiveMessageColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near,
                        Trimming = StringTrimming.Word
                    })
                    {
                        if (isRtl) sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                        g.DrawString(_message, Font, b,
                            new RectangleF(textDrawX, textY, textW, Height - textY - _padding), sf);
                    }
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(_actionText) && !_actionRect.IsEmpty)
            {
                try
                {
                    Color bg = _actionHover
                        ? ActiveActionBg
                        : Color.FromArgb(200, ActiveActionBg);

                    using (var path = Round(_actionRect, 6))
                    using (var b = new SolidBrush(bg))
                        g.FillPath(b, path);

                    using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                    using (var b = new SolidBrush(ActiveActionText))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    })
                        g.DrawString(_actionText, f, b, _actionRect, sf);
                }
                catch { }
            }

            if (_dismissible && !_closeRect.IsEmpty)
            {
                try
                {
                    int size = _closeRect.Width;

                    if (_closeHover)
                    {
                        using (var b = new SolidBrush(Color.FromArgb(40, ActiveCloseColor)))
                        using (var path = Round(_closeRect, size / 2))
                            g.FillPath(b, path);
                    }

                    using (var pen = new Pen(ActiveCloseColor, 1.6f))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        int cx = _closeRect.X + _closeRect.Width / 2;
                        int cy = _closeRect.Y + _closeRect.Height / 2;
                        int s = 4;
                        g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
                        g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
                    }
                }
                catch { }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool ch = _dismissible && !_closeRect.IsEmpty && _closeRect.Contains(e.Location);
            bool ah = !string.IsNullOrEmpty(_actionText) && !_actionRect.IsEmpty && _actionRect.Contains(e.Location);

            if (ch != _closeHover || ah != _actionHover)
            {
                _closeHover = ch;
                _actionHover = ah;
                Cursor = (ch || ah) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _closeHover = _actionHover = false;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            try
            {
                if (_dismissible && !_closeRect.IsEmpty && _closeRect.Contains(e.Location))
                {
                    AlertClosed?.Invoke(this, EventArgs.Empty);
                    var parent = Parent;
                    if (parent != null) parent.Controls.Remove(this);
                    Dispose();
                    return;
                }

                if (!string.IsNullOrEmpty(_actionText) && !_actionRect.IsEmpty && _actionRect.Contains(e.Location))
                    ActionClicked?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
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