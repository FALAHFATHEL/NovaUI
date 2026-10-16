using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    /// <summary>
    /// شبكة أيقونات مُحاكاة (Virtualized) — ترسم فقط العناصر المرئية.
    /// </summary>
    internal sealed class VirtualIconGrid : Panel
    {
        #region Colors

        private static readonly Color PrimaryColor = Color.FromArgb(99, 102, 241);
        private static readonly Color PrimaryDark = Color.FromArgb(79, 70, 229);
        private static readonly Color CardNormalBg = Color.FromArgb(248, 250, 252);
        private static readonly Color CardHoverBg = Color.FromArgb(238, 242, 255);
        private static readonly Color CardSelectedBg = Color.FromArgb(224, 231, 255);
        private static readonly Color CardBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color CardSelectedBord = Color.FromArgb(99, 102, 241);
        private static readonly Color NameTextColor = Color.FromArgb(100, 116, 139);

        #endregion

        #region Fields

        private readonly List<IconEntry> _items = new List<IconEntry>();
        private readonly VScrollBar _scroll;

        private int _cardWidth = 104;
        private int _cardHeight = 104;
        private int _margin = 6;
        private int _iconSize = 22;
        private string _packName = "";

        private int _hoverIndex = -1;
        private int _selectedIndex = -1;

        public event EventHandler<IconEntry> IconSelected;
        public event EventHandler<IconEntry> IconConfirmed;

        #endregion

        #region Nested Types

        internal sealed class IconEntry
        {
            public string Name;
            public string Glyph;
            public bool Visible = true;
        }

        #endregion

        #region Constructor

        public VirtualIconGrid()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw, true);

            BackColor = Color.White;
            DoubleBuffered = true;

            _scroll = new VScrollBar
            {
                Dock = DockStyle.Right,
                Visible = false,
                Width = 12
            };
            _scroll.ValueChanged += (s, e) => Invalidate();
            Controls.Add(_scroll);
        }

        #endregion

        #region Properties

        public string PackName
        {
            get { return _packName; }
            set { _packName = value ?? ""; Invalidate(); }
        }

        public int IconSize
        {
            get { return _iconSize; }
            set { _iconSize = Math.Max(8, value); Invalidate(); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; EnsureVisible(value); Invalidate(); }
        }

        public IconEntry SelectedEntry
        {
            get { return (_selectedIndex >= 0 && _selectedIndex < _items.Count) ? _items[_selectedIndex] : null; }
        }

        #endregion

        #region Public API

        public void SetItems(List<IconEntry> items)
        {
            _items.Clear();
            if (items != null) _items.AddRange(items);
            _hoverIndex = -1;
            _selectedIndex = -1;
            UpdateScroll();
            Invalidate();
        }

        public void SelectEntry(IconEntry entry)
        {
            int idx = _items.IndexOf(entry);
            if (idx >= 0)
            {
                _selectedIndex = idx;
                EnsureVisible(idx);
                Invalidate();
            }
        }

        public void ClearSelection()
        {
            _selectedIndex = -1;
            Invalidate();
        }

        #endregion

        #region Layout

        private int GetColumnsCount()
        {
            int avail = Width - (_scroll.Visible ? _scroll.Width : 0) - 8;
            int totalCardWidth = _cardWidth + _margin * 2;
            return Math.Max(1, avail / totalCardWidth);
        }

        private int GetVisibleCount()
        {
            int count = 0;
            for (int i = 0; i < _items.Count; i++)
                if (_items[i].Visible) count++;
            return count;
        }

        private int GetRowHeight()
        {
            return _cardHeight + _margin * 2;
        }

        private int GetTotalRows()
        {
            int cols = GetColumnsCount();
            int vis = GetVisibleCount();
            return (vis + cols - 1) / cols;
        }

        private void UpdateScroll()
        {
            int totalH = GetTotalRows() * GetRowHeight() + 8;

            if (totalH > ClientSize.Height)
            {
                _scroll.Visible = true;
                _scroll.Minimum = 0;
                _scroll.LargeChange = Math.Max(1, ClientSize.Height);
                _scroll.Maximum = Math.Max(0, totalH - ClientSize.Height + _scroll.LargeChange - 1);
                _scroll.SmallChange = GetRowHeight();
            }
            else
            {
                _scroll.Visible = false;
                _scroll.Value = 0;
            }
        }

        private void EnsureVisible(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _items.Count) return;

            int visibleIdx = 0;
            for (int i = 0; i < itemIndex; i++)
                if (_items[i].Visible) visibleIdx++;

            int cols = GetColumnsCount();
            int row = visibleIdx / cols;
            int top = row * GetRowHeight() + 4;
            int bottom = top + GetRowHeight();

            if (top < _scroll.Value)
                _scroll.Value = top;
            else if (bottom > _scroll.Value + ClientSize.Height)
                _scroll.Value = Math.Max(0, bottom - ClientSize.Height);
        }

        private int GetRealIndexAtPoint(Point p)
        {
            if (_items.Count == 0) return -1;

            int scrollW = _scroll.Visible ? _scroll.Width : 0;
            if (p.X > Width - scrollW) return -1;

            int cols = GetColumnsCount();
            int rowH = GetRowHeight();
            int cardTotalW = _cardWidth + _margin * 2;

            int offsetX = ((Width - scrollW) - cols * cardTotalW) / 2;
            if (offsetX < 4) offsetX = 4;

            int x = p.X - offsetX;
            int y = p.Y - 4 + _scroll.Value;

            if (x < 0 || y < 0) return -1;

            int col = x / cardTotalW;
            int row = y / rowH;
            if (col < 0 || col >= cols) return -1;

            int visibleIdx = row * cols + col;
            if (visibleIdx < 0) return -1;

            int seen = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                if (!_items[i].Visible) continue;
                if (seen == visibleIdx) return i;
                seen++;
            }
            return -1;
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var b = new SolidBrush(BackColor))
                g.FillRectangle(b, ClientRectangle);

            if (_items.Count == 0) return;

            int cols = GetColumnsCount();
            int rowH = GetRowHeight();
            int cardTotalW = _cardWidth + _margin * 2;
            int scrollW = _scroll.Visible ? _scroll.Width : 0;

            int offsetX = ((Width - scrollW) - cols * cardTotalW) / 2;
            if (offsetX < 4) offsetX = 4;

            int startRow = Math.Max(0, (_scroll.Value - 4) / rowH);
            int endRow = Math.Min(GetTotalRows(), startRow + (ClientSize.Height / rowH) + 2);

            int visibleIdx = 0;
            int totalVisible = GetVisibleCount();
            int itemIdx = 0;

            int targetVisibleIdx = startRow * cols;

            for (int i = 0; i < _items.Count; i++)
            {
                if (!_items[i].Visible) continue;
                if (visibleIdx == targetVisibleIdx)
                {
                    itemIdx = i;
                    break;
                }
                visibleIdx++;
            }

            visibleIdx = targetVisibleIdx;
            int currentRow = startRow;
            int currentCol = 0;

            for (int i = itemIdx; i < _items.Count; i++)
            {
                if (!_items[i].Visible) continue;
                if (currentRow >= endRow) break;

                int x = offsetX + currentCol * cardTotalW + _margin;
                int y = 4 + currentRow * rowH - _scroll.Value + _margin;

                var cardRect = new Rectangle(x, y, _cardWidth, _cardHeight);
                DrawCard(g, cardRect, i);

                currentCol++;
                if (currentCol >= cols)
                {
                    currentCol = 0;
                    currentRow++;
                }

                visibleIdx++;
                if (visibleIdx >= totalVisible) break;
            }
        }

        private void DrawCard(Graphics g, Rectangle rect, int index)
        {
            var item = _items[index];
            if (item == null) return;

            bool isSelected = index == _selectedIndex;
            bool isHover = index == _hoverIndex;

            Color bg = isSelected ? CardSelectedBg
                     : isHover ? CardHoverBg
                     : CardNormalBg;

            Color border = isSelected ? CardSelectedBord : CardBorder;
            float thickness = isSelected ? 1.6f : 1f;

            // 1) خلفية + إطار البطاقة
            using (var path = Rounded(rect, 12))
            {
                using (var b = new SolidBrush(bg))
                    g.FillPath(b, path);
                using (var p = new Pen(border, thickness))
                    g.DrawPath(p, path);
            }

            // 2) الأيقونة — ★ استخدام AddString لفرض الخط الخاص
            if (!string.IsNullOrEmpty(item.Glyph))
            {
                DrawGlyphPrecise(g, rect, item.Glyph, isSelected);
            }

            // 3) اسم الأيقونة
            using (var f = new Font("Segoe UI", 7.5F))
            using (var b = new SolidBrush(NameTextColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                g.DrawString(item.Name, f, b,
                    new RectangleF(rect.X + 4, rect.Bottom - 26, rect.Width - 8, 22), sf);
            }
        }

        /// <summary>
        /// ★ يرسم الرمز بدقة عبر GraphicsPath.AddString مع Scale-to-fit.
        /// </summary>
        private void DrawGlyphPrecise(Graphics g, Rectangle rect, string glyph, bool isSelected)
        {
            if (string.IsNullOrEmpty(glyph)) return;

            string key = string.IsNullOrEmpty(_packName) ? "Segoe MDL2 Assets" : _packName;

            Font font = null;
            try
            {
                font = FontManager.GetIconFont(key, _iconSize);
                if (font == null || font.FontFamily == null) return;

                using (var path = new GraphicsPath())
                {
                    using (var sf = StringFormat.GenericTypographic)
                    {
                        sf.FormatFlags |= StringFormatFlags.NoWrap |
                                          StringFormatFlags.NoClip |
                                          StringFormatFlags.MeasureTrailingSpaces;

                        path.AddString(
                            glyph,
                            font.FontFamily,
                            (int)font.Style,
                            font.Size,
                            PointF.Empty,
                            sf);
                    }

                    var bounds = path.GetBounds();
                    if (bounds.Width < 0.1f || bounds.Height < 0.1f) return;

                    float areaX = rect.X + 4;
                    float areaY = rect.Y + 6;
                    float areaW = rect.Width - 8;
                    float areaH = rect.Height - 34;

                    // ★ Scale-to-fit
                    float scaleX = bounds.Width > areaW ? areaW / bounds.Width : 1f;
                    float scaleY = bounds.Height > areaH ? areaH / bounds.Height : 1f;
                    float scale = Math.Min(scaleX, scaleY);

                    if (scale < 1f)
                    {
                        using (var m = new Matrix())
                        {
                            m.Scale(scale, scale);
                            path.Transform(m);
                        }
                        bounds = path.GetBounds();
                    }

                    float dx = areaX + (areaW - bounds.Width) / 2f - bounds.X;
                    float dy = areaY + (areaH - bounds.Height) / 2f - bounds.Y;

                    using (var m = new Matrix())
                    {
                        m.Translate(dx, dy);
                        path.Transform(m);
                    }

                    using (var b = new SolidBrush(isSelected ? PrimaryDark : PrimaryColor))
                        g.FillPath(b, path);
                }
            }
            catch { }
            finally
            {
                if (font != null) font.Dispose();
            }
        }

        private static GraphicsPath Rounded(Rectangle r, int radius)
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

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = GetRealIndexAtPoint(e.Location);

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
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            int idx = GetRealIndexAtPoint(e.Location);
            if (idx >= 0)
            {
                _selectedIndex = idx;
                Invalidate();

                var h = IconSelected;
                if (h != null) h(this, _items[idx]);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int idx = GetRealIndexAtPoint(e.Location);
            if (idx >= 0)
            {
                var h = IconConfirmed;
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
            Invalidate();
        }

        #endregion

        #region Resize

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateScroll();
            Invalidate();
        }

        #endregion
    }
}