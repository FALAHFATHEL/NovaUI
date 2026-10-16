using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("TagsChanged")]
    [Description("حقل إدخال وسوم احترافي بدعم كامل لـ RTL/LTR.")]
    public class NovaTagInput : Control
    {
        #region Fileds

        private readonly List<string> _tags = new List<string>();
        private readonly List<string> _suggestions = new List<string>();
        private readonly List<string> _filtered = new List<string>();

        private string _currentInput = "";
        private int _hoverTagIndex = -1;
        private Rectangle _hoverCloseRect = Rectangle.Empty;
        private int _suggestionIndex = 0;
        private bool _suggestionsVisible;
        private NovaSuggestionPopup _popup;

        private int _radius = 10;
        private int _tagHeight = 30;
        private int _tagPadding = 10;
        private int _tagSpacing = 6;
        private int _rowSpacing = 6;
        private int _inputMinWidth = 100;
        private int _innerPaddingX = 12;
        private int _innerPaddingY = 10;
        private int _closeSize = 18;
        private int _closeRightPadding = 6;
        private int _tagRadius = -1; // -1 = pill

        private Color _fillColor = Color.White;
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private int _borderSize = 1;
        private Color _tagBackColor = Color.FromArgb(224, 231, 255);
        private Color _tagHoverBackColor = Color.FromArgb(199, 210, 254);
        private Color _tagTextColor = Color.FromArgb(67, 56, 202);
        private Color _tagCloseColor = Color.FromArgb(99, 102, 241);
        private Color _tagCloseHoverColor = Color.FromArgb(239, 68, 68);
        private Color _tagCloseHoverBackColor = Color.FromArgb(50, 239, 68, 68);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _placeholderColor = Color.FromArgb(148, 163, 184);

        private bool _addOnComma = true;
        private bool _addOnSemicolon = true;
        private bool _removeOnDoubleBackspace = true;
        private bool _allowDuplicates = false;
        private int _maxTags = 0; // 0 = unlimited
        private int _maxTagLength = 0; // 0 = unlimited

        private string _placeholderText = "أضف وسماً واضغط Enter";

        private bool _focused;
        private DateTime _lastBackspaceTime = DateTime.MinValue;
        private readonly TextBox _inner;

        private Font _tagFont;
        private Font _closeFont;

        // Tooltip
        private readonly ToolTip _tooltip = new ToolTip();

        #endregion

        #region Events

        public event EventHandler TagsChanged;
        public event EventHandler<string> TagAdded;
        public event EventHandler<string> TagRemoved;
        public event EventHandler<string> ValidationFailed;

        #endregion

        public NovaTagInput()
        {
            //AutoScaleMode = AutoScaleMode.None;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            base.BackColor = Color.Transparent;
            Size = new Size(400, 52);
            Font = new Font("Segoe UI", 10F);

            _tagFont = new Font("Segoe UI", 10F);
            _closeFont = new Font("Segoe UI", 8F, FontStyle.Bold);

            _inner = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                ForeColor = _textColor,
                BackColor = _fillColor
            };
            _inner.TextChanged += Inner_TextChanged;
            _inner.KeyDown += Inner_KeyDown;
            _inner.GotFocus += (s, e) => { _focused = true; _inner.BackColor = _focusFillColor; Invalidate(); };
            _inner.LostFocus += (s, e) => { _focused = false; _inner.BackColor = _fillColor; HideSuggestions(); Invalidate(); };
            Controls.Add(_inner);
        }

        #region Properties

        [Browsable(false)]
        public List<string> Tags => _tags;

        [Category("NovaTagInput - الاقتراحات")]
        [Description("قائمة الاقتراحات التي تظهر أثناء الكتابة.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public List<string> Suggestions => _suggestions;

        [Category("NovaTagInput - النصوص")]
        [DefaultValue("أضف وسماً واضغط Enter")]
        public string PlaceholderText
        {
            get => _placeholderText;
            set { _placeholderText = value ?? ""; Invalidate(); }
        }

        [Category("NovaTagInput - الأبعاد"), DefaultValue(10)]
        public int BorderRadius
        {
            get => _radius;
            set { _radius = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaTagInput - الأبعاد"), DefaultValue(30)]
        public int TagHeight
        {
            get => _tagHeight;
            set { _tagHeight = Math.Max(20, value); Layout(); Invalidate(); }
        }

        [Category("NovaTagInput - الأبعاد"), DefaultValue(10)]
        public int TagPadding
        {
            get => _tagPadding;
            set { _tagPadding = Math.Max(4, value); Layout(); Invalidate(); }
        }

        [Category("NovaTagInput - الأبعاد"), DefaultValue(6)]
        public int TagSpacing
        {
            get => _tagSpacing;
            set { _tagSpacing = Math.Max(0, value); Layout(); Invalidate(); }
        }

        [Category("NovaTagInput - الأبعاد"), DefaultValue(6)]
        public int RowSpacing
        {
            get => _rowSpacing;
            set { _rowSpacing = Math.Max(0, value); Layout(); Invalidate(); }
        }

        [Category("NovaTagInput - الأبعاد"), DefaultValue(-1)]
        [Description("استدارة الـ Tag. -1 = كبسولة تلقائية.")]
        public int TagBorderRadius
        {
            get => _tagRadius;
            set { _tagRadius = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; if (!_focused) _inner.BackColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color FocusFillColor
        {
            get => _focusFillColor;
            set { _focusFillColor = value; if (_focused) _inner.BackColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color FocusBorderColor
        {
            get => _focusBorderColor;
            set { _focusBorderColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان"), DefaultValue(1)]
        public int BorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color TagBackColor
        {
            get => _tagBackColor;
            set { _tagBackColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color TagHoverBackColor
        {
            get => _tagHoverBackColor;
            set { _tagHoverBackColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color TagTextColor
        {
            get => _tagTextColor;
            set { _tagTextColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color TagCloseColor
        {
            get => _tagCloseColor;
            set { _tagCloseColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - الألوان")]
        public Color TagCloseHoverColor
        {
            get => _tagCloseHoverColor;
            set { _tagCloseHoverColor = value; Invalidate(); }
        }

        [Category("NovaTagInput - السلوك"), DefaultValue(true)]
        [Description("إضافة الوسم عند كتابة فاصلة \",\".")]
        public bool AddOnComma
        {
            get => _addOnComma;
            set => _addOnComma = value;
        }

        [Category("NovaTagInput - السلوك"), DefaultValue(true)]
        [Description("إضافة الوسم عند كتابة فاصلة منقوطة \";\".")]
        public bool AddOnSemicolon
        {
            get => _addOnSemicolon;
            set => _addOnSemicolon = value;
        }

        [Category("NovaTagInput - السلوك"), DefaultValue(true)]
        [Description("حذف آخر وسم عند ضغط Backspace مرتين على حقل فارغ.")]
        public bool RemoveOnDoubleBackspace
        {
            get => _removeOnDoubleBackspace;
            set => _removeOnDoubleBackspace = value;
        }

        [Category("NovaTagInput - السلوك"), DefaultValue(false)]
        [Description("السماح بالوسوم المكررة.")]
        public bool AllowDuplicates
        {
            get => _allowDuplicates;
            set => _allowDuplicates = value;
        }

        [Category("NovaTagInput - السلوك"), DefaultValue(0)]
        [Description("الحد الأقصى لعدد الوسوم. 0 = بلا حد.")]
        public int MaxTags
        {
            get => _maxTags;
            set => _maxTags = Math.Max(0, value);
        }

        [Category("NovaTagInput - السلوك"), DefaultValue(0)]
        [Description("الحد الأقصى لطول الوسم. 0 = بلا حد.")]
        public int MaxTagLength
        {
            get => _maxTagLength;
            set => _maxTagLength = Math.Max(0, value);
        }

        [Browsable(false)]
        public string CurrentInput => _currentInput;

        #endregion

        #region Public API

        public void AddTag(string tag)
        {
            tag = (tag ?? "").Trim();
            if (string.IsNullOrEmpty(tag)) return;

            if (_maxTagLength > 0 && tag.Length > _maxTagLength)
            {
                ValidationFailed?.Invoke(this, $"الحد الأقصى لطول الوسم {_maxTagLength} حرف");
                return;
            }

            if (!_allowDuplicates &&
                _tags.Any(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)))
            {
                _inner.Clear();
                _currentInput = "";
                HideSuggestions();
                RepositionInner();
                Invalidate();
                return;
            }

            if (_maxTags > 0 && _tags.Count >= _maxTags)
            {
                ValidationFailed?.Invoke(this, $"الحد الأقصى {_maxTags} وسم");
                return;
            }

            _tags.Add(tag);
            _inner.Clear();
            _currentInput = "";

            TagAdded?.Invoke(this, tag);
            TagsChanged?.Invoke(this, EventArgs.Empty);

            Layout();
            Invalidate();
        }

        public void AddTags(IEnumerable<string> tags)
        {
            if (tags == null) return;
            foreach (var t in tags) AddTag(t);
        }

        public void RemoveTag(string tag)
        {
            if (_tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) > 0)
            {
                TagRemoved?.Invoke(this, tag);
                TagsChanged?.Invoke(this, EventArgs.Empty);
                Layout();
                Invalidate();
            }
        }

        public void RemoveTagAt(int index)
        {
            if (index < 0 || index >= _tags.Count) return;
            var tag = _tags[index];
            _tags.RemoveAt(index);
            TagRemoved?.Invoke(this, tag);
            TagsChanged?.Invoke(this, EventArgs.Empty);
            Layout();
            Invalidate();
        }

        public void ClearTags()
        {
            if (_tags.Count == 0) return;
            _tags.Clear();
            TagsChanged?.Invoke(this, EventArgs.Empty);
            Layout();
            Invalidate();
        }

        public string[] GetTags() => _tags.ToArray();

        #endregion

        #region Layout

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Layout();
        }

        private void Layout()
        {
            RepositionInner();
        }

        private int MeasureTagWidth(string tag)
        {
            var ts = TextRenderer.MeasureText(tag, _tagFont,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            return ts.Width + _tagPadding * 2 + _closeSize + _closeRightPadding;
        }

        private void RepositionInner()
        {
            if (_inner == null || IsDisposed) return;

            bool rtl = RightToLeft == RightToLeft.Yes;
            int cursorX = rtl ? (Width - _innerPaddingX) : _innerPaddingX;
            int cursorY = _innerPaddingY;
            int maxW = Width - _innerPaddingX * 2;

            foreach (var tag in _tags)
            {
                int tw = MeasureTagWidth(tag);
                if (rtl)
                {
                    if (cursorX - tw < _innerPaddingX)
                    {
                        cursorX = Width - _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                    cursorX -= tw + _tagSpacing;
                }
                else
                {
                    if (cursorX + tw > _innerPaddingX + maxW)
                    {
                        cursorX = _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                    cursorX += tw + _tagSpacing;
                }
            }

            int inputW = Math.Max(_inputMinWidth, maxW - 20);
            int inputX;
            int inputY = cursorY + (_tagHeight - _inner.PreferredHeight) / 2;

            if (rtl)
            {
                inputX = Math.Max(_innerPaddingX, cursorX - inputW);

                if (inputX < _innerPaddingX)
                {
                    inputY += _tagHeight + _rowSpacing;
                    inputX = Width - _innerPaddingX - inputW;
                }
            }
            else
            {
                inputX = cursorX;
                if (inputX + inputW > Width - _innerPaddingX)
                {
                    inputY += _tagHeight + _rowSpacing;
                    inputX = _innerPaddingX;
                }
            }

            inputW = Math.Max(_inputMinWidth, Width - inputX - _innerPaddingX);
            _inner.SetBounds(inputX, inputY, inputW, _inner.PreferredHeight);

            int needed = inputY + _inner.PreferredHeight + _innerPaddingY + 4;
            if (needed > Height) Height = needed;
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (Width <= 2 || Height <= 2) return;

            bool rtl = RightToLeft == RightToLeft.Yes;

            // ============ الخلفية والحدود ============
            Color fill = _focused ? _focusFillColor : _fillColor;
            Color border = _focused ? _focusBorderColor : _borderColor;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Round(rect, _radius))
            {
                using (var b = new SolidBrush(fill))
                    g.FillPath(b, path);

                if (_borderSize > 0)
                    using (var p = new Pen(border, _borderSize))
                        g.DrawPath(p, path);
            }

            DrawTags(g, rtl);

            // ============ Placeholder ============
            if (string.IsNullOrEmpty(_inner.Text) && _tags.Count == 0 && !_focused)
            {
                var placeholderRect = new Rectangle(
                    _inner.Left, _inner.Top,
                    Math.Max(20, Width - _inner.Left - _innerPaddingX), _inner.Height);

                TextRenderer.DrawText(g, _placeholderText, Font, placeholderRect, _placeholderColor,
                    (rtl ? TextFormatFlags.Right : TextFormatFlags.Left) |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);
            }
        }

        private void DrawTags(Graphics g, bool rtl)
        {
            int cursorX = rtl ? (Width - _innerPaddingX) : _innerPaddingX;
            int cursorY = _innerPaddingY;
            int maxW = Width - _innerPaddingX * 2;

            for (int i = 0; i < _tags.Count; i++)
            {
                var tag = _tags[i];
                int tw = MeasureTagWidth(tag);

                // كسر السطر
                if (rtl)
                {
                    if (cursorX - tw < _innerPaddingX)
                    {
                        cursorX = Width - _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                }
                else
                {
                    if (cursorX + tw > _innerPaddingX + maxW)
                    {
                        cursorX = _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                }

                int tagX = rtl ? (cursorX - tw) : cursorX;
                var tagRect = new Rectangle(tagX, cursorY, tw, _tagHeight);

                DrawTag(g, tagRect, tag, i, rtl);

                cursorX += rtl ? -(tw + _tagSpacing) : (tw + _tagSpacing);
            }
        }

        private void DrawTag(Graphics g, Rectangle rect, string tag, int index, bool rtl)
        {
            bool isHover = index == _hoverTagIndex;

            int radius = _tagRadius < 0 ? _tagHeight / 2 : Math.Min(_tagRadius, _tagHeight / 2);

            using (var path = Round(rect, radius))
            using (var b = new SolidBrush(isHover ? _tagHoverBackColor : _tagBackColor))
                g.FillPath(b, path);

            Rectangle closeRect;
            if (rtl)
            {
                closeRect = new Rectangle(
                    rect.X + _closeRightPadding,
                    rect.Y + (rect.Height - _closeSize) / 2,
                    _closeSize, _closeSize);
            }
            else
            {
                closeRect = new Rectangle(
                    rect.Right - _closeSize - _closeRightPadding,
                    rect.Y + (rect.Height - _closeSize) / 2,
                    _closeSize, _closeSize);
            }

            Rectangle textRect;
            if (rtl)
            {
                textRect = new Rectangle(
                    closeRect.Right + 2,
                    rect.Y,
                    Math.Max(10, rect.Right - (closeRect.Right + 2) - _tagPadding),
                    rect.Height);
            }
            else
            {
                textRect = new Rectangle(
                    rect.X + _tagPadding,
                    rect.Y,
                    Math.Max(10, closeRect.Left - (rect.X + _tagPadding) - 2),
                    rect.Height);
            }

            // رسم النص
            TextRenderer.DrawText(g, tag, _tagFont, textRect, _tagTextColor,
                (rtl ? TextFormatFlags.Right : TextFormatFlags.Left) |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoPrefix);

            bool closeHover = isHover && _hoverCloseRect.Contains(PointToClient(Cursor.Position));

            if (closeHover)
            {
                using (var b = new SolidBrush(_tagCloseHoverBackColor))
                using (var path = Round(closeRect, _closeSize / 2))
                    g.FillPath(b, path);
            }

            Color closeColor = closeHover ? _tagCloseHoverColor : _tagCloseColor;

            using (var pen = new Pen(closeColor, 1.6f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                float cx = closeRect.X + closeRect.Width / 2f;
                float cy = closeRect.Y + closeRect.Height / 2f;
                float s = 4f;

                g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
                g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
            }

            if (isHover) _hoverCloseRect = closeRect;
        }

        #endregion

        #region Input Handling

        private void Inner_TextChanged(object sender, EventArgs e)
        {
            _currentInput = _inner.Text;
            UpdateSuggestions();
            RepositionInner();
            Invalidate();
        }

        private void Inner_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.Tab:
                    if (_suggestionsVisible && _suggestionIndex >= 0 && _suggestionIndex < _filtered.Count)
                    {
                        AddTag(_filtered[_suggestionIndex]);
                        e.SuppressKeyPress = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(_currentInput))
                    {
                        AddTag(_currentInput);
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.Back:
                    if (_inner.Text.Length == 0 && _tags.Count > 0)
                    {
                        if (!_removeOnDoubleBackspace)
                        {
                            RemoveTagAt(_tags.Count - 1);
                            e.SuppressKeyPress = true;
                        }
                        else
                        {
                            var now = DateTime.Now;
                            if ((now - _lastBackspaceTime).TotalMilliseconds < 600)
                            {
                                RemoveTagAt(_tags.Count - 1);
                                _lastBackspaceTime = DateTime.MinValue;
                            }
                            else
                            {
                                _lastBackspaceTime = now;
                            }
                        }
                    }
                    break;

                case Keys.Escape:
                    HideSuggestions();
                    break;

                case Keys.Down:
                    if (_suggestionsVisible && _filtered.Count > 0)
                    {
                        _suggestionIndex = (_suggestionIndex + 1) % _filtered.Count;
                        _popup?.SelectIndex(_suggestionIndex);
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.Up:
                    if (_suggestionsVisible && _filtered.Count > 0)
                    {
                        _suggestionIndex = (_suggestionIndex - 1 + _filtered.Count) % _filtered.Count;
                        _popup?.SelectIndex(_suggestionIndex);
                        e.SuppressKeyPress = true;
                    }
                    break;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if ((_addOnComma && e.KeyChar == ',') ||
                (_addOnSemicolon && e.KeyChar == ';'))
            {
                if (!string.IsNullOrWhiteSpace(_currentInput))
                {
                    AddTag(_currentInput);
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region Suggestions

        private void UpdateSuggestions()
        {
            if (string.IsNullOrWhiteSpace(_currentInput))
            {
                HideSuggestions();
                return;
            }

            _filtered.Clear();
            var q = _currentInput.Trim().ToLowerInvariant();

            foreach (var s in _suggestions)
            {
                if (s.ToLowerInvariant().Contains(q) &&
                    !_tags.Any(t => string.Equals(t, s, StringComparison.OrdinalIgnoreCase)))
                {
                    _filtered.Add(s);
                }
            }

            if (_filtered.Count == 0)
            {
                HideSuggestions();
                return;
            }

            _suggestionIndex = 0;
            ShowSuggestions();
        }

        private void ShowSuggestions()
        {
            if (_popup == null || _popup.IsDisposed)
            {
                _popup = new NovaSuggestionPopup(_filtered, RightToLeft);
                _popup.ItemChosen += (s, value) => { AddTag(value); _inner.Focus(); };
                _popup.Closed += (s, e) => _suggestionsVisible = false;
            }
            else
            {
                _popup.UpdateItems(_filtered);
            }

            _suggestionsVisible = true;

            bool rtl = RightToLeft == RightToLeft.Yes;
            int x = rtl ? (PointToScreen(new Point(_inner.Left, 0)).X) :
                          (PointToScreen(new Point(_inner.Left, 0)).X);
            int y = PointToScreen(new Point(0, _inner.Bottom + 4)).Y;

            _popup.Location = new Point(x, y);
            _popup.Width = Math.Max(200, _inner.Width);

            if (!_popup.Visible) _popup.Show();
        }

        private void HideSuggestions()
        {
            _suggestionsVisible = false;
            _popup?.Hide();
        }

        #endregion

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool rtl = RightToLeft == RightToLeft.Yes;
            int cursorX = rtl ? (Width - _innerPaddingX) : _innerPaddingX;
            int cursorY = _innerPaddingY;
            int maxW = Width - _innerPaddingX * 2;

            int newIdx = -1;
            Rectangle newCR = Rectangle.Empty;

            for (int i = 0; i < _tags.Count; i++)
            {
                int tw = MeasureTagWidth(_tags[i]);

                if (rtl)
                {
                    if (cursorX - tw < _innerPaddingX)
                    {
                        cursorX = Width - _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                    var tagRect = new Rectangle(cursorX - tw, cursorY, tw, _tagHeight);
                    if (tagRect.Contains(e.Location))
                    {
                        newIdx = i;
                        newCR = new Rectangle(
                            tagRect.X + _closeRightPadding,
                            tagRect.Y + (tagRect.Height - _closeSize) / 2,
                            _closeSize, _closeSize);
                        break;
                    }
                    cursorX -= tw + _tagSpacing;
                }
                else
                {
                    if (cursorX + tw > _innerPaddingX + maxW)
                    {
                        cursorX = _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                    var tagRect = new Rectangle(cursorX, cursorY, tw, _tagHeight);
                    if (tagRect.Contains(e.Location))
                    {
                        newIdx = i;
                        newCR = new Rectangle(
                            tagRect.Right - _closeSize - _closeRightPadding,
                            tagRect.Y + (tagRect.Height - _closeSize) / 2,
                            _closeSize, _closeSize);
                        break;
                    }
                    cursorX += tw + _tagSpacing;
                }
            }

            if (newIdx != _hoverTagIndex)
            {
                _hoverTagIndex = newIdx;
                _hoverCloseRect = newCR;
                Cursor = newIdx >= 0 ? Cursors.Hand : Cursors.IBeam;
                Invalidate();
            }
            else if (newIdx >= 0)
            {
                _hoverCloseRect = newCR;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverTagIndex != -1)
            {
                _hoverTagIndex = -1;
                _hoverCloseRect = Rectangle.Empty;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            bool rtl = RightToLeft == RightToLeft.Yes;
            int cursorX = rtl ? (Width - _innerPaddingX) : _innerPaddingX;
            int cursorY = _innerPaddingY;
            int maxW = Width - _innerPaddingX * 2;

            for (int i = 0; i < _tags.Count; i++)
            {
                int tw = MeasureTagWidth(_tags[i]);

                if (rtl)
                {
                    if (cursorX - tw < _innerPaddingX)
                    {
                        cursorX = Width - _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                    var tagRect = new Rectangle(cursorX - tw, cursorY, tw, _tagHeight);
                    var closeRect = new Rectangle(
                        tagRect.X + _closeRightPadding,
                        tagRect.Y + (tagRect.Height - _closeSize) / 2,
                        _closeSize, _closeSize);

                    if (closeRect.Contains(e.Location)) { RemoveTagAt(i); return; }
                    cursorX -= tw + _tagSpacing;
                }
                else
                {
                    if (cursorX + tw > _innerPaddingX + maxW)
                    {
                        cursorX = _innerPaddingX;
                        cursorY += _tagHeight + _rowSpacing;
                    }
                    var tagRect = new Rectangle(cursorX, cursorY, tw, _tagHeight);
                    var closeRect = new Rectangle(
                        tagRect.Right - _closeSize - _closeRightPadding,
                        tagRect.Y + (tagRect.Height - _closeSize) / 2,
                        _closeSize, _closeSize);

                    if (closeRect.Contains(e.Location)) { RemoveTagAt(i); return; }
                    cursorX += tw + _tagSpacing;
                }
            }

            _inner.Focus();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            _inner.RightToLeft = RightToLeft;
            Layout();
            Invalidate();
        }

        #endregion

        #region Helpers

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _popup?.Dispose();
                _inner?.Dispose();
                _tagFont?.Dispose();
                _closeFont?.Dispose();
                _tooltip?.Dispose();
            }
            base.Dispose(disposing);
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

        #endregion
    }

    #region Popup

    internal class NovaSuggestionPopup : Form
    {
        private List<string> _items;
        private int _selectedIndex, _hoverIndex = -1;
        public event EventHandler<string> ItemChosen;
        public event EventHandler Closed;

        private const int ItemH = 34;
        private const int Radius = 8;
        private const int MaxItems = 6;

        private readonly bool _rtl;
        private Font _font;

        public NovaSuggestionPopup(List<string> items, RightToLeft rtl)
        {
            _items = items;
            _rtl = rtl == RightToLeft.Yes;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            BackColor = Color.White;
            TopMost = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            _font = new Font("Segoe UI", 9.5F);

            MouseMove += (s, e) =>
            {
                int i = (e.Y - 6) / ItemH;
                if (i < 0 || i >= _items.Count) i = -1;
                if (i != _hoverIndex)
                {
                    _hoverIndex = i;
                    Cursor = i >= 0 ? Cursors.Hand : Cursors.Default;
                    Invalidate();
                }
            };
            MouseClick += (s, e) =>
            {
                int i = (e.Y - 6) / ItemH;
                if (i >= 0 && i < _items.Count)
                {
                    ItemChosen?.Invoke(this, _items[i]);
                    Hide();
                }
            };
            Deactivate += (s, e) => { Hide(); Closed?.Invoke(this, EventArgs.Empty); };

            UpdateItems(items);
        }

        public void UpdateItems(List<string> items)
        {
            _items = items;
            _selectedIndex = 0;
            _hoverIndex = -1;
            Height = Math.Min(_items.Count, MaxItems) * ItemH + 12;
            Invalidate();
        }

        public void SelectIndex(int i)
        {
            _selectedIndex = i;
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            using (var path = Round(new Rectangle(0, 0, Width, Height), Radius))
                Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.White);

            int y = 6;
            for (int i = 0; i < _items.Count && i < MaxItems; i++)
            {
                var rect = new Rectangle(4, y, Width - 8, ItemH - 2);

                if (i == _selectedIndex)
                    using (var b = new SolidBrush(Color.FromArgb(238, 242, 255)))
                    using (var path = Round(rect, 6))
                        g.FillPath(b, path);
                else if (i == _hoverIndex)
                    using (var b = new SolidBrush(Color.FromArgb(248, 250, 252)))
                    using (var path = Round(rect, 6))
                        g.FillPath(b, path);

                TextRenderer.DrawText(g, _items[i], _font, rect, Color.FromArgb(15, 23, 42),
                    (_rtl ? TextFormatFlags.Right : TextFormatFlags.Left) |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding);

                y += ItemH;
            }

            using (var p = new Pen(Color.FromArgb(226, 232, 240), 1))
            using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
                g.DrawPath(p, path);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) _font?.Dispose();
            base.Dispose(disposing);
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

    #endregion
}