using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NovaUI
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FFileItem
    {
        private string _filePath = "", _fileName = "";
        private long _fileSize;
        private DateTime _lastModified;

        public FFileItem() { }
        public FFileItem(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                var fi = new FileInfo(filePath);
                _filePath = filePath; _fileName = fi.Name; _fileSize = fi.Length; _lastModified = fi.LastWriteTime;
            }
        }

        [Category("FFileItem"), DefaultValue("")] public string FilePath { get => _filePath; set => _filePath = value ?? ""; }
        [Category("FFileItem"), DefaultValue("")] public string FileName { get => _fileName; set => _fileName = value ?? ""; }
        [Category("FFileItem"), DefaultValue(0L)] public long FileSize { get => _fileSize; set => _fileSize = value; }
        [Category("FFileItem")] public DateTime LastModified { get => _lastModified; set => _lastModified = value; }
        [Browsable(false)] public string Extension => string.IsNullOrEmpty(_fileName) ? "" : Path.GetExtension(_fileName).ToLowerInvariant();
        [Browsable(false)]
        public string FormattedSize
        {
            get
            {
                if (_fileSize < 1024) return _fileSize + " B";
                if (_fileSize < 1024 * 1024) return (_fileSize / 1024.0).ToString("N1") + " KB";
                if (_fileSize < 1024L * 1024 * 1024) return (_fileSize / (1024.0 * 1024)).ToString("N1") + " MB";
                return (_fileSize / (1024.0 * 1024 * 1024)).ToString("N2") + " GB";
            }
        }
        public override string ToString() => string.IsNullOrEmpty(_fileName) ? "(فارغ)" : _fileName;
    }

    [ToolboxItem(true)]
    [DefaultEvent("FilesChanged")]
    [Description("حقل اختيار ملفات احترافي مع دعم RTL/LTR كامل ومواضع أيقونات دقيقة.")]
    public class NovaFilePicker : UserControl
    {
        #region Fileds

        private readonly List<FFileItem> _files = new List<FFileItem>();

        private int _radius = 10;
        private int _chipHeight = 34;
        private int _chipSpacing = 8;
        private int _chipPadding = 12;
        private int _maxChipNameWidth = 180;

        private string _filter = "كل الملفات|*.*";
        private int _filterIndex = 1;
        private bool _multiselect = false;
        private string _buttonText = "اختر ملف";
        private string _placeholderText = "لم يتم اختيار أي ملف";
        private string _dragDropText = "اسحب الملفات هنا";

        private Color _fillColor = Color.FromArgb(248, 250, 252);
        private Color _focusFillColor = Color.White;
        private Color _borderColor = Color.FromArgb(226, 232, 240);
        private Color _focusBorderColor = Color.FromArgb(99, 102, 241);
        private Color _chipBackColor = Color.FromArgb(238, 242, 255);
        private Color _chipHoverBackColor = Color.FromArgb(224, 231, 255);
        private Color _chipTextColor = Color.FromArgb(67, 56, 202);
        private Color _chipIconColor = Color.FromArgb(99, 102, 241);
        private Color _chipSizeColor = Color.FromArgb(148, 163, 184);
        private Color _chipCloseColor = Color.FromArgb(148, 163, 184);
        private Color _chipCloseHoverColor = Color.FromArgb(239, 68, 68);
        private Color _placeholderColor = Color.FromArgb(148, 163, 184);
        private Color _dropHighlightColor = Color.FromArgb(99, 102, 241);
        private Color _addButtonColor = Color.FromArgb(99, 102, 241);
        private Color _addButtonBackColor = Color.FromArgb(30, 99, 102, 241);

        private bool _focused, _dragOver;
        private int _hoverChipIndex = -1;
        private Rectangle _hoverChipCloseRect = Rectangle.Empty;
        private Rectangle _addButtonRect = Rectangle.Empty;

        private Font _iconFont;
        private Font _iconFontSmall;
        private Font _smallFont;
        private Font _boldFont;

        #endregion

        #region Events

        public event EventHandler FilesChanged;
        public event EventHandler<FFileItem> FileAdded;
        public event EventHandler<FFileItem> FileRemoved;
        public event EventHandler<string> ValidationFailed;

        #endregion

        public NovaFilePicker()
        {
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);

            base.BackColor = Color.Transparent;
            Size = new Size(360, 56);
            Font = new Font("Segoe UI", 9.5F);
            Cursor = Cursors.Hand;
            AllowDrop = true;

            _iconFont = new Font("Segoe MDL2 Assets", 11F, FontStyle.Regular);
            _iconFontSmall = new Font("Segoe MDL2 Assets", 10F, FontStyle.Regular);
            _smallFont = new Font("Segoe UI", 8F, FontStyle.Regular);
            _boldFont = new Font("Segoe UI", 9F, FontStyle.Bold);

            DragEnter += (s, e) =>
            {
                if (!AllowDrop) return;
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    _dragOver = true;
                    Invalidate();
                    e.Effect = DragDropEffects.Copy;
                }
            };
            DragOver += (s, e) =>
            {
                if (!AllowDrop) return;
                e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                    ? DragDropEffects.Copy : DragDropEffects.None;
            };
            DragLeave += (s, e) => { _dragOver = false; Invalidate(); };
            DragDrop += (s, e) =>
            {
                _dragOver = false;
                if (!AllowDrop || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
                AddFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            };
        }

        #region Properties

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<FFileItem> Files => _files;

        [Category("NovaFilePicker"), DefaultValue("كل الملفات|*.*")]
        public string Filter { get => _filter; set => _filter = value ?? "كل الملفات|*.*"; }

        [Category("NovaFilePicker"), DefaultValue(1)]
        public int FilterIndex { get => _filterIndex; set => _filterIndex = Math.Max(1, value); }

        [Category("NovaFilePicker"), DefaultValue(false)]
        public bool Multiselect { get => _multiselect; set { _multiselect = value; UpdateHeight(); Invalidate(); } }

        [Category("NovaFilePicker"), DefaultValue("اختر ملف")]
        public string ButtonText { get => _buttonText; set { _buttonText = value ?? "اختر ملف"; Invalidate(); } }

        [Category("NovaFilePicker"), DefaultValue("لم يتم اختيار أي ملف")]
        public string PlaceholderText { get => _placeholderText; set { _placeholderText = value ?? ""; Invalidate(); } }

        [Category("NovaFilePicker"), DefaultValue("اسحب الملفات هنا")]
        public string DragDropText { get => _dragDropText; set { _dragDropText = value ?? ""; Invalidate(); } }

        [Category("NovaFilePicker"), DefaultValue(true)]
        public bool AllowFileDrop { get => AllowDrop; set => AllowDrop = value; }

        [Category("NovaFilePicker"), DefaultValue(10)]
        public int BorderRadius { get => _radius; set { _radius = Math.Max(0, value); Invalidate(); } }

        [Category("NovaFilePicker"), DefaultValue(0L)]
        public long MaxFileSize { get; set; } = 0L;

        [Category("NovaFilePicker"), DefaultValue(0)]
        public int MaxFiles { get; set; } = 0;

        [Category("NovaFilePicker"), DefaultValue(true)]
        public bool ShowFileSize { get; set; } = true;

        [Category("NovaFilePicker"), DefaultValue(true)]
        public bool ShowFileIcon { get; set; } = true;

        [Category("NovaFilePicker"), DefaultValue(true)]
        public bool ShowRemoveButton { get; set; } = true;

        [Category("NovaFilePicker"), DefaultValue(false)]
        public bool ReadOnly { get; set; } = false;

        [Category("NovaFilePicker")]
        public Color FillColor { get => _fillColor; set { _fillColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color FocusFillColor { get => _focusFillColor; set { _focusFillColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color FocusBorderColor { get => _focusBorderColor; set { _focusBorderColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color ChipBackColor { get => _chipBackColor; set { _chipBackColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color ChipHoverBackColor { get => _chipHoverBackColor; set { _chipHoverBackColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color ChipTextColor { get => _chipTextColor; set { _chipTextColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color ChipIconColor { get => _chipIconColor; set { _chipIconColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color PlaceholderColor { get => _placeholderColor; set { _placeholderColor = value; Invalidate(); } }

        [Category("NovaFilePicker")]
        public Color AddButtonColor { get => _addButtonColor; set { _addButtonColor = value; Invalidate(); } }

        [Browsable(false)]
        public FFileItem FirstFile => _files.Count > 0 ? _files[0] : null;

        [Browsable(false)]
        public string[] FilePaths => _files.Select(f => f.FilePath).ToArray();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = Color.Transparent;
        }

        #endregion

        #region Public API

        public void AddFile(string filePath)
        {
            if (ReadOnly || string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            if (MaxFileSize > 0)
            {
                long size = new FileInfo(filePath).Length;
                if (size > MaxFileSize)
                {
                    ValidationFailed?.Invoke(this, $"حجم الملف يتجاوز الحد ({MaxFileSize:N0} بايت)");
                    return;
                }
            }

            if (!_multiselect && _files.Count >= 1) _files.Clear();

            if (MaxFiles > 0 && _files.Count >= MaxFiles)
            {
                ValidationFailed?.Invoke(this, $"الحد الأقصى {MaxFiles} ملف");
                return;
            }

            if (_files.Any(f => string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase)))
                return;

            var item = new FFileItem(filePath);
            _files.Add(item);
            UpdateHeight();
            Invalidate();
            FileAdded?.Invoke(this, item);
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddFiles(string[] paths)
        {
            if (paths == null) return;
            foreach (var p in paths) AddFile(p);
        }

        public void RemoveFile(FFileItem item)
        {
            if (item == null || !_files.Remove(item)) return;
            UpdateHeight(); Invalidate();
            FileRemoved?.Invoke(this, item);
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveFileAt(int index)
        {
            if (index < 0 || index >= _files.Count) return;
            var item = _files[index];
            _files.RemoveAt(index);
            UpdateHeight(); Invalidate();
            FileRemoved?.Invoke(this, item);
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ClearFiles()
        {
            if (_files.Count == 0) return;
            _files.Clear();
            UpdateHeight(); Invalidate();
            FilesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Browse()
        {
            if (ReadOnly) return;
            using (var dlg = new OpenFileDialog
            {
                Filter = _filter,
                FilterIndex = _filterIndex,
                Multiselect = _multiselect
            })
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    AddFiles(dlg.FileNames);
        }

        #endregion

        #region Layout

        private void UpdateHeight()
        {
            if (_files.Count == 0)
            {
                Height = 56;
                return;
            }

            int rows = GetChipRows();
            int contentHeight = 20 + rows * (_chipHeight + _chipSpacing);
            Height = Math.Max(56, contentHeight);
        }

        private int GetChipRows()
        {
            int innerW = Width - 20;
            int used = 0;
            int rows = 1;

            for (int i = 0; i < _files.Count; i++)
            {
                int w = MeasureChipWidth(_files[i]);
                if (used + w > innerW)
                {
                    rows++;
                    used = 0;
                }
                used += w + _chipSpacing;
            }

            // زر الإضافة
            bool canAdd = !ReadOnly && (_multiselect || _files.Count == 0) && (MaxFiles <= 0 || _files.Count < MaxFiles);
            if (canAdd)
            {
                int addW = MeasureAddButtonWidth();
                if (used + addW > innerW) rows++;
            }

            return rows;
        }

        private int MeasureChipWidth(FFileItem f)
        {
            int w = _chipPadding * 2;

            if (ShowFileIcon) w += 22 + 6;
            w += Math.Min(_maxChipNameWidth, TextRenderer.MeasureText(f.FileName, Font).Width);
            if (ShowFileSize)
            {
                w += 8;
                w += TextRenderer.MeasureText(f.FormattedSize, _smallFont).Width;
            }
            if (ShowRemoveButton && !ReadOnly) w += 22;

            return Math.Max(60, w);
        }

        private int MeasureAddButtonWidth()
        {
            var textSize = TextRenderer.MeasureText(_buttonText, _boldFont);
            return 14 + 16 + 8 + textSize.Width + 14;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateHeight();
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

            Color fill = (_focused || _dragOver) ? _focusFillColor : _fillColor;
            Color border = _dragOver ? _dropHighlightColor
                         : _focused ? _focusBorderColor
                         : _borderColor;

            using (var path = Round(rect, _radius))
            {
                using (var b = new SolidBrush(fill))
                    g.FillPath(b, path);

                using (var p = new Pen(border, _dragOver ? 2f : 1.2f))
                {
                    if (_dragOver) p.DashStyle = DashStyle.Dash;
                    g.DrawPath(p, path);
                }
            }

            if (_dragOver)
            {
                TextRenderer.DrawText(g, _dragDropText, new Font("Segoe UI", 10F, FontStyle.Bold),
                    ClientRectangle, _dropHighlightColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            if (_files.Count == 0) DrawPlaceholder(g, rtl);
            else DrawChips(g, rtl);
        }

        private void DrawPlaceholder(Graphics g, bool rtl)
        {
            int iconSize = 20;
            int iconY = (Height - iconSize) / 2;
            int iconX = rtl ? Width - 20 - iconSize : 20;

            DrawIcon(g, "\uE7C3", new Rectangle(iconX, iconY, iconSize, iconSize),
                     _placeholderColor, 14F);

            int textLeft = rtl ? 56 : 48;
            int textRight = rtl ? Width - 48 : Width - 56;
            var textRect = new Rectangle(textLeft, 0, Math.Max(20, textRight - textLeft), Height);

            TextRenderer.DrawText(g, _placeholderText, Font, textRect, _placeholderColor,
                (rtl ? TextFormatFlags.Right : TextFormatFlags.Left) |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPadding);

            int addIconSize = 16;
            int addIconX = rtl ? 20 : Width - 20 - addIconSize;
            int addIconY = (Height - addIconSize) / 2;

            DrawIcon(g, "\uE710", new Rectangle(addIconX, addIconY, addIconSize, addIconSize),
                     _addButtonColor, 11F);
        }

        private void DrawChips(Graphics g, bool rtl)
        {
            int padding = 10;
            int innerW = Width - padding * 2;
            int y = 10;

            int used = 0;
            int rowY = y;

            _addButtonRect = Rectangle.Empty;

            for (int i = 0; i < _files.Count; i++)
            {
                var file = _files[i];
                int w = MeasureChipWidth(file);

                if (used + w > innerW && used > 0)
                {
                    used = 0;
                    rowY += _chipHeight + _chipSpacing;
                }

                int actualX = rtl ? (Width - padding - used - w) : (padding + used);

                DrawChip(g, new Rectangle(actualX, rowY, w, _chipHeight), file, i, rtl);

                used += w + _chipSpacing;
            }

            bool canAdd = !ReadOnly && (_multiselect || _files.Count == 0) && (MaxFiles <= 0 || _files.Count < MaxFiles);
            if (canAdd)
            {
                int addW = MeasureAddButtonWidth();
                if (used + addW > innerW && used > 0)
                {
                    used = 0;
                    rowY += _chipHeight + _chipSpacing;
                }

                int actualX = rtl ? (Width - padding - used - addW) : (padding + used);
                _addButtonRect = new Rectangle(actualX, rowY, addW, _chipHeight);

                DrawAddButton(g, _addButtonRect, rtl);
            }
        }

        private void DrawChip(Graphics g, Rectangle rect, FFileItem file, int index, bool rtl)
        {
            bool isHover = index == _hoverChipIndex;

            Color chipBg = isHover ? _chipHoverBackColor : _chipBackColor;
            using (var path = Round(rect, _chipHeight / 2))
            using (var b = new SolidBrush(chipBg))
                g.FillPath(b, path);

            int iconSize = 16;
            int closeSize = 18;
            int gap = 6;

            Rectangle iconRect = Rectangle.Empty;
            Rectangle nameRect = Rectangle.Empty;
            Rectangle sizeRect = Rectangle.Empty;
            Rectangle closeRect = Rectangle.Empty;

            if (rtl)
            {
                int rightEdge = rect.Right - _chipPadding;

                if (ShowFileIcon)
                {
                    iconRect = new Rectangle(rightEdge - iconSize,
                        rect.Y + (rect.Height - iconSize) / 2, iconSize, iconSize);
                    rightEdge = iconRect.Left - gap;
                }

                int nameW = Math.Min(_maxChipNameWidth, TextRenderer.MeasureText(file.FileName, Font).Width);
                nameRect = new Rectangle(rightEdge - nameW, rect.Y, nameW, rect.Height);
                rightEdge = nameRect.Left - 8;

                if (ShowFileSize)
                {
                    int szW = TextRenderer.MeasureText(file.FormattedSize, _smallFont).Width;
                    sizeRect = new Rectangle(rightEdge - szW, rect.Y, szW, rect.Height);
                    rightEdge = sizeRect.Left - 4;
                }

                if (ShowRemoveButton && !ReadOnly)
                {
                    closeRect = new Rectangle(rect.Left + 4,
                        rect.Y + (rect.Height - closeSize) / 2, closeSize, closeSize);
                }
            }
            else
            {
                int leftEdge = rect.X + _chipPadding;

                if (ShowFileIcon)
                {
                    iconRect = new Rectangle(leftEdge,
                        rect.Y + (rect.Height - iconSize) / 2, iconSize, iconSize);
                    leftEdge = iconRect.Right + gap;
                }

                int nameW = Math.Min(_maxChipNameWidth, TextRenderer.MeasureText(file.FileName, Font).Width);
                nameRect = new Rectangle(leftEdge, rect.Y, nameW, rect.Height);
                leftEdge = nameRect.Right + 8;

                if (ShowFileSize)
                {
                    int szW = TextRenderer.MeasureText(file.FormattedSize, _smallFont).Width;
                    sizeRect = new Rectangle(leftEdge, rect.Y, szW, rect.Height);
                    leftEdge = sizeRect.Right + 4;
                }

                if (ShowRemoveButton && !ReadOnly)
                {
                    closeRect = new Rectangle(rect.Right - closeSize - 4,
                        rect.Y + (rect.Height - closeSize) / 2, closeSize, closeSize);
                }
            }


            if (!iconRect.IsEmpty)
            {
                DrawIcon(g, GetFileIcon(file.Extension), iconRect, _chipIconColor, 11F);
            }

            if (!nameRect.IsEmpty)
            {
                TextRenderer.DrawText(g, file.FileName, Font, nameRect, _chipTextColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            }

            if (!sizeRect.IsEmpty)
            {
                TextRenderer.DrawText(g, file.FormattedSize, _smallFont, sizeRect, _chipSizeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            if (!closeRect.IsEmpty)
            {
                bool ch = isHover && _hoverChipCloseRect.Contains(PointToClient(Cursor.Position));
                Color color = ch ? _chipCloseHoverColor : _chipCloseColor;

                using (var pen = new Pen(color, 1.6f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    int cx = closeRect.X + closeRect.Width / 2;
                    int cy = closeRect.Y + closeRect.Height / 2;
                    int s = 3;

                    g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
                    g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
                }
            }
        }

        private void DrawAddButton(Graphics g, Rectangle rect, bool rtl)
        {
            using (var path = Round(rect, _chipHeight / 2))
            using (var b = new SolidBrush(_addButtonBackColor))
                g.FillPath(b, path);

            using (var path = Round(rect, _chipHeight / 2))
            using (var p = new Pen(_addButtonColor, 1.2f) { DashStyle = DashStyle.Dash })
                g.DrawPath(p, path);

            int iconSize = 16;
            int gap = 6;
            var textSize = TextRenderer.MeasureText(_buttonText, _boldFont);

            int contentW = iconSize + gap + textSize.Width;
            int contentX = rect.X + (rect.Width - contentW) / 2;

            var iconRect = new Rectangle(contentX,
                rect.Y + (rect.Height - iconSize) / 2, iconSize, iconSize);

            DrawIcon(g, "\uE710", iconRect, _addButtonColor, 11F);

            var textRect = new Rectangle(
                iconRect.Right + gap, rect.Y,
                rect.Right - (iconRect.Right + gap) - 6, rect.Height);

            TextRenderer.DrawText(g, _buttonText, _boldFont, textRect, _addButtonColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        private void DrawIcon(Graphics g, string glyph, Rectangle rect, Color color, float fontSize)
        {
            if (string.IsNullOrEmpty(glyph) || rect.Width <= 0 || rect.Height <= 0) return;

            var size = TextRenderer.MeasureText(glyph, _iconFont, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding);

            using (var f = new Font("Segoe MDL2 Assets", fontSize))
            {
                var measured = TextRenderer.MeasureText(glyph, f, new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.NoPadding);

                int x = rect.X + (rect.Width - measured.Width) / 2;
                int y = rect.Y + (rect.Height - measured.Height) / 2;

                TextRenderer.DrawText(g, glyph, f, new Point(x, y), color,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }

        #endregion

        #region Mouse Handling

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool rtl = RightToLeft == RightToLeft.Yes;
            int newIdx = -1;
            Rectangle newCloseRect = Rectangle.Empty;

            int padding = 10;
            int innerW = Width - padding * 2;
            int y = 10;
            int used = 0;
            int rowY = y;

            for (int i = 0; i < _files.Count; i++)
            {
                int w = MeasureChipWidth(_files[i]);

                if (used + w > innerW && used > 0)
                {
                    used = 0;
                    rowY += _chipHeight + _chipSpacing;
                }

                int actualX = rtl ? (Width - padding - used - w) : (padding + used);
                var rect = new Rectangle(actualX, rowY, w, _chipHeight);

                if (rect.Contains(e.Location))
                {
                    newIdx = i;
                    int closeSize = 18;
                    if (rtl)
                    {
                        newCloseRect = new Rectangle(rect.Left + 4,
                            rect.Y + (rect.Height - closeSize) / 2, closeSize, closeSize);
                    }
                    else
                    {
                        newCloseRect = new Rectangle(rect.Right - closeSize - 4,
                            rect.Y + (rect.Height - closeSize) / 2, closeSize, closeSize);
                    }
                    break;
                }

                used += w + _chipSpacing;
            }

            if (newIdx != _hoverChipIndex)
            {
                _hoverChipIndex = newIdx;
                _hoverChipCloseRect = newCloseRect;
                Invalidate();
            }
            else if (newIdx >= 0)
            {
                _hoverChipCloseRect = newCloseRect;
            }

            Cursor = newIdx >= 0 ? Cursors.Hand : Cursors.Hand;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverChipIndex = -1;
            _hoverChipCloseRect = Rectangle.Empty;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || ReadOnly) return;

            if (_hoverChipIndex >= 0 && _hoverChipCloseRect.Contains(e.Location))
            {
                RemoveFileAt(_hoverChipIndex);
                return;
            }

            if (!_addButtonRect.IsEmpty && _addButtonRect.Contains(e.Location))
            {
                Browse();
                return;
            }

            Browse();
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            _focused = true;
            Invalidate();
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            _focused = false;
            Invalidate();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            Invalidate();
        }

        #endregion

        #region Helpers

        private static string GetFileIcon(string ext)
        {
            switch (ext.ToLowerInvariant())
            {
                case ".pdf": return "\uEA90";
                case ".doc":
                case ".docx": return "\uE8A5";
                case ".xls":
                case ".xlsx": return "\uE9D9";
                case ".ppt":
                case ".pptx": return "\uE7F4";
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".bmp":
                case ".webp": return "\uEB9F";
                case ".mp3":
                case ".wav": return "\uE8D6";
                case ".mp4":
                case ".avi":
                case ".mov": return "\uE714";
                case ".zip":
                case ".rar":
                case ".7z": return "\uF012";
                case ".txt": return "\uE8A5";
                default: return "\uE7C3";
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _iconFont?.Dispose();
                _iconFontSmall?.Dispose();
                _smallFont?.Dispose();
                _boldFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
