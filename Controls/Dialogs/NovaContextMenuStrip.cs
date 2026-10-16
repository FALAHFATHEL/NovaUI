using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [Description("قائمة سياق احترافية بزوايا دائرية ناعمة، مرنة، وقابلة لإعادة الاستخدام بألوان مخصصة.")]
    public class NovaContextMenuStrip : ContextMenuStrip
    {
        #region Fileds

        private int _borderRadius = 10;
        private Color _fillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _itemHoverColor = Color.FromArgb(238, 242, 255);
        private Color _itemSelectedColor = Color.FromArgb(224, 231, 255);
        private Color _itemTextColor = Color.FromArgb(51, 65, 85);
        private Color _itemHoverTextColor = Color.FromArgb(67, 56, 202);
        private Color _separatorColor = Color.FromArgb(241, 245, 249);
        private Color _iconColor = Color.FromArgb(100, 116, 139);
        private int _itemHeight = 36;
        private int _itemPadding = 10;
        private int _itemRadius = 6;
        private bool _showIconMargin = false;

        #endregion

        public NovaContextMenuStrip()
        {
            Renderer = new FMenuRenderer(this);
            BackColor = _fillColor;
            Font = new Font("Segoe UI", 10F);
            ShowImageMargin = _showIconMargin;
            Padding = new Padding(6);
            AutoSize = true;
            DropShadowEnabled = true;

            HandleCreated += (s, e) => UpdateRenderer();
        }

        #region Properties

        [Category("NovaContextMenuStrip"), DefaultValue(10)]
        [Description("استدارة زوايا القائمة.")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; BackColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color ItemHoverColor
        {
            get => _itemHoverColor;
            set { _itemHoverColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color ItemSelectedColor
        {
            get => _itemSelectedColor;
            set { _itemSelectedColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color ItemTextColor
        {
            get => _itemTextColor;
            set { _itemTextColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color ItemHoverTextColor
        {
            get => _itemHoverTextColor;
            set { _itemHoverTextColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color SeparatorColor
        {
            get => _separatorColor;
            set { _separatorColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip")]
        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip"), DefaultValue(36)]
        public int ItemHeight
        {
            get => _itemHeight;
            set { _itemHeight = Math.Max(24, value); UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip"), DefaultValue(10)]
        public int ItemPadding
        {
            get => _itemPadding;
            set { _itemPadding = Math.Max(2, value); UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip"), DefaultValue(6)]
        public int ItemRadius
        {
            get => _itemRadius;
            set { _itemRadius = Math.Max(0, value); UpdateRenderer(); }
        }

        [Category("NovaContextMenuStrip"), DefaultValue(false)]
        [Description("عرض هامش جانبي للأيقونات.")]
        public bool ShowIconMargin
        {
            get => _showIconMargin;
            set { _showIconMargin = value; ShowImageMargin = value; UpdateRenderer(); }
        }

        #endregion

        private void UpdateRenderer()
        {
            if (Renderer is FMenuRenderer r) r.Refresh();
            Invalidate();
        }

        #region Renderer

        private class FMenuRenderer : ToolStripProfessionalRenderer
        {
            private readonly NovaContextMenuStrip _owner;

            public FMenuRenderer(NovaContextMenuStrip owner) : base(new FColorTable(owner))
            {
                _owner = owner;
                RoundedEdges = true;
            }

            public void Refresh() => _owner.Invalidate();

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                using (var path = Round(rect, _owner.BorderRadius))
                {
                    using (var b = new SolidBrush(_owner.FillColor))
                        g.FillPath(b, path);

                    if (_owner.BorderColor != Color.Transparent)
                    {
                        using (var p = new Pen(_owner.BorderColor, 1.2f))
                        {
                            p.Alignment = PenAlignment.Inset;
                            g.DrawPath(p, path);
                        }
                    }
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // منع الحدود الافتراضية ورسمها داخل الخلفية بدقة عالية
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var item = e.Item;
                if (!item.Enabled) return;

                var rect = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);

                if (item.Selected || item.Pressed)
                {
                    using (var path = Round(rect, _owner.ItemRadius))
                    using (var b = new SolidBrush(
                        item.Pressed ? _owner.ItemSelectedColor : _owner.ItemHoverColor))
                    {
                        g.FillPath(b, path);
                    }
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Selected || e.Item.Pressed
                    ? _owner.ItemHoverTextColor
                    : _owner.ItemTextColor;

                e.TextFont = _owner.Font;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = new Pen(_owner.SeparatorColor, 1))
                {
                    g.DrawLine(p, 12, e.Item.Height / 2,
                        e.Item.Width - 12, e.Item.Height / 2);
                }
            }

            protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
            {
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var rect = new Rectangle(
                    e.ImageRectangle.X,
                    e.ImageRectangle.Y,
                    e.ImageRectangle.Width,
                    e.ImageRectangle.Height);

                using (var f = new Font("Segoe MDL2 Assets", 11F))
                using (var b = new SolidBrush(_owner.ItemHoverTextColor))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString("\uE73E", f, b, rect, sf);
                }
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var color = e.Item.Selected || e.Item.Pressed
                    ? _owner.ItemHoverTextColor
                    : _owner.IconColor;

                var r = e.ArrowRectangle;
                using (var pen = new Pen(color, 1.6f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    float cx = r.X + r.Width / 2f;
                    float cy = r.Y + r.Height / 2f;

                    g.DrawLines(pen, new[]
                    {
                        new PointF(cx - 2, cy - 3),
                        new PointF(cx + 2, cy),
                        new PointF(cx - 2, cy + 3)
                    });
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
        }

        private class FColorTable : ProfessionalColorTable
        {
            private readonly NovaContextMenuStrip _owner;

            public FColorTable(NovaContextMenuStrip owner) { _owner = owner; }

            public override Color ToolStripDropDownBackground => _owner.FillColor;
            public override Color MenuBorder => Color.Transparent;
            public override Color MenuItemBorder => Color.Transparent;
            public override Color MenuItemSelected => _owner.ItemHoverColor;
            public override Color MenuItemSelectedGradientBegin => _owner.ItemHoverColor;
            public override Color MenuItemSelectedGradientEnd => _owner.ItemHoverColor;
            public override Color MenuItemPressedGradientBegin => _owner.ItemSelectedColor;
            public override Color MenuItemPressedGradientEnd => _owner.ItemSelectedColor;
            public override Color SeparatorDark => _owner.SeparatorColor;
            public override Color SeparatorLight => _owner.SeparatorColor;
            public override Color ImageMarginGradientBegin => Color.Transparent;
            public override Color ImageMarginGradientMiddle => Color.Transparent;
            public override Color ImageMarginGradientEnd => Color.Transparent;
        }

        #endregion

        #region Public API (Flexible & Reusable)

        public ToolStripMenuItem AddItem(string text, string iconGlyph = null, Action onClick = null)
        {
            var item = new ToolStripMenuItem(text);

            if (!string.IsNullOrEmpty(iconGlyph))
            {
                item.Text = iconGlyph + "   " + text;
            }

            if (onClick != null)
                item.Click += (s, e) => onClick();

            Items.Add(item);
            return item;
        }

        public ToolStripMenuItem AddCheckItem(string text, bool isChecked = false, Action<bool> onChecked = null)
        {
            var item = new ToolStripMenuItem(text)
            {
                CheckOnClick = true,
                Checked = isChecked
            };

            if (onChecked != null)
                item.CheckedChanged += (s, e) => onChecked(item.Checked);

            Items.Add(item);
            return item;
        }

        public void AddSeparator()
        {
            Items.Add(new ToolStripSeparator());
        }

        public void ClearItems()
        {
            Items.Clear();
        }

        public void ShowAt(Control control, int x, int y)
        {
            Show(control, new Point(x, y));
        }

        public void ShowAtCursor(Control control)
        {
            Show(control, control.PointToClient(Cursor.Position));
        }

        #endregion
    }
}

