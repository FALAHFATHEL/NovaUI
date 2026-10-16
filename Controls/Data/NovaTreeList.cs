using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace NovaUI
{
    #region NovaTreeListNode

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class NovaTreeListNode
    {
        private string _text = "";
        private string _icon = "";
        private string _iconFontName = "Segoe MDL2 Assets";
        private Color _iconColor = Color.Empty;
        private bool _expanded;

        public NovaTreeListNode() { }

        public NovaTreeListNode(string text = "", string icon = "")
        {
            _text = text ?? "";
            _icon = icon ?? "";
        }

        [Category("Node")]
        [DisplayName("Text")]
        [DefaultValue("")]
        public string Text { get => _text; set => _text = value ?? ""; }

        [Category("Node")]
        [DisplayName("Icon")]
        [Description("رمز MDL2 (مثال: \\uE8B7).")]
        [DefaultValue("")]
        public string Icon { get => _icon; set => _icon = value ?? ""; }

        [Category("Node")]
        [DisplayName("IconFont")]
        [DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName { get => _iconFontName; set => _iconFontName = value ?? "Segoe MDL2 Assets"; }

        [Category("Node")]
        [DisplayName("IconColor")]
        public Color NodeIconColor { get => _iconColor; set => _iconColor = value; }

        [Category("Node")]
        [DisplayName("Expanded")]
        [DefaultValue(false)]
        public bool Expanded { get => _expanded; set => _expanded = value; }

        [Browsable(false)]
        public Dictionary<string, string> Cells { get; set; } = new Dictionary<string, string>();

        [Browsable(false)]
        public bool HasChildren => Children.Count > 0;

        [Browsable(false)]
        public NovaTreeListNode Parent { get; set; }

        [Browsable(false)]
        public List<NovaTreeListNode> Children { get; } = new List<NovaTreeListNode>();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Tag { get; set; }

        public NovaTreeListNode AddChild(NovaTreeListNode child)
        {
            if (child == null) return null;
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        public NovaTreeListNode AddChild(string text, string icon = "")
        {
            var node = new NovaTreeListNode(text, icon) { Parent = this };
            Children.Add(node);
            return node;
        }

        public override string ToString() =>
            string.IsNullOrEmpty(_text) ? "(empty)" : _text;
    }

    #endregion

    #region NovaTreeListColumn

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class NovaTreeListColumn
    {
        private string _header = "";
        private int _width = 120;
        private int _minWidth = 60;
        private ContentAlignment _alignment = ContentAlignment.MiddleLeft;
        private string _cellKey = "";

        public NovaTreeListColumn() { }

        public NovaTreeListColumn(string header, string cellKey, int width = 120)
        {
            _header = header;
            _cellKey = cellKey;
            _width = width;
        }

        [Category("Column")]
        [DisplayName("Header")]
        [DefaultValue("")]
        public string Header { get => _header; set => _header = value ?? ""; }

        [Category("Column")]
        [DisplayName("Width")]
        [DefaultValue(120)]
        public int Width { get => _width; set => _width = Math.Max(20, value); }

        [Category("Column")]
        [DisplayName("MinWidth")]
        [DefaultValue(60)]
        public int MinWidth { get => _minWidth; set => _minWidth = Math.Max(20, value); }

        [Category("Column")]
        [DisplayName("Alignment")]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment Alignment { get => _alignment; set => _alignment = value; }

        [Category("Column")]
        [DisplayName("CellKey")]
        [DefaultValue("")]
        public string CellKey { get => _cellKey; set => _cellKey = value ?? ""; }

        public override string ToString() =>
            string.IsNullOrEmpty(_header) ? "(Without TITLE)" : _header;
    }

    #endregion

    [ToolboxItem(true)]
    [DefaultEvent("NodeSelected")]
    [Description("شجرة بجدول أعمدة احترافية مع دعم RTL/LTR وخصائص كاملة.")]
    public class NovaTreeList : Control
    {
        #region Fileds

        private readonly List<NovaTreeListNode> _roots = new List<NovaTreeListNode>();
        private readonly List<NovaTreeListColumn> _columns = new List<NovaTreeListColumn>();
        private readonly List<NovaTreeListNode> _visibleNodes = new List<NovaTreeListNode>();
        private readonly List<Rectangle> _expandRects = new List<Rectangle>();

        private NovaTreeListNode _selectedNode;
        private NovaTreeListNode _hoverNode;
        private int _hoverRowIndex = -1;
        private Rectangle _hoverExpandRect;

        private int _rowHeight = 38;
        private int _headerHeight = 42;
        private int _indentWidth = 22;
        private int _iconSize = 16;
        private int _firstColumnPadding = 12;
        private int _cellPadding = 12;
        private int _expandIconSize = 10;
        private int _borderRadius = 12;
        private bool _roundedBorder = true;

        private int _outerBorderSize = 1;
        private Color _outerBorderColor = Color.FromArgb(226, 232, 240);

        private Color _backColor = Color.White;
        private Color _headerBackColor = Color.FromArgb(248, 250, 252);
        private Color _headerBackColor2 = Color.FromArgb(241, 245, 249);
        private bool _headerGradient = false;
        private Color _headerTextColor = Color.FromArgb(15, 23, 42);
        private Color _headerBorderColor = Color.FromArgb(226, 232, 240);
        private Color _rowHoverColor = Color.FromArgb(248, 250, 252);
        private Color _rowSelectedColor = Color.FromArgb(238, 242, 255);
        private Color _rowSelectedTextColor = Color.FromArgb(67, 56, 202);
        private Color _textColor = Color.FromArgb(51, 65, 85);
        private Color _expandColor = Color.FromArgb(100, 116, 139);
        private Color _iconColor = Color.FromArgb(99, 102, 241);
        private Color _lineColor = Color.FromArgb(241, 245, 249);
        private bool _showRowLines = true;

        private Font _headerFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private Font _rowFont = new Font("Segoe UI", 9.5F);
        private Font _expandFont = new Font("Segoe MDL2 Assets", 8F);
        private readonly Dictionary<string, Font> _iconFontCache = new Dictionary<string, Font>();

        private VScrollBar _vScroll;
        private int _scrollOffset = 0;

        private bool _showTooltips = true;
        private bool _selectableRows = true;
        private bool _allowKeyboardNavigation = true;

        private readonly ToolTip _tooltip = new ToolTip();

        #endregion

        #region Events

        public event EventHandler<NovaTreeListNode> NodeSelected;
        public event EventHandler<NovaTreeListNode> NodeExpandedChanged;
        public event EventHandler<NovaTreeListNode> NodeDoubleClicked;

        #endregion

        public NovaTreeList()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            base.BackColor = Color.Transparent;
            Size = new Size(700, 400);
            Font = new Font("Segoe UI", 9.5F);
            Cursor = Cursors.Default;
            TabStop = true;

            _vScroll = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false,
                Width = 12
            };
            _vScroll.ValueChanged += (s, e) => { _scrollOffset = _vScroll.Value; Invalidate(); };
            Controls.Add(_vScroll);

            _columns.Add(new NovaTreeListColumn
            {
                Header = "الاسم",
                Width = 300,
                CellKey = "__text__"
            });
        }

        #region Properties

        [Browsable(false)]
        public List<NovaTreeListNode> Roots => _roots;

        [Category("NovaTreeList")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public List<NovaTreeListColumn> Columns => _columns;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NovaTreeListNode SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode == value) return;
                _selectedNode = value;
                Invalidate();
                NodeSelected?.Invoke(this, value);
            }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(38)]
        public int RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = Math.Max(24, value); UpdateScroll(); Invalidate(); }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(42)]
        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = Math.Max(24, value); UpdateScroll(); Invalidate(); }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(22)]
        public int IndentWidth
        {
            get => _indentWidth;
            set { _indentWidth = value; Invalidate(); }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(16)]
        public int IconSize
        {
            get => _iconSize;
            set { _iconSize = Math.Max(8, value); Invalidate(); }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(12)]
        public int FirstColumnPadding
        {
            get => _firstColumnPadding;
            set { _firstColumnPadding = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(12)]
        public int CellPadding
        {
            get => _cellPadding;
            set { _cellPadding = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(12)]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
        }

        [Category("NovaTreeList - Dim"), DefaultValue(true)]
        public bool RoundedBorder
        {
            get => _roundedBorder;
            set { _roundedBorder = value; UpdateRegion(); Invalidate(); }
        }

        [Category("NovaTreeList - Borders"), DefaultValue(1)]
        public int OuterBorderSize
        {
            get => _outerBorderSize;
            set { _outerBorderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaTreeList - Borders")]
        public Color OuterBorderColor
        {
            get => _outerBorderColor;
            set { _outerBorderColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Header")]
        public Color HeaderBackColor
        {
            get => _headerBackColor;
            set { _headerBackColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Header")]
        public Color HeaderBackColor2
        {
            get => _headerBackColor2;
            set { _headerBackColor2 = value; Invalidate(); }
        }

        [Category("NovaTreeList - Header"), DefaultValue(false)]
        public bool HeaderGradient
        {
            get => _headerGradient;
            set { _headerGradient = value; Invalidate(); }
        }

        [Category("NovaTreeList - Header")]
        public Color HeaderTextColor
        {
            get => _headerTextColor;
            set { _headerTextColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Header")]
        public Color HeaderBorderColor
        {
            get => _headerBorderColor;
            set { _headerBorderColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Header")]
        public Font HeaderFont
        {
            get => _headerFont;
            set { _headerFont = value ?? new Font("Segoe UI", 10F, FontStyle.Bold); Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Color RowHoverColor
        {
            get => _rowHoverColor;
            set { _rowHoverColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Color RowSelectedColor
        {
            get => _rowSelectedColor;
            set { _rowSelectedColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Color RowSelectedTextColor
        {
            get => _rowSelectedTextColor;
            set { _rowSelectedTextColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Font RowFont
        {
            get => _rowFont;
            set { _rowFont = value ?? new Font("Segoe UI", 9.5F); Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Color LineColor
        {
            get => _lineColor;
            set { _lineColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Rows"), DefaultValue(true)]
        public bool ShowRowLines
        {
            get => _showRowLines;
            set { _showRowLines = value; Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Color ExpandIconColor
        {
            get => _expandColor;
            set { _expandColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Rows")]
        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; Invalidate(); }
        }

        [Category("NovaTreeList - Behavior"), DefaultValue(true)]
        public bool ShowTooltips
        {
            get => _showTooltips;
            set => _showTooltips = value;
        }

        [Category("NovaTreeList - Behavior"), DefaultValue(true)]
        public bool SelectableRows
        {
            get => _selectableRows;
            set => _selectableRows = value;
        }

        [Category("NovaTreeList - Behavior"), DefaultValue(true)]
        public bool AllowKeyboardNavigation
        {
            get => _allowKeyboardNavigation;
            set { _allowKeyboardNavigation = value; TabStop = value; }
        }

        #endregion

        #region Public API

        public NovaTreeListNode AddRoot(string text, string icon = "")
        {
            var node = new NovaTreeListNode(text, icon);
            _roots.Add(node);
            UpdateVisibleNodes();
            UpdateScroll();
            Invalidate();
            return node;
        }

        public NovaTreeListNode AddRoot(NovaTreeListNode node)
        {
            if (node == null) return null;
            _roots.Add(node);
            UpdateVisibleNodes();
            UpdateScroll();
            Invalidate();
            return node;
        }

        public void AddColumn(string header, string cellKey, int width = 120,
                              ContentAlignment alignment = ContentAlignment.MiddleLeft)
        {
            _columns.Add(new NovaTreeListColumn
            {
                Header = header,
                CellKey = cellKey,
                Width = width,
                Alignment = alignment
            });
            Invalidate();
        }

        public void ClearNodes()
        {
            _roots.Clear();
            _selectedNode = null;
            UpdateVisibleNodes();
            UpdateScroll();
            Invalidate();
        }

        public void ExpandAll()
        {
            foreach (var r in _roots) ExpandRecursive(r, true);
            UpdateVisibleNodes();
            UpdateScroll();
            Invalidate();
        }

        public void CollapseAll()
        {
            foreach (var r in _roots) ExpandRecursive(r, false);
            UpdateVisibleNodes();
            UpdateScroll();
            Invalidate();
        }

        private void ExpandRecursive(NovaTreeListNode node, bool expand)
        {
            if (node.HasChildren)
            {
                node.Expanded = expand;
                foreach (var c in node.Children) ExpandRecursive(c, expand);
            }
        }

        public void RefreshNodes()
        {
            UpdateVisibleNodes();
            UpdateScroll();
            Invalidate();
        }

        #endregion

        #region Icon Fonts Cache

        private Font GetIconFont(string fontName, float size)
        {
            string key = fontName + "|" + size.ToString("0.##");
            if (_iconFontCache.TryGetValue(key, out var cached)) return cached;

            try
            {
                var font = new Font(fontName, size);
                _iconFontCache[key] = font;
                return font;
            }
            catch
            {
                // fallback
                if (_iconFontCache.TryGetValue("Segoe MDL2 Assets|" + size.ToString("0.##"), out var fallback))
                    return fallback;

                var f = new Font("Segoe MDL2 Assets", size);
                _iconFontCache["Segoe MDL2 Assets|" + size.ToString("0.##")] = f;
                return f;
            }
        }

        #endregion

        #region Hierarchy Helpers

        private void UpdateVisibleNodes()
        {
            _visibleNodes.Clear();
            foreach (var r in _roots) AddVisible(r);
        }

        private void AddVisible(NovaTreeListNode node)
        {
            _visibleNodes.Add(node);
            if (node.Expanded)
                foreach (var c in node.Children) AddVisible(c);
        }

        private int GetNodeLevel(NovaTreeListNode node)
        {
            int level = 0;
            var p = node.Parent;
            while (p != null) { level++; p = p.Parent; }
            return level;
        }

        #endregion

        #region Region

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            if (!_roundedBorder || Width <= 2 || Height <= 2)
            {
                Region = null;
                return;
            }

            using (var path = Round(new Rectangle(0, 0, Width, Height), _borderRadius))
            {
                var old = Region;
                Region = new Region(path);
                old?.Dispose();
            }
        }

        #endregion

        #region Scroll

        private void UpdateScroll()
        {
            if (_vScroll == null) return;

            int viewH = Height - _headerHeight;
            int totalH = _visibleNodes.Count * _rowHeight;

            _vScroll.Visible = totalH > viewH;

            if (_vScroll.Visible)
            {
                _vScroll.Minimum = 0;
                _vScroll.Maximum = Math.Max(0, totalH - viewH + _vScroll.LargeChange - 1);
                _vScroll.LargeChange = viewH;
                _vScroll.SmallChange = _rowHeight;
                if (_scrollOffset > _vScroll.Maximum)
                    _vScroll.Value = _vScroll.Maximum;
            }
            else
            {
                _scrollOffset = 0;
            }
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

            using (var b = new SolidBrush(_backColor))
                g.FillRectangle(b, ClientRectangle);

            DrawHeader(g, rtl);

            DrawRows(g, rtl);

            if (_outerBorderSize > 0)
            {
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = _roundedBorder ? Round(rect, _borderRadius) : Round(rect, 0))
                using (var p = new Pen(_outerBorderColor, _outerBorderSize))
                {
                    p.Alignment = PenAlignment.Inset;
                    g.DrawPath(p, path);
                }
            }
        }

        private void DrawHeader(Graphics g, bool rtl)
        {
            var headerRect = new Rectangle(0, 0, Width, _headerHeight);

            if (_headerGradient)
            {
                using (var brush = new LinearGradientBrush(headerRect,
                    _headerBackColor, _headerBackColor2, LinearGradientMode.Vertical))
                    g.FillRectangle(brush, headerRect);
            }
            else
            {
                using (var b = new SolidBrush(_headerBackColor))
                    g.FillRectangle(b, headerRect);
            }

            using (var p = new Pen(_headerBorderColor, 1))
                g.DrawLine(p, 0, _headerHeight - 1, Width, _headerHeight - 1);

            int x = 0;
            int scrollbarW = _vScroll.Visible ? _vScroll.Width : 0;

            for (int i = 0; i < _columns.Count; i++)
            {
                var col = _columns[i];
                int colW = col.Width;
                if (i == _columns.Count - 1)
                    colW = Width - x - scrollbarW;

                int colX = rtl ? (Width - scrollbarW - x - colW) : x;
                var colRect = new Rectangle(colX, 0, colW, _headerHeight);

                var align = col.Alignment;
                if (rtl)
                {
                    if (align == ContentAlignment.MiddleLeft) align = ContentAlignment.MiddleRight;
                    else if (align == ContentAlignment.MiddleRight) align = ContentAlignment.MiddleLeft;
                }

                TextRenderer.DrawText(g, col.Header, _headerFont, colRect, _headerTextColor,
                    GetTextFlags(align) | TextFormatFlags.EndEllipsis);

                if (i < _columns.Count - 1)
                {
                    int sepX = rtl ? colX : (colX + colW - 1);
                    using (var p = new Pen(_headerBorderColor, 1))
                        g.DrawLine(p, sepX, 8, sepX, _headerHeight - 8);
                }

                x += colW;
            }
        }

        private void DrawRows(Graphics g, bool rtl)
        {
            _expandRects.Clear();

            int firstVisible = Math.Max(0, _scrollOffset / _rowHeight);
            int lastVisible = Math.Min(_visibleNodes.Count - 1,
                firstVisible + (Height - _headerHeight) / _rowHeight + 1);

            int y = _headerHeight - (_scrollOffset % _rowHeight) + (firstVisible * _rowHeight);

            var oldClip = g.Clip;
            g.SetClip(new Rectangle(0, _headerHeight, Width, Height - _headerHeight));

            for (int i = firstVisible; i <= lastVisible && i < _visibleNodes.Count; i++)
            {
                var node = _visibleNodes[i];
                var rowRect = new Rectangle(0, y, Width, _rowHeight);
                DrawRow(g, node, rowRect, i, rtl);
                y += _rowHeight;
            }

            g.Clip = oldClip;
        }

        private void DrawRow(Graphics g, NovaTreeListNode node, Rectangle rowRect, int index, bool rtl)
        {
            bool isSelected = node == _selectedNode;
            bool isHover = index == _hoverRowIndex && !isSelected;

            if (isSelected)
            {
                using (var b = new SolidBrush(_rowSelectedColor))
                    g.FillRectangle(b, rowRect);
            }
            else if (isHover)
            {
                using (var b = new SolidBrush(_rowHoverColor))
                    g.FillRectangle(b, rowRect);
            }

            if (_showRowLines && index < _visibleNodes.Count - 1)
            {
                using (var p = new Pen(_lineColor, 1))
                    g.DrawLine(p, 0, rowRect.Bottom - 1, Width, rowRect.Bottom - 1);
            }

            int level = GetNodeLevel(node);
            int scrollbarW = _vScroll.Visible ? _vScroll.Width : 0;
            int contentW = Width - scrollbarW;

            int startX = rtl
                ? (contentW - _firstColumnPadding - level * _indentWidth)
                : (_firstColumnPadding + level * _indentWidth);

            int expandSize = 18;
            var expandRect = rtl
                ? new Rectangle(startX - expandSize, rowRect.Y + (rowRect.Height - expandSize) / 2, expandSize, expandSize)
                : new Rectangle(startX, rowRect.Y + (rowRect.Height - expandSize) / 2, expandSize, expandSize);

            _expandRects.Add(expandRect);

            if (node.HasChildren)
            {
                string glyph = node.Expanded ? "\uE70D" : "\uE76C";

                TextRenderer.DrawText(g, glyph, _expandFont, expandRect, _expandColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            int iconW = 0;
            if (!string.IsNullOrEmpty(node.Icon)) iconW = _iconSize + 8;

            int iconX = rtl ? (expandRect.Left - iconW) : (expandRect.Right);

            if (!string.IsNullOrEmpty(node.Icon))
            {
                var iconFont = GetIconFont(
                    string.IsNullOrEmpty(node.IconFontName) ? "Segoe MDL2 Assets" : node.IconFontName,
                    11F);

                var iconRect = new Rectangle(iconX, rowRect.Y + (rowRect.Height - _iconSize) / 2,
                    _iconSize, _iconSize);

                Color iconClr = node.NodeIconColor != Color.Empty ? node.NodeIconColor : _iconColor;

                var measured = TextRenderer.MeasureText(node.Icon, iconFont,
                    new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);

                int drawX = iconRect.X + (iconRect.Width - measured.Width) / 2;
                int drawY = iconRect.Y + (iconRect.Height - measured.Height) / 2;

                TextRenderer.DrawText(g, node.Icon, iconFont, new Point(drawX, drawY), iconClr,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }

            int textStartX = rtl
                ? (iconX - (string.IsNullOrEmpty(node.Icon) ? 0 : 0))
                : (iconX + iconW);

            if (rtl)
            {
                textStartX = iconX;
            }

            // ============ Columns ============
            int colX = 0;
            Color textCol = isSelected ? _rowSelectedTextColor : _textColor;

            for (int i = 0; i < _columns.Count; i++)
            {
                var col = _columns[i];
                int colW = col.Width;
                if (i == _columns.Count - 1)
                    colW = contentW - colX;

                int colDrawX = rtl ? (contentW - colX - colW) : colX;
                var cellRect = new Rectangle(colDrawX, rowRect.Y, colW, rowRect.Height);

                string cellText;
                Rectangle cellTextRect;

                if (col.CellKey == "__text__")
                {
                    cellText = node.Text;

                    if (rtl)
                    {
                        int cellLeft = cellRect.X + _cellPadding;
                        int cellRight = textStartX - 4;
                        if (cellRight < cellLeft) cellRight = cellRect.Right - _cellPadding;
                        cellTextRect = new Rectangle(cellLeft, cellRect.Y,
                            Math.Max(20, cellRight - cellLeft), cellRect.Height);
                    }
                    else
                    {
                        int cellRight = cellRect.Right - _cellPadding;
                        cellTextRect = new Rectangle(textStartX, cellRect.Y,
                            Math.Max(20, cellRight - textStartX), cellRect.Height);
                    }
                }
                else
                {
                    cellText = node.Cells.ContainsKey(col.CellKey) ? node.Cells[col.CellKey] : "";
                    var align = col.Alignment;
                    if (rtl)
                    {
                        if (align == ContentAlignment.MiddleLeft) align = ContentAlignment.MiddleRight;
                        else if (align == ContentAlignment.MiddleRight) align = ContentAlignment.MiddleLeft;
                    }
                    cellTextRect = new Rectangle(cellRect.X + _cellPadding, cellRect.Y,
                        colW - _cellPadding * 2, cellRect.Height);
                }

                TextRenderer.DrawText(g, cellText, _rowFont, cellTextRect, textCol,
                    (rtl ? TextFormatFlags.Right : TextFormatFlags.Left) |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding |
                    TextFormatFlags.NoPrefix);

                colX += colW;
            }

            if (index == _hoverRowIndex)
                _hoverExpandRect = expandRect;
        }

        private static TextFormatFlags GetTextFlags(ContentAlignment align)
        {
            switch (align)
            {
                case ContentAlignment.TopLeft: return TextFormatFlags.Left | TextFormatFlags.Top;
                case ContentAlignment.TopCenter: return TextFormatFlags.HorizontalCenter | TextFormatFlags.Top;
                case ContentAlignment.TopRight: return TextFormatFlags.Right | TextFormatFlags.Top;
                case ContentAlignment.MiddleLeft: return TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
                case ContentAlignment.MiddleCenter: return TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                case ContentAlignment.MiddleRight: return TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
                case ContentAlignment.BottomLeft: return TextFormatFlags.Left | TextFormatFlags.Bottom;
                case ContentAlignment.BottomCenter: return TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;
                case ContentAlignment.BottomRight: return TextFormatFlags.Right | TextFormatFlags.Bottom;
                default: return TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            }
        }

        #endregion

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int idx = GetIndexAtPoint(e.Location);
            var node = idx >= 0 && idx < _visibleNodes.Count ? _visibleNodes[idx] : null;

            if (idx != _hoverRowIndex || node != _hoverNode)
            {
                _hoverRowIndex = idx;
                _hoverNode = node;
                Cursor = node != null ? Cursors.Hand : Cursors.Default;

                if (_showTooltips && node != null && !string.IsNullOrEmpty(node.Text))
                    _tooltip.SetToolTip(this, node.Text);
                else
                    _tooltip.SetToolTip(this, "");

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverRowIndex = -1;
            _hoverNode = null;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            Focus();

            int idx = GetIndexAtPoint(e.Location);
            if (idx < 0 || idx >= _visibleNodes.Count) return;

            var node = _visibleNodes[idx];

            if (node.HasChildren && _hoverExpandRect.Contains(e.Location))
            {
                node.Expanded = !node.Expanded;
                UpdateVisibleNodes();
                UpdateScroll();
                NodeExpandedChanged?.Invoke(this, node);
                Invalidate();
                return;
            }

            if (_selectableRows)
            {
                SelectedNode = node;
                Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            int idx = GetIndexAtPoint(e.Location);
            if (idx < 0 || idx >= _visibleNodes.Count) return;

            var node = _visibleNodes[idx];

            if (node.HasChildren)
            {
                node.Expanded = !node.Expanded;
                UpdateVisibleNodes();
                UpdateScroll();
                NodeExpandedChanged?.Invoke(this, node);
            }

            NodeDoubleClicked?.Invoke(this, node);
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!_vScroll.Visible) return;

            int newVal = _vScroll.Value - e.Delta / 2;
            _vScroll.Value = Math.Max(_vScroll.Minimum, Math.Min(_vScroll.Maximum, newVal));
        }

        private int GetIndexAtPoint(Point p)
        {
            if (p.Y < _headerHeight) return -1;
            if (p.X > Width - (_vScroll.Visible ? _vScroll.Width : 0)) return -1;

            int relY = p.Y - _headerHeight + _scrollOffset;
            int idx = relY / _rowHeight;
            if (idx < 0 || idx >= _visibleNodes.Count) return -1;
            return idx;
        }

        #endregion

        #region Keyboard

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Enter:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!_allowKeyboardNavigation) return;

            bool rtl = RightToLeft == RightToLeft.Yes;
            int currentIndex = _selectedNode != null ? _visibleNodes.IndexOf(_selectedNode) : -1;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (currentIndex > 0)
                        SelectedNode = _visibleNodes[currentIndex - 1];
                    else if (currentIndex < 0 && _visibleNodes.Count > 0)
                        SelectedNode = _visibleNodes[0];
                    EnsureVisible(_selectedNode);
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Down:
                    if (currentIndex >= 0 && currentIndex < _visibleNodes.Count - 1)
                        SelectedNode = _visibleNodes[currentIndex + 1];
                    else if (currentIndex < 0 && _visibleNodes.Count > 0)
                        SelectedNode = _visibleNodes[0];
                    EnsureVisible(_selectedNode);
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Right:
                    if (_selectedNode != null)
                    {
                        if (rtl)
                        {
                            if (_selectedNode.Parent != null)
                                SelectedNode = _selectedNode.Parent;
                        }
                        else
                        {
                            if (_selectedNode.HasChildren && !_selectedNode.Expanded)
                            {
                                _selectedNode.Expanded = true;
                                UpdateVisibleNodes();
                                UpdateScroll();
                                NodeExpandedChanged?.Invoke(this, _selectedNode);
                                Invalidate();
                            }
                        }
                    }
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Left:
                    if (_selectedNode != null)
                    {
                        if (rtl)
                        {
                            if (_selectedNode.HasChildren && !_selectedNode.Expanded)
                            {
                                _selectedNode.Expanded = true;
                                UpdateVisibleNodes();
                                UpdateScroll();
                                NodeExpandedChanged?.Invoke(this, _selectedNode);
                                Invalidate();
                            }
                        }
                        else
                        {
                            if (_selectedNode.Parent != null)
                                SelectedNode = _selectedNode.Parent;
                        }
                    }
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Enter:
                    if (_selectedNode != null && _selectedNode.HasChildren)
                    {
                        _selectedNode.Expanded = !_selectedNode.Expanded;
                        UpdateVisibleNodes();
                        UpdateScroll();
                        NodeExpandedChanged?.Invoke(this, _selectedNode);
                        Invalidate();
                    }
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        private void EnsureVisible(NovaTreeListNode node)
        {
            if (node == null) return;

            int idx = _visibleNodes.IndexOf(node);
            if (idx < 0) return;

            int rowTop = idx * _rowHeight;
            int rowBottom = rowTop + _rowHeight;
            int viewTop = _scrollOffset;
            int viewBottom = _scrollOffset + (Height - _headerHeight);

            if (rowTop < viewTop)
                _vScroll.Value = rowTop;
            else if (rowBottom > viewBottom)
                _vScroll.Value = Math.Max(0, rowBottom - (Height - _headerHeight));
        }

        #endregion

        #region RTL

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        #endregion

        #region Helpers

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _headerFont?.Dispose();
                _rowFont?.Dispose();
                _expandFont?.Dispose();
                _vScroll?.Dispose();
                _tooltip?.Dispose();

                foreach (var f in _iconFontCache.Values) f.Dispose();
                _iconFontCache.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}