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
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("عنصر واحد داخل NovaListBox.")]
    public class NovaListBoxItem
    {
        private string _text = "";
        private string _description = "";
        private string _icon = "";
        private string _iconFontName = "Segoe MDL2 Assets";
        private string _iconPackName = "";
        private Color _iconColor = Color.FromArgb(99, 102, 241);
        private object _tag;

        public NovaListBoxItem() { }

        public NovaListBoxItem(string text, string description = null, string icon = null)
        {
            _text = text ?? "";
            _description = description ?? "";
            _icon = icon ?? "";
        }

        [Category("NovaListBoxItem"), DefaultValue("")]
        public string Text { get { return _text; } set { _text = value ?? ""; } }

        [Category("NovaListBoxItem"), DefaultValue("")]
        public string Description { get { return _description; } set { _description = value ?? ""; } }

        [Category("NovaListBoxItem"), DefaultValue("")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        public string Icon { get { return _icon; } set { _icon = value ?? ""; } }

        [Category("NovaListBoxItem"), DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName
        {
            get { return _iconFontName; }
            set { _iconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; }
        }

        [Category("NovaListBoxItem"), DefaultValue("")]
        public string IconPackName { get { return _iconPackName; } set { _iconPackName = value ?? ""; } }

        [Category("NovaListBoxItem")]
        public Color IconColor { get { return _iconColor; } set { _iconColor = value; } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Tag { get { return _tag; } set { _tag = value; } }

        public override string ToString()
        {
            return string.IsNullOrEmpty(_text) ? "(EMPTY)" : _text;
        }
    }

    [ToolboxItem(true)]
    [DefaultEvent("SelectedIndexChanged")]
    [Description("قائمة مخصصة احترافية مع أيقونات ووصف وتحديد متعدد ودعم كامل لـ RTL و LTR.")]
    public class NovaListBox : Control
    {
        private readonly List<NovaListBoxItem> _items = new List<NovaListBoxItem>();
        private readonly List<int> _selectedIndices = new List<int>();

        private int _hoverIndex = -1;
        private int _itemHeight = 52;
        private int _radius = 10;
        private int _itemRadius = 8;
        private int _padding = 8;

        private bool _multiSelect = false;
        private bool _showCheckboxes = false;

        private string _defaultIconFontName = "Segoe MDL2 Assets";
        private string _defaultIconPackName = "";

        private Color _fillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _itemHoverBack = Color.FromArgb(248, 250, 252);
        private Color _itemSelectedBack = Color.FromArgb(238, 242, 255);
        private Color _itemSelectedTextColor = Color.FromArgb(67, 56, 202);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _descColor = Color.FromArgb(100, 116, 139);
        private Color _iconColor = Color.FromArgb(99, 102, 241);
        private Color _checkboxBorderColor = Color.FromArgb(203, 213, 225);
        private Color _checkboxCheckColor = Color.White;
        private Color _checkboxFillColor = Color.FromArgb(99, 102, 241);
        private Color _emptyColor = Color.FromArgb(148, 163, 184);

        private VScrollBar _scroll;
        private int _scrollOffset;

        public event EventHandler SelectedIndexChanged;
        public event EventHandler<NovaListBoxItem> ItemDoubleClicked;

        public NovaListBox()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(320, 300);
            Font = new Font("Segoe UI", 10F);
            Cursor = Cursors.Hand;

            _scroll = new VScrollBar { Dock = DockStyle.Right, Visible = false, Width = 12 };
            _scroll.ValueChanged += (s, e) => { _scrollOffset = _scroll.Value; Invalidate(); };
            Controls.Add(_scroll);
        }

        [Category("NovaListBox")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public List<NovaListBoxItem> Items
        {
            get { return _items; }
            set
            {
                _items.Clear();
                if (value != null) _items.AddRange(value);
                UpdateScroll();
                Invalidate();
            }
        }

        [Category("NovaListBox - Icon"), DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName
        {
            get { return _defaultIconFontName; }
            set { _defaultIconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); }
        }

        [Category("NovaListBox - Icon"), DefaultValue("")]
        public string IconPackName
        {
            get { return _defaultIconPackName; }
            set { _defaultIconPackName = value ?? ""; Invalidate(); }
        }

        [Category("NovaListBox"), DefaultValue(52)]
        public int ItemHeight { get { return _itemHeight; } set { _itemHeight = Math.Max(28, value); UpdateScroll(); Invalidate(); } }

        [Category("NovaListBox"), DefaultValue(10)]
        public int BorderRadius { get { return _radius; } set { _radius = value; Invalidate(); } }

        [Category("NovaListBox"), DefaultValue(8)]
        public int ItemRadius { get { return _itemRadius; } set { _itemRadius = value; Invalidate(); } }

        [Category("NovaListBox"), DefaultValue(8)]
        public int Padding_Items { get { return _padding; } set { _padding = value; UpdateScroll(); Invalidate(); } }

        [Category("NovaListBox"), DefaultValue(false)]
        public bool MultiSelect
        {
            get { return _multiSelect; }
            set
            {
                _multiSelect = value;
                if (!value && _selectedIndices.Count > 1)
                {
                    int first = _selectedIndices[0];
                    _selectedIndices.Clear();
                    _selectedIndices.Add(first);
                }
                Invalidate();
            }
        }

        [Category("NovaListBox"), DefaultValue(false)]
        public bool ShowCheckboxes { get { return _showCheckboxes; } set { _showCheckboxes = value; Invalidate(); } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get { return _selectedIndices.Count > 0 ? _selectedIndices[0] : -1; }
            set
            {
                _selectedIndices.Clear();
                if (value >= 0 && value < _items.Count) _selectedIndices.Add(value);
                Invalidate();
                var h = SelectedIndexChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NovaListBoxItem SelectedItem
        {
            get { return SelectedIndex >= 0 && SelectedIndex < _items.Count ? _items[SelectedIndex] : null; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<NovaListBoxItem> SelectedItems
        {
            get { return _selectedIndices.Where(i => i >= 0 && i < _items.Count).Select(i => _items[i]).ToList(); }
        }

        [Category("NovaListBox")] public Color FillColor { get { return _fillColor; } set { _fillColor = value; Invalidate(); } }
        [Category("NovaListBox")] public Color BorderColor { get { return _borderColor; } set { _borderColor = value; Invalidate(); } }
        [Category("NovaListBox")] public Color ItemHoverBackColor { get { return _itemHoverBack; } set { _itemHoverBack = value; Invalidate(); } }
        [Category("NovaListBox")] public Color ItemSelectedBackColor { get { return _itemSelectedBack; } set { _itemSelectedBack = value; Invalidate(); } }
        [Category("NovaListBox")] public Color ItemSelectedTextColor { get { return _itemSelectedTextColor; } set { _itemSelectedTextColor = value; Invalidate(); } }
        [Category("NovaListBox")] public Color TextColor { get { return _textColor; } set { _textColor = value; Invalidate(); } }
        [Category("NovaListBox")] public Color DescriptionColor { get { return _descColor; } set { _descColor = value; Invalidate(); } }
        [Category("NovaListBox")] public Color IconColor { get { return _iconColor; } set { _iconColor = value; Invalidate(); } }
        [Category("NovaListBox")] public Color CheckboxBorderColor { get { return _checkboxBorderColor; } set { _checkboxBorderColor = value; Invalidate(); } }
        [Category("NovaListBox")] public Color CheckboxFillColor { get { return _checkboxFillColor; } set { _checkboxFillColor = value; Invalidate(); } }

        public void AddItem(NovaListBoxItem item)
        {
            if (item == null) return;
            _items.Add(item);
            UpdateScroll();
            Invalidate();
        }

        public void AddItem(string text, string description = null, string icon = null)
        {
            AddItem(new NovaListBoxItem(text, description, icon));
        }

        public void AddItems(IEnumerable<NovaListBoxItem> items)
        {
            if (items == null) return;
            foreach (var item in items) _items.Add(item);
            UpdateScroll();
            Invalidate();
        }

        public void ClearItems()
        {
            _items.Clear();
            _selectedIndices.Clear();
            _hoverIndex = -1;
            UpdateScroll();
            Invalidate();
        }

        public void RemoveItemAt(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            _items.RemoveAt(index);
            _selectedIndices.RemoveAll(i => i == index);
            for (int i = 0; i < _selectedIndices.Count; i++)
                if (_selectedIndices[i] > index) _selectedIndices[i]--;
            UpdateScroll();
            Invalidate();
        }

        public void ClearSelection()
        {
            _selectedIndices.Clear();
            Invalidate();
            var h = SelectedIndexChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        public void SelectAll()
        {
            if (!_multiSelect) return;
            _selectedIndices.Clear();
            for (int i = 0; i < _items.Count; i++) _selectedIndices.Add(i);
            Invalidate();
            var h = SelectedIndexChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        private void UpdateScroll()
        {
            if (_scroll == null) return;
            int viewH = Height - _padding * 2;
            int totalH = _items.Count * _itemHeight;

            _scroll.Visible = totalH > viewH;

            if (_scroll.Visible)
            {
                _scroll.Minimum = 0;
                _scroll.Maximum = Math.Max(0, totalH - viewH + _scroll.LargeChange - 1);
                _scroll.LargeChange = viewH;
                _scroll.SmallChange = _itemHeight;

                if (_scrollOffset > _scroll.Maximum) _scroll.Value = _scroll.Maximum;
            }
            else _scrollOffset = 0;
        }

        private string GetItemFontKey(NovaListBoxItem item)
        {
            if (item == null) return _defaultIconFontName;
            if (!string.IsNullOrEmpty(item.IconPackName)) return item.IconPackName;
            if (!string.IsNullOrEmpty(item.IconFontName)) return item.IconFontName;
            if (!string.IsNullOrEmpty(_defaultIconPackName)) return _defaultIconPackName;
            return _defaultIconFontName;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), _radius))
            using (var b = new SolidBrush(_fillColor))
                g.FillPath(b, path);

            if (_items.Count == 0)
            {
                using (var b = new SolidBrush(_emptyColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("No Element Found", Font, b, ClientRectangle, sf);
            }
            else
            {
                int viewportH = Height - _padding * 2;
                int startY = _padding - _scrollOffset;

                var oldClip = g.Clip;
                g.SetClip(new Rectangle(_padding, _padding, Width - _padding * 2, viewportH));

                for (int i = 0; i < _items.Count; i++)
                {
                    var itemRect = new Rectangle(_padding, startY + i * _itemHeight,
                        Width - _padding * 2, _itemHeight - 4);

                    if (itemRect.Bottom < 0 || itemRect.Top > Height) continue;
                    DrawItem(g, itemRect, i);
                }

                g.Clip = oldClip;
            }

            using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), _radius))
            using (var p = new Pen(_borderColor, 1))
                g.DrawPath(p, path);
        }

        private void DrawItem(Graphics g, Rectangle rect, int index)
        {
            var item = _items[index];
            bool isSelected = _selectedIndices.Contains(index);
            bool isHover = index == _hoverIndex && !isSelected;

            if (isSelected)
            {
                using (var path = Round(rect, _itemRadius))
                using (var b = new SolidBrush(_itemSelectedBack))
                    g.FillPath(b, path);
            }
            else if (isHover)
            {
                using (var path = Round(rect, _itemRadius))
                using (var b = new SolidBrush(_itemHoverBack))
                    g.FillPath(b, path);
            }

            bool isRtl = RightToLeft == RightToLeft.Yes;

            if (isRtl)
            {
                int currentRight = rect.Right - 12;
                DrawCheckboxIfNeeded(g, rect, isSelected, true, ref currentRight);
                DrawIconIfNeeded(g, rect, item, true, ref currentRight);
                DrawTextRtl(g, rect, item, isSelected, currentRight);
            }
            else
            {
                int contentX = rect.X + 12;
                DrawCheckboxIfNeeded(g, rect, isSelected, false, ref contentX);
                DrawIconIfNeeded(g, rect, item, false, ref contentX);
                DrawTextLtr(g, rect, item, isSelected, contentX);
            }
        }

        private void DrawCheckboxIfNeeded(Graphics g, Rectangle rect, bool isSelected, bool rtl, ref int pos)
        {
            if (!_showCheckboxes) return;

            int cbSize = 20;
            int cbX = rtl ? pos - cbSize : pos;
            var cbRect = new Rectangle(cbX, rect.Y + (rect.Height - cbSize) / 2, cbSize, cbSize);

            using (var path = Round(cbRect, 6))
            {
                if (isSelected)
                {
                    using (var b = new SolidBrush(_checkboxFillColor)) g.FillPath(b, path);
                    using (var p = new Pen(_checkboxFillColor, 1.5f)) g.DrawPath(p, path);
                }
                else
                {
                    using (var b = new SolidBrush(Color.White)) g.FillPath(b, path);
                    using (var p = new Pen(_checkboxBorderColor, 1.5f)) g.DrawPath(p, path);
                }
            }

            if (isSelected)
            {
                using (var pen = new Pen(_checkboxCheckColor, 2f))
                {
                    pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                    int cx = cbRect.X + cbRect.Width / 2;
                    int cy = cbRect.Y + cbRect.Height / 2;
                    g.DrawLines(pen, new[]
                    {
                        new PointF(cx - 4, cy),
                        new PointF(cx - 1, cy + 3),
                        new PointF(cx + 4, cy - 3)
                    });
                }
            }

            pos += rtl ? -(cbSize + 10) : (cbSize + 10);
        }

        private void DrawIconIfNeeded(Graphics g, Rectangle rect, NovaListBoxItem item, bool rtl, ref int pos)
        {
            if (string.IsNullOrEmpty(item.Icon)) return;

            var iconRect = rtl
                ? new Rectangle(pos - 28, rect.Y + (rect.Height - 28) / 2, 28, 28)
                : new Rectangle(pos, rect.Y + (rect.Height - 28) / 2, 28, 28);

            Color iconClr = item.IconColor.A == 255 ? item.IconColor : _iconColor;

            using (var path = Round(iconRect, 8))
            using (var b = new SolidBrush(Color.FromArgb(30, iconClr)))
                g.FillPath(b, path);

            try
            {
                using (var f = FontManager.GetIconFont(GetItemFontKey(item), 12F))
                using (var b = new SolidBrush(iconClr))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(item.Icon, f, b, iconRect, sf);
            }
            catch { }

            pos += rtl ? -38 : 38;
        }

        private void DrawTextRtl(Graphics g, Rectangle rect, NovaListBoxItem item, bool isSelected, int currentRight)
        {
            bool hasDesc = !string.IsNullOrEmpty(item.Description);
            Color textCol = isSelected ? _itemSelectedTextColor : _textColor;
            float textW = Math.Max(10, currentRight - (rect.X + 12));
            var area = new RectangleF(rect.X + 12, rect.Y, textW, rect.Height);

            if (hasDesc)
            {
                using (var b = new SolidBrush(textCol))
                using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near })
                {
                    sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    g.DrawString(item.Text, new Font(Font, FontStyle.Bold), b,
                        new RectangleF(area.X, rect.Y + 10, area.Width, 20), sf);
                }
                using (var b = new SolidBrush(_descColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter })
                {
                    sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    g.DrawString(item.Description, new Font("Segoe UI", 8.5F), b,
                        new RectangleF(area.X, rect.Y + rect.Height / 2 + 2, area.Width, 20), sf);
                }
            }
            else
            {
                using (var b = new SolidBrush(textCol))
                using (var sf = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                {
                    sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    g.DrawString(item.Text, Font, b, area, sf);
                }
            }
        }

        private void DrawTextLtr(Graphics g, Rectangle rect, NovaListBoxItem item, bool isSelected, int contentX)
        {
            bool hasDesc = !string.IsNullOrEmpty(item.Description);
            Color textCol = isSelected ? _itemSelectedTextColor : _textColor;
            var textAreaW = rect.Right - contentX - 12;

            if (hasDesc)
            {
                using (var b = new SolidBrush(textCol))
                    g.DrawString(item.Text, new Font(Font, FontStyle.Bold), b, contentX, rect.Y + 10);

                using (var b = new SolidBrush(_descColor))
                using (var sf = new StringFormat { Trimming = StringTrimming.EllipsisCharacter })
                    g.DrawString(item.Description, new Font("Segoe UI", 8.5F), b,
                        new RectangleF(contentX, rect.Y + rect.Height / 2 + 2, textAreaW, 20), sf);
            }
            else
            {
                using (var b = new SolidBrush(textCol))
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                    g.DrawString(item.Text, Font, b, new RectangleF(contentX, rect.Y, textAreaW, rect.Height), sf);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = GetIndexAtPoint(e.Location);
            if (idx != _hoverIndex)
            {
                _hoverIndex = idx;
                Cursor = idx >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            int idx = GetIndexAtPoint(e.Location);
            if (idx < 0) return;

            if (_multiSelect)
            {
                if (ModifierKeys.HasFlag(Keys.Control) || _showCheckboxes)
                {
                    if (_selectedIndices.Contains(idx)) _selectedIndices.Remove(idx);
                    else _selectedIndices.Add(idx);
                }
                else if (ModifierKeys.HasFlag(Keys.Shift) && _selectedIndices.Count > 0)
                {
                    int anchor = _selectedIndices[_selectedIndices.Count - 1];
                    int from = Math.Min(anchor, idx);
                    int to = Math.Max(anchor, idx);
                    _selectedIndices.Clear();
                    for (int i = from; i <= to; i++) _selectedIndices.Add(i);
                }
                else
                {
                    _selectedIndices.Clear();
                    _selectedIndices.Add(idx);
                }
            }
            else
            {
                _selectedIndices.Clear();
                _selectedIndices.Add(idx);
            }

            Invalidate();
            var h = SelectedIndexChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int idx = GetIndexAtPoint(e.Location);
            if (idx >= 0)
            {
                var h = ItemDoubleClicked;
                if (h != null) h(this, _items[idx]);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!_scroll.Visible) return;

            int newVal = _scroll.Value - e.Delta;
            newVal = Math.Max(_scroll.Minimum, Math.Min(_scroll.Maximum, newVal));
            _scroll.Value = newVal;
        }

        private int GetIndexAtPoint(Point p)
        {
            if (p.X < _padding || p.X > Width - _padding) return -1;
            if (p.Y < _padding || p.Y > Height - _padding) return -1;

            int relY = p.Y - _padding + _scrollOffset;
            int idx = relY / _itemHeight;
            if (idx < 0 || idx >= _items.Count) return -1;
            return idx;
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); UpdateScroll(); }
        protected override void OnRightToLeftChanged(EventArgs e) { base.OnRightToLeftChanged(e); Invalidate(); }

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