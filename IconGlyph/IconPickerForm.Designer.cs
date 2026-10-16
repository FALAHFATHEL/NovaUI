namespace NovaUI
{
    partial class IconPickerForm
    {
        private System.ComponentModel.IContainer components = null;

        // Header
        private System.Windows.Forms.Panel _headerPanel;
        private System.Windows.Forms.Label _titleLabel;
        private System.Windows.Forms.Label _subtitleLabel;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.Panel _headerAccent;

        // Toolbar
        private System.Windows.Forms.Panel _toolbarPanel;
        private System.Windows.Forms.ComboBox _cmbPacks;
        private System.Windows.Forms.Panel _searchPanel;
        private System.Windows.Forms.Label _searchIconLabel;
        private System.Windows.Forms.TextBox _txtSearch;
        private System.Windows.Forms.Button _clearSearchBtn;
        private System.Windows.Forms.Label _sizeLabel;
        private System.Windows.Forms.TrackBar _sizeTrackBar;
        private System.Windows.Forms.Label _sizeValueLabel;

        // Content
        private System.Windows.Forms.Panel _contentPanel;
        private NovaUI.VirtualIconGrid _grid;
        private System.Windows.Forms.Panel _emptyStatePanel;
        private System.Windows.Forms.Label _emptyStateIcon;
        private System.Windows.Forms.Label _emptyStateLabel;

        // Status
        private System.Windows.Forms.Panel _statusPanel;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.Label _lblPreview;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (_previewFont != null) { _previewFont.Dispose(); _previewFont = null; }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._headerPanel = new System.Windows.Forms.Panel();
            this._titleLabel = new System.Windows.Forms.Label();
            this._subtitleLabel = new System.Windows.Forms.Label();
            this._closeButton = new System.Windows.Forms.Button();
            this._headerAccent = new System.Windows.Forms.Panel();
            this._toolbarPanel = new System.Windows.Forms.Panel();
            this._cmbPacks = new System.Windows.Forms.ComboBox();
            this._searchPanel = new System.Windows.Forms.Panel();
            this._searchIconLabel = new System.Windows.Forms.Label();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this._clearSearchBtn = new System.Windows.Forms.Button();
            this._sizeLabel = new System.Windows.Forms.Label();
            this._sizeTrackBar = new System.Windows.Forms.TrackBar();
            this._sizeValueLabel = new System.Windows.Forms.Label();
            this._contentPanel = new System.Windows.Forms.Panel();
            this._emptyStatePanel = new System.Windows.Forms.Panel();
            this._emptyStateLabel = new System.Windows.Forms.Label();
            this._emptyStateIcon = new System.Windows.Forms.Label();
            this._statusPanel = new System.Windows.Forms.Panel();
            this._lblStatus = new System.Windows.Forms.Label();
            this._lblPreview = new System.Windows.Forms.Label();
            this._grid = new NovaUI.VirtualIconGrid();
            this._headerPanel.SuspendLayout();
            this._toolbarPanel.SuspendLayout();
            this._searchPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._sizeTrackBar)).BeginInit();
            this._contentPanel.SuspendLayout();
            this._emptyStatePanel.SuspendLayout();
            this._statusPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _headerPanel
            // 
            this._headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this._headerPanel.Controls.Add(this._titleLabel);
            this._headerPanel.Controls.Add(this._subtitleLabel);
            this._headerPanel.Controls.Add(this._closeButton);
            this._headerPanel.Controls.Add(this._headerAccent);
            this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._headerPanel.Location = new System.Drawing.Point(0, 0);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Padding = new System.Windows.Forms.Padding(24, 0, 16, 0);
            this._headerPanel.Size = new System.Drawing.Size(1228, 84);
            this._headerPanel.TabIndex = 2;
            // 
            // _titleLabel
            // 
            this._titleLabel.BackColor = System.Drawing.Color.Transparent;
            this._titleLabel.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this._titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this._titleLabel.Location = new System.Drawing.Point(532, 9);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this._titleLabel.Size = new System.Drawing.Size(400, 39);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "معرض الأيقونات";
            // 
            // _subtitleLabel
            // 
            this._subtitleLabel.BackColor = System.Drawing.Color.Transparent;
            this._subtitleLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._subtitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this._subtitleLabel.Location = new System.Drawing.Point(428, 48);
            this._subtitleLabel.Name = "_subtitleLabel";
            this._subtitleLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this._subtitleLabel.Size = new System.Drawing.Size(500, 28);
            this._subtitleLabel.TabIndex = 1;
            this._subtitleLabel.Text = "اختر حزمة، ابحث بالاسم، ثم انقر على الأيقونة لتطبيقها";
            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.BackColor = System.Drawing.Color.Transparent;
            this._closeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this._closeButton.FlatAppearance.BorderSize = 0;
            this._closeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this._closeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._closeButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this._closeButton.Location = new System.Drawing.Point(1172, 18);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(36, 36);
            this._closeButton.TabIndex = 2;
            this._closeButton.TabStop = false;
            this._closeButton.Text = "✕";
            this._closeButton.UseVisualStyleBackColor = false;
            // 
            // _headerAccent
            // 
            this._headerAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._headerAccent.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._headerAccent.Location = new System.Drawing.Point(24, 83);
            this._headerAccent.Name = "_headerAccent";
            this._headerAccent.Size = new System.Drawing.Size(1188, 1);
            this._headerAccent.TabIndex = 3;
            // 
            // _toolbarPanel
            // 
            this._toolbarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._toolbarPanel.Controls.Add(this._cmbPacks);
            this._toolbarPanel.Controls.Add(this._searchPanel);
            this._toolbarPanel.Controls.Add(this._sizeLabel);
            this._toolbarPanel.Controls.Add(this._sizeTrackBar);
            this._toolbarPanel.Controls.Add(this._sizeValueLabel);
            this._toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._toolbarPanel.Location = new System.Drawing.Point(0, 84);
            this._toolbarPanel.Name = "_toolbarPanel";
            this._toolbarPanel.Padding = new System.Windows.Forms.Padding(24, 14, 24, 14);
            this._toolbarPanel.Size = new System.Drawing.Size(1228, 64);
            this._toolbarPanel.TabIndex = 1;
            // 
            // _cmbPacks
            // 
            this._cmbPacks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this._cmbPacks.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbPacks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._cmbPacks.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this._cmbPacks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this._cmbPacks.Location = new System.Drawing.Point(16, 15);
            this._cmbPacks.Name = "_cmbPacks";
            this._cmbPacks.Size = new System.Drawing.Size(210, 33);
            this._cmbPacks.TabIndex = 0;
            // 
            // _searchPanel
            // 
            this._searchPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._searchPanel.Controls.Add(this._searchIconLabel);
            this._searchPanel.Controls.Add(this._txtSearch);
            this._searchPanel.Controls.Add(this._clearSearchBtn);
            this._searchPanel.Location = new System.Drawing.Point(246, 6);
            this._searchPanel.Name = "_searchPanel";
            this._searchPanel.Size = new System.Drawing.Size(379, 52);
            this._searchPanel.TabIndex = 1;
            // 
            // _searchIconLabel
            // 
            this._searchIconLabel.BackColor = System.Drawing.Color.Transparent;
            this._searchIconLabel.Font = new System.Drawing.Font("Segoe MDL2 Assets", 12F);
            this._searchIconLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this._searchIconLabel.Location = new System.Drawing.Point(6, 0);
            this._searchIconLabel.Name = "_searchIconLabel";
            this._searchIconLabel.Size = new System.Drawing.Size(24, 32);
            this._searchIconLabel.TabIndex = 0;
            this._searchIconLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _txtSearch
            // 
            this._txtSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this._txtSearch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this._txtSearch.Location = new System.Drawing.Point(36, 9);
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(288, 27);
            this._txtSearch.TabIndex = 1;
            // 
            // _clearSearchBtn
            // 
            this._clearSearchBtn.BackColor = System.Drawing.Color.Transparent;
            this._clearSearchBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this._clearSearchBtn.FlatAppearance.BorderSize = 0;
            this._clearSearchBtn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this._clearSearchBtn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this._clearSearchBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._clearSearchBtn.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this._clearSearchBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this._clearSearchBtn.Location = new System.Drawing.Point(339, 8);
            this._clearSearchBtn.Name = "_clearSearchBtn";
            this._clearSearchBtn.Size = new System.Drawing.Size(24, 24);
            this._clearSearchBtn.TabIndex = 2;
            this._clearSearchBtn.TabStop = false;
            this._clearSearchBtn.Text = "✕";
            this._clearSearchBtn.UseVisualStyleBackColor = false;
            this._clearSearchBtn.Visible = false;
            // 
            // _sizeLabel
            // 
            this._sizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sizeLabel.BackColor = System.Drawing.Color.Transparent;
            this._sizeLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._sizeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this._sizeLabel.Location = new System.Drawing.Point(938, 17);
            this._sizeLabel.Name = "_sizeLabel";
            this._sizeLabel.Size = new System.Drawing.Size(82, 25);
            this._sizeLabel.TabIndex = 2;
            this._sizeLabel.Text = "الحجم";
            this._sizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _sizeTrackBar
            // 
            this._sizeTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sizeTrackBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._sizeTrackBar.Location = new System.Drawing.Point(1026, 16);
            this._sizeTrackBar.Maximum = 50;
            this._sizeTrackBar.Minimum = 14;
            this._sizeTrackBar.Name = "_sizeTrackBar";
            this._sizeTrackBar.Size = new System.Drawing.Size(140, 69);
            this._sizeTrackBar.TabIndex = 3;
            this._sizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this._sizeTrackBar.Value = 50;
            // 
            // _sizeValueLabel
            // 
            this._sizeValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sizeValueLabel.BackColor = System.Drawing.Color.Transparent;
            this._sizeValueLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._sizeValueLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(180)))), ((int)(((byte)(252)))));
            this._sizeValueLabel.Location = new System.Drawing.Point(1172, 22);
            this._sizeValueLabel.Name = "_sizeValueLabel";
            this._sizeValueLabel.Size = new System.Drawing.Size(36, 20);
            this._sizeValueLabel.TabIndex = 4;
            this._sizeValueLabel.Text = "50";
            this._sizeValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _contentPanel
            // 
            this._contentPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this._contentPanel.Controls.Add(this._emptyStatePanel);
            this._contentPanel.Controls.Add(this._grid);
            this._contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._contentPanel.Location = new System.Drawing.Point(0, 148);
            this._contentPanel.Name = "_contentPanel";
            this._contentPanel.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this._contentPanel.Size = new System.Drawing.Size(1228, 482);
            this._contentPanel.TabIndex = 0;
            // 
            // _emptyStatePanel
            // 
            this._emptyStatePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this._emptyStatePanel.Controls.Add(this._emptyStateLabel);
            this._emptyStatePanel.Controls.Add(this._emptyStateIcon);
            this._emptyStatePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._emptyStatePanel.Location = new System.Drawing.Point(16, 12);
            this._emptyStatePanel.Name = "_emptyStatePanel";
            this._emptyStatePanel.Size = new System.Drawing.Size(1196, 458);
            this._emptyStatePanel.TabIndex = 1;
            this._emptyStatePanel.Visible = false;
            // 
            // _emptyStateLabel
            // 
            this._emptyStateLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._emptyStateLabel.Font = new System.Drawing.Font("Segoe UI", 11F);
            this._emptyStateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this._emptyStateLabel.Location = new System.Drawing.Point(0, 80);
            this._emptyStateLabel.Name = "_emptyStateLabel";
            this._emptyStateLabel.Size = new System.Drawing.Size(1196, 30);
            this._emptyStateLabel.TabIndex = 0;
            this._emptyStateLabel.Text = "لا توجد أيقونات مطابقة";
            this._emptyStateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _emptyStateIcon
            // 
            this._emptyStateIcon.Dock = System.Windows.Forms.DockStyle.Top;
            this._emptyStateIcon.Font = new System.Drawing.Font("Segoe MDL2 Assets", 42F);
            this._emptyStateIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this._emptyStateIcon.Location = new System.Drawing.Point(0, 0);
            this._emptyStateIcon.Name = "_emptyStateIcon";
            this._emptyStateIcon.Size = new System.Drawing.Size(1196, 80);
            this._emptyStateIcon.TabIndex = 1;
            this._emptyStateIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _statusPanel
            // 
            this._statusPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this._statusPanel.Controls.Add(this._lblStatus);
            this._statusPanel.Controls.Add(this._lblPreview);
            this._statusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._statusPanel.Location = new System.Drawing.Point(0, 630);
            this._statusPanel.Name = "_statusPanel";
            this._statusPanel.Padding = new System.Windows.Forms.Padding(24, 0, 24, 0);
            this._statusPanel.Size = new System.Drawing.Size(1228, 34);
            this._statusPanel.TabIndex = 3;
            // 
            // _lblStatus
            // 
            this._lblStatus.AutoSize = true;
            this._lblStatus.BackColor = System.Drawing.Color.Transparent;
            this._lblStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this._lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this._lblStatus.Location = new System.Drawing.Point(24, 0);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this._lblStatus.Size = new System.Drawing.Size(0, 31);
            this._lblStatus.TabIndex = 0;
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblPreview
            // 
            this._lblPreview.AutoSize = true;
            this._lblPreview.BackColor = System.Drawing.Color.Transparent;
            this._lblPreview.Dock = System.Windows.Forms.DockStyle.Right;
            this._lblPreview.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this._lblPreview.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(180)))), ((int)(((byte)(252)))));
            this._lblPreview.Location = new System.Drawing.Point(1204, 0);
            this._lblPreview.Name = "_lblPreview";
            this._lblPreview.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this._lblPreview.Size = new System.Drawing.Size(0, 31);
            this._lblPreview.TabIndex = 1;
            this._lblPreview.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _grid
            // 
            this._grid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grid.IconSize = 22;
            this._grid.Location = new System.Drawing.Point(16, 12);
            this._grid.Name = "_grid";
            this._grid.PackName = "";
            this._grid.SelectedIndex = -1;
            this._grid.Size = new System.Drawing.Size(1196, 458);
            this._grid.TabIndex = 0;
            // 
            // IconPickerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.ClientSize = new System.Drawing.Size(1228, 664);
            this.Controls.Add(this._contentPanel);
            this.Controls.Add(this._toolbarPanel);
            this.Controls.Add(this._headerPanel);
            this.Controls.Add(this._statusPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(640, 420);
            this.Name = "IconPickerForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "معرض الأيقونات";
            this._headerPanel.ResumeLayout(false);
            this._toolbarPanel.ResumeLayout(false);
            this._toolbarPanel.PerformLayout();
            this._searchPanel.ResumeLayout(false);
            this._searchPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._sizeTrackBar)).EndInit();
            this._contentPanel.ResumeLayout(false);
            this._emptyStatePanel.ResumeLayout(false);
            this._statusPanel.ResumeLayout(false);
            this._statusPanel.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}