using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("ItemSelected")]
    [Description("شريط جانبي احترافي يدعم RTL/LTR تلقائياً مع عنوان فرعي للأقسام.")]
    public class NovaSidebar : Control
    {
        private readonly List<SidebarItem> _items = new List<SidebarItem>();
        private int _selectedIndex = 0;
        private int _hoverIndex = -1;
        private int _itemHeight = 48;
        private int _padding = 12;
        private int _iconSize = 15;
        private int _itemRadius = 10;

        private string _defaultIconFontName = "Segoe MDL2 Assets";
        private string _defaultIconPackName = "";

        private Color _fillColor = Color.White;
        private Color _itemHoverBack = Color.FromArgb(241, 245, 249);
        private Color _itemSelectedBack = Color.FromArgb(238, 242, 255);
        private Color _itemSelectedTextColor = Color.FromArgb(99, 102, 241);
        private Color _itemTextColor = Color.FromArgb(71, 85, 105);
        private Color _iconColor = Color.FromArgb(100, 116, 139);
        private Color _badgeColor = Color.FromArgb(239, 68, 68);
        private Color _badgeTextColor = Color.White;
        private Color _indicatorColor = Color.FromArgb(99, 102, 241);

        // ★ مظهر العنوان الفرعي
        private bool _showSectionHeaders = true;
        private Color _sectionHeaderColor = Color.FromArgb(148, 163, 184);
        private Font _sectionHeaderFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        private int _sectionHeaderHeight = 34;

        private Font _itemFont = new Font("Segoe UI", 10F);
        private bool _showActiveIndicator = true;

        private string _headerText = "";
        private string _headerIcon = "";
        private string _headerIconFontName = "Segoe MDL2 Assets";
        private string _headerIconPackName = "";
        private int _headerHeight = 70;

        // ★ Layout cache
        private readonly List<LayoutEntry> _layout = new List<LayoutEntry>();
        private struct LayoutEntry
        {
            public Rectangle Rect;
            public int ItemIndex;
            public bool IsHeader;
        }

        public event EventHandler ItemSelected;

        public NovaSidebar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(240, 560);
            Cursor = Cursors.Hand;
        }

        #region Properties

        [Category("NovaSidebar")]
        [Description("عناصر الشريط الجانبي.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public List<SidebarItem> Items
        {
            get => _items;
            set { _items.Clear(); if (value != null) _items.AddRange(value); Invalidate(); }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                int v = Math.Max(-1, Math.Min(_items.Count - 1, value));
                if (_selectedIndex == v) return;
                _selectedIndex = v;
                Invalidate();
                ItemSelected?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SidebarItem SelectedItem
            => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

        [Category("NovaSidebar - Icon")]
        [Description("اسم عائلة الخط الافتراضي لعناصر الشريط.")]
        [DefaultValue("Segoe MDL2 Assets")]
        public string IconFontName
        {
            get => _defaultIconFontName;
            set { _defaultIconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); }
        }

        [Category("NovaSidebar - Icon")]
        [Description("اسم حزمة الأيقونات الافتراضية.")]
        [DefaultValue("")]
        public string IconPackName
        {
            get => _defaultIconPackName;
            set { _defaultIconPackName = value ?? ""; Invalidate(); }
        }

        [Category("NovaSidebar"), DefaultValue(48)]
        public int ItemHeight { get => _itemHeight; set { _itemHeight = Math.Max(30, value); Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue(12)]
        public int Padding_Items { get => _padding; set { _padding = Math.Max(0, value); Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue(10)]
        public int ItemRadius { get => _itemRadius; set { _itemRadius = value; Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue(15)]
        public int IconSize { get => _iconSize; set { _iconSize = Math.Max(8, value); Invalidate(); } }

        // ★★★ خصائص العنوان الفرعي ★★★
        [Category("NovaSidebar - Section Headers")]
        [Description("عند الإيقاف يتم إخفاء كل العناوين الفرعية وتُعرض العناصر فقط.")]
        [DefaultValue(true)]
        public bool ShowSectionHeaders
        {
            get => _showSectionHeaders;
            set { _showSectionHeaders = value; Invalidate(); }
        }

        [Category("NovaSidebar - Section Headers")]
        [Description("لون نص العنوان الفرعي.")]
        public Color SectionHeaderColor
        {
            get => _sectionHeaderColor;
            set { _sectionHeaderColor = value; Invalidate(); }
        }

        [Category("NovaSidebar - Section Headers")]
        [Description("خط العنوان الفرعي.")]
        public Font SectionHeaderFont
        {
            get => _sectionHeaderFont;
            set { _sectionHeaderFont = value ?? new Font("Segoe UI", 8.5F, FontStyle.Bold); Invalidate(); }
        }

        [Category("NovaSidebar - Section Headers")]
        [Description("ارتفاع منطقة العنوان الفرعي بالبكسل.")]
        [DefaultValue(34)]
        public int SectionHeaderHeight
        {
            get => _sectionHeaderHeight;
            set { _sectionHeaderHeight = Math.Max(20, value); Invalidate(); }
        }

        [Category("NovaSidebar")] public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color ItemHoverBackColor { get => _itemHoverBack; set { _itemHoverBack = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color ItemSelectedBackColor { get => _itemSelectedBack; set { _itemSelectedBack = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color ItemSelectedTextColor { get => _itemSelectedTextColor; set { _itemSelectedTextColor = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color ItemTextColor { get => _itemTextColor; set { _itemTextColor = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color IconColor { get => _iconColor; set { _iconColor = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color BadgeColor { get => _badgeColor; set { _badgeColor = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color BadgeTextColor { get => _badgeTextColor; set { _badgeTextColor = value; Invalidate(); } }
        [Category("NovaSidebar")] public Color IndicatorColor { get => _indicatorColor; set { _indicatorColor = value; Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue(true)]
        public bool ShowActiveIndicator { get => _showActiveIndicator; set { _showActiveIndicator = value; Invalidate(); } }

        [Category("NovaSidebar")]
        public Font ItemFont { get => _itemFont; set { _itemFont = value ?? new Font("Segoe UI", 10F); Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue("")]
        public string HeaderText { get => _headerText; set { _headerText = value ?? ""; Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue("")]
        [Description("أيقونة رأس الشريط (يُعبَّأ تلقائياً من المنتقي).")]
        [Editor(typeof(IconGlyphEditor), typeof(UITypeEditor))]
        public string HeaderIcon { get => _headerIcon; set { _headerIcon = value ?? ""; Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue("Segoe MDL2 Assets")]
        public string HeaderIconFontName
        {
            get => _headerIconFontName;
            set { _headerIconFontName = string.IsNullOrEmpty(value) ? "Segoe MDL2 Assets" : value; Invalidate(); }
        }

        [Category("NovaSidebar"), DefaultValue("")]
        public string HeaderIconPackName
        {
            get => _headerIconPackName;
            set { _headerIconPackName = value ?? ""; Invalidate(); }
        }

        [Category("NovaSidebar"), DefaultValue(70)]
        public int HeaderHeight { get => _headerHeight; set { _headerHeight = Math.Max(0, value); Invalidate(); } }

        [Category("NovaSidebar"), DefaultValue(RightToLeft.No)]
        [Description("عربي = Yes (أيقونة يمين، نص يمين) — إنجليزي = No (أيقونة يسار، نص يسار).")]
        public override RightToLeft RightToLeft
        {
            get => base.RightToLeft;
            set { base.RightToLeft = value; Invalidate(); }
        }

        #endregion

        #region Public API

        public void AddItem(string text, string icon, string tag = null, string badge = null)
            => _items.Add(new SidebarItem(text, icon, tag, badge));

        public void AddItem(SidebarItem item)
        {
            if (item != null) _items.Add(item);
            Invalidate();
        }

        /// <summary>إضافة عنوان قسم فرعي جديد.</summary>
        public void AddSectionHeader(string title)
            => _items.Add(new SidebarItem { Text = title, IsSectionHeader = true });

        public void RemoveItem(SidebarItem item)
        {
            if (_items.Remove(item)) Invalidate();
        }

        public void ClearItems()
        {
            _items.Clear();
            _selectedIndex = -1;
            Invalidate();
        }

        #endregion

        #region Layout

        private void BuildLayout()
        {
            _layout.Clear();
            int y = _headerHeight + _padding;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                if (item.IsSectionHeader)
                {
                    if (!_showSectionHeaders) continue;
                    _layout.Add(new LayoutEntry
                    {
                        Rect = new Rectangle(_padding, y, Math.Max(0, Width - _padding * 2), _sectionHeaderHeight),
                        ItemIndex = i,
                        IsHeader = true
                    });
                    y += _sectionHeaderHeight;
                }
                else
                {
                    _layout.Add(new LayoutEntry
                    {
                        Rect = new Rectangle(_padding, y, Math.Max(0, Width - _padding * 2), _itemHeight - 4),
                        ItemIndex = i,
                        IsHeader = false
                    });
                    y += _itemHeight;
                }
            }
        }

        private int GetItemAtY(int y)
        {
            BuildLayout();
            foreach (var e in _layout)
            {
                if (e.IsHeader) continue;
                if (y >= e.Rect.Top && y < e.Rect.Bottom) return e.ItemIndex;
            }
            return -1;
        }

        #endregion

        #region Paint

        private string GetItemFontKey(SidebarItem item)
        {
            if (item == null) return _defaultIconFontName;
            if (!string.IsNullOrEmpty(item.IconPackName)) return item.IconPackName;
            if (!string.IsNullOrEmpty(item.IconFontName)) return item.IconFontName;
            if (!string.IsNullOrEmpty(_defaultIconPackName)) return _defaultIconPackName;
            return _defaultIconFontName;
        }

        private string GetHeaderFontKey()
        {
            if (!string.IsNullOrEmpty(_headerIconPackName)) return _headerIconPackName;
            if (!string.IsNullOrEmpty(_headerIconFontName)) return _headerIconFontName;
            return "Segoe MDL2 Assets";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            bool rtl = RightToLeft == RightToLeft.Yes;

            using (var b = new SolidBrush(_fillColor))
                g.FillRectangle(b, ClientRectangle);

            using (var p = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
            {
                if (rtl) g.DrawLine(p, 0, 0, 0, Height);        // RTL: الفاصل على اليسار
                else g.DrawLine(p, Width - 1, 0, Width - 1, Height); // LTR: الفاصل على اليمين
            }

            if (_headerHeight > 0 && (!string.IsNullOrEmpty(_headerText) || !string.IsNullOrEmpty(_headerIcon)))
                DrawHeader(g, rtl);

            BuildLayout();
            foreach (var entry in _layout)
            {
                if (entry.IsHeader)
                    DrawSectionHeader(g, entry.Rect, _items[entry.ItemIndex].Text, rtl);
                else
                    DrawItem(g, entry.Rect, entry.ItemIndex, rtl);
            }
        }

        private void DrawHeader(Graphics g, bool rtl)
        {
            using (var p = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
                g.DrawLine(p, _padding, _headerHeight - 1, Width - _padding, _headerHeight - 1);

            int iconBoxSize = 32;
            int iconBoxY = (_headerHeight - iconBoxSize) / 2;

            Rectangle iconBoxRect, textRect;
            if (rtl)
            {
                // RTL: أيقونة الرأس على اليمين
                iconBoxRect = new Rectangle(Width - _padding - iconBoxSize - 8, iconBoxY, iconBoxSize, iconBoxSize);
                int textRight = iconBoxRect.Left - 10;
                textRect = new Rectangle(_padding + 8, 0, Math.Max(20, textRight - _padding - 8), _headerHeight);
            }
            else
            {
                // LTR: أيقونة الرأس على اليسار
                iconBoxRect = new Rectangle(_padding + 8, iconBoxY, iconBoxSize, iconBoxSize);
                int textLeft = iconBoxRect.Right + 10;
                textRect = new Rectangle(textLeft, 0, Math.Max(20, Width - textLeft - _padding - 8), _headerHeight);
            }

            if (!string.IsNullOrEmpty(_headerIcon))
            {
                using (var path = Round(iconBoxRect, 8))
                using (var b = new SolidBrush(Color.FromArgb(35, _indicatorColor)))
                    g.FillPath(b, path);

                try
                {
                    using (var f = FontManager.GetIconFont(GetHeaderFontKey(), 16F))
                    using (var b = new SolidBrush(_indicatorColor))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(_headerIcon, f, b, iconBoxRect, sf);
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(_headerText))
            {
                using (var f = new Font("Segoe UI", 12F, FontStyle.Bold))
                using (var b = new SolidBrush(_itemTextColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                {
                    if (rtl) sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    g.DrawString(_headerText, f, b, textRect, sf);
                }
            }
        }

        // ★★★ رسم العنوان الفرعي ★★★
        private void DrawSectionHeader(Graphics g, Rectangle rect, string text, bool rtl)
        {
            if (string.IsNullOrEmpty(text)) return;

            var inner = new Rectangle(rect.X + 8, rect.Y, Math.Max(0, rect.Width - 16), rect.Height);

            using (var b = new SolidBrush(_sectionHeaderColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                if (rtl) sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                g.DrawString(text, _sectionHeaderFont, b, inner, sf);
            }
        }

        // ★★★ رسم العنصر بعد الإصلاح ★★★
        private void DrawItem(Graphics g, Rectangle itemRect, int index, bool rtl)
        {
            var item = _items[index];
            bool isSelected = index == _selectedIndex;
            bool isHover = index == _hoverIndex;

            // الخلفية
            if (isSelected)
            {
                using (var path = Round(itemRect, _itemRadius))
                using (var b = new SolidBrush(_itemSelectedBack))
                    g.FillPath(b, path);
            }
            else if (isHover)
            {
                using (var path = Round(itemRect, _itemRadius))
                using (var b = new SolidBrush(_itemHoverBack))
                    g.FillPath(b, path);
            }

            // المؤشر النشط — نفس جهة الأيقونة
            if (isSelected && _showActiveIndicator)
            {
                int indX = rtl ? itemRect.Right + 1 : itemRect.X - 4;
                var indicator = new Rectangle(indX, itemRect.Y + 10, 3, itemRect.Height - 20);
                using (var path = Round(indicator, 2))
                using (var b = new SolidBrush(_indicatorColor))
                    g.FillPath(b, path);
            }

            int iconSlot = _itemHeight - 10;

            // قياس الشارة مسبقاً
            int badgeWidth = 0;
            if (!string.IsNullOrEmpty(item.Badge))
            {
                using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
                {
                    var ts = TextRenderer.MeasureText(item.Badge, f);
                    badgeWidth = Math.Max(ts.Width + 12, 22);
                }
            }

            Rectangle iconRect;
            Rectangle textRect;
            Rectangle badgeRect = Rectangle.Empty;

            if (rtl)
            {
                // ★ RTL: الأيقونة على اليمين، الشارة على اليسار
                iconRect = new Rectangle(
                    itemRect.Right - iconSlot - 4,
                    itemRect.Y + (itemRect.Height - iconSlot) / 2,
                    iconSlot, iconSlot);

                int textLeft = itemRect.X + 4;
                if (badgeWidth > 0)
                {
                    int bh = 20;
                    badgeRect = new Rectangle(
                        itemRect.X + 4,
                        itemRect.Y + (itemRect.Height - bh) / 2,
                        badgeWidth, bh);
                    textLeft += badgeWidth + 6;
                }

                int textRight = iconRect.Left - 6;
                textRect = new Rectangle(
                    textLeft, itemRect.Y,
                    Math.Max(20, textRight - textLeft), itemRect.Height);
            }
            else
            {
                // ★ LTR: الأيقونة على اليسار، الشارة على اليمين
                iconRect = new Rectangle(
                    itemRect.X + 4,
                    itemRect.Y + (itemRect.Height - iconSlot) / 2,
                    iconSlot, iconSlot);

                int textRight = itemRect.Right - 4;
                if (badgeWidth > 0)
                {
                    int bh = 20;
                    badgeRect = new Rectangle(
                        itemRect.Right - badgeWidth - 4,
                        itemRect.Y + (itemRect.Height - bh) / 2,
                        badgeWidth, bh);
                    textRight -= badgeWidth + 6;
                }

                int textLeft = iconRect.Right + 6;
                textRect = new Rectangle(
                    textLeft, itemRect.Y,
                    Math.Max(20, textRight - textLeft), itemRect.Height);
            }

            Color itemColor = isSelected ? _itemSelectedTextColor : _itemTextColor;
            Color iconCol = isSelected ? _itemSelectedTextColor : _iconColor;

            // رسم الأيقونة
            if (!string.IsNullOrEmpty(item.Icon))
            {
                try
                {
                    using (var f = FontManager.GetIconFont(GetItemFontKey(item), _iconSize))
                    using (var b = new SolidBrush(iconCol))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(item.Icon, f, b, iconRect, sf);
                }
                catch { }
            }

            // رسم النص — Near = حافة البداية تلقائياً حسب RTL
            using (var f = new Font(_itemFont, isSelected ? FontStyle.Bold : FontStyle.Regular))
            using (var b = new SolidBrush(itemColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
            {
                if (rtl) sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                g.DrawString(item.Text ?? "", f, b, textRect, sf);
            }

            // رسم الشارة
            if (badgeWidth > 0 && !badgeRect.IsEmpty)
                DrawBadge(g, badgeRect, item.Badge);
        }

        private void DrawBadge(Graphics g, Rectangle badgeRect, string badgeText)
        {
            using (var path = Round(badgeRect, badgeRect.Height / 2))
            using (var b = new SolidBrush(_badgeColor))
                g.FillPath(b, path);

            using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var b = new SolidBrush(_badgeTextColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(badgeText, f, b, badgeRect, sf);
        }

        #endregion

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = GetItemAtY(e.Y);
            if (idx != _hoverIndex)
            {
                _hoverIndex = idx;
                Cursor = idx >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoverIndex = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int idx = GetItemAtY(e.Y);
            if (idx >= 0) SelectedIndex = idx;
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        #endregion

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