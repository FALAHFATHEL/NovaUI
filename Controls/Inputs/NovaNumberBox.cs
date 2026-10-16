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
    public class NovaNumberBox : UserControl
    {
        private readonly TextBox _inner;
        private bool _focused, _upHover, _upDown, _downHover, _downDown;
        private Rectangle _upRect, _downRect;
        private decimal _value, _minimum = 0m, _maximum = 1000000m, _step = 1m;
        private int _decimals = 0;
        private bool _useThousands = true;
        private string _prefix = "", _suffix = "";
        private int _radius = 10, _buttonWidth = 32;
        private bool _showButtons = true;
        private Color _fillColor = Color.FromArgb(248, 250, 252);
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _buttonColor = Color.FromArgb(100, 116, 139);
        private Color _buttonHoverColor = Color.FromArgb(99, 102, 241);
        private Color _buttonBackHover = Color.FromArgb(238, 242, 255);
        private Color _placeholderColor = Color.FromArgb(148, 163, 184);

        public event EventHandler ValueChanged;

        public NovaNumberBox()
        {
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(200, 52);

            _inner = new TextBox { BorderStyle = BorderStyle.None, AutoSize = false, Font = new Font("Segoe UI", 11F), ForeColor = _textColor, BackColor = _fillColor, TextAlign = HorizontalAlignment.Left };
            _inner.GotFocus += (s, e) => { _focused = true; Invalidate(); };
            _inner.LostFocus += (s, e) => { _focused = false; CommitText(); Invalidate(); };
            _inner.TextChanged += (s, e) => ApplyFormatting();
            _inner.KeyDown += Inner_KeyDown;
            Controls.Add(_inner);
            ApplyFormatting();
        }

        [Category("NovaNumberBox")] public decimal Value { get => _value; set { decimal v = Clamp(value); if (_value == v) return; _value = v; ApplyFormatting(); ValueChanged?.Invoke(this, EventArgs.Empty); } }
        [Category("NovaNumberBox"), DefaultValue(typeof(decimal), "0")] public decimal Minimum { get => _minimum; set { _minimum = value; if (_maximum < _minimum) _maximum = _minimum; Value = Clamp(_value); } }
        [Category("NovaNumberBox"), DefaultValue(typeof(decimal), "1000000")] public decimal Maximum { get => _maximum; set { _maximum = value; if (_minimum > _maximum) _maximum = _minimum; Value = Clamp(_value); } }
        [Category("NovaNumberBox"), DefaultValue(typeof(decimal), "1")] public decimal Step { get => _step; set { _step = value <= 0 ? 1m : value; } }
        [Category("NovaNumberBox"), DefaultValue(0)] public int Decimals { get => _decimals; set { _decimals = Math.Max(0, Math.Min(6, value)); ApplyFormatting(); } }
        [Category("NovaNumberBox"), DefaultValue(true)] public bool UseThousandsSeparator { get => _useThousands; set { _useThousands = value; ApplyFormatting(); } }
        [Category("NovaNumberBox"), DefaultValue("")] public string Prefix { get => _prefix; set { _prefix = value ?? ""; LayoutInner(); ApplyFormatting(); } }
        [Category("NovaNumberBox"), DefaultValue("")] public string Suffix { get => _suffix; set { _suffix = value ?? ""; LayoutInner(); ApplyFormatting(); } }
        [Category("NovaNumberBox"), DefaultValue(10)] public int BorderRadius { get => _radius; set { _radius = Math.Max(0, value); Invalidate(); } }
        [Category("NovaNumberBox"), DefaultValue(true)] public bool ShowButtons { get => _showButtons; set { _showButtons = value; LayoutInner(); Invalidate(); } }
        [Category("NovaNumberBox")] public Color FillColor { get => _fillColor; set { _fillColor = value; if (!_focused) _inner.BackColor = value; Invalidate(); } }
        [Category("NovaNumberBox")] public Color FocusFillColor { get => _focusFillColor; set { _focusFillColor = value; if (_focused) _inner.BackColor = value; Invalidate(); } }
        [Category("NovaNumberBox")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("NovaNumberBox")] public Color FocusBorderColor { get => _focusBorderColor; set { _focusBorderColor = value; Invalidate(); } }
        [Category("NovaNumberBox")] public Color TextColor { get => _textColor; set { _textColor = value; _inner.ForeColor = value; Invalidate(); } }
        [Browsable(false)] public TextBox InnerTextBox => _inner;

        private decimal Clamp(decimal v) { if (v < _minimum) return _minimum; if (v > _maximum) return _maximum; return v; }
        private void Increment() { Value = Clamp(_value + _step); }
        private void Decrement() { Value = Clamp(_value - _step); }

        private void ApplyFormatting()
        {
            if (_inner == null || _inner.IsDisposed) return;
            string num = _value.ToString((_useThousands ? "N" : "F") + _decimals, CultureInfo.CurrentCulture);
            string display = _prefix + num + _suffix;
            if (_inner.Text != display) { _inner.Text = display; _inner.SelectionStart = display.Length; }
        }

        private void CommitText()
        {
            string raw = _inner.Text;
            if (!string.IsNullOrEmpty(_prefix) && raw.StartsWith(_prefix)) raw = raw.Substring(_prefix.Length);
            if (!string.IsNullOrEmpty(_suffix) && raw.EndsWith(_suffix)) raw = raw.Substring(0, raw.Length - _suffix.Length);
            raw = raw.Replace(",", "").Replace(" ", "").Trim();
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out var v)) { decimal c = Clamp(v); if (c != _value) { _value = c; ValueChanged?.Invoke(this, EventArgs.Empty); } }
            ApplyFormatting();
        }

        private void Inner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) { Increment(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Down) { Decrement(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Enter) { CommitText(); e.SuppressKeyPress = true; }
        }

        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); LayoutInner(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); LayoutInner(); }

        private void LayoutInner()
        {
            if (_inner == null || _inner.IsDisposed) return;
            if (Width <= 0 || Height <= 0) return;
            int leftPad = 14 + TextRenderer.MeasureText(_prefix, Font).Width;
            int rightPad = 14 + TextRenderer.MeasureText(_suffix, Font).Width;

            int buttonsTotalWidth = _showButtons ? _buttonWidth + 4 : 0;
            if (_showButtons) rightPad += buttonsTotalWidth;

            int w = Math.Max(20, Width - leftPad - rightPad);
            int textH = _inner.PreferredHeight;
            if (textH <= 0 || textH > Height) textH = Math.Min(Height - 8, 24);
            _inner.SetBounds(leftPad, (Height - textH) / 2, w, textH);

            int btnX = Width - _buttonWidth - 4;
            int btnH = (Height - 10) / 2;
            _upRect = new Rectangle(btnX, 3, _buttonWidth, btnH);
            _downRect = new Rectangle(btnX, _upRect.Bottom + 2, _buttonWidth, btnH);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = _focused ? _focusFillColor : _fillColor;
            Color border = _focused ? _focusBorderColor : _borderColor;
            using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), _radius))
            {
                using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                using (var p = new Pen(border, 1.2f)) g.DrawPath(p, path);
            }
            if (!string.IsNullOrEmpty(_prefix))
                using (var b = new SolidBrush(_placeholderColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                    g.DrawString(_prefix, Font, b, new RectangleF(14, 0, 40, Height), sf);
            if (!string.IsNullOrEmpty(_suffix))
            {
                int sx = Width - 14 - (_showButtons ? _buttonWidth + 4 : 0);
                using (var b = new SolidBrush(_placeholderColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center })
                    g.DrawString(_suffix, Font, b, new RectangleF(sx - 60, 0, 60, Height), sf);
            }
            if (_showButtons) { DrawButton(g, _upRect, true, _upHover, _upDown); DrawButton(g, _downRect, false, _downHover, _downDown); }
        }

        private void DrawButton(Graphics g, Rectangle rect, bool isUp, bool hover, bool down)
        {
            if (hover || down)
                using (var path = Round(rect, 4))
                using (var b = new SolidBrush(down ? Color.FromArgb(224, 231, 255) : _buttonBackHover))
                    g.FillPath(b, path);
            Color col = hover || down ? _buttonHoverColor : _buttonColor;
            using (var pen = new Pen(col, 1.8f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                float cx = rect.X + rect.Width / 2f, cy = rect.Y + rect.Height / 2f, s = 3.5f;
                if (isUp) g.DrawLines(pen, new[] { new PointF(cx - s, cy + s / 2), new PointF(cx, cy - s / 2), new PointF(cx + s, cy + s / 2) });
                else g.DrawLines(pen, new[] { new PointF(cx - s, cy - s / 2), new PointF(cx, cy + s / 2), new PointF(cx + s, cy - s / 2) });
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_showButtons) return;
            bool uh = _upRect.Contains(e.Location), dh = _downRect.Contains(e.Location);
            if (uh != _upHover || dh != _downHover) { _upHover = uh; _downHover = dh; Cursor = (uh || dh) ? Cursors.Hand : Cursors.IBeam; Invalidate(); }
        }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _upHover = _downHover = false; _upDown = _downDown = false; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!_showButtons) { _inner.Focus(); return; }
            if (_upRect.Contains(e.Location)) { _upDown = true; Increment(); Invalidate(); }
            else if (_downRect.Contains(e.Location)) { _downDown = true; Decrement(); Invalidate(); }
            else _inner.Focus();
        }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _upDown = _downDown = false; Invalidate(); }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        protected override void Dispose(bool disposing) { if (disposing && _inner != null && !_inner.IsDisposed) _inner.Dispose(); base.Dispose(disposing); }
    }
}

