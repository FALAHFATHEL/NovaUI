using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    public enum ToolbarItemType { Button, Separator, Label, Toggle, Dropdown }
    public enum ToolbarAlignment { Start, Center, End }

    #region ToolbarItem

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("عنصر واحد داخل NovaToolBar.")]
    public class ToolbarItem
    {
        private string _text = "";
        private string _icon = "";
        private string _tooltipText = "";
        private bool _checked = false;
        private bool _enabled = true;
        private ToolbarItemType _type = ToolbarItemType.Button;
        private List<ToolbarItem> _dropdownItems;
        private object _tag;

        public ToolbarItem() { }
        public ToolbarItem(string text = "", string icon = "")
        {
            _text = text ?? "";
            _icon = icon ?? "";
        }

        [Category("ToolbarItem")]
        [DisplayName("Type")]
        [DefaultValue(ToolbarItemType.Button)]
        public ToolbarItemType Type
        {
            get => _type;
            set => _type = value;
        }

        [Category("ToolbarItem")]
        [DisplayName("Text")]
        [DefaultValue("")]
        public string Text
        {
            get => _text;
            set => _text = value ?? "";
        }

        [Category("ToolbarItem")]
        [DisplayName("Icon")]
        [Description("رمز MDL2 (مثل: \\uE710).")]
        [DefaultValue("")]
        public string Icon
        {
            get => _icon;
            set => _icon = value ?? "";
        }

        [Category("ToolbarItem")]
        [DisplayName("Tooltip")]
        [DefaultValue("")]
        public string TooltipText
        {
            get => _tooltipText;
            set => _tooltipText = value ?? "";
        }

        [Category("ToolbarItem")]
        [DisplayName("Checked")]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set => _checked = value;
        }

        [Category("ToolbarItem")]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        [Category("ToolbarItem")]
        [DisplayName("DropdownItems")]
        [Browsable(false)]
        public List<ToolbarItem> DropdownItems
        {
            get => _dropdownItems;
            set => _dropdownItems = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Action<ToolbarItem> Click { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Tag
        {
            get => _tag;
            set => _tag = value;
        }

        public override string ToString()
        {
            if (_type == ToolbarItemType.Separator) return "— فاصل —";
            if (_type == ToolbarItemType.Label) return "[نص] " + _text;
            return string.IsNullOrEmpty(_text) ? "[أيقونة]" : _text;
        }
    }

    #endregion

    [ToolboxItem(true)]
    [DefaultEvent("ItemClicked")]
    [Description("شريط أدوات احترافي بدعم كامل لـ RTL/LTR وخصائص قابلة للتحكم.")]
    public class NovaToolBar : Control
    {
        #region FILEDS

        private readonly List<ToolbarItem> _items = new List<ToolbarItem>();
        private readonly List<(ToolbarItem item, Rectangle rect)> _itemRects
            = new List<(ToolbarItem, Rectangle)>();

        private int _hoverIndex = -1;
        private int _pressedIndex = -1;

        private int _toolbarHeight = 46;
        private int _itemHeight = 34;
        private int _itemPadding = 12;
        private int _itemSpacing = 4;
        private int _itemRadius = 6;
        private int _iconSize = 11;
        private int _iconTextGap = 6;
        private int _startPadding = 8;
        private int _endPadding = 8;
        private int _dropdownArrowWidth = 14;

        private int _borderRadius = 8;
        private Color _toolbarBackColor = Color.White;
        private Color _toolbarBackColor2 = Color.FromArgb(248, 250, 252);
        private bool _gradientEnabled = false;
        private float _gradientAngle = 90f;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private int _borderSize = 1;

        private Color _itemTextColor = Color.FromArgb(51, 65, 85);
        private Color _itemIconColor = Color.FromArgb(100, 116, 139);
        private Color _itemDisabledTextColor = Color.FromArgb(203, 213, 225);
        private Color _itemDisabledIconColor = Color.FromArgb(203, 213, 225);
        private Color _itemHoverBackColor = Color.FromArgb(241, 245, 249);
        private Color _itemHoverTextColor = Color.FromArgb(15, 23, 42);
        private Color _itemHoverIconColor = Color.FromArgb(15, 23, 42);
        private Color _itemPressedBackColor = Color.FromArgb(226, 232, 240);
        private Color _itemSelectedBackColor = Color.FromArgb(238, 242, 255);
        private Color _itemSelectedTextColor = Color.FromArgb(67, 56, 202);
        private Color _itemSelectedIconColor = Color.FromArgb(67, 56, 202);

        private Color _separatorColor = Color.FromArgb(226, 232, 240);
        private int _separatorWidth = 1;
        private int _separatorPadding = 8;

        private Color _labelColor = Color.FromArgb(148, 163, 184);
        private Font _labelFont;
        private FontStyle _labelFontStyle = FontStyle.Italic;

        // Align
        private ToolbarAlignment _itemsAlignment = ToolbarAlignment.Start;

        // Fonts
        private Font _itemFont;
        private Font _iconFont;

        // Dropdown menu
        private ContextMenuStrip _dropdownMenu;
        private ToolTip _tooltip;

        #endregion

        #region EVENTS

        public event EventHandler<ToolbarItem> ItemClicked;
        public event EventHandler<ToolbarItem> ItemCheckedChanged;
        public event EventHandler<ToolbarItem> ItemHover;

        #endregion

        public NovaToolBar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            base.BackColor = Color.Transparent;
            Size = new Size(700, _toolbarHeight);
            Font = new Font("Segoe UI", 9.5F);
            Cursor = Cursors.Default;

            // Fonts 
            _itemFont = new Font("Segoe UI", 9.5F);
            _iconFont = new Font("Segoe MDL2 Assets", _iconSize);
            _labelFont = new Font("Segoe UI", 9F, _labelFontStyle);

            _tooltip = new ToolTip { ShowAlways = true };
        }

        #region Properties

        // ============ ELEMETS ============

        [Category("NovaToolBar")]
        [Description("عناصر شريط الأدوات.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public List<ToolbarItem> Items => _items;

        // ============ DIMENTIONS ============

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(46)]
        public int ToolbarHeight
        {
            get => _toolbarHeight;
            set
            {
                _toolbarHeight = Math.Max(30, value);
                Height = _toolbarHeight;
                LayoutItems();
                Invalidate();
            }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(34)]
        public int ItemHeight
        {
            get => _itemHeight;
            set { _itemHeight = Math.Max(20, value); LayoutItems(); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(12)]
        [Description("هامش داخلي لكل عنصر (يسار/يمين).")]
        public int ItemPadding
        {
            get => _itemPadding;
            set { _itemPadding = Math.Max(2, value); LayoutItems(); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(4)]
        public int ItemSpacing
        {
            get => _itemSpacing;
            set { _itemSpacing = Math.Max(0, value); LayoutItems(); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(6)]
        public int ItemRadius
        {
            get => _itemRadius;
            set { _itemRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(11)]
        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = Math.Max(6, value);
                _iconFont?.Dispose();
                _iconFont = new Font("Segoe MDL2 Assets", _iconSize);
                LayoutItems();
                Invalidate();
            }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(6)]
        public int IconTextGap
        {
            get => _iconTextGap;
            set { _iconTextGap = Math.Max(0, value); LayoutItems(); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(8)]
        public int StartPadding
        {
            get => _startPadding;
            set { _startPadding = Math.Max(0, value); LayoutItems(); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(8)]
        public int EndPadding
        {
            get => _endPadding;
            set { _endPadding = Math.Max(0, value); LayoutItems(); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(14)]
        public int DropdownArrowWidth
        {
            get => _dropdownArrowWidth;
            set { _dropdownArrowWidth = Math.Max(8, value); LayoutItems(); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(8)]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaToolBar - DIMENTIONS"), DefaultValue(ToolbarAlignment.Start)]
        [Description("محاذاة العناصر: Start (بداية)، Center (وسط)، End (نهاية).")]
        public ToolbarAlignment ItemsAlignment
        {
            get => _itemsAlignment;
            set { _itemsAlignment = value; LayoutItems(); Invalidate(); }
        }

        // ============ COLORS ============

        [Category("NovaToolBar - COLORS")]
        public Color ToolbarBackColor
        {
            get => _toolbarBackColor;
            set { _toolbarBackColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - COLORS")]
        public Color ToolbarBackColor2
        {
            get => _toolbarBackColor2;
            set { _toolbarBackColor2 = value; Invalidate(); }
        }

        [Category("NovaToolBar - COLORS"), DefaultValue(false)]
        public bool GradientEnabled
        {
            get => _gradientEnabled;
            set { _gradientEnabled = value; Invalidate(); }
        }

        [Category("NovaToolBar - COLORS"), DefaultValue(90f)]
        public float GradientAngle
        {
            get => _gradientAngle;
            set { _gradientAngle = value; Invalidate(); }
        }

        [Category("NovaToolBar - COLORS")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - COLORS"), DefaultValue(1)]
        public int BorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); Invalidate(); }
        }

        // ============ ELEMENT COLOR ============

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemTextColor
        {
            get => _itemTextColor;
            set { _itemTextColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemIconColor
        {
            get => _itemIconColor;
            set { _itemIconColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemDisabledTextColor
        {
            get => _itemDisabledTextColor;
            set { _itemDisabledTextColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemDisabledIconColor
        {
            get => _itemDisabledIconColor;
            set { _itemDisabledIconColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemHoverBackColor
        {
            get => _itemHoverBackColor;
            set { _itemHoverBackColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemHoverTextColor
        {
            get => _itemHoverTextColor;
            set { _itemHoverTextColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemHoverIconColor
        {
            get => _itemHoverIconColor;
            set { _itemHoverIconColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemPressedBackColor
        {
            get => _itemPressedBackColor;
            set { _itemPressedBackColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemSelectedBackColor
        {
            get => _itemSelectedBackColor;
            set { _itemSelectedBackColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemSelectedTextColor
        {
            get => _itemSelectedTextColor;
            set { _itemSelectedTextColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - ECOLOR")]
        public Color ItemSelectedIconColor
        {
            get => _itemSelectedIconColor;
            set { _itemSelectedIconColor = value; Invalidate(); }
        }

        // ============ SEPERATOR  ============

        [Category("NovaToolBar - SEPERATOR")]
        public Color SeparatorColor
        {
            get => _separatorColor;
            set { _separatorColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - SEPERATOR"), DefaultValue(1)]
        public int SeparatorWidth
        {
            get => _separatorWidth;
            set { _separatorWidth = Math.Max(1, value); Invalidate(); }
        }

        [Category("NovaToolBar - SEPERATOR"), DefaultValue(8)]
        public int SeparatorPadding
        {
            get => _separatorPadding;
            set { _separatorPadding = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaToolBar - SEPERATOR")]
        public Color LabelColor
        {
            get => _labelColor;
            set { _labelColor = value; Invalidate(); }
        }

        [Category("NovaToolBar - SEPERATOR")]
        public FontStyle LabelFontStyle
        {
            get => _labelFontStyle;
            set
            {
                _labelFontStyle = value;
                _labelFont?.Dispose();
                _labelFont = new Font("Segoe UI", 9F, _labelFontStyle);
                LayoutItems();
                Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = Color.Transparent;
        }

        #endregion

        #region Public API

        public void AddButton(string text, string icon, Action<ToolbarItem> onClick = null)
        {
            _items.Add(new ToolbarItem(text, icon) { Click = onClick });
            LayoutItems();
            Invalidate();
        }

        public void AddToggle(string text, string icon, bool checked_ = false, Action<ToolbarItem> onClick = null)
        {
            _items.Add(new ToolbarItem(text, icon)
            {
                Type = ToolbarItemType.Toggle,
                Checked = checked_,
                Click = onClick
            });
            LayoutItems();
            Invalidate();
        }

        public void AddDropdown(string text, string icon, List<ToolbarItem> items)
        {
            _items.Add(new ToolbarItem(text, icon)
            {
                Type = ToolbarItemType.Dropdown,
                DropdownItems = items
            });
            LayoutItems();
            Invalidate();
        }

        public void AddSeparator()
        {
            _items.Add(new ToolbarItem { Type = ToolbarItemType.Separator });
            LayoutItems();
            Invalidate();
        }

        public void AddLabel(string text)
        {
            _items.Add(new ToolbarItem(text) { Type = ToolbarItemType.Label });
            LayoutItems();
            Invalidate();
        }

        public void ClearItems()
        {
            _items.Clear();
            _itemRects.Clear();
            Invalidate();
        }

        #endregion

        #region Layout

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            LayoutItems();
        }

        private void LayoutItems()
        {
            _itemRects.Clear();
            if (_items.Count == 0) return;

            bool rtl = RightToLeft == RightToLeft.Yes;
            int y = (_toolbarHeight - _itemHeight) / 2;

            // قياس كل عرض
            int totalWidth = 0;
            var widths = new int[_items.Count];
            for (int i = 0; i < _items.Count; i++)
            {
                widths[i] = MeasureItemWidth(_items[i]);
                totalWidth += widths[i];
                if (_items[i].Type != ToolbarItemType.Separator &&
                    _items[i].Type != ToolbarItemType.Label)
                    totalWidth += _itemSpacing;
            }
            if (_items.Count > 0 && _items[_items.Count - 1].Type != ToolbarItemType.Separator)
                totalWidth -= _itemSpacing;

            int startX;
            switch (_itemsAlignment)
            {
                case ToolbarAlignment.Center:
                    startX = Math.Max(_startPadding, (Width - totalWidth) / 2);
                    break;
                case ToolbarAlignment.End:
                    startX = Math.Max(_startPadding, Width - _endPadding - totalWidth);
                    break;
                default: // Start
                    startX = _startPadding;
                    break;
            }

            int cursor = rtl ? (Width - startX) : startX;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                int w = widths[i];

                Rectangle rect;
                if (rtl)
                {
                    cursor -= w;
                    rect = new Rectangle(cursor, y, w, _itemHeight);

                    if (item.Type == ToolbarItemType.Separator)
                        rect = new Rectangle(cursor, y + _separatorPadding,
                                             _separatorWidth, _itemHeight - _separatorPadding * 2);
                    else if (item.Type == ToolbarItemType.Label)
                        rect = new Rectangle(cursor, 0, w, _toolbarHeight);

                    cursor -= _itemSpacing;
                }
                else
                {
                    rect = new Rectangle(cursor, y, w, _itemHeight);

                    if (item.Type == ToolbarItemType.Separator)
                        rect = new Rectangle(cursor, y + _separatorPadding,
                                             _separatorWidth, _itemHeight - _separatorPadding * 2);
                    else if (item.Type == ToolbarItemType.Label)
                        rect = new Rectangle(cursor, 0, w, _toolbarHeight);

                    cursor += w + _itemSpacing;
                }

                _itemRects.Add((item, rect));
            }
        }

        private int MeasureItemWidth(ToolbarItem item)
        {
            if (item.Type == ToolbarItemType.Separator) return _separatorWidth;
            if (item.Type == ToolbarItemType.Label)
            {
                var s = TextRenderer.MeasureText(item.Text, _labelFont,
                    new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                return s.Width + 16;
            }

            int w = _itemPadding * 2;

            if (!string.IsNullOrEmpty(item.Icon))
            {
                var iconSize = TextRenderer.MeasureText(item.Icon, _iconFont,
                    new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                w += iconSize.Width + 4;
            }

            if (!string.IsNullOrEmpty(item.Text))
            {
                var textSize = TextRenderer.MeasureText(item.Text, _itemFont,
                    new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                w += textSize.Width;
                if (!string.IsNullOrEmpty(item.Icon)) w += _iconTextGap;
            }

            if (item.Type == ToolbarItemType.Dropdown) w += _dropdownArrowWidth;

            return Math.Max(32, w);
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            bool rtl = RightToLeft == RightToLeft.Yes;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var path = Round(rect, _borderRadius))
            {
                if (_gradientEnabled)
                {
                    using (var brush = new LinearGradientBrush(rect,
                        _toolbarBackColor, _toolbarBackColor2, _gradientAngle))
                        g.FillPath(brush, path);
                }
                else
                {
                    using (var b = new SolidBrush(_toolbarBackColor))
                        g.FillPath(b, path);
                }
            }

            if (_borderSize > 0)
            {
                using (var path = Round(rect, _borderRadius))
                using (var p = new Pen(_borderColor, _borderSize))
                {
                    p.Alignment = PenAlignment.Inset;
                    g.DrawPath(p, path);
                }
            }

            for (int i = 0; i < _itemRects.Count; i++)
            {
                var (item, itemRect) = _itemRects[i];
                int itemIndex = _items.IndexOf(item);

                if (item.Type == ToolbarItemType.Separator)
                {
                    using (var p = new Pen(_separatorColor, _separatorWidth))
                        g.DrawLine(p, itemRect.X, itemRect.Y,
                                       itemRect.X, itemRect.Bottom);
                    continue;
                }

                if (item.Type == ToolbarItemType.Label)
                {
                    TextRenderer.DrawText(g, item.Text, _labelFont, itemRect, _labelColor,
                        (rtl ? TextFormatFlags.Right : TextFormatFlags.Left) |
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    continue;
                }

                DrawItem(g, item, itemRect, itemIndex, rtl);
            }
        }

        private void DrawItem(Graphics g, ToolbarItem item, Rectangle rect, int index, bool rtl)
        {
            bool selected = item.Type == ToolbarItemType.Toggle && item.Checked;
            bool hover = index == _hoverIndex && item.Enabled;
            bool pressed = index == _pressedIndex && item.Enabled;

            Color? bg = null;
            if (pressed) bg = _itemPressedBackColor;
            else if (selected) bg = _itemSelectedBackColor;
            else if (hover) bg = _itemHoverBackColor;

            if (bg.HasValue)
            {
                using (var path = Round(rect, _itemRadius))
                using (var b = new SolidBrush(bg.Value))
                    g.FillPath(b, path);
            }

            // ============ COLORS ============
            Color iconColor = !item.Enabled ? _itemDisabledIconColor
                            : selected ? _itemSelectedIconColor
                            : hover ? _itemHoverIconColor
                            : _itemIconColor;

            Color textColor = !item.Enabled ? _itemDisabledTextColor
                            : selected ? _itemSelectedTextColor
                            : hover ? _itemHoverTextColor
                            : _itemTextColor;

            bool hasIcon = !string.IsNullOrEmpty(item.Icon);
            bool hasText = !string.IsNullOrEmpty(item.Text);
            bool hasArrow = item.Type == ToolbarItemType.Dropdown;

            int iconW = 0;
            if (hasIcon)
            {
                var size = TextRenderer.MeasureText(item.Icon, _iconFont,
                    new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                iconW = size.Width + 4;
            }

            int textW = 0;
            if (hasText)
            {
                var size = TextRenderer.MeasureText(item.Text, _itemFont,
                    new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                textW = size.Width;
            }

            int arrowW = hasArrow ? _dropdownArrowWidth : 0;

            int totalContentW = iconW + (hasIcon && hasText ? _iconTextGap : 0) + textW + arrowW;

            int contentX = rect.X + (rect.Width - totalContentW) / 2;

            if (rtl)
            {
                int rightCursor = rect.Right - (rect.Width - totalContentW) / 2;

                if (hasIcon)
                {
                    var iconRect = new Rectangle(
                        rightCursor - iconW, rect.Y, iconW, rect.Height);
                    DrawIconCentered(g, item.Icon, iconRect, iconColor);
                    rightCursor = iconRect.Left;
                    if (hasText) rightCursor -= _iconTextGap;
                }

                if (hasText)
                {
                    var textRect = new Rectangle(rightCursor - textW, rect.Y, textW, rect.Height);
                    TextRenderer.DrawText(g, item.Text, _itemFont, textRect, textColor,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    rightCursor = textRect.Left;
                }

                if (hasArrow)
                {
                    var arrowRect = new Rectangle(rightCursor - arrowW, rect.Y, arrowW, rect.Height);
                    DrawArrow(g, arrowRect, iconColor, rtl);
                }
            }
            else
            {
                int cursor = contentX;

                if (hasIcon)
                {
                    var iconRect = new Rectangle(cursor, rect.Y, iconW, rect.Height);
                    DrawIconCentered(g, item.Icon, iconRect, iconColor);
                    cursor += iconW;
                    if (hasText) cursor += _iconTextGap;
                }

                if (hasText)
                {
                    var textRect = new Rectangle(cursor, rect.Y, textW, rect.Height);
                    TextRenderer.DrawText(g, item.Text, _itemFont, textRect, textColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    cursor += textW;
                }

                if (hasArrow)
                {
                    var arrowRect = new Rectangle(cursor, rect.Y, arrowW, rect.Height);
                    DrawArrow(g, arrowRect, iconColor, rtl);
                }
            }
        }

        private void DrawIconCentered(Graphics g, string glyph, Rectangle rect, Color color)
        {
            if (_iconFont == null || string.IsNullOrEmpty(glyph)) return;

            var measured = TextRenderer.MeasureText(glyph, _iconFont,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            int x = rect.X + (rect.Width - measured.Width) / 2;
            int y = rect.Y + (rect.Height - measured.Height) / 2;

            TextRenderer.DrawText(g, glyph, _iconFont, new Point(x, y), color,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        private void DrawArrow(Graphics g, Rectangle rect, Color color, bool rtl)
        {
            using (var pen = new Pen(color, 1.6f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;

                g.DrawLines(pen, new[]
                {
                    new PointF(cx - 3, cy - 2),
                    new PointF(cx, cy + 1),
                    new PointF(cx + 3, cy - 2)
                });
            }
        }

        #endregion

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = GetIndexAtPoint(e.Location);

            if (idx != _hoverIndex)
            {
                int oldIdx = _hoverIndex;
                _hoverIndex = idx;

                Cursor = idx >= 0 && _items[idx].Enabled &&
                         _items[idx].Type != ToolbarItemType.Label &&
                         _items[idx].Type != ToolbarItemType.Separator
                    ? Cursors.Hand : Cursors.Default;

                // Tooltip
                if (idx >= 0 && !string.IsNullOrEmpty(_items[idx].TooltipText))
                    _tooltip.SetToolTip(this, _items[idx].TooltipText);
                else
                    _tooltip.SetToolTip(this, "");

                if (idx >= 0) ItemHover?.Invoke(this, _items[idx]);

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                _pressedIndex = -1;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            int idx = GetIndexAtPoint(e.Location);
            if (idx < 0) return;

            var item = _items[idx];
            if (!item.Enabled ||
                item.Type == ToolbarItemType.Separator ||
                item.Type == ToolbarItemType.Label) return;

            _pressedIndex = idx;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_pressedIndex < 0)
            {
                int idx = GetIndexAtPoint(e.Location);
                if (idx >= 0 && _items[idx].Type == ToolbarItemType.Dropdown)
                    ShowDropdown(idx, e.Location);
                return;
            }

            int pIdx = _pressedIndex;
            _pressedIndex = -1;

            int uIdx = GetIndexAtPoint(e.Location);
            if (uIdx != pIdx) { Invalidate(); return; }

            var item = _items[pIdx];

            if (item.Type == ToolbarItemType.Toggle)
            {
                item.Checked = !item.Checked;
                ItemCheckedChanged?.Invoke(this, item);
            }
            else if (item.Type == ToolbarItemType.Dropdown)
            {
                ShowDropdown(pIdx, e.Location);
            }
            else
            {
                item.Click?.Invoke(item);
                ItemClicked?.Invoke(this, item);
            }

            Invalidate();
        }

        private void ShowDropdown(int index, Point location)
        {
            var item = _items[index];
            if (item.DropdownItems == null || item.DropdownItems.Count == 0) return;

            if (_dropdownMenu != null && !_dropdownMenu.IsDisposed)
                _dropdownMenu.Dispose();

            _dropdownMenu = new ContextMenuStrip
            {
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                ShowImageMargin = false,
                Padding = new Padding(6)
            };

            foreach (var sub in item.DropdownItems)
            {
                if (sub.Type == ToolbarItemType.Separator)
                {
                    _dropdownMenu.Items.Add(new ToolStripSeparator());
                    continue;
                }

                var tsItem = new ToolStripMenuItem(sub.Text);
                if (!string.IsNullOrEmpty(sub.Icon))
                    tsItem.Text = sub.Icon + "   " + sub.Text;

                tsItem.Enabled = sub.Enabled;

                var captured = sub;
                tsItem.Click += (s, e) =>
                {
                    captured.Click?.Invoke(captured);
                    ItemClicked?.Invoke(this, captured);
                };

                _dropdownMenu.Items.Add(tsItem);
            }

            // الحصول على rectangle العنصر
            var itemRect = _itemRects.Find(t => t.item == item).rect;
            _dropdownMenu.Show(this, new Point(itemRect.X, itemRect.Bottom + 2));
        }

        private int GetIndexAtPoint(Point p)
        {
            for (int i = 0; i < _itemRects.Count; i++)
                if (_itemRects[i].rect.Contains(p)) return _items.IndexOf(_itemRects[i].item);
            return -1;
        }

        #endregion

        #region RTL

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            LayoutItems();
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
                _itemFont?.Dispose();
                _iconFont?.Dispose();
                _labelFont?.Dispose();
                _dropdownMenu?.Dispose();
                _tooltip?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}