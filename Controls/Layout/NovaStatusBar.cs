using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    #region StatusBarItem

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("عنصر واحد داخل NovaStatusBar.")]
    public class StatusBarItem
    {
        private string _text = "";
        private string _icon = "";
        private Color _iconColor = Color.Empty;
        private Color _textColor = Color.Empty;
        private StatusItemAlign _align = StatusItemAlign.Left;
        private int _minWidth = 0;
        private bool _enabled = true;
        private string _tooltip = "";
        private object _tag;

        public StatusBarItem() { }

        public StatusBarItem(string text, string icon = "")
        {
            _text = text ?? "";
            _icon = icon ?? "";
        }

        [Category("StatusBarItem")]
        [DisplayName("Text")]
        [Description("نص العنصر.")]
        [DefaultValue("")]
        public string Text
        {
            get => _text;
            set => _text = value ?? "";
        }

        [Category("StatusBarItem")]
        [DisplayName("Icon")]
        [Description("رمز أيقونة MDL2 (اتركه فارغاً لبدون أيقونة).")]
        [DefaultValue("")]
        public string Icon
        {
            get => _icon;
            set => _icon = value ?? "";
        }

        [Category("StatusBarItem")]
        [DisplayName("IconColor")]
        [Description("لون الأيقونة (فارغ = يستخدم IconColor العام).")]
        public Color ItemIconColor
        {
            get => _iconColor;
            set => _iconColor = value;
        }

        [Category("StatusBarItem")]
        [DisplayName("TextColor")]
        [Description("لون النص (فارغ = يستخدم TextColor العام).")]
        public Color ItemTextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        [Category("StatusBarItem")]
        [DisplayName("Align")]
        [Description("مكان العنصر: Left = البداية، Right = النهاية (يتأثر بـ RTL).")]
        [DefaultValue(StatusItemAlign.Left)]
        public StatusItemAlign Align
        {
            get => _align;
            set => _align = value;
        }

        [Category("StatusBarItem")]
        [DisplayName("MinWidth")]
        [Description("العرض الأدنى بالبكسل (0 = تلقائي).")]
        [DefaultValue(0)]
        public int MinWidth
        {
            get => _minWidth;
            set => _minWidth = Math.Max(0, value);
        }

        [Category("StatusBarItem")]
        [DisplayName("Enabled")]
        [DefaultValue(true)]
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        [Category("StatusBarItem")]
        [DisplayName("Tooltip")]
        [Description("تلميح يظهر عند المرور (اختياري).")]
        [DefaultValue("")]
        public string Tooltip
        {
            get => _tooltip;
            set => _tooltip = value ?? "";
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Tag
        {
            get => _tag;
            set => _tag = value;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_text) && string.IsNullOrEmpty(_icon)) return "(فارغ)";
            if (string.IsNullOrEmpty(_text)) return "[أيقونة]";
            return _text;
        }
    }

    public enum StatusItemAlign { Left, Center, Right }

    public enum StatusBarBorderSide { None, Top, Bottom }

    #endregion

    [ToolboxItem(true)]
    [DefaultEvent("ItemClicked")]
    [Description("شريط حالة احترافي بمواضع أيقونات دقيقة ودعم كامل لـ RTL/LTR.")]
    public class NovaStatusBar : Control
    {
        #region Fileds

        private readonly List<StatusBarItem> _items = new List<StatusBarItem>();
        private readonly List<(StatusBarItem item, Rectangle rect)> _itemRects
            = new List<(StatusBarItem, Rectangle)>();

        private int _barHeight = 34;
        private int _iconSize = 12;
        private int _itemPadding = 14;
        private int _itemGap = 10;
        private int _iconTextGap = 6;
        private int _radius = 0;

        private Color _barBackColor = Color.FromArgb(248, 250, 252);
        private Color _barBackColor2 = Color.FromArgb(241, 245, 249);
        private bool _gradientEnabled = false;
        private float _gradientAngle = 90f;

        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private int _borderSize = 1;
        private StatusBarBorderSide _borderSide = StatusBarBorderSide.Top;

        private Color _textColor = Color.FromArgb(71, 85, 105);
        private Color _iconColor = Color.FromArgb(100, 116, 139);
        private Color _disabledTextColor = Color.FromArgb(203, 213, 225);
        private Color _disabledIconColor = Color.FromArgb(203, 213, 225);

        private Color _separatorColor = Color.FromArgb(226, 232, 240);
        private int _separatorThickness = 1;
        private int _separatorPadding = 8;

        // Hover
        private bool _itemHoverEnabled = true;
        private Color _itemHoverBackColor = Color.FromArgb(238, 242, 255);
        private Color _itemHoverTextColor = Color.FromArgb(67, 56, 202);
        private Color _itemHoverIconColor = Color.FromArgb(67, 56, 202);

        private bool _showSeparators = true;

        private bool _showClock = true;
        private string _clockFormat = "HH:mm:ss";
        private string _clockIcon = "\uE823";
        private Color _clockTextColor = Color.FromArgb(71, 85, 105);
        private Color _clockIconColor = Color.FromArgb(100, 116, 139);
        private StatusItemAlign _clockAlign = StatusItemAlign.Right;
        private Timer _clockTimer;
        private DateTime _clockTime = DateTime.Now;

        // Hover state
        private StatusBarItem _hoverItem;

        private Font _iconFont;

        // Tooltip
        private readonly ToolTip _tooltip = new ToolTip();

        #endregion

        #region Events

        public event EventHandler<StatusBarItem> ItemClicked;
        public event EventHandler<StatusBarItem> ItemHover;

        #endregion

        public NovaStatusBar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Height = _barHeight;
            Dock = DockStyle.Bottom;
            Font = new Font("Segoe UI", 9F);
            Cursor = Cursors.Default;

            // Fonts
            _iconFont = new Font("Segoe MDL2 Assets", _iconSize * 0.9f);

            _clockTimer = new Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) => { _clockTime = DateTime.Now; Invalidate(); };
        }

        #region Properties - الأبعاد

        [Category("NovaStatusBar - الأبعاد"), DefaultValue(34)]
        [Description("ارتفاع الشريط.")]
        public int BarHeight
        {
            get => _barHeight;
            set { _barHeight = Math.Max(20, value); Height = _barHeight; Invalidate(); }
        }

        [Category("NovaStatusBar - الأبعاد"), DefaultValue(12)]
        [Description("حجم أيقونات العناصر.")]
        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = Math.Max(6, value);
                RebuildIconFont();
                Invalidate();
            }
        }

        [Category("NovaStatusBar - الأبعاد"), DefaultValue(14)]
        [Description("المسافة بين حافة الشريط وأول/آخر عنصر.")]
        public int ItemPadding
        {
            get => _itemPadding;
            set { _itemPadding = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaStatusBar - الأبعاد"), DefaultValue(10)]
        [Description("المسافة بين العناصر.")]
        public int ItemGap
        {
            get => _itemGap;
            set { _itemGap = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaStatusBar - الأبعاد"), DefaultValue(6)]
        [Description("المسافة بين الأيقونة والنص داخل العنصر.")]
        public int IconTextGap
        {
            get => _iconTextGap;
            set { _iconTextGap = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaStatusBar - الأبعاد"), DefaultValue(0)]
        [Description("استدارة زوايا الشريط.")]
        public int BorderRadius
        {
            get => _radius;
            set { _radius = Math.Max(0, value); Invalidate(); }
        }

        #endregion

        #region Properties - الألوان

        [Category("NovaStatusBar - الألوان")]
        [Description("لون الخلفية الأساسي.")]
        public Color BarBackColor
        {
            get => _barBackColor;
            set { _barBackColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الألوان")]
        [Description("لون الخلفية الثاني (للتدرج).")]
        public Color BarBackColor2
        {
            get => _barBackColor2;
            set { _barBackColor2 = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الألوان"), DefaultValue(false)]
        [Description("تفعيل الخلفية المتدرجة.")]
        public bool GradientEnabled
        {
            get => _gradientEnabled;
            set { _gradientEnabled = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الألوان"), DefaultValue(90f)]
        public float GradientAngle
        {
            get => _gradientAngle;
            set { _gradientAngle = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الألوان")]
        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الألوان")]
        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الألوان")]
        [Description("لون النص للعناصر المعطلة.")]
        public Color DisabledTextColor
        {
            get => _disabledTextColor;
            set { _disabledTextColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الألوان")]
        public Color DisabledIconColor
        {
            get => _disabledIconColor;
            set { _disabledIconColor = value; Invalidate(); }
        }

        #endregion

        #region Properties - Borders

        [Category("NovaStatusBar - Borders")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - الحدود"), DefaultValue(1)]
        public int BorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("NovaStatusBar - Borders"), DefaultValue(StatusBarBorderSide.Top)]
        [Description("جهة الحدود: Top (فوق) أو Bottom (تحت).")]
        public StatusBarBorderSide BorderSide
        {
            get => _borderSide;
            set { _borderSide = value; Invalidate(); }
        }

        #endregion

        #region Properties - DET

        [Category("NovaStatusBar - DET"), DefaultValue(true)]
        [Description("إظهار المحددات بين العناصر.")]
        public bool ShowSeparators
        {
            get => _showSeparators;
            set { _showSeparators = value; Invalidate(); }
        }

        [Category("NovaStatusBar - DET")]
        public Color SeparatorColor
        {
            get => _separatorColor;
            set { _separatorColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - DET"), DefaultValue(1)]
        public int SeparatorThickness
        {
            get => _separatorThickness;
            set { _separatorThickness = Math.Max(1, value); Invalidate(); }
        }

        [Category("NovaStatusBar - DET"), DefaultValue(8)]
        [Description("المسافة من أعلى/أسفل المحدد.")]
        public int SeparatorPadding
        {
            get => _separatorPadding;
            set { _separatorPadding = Math.Max(0, value); Invalidate(); }
        }

        #endregion

        #region Properties - Hover

        [Category("NovaStatusBar - Hover"), DefaultValue(true)]
        [Description("تفعيل تأثير Hover على العناصر.")]
        public bool ItemHoverEnabled
        {
            get => _itemHoverEnabled;
            set { _itemHoverEnabled = value; Invalidate(); }
        }

        [Category("NovaStatusBar - Hover")]
        public Color ItemHoverBackColor
        {
            get => _itemHoverBackColor;
            set { _itemHoverBackColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - Hover")]
        public Color ItemHoverTextColor
        {
            get => _itemHoverTextColor;
            set { _itemHoverTextColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - Hover")]
        public Color ItemHoverIconColor
        {
            get => _itemHoverIconColor;
            set { _itemHoverIconColor = value; Invalidate(); }
        }

        #endregion

        #region Properties - CLOCK

        [Category("NovaStatusBar - CLOCK"), DefaultValue(true)]
        public bool ShowClock
        {
            get => _showClock;
            set { _showClock = value; UpdateClockTimer(); Invalidate(); }
        }

        [Category("NovaStatusBar - CLOCK"), DefaultValue("HH:mm:ss")]
        [Description("صيغة الساعة (مثل: HH:mm:ss، hh:mm tt، HH:mm).")]
        public string ClockFormat
        {
            get => _clockFormat;
            set { _clockFormat = value ?? "HH:mm:ss"; Invalidate(); }
        }

        [Category("NovaStatusBar - CLOCK"), DefaultValue("\uE823")]
        [Description("أيقونة الساعة (MDL2). اتركه فارغاً لإخفائها.")]
        public string ClockIcon
        {
            get => _clockIcon;
            set { _clockIcon = value ?? ""; Invalidate(); }
        }

        [Category("NovaStatusBar - CLOCK")]
        public Color ClockTextColor
        {
            get => _clockTextColor;
            set { _clockTextColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - CLOCK")]
        public Color ClockIconColor
        {
            get => _clockIconColor;
            set { _clockIconColor = value; Invalidate(); }
        }

        [Category("NovaStatusBar - CLOCK"), DefaultValue(StatusItemAlign.Right)]
        public StatusItemAlign ClockAlign
        {
            get => _clockAlign;
            set { _clockAlign = value; Invalidate(); }
        }

        #endregion

        #region Properties - ELEMENTS

        [Category("NovaStatusBar")]
        [Description("عناصر الشريط.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public List<StatusBarItem> Items => _items;

        #endregion

        #region Public API

        public StatusBarItem AddItem(string text, string icon = "",
                                     StatusItemAlign align = StatusItemAlign.Left,
                                     Color? iconColor = null)
        {
            var item = new StatusBarItem(text, icon)
            {
                Align = align,
                ItemIconColor = iconColor ?? Color.Empty
            };
            _items.Add(item);
            Invalidate();
            return item;
        }

        public void UpdateItem(StatusBarItem item, string newText,
                               string newIcon = null, Color? newIconColor = null)
        {
            if (item == null) return;
            item.Text = newText;
            if (newIcon != null) item.Icon = newIcon;
            if (newIconColor.HasValue) item.ItemIconColor = newIconColor.Value;
            Invalidate();
        }

        public void RemoveItem(StatusBarItem item)
        {
            if (item == null || !_items.Remove(item)) return;
            Invalidate();
        }

        public void ClearItems()
        {
            _items.Clear();
            Invalidate();
        }

        #endregion

        #region Fonts

        private void RebuildIconFont()
        {
            _iconFont?.Dispose();
            _iconFont = new Font("Segoe MDL2 Assets", _iconSize * 0.9f, FontStyle.Regular);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        #endregion

        #region Timer

        private void UpdateClockTimer()
        {
            if (_showClock && Visible && !DesignMode)
                _clockTimer.Start();
            else
                _clockTimer.Stop();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateClockTimer();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateClockTimer();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _clockTimer?.Stop();
            base.OnHandleDestroyed(e);
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

            // ============ الخلفية ============
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Round(rect, _radius))
            {
                if (_gradientEnabled)
                {
                    using (var brush = new LinearGradientBrush(rect, _barBackColor, _barBackColor2, _gradientAngle))
                        g.FillPath(brush, path);
                }
                else
                {
                    using (var b = new SolidBrush(_barBackColor))
                        g.FillPath(b, path);
                }
            }

            if (_borderSize > 0 && _borderSide != StatusBarBorderSide.None)
            {
                using (var p = new Pen(_borderColor, _borderSize))
                {
                    if (_borderSide == StatusBarBorderSide.Top)
                        g.DrawLine(p, 0, _borderSize / 2f, Width, _borderSize / 2f);
                    else
                        g.DrawLine(p, 0, Height - _borderSize / 2f - 0.5f,
                                       Width, Height - _borderSize / 2f - 0.5f);
                }
            }

            _itemRects.Clear();

            var leftGroup = new List<StatusBarItem>();
            var centerGroup = new List<StatusBarItem>();
            var rightGroup = new List<StatusBarItem>();
            var clockGroup = new List<StatusBarItem>();

            foreach (var item in _items)
            {
                switch (item.Align)
                {
                    case StatusItemAlign.Left: leftGroup.Add(item); break;
                    case StatusItemAlign.Center: centerGroup.Add(item); break;
                    case StatusItemAlign.Right: rightGroup.Add(item); break;
                }
            }

            StatusBarItem clockItem = null;
            if (_showClock)
            {
                clockItem = new StatusBarItem
                {
                    Text = _clockTime.ToString(_clockFormat),
                    Icon = _clockIcon,
                    Align = _clockAlign,
                    ItemIconColor = _clockIconColor,
                    ItemTextColor = _clockTextColor
                };

                switch (_clockAlign)
                {
                    case StatusItemAlign.Left: leftGroup.Add(clockItem); break;
                    case StatusItemAlign.Center: centerGroup.Add(clockItem); break;
                    case StatusItemAlign.Right: rightGroup.Add(clockItem); break;
                }
            }

            // ============ 1) Left Group ============
            int startX = _itemPadding;
            int y = 0;
            int rowH = Height;

            for (int i = 0; i < leftGroup.Count; i++)
            {
                var item = leftGroup[i];
                int w = MeasureItemWidth(item);

                int actualX = rtl ? (Width - startX - w) : startX;
                var itemRect = new Rectangle(actualX, y, w, rowH);

                DrawItem(g, item, itemRect, rtl);
                _itemRects.Add((item, itemRect));

                startX += w;

                if (_showSeparators && i < leftGroup.Count - 1)
                {
                    int sepX = rtl ? (Width - startX - _itemGap / 2) : (startX + _itemGap / 2);
                    DrawSeparator(g, sepX);
                }

                startX += _itemGap;
            }

            // ============ 2) Right Group ============
            int endX = _itemPadding;
            for (int i = 0; i < rightGroup.Count; i++)
            {
                var item = rightGroup[i];
                int w = MeasureItemWidth(item);

                int actualX = rtl ? endX : (Width - endX - w);
                var itemRect = new Rectangle(actualX, y, w, rowH);

                DrawItem(g, item, itemRect, rtl);
                _itemRects.Add((item, itemRect));

                endX += w;

                if (_showSeparators && i < rightGroup.Count - 1)
                {
                    int sepX = rtl ? (endX + _itemGap / 2) : (Width - endX - _itemGap / 2);
                    DrawSeparator(g, sepX);
                }

                endX += _itemGap;
            }

            // ============ 3) Center Group ============
            if (centerGroup.Count > 0)
            {
                int totalCenterW = 0;
                for (int i = 0; i < centerGroup.Count; i++)
                {
                    totalCenterW += MeasureItemWidth(centerGroup[i]);
                    if (i < centerGroup.Count - 1) totalCenterW += _itemGap;
                }

                int centerX = (Width - totalCenterW) / 2;

                for (int i = 0; i < centerGroup.Count; i++)
                {
                    var item = centerGroup[i];
                    int w = MeasureItemWidth(item);
                    var itemRect = new Rectangle(centerX, y, w, rowH);

                    DrawItem(g, item, itemRect, rtl);
                    _itemRects.Add((item, itemRect));

                    centerX += w;

                    if (_showSeparators && i < centerGroup.Count - 1)
                    {
                        int sepX = centerX + _itemGap / 2;
                        DrawSeparator(g, sepX);
                    }

                    centerX += _itemGap;
                }
            }
        }

        private void DrawSeparator(Graphics g, int x)
        {
            using (var p = new Pen(_separatorColor, _separatorThickness))
            {
                g.DrawLine(p, x, _separatorPadding, x, Height - _separatorPadding);
            }
        }

        private void DrawItem(Graphics g, StatusBarItem item, Rectangle rect, bool rtl)
        {
            bool isHover = _itemHoverEnabled && item == _hoverItem && item.Enabled;

            if (isHover)
            {
                var hoverRect = new Rectangle(rect.X - 4, rect.Y + 3,
                                              rect.Width + 8, rect.Height - 6);
                using (var path = Round(hoverRect, 6))
                using (var b = new SolidBrush(_itemHoverBackColor))
                    g.FillPath(b, path);
            }

            Color textColor = !item.Enabled ? _disabledTextColor
                            : isHover ? _itemHoverTextColor
                            : item.ItemTextColor != Color.Empty ? item.ItemTextColor
                            : _textColor;

            Color iconColor = !item.Enabled ? _disabledIconColor
                            : isHover ? _itemHoverIconColor
                            : item.ItemIconColor != Color.Empty ? item.ItemIconColor
                            : _iconColor;

            int contentX = rect.X;
            int contentW = rect.Width;

            int iconW = 0;
            int textW = 0;

            if (!string.IsNullOrEmpty(item.Icon)) iconW = _iconSize + 4;
            if (!string.IsNullOrEmpty(item.Text))
                textW = TextRenderer.MeasureText(item.Text, Font, new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.NoPadding).Width;

            bool hasBoth = iconW > 0 && textW > 0;
            int totalW = iconW + textW + (hasBoth ? _iconTextGap : 0);

            int alignOffset = (rect.Width - totalW) / 2;
            int cursor = rect.X + alignOffset;

            Rectangle iconRect = Rectangle.Empty;
            Rectangle textRect = Rectangle.Empty;

            if (rtl)
            {
                if (textW > 0)
                {
                    textRect = new Rectangle(cursor, rect.Y, textW, rect.Height);
                    cursor += textW;
                }
                if (hasBoth) cursor += _iconTextGap;
                if (iconW > 0)
                {
                    iconRect = new Rectangle(cursor, rect.Y, iconW, rect.Height);
                }
            }
            else
            {
                if (iconW > 0)
                {
                    iconRect = new Rectangle(cursor, rect.Y, iconW, rect.Height);
                    cursor += iconW;
                }
                if (hasBoth) cursor += _iconTextGap;
                if (textW > 0)
                {
                    textRect = new Rectangle(cursor, rect.Y, textW, rect.Height);
                }
            }

            if (!iconRect.IsEmpty && !string.IsNullOrEmpty(item.Icon))
            {
                DrawIconCentered(g, item.Icon, iconRect, iconColor);
            }

            if (!textRect.IsEmpty && !string.IsNullOrEmpty(item.Text))
            {
                TextRenderer.DrawText(g, item.Text, Font, textRect, textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
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

        #endregion

        #region Measurement

        private int MeasureItemWidth(StatusBarItem item)
        {
            int w = 0;

            if (!string.IsNullOrEmpty(item.Icon))
                w += _iconSize + 4;

            if (!string.IsNullOrEmpty(item.Text))
            {
                var textSize = TextRenderer.MeasureText(item.Text, Font,
                    new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.NoPadding);
                w += textSize.Width;
            }

            if (!string.IsNullOrEmpty(item.Icon) && !string.IsNullOrEmpty(item.Text))
                w += _iconTextGap;

            w += 8;

            return Math.Max(item.MinWidth, w);
        }

        #endregion

        #region Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var hit = HitTest(e.Location);

            if (hit != _hoverItem)
            {
                _hoverItem = hit;
                Cursor = hit != null && hit.Enabled ? Cursors.Hand : Cursors.Default;
                ItemHover?.Invoke(this, hit);

                if (hit != null && !string.IsNullOrEmpty(hit.Tooltip))
                    _tooltip.SetToolTip(this, hit.Tooltip);
                else
                    _tooltip.SetToolTip(this, "");

                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverItem != null)
            {
                _hoverItem = null;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;

            var hit = HitTest(e.Location);
            if (hit != null && hit.Enabled)
                ItemClicked?.Invoke(this, hit);
        }

        private StatusBarItem HitTest(Point p)
        {
            foreach (var (item, rect) in _itemRects)
            {
                if (rect.Contains(p)) return item;
            }
            return null;
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
            if (radius <= 0) { p.AddRectangle(r); return p; }
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
                _clockTimer?.Stop();
                _clockTimer?.Dispose();
                _iconFont?.Dispose();
                _tooltip?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}