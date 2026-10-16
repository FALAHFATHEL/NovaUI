using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NovaUI
{
    [DesignerCategory("Form")]
    public partial class IconPickerForm : Form
    {
        #region P/Invoke — Cue Banner

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        #endregion

        #region Public Results

        public string SelectedGlyph { get; private set; }
        public string SelectedIconName { get; private set; }
        public string SelectedPackName { get; private set; }
        public string LoadedFontFamilyName { get; private set; }

        #endregion

        #region Colors

        private static readonly Color SearchFocusBord = Color.FromArgb(129, 140, 248);
        private static readonly Color SearchNormBord = Color.FromArgb(51, 56, 85);

        #endregion

        #region Fields

        private Font _previewFont;
        private int _currentIconSize = 50;
        private bool _searchFocused;

        // ★ Virtualization
        private List<VirtualIconGrid.IconEntry> _allEntries = new List<VirtualIconGrid.IconEntry>();
        private string _currentPackName = "";

        #endregion

        #region Design-Time Guard

        private static bool IsDesignTime
        {
            get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
        }

        #endregion

        #region Constructors

        public IconPickerForm() : this(null, null) { }

        public IconPickerForm(string currentPackName, string currentGlyph)
        {
            SelectedPackName = currentPackName ?? "";
            SelectedGlyph = currentGlyph ?? "";

            InitializeComponent();

            if (IsDesignTime)
            {
                AddDesignTimePreview();
                return;
            }

            WireEvents();
            ApplyCustomPainting();

            try { IconPackRegistry.EnsureInitialized(); } catch { }
            LoadPacks();
        }

        private void AddDesignTimePreview()
        {
            var demo = new List<VirtualIconGrid.IconEntry>();
            for (int i = 0; i < 24; i++)
            {
                demo.Add(new VirtualIconGrid.IconEntry
                {
                    Name = "icon_" + i,
                    Glyph = "\uE77B",
                    Visible = true
                });
            }
            _grid.SetItems(demo);
            _grid.PackName = "Segoe MDL2 Assets";
            _grid.IconSize = _currentIconSize;
            _lblStatus.Text = "معاينة تصميم — الأيقونات الحقيقية تُحمَّل عند التشغيل";
        }

        #endregion

        #region Event Wiring

        private void WireEvents()
        {
            _txtSearch.HandleCreated += (s, e) =>
            {
                try { SendMessage(_txtSearch.Handle, EM_SETCUEBANNER, IntPtr.Zero, "ابحث عن أيقونة..."); }
                catch { }
            };

            _closeButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            _closeButton.MouseEnter += (s, e) => _closeButton.ForeColor = Color.FromArgb(239, 68, 68);
            _closeButton.MouseLeave += (s, e) => _closeButton.ForeColor = Color.FromArgb(100, 116, 139);

            _cmbPacks.SelectedIndexChanged += (s, e) => LoadIconsForSelectedPack();

            _txtSearch.TextChanged += (s, e) =>
            {
                _clearSearchBtn.Visible = _txtSearch.Text.Length > 0;
                FilterIcons();
            };
            _txtSearch.GotFocus += (s, e) => { _searchFocused = true; _searchPanel.Invalidate(); };
            _txtSearch.LostFocus += (s, e) => { _searchFocused = false; _searchPanel.Invalidate(); };
            _txtSearch.KeyDown += TxtSearch_KeyDown;

            _clearSearchBtn.Click += (s, e) => { _txtSearch.Clear(); _txtSearch.Focus(); };
            _clearSearchBtn.MouseEnter += (s, e) => _clearSearchBtn.ForeColor = Color.FromArgb(239, 68, 68);
            _clearSearchBtn.MouseLeave += (s, e) => _clearSearchBtn.ForeColor = Color.FromArgb(148, 163, 184);

            _sizeTrackBar.ValueChanged += (s, e) =>
            {
                _currentIconSize = _sizeTrackBar.Value;
                _sizeValueLabel.Text = _currentIconSize.ToString();

                if (_grid != null)
                {
                    _grid.IconSize = _currentIconSize;
                    _grid.Invalidate();
                }
            };

            // ★ أحداث الشبكة
            _grid.IconSelected += Grid_IconSelected;
            _grid.IconConfirmed += Grid_IconConfirmed;

            KeyDown += IconPickerForm_KeyDown;
            Shown += (s, e) => { _txtSearch.Focus(); };
        }

        private void ApplyCustomPainting()
        {
            _searchPanel.Paint += SearchPanel_Paint;
        }

        #endregion

        #region Pack Loading

        private void LoadPacks()
        {
            _cmbPacks.Items.Clear();
            foreach (var p in IconPackRegistry.Packs)
                _cmbPacks.Items.Add(p.DisplayName);

            if (_cmbPacks.Items.Count == 0)
            {
                _lblStatus.Text = "لا توجد حزم أيقونات مسجّلة";
                ShowEmptyState("لا توجد حزم مسجّلة");
                return;
            }

            int idx = 0;
            if (!string.IsNullOrEmpty(SelectedPackName))
            {
                for (int i = 0; i < _cmbPacks.Items.Count; i++)
                {
                    if (string.Equals(_cmbPacks.Items[i].ToString(),
                        SelectedPackName, StringComparison.OrdinalIgnoreCase))
                    { idx = i; break; }
                }
            }
            _cmbPacks.SelectedIndex = idx;
        }

        private void LoadIconsForSelectedPack()
        {
            _grid.SuspendLayout();

            string packName = _cmbPacks.SelectedItem != null
                ? _cmbPacks.SelectedItem.ToString() : "";
            var pack = IconPackRegistry.GetByName(packName);
            if (pack == null) { _grid.ResumeLayout(); return; }

            _currentPackName = packName;
            LoadedFontFamilyName = BuildPreviewFont(pack);

            // ★ القائمة الكاملة — لا Pagination
            _allEntries.Clear();
            var icons = IconPackRegistry.GetIcons(pack);

            // ★ الحلقة الناقصة — بدونها لا تظهر أي أيقونة
            foreach (var kv in icons.OrderBy(k => k.Key))
            {
                _allEntries.Add(new VirtualIconGrid.IconEntry
                {
                    Name = kv.Key,
                    Glyph = kv.Value,
                    Visible = true
                });
            }

            _grid.PackName = packName;
            _grid.IconSize = _currentIconSize;
            _grid.SetItems(_allEntries);

            _grid.ResumeLayout();

            _lblStatus.Text = string.Format("📦 {0}  •  {1} أيقونة  •  خط: {2}",
                pack.DisplayName, _allEntries.Count, LoadedFontFamilyName);

            _txtSearch.Focus();
        }

        private string BuildPreviewFont(IIconPack pack)
        {
            if (_previewFont != null) { _previewFont.Dispose(); _previewFont = null; }

            try
            {
                _previewFont = FontManager.GetIconFontByPack(pack.DisplayName, 16F);
                string familyName = FontManager.GetFamilyName(pack);
                if (!string.IsNullOrEmpty(familyName)) return familyName;
            }
            catch { }

            _previewFont = new Font("Segoe MDL2 Assets", 16F);
            return "Segoe MDL2 Assets";
        }

        #endregion

        #region Grid Events

        private void Grid_IconSelected(object sender, VirtualIconGrid.IconEntry entry)
        {
            if (entry == null) return;

            SelectedGlyph = entry.Glyph;
            SelectedIconName = entry.Name;
            SelectedPackName = _currentPackName;

            _lblPreview.Text = string.Format("{0}  •  {1}", entry.Glyph, entry.Name);
        }

        private void Grid_IconConfirmed(object sender, VirtualIconGrid.IconEntry entry)
        {
            Grid_IconSelected(sender, entry);
            ConfirmSelection();
        }

        #endregion

        #region Search & Filter

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_txtSearch.Text.Length > 0) { _txtSearch.Clear(); e.SuppressKeyPress = true; }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                var first = _allEntries.FirstOrDefault(x => x.Visible);
                if (first != null)
                {
                    _grid.SelectEntry(first);
                    Grid_IconSelected(null, first);
                    ConfirmSelection();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                _grid.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void FilterIcons()
        {
            string query = _txtSearch.Text.Trim();
            var terms = query.Length == 0
                ? new string[0]
                : query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int visibleCount = 0;
            foreach (var entry in _allEntries)
            {
                bool matches = true;
                foreach (var term in terms)
                {
                    if (entry.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0)
                    { matches = false; break; }
                }
                entry.Visible = matches;
                if (matches) visibleCount++;
            }

            _grid.PackName = _currentPackName;
            _grid.IconSize = _currentIconSize;
            _grid.SetItems(_allEntries);

            if (visibleCount == 0 && _allEntries.Count > 0)
                ShowEmptyState("لا توجد أيقونات مطابقة لـ \"" + query + "\"");
            else
                HideEmptyState();

            if (_allEntries.Count > 0)
            {
                _lblStatus.Text = terms.Length > 0
                    ? string.Format("🔍 {0} نتيجة من {1}", visibleCount, _allEntries.Count)
                    : string.Format("📦 {0} أيقونة", _allEntries.Count);
            }
        }

        private void ShowEmptyState(string message)
        {
            _emptyStateLabel.Text = message;
            _emptyStatePanel.Visible = true;
            _emptyStatePanel.BringToFront();
        }

        private void HideEmptyState()
        {
            _emptyStatePanel.Visible = false;
            _grid.BringToFront();
        }

        #endregion

        #region Selection

        private void ConfirmSelection()
        {
            if (string.IsNullOrEmpty(SelectedGlyph)) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region Keyboard

        private void IconPickerForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Enter && !string.IsNullOrEmpty(SelectedGlyph))
            {
                ConfirmSelection();
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.F && e.Control)
            {
                _txtSearch.Focus();
                _txtSearch.SelectAll();
                e.SuppressKeyPress = true;
            }
        }

        #endregion

        #region Search Panel Painting

        private void SearchPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, _searchPanel.Width - 1, _searchPanel.Height - 1);

            using (var path = RoundedRect(rect, 8))
            {
                // ★ خلفية داكنة بدل الأبيض
                using (var b = new SolidBrush(Color.FromArgb(30, 41, 59)))  // Slate-800
                    g.FillPath(b, path);

                using (var p = new Pen(_searchFocused ? SearchFocusBord : SearchNormBord,
                                       _searchFocused ? 1.6f : 1f))
                    g.DrawPath(p, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion
    }
}