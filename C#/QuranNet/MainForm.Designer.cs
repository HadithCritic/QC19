namespace QuranNet
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.TabControl = new System.Windows.Forms.TabControl();
            this.NodesTabPage = new System.Windows.Forms.TabPage();
            this.NodesListView = new System.Windows.Forms.ListView();
            this.NodesColumnHeaderId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NodesColumnHeaderX = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NodesColumnHeaderY = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NodesColumnHeaderZ = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NodesColumnHeaderText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NodesColumnHeaderMeaning = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NodesColumnHeaderRoot = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NodesColumnHeaderValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LinksTabPage = new System.Windows.Forms.TabPage();
            this.LinksListView = new System.Windows.Forms.ListView();
            this.LinksColumnHeaderId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LinksColumnHeaderSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LinksColumnHeaderTarget = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LinksColumnHeaderLabel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LinksColumnHeaderRange = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LinksColumnHeaderNumberInRange = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LinkColumnHeaderVerseLink = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NetworkTabPage = new System.Windows.Forms.TabPage();
            this.NetworkPanel = new System.Windows.Forms.Panel();
            this.NetworkTextBox = new System.Windows.Forms.TextBox();
            this.OptionsPanel = new System.Windows.Forms.Panel();
            this.IncludeWordsCheckBox = new System.Windows.Forms.CheckBox();
            this.ChaptersComboBox = new System.Windows.Forms.ComboBox();
            this.MinRangeComboBox = new System.Windows.Forms.ComboBox();
            this.BuildNetworkButton = new System.Windows.Forms.Button();
            this.OpenNetworkFolderButton = new System.Windows.Forms.Button();
            this.LinkToComboBox = new System.Windows.Forms.ComboBox();
            this.LinkByComboBox = new System.Windows.Forms.ComboBox();
            this.NumericalSystemComboBox = new System.Windows.Forms.ComboBox();
            this.TextModeComboBox = new System.Windows.Forms.ComboBox();
            ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.TabControl.SuspendLayout();
            this.NodesTabPage.SuspendLayout();
            this.LinksTabPage.SuspendLayout();
            this.NetworkTabPage.SuspendLayout();
            this.NetworkPanel.SuspendLayout();
            this.OptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.NodesTabPage);
            this.TabControl.Controls.Add(this.LinksTabPage);
            this.TabControl.Controls.Add(this.NetworkTabPage);
            this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl.Location = new System.Drawing.Point(0, 0);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(551, 572);
            this.TabControl.TabIndex = 7;
            // 
            // NodesTabPage
            // 
            this.NodesTabPage.Controls.Add(this.NodesListView);
            this.NodesTabPage.Location = new System.Drawing.Point(4, 22);
            this.NodesTabPage.Name = "NodesTabPage";
            this.NodesTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.NodesTabPage.Size = new System.Drawing.Size(543, 546);
            this.NodesTabPage.TabIndex = 0;
            this.NodesTabPage.Text = "Nodes";
            this.NodesTabPage.UseVisualStyleBackColor = true;
            // 
            // NodesListView
            // 
            this.NodesListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.NodesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NodesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NodesColumnHeaderId,
            this.NodesColumnHeaderX,
            this.NodesColumnHeaderY,
            this.NodesColumnHeaderZ,
            this.NodesColumnHeaderText,
            this.NodesColumnHeaderMeaning,
            this.NodesColumnHeaderRoot,
            this.NodesColumnHeaderValue});
            this.NodesListView.FullRowSelect = true;
            this.NodesListView.Location = new System.Drawing.Point(0, 0);
            this.NodesListView.Name = "NodesListView";
            this.NodesListView.Size = new System.Drawing.Size(541, 523);
            this.NodesListView.TabIndex = 1;
            this.NodesListView.UseCompatibleStateImageBehavior = false;
            this.NodesListView.View = System.Windows.Forms.View.Details;
            this.NodesListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.NodesListView_ColumnClick);
            this.NodesListView.DoubleClick += new System.EventHandler(this.NodesListView_DoubleClick);
            // 
            // NodesColumnHeaderId
            // 
            this.NodesColumnHeaderId.Text = "Id";
            this.NodesColumnHeaderId.Width = 40;
            // 
            // NodesColumnHeaderX
            // 
            this.NodesColumnHeaderX.Text = "X";
            this.NodesColumnHeaderX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NodesColumnHeaderX.Width = 36;
            // 
            // NodesColumnHeaderY
            // 
            this.NodesColumnHeaderY.Text = "Y";
            this.NodesColumnHeaderY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NodesColumnHeaderY.Width = 36;
            // 
            // NodesColumnHeaderZ
            // 
            this.NodesColumnHeaderZ.Text = "Z";
            this.NodesColumnHeaderZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NodesColumnHeaderZ.Width = 36;
            // 
            // NodesColumnHeaderText
            // 
            this.NodesColumnHeaderText.Text = "Text";
            this.NodesColumnHeaderText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NodesColumnHeaderText.Width = 73;
            // 
            // NodesColumnHeaderMeaning
            // 
            this.NodesColumnHeaderMeaning.Text = "Meaning";
            this.NodesColumnHeaderMeaning.Width = 144;
            // 
            // NodesColumnHeaderRoot
            // 
            this.NodesColumnHeaderRoot.Text = "Root";
            this.NodesColumnHeaderRoot.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NodesColumnHeaderRoot.Width = 57;
            // 
            // NodesColumnHeaderValue
            // 
            this.NodesColumnHeaderValue.Text = "Value";
            this.NodesColumnHeaderValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.NodesColumnHeaderValue.Width = 66;
            // 
            // LinksTabPage
            // 
            this.LinksTabPage.Controls.Add(this.LinksListView);
            this.LinksTabPage.Location = new System.Drawing.Point(4, 22);
            this.LinksTabPage.Name = "LinksTabPage";
            this.LinksTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.LinksTabPage.Size = new System.Drawing.Size(543, 546);
            this.LinksTabPage.TabIndex = 1;
            this.LinksTabPage.Text = "Links";
            this.LinksTabPage.UseVisualStyleBackColor = true;
            // 
            // LinksListView
            // 
            this.LinksListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LinksListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LinksColumnHeaderId,
            this.LinksColumnHeaderSource,
            this.LinksColumnHeaderTarget,
            this.LinksColumnHeaderLabel,
            this.LinksColumnHeaderRange,
            this.LinksColumnHeaderNumberInRange,
            this.LinkColumnHeaderVerseLink});
            this.LinksListView.FullRowSelect = true;
            this.LinksListView.Location = new System.Drawing.Point(0, 0);
            this.LinksListView.Name = "LinksListView";
            this.LinksListView.Size = new System.Drawing.Size(541, 523);
            this.LinksListView.TabIndex = 2;
            this.LinksListView.UseCompatibleStateImageBehavior = false;
            this.LinksListView.View = System.Windows.Forms.View.Details;
            this.LinksListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LinksListView_ColumnClick);
            this.LinksListView.DoubleClick += new System.EventHandler(this.LinksListView_DoubleClick);
            // 
            // LinksColumnHeaderId
            // 
            this.LinksColumnHeaderId.Text = "Id";
            this.LinksColumnHeaderId.Width = 50;
            // 
            // LinksColumnHeaderSource
            // 
            this.LinksColumnHeaderSource.Text = "Source";
            this.LinksColumnHeaderSource.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LinksColumnHeaderSource.Width = 72;
            // 
            // LinksColumnHeaderTarget
            // 
            this.LinksColumnHeaderTarget.Text = "Target";
            this.LinksColumnHeaderTarget.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.LinksColumnHeaderTarget.Width = 72;
            // 
            // LinksColumnHeaderLabel
            // 
            this.LinksColumnHeaderLabel.Text = "Label";
            this.LinksColumnHeaderLabel.Width = 148;
            // 
            // LinksColumnHeaderRange
            // 
            this.LinksColumnHeaderRange.Text = "Range";
            this.LinksColumnHeaderRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LinksColumnHeaderNumberInRange
            // 
            this.LinksColumnHeaderNumberInRange.Text = "#";
            this.LinksColumnHeaderNumberInRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LinkColumnHeaderVerseLink
            // 
            this.LinkColumnHeaderVerseLink.Text = "Verse";
            this.LinkColumnHeaderVerseLink.Width = 56;
            // 
            // NetworkTabPage
            // 
            this.NetworkTabPage.Controls.Add(this.NetworkPanel);
            this.NetworkTabPage.Location = new System.Drawing.Point(4, 22);
            this.NetworkTabPage.Name = "NetworkTabPage";
            this.NetworkTabPage.Size = new System.Drawing.Size(543, 546);
            this.NetworkTabPage.TabIndex = 2;
            this.NetworkTabPage.Text = "Network";
            this.NetworkTabPage.UseVisualStyleBackColor = true;
            // 
            // NetworkPanel
            // 
            this.NetworkPanel.Controls.Add(this.NetworkTextBox);
            this.NetworkPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NetworkPanel.Location = new System.Drawing.Point(0, 0);
            this.NetworkPanel.Name = "NetworkPanel";
            this.NetworkPanel.Size = new System.Drawing.Size(543, 546);
            this.NetworkPanel.TabIndex = 0;
            // 
            // NetworkTextBox
            // 
            this.NetworkTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NetworkTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.NetworkTextBox.Location = new System.Drawing.Point(0, 0);
            this.NetworkTextBox.Multiline = true;
            this.NetworkTextBox.Name = "NetworkTextBox";
            this.NetworkTextBox.ReadOnly = true;
            this.NetworkTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.NetworkTextBox.Size = new System.Drawing.Size(540, 523);
            this.NetworkTextBox.TabIndex = 0;
            this.NetworkTextBox.WordWrap = false;
            // 
            // OptionsPanel
            // 
            this.OptionsPanel.Controls.Add(this.IncludeWordsCheckBox);
            this.OptionsPanel.Controls.Add(this.ChaptersComboBox);
            this.OptionsPanel.Controls.Add(this.MinRangeComboBox);
            this.OptionsPanel.Controls.Add(this.BuildNetworkButton);
            this.OptionsPanel.Controls.Add(this.OpenNetworkFolderButton);
            this.OptionsPanel.Controls.Add(this.LinkToComboBox);
            this.OptionsPanel.Controls.Add(this.LinkByComboBox);
            this.OptionsPanel.Controls.Add(this.NumericalSystemComboBox);
            this.OptionsPanel.Controls.Add(this.TextModeComboBox);
            this.OptionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.OptionsPanel.Location = new System.Drawing.Point(0, 545);
            this.OptionsPanel.Name = "OptionsPanel";
            this.OptionsPanel.Size = new System.Drawing.Size(551, 27);
            this.OptionsPanel.TabIndex = 8;
            // 
            // IncludeWordsCheckBox
            // 
            this.IncludeWordsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.IncludeWordsCheckBox.AutoSize = true;
            this.IncludeWordsCheckBox.Location = new System.Drawing.Point(505, 7);
            this.IncludeWordsCheckBox.Name = "IncludeWordsCheckBox";
            this.IncludeWordsCheckBox.Size = new System.Drawing.Size(15, 14);
            this.IncludeWordsCheckBox.TabIndex = 9;
            this.ToolTip.SetToolTip(this.IncludeWordsCheckBox, "Include words");
            this.IncludeWordsCheckBox.UseVisualStyleBackColor = true;
            this.IncludeWordsCheckBox.CheckedChanged += new System.EventHandler(this.IncludeWordsCheckBox_CheckedChanged);
            // 
            // ChaptersComboBox
            // 
            this.ChaptersComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ChaptersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ChaptersComboBox.FormattingEnabled = true;
            this.ChaptersComboBox.Location = new System.Drawing.Point(3, 3);
            this.ChaptersComboBox.Name = "ChaptersComboBox";
            this.ChaptersComboBox.Size = new System.Drawing.Size(47, 21);
            this.ChaptersComboBox.TabIndex = 2;
            this.ToolTip.SetToolTip(this.ChaptersComboBox, "Chapters");
            this.ChaptersComboBox.SelectedIndexChanged += new System.EventHandler(this.ChaptersComboBox_SelectedIndexChanged);
            // 
            // MinRangeComboBox
            // 
            this.MinRangeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MinRangeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MinRangeComboBox.FormattingEnabled = true;
            this.MinRangeComboBox.Location = new System.Drawing.Point(380, 3);
            this.MinRangeComboBox.Name = "MinRangeComboBox";
            this.MinRangeComboBox.Size = new System.Drawing.Size(36, 21);
            this.MinRangeComboBox.TabIndex = 7;
            this.ToolTip.SetToolTip(this.MinRangeComboBox, "Minimum repeated nodes");
            this.MinRangeComboBox.SelectedIndexChanged += new System.EventHandler(this.MinRangeComboBox_SelectedIndexChanged);
            // 
            // BuildNetworkButton
            // 
            this.BuildNetworkButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BuildNetworkButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.BuildNetworkButton.Location = new System.Drawing.Point(415, 2);
            this.BuildNetworkButton.Name = "BuildNetworkButton";
            this.BuildNetworkButton.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.BuildNetworkButton.Size = new System.Drawing.Size(112, 23);
            this.BuildNetworkButton.TabIndex = 8;
            this.BuildNetworkButton.Text = "Build Network";
            this.BuildNetworkButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolTip.SetToolTip(this.BuildNetworkButton, "Build nodes and links, and save network");
            this.BuildNetworkButton.UseVisualStyleBackColor = true;
            this.BuildNetworkButton.Click += new System.EventHandler(this.BuildNetworkButton_Click);
            // 
            // OpenNetworkFolderButton
            // 
            this.OpenNetworkFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenNetworkFolderButton.Image = ((System.Drawing.Image)(resources.GetObject("OpenNetworkFolderButton.Image")));
            this.OpenNetworkFolderButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OpenNetworkFolderButton.Location = new System.Drawing.Point(526, 2);
            this.OpenNetworkFolderButton.Name = "OpenNetworkFolderButton";
            this.OpenNetworkFolderButton.Size = new System.Drawing.Size(23, 23);
            this.OpenNetworkFolderButton.TabIndex = 10;
            this.ToolTip.SetToolTip(this.OpenNetworkFolderButton, "Open folder with network files");
            this.OpenNetworkFolderButton.UseVisualStyleBackColor = true;
            this.OpenNetworkFolderButton.Click += new System.EventHandler(this.OpenNetworkFolderButton_Click);
            // 
            // LinkToComboBox
            // 
            this.LinkToComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkToComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LinkToComboBox.FormattingEnabled = true;
            this.LinkToComboBox.Location = new System.Drawing.Point(316, 3);
            this.LinkToComboBox.Name = "LinkToComboBox";
            this.LinkToComboBox.Size = new System.Drawing.Size(64, 21);
            this.LinkToComboBox.TabIndex = 6;
            this.ToolTip.SetToolTip(this.LinkToComboBox, "Link to _____ nodes");
            this.LinkToComboBox.SelectedIndexChanged += new System.EventHandler(this.LinkToComboBox_SelectedIndexChanged);
            // 
            // LinkByComboBox
            // 
            this.LinkByComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkByComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LinkByComboBox.FormattingEnabled = true;
            this.LinkByComboBox.Location = new System.Drawing.Point(266, 3);
            this.LinkByComboBox.Name = "LinkByComboBox";
            this.LinkByComboBox.Size = new System.Drawing.Size(50, 21);
            this.LinkByComboBox.TabIndex = 5;
            this.ToolTip.SetToolTip(this.LinkByComboBox, "Link nodes with equal ...");
            this.LinkByComboBox.SelectedIndexChanged += new System.EventHandler(this.LinkByComboBox_SelectedIndexChanged);
            // 
            // NumericalSystemComboBox
            // 
            this.NumericalSystemComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NumericalSystemComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NumericalSystemComboBox.Location = new System.Drawing.Point(130, 3);
            this.NumericalSystemComboBox.Name = "NumericalSystemComboBox";
            this.NumericalSystemComboBox.Size = new System.Drawing.Size(136, 21);
            this.NumericalSystemComboBox.TabIndex = 4;
            this.ToolTip.SetToolTip(this.NumericalSystemComboBox, "Text letter-value");
            this.NumericalSystemComboBox.SelectedIndexChanged += new System.EventHandler(this.NumericalSystemComboBox_SelectedIndexChanged);
            this.NumericalSystemComboBox.MouseHover += new System.EventHandler(this.NumericalSystemComboBox_MouseHover);
            // 
            // TextModeComboBox
            // 
            this.TextModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TextModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TextModeComboBox.FormattingEnabled = true;
            this.TextModeComboBox.Location = new System.Drawing.Point(50, 3);
            this.TextModeComboBox.Name = "TextModeComboBox";
            this.TextModeComboBox.Size = new System.Drawing.Size(80, 21);
            this.TextModeComboBox.TabIndex = 3;
            this.ToolTip.SetToolTip(this.TextModeComboBox, "Text simplification");
            this.TextModeComboBox.SelectedIndexChanged += new System.EventHandler(this.TextModeComboBox_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AcceptButton = this.BuildNetworkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 572);
            this.Controls.Add(this.OptionsPanel);
            this.Controls.Add(this.TabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "QuranNet";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.TabControl.ResumeLayout(false);
            this.NodesTabPage.ResumeLayout(false);
            this.LinksTabPage.ResumeLayout(false);
            this.NetworkTabPage.ResumeLayout(false);
            this.NetworkPanel.ResumeLayout(false);
            this.NetworkPanel.PerformLayout();
            this.OptionsPanel.ResumeLayout(false);
            this.OptionsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage NodesTabPage;
        private System.Windows.Forms.TabPage LinksTabPage;
        private System.Windows.Forms.TabPage NetworkTabPage;
        private System.Windows.Forms.ListView NodesListView;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderId;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderX;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderY;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderZ;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderText;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderMeaning;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderRoot;
        private System.Windows.Forms.ColumnHeader NodesColumnHeaderValue;
        private System.Windows.Forms.ListView LinksListView;
        private System.Windows.Forms.ColumnHeader LinksColumnHeaderId;
        private System.Windows.Forms.ColumnHeader LinksColumnHeaderSource;
        private System.Windows.Forms.ColumnHeader LinksColumnHeaderTarget;
        private System.Windows.Forms.ColumnHeader LinksColumnHeaderLabel;
        private System.Windows.Forms.ColumnHeader LinksColumnHeaderRange;
        private System.Windows.Forms.ColumnHeader LinksColumnHeaderNumberInRange;
        private System.Windows.Forms.ColumnHeader LinkColumnHeaderVerseLink;
        private System.Windows.Forms.Panel NetworkPanel;
        private System.Windows.Forms.Panel OptionsPanel;
        private System.Windows.Forms.ComboBox TextModeComboBox;
        private System.Windows.Forms.ComboBox NumericalSystemComboBox;
        private System.Windows.Forms.ComboBox LinkByComboBox;
        private System.Windows.Forms.ComboBox LinkToComboBox;
        private System.Windows.Forms.Button BuildNetworkButton;
        private System.Windows.Forms.Button OpenNetworkFolderButton;
        private System.Windows.Forms.CheckBox IncludeWordsCheckBox;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.TextBox NetworkTextBox;
        private System.Windows.Forms.ComboBox ChaptersComboBox;
        private System.Windows.Forms.ComboBox MinRangeComboBox;
    }
}
