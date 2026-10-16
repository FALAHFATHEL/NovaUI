using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("SearchRequested")]
    [Description("حقل بحث احترافي متجاوب ونقي وخالٍ من العيوب البصرية.")]
    public class NovaSearchBox : UserControl
    {
        private readonly TextBox _inner;
        private readonly Timer _debounce;
        private readonly NovaLoadingSpinner _spinner;

        private bool _focused;
        private bool _clearHover;
        private bool _showSuggestions;
        private FSuggestionPopup _popup;

        private int _radius = 24;
        private int _iconAreaWidth = 44;
        private int _clearSize = 24;
        private int _debounceMs = 350;

        private string _placeholderText = "ابحث...";
        private string _iconGlyph = "\uE721";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";
        private bool _isLoading;
        private bool _showClearButton = true;

        private readonly List<string> _suggestions = new List<string>();
        private readonly List<string> _recentSearches = new List<string>();
        private readonly List<string> _filteredSuggestions = new List<string>();

        private Color _fillColor = Color.FromArgb(248, 250, 252);
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _iconColor = Color.FromArgb(148, 163, 184);
        private Color _iconFocusColor = Color.FromArgb(99, 102, 241);
        private Color _clearColor = Color.FromArgb(148, 163, 184);
        private Color _clearHoverColor = Color.FromArgb(239, 68, 68);

        private Rectangle _clearRect;

        public event EventHandler<string> SearchRequested;
        public event EventHandler<string> SearchTextChanged;
        public event EventHandler<string> SuggestionSelected;

        public NovaSearchBox()
        {
            AutoScaleMode = AutoScaleMode.None;
            AutoScaleDimensions = new SizeF(6F, 13F);

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(340, 48);
            MinimumSize = new Size(120, 36);

            _inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                Font = new Font("Segoe UI", 11F),
                ForeColor = _textColor,
                BackColor = _fillColor
            };

            _inner.GotFocus += (s, e) => { _focused = true; Invalidate(); };
            _inner.LostFocus += (s, e) => { _focused = false; Invalidate(); };
            _inner.TextChanged += Inner_TextChanged;
            _inner.KeyDown += Inner_KeyDown;

            _spinner = new NovaLoadingSpinner
            {
                Size = new Size(20, 20),
                Thickness = 2,
                SpinnerColor = Color.FromArgb(99, 102, 241),
                TrackColor = Color.FromArgb(226, 232, 240),
                Speed = 30,
                Visible = false
            };

            _debounce = new Timer { Interval = _debounceMs };
            _debounce.Tick += (s, e) =>
            {
                _debounce.Stop();
                var h = SearchRequested;
                if (h != null) h(this, _inner.Text.Trim());
            };

            Controls.Add(_inner);
            Controls.Add(_spinner);
        }

        [Category("NovaSearchBox"), DefaultValue("ابحث...")]
        public string PlaceholderText { get { return _placeholderText; } set { _placeholderText = value ?? ""; Invalidate(); } }

        [Category("NovaSearchBox"), DefaultValue("\uE721")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        public string IconGlyph { get { return _iconGlyph; } set { _iconGlyph = value ?? ""; Invalidate(); } }

        [Category("NovaSearchBox"), DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName
        {
            get { return _iconFontName; }
            set { _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); }
        }

        [Category("NovaSearchBox"), DefaultValue("")]
        public string IconPackName { get { return _iconPackName; } set { _iconPackName = value ?? ""; Invalidate(); } }

        [Category("NovaSearchBox"), DefaultValue(24)]
        public int BorderRadius { get { return _radius; } set { _radius = Math.Max(0, value); LayoutInner(); Invalidate(); } }

        [Category("NovaSearchBox"), DefaultValue(44)]
        public int IconAreaWidth { get { return _iconAreaWidth; } set { _iconAreaWidth = Math.Max(20, value); LayoutInner(); Invalidate(); } }

        [Category("NovaSearchBox"), DefaultValue(350)]
        public int DebounceMs
        {
            get { return _debounceMs; }
            set { _debounceMs = Math.Max(0, value); if (_debounce != null) _debounce.Interval = _debounceMs; }
        }

        [Category("NovaSearchBox"), DefaultValue(false)]
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                if (_spinner != null)
                {
                    _spinner.Visible = value;
                    if (value)
                    {
                        try { _spinner.GetType().GetProperty("Animating")?.SetValue(_spinner, true); } catch { }
                    }
                }
                LayoutInner();
                Invalidate();
            }
        }

        [Category("NovaSearchBox"), DefaultValue(true)]
        public bool ShowClearButton { get { return _showClearButton; } set { _showClearButton = value; Invalidate(); } }

        [Category("NovaSearchBox")]
        public Color FillColor { get { return _fillColor; } set { _fillColor = value; if (!_focused) _inner.BackColor = value; Invalidate(); } }

        [Category("NovaSearchBox")]
        public Color FocusFillColor { get { return _focusFillColor; } set { _focusFillColor = value; if (_focused) _inner.BackColor = value; Invalidate(); } }

        [Category("NovaSearchBox")]
        public Color BorderColor { get { return _borderColor; } set { _borderColor = value; Invalidate(); } }

        [Category("NovaSearchBox")]
        public Color FocusBorderColor { get { return _focusBorderColor; } set { _focusBorderColor = value; Invalidate(); } }

        [Category("NovaSearchBox")]
        public Color TextColor { get { return _textColor; } set { _textColor = value; _inner.ForeColor = value; Invalidate(); } }

        [Category("NovaSearchBox")]
        public Color IconColor { get { return _iconColor; } set { _iconColor = value; Invalidate(); } }

        [Category("NovaSearchBox")]
        public Color ClearHoverColor { get { return _clearHoverColor; } set { _clearHoverColor = value; Invalidate(); } }

        [Browsable(false)] public List<string> Suggestions { get { return _suggestions; } }
        [Browsable(false)] public List<string> RecentSearches { get { return _recentSearches; } }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Appearance")]
        public override string Text
        {
            get { return _inner.Text; }
            set { if (_inner.Text != value) _inner.Text = value ?? ""; }
        }

        [Browsable(false)] public TextBox InnerTextBox { get { return _inner; } }

        public void Clear() { _inner.Clear(); }

        public void AddSuggestion(string suggestion)
        {
            if (string.IsNullOrWhiteSpace(suggestion)) return;
            if (_suggestions.Any(s => string.Equals(s, suggestion, StringComparison.OrdinalIgnoreCase))) return;
            _suggestions.Add(suggestion);
        }

        public void AddSuggestions(IEnumerable<string> suggestions)
        {
            if (suggestions == null) return;
            foreach (var s in suggestions) AddSuggestion(s);
        }

        public void ClearSuggestions() { _suggestions.Clear(); }

        public void AddRecentSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return;
            _recentSearches.RemoveAll(s => string.Equals(s, term, StringComparison.OrdinalIgnoreCase));
            _recentSearches.Insert(0, term);
            if (_recentSearches.Count > 8) _recentSearches.RemoveAt(_recentSearches.Count - 1);
        }

        private void Inner_TextChanged(object sender, EventArgs e)
        {
            var h = SearchTextChanged;
            if (h != null) h(this, _inner.Text);

            _debounce.Stop();
            if (!string.IsNullOrWhiteSpace(_inner.Text)) _debounce.Start();
            else HideSuggestions();

            UpdateSuggestionList();
            Invalidate();
        }

        private void Inner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _debounce.Stop();
                var query = _inner.Text.Trim();
                if (!string.IsNullOrEmpty(query))
                {
                    AddRecentSearch(query);
                    var h = SearchRequested;
                    if (h != null) h(this, query);
                }
                HideSuggestions();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideSuggestions();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down && _showSuggestions && _popup != null)
            {
                _popup.SelectNext();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up && _showSuggestions && _popup != null)
            {
                _popup.SelectPrevious();
                e.SuppressKeyPress = true;
            }
        }

        private void UpdateSuggestionList()
        {
            _filteredSuggestions.Clear();
            var query = _inner.Text.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(query))
                _filteredSuggestions.AddRange(_recentSearches);
            else
                _filteredSuggestions.AddRange(_suggestions.Where(s => s.ToLowerInvariant().Contains(query)).Take(8));

            if (_filteredSuggestions.Count > 0 && _focused) ShowSuggestions();
            else HideSuggestions();
        }

        private void ShowSuggestions()
        {
            if (_popup == null || _popup.IsDisposed)
            {
                _popup = new FSuggestionPopup(_filteredSuggestions, _inner.Text.Trim().Length == 0);
                _popup.SuggestionChosen += (s, value) =>
                {
                    _inner.Text = value;
                    _inner.SelectionStart = value.Length;
                    var h1 = SuggestionSelected;
                    if (h1 != null) h1(this, value);
                    HideSuggestions();
                    _debounce.Stop();
                    AddRecentSearch(value);
                    var h2 = SearchRequested;
                    if (h2 != null) h2(this, value);
                };
                _popup.Hidden += (s, e) => _showSuggestions = false;
            }
            else
            {
                _popup.UpdateItems(_filteredSuggestions, _inner.Text.Trim().Length == 0);
            }

            _showSuggestions = true;
            var screenPos = PointToScreen(new Point(0, Height + 4));
            _popup.Location = screenPos;
            _popup.Width = Width;

            if (!_popup.Visible) _popup.Show();
        }

        private void HideSuggestions()
        {
            _showSuggestions = false;
            if (_popup != null) _popup.Hide();
        }

        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); LayoutInner(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); LayoutInner(); }

        private void LayoutInner()
        {
            if (_inner == null || _inner.IsDisposed) return;
            if (Width <= 0 || Height <= 0) return;

            int leftPad = _iconAreaWidth;
            int rightPad = 14 + (_isLoading ? 26 : 0) + (_showClearButton && _inner.Text.Length > 0 ? 26 : 0);

            int w = Math.Max(20, Width - leftPad - rightPad);
            int textH = _inner.PreferredHeight;
            if (textH <= 0 || textH > Height) textH = Math.Min(Height - 8, 24);
            int y = (Height - textH) / 2;

            _inner.SetBounds(leftPad, y, w, textH);

            if (_spinner != null)
            {
                int spinnerX = Width - 14 - 22 - (_showClearButton && _inner.Text.Length > 0 ? 26 : 0);
                _spinner.Location = new Point(spinnerX, (Height - _spinner.Height) / 2);
            }

            _clearRect = new Rectangle(Width - 14 - _clearSize, (Height - _clearSize) / 2, _clearSize, _clearSize);
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

            Color fill = _focused ? _focusFillColor : _fillColor;
            Color border = _focused ? _focusBorderColor : _borderColor;

            using (var path = Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), _radius))
            {
                using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                using (var p = new Pen(border, 1.2f)) g.DrawPath(p, path);
            }

            if (!string.IsNullOrEmpty(_iconGlyph))
            {
                try
                {
                    using (var f = FontManager.GetIconFont(GetIconFontKey(), 13F))
                    using (var b = new SolidBrush(_focused ? _iconFocusColor : _iconColor))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoClip })
                        g.DrawString(_iconGlyph, f, b, new RectangleF(0, 0, _iconAreaWidth, Height), sf);
                }
                catch { }
            }

            if (string.IsNullOrEmpty(_inner.Text) && !string.IsNullOrEmpty(_placeholderText))
            {
                using (var b = new SolidBrush(_iconColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                    g.DrawString(_placeholderText, Font, b, new RectangleF(_iconAreaWidth, 0, Math.Max(20, Width - _iconAreaWidth - 20), Height), sf);
            }

            if (_showClearButton && _inner.Text.Length > 0 && !_isLoading)
            {
                if (_clearHover)
                {
                    using (var b = new SolidBrush(Color.FromArgb(254, 226, 226)))
                    using (var path = Round(_clearRect, _clearRect.Width / 2))
                        g.FillPath(b, path);
                }

                using (var pen = new Pen(_clearHover ? _clearHoverColor : _clearColor, 1.8f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    int cx = _clearRect.X + _clearRect.Width / 2;
                    int cy = _clearRect.Y + _clearRect.Height / 2;
                    int s = 4;
                    g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
                    g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool ch = _clearRect.Contains(e.Location) && _showClearButton && _inner.Text.Length > 0;
            if (ch != _clearHover)
            {
                _clearHover = ch;
                Cursor = ch ? Cursors.Hand : Cursors.IBeam;
                Invalidate(_clearRect);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_clearHover) { _clearHover = false; Invalidate(_clearRect); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_clearRect.Contains(e.Location) && _showClearButton && _inner.Text.Length > 0)
            {
                Clear();
                return;
            }
            _inner.Focus();
        }

        protected override void OnEnter(EventArgs e) { base.OnEnter(e); _inner.Focus(); }

        private static GraphicsPath Round(RectangleF r, int radius)
        {
            var p = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_popup != null) _popup.Dispose();
                if (_debounce != null) { _debounce.Stop(); _debounce.Dispose(); }
                if (_inner != null) _inner.Dispose();
                if (_spinner != null) _spinner.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal class FSuggestionPopup : Form
    {
        private List<string> _items;
        private bool _showHeaders;
        private int _selectedIndex;
        private int _hoverIndex = -1;

        public event EventHandler<string> SuggestionChosen;
        public event EventHandler Hidden;

        private const int ItemH = 36;
        private const int Radius = 10;
        private const int MaxItems = 8;

        public FSuggestionPopup(List<string> items, bool showHeaders)
        {
            _items = items;
            _showHeaders = showHeaders;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = Color.White;
            TopMost = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            MouseMove += OnPopupMouseMove;
            MouseLeave += (s, e) => { _hoverIndex = -1; Invalidate(); };
            MouseClick += OnPopupMouseClick;
            Deactivate += (s, e) => HidePopup();

            UpdateItems(items, showHeaders);
        }

        public void UpdateItems(List<string> items, bool showHeaders)
        {
            _items = items;
            _showHeaders = showHeaders;
            _selectedIndex = items.Count > 0 ? 0 : -1;
            _hoverIndex = -1;

            int headerH = showHeaders && items.Count > 0 ? 26 : 0;
            Height = headerH + Math.Min(items.Count, MaxItems) * ItemH + 16;

            if (Width > 0 && Height > 0)
                Region = new Region(Round(new RectangleF(0, 0, Width, Height), Radius));

            Invalidate();
        }

        public void SelectNext()
        {
            if (_items.Count == 0) return;
            _selectedIndex = (_selectedIndex + 1) % _items.Count;
            Invalidate();
        }

        public void SelectPrevious()
        {
            if (_items.Count == 0) return;
            _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Width > 0 && Height > 0)
                Region = new Region(Round(new RectangleF(0, 0, Width, Height), Radius));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width > 0 && Height > 0)
                Region = new Region(Round(new RectangleF(0, 0, Width, Height), Radius));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            int y = 6;
            if (_showHeaders && _items.Count > 0)
            {
                using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                using (var b = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    g.DrawString("عمليات البحث الأخيرة", f, b, 14, y + 4);
                y += 26;
            }

            for (int i = 0; i < _items.Count && i < MaxItems; i++)
            {
                var rect = new Rectangle(4, y, Width - 8, ItemH - 2);
                bool selected = i == _selectedIndex;
                bool hover = i == _hoverIndex;

                if (selected || hover)
                {
                    using (var path = Round(rect, 6))
                    using (var b = new SolidBrush(selected ? Color.FromArgb(238, 242, 255) : Color.FromArgb(248, 250, 252)))
                        g.FillPath(b, path);
                }

                try
                {
                    using (var f = FontManager.GetIconFont("Segoe MDL2 Assets", 10F))
                    using (var b = new SolidBrush(Color.FromArgb(148, 163, 184)))
                    {
                        var iconGlyph = _showHeaders ? "\uE823" : "\uE721";
                        g.DrawString(iconGlyph, f, b, new RectangleF(rect.X + 10, rect.Y + (rect.Height - 16) / 2, 16, 16), new StringFormat { FormatFlags = StringFormatFlags.NoClip });
                    }
                }
                catch { }

                using (var b = new SolidBrush(Color.FromArgb(15, 23, 42)))
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                    g.DrawString(_items[i], new Font("Segoe UI", 10F), b, new RectangleF(rect.X + 34, rect.Y, rect.Width - 44, rect.Height), sf);

                y += ItemH;
            }

            using (var p = new Pen(Color.FromArgb(226, 232, 240), 1))
            using (var path = Round(new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f), Radius))
                g.DrawPath(p, path);
        }

        private void OnPopupMouseMove(object sender, MouseEventArgs e)
        {
            int headerH = _showHeaders ? 26 : 0;
            int idx = (e.Y - 6 - headerH) / ItemH;
            if (idx < 0 || idx >= _items.Count) idx = -1;

            if (idx != _hoverIndex)
            {
                _hoverIndex = idx;
                _selectedIndex = idx >= 0 ? idx : _selectedIndex;
                Cursor = idx >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        private void OnPopupMouseClick(object sender, MouseEventArgs e)
        {
            int headerH = _showHeaders ? 26 : 0;
            int idx = (e.Y - 6 - headerH) / ItemH;
            if (idx >= 0 && idx < _items.Count)
            {
                var h = SuggestionChosen;
                if (h != null) h(this, _items[idx]);
                HidePopup();
            }
        }

        private void HidePopup()
        {
            Hide();
            var h = Hidden;
            if (h != null) h(this, EventArgs.Empty);
        }

        private static GraphicsPath Round(RectangleF r, int radius)
        {
            var p = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
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
                const int CS_DROPSHADOW = 0x20000;
                const int WS_EX_NOACTIVATE = 0x08000000;
                var cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }
    }
}
