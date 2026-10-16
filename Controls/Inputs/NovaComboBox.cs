using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NovaUI
{
    [ToolboxItem(true)]
    [DefaultEvent("SelectedIndexChanged")]
    public class NovaComboBox : Control
    {
        private readonly List<object> _items = new List<object>();
        private int _selectedIndex = -1;
        private bool _hover, _open;
        private int _borderRadius = 10;
        private int _itemHeight = 38;
        private int _maxDropDownHeight = 260;
        private Color _fillColor = Color.FromArgb(248, 250, 252);
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private Color _textColor = Color.FromArgb(15, 23, 42);
        private Color _placeholderColor = Color.FromArgb(148, 163, 184);
        private Color _arrowColor = Color.FromArgb(100, 116, 139);
        private string _placeholderText = "اختر...";
        private NovaComboBoxDropDown _dropdown;

        public event EventHandler SelectedIndexChanged;

        public NovaComboBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(280, 46);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 11F);
        }

        [Browsable(false)] public List<object> Items => _items;
        [Browsable(false)] public object SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

        [Category("NovaComboBox"), DefaultValue(-1)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                int v = (value >= -1 && value < _items.Count) ? value : -1;
                if (_selectedIndex == v) return;
                _selectedIndex = v; Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("NovaComboBox"), DefaultValue(10)] public int BorderRadius { get => _borderRadius; set { _borderRadius = value; Invalidate(); } }
        [Category("NovaComboBox"), DefaultValue(38)] public int ItemHeight { get => _itemHeight; set { _itemHeight = Math.Max(24, value); } }
        [Category("NovaComboBox"), DefaultValue(260)] public int MaxDropDownHeight { get => _maxDropDownHeight; set { _maxDropDownHeight = Math.Max(80, value); } }
        [Category("NovaComboBox"), DefaultValue("اختر...")] public string PlaceholderText { get => _placeholderText; set { _placeholderText = value ?? ""; Invalidate(); } }
        [Category("NovaComboBox")] public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        [Category("NovaComboBox")] public Color FocusFillColor { get => _focusFillColor; set { _focusFillColor = value; Invalidate(); } }
        [Category("NovaComboBox")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("NovaComboBox")] public Color FocusBorderColor { get => _focusBorderColor; set { _focusBorderColor = value; Invalidate(); } }
        [Category("NovaComboBox")] public Color TextColor { get => _textColor; set { _textColor = value; Invalidate(); } }
        [Category("NovaComboBox")] public Color PlaceholderColor { get => _placeholderColor; set { _placeholderColor = value; Invalidate(); } }
        [Category("NovaComboBox")] public Color ArrowColor { get => _arrowColor; set { _arrowColor = value; Invalidate(); } }

        public void AddItem(object item) { _items.Add(item); Invalidate(); }
        public void Clear() { _items.Clear(); _selectedIndex = -1; Invalidate(); }

        private void DrawParentBackground(Graphics g)
        {
            if (Parent == null) return;
            var state = g.Save();
            g.TranslateTransform(-this.Left, -this.Top);
            using (var pe = new PaintEventArgs(g, Parent.ClientRectangle))
            {
                InvokePaintBackground(Parent, pe);
                InvokePaint(Parent, pe);
            }
            g.Restore(state);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            DrawParentBackground(g);

            if (Width <= 2 || Height <= 2) return;

            Color fill = (_open || _hover) ? _focusFillColor : _fillColor;
            Color border = _open ? _focusBorderColor : _hover ? _focusBorderColor : _borderColor;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (var path = Round(rect, _borderRadius))
            {
                using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                using (var p = new Pen(border, 1.5f)) g.DrawPath(p, path);
            }

            bool hasSel = _selectedIndex >= 0 && _selectedIndex < _items.Count;
            string text = hasSel ? _items[_selectedIndex]?.ToString() : _placeholderText;
            Color textColor = hasSel ? _textColor : _placeholderColor;

            var textRect = new Rectangle(14, 0, Width - 46, Height);
            using (var b = new SolidBrush(textColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.DirectionRightToLeft, Trimming = StringTrimming.EllipsisCharacter })
                g.DrawString(text ?? "", Font, b, textRect, sf);

            float cx = Width - 20, cy = Height / 2f;
            using (var pen = new Pen(_arrowColor, 2f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                if (_open) g.DrawLines(pen, new[] { new PointF(cx - 5, cy + 3), new PointF(cx, cy - 3), new PointF(cx + 5, cy + 3) });
                else g.DrawLines(pen, new[] { new PointF(cx - 5, cy - 3), new PointF(cx, cy + 3), new PointF(cx + 5, cy - 3) });
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left && !_open) ShowDropDown(); base.OnMouseDown(e); }

        private void ShowDropDown()
        {
            if (_items.Count == 0) return;
            _open = true; Invalidate();
            _dropdown = new NovaComboBoxDropDown(this, _items, _selectedIndex, _itemHeight, _maxDropDownHeight, _borderRadius);
            _dropdown.ItemSelected += idx => { SelectedIndex = idx; CloseDropDown(); };
            _dropdown.DropDownClosed += (s, e) => CloseDropDown();
            var sp = PointToScreen(new Point(0, Height));
            _dropdown.Location = sp; _dropdown.Width = Width; _dropdown.Show();
        }

        private void CloseDropDown()
        {
            if (_dropdown != null)
            {
                var temp = _dropdown;
                _dropdown = null;
                _open = false;
                Invalidate();
                if (!temp.IsDisposed)
                {
                    temp.Close();
                }
            }
        }

        protected override void OnHandleDestroyed(EventArgs e) { CloseDropDown(); base.OnHandleDestroyed(e); }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }

    internal class NovaComboBoxDropDown : Form
    {
        private readonly List<object> _items;
        private readonly int _itemHeight;
        private readonly Color _backColor = Color.White;
        private readonly Color _hoverColor = Color.FromArgb(238, 242, 255);
        private readonly Color _selectedColor = Color.FromArgb(224, 231, 255);
        private readonly Font _itemFont = new Font("Segoe UI", 10F);
        private readonly Color _textColor = Color.FromArgb(15, 23, 42);
        private readonly int _radius;
        private int _hoverIndex = -1, _selectedIndex;
        private bool _isClosing = false;

        public event Action<int> ItemSelected;
        public event EventHandler DropDownClosed;

        public NovaComboBoxDropDown(NovaComboBox owner, List<object> items, int selected, int itemHeight, int maxHeight, int radius)
        {
            _items = items; _itemHeight = itemHeight; _selectedIndex = selected; _radius = radius;
            FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual; BackColor = Color.White;
            int total = items.Count * itemHeight + 8;
            Height = Math.Min(total, maxHeight);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            Deactivate += (s, e) => {
                if (!_isClosing)
                {
                    _isClosing = true;
                    Close();
                }
            };

            MouseMove += (s, e) => { int i = (e.Y - 4) / _itemHeight; if (i < 0 || i >= _items.Count) i = -1; if (i != _hoverIndex) { _hoverIndex = i; Invalidate(); } };
            MouseLeave += (s, e) => { _hoverIndex = -1; Invalidate(); };
            MouseClick += (s, e) => { int i = (e.Y - 4) / _itemHeight; if (i >= 0 && i < _items.Count) ItemSelected?.Invoke(i); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Round(rect, _radius))
            using (var b = new SolidBrush(_backColor)) g.FillPath(b, path);
            using (var p = new Pen(Color.FromArgb(226, 232, 240), 1))
            using (var path = Round(rect, _radius)) g.DrawPath(p, path);

            int y = 4;
            for (int i = 0; i < _items.Count; i++)
            {
                var ir = new Rectangle(4, y, Width - 8, _itemHeight);
                if (i == _selectedIndex) { using (var b = new SolidBrush(_selectedColor)) using (var path = Round(ir, 6)) g.FillPath(b, path); }
                else if (i == _hoverIndex) { using (var b = new SolidBrush(_hoverColor)) using (var path = Round(ir, 6)) g.FillPath(b, path); }
                var tr = new Rectangle(ir.X + 12, ir.Y, ir.Width - 24, ir.Height);
                using (var b = new SolidBrush(_textColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.DirectionRightToLeft, Trimming = StringTrimming.EllipsisCharacter })
                    g.DrawString(_items[i]?.ToString() ?? "", _itemFont, b, tr, sf);
                y += _itemHeight;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e) { base.OnFormClosed(e); DropDownClosed?.Invoke(this, EventArgs.Empty); }

        protected override CreateParams CreateParams
        {
            get { const int CS_DROPSHADOW = 0x20000; var cp = base.CreateParams; cp.ClassStyle |= CS_DROPSHADOW; return cp; }
        }

        private static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }
}
