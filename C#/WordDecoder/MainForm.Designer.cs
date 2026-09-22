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
        this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
        this.VersionLabel = new System.Windows.Forms.Label();
        this.SaveResultsLabel = new System.Windows.Forms.Label();
        this.ElapsedTimeLabel = new System.Windows.Forms.Label();
        this.ViewDictionaryLabel = new System.Windows.Forms.Label();
        this.SearchButton = new System.Windows.Forms.Button();
        this.SearchInDictionaryCheckBox = new System.Windows.Forms.CheckBox();
        this.ResultsListBox = new System.Windows.Forms.ListBox();
        this.NWLabel = new System.Windows.Forms.Label();
        this.WLabel = new System.Windows.Forms.Label();
        this.SWLabel = new System.Windows.Forms.Label();
        this.SLabel = new System.Windows.Forms.Label();
        this.NLabel = new System.Windows.Forms.Label();
        this.SELabel = new System.Windows.Forms.Label();
        this.ELabel = new System.Windows.Forms.Label();
        this.NELabel = new System.Windows.Forms.Label();
        this.CLabel = new System.Windows.Forms.Label();
        this.NumericalSystemComboBox = new System.Windows.Forms.ComboBox();
        this.TextModeComboBox = new System.Windows.Forms.ComboBox();
        this.CipherTextBox = new System.Windows.Forms.TextBox();
        this.MessageTextBox = new System.Windows.Forms.TextBox();
        this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
        this.NotifyIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.MTextBox = new System.Windows.Forms.TextBox();
        this.CTextBox = new System.Windows.Forms.TextBox();
        this.ProgressBar = new System.Windows.Forms.ProgressBar();
        this.ElapsedTimeValueLabel = new System.Windows.Forms.Label();
        this.ProgressLabel = new System.Windows.Forms.Label();
        this.ProgressValueLabel = new System.Windows.Forms.Label();
        this.ResultsCountLabel = new System.Windows.Forms.Label();
        this.NotifyIconContextMenuStrip.SuspendLayout();
        this.SuspendLayout();
        // 
        // VersionLabel
        // 
        this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.VersionLabel.BackColor = System.Drawing.Color.Transparent;
        this.VersionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.VersionLabel.Font = new System.Drawing.Font("Verdana", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.VersionLabel.ForeColor = System.Drawing.Color.Black;
        this.VersionLabel.Location = new System.Drawing.Point(3, 345);
        this.VersionLabel.Name = "VersionLabel";
        this.VersionLabel.Size = new System.Drawing.Size(278, 21);
        this.VersionLabel.TabIndex = 999;
        this.VersionLabel.Tag = "http://qurancode.com";
        this.VersionLabel.Text = "w w w . q u r a n c o d e . c o m";
        this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        this.ToolTip.SetToolTip(this.VersionLabel, "©2009-2026 Ali Adams");
        this.VersionLabel.Click += new System.EventHandler(this.LinkLabel_Click);
        this.VersionLabel.MouseHover += new System.EventHandler(this.VersionLabel_MouseHover);
        // 
        // SaveResultsLabel
        // 
        this.SaveResultsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.SaveResultsLabel.BackColor = System.Drawing.Color.Transparent;
        this.SaveResultsLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.SaveResultsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.SaveResultsLabel.Image = ((System.Drawing.Image)(resources.GetObject("SaveResultsLabel.Image")));
        this.SaveResultsLabel.Location = new System.Drawing.Point(74, 271);
        this.SaveResultsLabel.Name = "SaveResultsLabel";
        this.SaveResultsLabel.Size = new System.Drawing.Size(19, 19);
        this.SaveResultsLabel.TabIndex = 8;
        this.SaveResultsLabel.Text = " ";
        this.SaveResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.ToolTip.SetToolTip(this.SaveResultsLabel, "Save found words");
        this.SaveResultsLabel.Click += new System.EventHandler(this.SaveResultsLabel_Click);
        // 
        // ElapsedTimeLabel
        // 
        this.ElapsedTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.ElapsedTimeLabel.BackColor = System.Drawing.SystemColors.Info;
        this.ElapsedTimeLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ElapsedTimeLabel.Font = new System.Drawing.Font("Tahoma", 8F);
        this.ElapsedTimeLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
        this.ElapsedTimeLabel.Location = new System.Drawing.Point(93, 271);
        this.ElapsedTimeLabel.Name = "ElapsedTimeLabel";
        this.ElapsedTimeLabel.Size = new System.Drawing.Size(57, 19);
        this.ElapsedTimeLabel.TabIndex = 53;
        this.ElapsedTimeLabel.Text = "Elapsed";
        this.ElapsedTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.ToolTip.SetToolTip(this.ElapsedTimeLabel, "Elapsed Time");
        this.ElapsedTimeLabel.Click += new System.EventHandler(this.ElapsedTimeLabel_Click);
        // 
        // ViewDictionaryLabel
        // 
        this.ViewDictionaryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.ViewDictionaryLabel.BackColor = System.Drawing.Color.Transparent;
        this.ViewDictionaryLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ViewDictionaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ViewDictionaryLabel.Image = ((System.Drawing.Image)(resources.GetObject("ViewDictionaryLabel.Image")));
        this.ViewDictionaryLabel.Location = new System.Drawing.Point(3, 271);
        this.ViewDictionaryLabel.Name = "ViewDictionaryLabel";
        this.ViewDictionaryLabel.Size = new System.Drawing.Size(19, 19);
        this.ViewDictionaryLabel.TabIndex = 7;
        this.ViewDictionaryLabel.Text = " ";
        this.ViewDictionaryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.ToolTip.SetToolTip(this.ViewDictionaryLabel, "View dictionary");
        this.ViewDictionaryLabel.Click += new System.EventHandler(this.ViewDictionaryLabel_Click);
        // 
        // SearchButton
        // 
        this.SearchButton.BackColor = System.Drawing.SystemColors.Control;
        this.SearchButton.Cursor = System.Windows.Forms.Cursors.Hand;
        this.SearchButton.Enabled = false;
        this.SearchButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.SearchButton.Location = new System.Drawing.Point(3, 23);
        this.SearchButton.Name = "SearchButton";
        this.SearchButton.Size = new System.Drawing.Size(90, 22);
        this.SearchButton.TabIndex = 3;
        this.SearchButton.Text = "Search";
        this.ToolTip.SetToolTip(this.SearchButton, "Ctrl+Q for Boggle");
        this.SearchButton.UseVisualStyleBackColor = false;
        this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
        // 
        // SearchInDictionaryCheckBox
        // 
        this.SearchInDictionaryCheckBox.BackColor = System.Drawing.SystemColors.Control;
        this.SearchInDictionaryCheckBox.Checked = true;
        this.SearchInDictionaryCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        this.SearchInDictionaryCheckBox.Location = new System.Drawing.Point(8, 27);
        this.SearchInDictionaryCheckBox.Name = "SearchInDictionaryCheckBox";
        this.SearchInDictionaryCheckBox.Size = new System.Drawing.Size(14, 14);
        this.SearchInDictionaryCheckBox.TabIndex = 1000;
        this.ToolTip.SetToolTip(this.SearchInDictionaryCheckBox, "Search in dictionary only");
        this.SearchInDictionaryCheckBox.UseVisualStyleBackColor = false;
        this.SearchInDictionaryCheckBox.CheckedChanged += new System.EventHandler(this.SearchInDictionaryCheckBox_CheckedChanged);
        // 
        // ResultsListBox
        // 
        this.ResultsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)));
        this.ResultsListBox.BackColor = System.Drawing.SystemColors.Window;
        this.ResultsListBox.FormattingEnabled = true;
        this.ResultsListBox.Location = new System.Drawing.Point(3, 45);
        this.ResultsListBox.Name = "ResultsListBox";
        this.ResultsListBox.Size = new System.Drawing.Size(90, 225);
        this.ResultsListBox.TabIndex = 5;
        // 
        // NWLabel
        // 
        this.NWLabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.NWLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NWLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.NWLabel.Location = new System.Drawing.Point(73, 25);
        this.NWLabel.Name = "NWLabel";
        this.NWLabel.Size = new System.Drawing.Size(6, 6);
        this.NWLabel.TabIndex = 100;
        this.NWLabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // WLabel
        // 
        this.WLabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.WLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.WLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.WLabel.Location = new System.Drawing.Point(73, 31);
        this.WLabel.Name = "WLabel";
        this.WLabel.Size = new System.Drawing.Size(6, 6);
        this.WLabel.TabIndex = 101;
        this.WLabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // SWLabel
        // 
        this.SWLabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.SWLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.SWLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.SWLabel.Location = new System.Drawing.Point(73, 37);
        this.SWLabel.Name = "SWLabel";
        this.SWLabel.Size = new System.Drawing.Size(6, 6);
        this.SWLabel.TabIndex = 102;
        this.SWLabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // SLabel
        // 
        this.SLabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.SLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.SLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.SLabel.Location = new System.Drawing.Point(79, 37);
        this.SLabel.Name = "SLabel";
        this.SLabel.Size = new System.Drawing.Size(6, 6);
        this.SLabel.TabIndex = 105;
        this.SLabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // NLabel
        // 
        this.NLabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.NLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.NLabel.Location = new System.Drawing.Point(79, 25);
        this.NLabel.Name = "NLabel";
        this.NLabel.Size = new System.Drawing.Size(6, 6);
        this.NLabel.TabIndex = 103;
        this.NLabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // SELabel
        // 
        this.SELabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.SELabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.SELabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.SELabel.Location = new System.Drawing.Point(85, 37);
        this.SELabel.Name = "SELabel";
        this.SELabel.Size = new System.Drawing.Size(6, 6);
        this.SELabel.TabIndex = 108;
        this.SELabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // ELabel
        // 
        this.ELabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.ELabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.ELabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ELabel.Location = new System.Drawing.Point(85, 31);
        this.ELabel.Name = "ELabel";
        this.ELabel.Size = new System.Drawing.Size(6, 6);
        this.ELabel.TabIndex = 107;
        this.ELabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // NELabel
        // 
        this.NELabel.BackColor = System.Drawing.SystemColors.ControlDark;
        this.NELabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NELabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.NELabel.Location = new System.Drawing.Point(85, 25);
        this.NELabel.Name = "NELabel";
        this.NELabel.Size = new System.Drawing.Size(6, 6);
        this.NELabel.TabIndex = 106;
        this.NELabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // CLabel
        // 
        this.CLabel.BackColor = System.Drawing.SystemColors.ControlDarkDark;
        this.CLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.CLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.CLabel.Location = new System.Drawing.Point(79, 31);
        this.CLabel.Name = "CLabel";
        this.CLabel.Size = new System.Drawing.Size(6, 6);
        this.CLabel.TabIndex = 109;
        this.CLabel.Click += new System.EventHandler(this.DirectionsLabel_Click);
        // 
        // NumericalSystemComboBox
        // 
        this.NumericalSystemComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumericalSystemComboBox.BackColor = System.Drawing.SystemColors.Window;
        this.NumericalSystemComboBox.DropDownHeight = 1024;
        this.NumericalSystemComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.NumericalSystemComboBox.FormattingEnabled = true;
        this.NumericalSystemComboBox.IntegralHeight = false;
        this.NumericalSystemComboBox.Location = new System.Drawing.Point(93, 2);
        this.NumericalSystemComboBox.Name = "NumericalSystemComboBox";
        this.NumericalSystemComboBox.Size = new System.Drawing.Size(188, 21);
        this.NumericalSystemComboBox.TabIndex = 2;
        this.NumericalSystemComboBox.SelectedIndexChanged += new System.EventHandler(this.NumericalSystemComboBox_SelectedIndexChanged);
        this.NumericalSystemComboBox.MouseHover += new System.EventHandler(this.NumericalSystemComboBox_MouseHover);
        // 
        // TextModeComboBox
        // 
        this.TextModeComboBox.BackColor = System.Drawing.SystemColors.Window;
        this.TextModeComboBox.DropDownHeight = 1024;
        this.TextModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.TextModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.TextModeComboBox.FormattingEnabled = true;
        this.TextModeComboBox.IntegralHeight = false;
        this.TextModeComboBox.Location = new System.Drawing.Point(3, 2);
        this.TextModeComboBox.Name = "TextModeComboBox";
        this.TextModeComboBox.Size = new System.Drawing.Size(90, 21);
        this.TextModeComboBox.TabIndex = 1;
        this.TextModeComboBox.SelectedIndexChanged += new System.EventHandler(this.TextModeComboBox_SelectedIndexChanged);
        // 
        // CipherTextBox
        // 
        this.CipherTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CipherTextBox.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.CipherTextBox.Location = new System.Drawing.Point(93, 23);
        this.CipherTextBox.Name = "CipherTextBox";
        this.CipherTextBox.Size = new System.Drawing.Size(188, 24);
        this.CipherTextBox.TabIndex = 4;
        this.CipherTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        this.CipherTextBox.TextChanged += new System.EventHandler(this.CipherTextBox_TextChanged);
        this.CipherTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CipherTextBox_KeyPress);
        // 
        // MessageTextBox
        // 
        this.MessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MessageTextBox.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.MessageTextBox.Location = new System.Drawing.Point(93, 45);
        this.MessageTextBox.Multiline = true;
        this.MessageTextBox.Name = "MessageTextBox";
        this.MessageTextBox.ReadOnly = true;
        this.MessageTextBox.Size = new System.Drawing.Size(188, 225);
        this.MessageTextBox.TabIndex = 20;
        this.MessageTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        this.MessageTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
        this.MessageTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FixMicrosoft);
        // 
        // NotifyIcon
        // 
        this.NotifyIcon.ContextMenuStrip = this.NotifyIconContextMenuStrip;
        this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
        this.NotifyIcon.Text = "Word Decoder";
        this.NotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseClick);
        // 
        // NotifyIconContextMenuStrip
        // 
        this.NotifyIconContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
        this.NotifyIconContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripMenuItem,
            this.ExitToolStripMenuItem});
        this.NotifyIconContextMenuStrip.Name = "NotifyIconContextMenuStrip";
        this.NotifyIconContextMenuStrip.Size = new System.Drawing.Size(108, 48);
        // 
        // AboutToolStripMenuItem
        // 
        this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
        this.AboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
        this.AboutToolStripMenuItem.Text = "About";
        this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
        // 
        // ExitToolStripMenuItem
        // 
        this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
        this.ExitToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
        this.ExitToolStripMenuItem.Text = "Exit";
        this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
        // 
        // MTextBox
        // 
        this.MTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.MTextBox.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.MTextBox.Location = new System.Drawing.Point(3, 300);
        this.MTextBox.Name = "MTextBox";
        this.MTextBox.Size = new System.Drawing.Size(278, 24);
        this.MTextBox.TabIndex = 81;
        this.MTextBox.Text = "اللهم صل على محمد وءال محمد";
        this.MTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        this.MTextBox.TextChanged += new System.EventHandler(this.MTextBox_TextChanged);
        // 
        // CTextBox
        // 
        this.CTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.CTextBox.BackColor = System.Drawing.SystemColors.Control;
        this.CTextBox.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.CTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
        this.CTextBox.Location = new System.Drawing.Point(3, 321);
        this.CTextBox.Name = "CTextBox";
        this.CTextBox.ReadOnly = true;
        this.CTextBox.Size = new System.Drawing.Size(278, 24);
        this.CTextBox.TabIndex = 82;
        this.CTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        // 
        // ProgressBar
        // 
        this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ProgressBar.Location = new System.Drawing.Point(3, 291);
        this.ProgressBar.Name = "ProgressBar";
        this.ProgressBar.Size = new System.Drawing.Size(278, 10);
        this.ProgressBar.TabIndex = 50;
        // 
        // ElapsedTimeValueLabel
        // 
        this.ElapsedTimeValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ElapsedTimeValueLabel.BackColor = System.Drawing.SystemColors.Info;
        this.ElapsedTimeValueLabel.Font = new System.Drawing.Font("Tahoma", 10F);
        this.ElapsedTimeValueLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
        this.ElapsedTimeValueLabel.Location = new System.Drawing.Point(144, 271);
        this.ElapsedTimeValueLabel.Name = "ElapsedTimeValueLabel";
        this.ElapsedTimeValueLabel.Size = new System.Drawing.Size(102, 19);
        this.ElapsedTimeValueLabel.TabIndex = 54;
        this.ElapsedTimeValueLabel.Text = "000d 00:00:00";
        this.ElapsedTimeValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ProgressLabel
        // 
        this.ProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ProgressLabel.BackColor = System.Drawing.SystemColors.Info;
        this.ProgressLabel.Font = new System.Drawing.Font("Tahoma", 8F);
        this.ProgressLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
        this.ProgressLabel.Location = new System.Drawing.Point(93, 271);
        this.ProgressLabel.Name = "ProgressLabel";
        this.ProgressLabel.Size = new System.Drawing.Size(188, 19);
        this.ProgressLabel.TabIndex = 51;
        this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ProgressValueLabel
        // 
        this.ProgressValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ProgressValueLabel.BackColor = System.Drawing.SystemColors.Info;
        this.ProgressValueLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ProgressValueLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
        this.ProgressValueLabel.Location = new System.Drawing.Point(242, 271);
        this.ProgressValueLabel.Name = "ProgressValueLabel";
        this.ProgressValueLabel.Size = new System.Drawing.Size(36, 19);
        this.ProgressValueLabel.TabIndex = 56;
        this.ProgressValueLabel.Text = "100%";
        this.ProgressValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ResultsCountLabel
        // 
        this.ResultsCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.ResultsCountLabel.BackColor = System.Drawing.Color.Transparent;
        this.ResultsCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ResultsCountLabel.ForeColor = System.Drawing.Color.SteelBlue;
        this.ResultsCountLabel.Location = new System.Drawing.Point(3, 271);
        this.ResultsCountLabel.Name = "ResultsCountLabel";
        this.ResultsCountLabel.Size = new System.Drawing.Size(90, 19);
        this.ResultsCountLabel.TabIndex = 6;
        this.ResultsCountLabel.Text = "0";
        this.ResultsCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // MainForm
        // 
        this.AcceptButton = this.SearchButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.LightSteelBlue;
        this.ClientSize = new System.Drawing.Size(284, 362);
        this.Controls.Add(this.SearchInDictionaryCheckBox);
        this.Controls.Add(this.ViewDictionaryLabel);
        this.Controls.Add(this.CTextBox);
        this.Controls.Add(this.ProgressBar);
        this.Controls.Add(this.VersionLabel);
        this.Controls.Add(this.MTextBox);
        this.Controls.Add(this.MessageTextBox);
        this.Controls.Add(this.CipherTextBox);
        this.Controls.Add(this.NumericalSystemComboBox);
        this.Controls.Add(this.TextModeComboBox);
        this.Controls.Add(this.SaveResultsLabel);
        this.Controls.Add(this.ResultsCountLabel);
        this.Controls.Add(this.SearchButton);
        this.Controls.Add(this.CLabel);
        this.Controls.Add(this.SELabel);
        this.Controls.Add(this.ELabel);
        this.Controls.Add(this.NELabel);
        this.Controls.Add(this.SLabel);
        this.Controls.Add(this.NLabel);
        this.Controls.Add(this.SWLabel);
        this.Controls.Add(this.WLabel);
        this.Controls.Add(this.NWLabel);
        this.Controls.Add(this.ResultsListBox);
        this.Controls.Add(this.ElapsedTimeValueLabel);
        this.Controls.Add(this.ProgressValueLabel);
        this.Controls.Add(this.ElapsedTimeLabel);
        this.Controls.Add(this.ProgressLabel);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.KeyPreview = true;
        this.MinimumSize = new System.Drawing.Size(300, 153);
        this.Name = "MainForm";
        this.ShowInTaskbar = false;
        this.Text = "Word Decoder";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.Shown += new System.EventHandler(this.MainForm_Shown);
        this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
        this.Resize += new System.EventHandler(this.MainForm_Resize);
        this.NotifyIconContextMenuStrip.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolTip ToolTip;
    private System.Windows.Forms.ComboBox NumericalSystemComboBox;
    private System.Windows.Forms.ComboBox TextModeComboBox;
    private System.Windows.Forms.TextBox CipherTextBox;
    private System.Windows.Forms.TextBox MessageTextBox;
    private System.Windows.Forms.NotifyIcon NotifyIcon;
    private System.Windows.Forms.ContextMenuStrip NotifyIconContextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
    private System.Windows.Forms.TextBox MTextBox;
    private System.Windows.Forms.TextBox CTextBox;
    private System.Windows.Forms.Label VersionLabel;
    private System.Windows.Forms.ProgressBar ProgressBar;
    private System.Windows.Forms.Label ElapsedTimeLabel;
    private System.Windows.Forms.Label ElapsedTimeValueLabel;
    private System.Windows.Forms.Label ProgressLabel;
    private System.Windows.Forms.Label ProgressValueLabel;
    private System.Windows.Forms.Button SearchButton;
    private System.Windows.Forms.ListBox ResultsListBox;
    private System.Windows.Forms.Label SaveResultsLabel;
    private System.Windows.Forms.Label ResultsCountLabel;
    private System.Windows.Forms.Label NWLabel;
    private System.Windows.Forms.Label WLabel;
    private System.Windows.Forms.Label SWLabel;
    private System.Windows.Forms.Label SLabel;
    private System.Windows.Forms.Label NLabel;
    private System.Windows.Forms.Label SELabel;
    private System.Windows.Forms.Label ELabel;
    private System.Windows.Forms.Label NELabel;
    private System.Windows.Forms.Label CLabel;
    private System.Windows.Forms.Label ViewDictionaryLabel;
    private System.Windows.Forms.CheckBox SearchInDictionaryCheckBox;
}
