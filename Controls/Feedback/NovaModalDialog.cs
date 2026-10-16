using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    public enum DialogType { Info, Success, Warning, Error, Question }

    public static class NovaModalDialog
    {
        public static bool Show(
            Form owner,
            string title,
            string message,
            DialogType type = DialogType.Info,
            string confirmText = "OK",
            string cancelText = null)
        {
            using (var dimmer = new NovaModalDimmer(owner))
            using (var dialog = new NovaModalDialogForm(owner, title, message, type, confirmText, cancelText))
            {
                dimmer.Show(owner);
                dialog.ShowDialog(owner);
                dimmer.Close();
                return dialog.Confirmed;
            }
        }

        public static bool Confirm(Form owner, string title, string message,
                                   DialogType type = DialogType.Question,
                                   string yesText = "Confirm",
                                   string noText = "Cancel")
            => Show(owner, title, message, type, yesText, noText);
    }

    // ================================================================
    // 1) Dimmer 
    // ================================================================
    internal class NovaModalDimmer : Form
    {
        public NovaModalDimmer(Form owner)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            BackColor = Color.Black;
            Opacity = 0.55;
            TopMost = true;
            SetStyle(ControlStyles.Selectable, false);

            if (owner != null && owner.Visible)
            {
                var topLeft = owner.PointToScreen(Point.Empty);
                Bounds = new Rectangle(topLeft, owner.Size);
                Owner = owner;
            }
            else
            {
                Bounds = Screen.PrimaryScreen.Bounds;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_NOACTIVATE = 0x08000000;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
                return cp;
            }
        }
    }

    // ================================================================
    // 2) Dialog 
    // ================================================================
    internal class NovaModalDialogForm : Form
    {
        // ---------- مقاسات قابلة للتخصيص ----------
        private const int CardMargin = 8;    
        private const int CardPadding = 28;   
        private const int CardRadius = 16;
        private const int ButtonRadius = 10;

        private const int IconSize = 56;
        private const int IconGap = 16;   
        private const int GapTitleMessage = 14;
        private const int GapMessageBtn = 24;  

        private const int ButtonHeight = 44;
        private const int ButtonMinWidth = 110;
        private const int ButtonPaddingH = 24;   
        private const int ButtonGap = 10;

        private const int MinContentWidth = 320; 
        private const int MaxContentWidth = 560; 

        // ---------- المحتوى ----------
        private readonly string _title;
        private readonly string _message;
        private readonly DialogType _type;
        private readonly string _confirmText;
        private readonly string _cancelText;

        private readonly Color _accent;
        private readonly string _icon;
        private readonly bool _rtl;

        // ---------- الخطوط ----------
        private readonly Font _titleFont = new Font("Segoe UI", 15F, FontStyle.Bold);
        private readonly Font _messageFont = new Font("Segoe UI", 10.5F);
        private readonly Font _buttonFont = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        private readonly Font _iconFont = new Font("Segoe MDL2 Assets", 24F);

        // ---------- التخطيط المحسوب ----------
        private Rectangle _cardRect;
        private Rectangle _titleRect;
        private Rectangle _iconRect;
        private Rectangle _messageRect;
        private Rectangle _confirmRect;
        private Rectangle _cancelRect;

        // ---------- حالة التفاعل ----------
        private bool _confirmHover, _cancelHover;
        private bool _confirmPressed, _cancelPressed;

        public bool Confirmed { get; private set; }

        public NovaModalDialogForm(Form owner, string title, string message, DialogType type,
                                   string confirmText, string cancelText)
        {
            _title = title ?? string.Empty;
            _message = message ?? string.Empty;
            _type = type;
            _confirmText = string.IsNullOrEmpty(confirmText) ? "OK" : confirmText;
            _cancelText = cancelText;

            switch (type)
            {
                case DialogType.Success: _accent = Color.FromArgb(34, 197, 94); _icon = "\uE73E"; break;
                case DialogType.Warning: _accent = Color.FromArgb(245, 158, 11); _icon = "\uE7BA"; break;
                case DialogType.Error: _accent = Color.FromArgb(239, 68, 68); _icon = "\uEA39"; break;
                case DialogType.Question: _accent = Color.FromArgb(99, 102, 241); _icon = "\uE9CE"; break;
                default: _accent = Color.FromArgb(99, 102, 241); _icon = "\uE946"; break;
            }

            _rtl = DetectRtl(_title) || DetectRtl(_message);

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.White;
            KeyPreview = true;
            DoubleBuffered = true;
            TopMost = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            ComputeLayout();
            CenterInOwner(owner);

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape && !string.IsNullOrEmpty(_cancelText))
                {
                    Confirmed = false;
                    Close();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    Confirmed = true;
                    Close();
                }
            };

            MouseMove += OnFormMouseMove;
            MouseDown += OnFormMouseDown;
            MouseUp += OnFormMouseUp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _titleFont?.Dispose();
                _messageFont?.Dispose();
                _buttonFont?.Dispose();
                _iconFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ============================================================
        //  التخطيط
        // ============================================================
        private void ComputeLayout()
        {
            Size titleNatural = TextRenderer.MeasureText(
                _title, _titleFont,
                new Size(10000, 10000),
                TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            Size msgNatural = string.IsNullOrEmpty(_message)
                ? Size.Empty
                : TextRenderer.MeasureText(
                    _message, _messageFont,
                    new Size(10000, 10000),
                    TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            int confirmW = MeasureButtonWidth(_confirmText);
            int cancelW = string.IsNullOrEmpty(_cancelText) ? 0 : MeasureButtonWidth(_cancelText);
            int buttonsW = confirmW + (cancelW > 0 ? ButtonGap + cancelW : 0);

            int headerW = titleNatural.Width + IconGap + IconSize;
            int idealContentW = Math.Max(Math.Max(headerW, msgNatural.Width), buttonsW);
            int contentW = Math.Max(MinContentWidth, Math.Min(MaxContentWidth, idealContentW));
            if (buttonsW > contentW) contentW = buttonsW; // لا نقطع الأزرار أبدًا

            int msgHeight;
            if (!string.IsNullOrEmpty(_message))
            {
                Size wrapped = TextRenderer.MeasureText(
                    _message, _messageFont,
                    new Size(contentW, 10000),
                    TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                msgHeight = Math.Max(wrapped.Height, _messageFont.Height);
            }
            else msgHeight = _messageFont.Height;

            int headerH = Math.Max(IconSize, Math.Max(titleNatural.Height, 24));

            int contentH = headerH + GapTitleMessage + msgHeight + GapMessageBtn + ButtonHeight;
            int cardW = contentW + CardPadding * 2;
            int cardH = contentH + CardPadding * 2;
            int dialogW = cardW + CardMargin * 2;
            int dialogH = cardH + CardMargin * 2;

            ClientSize = new Size(dialogW, dialogH);

            _cardRect = new Rectangle(CardMargin, CardMargin, cardW, cardH);

            int cLeft = _cardRect.X + CardPadding;
            int cTop = _cardRect.Y + CardPadding;

            int iconY = cTop + (headerH - IconSize) / 2;
            int titleH = Math.Max(titleNatural.Height, 24);
            int titleY = cTop + (headerH - titleH) / 2;
            int titleW = contentW - IconSize - IconGap;

            if (_rtl)
            {
                _iconRect = new Rectangle(cLeft, iconY, IconSize, IconSize);
                _titleRect = new Rectangle(_iconRect.Right + IconGap, titleY, titleW, titleH);
            }
            else
            {
                _iconRect = new Rectangle(cLeft + contentW - IconSize, iconY, IconSize, IconSize);
                _titleRect = new Rectangle(cLeft, titleY, titleW, titleH);
            }

            _messageRect = new Rectangle(cLeft, cTop + headerH + GapTitleMessage, contentW, msgHeight);

            int btnY = _cardRect.Bottom - CardPadding - ButtonHeight;

            if (_rtl)
            {
                _confirmRect = new Rectangle(cLeft, btnY, confirmW, ButtonHeight);
                _cancelRect = cancelW > 0
                    ? new Rectangle(_confirmRect.Right + ButtonGap, btnY, cancelW, ButtonHeight)
                    : Rectangle.Empty;
            }
            else
            {
                _confirmRect = new Rectangle(cLeft + contentW - confirmW, btnY, confirmW, ButtonHeight);
                _cancelRect = cancelW > 0
                    ? new Rectangle(_confirmRect.Left - ButtonGap - cancelW, btnY, cancelW, ButtonHeight)
                    : Rectangle.Empty;
            }
        }

        private int MeasureButtonWidth(string text)
        {
            Size sz = TextRenderer.MeasureText(
                text ?? string.Empty, _buttonFont,
                new Size(10000, 10000),
                TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            return Math.Max(ButtonMinWidth, sz.Width + ButtonPaddingH * 2);
        }

        private void CenterInOwner(Form owner)
        {
            Rectangle anchor;
            if (owner != null && owner.Visible)
                anchor = owner.RectangleToScreen(owner.ClientRectangle);
            else
                anchor = Screen.FromPoint(Cursor.Position).WorkingArea;

            if (Width > anchor.Width || Height > anchor.Height)
                anchor = Screen.FromRectangle(anchor).WorkingArea;

            int x = anchor.X + (anchor.Width - Width) / 2;
            int y = anchor.Y + (anchor.Height - Height) / 2;

            var screen = Screen.FromRectangle(anchor).WorkingArea;
            x = Math.Max(screen.X, Math.Min(x, screen.Right - Width));
            y = Math.Max(screen.Y, Math.Min(y, screen.Bottom - Height));

            Location = new Point(x, y);
        }

        private static bool DetectRtl(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if ((c >= 0x0590 && c <= 0x08FF) ||
                    (c >= 0xFB1D && c <= 0xFDFF) ||
                    (c >= 0xFE70 && c <= 0xFEFF))
                    return true;
                if (char.IsLetter(c)) return false; 
            }
            return false;
        }

        // ============================================================
        //  (fade in)
        // ============================================================
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Opacity = 0;
            Animator.Animate(this, 0f, 1f, 180,
                v => { if (!IsDisposed) Opacity = v; },
                null,
                Animator.EaseOutCubic);
        }

        // ============================================================
        //  الرسم
        // ============================================================
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawShadow(g);

            using (var path = Round(_cardRect, CardRadius))
            using (var b = new SolidBrush(Color.White))
                g.FillPath(b, path);

            var topStrip = new Rectangle(_cardRect.X, _cardRect.Y, _cardRect.Width, 6);
            using (var path = RoundTop(topStrip, CardRadius))
            using (var b = new SolidBrush(_accent))
                g.FillPath(b, path);

            using (var b = new SolidBrush(Color.FromArgb(38, _accent.R, _accent.G, _accent.B)))
                g.FillEllipse(b, _iconRect);

            var iconFlags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                            TextFormatFlags.NoPadding;
            TextRenderer.DrawText(g, _icon, _iconFont, _iconRect, _accent, iconFlags);

            var titleFlags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
                             TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                             TextFormatFlags.NoPadding;
            titleFlags |= _rtl
                ? TextFormatFlags.Right | TextFormatFlags.RightToLeft
                : TextFormatFlags.Left;
            TextRenderer.DrawText(g, _title, _titleFont, _titleRect,
                Color.FromArgb(15, 23, 42), titleFlags);

            var msgFlags = TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak |
                           TextFormatFlags.Top | TextFormatFlags.NoPadding;
            msgFlags |= _rtl
                ? TextFormatFlags.Right | TextFormatFlags.RightToLeft
                : TextFormatFlags.Left;
            TextRenderer.DrawText(g, _message, _messageFont, _messageRect,
                Color.FromArgb(71, 85, 105), msgFlags);

            DrawButton(g, _confirmRect, _confirmText, _accent, true, _confirmHover, _confirmPressed);
            if (_cancelRect != Rectangle.Empty)
                DrawButton(g, _cancelRect, _cancelText,
                    Color.FromArgb(226, 232, 240), false, _cancelHover, _cancelPressed);
        }

        private void DrawShadow(Graphics g)
        {
            for (int i = 6; i >= 1; i--)
            {
                int alpha = 8 + (6 - i) * 4;
                var r = new Rectangle(
                    _cardRect.X - i / 2,
                    _cardRect.Y + i,
                    _cardRect.Width + i,
                    _cardRect.Height + i);

                using (var path = Round(r, CardRadius + i / 2))
                using (var b = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                    g.FillPath(b, path);
            }
        }

        private void DrawButton(Graphics g, Rectangle rect, string text,
            Color baseColor, bool filled, bool hover, bool pressed)
        {
            Color fill, textColor;

            if (filled)
            {
                fill = pressed ? Darken(baseColor, 0.85f)
                     : hover ? Darken(baseColor, 0.92f)
                     : baseColor;
                textColor = Color.White;
            }
            else
            {
                fill = pressed ? Darken(baseColor, 0.90f)
                     : hover ? Darken(baseColor, 0.95f)
                     : baseColor;
                textColor = Color.FromArgb(51, 65, 85);
            }

            using (var path = Round(rect, ButtonRadius))
            using (var b = new SolidBrush(fill))
                g.FillPath(b, path);

            var flags = TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.NoPadding;
            if (_rtl) flags |= TextFormatFlags.RightToLeft;

            TextRenderer.DrawText(g, text, _buttonFont, rect, textColor, flags);
        }

        // ============================================================
        //  التفاعل
        // ============================================================
        private void OnFormMouseMove(object sender, MouseEventArgs e)
        {
            bool ch = _confirmRect.Contains(e.Location);
            bool xh = _cancelRect != Rectangle.Empty && _cancelRect.Contains(e.Location);

            if (ch != _confirmHover || xh != _cancelHover)
            {
                _confirmHover = ch;
                _cancelHover = xh;
                Cursor = (ch || xh) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        private void OnFormMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (_confirmRect.Contains(e.Location)) _confirmPressed = true;
            if (_cancelRect != Rectangle.Empty && _cancelRect.Contains(e.Location)) _cancelPressed = true;
            Invalidate();
        }

        private void OnFormMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            if (_confirmPressed && _confirmRect.Contains(e.Location))
            {
                Confirmed = true;
                Close();
                return;
            }

            if (_cancelPressed && _cancelRect != Rectangle.Empty && _cancelRect.Contains(e.Location))
            {
                Confirmed = false;
                Close();
                return;
            }

            _confirmPressed = _cancelPressed = false;
            Invalidate();
        }

        // ============================================================
        //  مساعدات الرسم
        // ============================================================
        private static Color Darken(Color c, float f)
            => Color.FromArgb(c.A,
                (int)Math.Max(0, c.R * f),
                (int)Math.Max(0, c.G * f),
                (int)Math.Max(0, c.B * f));

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

        private static GraphicsPath RoundTop(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            p.CloseFigure();
            return p;
        }
    }
}