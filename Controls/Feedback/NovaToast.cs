using NovaUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    public enum ToastType { Success, Error, Warning, Info }

    public static class NovaToast
    {
        private static readonly List<NovaTostForm> _active = new List<NovaTostForm>();
        private const int W = 340;
        private const int H = 70;
        private const int Margin = 18;
        private const int Spacing = 10;

        public static void Success(Form owner, string msg, int ms = 3200) => Show(owner, msg, ToastType.Success, ms);
        public static void Error(Form owner, string msg, int ms = 4200) => Show(owner, msg, ToastType.Error, ms);
        public static void Warning(Form owner, string msg, int ms = 3800) => Show(owner, msg, ToastType.Warning, ms);
        public static void Info(Form owner, string msg, int ms = 3200) => Show(owner, msg, ToastType.Info, ms);

        public static void Show(Form owner, string message, ToastType type, int durationMs)
        {
            var toast = new NovaTostForm(message, type);

            var screen = (owner != null && owner.Visible)
                ? Screen.FromControl(owner)
                : Screen.PrimaryScreen;
            var area = screen.WorkingArea;

            int targetX = area.Right - W - Margin;
            int targetY = area.Bottom - H - Margin - (_active.Count * (H + Spacing));

            toast.StartPosition = FormStartPosition.Manual;
            toast.Bounds = new Rectangle(targetX + 60, targetY, W, H);
            toast.Opacity = 0;

            _active.Add(toast);

            toast.FormClosed += (s, e) =>
            {
                _active.Remove(toast);
                RepositionAll(area);
            };

            toast.Show();
            toast.PlayInAnimation(targetX, targetY);
            toast.ScheduleClose(durationMs);
        }

        private static void RepositionAll(Rectangle area)
        {
            int baseX = area.Right - W - Margin;
            int baseY = area.Bottom - H - Margin;

            for (int i = 0; i < _active.Count; i++)
            {
                int y = baseY - (i * (H + Spacing));
                _active[i].MoveTo(baseX, y);
            }
        }
    }

    internal class NovaTostForm : Form
    {
        private readonly Timer _closeTimer;
        private readonly ToastType _type;
        private readonly Color _accent;
        private readonly string _icon;
        private readonly string _message;
        private bool _closing;

        private const int SizeW = 340;
        private const int SizeH = 70;

        public NovaTostForm(string message, ToastType type)
        {
            _message = message ?? "";
            _type = type;

            switch (type)
            {
                case ToastType.Success: _accent = Color.FromArgb(34, 197, 94); _icon = "\uE73E"; break;
                case ToastType.Error: _accent = Color.FromArgb(239, 68, 68); _icon = "\uEA39"; break;
                case ToastType.Warning: _accent = Color.FromArgb(245, 158, 11); _icon = "\uE7BA"; break;
                default: _accent = Color.FromArgb(99, 102, 241); _icon = "\uE946"; break;
            }

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            Size = new Size(SizeW, SizeH);
            BackColor = Color.FromArgb(30, 41, 59);
            DoubleBuffered = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            _closeTimer = new Timer { Interval = 3200 };
            _closeTimer.Tick += (s, e) => { _closeTimer.Stop(); StartClose(); };
        }

        public void PlayInAnimation(int targetX, int targetY)
        {
            int startX = targetX + 60;

            Animator.Animate(this, 0, 1, 260, v =>
            {
                if (IsDisposed) return;
                int x = (int)(startX + (targetX - startX) * v);
                Location = new Point(x, targetY);
                Opacity = Math.Min(1.0, v);
            }, null, Animator.EaseOutCubic);
        }

        public void MoveTo(int x, int y)
        {
            Animator.Animate(this, 0, 1, 200, v =>
            {
                if (IsDisposed) return;
                int nx = (int)(Location.X + (x - Location.X) * v);
                int ny = (int)(Location.Y + (y - Location.Y) * v);
                Location = new Point(nx, ny);
            });
        }

        public void ScheduleClose(int ms)
        {
            _closeTimer.Interval = ms;
            _closeTimer.Start();
        }

        private void StartClose()
        {
            if (_closing) return;
            _closing = true;

            int startX = Location.X;
            int endX = startX + 80;

            Animator.Animate(this, 1, 0, 220, v =>
            {
                if (IsDisposed) return;
                int x = (int)(startX + (endX - startX) * (1 - v));
                Location = new Point(x, Location.Y);
                Opacity = v;
            }, () =>
            {
                if (IsDisposed) return;
                _closeTimer.Dispose();
                Close();
            }, Animator.EaseInOutCubic);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Round(rect, 10))
            using (var b = new SolidBrush(BackColor))
                g.FillPath(b, path);

            using (var b = new SolidBrush(_accent))
                g.FillRectangle(b, Width - 5, 0, 5, Height);

            using (var f = new Font("Segoe MDL2 Assets", 18F))
            using (var b = new SolidBrush(_accent))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
                g.DrawString(_icon, f, b, new RectangleF(Width - 62, 0, 50, Height), sf);

            using (var f = new Font("Segoe UI", 10.5F, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft,
                Trimming = StringTrimming.EllipsisCharacter
            })
                g.DrawString(_message, f, b, new Rectangle(20, 0, Width - 88, Height), sf);
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
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
}
