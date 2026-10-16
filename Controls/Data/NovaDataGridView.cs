using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [Description("جدول بيانات احترافي يدعم RTL/LTR تلقائياً مع أعمدة تنعكس حسب اتجاه الواجهة.")]
    public class NovaDataGridView : DataGridView
    {
        #region Fileds

        // Outer Shape
        private int _borderRadius = 12;
        private bool _roundedBorder = true;
        private Color _outerBorderColor = Color.FromArgb(226, 232, 240);
        private int _outerBorderSize = 1;
        private bool _shadowEnabled = false;
        private int _shadowDepth = 6;
        private int _shadowOpacity = 15;

        // Header
        private Color _headerBackColor = Color.FromArgb(248, 250, 252);
        private Color _headerBackColor2 = Color.FromArgb(241, 245, 249);
        private bool _headerGradient = false;
        private Color _headerTextColor = Color.FromArgb(15, 23, 42);
        private Color _headerBorderColor = Color.FromArgb(226, 232, 240);
        private Font _headerFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private int _headerHeight = 46;
        private DataGridViewContentAlignment _headerAlignment = DataGridViewContentAlignment.MiddleCenter;
        private bool _sortGlyphVisible = true;
        private Color _sortGlyphColor = Color.FromArgb(99, 102, 241);

        // Rows
        private Color _rowBackColor = Color.White;
        private Color _alternatingRowColor = Color.FromArgb(250, 251, 253);
        private Color _rowHoverColor = Color.FromArgb(241, 245, 249);
        private Color _rowTextColor = Color.FromArgb(51, 65, 85);
        private Font _rowFont = new Font("Segoe UI", 10F);

        // Selection
        private Color _selectionBackColor = Color.FromArgb(224, 231, 255);
        private Color _selectionTextColor = Color.FromArgb(67, 56, 202);
        private bool _showSelectionLeftBorder = true;
        private Color _selectionBorderColor = Color.FromArgb(99, 102, 241);
        private int _selectionBorderWidth = 3;

        // Grid
        private Color _gridLineColor = Color.FromArgb(241, 245, 249);
        private bool _showVerticalLines = false;
        private bool _showHorizontalLines = true;

        // Rows
        private int _rowHeightCustom = 42;
        private Padding _cellPaddingCustom = new Padding(12, 0, 12, 0);

        // Hover
        private int _hoverRowIndex = -1;
        private bool _showRowHover = true;

        // Header Redesign
        private bool _headerTopAccentEnabled = false;
        private Color _headerTopAccentColor = Color.FromArgb(99, 102, 241);
        private int _headerTopAccentHeight = 3;

        #endregion

        public NovaDataGridView()
        {
            typeof(DataGridView)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(this, true, null);

            BorderStyle = BorderStyle.None;
            EnableHeadersVisualStyles = false;
            BackgroundColor = Color.White;
            GridColor = _gridLineColor;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = true;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            RowTemplate.Height = _rowHeightCustom;
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            ScrollBars = ScrollBars.Vertical;

            ApplyStyles();

            // Events
            CellMouseEnter += FDataGridView_CellMouseEnter;
            CellMouseLeave += FDataGridView_CellMouseLeave;
            DataBindingComplete += (s, e) => ApplyStyles();
            ColumnAdded += (s, e) => ApplyStyles();
            RightToLeftChanged += (s, e) => { UpdateRegion(); Invalidate(); };
        }

        #region Properties

        // ============ Outer Shape ============

        [Category("FDataGridView - Shape"), DefaultValue(12)]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
        }

        [Category("FDataGridView - Shape"), DefaultValue(true)]
        public bool RoundedBorder
        {
            get => _roundedBorder;
            set { _roundedBorder = value; UpdateRegion(); Invalidate(); }
        }

        [Category("FDataGridView - Shape")]
        public Color OuterBorderColor
        {
            get => _outerBorderColor;
            set { _outerBorderColor = value; Invalidate(); }
        }

        [Category("FDataGridView - Shape"), DefaultValue(1)]
        public int OuterBorderSize
        {
            get => _outerBorderSize;
            set { _outerBorderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("FDataGridView - Shape"), DefaultValue(false)]
        public bool ShadowEnabled
        {
            get => _shadowEnabled;
            set { _shadowEnabled = value; Invalidate(); }
        }

        [Category("FDataGridView - Shape"), DefaultValue(6)]
        public int ShadowDepth
        {
            get => _shadowDepth;
            set { _shadowDepth = Math.Max(0, Math.Min(20, value)); Invalidate(); }
        }

        [Category("FDataGridView - Shape"), DefaultValue(15)]
        public int ShadowOpacity
        {
            get => _shadowOpacity;
            set { _shadowOpacity = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        // ============ Header ============

        [Category("FDataGridView - Header")]
        public Color HeaderBackColor
        {
            get => _headerBackColor;
            set { _headerBackColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Header")]
        public Color HeaderBackColor2
        {
            get => _headerBackColor2;
            set { _headerBackColor2 = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Header"), DefaultValue(false)]
        [Description("تفعيل التدرج اللوني لخلفية الرأس.")]
        public bool HeaderGradient
        {
            get => _headerGradient;
            set { _headerGradient = value; Invalidate(); }
        }

        [Category("FDataGridView - Header")]
        public Color HeaderTextColor
        {
            get => _headerTextColor;
            set { _headerTextColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Header")]
        public Color HeaderBorderColor
        {
            get => _headerBorderColor;
            set { _headerBorderColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Header")]
        public Font HeaderFont
        {
            get => _headerFont;
            set { _headerFont = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Header"), DefaultValue(46)]
        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = Math.Max(28, value); ColumnHeadersHeight = value; Invalidate(); }
        }

        [Category("FDataGridView - Header")]
        public DataGridViewContentAlignment HeaderAlignment
        {
            get => _headerAlignment;
            set { _headerAlignment = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Header"), DefaultValue(true)]
        public bool SortGlyphVisible
        {
            get => _sortGlyphVisible;
            set { _sortGlyphVisible = value; Invalidate(); }
        }

        [Category("FDataGridView - Header")]
        public Color SortGlyphColor
        {
            get => _sortGlyphColor;
            set { _sortGlyphColor = value; Invalidate(); }
        }

        [Category("FDataGridView - Header"), DefaultValue(false)]
        [Description("شريط لوني على الحافة العلوية للرأس.")]
        public bool HeaderTopAccentEnabled
        {
            get => _headerTopAccentEnabled;
            set { _headerTopAccentEnabled = value; Invalidate(); }
        }

        [Category("FDataGridView - Header")]
        public Color HeaderTopAccentColor
        {
            get => _headerTopAccentColor;
            set { _headerTopAccentColor = value; Invalidate(); }
        }

        [Category("FDataGridView - Header"), DefaultValue(3)]
        public int HeaderTopAccentHeight
        {
            get => _headerTopAccentHeight;
            set { _headerTopAccentHeight = Math.Max(1, Math.Min(10, value)); Invalidate(); }
        }

        // ============ ROWS ============

        [Category("FDataGridView - ROWS")]
        public Color RowBackColor
        {
            get => _rowBackColor;
            set { _rowBackColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - ROWS")]
        public Color AlternatingRowColor
        {
            get => _alternatingRowColor;
            set { _alternatingRowColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - ROWS")]
        public Color RowHoverColor
        {
            get => _rowHoverColor;
            set { _rowHoverColor = value; Invalidate(); }
        }

        [Category("FDataGridView - ROWS"), DefaultValue(true)]
        public bool ShowRowHover
        {
            get => _showRowHover;
            set { _showRowHover = value; Invalidate(); }
        }

        [Category("FDataGridView - ROWS")]
        public Color RowTextColor
        {
            get => _rowTextColor;
            set { _rowTextColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - ROWS")]
        public Font RowFont
        {
            get => _rowFont;
            set { _rowFont = value ?? new Font("Segoe UI", 10F); ApplyStyles(); Invalidate(); }
        }

        // ============ Selection ============

        [Category("FDataGridView - Selection")]
        public Color SelectionBackColor
        {
            get => _selectionBackColor;
            set { _selectionBackColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Selection")]
        public Color SelectionTextColor
        {
            get => _selectionTextColor;
            set { _selectionTextColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - Selection"), DefaultValue(true)]
        [Description("عرض شريط لوني على حافة الصف المُحدَّد.")]
        public bool ShowSelectionLeftBorder
        {
            get => _showSelectionLeftBorder;
            set { _showSelectionLeftBorder = value; Invalidate(); }
        }

        [Category("FDataGridView - Selection")]
        public Color SelectionBorderColor
        {
            get => _selectionBorderColor;
            set { _selectionBorderColor = value; Invalidate(); }
        }

        [Category("FDataGridView - الSelectionتحديد"), DefaultValue(3)]
        public int SelectionBorderWidth
        {
            get => _selectionBorderWidth;
            set { _selectionBorderWidth = Math.Max(1, Math.Min(8, value)); Invalidate(); }
        }

        // ============ GRID ============

        [Category("FDataGridView - GRID")]
        public Color GridLineColor
        {
            get => _gridLineColor;
            set { _gridLineColor = value; GridColor = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - GRID"), DefaultValue(false)]
        public bool ShowVerticalLinesCustom
        {
            get => _showVerticalLines;
            set { _showVerticalLines = value; ApplyStyles(); Invalidate(); }
        }

        [Category("FDataGridView - GRID"), DefaultValue(true)]
        public bool ShowHorizontalLinesCustom
        {
            get => _showHorizontalLines;
            set { _showHorizontalLines = value; ApplyStyles(); Invalidate(); }
        }

        // ============ CELLS ============

        [Category("FDataGridView - CELLS"), DefaultValue(42)]
        public int RowHeightCustom
        {
            get => _rowHeightCustom;
            set { _rowHeightCustom = Math.Max(24, value); RowTemplate.Height = _rowHeightCustom; Invalidate(); }
        }

        [Category("FDataGridView - CELLS")]
        public Padding CellPaddingCustom
        {
            get => _cellPaddingCustom;
            set { _cellPaddingCustom = value; ApplyStyles(); Invalidate(); }
        }

        #endregion

        #region RTL/LTR

        public void ApplyDirectionToColumns()
        {
            if (Columns.Count == 0) return;

            var columns = new DataGridViewColumn[Columns.Count];
            for (int i = 0; i < Columns.Count; i++)
                columns[i] = Columns[i];

            Columns.Clear();

            if (RightToLeft == RightToLeft.Yes)
            {
                for (int i = 0; i < columns.Length; i++)
                    Columns.Add(columns[i]);
            }
            else
            {
                for (int i = columns.Length - 1; i >= 0; i--)
                    Columns.Add(columns[i]);
            }

            var align = RightToLeft == RightToLeft.Yes
                ? DataGridViewContentAlignment.MiddleRight
                : DataGridViewContentAlignment.MiddleLeft;

            foreach (DataGridViewColumn col in Columns)
            {
                if (col.DefaultCellStyle.Alignment == DataGridViewContentAlignment.NotSet)
                    col.DefaultCellStyle.Alignment = align;
            }
        }

        #endregion

        #region ApplyStyles

        private void ApplyStyles()
        {
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _headerBackColor,
                ForeColor = _headerTextColor,
                SelectionBackColor = _headerBackColor,
                SelectionForeColor = _headerTextColor,
                Font = _headerFont,
                Alignment = _headerAlignment,
                Padding = new Padding(8, 0, 8, 0),
                WrapMode = DataGridViewTriState.False
            };

            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _rowBackColor,
                ForeColor = _rowTextColor,
                SelectionBackColor = _selectionBackColor,
                SelectionForeColor = _selectionTextColor,
                Font = _rowFont,
                Padding = _cellPaddingCustom,
                Alignment = RightToLeft == RightToLeft.Yes
                    ? DataGridViewContentAlignment.MiddleRight
                    : DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False
            };

            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = _alternatingRowColor,
                ForeColor = _rowTextColor,
                SelectionBackColor = _selectionBackColor,
                SelectionForeColor = _selectionTextColor,
                Padding = _cellPaddingCustom
            };

            ColumnHeadersHeight = _headerHeight;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            if (_showVerticalLines && _showHorizontalLines)
                CellBorderStyle = DataGridViewCellBorderStyle.Single;
            else if (!_showVerticalLines && _showHorizontalLines)
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            else if (_showVerticalLines && !_showHorizontalLines)
                CellBorderStyle = DataGridViewCellBorderStyle.SingleVertical;
            else
                CellBorderStyle = DataGridViewCellBorderStyle.None;

            GridColor = _gridLineColor;
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

        #region Paint

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                e.PaintBackground(e.ClipBounds, false);

                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (_headerGradient)
                {
                    using (var brush = new LinearGradientBrush(
                        e.CellBounds,
                        _headerBackColor,
                        _headerBackColor2,
                        LinearGradientMode.Vertical))
                        g.FillRectangle(brush, e.CellBounds);
                }
                else
                {
                    using (var b = new SolidBrush(_headerBackColor))
                        g.FillRectangle(b, e.CellBounds);
                }

                if (_headerTopAccentEnabled)
                {
                    var accentRect = new Rectangle(
                        e.CellBounds.X,
                        e.CellBounds.Y,
                        e.CellBounds.Width,
                        _headerTopAccentHeight);

                    using (var b = new SolidBrush(_headerTopAccentColor))
                        g.FillRectangle(b, accentRect);
                }

                using (var p = new Pen(_headerBorderColor, 1))
                    g.DrawLine(p,
                        e.CellBounds.Left, e.CellBounds.Bottom - 1,
                        e.CellBounds.Right, e.CellBounds.Bottom - 1);

                if (_showVerticalLines && e.ColumnIndex < Columns.Count - 1)
                {
                    using (var p = new Pen(_headerBorderColor, 1))
                        g.DrawLine(p,
                            e.CellBounds.Right - 1, e.CellBounds.Y + 8,
                            e.CellBounds.Right - 1, e.CellBounds.Bottom - 8);
                }

                if (e.Value != null)
                {
                    var headerCell = Columns[e.ColumnIndex].HeaderCell;
                    var align = headerCell.Style.Alignment != DataGridViewContentAlignment.NotSet
                        ? headerCell.Style.Alignment
                        : _headerAlignment;

                    var textRect = new Rectangle(
                        e.CellBounds.X + 10,
                        e.CellBounds.Y + (_headerTopAccentEnabled ? _headerTopAccentHeight : 0),
                        e.CellBounds.Width - 20,
                        e.CellBounds.Height - (_headerTopAccentEnabled ? _headerTopAccentHeight : 0));

                    bool hasSort = _sortGlyphVisible && SortOrder != SortOrder.None && SortedColumn == Columns[e.ColumnIndex];
                    if (hasSort) textRect.Width -= 18;

                    TextRenderer.DrawText(
                        g,
                        e.Value.ToString(),
                        _headerFont,
                        textRect,
                        _headerTextColor,
                        GetTextFlags(align) | TextFormatFlags.EndEllipsis);
                }

                if (_sortGlyphVisible &&
                    Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection != SortOrder.None)
                {
                    string glyph = Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Ascending
                        ? "\uE70E"  
                        : "\uE70D"; 

                    var glyphRect = new Rectangle(
                        RightToLeft == RightToLeft.Yes
                            ? e.CellBounds.Left + 4
                            : e.CellBounds.Right - 22,
                        e.CellBounds.Y,
                        18,
                        e.CellBounds.Height);

                    using (var f = new Font("Segoe MDL2 Assets", 9F))
                    using (var b = new SolidBrush(_sortGlyphColor))
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    })
                        g.DrawString(glyph, f, b, glyphRect, sf);
                }

                e.Handled = true;
                return;
            }

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
                bool isHover = _showRowHover && e.RowIndex == _hoverRowIndex && !isSelected;

                if (isSelected)
                {
                    base.OnCellPainting(e);
                    return;
                }

                e.PaintBackground(e.CellBounds, false);

                // Hover
                if (isHover)
                {
                    using (var b = new SolidBrush(_rowHoverColor))
                        e.Graphics.FillRectangle(b, e.CellBounds);
                }

                e.PaintContent(e.CellBounds);
                e.Handled = true;
                return;
            }

            base.OnCellPainting(e);
        }

        protected override void OnRowPostPaint(DataGridViewRowPostPaintEventArgs e)
        {
            base.OnRowPostPaint(e);

            if (e.RowIndex < 0) return;

            bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
            if (!isSelected) return;
            if (!_showSelectionLeftBorder) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int x = RightToLeft == RightToLeft.Yes
                ? ClientRectangle.Width - _selectionBorderWidth
                : 0;

            var barRect = new Rectangle(
                x,
                e.RowBounds.Y,
                _selectionBorderWidth,
                e.RowBounds.Height - 1);

            using (var path = Round(barRect, _selectionBorderWidth / 2))
            using (var b = new SolidBrush(_selectionBorderColor))
                g.FillPath(b, path);
        }

        protected override void OnRowPrePaint(DataGridViewRowPrePaintEventArgs e)
        {
            e.PaintParts &= ~DataGridViewPaintParts.Focus;
            base.OnRowPrePaint(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (_shadowEnabled && _shadowDepth > 0)
            {
                for (int i = _shadowDepth; i >= 1; i--)
                {
                    int alpha = (_shadowOpacity * 255 / 100) * (_shadowDepth - i + 1) / (_shadowDepth + 2);
                    if (alpha < 1) continue;

                    var sr = new Rectangle(
                        i / 3,
                        i / 2,
                        Width - 1,
                        Height - 1);

                }
            }

            if (_outerBorderSize > 0)
            {
                var rect = new Rectangle(0, 0, Width - 1, Height - 1);

                using (var path = _roundedBorder ? Round(rect, _borderRadius) : RoundedRect(rect, 0))
                using (var p = new Pen(_outerBorderColor, _outerBorderSize))
                {
                    p.Alignment = PenAlignment.Inset;
                    g.DrawPath(p, path);
                }
            }
        }

        #endregion

        #region Mouse Hover

        private void FDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.RowIndex != _hoverRowIndex)
            {
                int oldRow = _hoverRowIndex;
                _hoverRowIndex = e.RowIndex;

                if (oldRow >= 0 && oldRow < Rows.Count) InvalidateRow(oldRow);
                if (_hoverRowIndex >= 0 && _hoverRowIndex < Rows.Count) InvalidateRow(_hoverRowIndex);
            }
        }

        private void FDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoverRowIndex >= 0 && _hoverRowIndex < Rows.Count)
            {
                int old = _hoverRowIndex;
                _hoverRowIndex = -1;
                InvalidateRow(old);
            }
        }

        #endregion

        #region Helpers

        private static TextFormatFlags GetTextFlags(DataGridViewContentAlignment align)
        {
            switch (align)
            {
                case DataGridViewContentAlignment.TopLeft: return TextFormatFlags.Left | TextFormatFlags.Top;
                case DataGridViewContentAlignment.TopCenter: return TextFormatFlags.HorizontalCenter | TextFormatFlags.Top;
                case DataGridViewContentAlignment.TopRight: return TextFormatFlags.Right | TextFormatFlags.Top;
                case DataGridViewContentAlignment.MiddleLeft: return TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
                case DataGridViewContentAlignment.MiddleCenter: return TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                case DataGridViewContentAlignment.MiddleRight: return TextFormatFlags.Right | TextFormatFlags.VerticalCenter;
                case DataGridViewContentAlignment.BottomLeft: return TextFormatFlags.Left | TextFormatFlags.Bottom;
                case DataGridViewContentAlignment.BottomCenter: return TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom;
                case DataGridViewContentAlignment.BottomRight: return TextFormatFlags.Right | TextFormatFlags.Bottom;
                default: return TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            }
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

        private static GraphicsPath RoundedRect(Rectangle r, int radius) => Round(r, radius);

        #endregion
    }
}