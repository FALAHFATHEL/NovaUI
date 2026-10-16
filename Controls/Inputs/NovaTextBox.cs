using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("TextChanged")]
    [DefaultProperty("PlaceholderText")]
    [Description("حقل إدخال احترافي بزوايا دائرية وأيقونة ودعم كامل لشفافية FPanel و RTL.")]
    public class NovaTextBox : UserControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        private readonly TextBox _inner;
        private bool _focused, _eyeHover, _reveal;
        private Rectangle _eyeRect;

        private int _radius = 10;
        private int _iconAreaWidth = 46;
        private int _eyeWidth = 34;

        private string _placeholderText = "";
        private string _iconGlyph = "\uE77B";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";
        private bool _isPassword;
        private bool _showToggle;

        private Color _fillColor = Color.FromArgb(248, 250, 252);
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _iconColor = Color.FromArgb(148, 163, 184);
        private Color _iconFocusColor = Color.FromArgb(99, 102, 241);

        public NovaTextBox()
        {
            try { IconPackRegistry.EnsureInitialized(); } catch { }

            AutoScaleMode = AutoScaleMode.None;
            AutoScaleDimensions = new SizeF(6F, 13F);

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(320, 52);
            MinimumSize = new Size(80, 36);
            Cursor = Cursors.IBeam;
            TabStop = true;

            _inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Font = new Font("Segoe UI", 11F),
                ForeColor = _textColor,
                BackColor = _fillColor,
                TabStop = false
            };

            _inner.GotFocus += (s, e) => { _focused = true; Invalidate(); };
            _inner.LostFocus += (s, e) => { _focused = false; Invalidate(); };
            _inner.TextChanged += (s, e) => OnTextChanged(e);
            _inner.KeyDown += (s, e) => OnKeyDown(e);
            _inner.KeyPress += (s, e) => OnKeyPress(e);
            _inner.KeyUp += (s, e) => OnKeyUp(e);
            _inner.HandleCreated += (s, e) => ApplyCue();

            Controls.Add(_inner);
        }

        [Category("NovaTextBox"), DefaultValue("")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set { _placeholderText = value ?? ""; ApplyCue(); Invalidate(); }
        }

        [Category("NovaControls")]
        [Description("رمز الأيقونة (يُعبَّأ تلقائياً من المنتقي).")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        [DefaultValue("")]
        public string IconGlyph
        {
            get { return _iconGlyph; }
            set { _iconGlyph = value ?? ""; Invalidate(); }
        }

        [Category("NovaControls")]
        [DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName
        {
            get { return _iconFontName; }
            set { _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); }
        }

        [Category("NovaControls")]
        [DefaultValue("")]
        public string IconPackName
        {
            get { return _iconPackName; }
            set { _iconPackName = value ?? ""; Invalidate(); }
        }

        [Category("NovaTextBox"), DefaultValue(false)]
        public bool IsPassword
        {
            get { return _isPassword; }
            set { _isPassword = value; _inner.UseSystemPasswordChar = value && !_reveal; }
        }

        [Category("NovaTextBox"), DefaultValue(false)]
        public bool ShowToggle
        {
            get { return _showToggle; }
            set { _showToggle = value; LayoutInner(); Invalidate(); }
        }

        [Category("NovaTextBox"), DefaultValue(10)]
        public int Radius
        {
            get { return _radius; }
            set { _radius = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaTextBox"), DefaultValue(46)]
        public int IconAreaWidth
        {
            get { return _iconAreaWidth; }
            set { _iconAreaWidth = Math.Max(0, value); LayoutInner(); Invalidate(); }
        }

        [Category("NovaTextBox")] public Color FillColor { get { return _fillColor; } set { _fillColor = value; if (!_focused) _inner.BackColor = value; Invalidate(); } }
        [Category("NovaTextBox")] public Color FocusFillColor { get { return _focusFillColor; } set { _focusFillColor = value; if (_focused) _inner.BackColor = value; Invalidate(); } }
        [Category("NovaTextBox")] public Color BorderColor { get { return _borderColor; } set { _borderColor = value; Invalidate(); } }
        [Category("NovaTextBox")] public Color FocusBorderColor { get { return _focusBorderColor; } set { _focusBorderColor = value; Invalidate(); } }
        [Category("NovaTextBox")] public Color TextColor { get { return _textColor; } set { _textColor = value; _inner.ForeColor = value; Invalidate(); } }
        [Category("NovaTextBox")] public Color IconColor { get { return _iconColor; } set { _iconColor = value; Invalidate(); } }
        [Category("NovaTextBox")] public Color IconFocusColor { get { return _iconFocusColor; } set { _iconFocusColor = value; Invalidate(); } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        public override string Text
        {
            get { return _inner.Text; }
            set { if (_inner.Text != value) _inner.Text = value ?? ""; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox InnerTextBox { get { return _inner; } }

        public override Font Font
        {
            get { return base.Font; }
            set { base.Font = value; if (_inner != null) { _inner.Font = value; LayoutInner(); } }
        }

        [Browsable(false)] public int SelectionStart { get { return _inner.SelectionStart; } set { _inner.SelectionStart = value; } }
        [Browsable(false)] public int SelectionLength { get { return _inner.SelectionLength; } set { _inner.SelectionLength = value; } }
        [Browsable(false)] public int MaxLength { get { return _inner.MaxLength; } set { _inner.MaxLength = value; } }
        [Browsable(false)] public bool ReadOnly { get { return _inner.ReadOnly; } set { _inner.ReadOnly = value; } }
        [Browsable(false)] public bool IsEmpty { get { return string.IsNullOrEmpty(_inner.Text); } }
        [Browsable(false)] public HorizontalAlignment TextAlign { get { return _inner.TextAlign; } set { _inner.TextAlign = value; } }

        public void Clear() { _inner.Clear(); }
        public void SelectAll() { _inner.SelectAll(); }
        public void AppendText(string text) { _inner.AppendText(text ?? ""); }
        public void Select(int start, int length) { _inner.Select(start, length); }
        public new bool Focus() { return _inner != null && _inner.Focus(); }
        public void Copy() { _inner.Copy(); }
        public void Cut() { _inner.Cut(); }
        public void Paste() { _inner.Paste(); }
        public void Undo() { _inner.Undo(); }

        protected override void OnCreateControl() { base.OnCreateControl(); ApplyCue(); LayoutInner(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ApplyCue(); LayoutInner(); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); LayoutInner(); }

        private void LayoutInner()
        {
            if (_inner == null || _inner.IsDisposed) return;
            if (Width <= 0 || Height <= 0) return;

            bool isRtl = RightToLeft == RightToLeft.Yes;
            _inner.RightToLeft = RightToLeft;

            int iconW = string.IsNullOrEmpty(_iconGlyph) ? 10 : _iconAreaWidth;
            int toggleW = _showToggle ? _eyeWidth : 0;

            int leftPadding = isRtl ? (toggleW > 0 ? toggleW + 6 : 14) : iconW;
            int rightPadding = isRtl ? iconW : (14 + (toggleW > 0 ? toggleW : 0));

            int x = leftPadding;
            int w = Math.Max(10, Width - leftPadding - rightPadding);

            int textH = _inner.PreferredHeight;
            if (textH <= 0 || textH > Height) textH = Math.Min(Height - 8, 24);

            int y = (Height - textH) / 2;

            _inner.SetBounds(x, y, w, textH);
            _inner.BringToFront();
        }

        private void ApplyCue()
        {
            if (_inner != null && _inner.IsHandleCreated)
            {
                try { SendMessage(_inner.Handle, EM_SETCUEBANNER, IntPtr.Zero, _placeholderText ?? ""); }
                catch { }
            }
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
            catch { }
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
            Color fill = _focused ? _focusFillColor : _fillColor;
            Color border = _focused ? _focusBorderColor : _borderColor;

            _inner.BackColor = fill;

            try
            {
                using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), _radius))
                {
                    using (var b = new SolidBrush(fill))
                        g.FillPath(b, path);
                    using (var p = new Pen(border, 1.5f))
                        g.DrawPath(p, path);
                }
            }
            catch { }

            DrawIconGlyph(g, isRtl);
            DrawPasswordToggle(g, isRtl);
        }

        private void DrawIconGlyph(Graphics g, bool isRtl)
        {
            if (string.IsNullOrEmpty(_iconGlyph)) return;

            try
            {
                float iconX = isRtl ? Width - _iconAreaWidth : 0;
                var iconRect = new RectangleF(iconX, 0, _iconAreaWidth, Height);

                string fontKey = !string.IsNullOrEmpty(_iconPackName)
                    ? _iconPackName
                    : (!string.IsNullOrEmpty(_iconFontName) ? _iconFontName : "Segoe MDL2 Assets");

                using (var f = FontManager.GetIconFont(fontKey, 13F))
                using (var b = new SolidBrush(_focused ? _iconFocusColor : _iconColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(_iconGlyph, f, b, iconRect, sf);
                }
            }
            catch { }
        }

        private void DrawPasswordToggle(Graphics g, bool isRtl)
        {
            if (!_showToggle) return;

            try
            {
                int eyeX = isRtl ? 6 : Width - _eyeWidth - 6;
                _eyeRect = new Rectangle(eyeX, (Height - _eyeWidth) / 2, _eyeWidth, _eyeWidth);

                using (var f = FontManager.GetIconFont("Segoe MDL2 Assets", 12F))
                using (var b = new SolidBrush(_eyeHover ? _focusBorderColor : _iconColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(_reveal ? "\uED1A" : "\uE7B3", f, b, _eyeRect, sf);
                }
            }
            catch { }
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_showToggle && _eyeRect.Contains(e.Location))
            {
                _reveal = !_reveal;
                _inner.UseSystemPasswordChar = _isPassword && !_reveal;
                Invalidate(_eyeRect);
                _inner.Focus();
                return;
            }
            _inner.Focus();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_showToggle) return;
            bool h = _eyeRect.Contains(e.Location);
            if (h != _eyeHover)
            {
                _eyeHover = h;
                Cursor = h ? Cursors.Hand : Cursors.IBeam;
                Invalidate(_eyeRect);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_eyeHover) { _eyeHover = false; Cursor = Cursors.IBeam; Invalidate(_eyeRect); }
        }

        protected override void OnEnter(EventArgs e) { base.OnEnter(e); _inner.Focus(); }
        protected override void OnRightToLeftChanged(EventArgs e) { base.OnRightToLeftChanged(e); LayoutInner(); Invalidate(); }
        protected override void OnParentBackColorChanged(EventArgs e) { base.OnParentBackColorChanged(e); Invalidate(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _inner != null && !_inner.IsDisposed)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}