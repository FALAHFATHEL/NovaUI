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
    [DefaultEvent("SelectionChanged")]
    public class NovaMultiSelect : Control
    {
        private readonly List<string> _items = new List<string>();
        private readonly List<string> _selected = new List<string>();
        private int _radius = 10, _chipHeight = 26, _chipPadding = 8, _chipSpacing = 6, _maxDisplayChips = 3;
        private bool _open, _focused, _hover;
        private NovaMultiSelectPopup _popup;
        private Color _fillColor = Color.FromArgb(248, 250, 252);
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private Color _chipBackColor = Color.FromArgb(224, 231, 255);
        private Color _chipTextColor = Color.FromArgb(67, 56, 202);
        private Color _chipRemoveColor = Color.FromArgb(99, 102, 241);
        private Color _chipRemoveHoverColor = Color.FromArgb(239, 68, 68);
        private Color _placeholderColor = Color.FromArgb(148, 163, 184);
        private Color _arrowColor = Color.FromArgb(100, 116, 139);
        private string _placeholderText = "اختر...";
        private string _overflowFormat = "+{0} أخرى";
        private int _hoverChipIndex = -1;
        private Rectangle _hoverChipCloseRect = Rectangle.Empty;

        public event EventHandler SelectionChanged;
        public event EventHandler<string> ItemSelected;
        public event EventHandler<string> ItemDeselected;

        public NovaMultiSelect()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(340, 48);
            Font = new Font("Segoe UI", 9.5F);
            Cursor = Cursors.Hand;
            UpdateHeight();
        }

        [Browsable(false)] public List<string> Items => _items;
        [Browsable(false)] public List<string> SelectedItems => _selected;
        [Category("NovaMultiSelect"), DefaultValue("اختر...")] public string PlaceholderText { get => _placeholderText; set { _placeholderText = value ?? ""; Invalidate(); } }
        [Category("NovaMultiSelect"), DefaultValue("+{0} أخرى")] public string OverflowFormat { get => _overflowFormat; set { _overflowFormat = value ?? "+{0}"; Invalidate(); } }
        [Category("NovaMultiSelect"), DefaultValue(3)] public int MaxDisplayChips { get => _maxDisplayChips; set { _maxDisplayChips = Math.Max(1, value); UpdateHeight(); Invalidate(); } }
        [Category("NovaMultiSelect"), DefaultValue(10)] public int BorderRadius { get => _radius; set { _radius = Math.Max(0, value); Invalidate(); } }
        [Category("NovaMultiSelect")] public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }
        [Category("NovaMultiSelect")] public Color FocusFillColor { get => _focusFillColor; set { _focusFillColor = value; Invalidate(); } }
        [Category("NovaMultiSelect")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("NovaMultiSelect")] public Color FocusBorderColor { get => _focusBorderColor; set { _focusBorderColor = value; Invalidate(); } }
        [Category("NovaMultiSelect")] public Color ChipBackColor { get => _chipBackColor; set { _chipBackColor = value; Invalidate(); } }
        [Category("NovaMultiSelect")] public Color ChipTextColor { get => _chipTextColor; set { _chipTextColor = value; Invalidate(); } }

        public void AddItem(string item) { if (string.IsNullOrWhiteSpace(item) || _items.Contains(item)) return; _items.Add(item); }
        public void AddItems(IEnumerable<string> items) { if (items == null) return; foreach (var i in items) AddItem(i); }
        public void Select(string item)
        {
            if (!_items.Contains(item)) _items.Add(item);
            if (_selected.Contains(item)) return;
            _selected.Add(item); UpdateHeight(); Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty); ItemSelected?.Invoke(this, item);
        }
        public void Deselect(string item) { if (_selected.Remove(item)) { UpdateHeight(); Invalidate(); SelectionChanged?.Invoke(this, EventArgs.Empty); ItemDeselected?.Invoke(this, item); } }
        public void Toggle(string item) { if (_selected.Contains(item)) Deselect(item); else Select(item); }
        public void ClearSelection() { if (_selected.Count == 0) return; _selected.Clear(); UpdateHeight(); Invalidate(); SelectionChanged?.Invoke(this, EventArgs.Empty); }
        public void SelectAll() { _selected.Clear(); _selected.AddRange(_items); UpdateHeight(); Invalidate(); SelectionChanged?.Invoke(this, EventArgs.Empty); }

        private void UpdateHeight()
        {
            if (_selected.Count == 0) { Height = 48; return; }
            int rows = GetChipRows();
            Height = Math.Max(48, rows * (_chipHeight + _chipSpacing) + 20);
        }

        private int GetChipRows()
        {
            int innerW = Width - 60, x = 0, rows = 1;
            int displayCount = Math.Min(_selected.Count, _maxDisplayChips);
            for (int i = 0; i < displayCount; i++)
            {
                int cw = MeasureChipWidth(_selected[i]);
                if (x + cw > innerW) { rows++; x = 0; }
                x += cw + _chipSpacing;
            }
            if (_selected.Count > _maxDisplayChips)
            {
                int ow = MeasureChipWidth(string.Format(_overflowFormat, _selected.Count - _maxDisplayChips));
                if (x + ow > innerW) rows++;
            }
            return rows;
        }

        private int MeasureChipWidth(string text)
        {
            var size = TextRenderer.MeasureText(text, Font);
            return size.Width + _chipPadding * 2 + 22;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = _open || _focused ? _focusFillColor : _fillColor;
            Color border = _open ? _focusBorderColor : _hover ? _focusBorderColor : _focused ? _focusBorderColor : _borderColor;
            using (var path = Round(new Rectangle(0, 0, Width - 1, Height - 1), _radius))
            {
                using (var b = new SolidBrush(fill)) g.FillPath(b, path);
                using (var p = new Pen(border, 1.2f)) g.DrawPath(p, path);
            }
            if (_selected.Count > 0) DrawChips(g); else DrawPlaceholder(g);
            DrawArrow(g);
        }

        private void DrawChips(Graphics g)
        {
            int x = 10, y = 10, innerW = Width - 60;
            int displayCount = Math.Min(_selected.Count, _maxDisplayChips);
            for (int i = 0; i < displayCount; i++)
            {
                int cw = MeasureChipWidth(_selected[i]);
                if (x + cw > innerW) { x = 10; y += _chipHeight + _chipSpacing; }
                DrawChip(g, new Rectangle(x, y, cw, _chipHeight), _selected[i], i);
                x += cw + _chipSpacing;
            }
            if (_selected.Count > _maxDisplayChips)
            {
                string ot = string.Format(_overflowFormat, _selected.Count - _maxDisplayChips);
                int ow = MeasureChipWidth(ot);
                if (x + ow > innerW) { x = 10; y += _chipHeight + _chipSpacing; }
                var rect = new Rectangle(x, y, ow, _chipHeight);
                using (var path = Round(rect, _chipHeight / 2))
                using (var b = new SolidBrush(Color.FromArgb(226, 232, 240))) g.FillPath(b, path);
                using (var b = new SolidBrush(Color.FromArgb(71, 85, 105)))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(ot, Font, b, rect, sf);
            }
        }

        private void DrawChip(Graphics g, Rectangle rect, string text, int index)
        {
            bool isHover = index == _hoverChipIndex;
            using (var path = Round(rect, _chipHeight / 2))
            using (var b = new SolidBrush(_chipBackColor)) g.FillPath(b, path);
            var tr = new Rectangle(rect.X + _chipPadding, rect.Y, rect.Width - _chipPadding - 22, rect.Height);
            using (var b = new SolidBrush(_chipTextColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                g.DrawString(text, Font, b, tr, sf);
            var cr = new Rectangle(rect.Right - 22, rect.Y + (rect.Height - 18) / 2, 18, 18);
            bool ch = isHover && _hoverChipCloseRect.Contains(PointToClient(Cursor.Position));
            Color cc = ch ? _chipRemoveHoverColor : _chipRemoveColor;
            using (var pen = new Pen(cc, 1.6f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                int cx = cr.X + cr.Width / 2, cy = cr.Y + cr.Height / 2, s = 3;
                g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
                g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
            }
            if (isHover) _hoverChipCloseRect = cr;
        }

        private void DrawPlaceholder(Graphics g)
        {
            using (var b = new SolidBrush(_placeholderColor))
            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                g.DrawString(_placeholderText, Font, b, new RectangleF(14, 0, Width - 60, Height), sf);
        }

        private void DrawArrow(Graphics g)
        {
            float cx = Width - 20, cy = Height / 2f;
            using (var pen = new Pen(_arrowColor, 2f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                if (_open) g.DrawLines(pen, new[] { new PointF(cx - 5, cy + 3), new PointF(cx, cy - 3), new PointF(cx + 5, cy + 3) });
                else g.DrawLines(pen, new[] { new PointF(cx - 5, cy - 3), new PointF(cx, cy + 3), new PointF(cx + 5, cy - 3) });
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; if (_hoverChipIndex != -1) { _hoverChipIndex = -1; _hoverChipCloseRect = Rectangle.Empty; Invalidate(); } base.OnMouseLeave(e); }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int newIdx = -1;
            Rectangle ncr = Rectangle.Empty;
            int x = 10, y = 10, innerW = Width - 60;
            int displayCount = Math.Min(_selected.Count, _maxDisplayChips);
            for (int i = 0; i < displayCount; i++)
            {
                int cw = MeasureChipWidth(_selected[i]);
                if (x + cw > innerW) { x = 10; y += _chipHeight + _chipSpacing; }
                var rect = new Rectangle(x, y, cw, _chipHeight);
                if (rect.Contains(e.Location)) { newIdx = i; ncr = new Rectangle(rect.Right - 22, rect.Y + (rect.Height - 18) / 2, 18, 18); break; }
                x += cw + _chipSpacing;
            }
            if (newIdx != _hoverChipIndex) { _hoverChipIndex = newIdx; _hoverChipCloseRect = ncr; Invalidate(); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            if (_hoverChipIndex >= 0 && _hoverChipCloseRect.Contains(e.Location)) { Deselect(_selected[_hoverChipIndex]); return; }
            TogglePopup();
        }

        private void TogglePopup() { if (_open) ClosePopup(); else OpenPopup(); }

        private void OpenPopup()
        {
            _open = true; _focused = true; Invalidate();
            _popup = new NovaMultiSelectPopup(_items, _selected);
            _popup.ItemToggled += item => Toggle(item);
            _popup.ClearRequested += () => ClearSelection();
            _popup.SelectAllRequested += () => SelectAll();
            _popup.Closed += (s, e) => ClosePopup();
            var sp = PointToScreen(new Point(0, Height + 4));
            _popup.Location = sp; _popup.Width = Width; _popup.Show(); _popup.BringToFront();
        }

        private void ClosePopup()
        {
            _open = false; _focused = false;
            if (_popup != null && !_popup.IsDisposed) { _popup.Close(); _popup = null; }
            Invalidate();
        }

        protected override void OnHandleDestroyed(EventArgs e) { ClosePopup(); base.OnHandleDestroyed(e); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); UpdateHeight(); }

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

    internal class NovaMultiSelectPopup : Form
    {
        private readonly List<string> _items;
        private readonly List<string> _selected;
        private List<string> _filtered = new List<string>();
        private string _searchText = "";
        private int _hoverIndex = -1;
        private bool _selectAllHover, _clearHover;
        public event Action<string> ItemToggled;
        public event Action ClearRequested;
        public event Action SelectAllRequested;
        public event EventHandler Closed;
        private const int SearchH = 44, ActionsH = 40, ItemH = 36, MaxItems = 8, Radius = 10;
        private TextBox _searchBox;
        private Rectangle _selectAllRect, _clearRect;

        public NovaMultiSelectPopup(List<string> items, List<string> selected)
        {
            _items = items; _selected = selected;
            FormBorderStyle = FormBorderStyle.None; StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false; DoubleBuffered = true; BackColor = Color.White; TopMost = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            _searchBox = new TextBox { BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10F), Location = new Point(38, 13), Width = 200 };
            _searchBox.TextChanged += (s, e) => { _searchText = _searchBox.Text.Trim(); ApplyFilter(); Invalidate(); };
            Controls.Add(_searchBox);
            ApplyFilter();
            MouseMove += OnPopupMouseMove;
            MouseLeave += (s, e) => { _hoverIndex = -1; Invalidate(); };
            MouseClick += OnPopupMouseClick;
            Deactivate += (s, e) => { Hide(); Closed?.Invoke(this, EventArgs.Empty); };
            UpdateSize();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrEmpty(_searchText)) _filtered = new List<string>(_items);
            else _filtered = _items.Where(i => i.ToLowerInvariant().Contains(_searchText.ToLowerInvariant())).ToList();
            UpdateSize();
        }

        private void UpdateSize()
        {
            int itemsH = Math.Min(_filtered.Count, MaxItems) * ItemH;
            Height = SearchH + itemsH + ActionsH + 12;
            if (_filtered.Count == 0) Height = SearchH + 60 + ActionsH + 12;
        }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Region = new Region(Gfx.Round(new Rectangle(0, 0, Width, Height), Radius)); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            using (var f = new Font("Segoe MDL2 Assets", 11F))
            using (var b = new SolidBrush(Color.FromArgb(148, 163, 184)))
                g.DrawString("\uE721", f, b, 14, 14);
            using (var p = new Pen(Color.FromArgb(241, 245, 249), 1)) g.DrawLine(p, 0, SearchH, Width, SearchH);

            int y = SearchH + 6;
            if (_filtered.Count == 0)
            {
                using (var f = new Font("Segoe UI", 9.5F))
                using (var b = new SolidBrush(Color.FromArgb(148, 163, 184)))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("لا توجد نتائج", f, b, new RectangleF(0, y, Width, 48), sf);
            }
            else
            {
                for (int i = 0; i < _filtered.Count && i < MaxItems; i++)
                {
                    var item = _filtered[i];
                    var rect = new Rectangle(6, y, Width - 12, ItemH - 2);
                    bool sel = _selected.Contains(item);
                    bool hover = i == _hoverIndex;
                    if (hover)
                        using (var path = Gfx.Round(rect, 6))
                        using (var b = new SolidBrush(Color.FromArgb(248, 250, 252))) g.FillPath(b, path);
                    var cbRect = new Rectangle(rect.X + 10, rect.Y + (rect.Height - 20) / 2, 20, 20);
                    using (var path = Gfx.Round(cbRect, 5))
                    {
                        if (sel) { using (var b = new SolidBrush(Color.FromArgb(99, 102, 241))) g.FillPath(b, path); }
                        else { using (var b = new SolidBrush(Color.White)) g.FillPath(b, path); using (var p = new Pen(Color.FromArgb(203, 213, 225), 1.5f)) g.DrawPath(p, path); }
                    }
                    if (sel)
                    {
                        using (var pen = new Pen(Color.White, 2f))
                        {
                            pen.StartCap = pen.EndCap = LineCap.Round;
                            int cx = cbRect.X + cbRect.Width / 2, cy = cbRect.Y + cbRect.Height / 2;
                            g.DrawLines(pen, new[] { new PointF(cx - 4, cy), new PointF(cx - 1, cy + 3), new PointF(cx + 4, cy - 3) });
                        }
                    }
                    using (var b = new SolidBrush(Color.FromArgb(15, 23, 42)))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                        g.DrawString(item, new Font("Segoe UI", 10F), b, new RectangleF(cbRect.Right + 10, rect.Y, rect.Width - cbRect.Right - 20, rect.Height), sf);
                    y += ItemH;
                }
            }

            int actionsY = Height - ActionsH - 6;
            using (var p = new Pen(Color.FromArgb(241, 245, 249), 1)) g.DrawLine(p, 0, actionsY, Width, actionsY);

            var saSize = TextRenderer.MeasureText("تحديد الكل", new Font("Segoe UI", 9F, FontStyle.Bold));
            _selectAllRect = new Rectangle(10, actionsY + 8, saSize.Width + 20, 24);
            var cSize = TextRenderer.MeasureText("مسح", new Font("Segoe UI", 9F, FontStyle.Bold));
            _clearRect = new Rectangle(_selectAllRect.Right + 8, actionsY + 8, cSize.Width + 20, 24);

            using (var path = Gfx.Round(_selectAllRect, 6))
            using (var b = new SolidBrush(_selectAllHover ? Color.FromArgb(224, 231, 255) : Color.FromArgb(238, 242, 255))) g.FillPath(b, path);
            using (var b = new SolidBrush(Color.FromArgb(67, 56, 202)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("تحديد الكل", new Font("Segoe UI", 9F, FontStyle.Bold), b, _selectAllRect, sf);

            using (var path = Gfx.Round(_clearRect, 6))
            using (var b = new SolidBrush(_clearHover ? Color.FromArgb(254, 226, 226) : Color.FromArgb(254, 242, 242))) g.FillPath(b, path);
            using (var b = new SolidBrush(Color.FromArgb(220, 38, 38)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("مسح", new Font("Segoe UI", 9F, FontStyle.Bold), b, _clearRect, sf);

            using (var p = new Pen(Color.FromArgb(226, 232, 240), 1))
            using (var path = Gfx.Round(new Rectangle(0, 0, Width - 1, Height - 1), Radius)) g.DrawPath(p, path);
        }

        private void OnPopupMouseMove(object sender, MouseEventArgs e)
        {
            int idx = -1;
            int itemsY = SearchH + 6;
            if (e.Y >= itemsY) { idx = (e.Y - itemsY) / ItemH; if (idx < 0 || idx >= _filtered.Count || idx >= MaxItems) idx = -1; }
            bool saH = _selectAllRect.Contains(e.Location);
            bool clH = _clearRect.Contains(e.Location);
            if (idx != _hoverIndex || saH != _selectAllHover || clH != _clearHover)
            { _hoverIndex = idx; _selectAllHover = saH; _clearHover = clH; Cursor = (idx >= 0 || saH || clH) ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }

        private void OnPopupMouseClick(object sender, MouseEventArgs e)
        {
            if (_selectAllRect.Contains(e.Location)) { SelectAllRequested?.Invoke(); Invalidate(); return; }
            if (_clearRect.Contains(e.Location)) { ClearRequested?.Invoke(); Invalidate(); return; }
            int itemsY = SearchH + 6;
            if (e.Y >= itemsY)
            {
                int idx = (e.Y - itemsY) / ItemH;
                if (idx >= 0 && idx < _filtered.Count && idx < MaxItems) { ItemToggled?.Invoke(_filtered[idx]); Invalidate(); }
            }
        }

        protected override CreateParams CreateParams
        {
            get { const int CS_DROPSHADOW = 0x20000; var cp = base.CreateParams; cp.ClassStyle |= CS_DROPSHADOW; return cp; }
        }
    }
}