using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("ValueChanged")]
    public class NovaDatePicker : Control
    {
        private DateTime _value = DateTime.Today;
        private bool _hover, _open;
        private int _borderRadius = 10;
        private Color _fillColor = Color.FromArgb(248, 250, 252);
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _iconColor = Color.FromArgb(148, 163, 184);
        private Color _iconFocusColor = Color.FromArgb(99, 102, 241);
        private string _format = "yyyy/MM/dd";
        private NovaDatePickerDropdown _dropdown;

        public event EventHandler ValueChanged;

        public NovaDatePicker()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(260, 48);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 11F);
        }

        [Category("NovaDatePicker")]
        public DateTime Value
        {
            get => _value;
            set { if (_value.Date == value.Date) return; _value = value.Date; Invalidate(); ValueChanged?.Invoke(this, EventArgs.Empty); }
        }

        [Category("NovaDatePicker"), DefaultValue("yyyy/MM/dd")] public string DateFormat { get => _format; set { _format = value; Invalidate(); } }
        [Category("NovaDatePicker"), DefaultValue(10)] public int BorderRadius { get => _borderRadius; set { _borderRadius = value; Invalidate(); } }
        [Category("NovaDatePicker")] public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        [Category("NovaDatePicker")] public Color FocusFillColor { get => _focusFillColor; set { _focusFillColor = value; Invalidate(); } }
        [Category("NovaDatePicker")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("NovaDatePicker")] public Color FocusBorderColor { get => _focusBorderColor; set { _focusBorderColor = value; Invalidate(); } }
        [Category("NovaDatePicker")] public Color TextColor { get => _textColor; set { _textColor = value; Invalidate(); } }
        [Category("NovaDatePicker")] public Color IconColor { get => _iconColor; set { _iconColor = value; Invalidate(); } }

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

            Color fill = (_open || _hover) ? _focusFillColor : _fillColor;
            Color border = _open ? _focusBorderColor : _hover ? _focusBorderColor : _borderColor;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var path = Round(rect, _borderRadius))
            {
                using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                using (var p = new Pen(border, 1.5f)) g.DrawPath(p, path);
            }

            var textRect = new Rectangle(46, 0, Width - 90, Height);
            using (var b = new SolidBrush(_textColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.DirectionRightToLeft })
                g.DrawString(_value.ToString(_format, CultureInfo.InvariantCulture), Font, b, textRect, sf);

            using (var f = new Font("Segoe MDL2 Assets", 13F))
            using (var b = new SolidBrush(_open || _hover ? _iconFocusColor : _iconColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("\uE787", f, b, new RectangleF(Width - 46, 0, 36, Height), sf);

            using (var pen = new Pen(_open || _hover ? _iconFocusColor : _iconColor, 1.8f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                float cx = 22, cy = Height / 2f;
                if (_open) g.DrawLines(pen, new[] { new PointF(cx - 4, cy + 2), new PointF(cx, cy - 2), new PointF(cx + 4, cy + 2) });
                else g.DrawLines(pen, new[] { new PointF(cx - 4, cy - 2), new PointF(cx, cy + 2), new PointF(cx + 4, cy - 2) });
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left && !_open) ShowCalendar(); base.OnMouseDown(e); }

        private void ShowCalendar()
        {
            if (_open) return;
            _open = true; Invalidate();
            _dropdown = new NovaDatePickerDropdown(_value, _borderRadius);
            _dropdown.DateSelected += dt => { Value = dt; CloseCalendar(); };
            _dropdown.DateClosed += (s, e) => CloseCalendar();
            var sp = PointToScreen(new Point(Width - _dropdown.Width, Height + 4));
            _dropdown.Location = sp; _dropdown.Show();
        }

        private void CloseCalendar()
        {
            if (!_open) return;
            _open = false;

            if (_dropdown != null && !_dropdown.IsDisposed)
            {
                var tmp = _dropdown;
                _dropdown = null;
                tmp.Close();
            }
            Invalidate();
        }

        protected override void OnHandleDestroyed(EventArgs e) { CloseCalendar(); base.OnHandleDestroyed(e); }

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

    internal class NovaDatePickerDropdown : Form
    {
        private DateTime _viewMonth, _selectedDate;
        private int _hoverDay = -1;
        private Rectangle _prevRect, _nextRect;
        public event Action<DateTime> DateSelected;
        public event EventHandler DateClosed;
        private const int W = 320, H = 340, CellSize = 40;
        private readonly Color _accent = Color.FromArgb(99, 102, 241);
        private readonly Color _headerBack = Color.FromArgb(248, 250, 252);
        private readonly Color _textColor = Color.FromArgb(15, 23, 42);
        private readonly Color _mutedColor = Color.FromArgb(148, 163, 184);
        private readonly Color _hoverBack = Color.FromArgb(238, 242, 255);
        private readonly Color _todayBorder = Color.FromArgb(99, 102, 241);
        private bool _isClosing = false;

        public NovaDatePickerDropdown(DateTime initial, int radius)
        {
            _selectedDate = initial.Date;
            _viewMonth = new DateTime(initial.Year, initial.Month, 1);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(W, H);
            BackColor = Color.White;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Deactivate += (s, e) => SafeClose();
            MouseMove += Calendar_MouseMove;
            MouseLeave += (s, e) => { _hoverDay = -1; Invalidate(); };
            MouseClick += Calendar_MouseClick;
        }

        private void SafeClose()
        {
            if (_isClosing) return;
            _isClosing = true;
            Close();
        }

        private void Calendar_MouseMove(object sender, MouseEventArgs e)
        {
            int day = GetDayAt(e.Location);
            if (day != _hoverDay) { _hoverDay = day; Cursor = (day > 0 || _prevRect.Contains(e.Location) || _nextRect.Contains(e.Location)) ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }

        private void Calendar_MouseClick(object sender, MouseEventArgs e)
        {
            if (_prevRect.Contains(e.Location)) { _viewMonth = _viewMonth.AddMonths(-1); Invalidate(); return; }
            if (_nextRect.Contains(e.Location)) { _viewMonth = _viewMonth.AddMonths(1); Invalidate(); return; }
            int day = GetDayAt(e.Location);
            if (day > 0) DateSelected?.Invoke(new DateTime(_viewMonth.Year, _viewMonth.Month, day));
        }

        private int GetDayAt(Point p)
        {
            int gridTop = 100, gridLeft = (W - 7 * CellSize) / 2;
            int col = (p.X - gridLeft) / CellSize;
            int row = (p.Y - gridTop) / CellSize;
            if (col < 0 || col > 6 || row < 0 || row > 5) return -1;
            int firstDayOffset = ((int)_viewMonth.DayOfWeek + 1) % 7;
            int daysInMonth = DateTime.DaysInMonth(_viewMonth.Year, _viewMonth.Month);
            int dayNum = row * 7 + col - firstDayOffset + 1;
            if (dayNum < 1 || dayNum > daysInMonth) return -1;
            return dayNum;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            using (var p = new Pen(Color.FromArgb(226, 232, 240), 1)) g.DrawRectangle(p, 0, 0, W - 1, H - 1);
            using (var b = new SolidBrush(_headerBack)) g.FillRectangle(b, 1, 1, W - 2, 58);

            string monthName = GetArabicMonth(_viewMonth.Month) + " " + _viewMonth.Year;
            using (var f = new Font("Segoe UI", 13F, FontStyle.Bold))
            using (var b = new SolidBrush(_textColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(monthName, f, b, new RectangleF(0, 10, W, 40), sf);

            _prevRect = new Rectangle(12, 15, 32, 32);
            _nextRect = new Rectangle(W - 44, 15, 32, 32);
            DrawNavButton(g, _prevRect, "\uE76B");
            DrawNavButton(g, _nextRect, "\uE76C");

            string[] dayNames = { "س", "ح", "ن", "ث", "ر", "خ", "ج" };
            int gridLeft = (W - 7 * CellSize) / 2;
            for (int i = 0; i < 7; i++)
            {
                var cr = new Rectangle(gridLeft + i * CellSize, 68, CellSize, 26);
                using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (var b = new SolidBrush(_mutedColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(dayNames[i], f, b, cr, sf);
            }
            DrawDaysGrid(g, gridLeft);
        }

        private void DrawNavButton(Graphics g, Rectangle rect, string glyph)
        {
            bool hover = rect.Contains(PointToClient(Cursor.Position));
            if (hover)
                using (var b = new SolidBrush(_hoverBack))
                using (var path = Round(rect, 8)) g.FillPath(b, path);
            using (var f = new Font("Segoe MDL2 Assets", 12F))
            using (var b = new SolidBrush(hover ? _accent : _mutedColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(glyph, f, b, rect, sf);
        }

        private void DrawDaysGrid(Graphics g, int gridLeft)
        {
            int gridTop = 100;
            int firstDayOffset = ((int)_viewMonth.DayOfWeek + 1) % 7;
            int daysInMonth = DateTime.DaysInMonth(_viewMonth.Year, _viewMonth.Month);
            var today = DateTime.Today;

            for (int day = 1; day <= daysInMonth; day++)
            {
                int idx = day - 1 + firstDayOffset;
                int col = idx % 7, row = idx / 7;
                int x = gridLeft + col * CellSize + 2, y = gridTop + row * CellSize + 2;
                var cr = new Rectangle(x, y, CellSize - 4, CellSize - 4);

                bool isSel = _selectedDate.Year == _viewMonth.Year && _selectedDate.Month == _viewMonth.Month && _selectedDate.Day == day;
                bool isToday = today.Year == _viewMonth.Year && today.Month == _viewMonth.Month && today.Day == day;
                bool isHover = _hoverDay == day;

                if (isSel) using (var b = new SolidBrush(_accent)) using (var path = Round(cr, 10)) g.FillPath(b, path);
                else if (isHover) using (var b = new SolidBrush(_hoverBack)) using (var path = Round(cr, 10)) g.FillPath(b, path);
                if (isToday && !isSel) using (var p = new Pen(_todayBorder, 1.5f)) using (var path = Round(cr, 10)) g.DrawPath(p, path);

                Color tc = isSel ? Color.White : _textColor;
                using (var f = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (var b = new SolidBrush(tc))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(day.ToString(), f, b, cr, sf);
            }
        }

        private static string GetArabicMonth(int m)
        { string[] months = { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" }; return months[m - 1]; }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        protected override CreateParams CreateParams { get { const int CS_DROPSHADOW = 0x20000; var cp = base.CreateParams; cp.ClassStyle |= CS_DROPSHADOW; return cp; } }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            DateClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
