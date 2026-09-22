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
        ToolTip = new System.Windows.Forms.ToolTip(this.components);
        this.VersionLabel = new System.Windows.Forms.Label();
        this.ResultsSaveButton = new System.Windows.Forms.Label();
        this.ElapsedTimeLabel = new System.Windows.Forms.Label();
        this.LoadLetterValuesButton = new System.Windows.Forms.Button();
        this.NumericalSystemComboBox = new System.Windows.Forms.ComboBox();
        this.TextModeComboBox = new System.Windows.Forms.ComboBox();
        this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
        this.NotifyIconContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.ProgressBar = new System.Windows.Forms.ProgressBar();
        this.ElapsedTimeValueLabel = new System.Windows.Forms.Label();
        this.ProgressLabel = new System.Windows.Forms.Label();
        this.ResultsCountLabel = new System.Windows.Forms.Label();
        this.NumbersTextBox = new System.Windows.Forms.TextBox();
        this.SumNumericUpDown = new System.Windows.Forms.NumericUpDown();
        this.SizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
        this.SumLabel = new System.Windows.Forms.Label();
        this.SizeLabel = new System.Windows.Forms.Label();
        this.ResultsListView = new System.Windows.Forms.ListView();
        this.ResultsColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.ResultsColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.ResultsColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.FindWordsButton = new System.Windows.Forms.Button();
        this.AutoRunCheckBox = new System.Windows.Forms.CheckBox();
        this.NotifyIconContextMenuStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SumNumericUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.SizeNumericUpDown)).BeginInit();
        this.SuspendLayout();
        // 
        // VersionLabel
        // 
        this.VersionLabel.BackColor = System.Drawing.Color.Transparent;
        this.VersionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.VersionLabel.Font = new System.Drawing.Font("Verdana", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.VersionLabel.ForeColor = System.Drawing.Color.SteelBlue;
        this.VersionLabel.Location = new System.Drawing.Point(0, 345);
        this.VersionLabel.Name = "VersionLabel";
        this.VersionLabel.Size = new System.Drawing.Size(462, 17);
        this.VersionLabel.TabIndex = 19;
        this.VersionLabel.Tag = "http://qurancode.com";
        this.VersionLabel.Text = "w w w . q u r a n c o d e . c o m";
        this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        this.ToolTip.SetToolTip(this.VersionLabel, "©2009-2026 Ali Adams");
        this.VersionLabel.Click += new System.EventHandler(this.LinkLabel_Click);
        this.VersionLabel.MouseHover += new System.EventHandler(this.VersionLabel_MouseHover);
        // 
        // ResultsSaveButton
        // 
        this.ResultsSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ResultsSaveButton.BackColor = System.Drawing.Color.Transparent;
        this.ResultsSaveButton.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ResultsSaveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ResultsSaveButton.Image = ((System.Drawing.Image)(resources.GetObject("ResultsSaveButton.Image")));
        this.ResultsSaveButton.Location = new System.Drawing.Point(440, 315);
        this.ResultsSaveButton.Name = "ResultsSaveButton";
        this.ResultsSaveButton.Size = new System.Drawing.Size(19, 19);
        this.ResultsSaveButton.TabIndex = 17;
        this.ResultsSaveButton.Text = " ";
        this.ResultsSaveButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.ToolTip.SetToolTip(this.ResultsSaveButton, "Save found words");
        this.ResultsSaveButton.Click += new System.EventHandler(this.ResultsSaveButton_Click);
        // 
        // ElapsedTimeLabel
        // 
        this.ElapsedTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.ElapsedTimeLabel.BackColor = System.Drawing.SystemColors.Info;
        this.ElapsedTimeLabel.Cursor = System.Windows.Forms.Cursors.Default;
        this.ElapsedTimeLabel.Font = new System.Drawing.Font("Tahoma", 8F);
        this.ElapsedTimeLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
        this.ElapsedTimeLabel.Location = new System.Drawing.Point(6, 315);
        this.ElapsedTimeLabel.Name = "ElapsedTimeLabel";
        this.ElapsedTimeLabel.Size = new System.Drawing.Size(103, 19);
        this.ElapsedTimeLabel.TabIndex = 13;
        this.ElapsedTimeLabel.Text = "Elapsed time";
        this.ElapsedTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.ToolTip.SetToolTip(this.ElapsedTimeLabel, "Elapsed Time");
        // 
        // LoadLetterValuesButton
        // 
        this.LoadLetterValuesButton.Cursor = System.Windows.Forms.Cursors.Hand;
        this.LoadLetterValuesButton.Image = ((System.Drawing.Image)(resources.GetObject("LoadLetterValuesButton.Image")));
        this.LoadLetterValuesButton.Location = new System.Drawing.Point(3, 86);
        this.LoadLetterValuesButton.Name = "LoadLetterValuesButton";
        this.LoadLetterValuesButton.Size = new System.Drawing.Size(34, 24);
        this.LoadLetterValuesButton.TabIndex = 4;
        this.ToolTip.SetToolTip(this.LoadLetterValuesButton, "Restore default numbers");
        this.LoadLetterValuesButton.UseVisualStyleBackColor = true;
        this.LoadLetterValuesButton.Click += new System.EventHandler(this.LoadLetterValuesButton_Click);
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
        this.NumericalSystemComboBox.Size = new System.Drawing.Size(366, 21);
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
        // NotifyIcon
        // 
        this.NotifyIcon.ContextMenuStrip = this.NotifyIconContextMenuStrip;
        this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
        this.NotifyIcon.Text = "Word Finder";
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
        // ProgressBar
        // 
        this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ProgressBar.Location = new System.Drawing.Point(1, 336);
        this.ProgressBar.Name = "ProgressBar";
        this.ProgressBar.Size = new System.Drawing.Size(461, 10);
        this.ProgressBar.TabIndex = 18;
        // 
        // ElapsedTimeValueLabel
        // 
        this.ElapsedTimeValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ElapsedTimeValueLabel.BackColor = System.Drawing.SystemColors.Info;
        this.ElapsedTimeValueLabel.Font = new System.Drawing.Font("Tahoma", 10F);
        this.ElapsedTimeValueLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
        this.ElapsedTimeValueLabel.Location = new System.Drawing.Point(115, 315);
        this.ElapsedTimeValueLabel.Name = "ElapsedTimeValueLabel";
        this.ElapsedTimeValueLabel.Size = new System.Drawing.Size(186, 19);
        this.ElapsedTimeValueLabel.TabIndex = 14;
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
        this.ProgressLabel.Location = new System.Drawing.Point(3, 315);
        this.ProgressLabel.Name = "ProgressLabel";
        this.ProgressLabel.Size = new System.Drawing.Size(353, 19);
        this.ProgressLabel.TabIndex = 51;
        this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ResultsCountLabel
        // 
        this.ResultsCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ResultsCountLabel.BackColor = System.Drawing.Color.Transparent;
        this.ResultsCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ResultsCountLabel.ForeColor = System.Drawing.Color.SteelBlue;
        this.ResultsCountLabel.Location = new System.Drawing.Point(362, 315);
        this.ResultsCountLabel.Name = "ResultsCountLabel";
        this.ResultsCountLabel.Size = new System.Drawing.Size(75, 19);
        this.ResultsCountLabel.TabIndex = 16;
        this.ResultsCountLabel.Text = "0";
        this.ResultsCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // NumbersTextBox
        // 
        this.NumbersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.NumbersTextBox.Location = new System.Drawing.Point(3, 24);
        this.NumbersTextBox.Multiline = true;
        this.NumbersTextBox.Name = "NumbersTextBox";
        this.NumbersTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.NumbersTextBox.Size = new System.Drawing.Size(456, 61);
        this.NumbersTextBox.TabIndex = 3;
        this.NumbersTextBox.TextChanged += new System.EventHandler(this.NumbersTextBox_TextChanged);
        this.NumbersTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NumbersTextBox_KeyDown);
        this.NumbersTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumbersTextBox_KeyPress);
        // 
        // SumNumericUpDown
        // 
        this.SumNumericUpDown.Location = new System.Drawing.Point(187, 88);
        this.SumNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
        this.SumNumericUpDown.Name = "SumNumericUpDown";
        this.SumNumericUpDown.Size = new System.Drawing.Size(53, 20);
        this.SumNumericUpDown.TabIndex = 8;
        this.SumNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        this.SumNumericUpDown.Value = new decimal(new int[] {
            92,
            0,
            0,
            0});
        this.SumNumericUpDown.TextChanged += new System.EventHandler(this.SumNumericUpDown_TextChanged);
        this.SumNumericUpDown.ValueChanged += new System.EventHandler(this.SumNumericUpDown_ValueChanged);
        // 
        // SizeNumericUpDown
        // 
        this.SizeNumericUpDown.Location = new System.Drawing.Point(81, 88);
        this.SizeNumericUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
        this.SizeNumericUpDown.Name = "SizeNumericUpDown";
        this.SizeNumericUpDown.Size = new System.Drawing.Size(37, 20);
        this.SizeNumericUpDown.TabIndex = 6;
        this.SizeNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        this.SizeNumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
        this.SizeNumericUpDown.TextChanged += new System.EventHandler(this.SizeNumericUpDown_TextChanged);
        this.SizeNumericUpDown.ValueChanged += new System.EventHandler(this.SizeNumericUpDown_ValueChanged);
        // 
        // SumLabel
        // 
        this.SumLabel.AutoSize = true;
        this.SumLabel.Location = new System.Drawing.Point(123, 91);
        this.SumLabel.Name = "SumLabel";
        this.SumLabel.Size = new System.Drawing.Size(63, 13);
        this.SumLabel.TabIndex = 7;
        this.SumLabel.Text = "Word Value";
        // 
        // SizeLabel
        // 
        this.SizeLabel.AutoSize = true;
        this.SizeLabel.Location = new System.Drawing.Point(43, 91);
        this.SizeLabel.Name = "SizeLabel";
        this.SizeLabel.Size = new System.Drawing.Size(39, 13);
        this.SizeLabel.TabIndex = 5;
        this.SizeLabel.Text = "Letters";
        // 
        // ResultsListView
        // 
        this.ResultsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ResultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ResultsColumnHeader1,
            this.ResultsColumnHeader2,
            this.ResultsColumnHeader3});
        this.ResultsListView.FullRowSelect = true;
        this.ResultsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        this.ResultsListView.HideSelection = false;
        this.ResultsListView.Location = new System.Drawing.Point(3, 110);
        this.ResultsListView.MultiSelect = false;
        this.ResultsListView.Name = "ResultsListView";
        this.ResultsListView.Size = new System.Drawing.Size(456, 203);
        this.ResultsListView.TabIndex = 11;
        this.ResultsListView.UseCompatibleStateImageBehavior = false;
        this.ResultsListView.View = System.Windows.Forms.View.Details;
        // 
        // ResultsColumnHeader1
        // 
        this.ResultsColumnHeader1.Text = "#";
        this.ResultsColumnHeader1.Width = 32;
        // 
        // ResultsColumnHeader2
        // 
        this.ResultsColumnHeader2.Text = "Letters";
        this.ResultsColumnHeader2.Width = 120;
        // 
        // ResultsColumnHeader3
        // 
        this.ResultsColumnHeader3.Text = "Words";
        this.ResultsColumnHeader3.Width = 282;
        // 
        // FindWordsButton
        // 
        this.FindWordsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.FindWordsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.FindWordsButton.Location = new System.Drawing.Point(331, 86);
        this.FindWordsButton.Name = "FindWordsButton";
        this.FindWordsButton.Size = new System.Drawing.Size(129, 24);
        this.FindWordsButton.TabIndex = 10;
        this.FindWordsButton.Text = "Find";
        this.FindWordsButton.UseVisualStyleBackColor = true;
        this.FindWordsButton.Click += new System.EventHandler(this.FindWordsButton_Click);
        // 
        // AutoRunCheckBox
        // 
        this.AutoRunCheckBox.AutoSize = true;
        this.AutoRunCheckBox.BackColor = System.Drawing.Color.Transparent;
        this.AutoRunCheckBox.Location = new System.Drawing.Point(263, 91);
        this.AutoRunCheckBox.Name = "AutoRunCheckBox";
        this.AutoRunCheckBox.Size = new System.Drawing.Size(68, 17);
        this.AutoRunCheckBox.TabIndex = 9;
        this.AutoRunCheckBox.Text = "AutoRun";
        this.AutoRunCheckBox.UseVisualStyleBackColor = false;
        this.AutoRunCheckBox.CheckedChanged += new System.EventHandler(this.AutoRunCheckBox_CheckedChanged);
        // 
        // MainForm
        // 
        this.AcceptButton = this.FindWordsButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.LightSteelBlue;
        this.ClientSize = new System.Drawing.Size(462, 362);
        this.Controls.Add(this.FindWordsButton);
        this.Controls.Add(this.AutoRunCheckBox);
        this.Controls.Add(this.ResultsCountLabel);
        this.Controls.Add(this.ResultsSaveButton);
        this.Controls.Add(this.ResultsListView);
        this.Controls.Add(this.SumNumericUpDown);
        this.Controls.Add(this.SizeNumericUpDown);
        this.Controls.Add(this.SumLabel);
        this.Controls.Add(this.SizeLabel);
        this.Controls.Add(this.LoadLetterValuesButton);
        this.Controls.Add(this.NumbersTextBox);
        this.Controls.Add(this.ProgressBar);
        this.Controls.Add(this.VersionLabel);
        this.Controls.Add(this.NumericalSystemComboBox);
        this.Controls.Add(this.TextModeComboBox);
        this.Controls.Add(this.ElapsedTimeValueLabel);
        this.Controls.Add(this.ElapsedTimeLabel);
        this.Controls.Add(this.ProgressLabel);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.KeyPreview = true;
        this.MinimumSize = new System.Drawing.Size(478, 400);
        this.Name = "MainForm";
        this.ShowInTaskbar = false;
        this.Text = "Word Finder";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.Shown += new System.EventHandler(this.MainForm_Shown);
        this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
        this.Resize += new System.EventHandler(this.MainForm_Resize);
        this.NotifyIconContextMenuStrip.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SumNumericUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.SizeNumericUpDown)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ToolTip ToolTip;
    private System.Windows.Forms.ComboBox NumericalSystemComboBox;
    private System.Windows.Forms.ComboBox TextModeComboBox;
    private System.Windows.Forms.NotifyIcon NotifyIcon;
    private System.Windows.Forms.ContextMenuStrip NotifyIconContextMenuStrip;
    private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
    private System.Windows.Forms.Label VersionLabel;
    private System.Windows.Forms.ProgressBar ProgressBar;
    private System.Windows.Forms.Label ElapsedTimeLabel;
    private System.Windows.Forms.Label ElapsedTimeValueLabel;
    private System.Windows.Forms.Label ProgressLabel;
    private System.Windows.Forms.Label ResultsCountLabel;
    private System.Windows.Forms.TextBox NumbersTextBox;
    private System.Windows.Forms.Button LoadLetterValuesButton;
    private System.Windows.Forms.NumericUpDown SumNumericUpDown;
    private System.Windows.Forms.NumericUpDown SizeNumericUpDown;
    private System.Windows.Forms.Label SumLabel;
    private System.Windows.Forms.Label SizeLabel;
    private System.Windows.Forms.ListView ResultsListView;
    private System.Windows.Forms.ColumnHeader ResultsColumnHeader1;
    private System.Windows.Forms.ColumnHeader ResultsColumnHeader2;
    private System.Windows.Forms.Button FindWordsButton;
    private System.Windows.Forms.Label ResultsSaveButton;
    private System.Windows.Forms.CheckBox AutoRunCheckBox;
    private System.Windows.Forms.ColumnHeader ResultsColumnHeader3;
}
