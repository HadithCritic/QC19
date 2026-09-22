namespace InitialLetters
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
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.InitialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UniqueLettersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UniqueWordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AllWordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InitialLettersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Timer = new System.Windows.Forms.Timer(this.components);
            this.ListView = new System.Windows.Forms.ListView();
            this.ColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.LettersTextBox = new System.Windows.Forms.TextBox();
            this.ResultPanel = new System.Windows.Forms.Panel();
            this.GenerateSentencesDuplicateLettersCheckBox = new System.Windows.Forms.CheckBox();
            this.ResultLabel = new System.Windows.Forms.Label();
            this.ElapsedTimeLabel = new System.Windows.Forms.Label();
            ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.MenuStrip.SuspendLayout();
            this.ResultPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MenuStrip
            // 
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InitialsToolStripMenuItem,
            this.RunToolStripMenuItem,
            this.OpenToolStripMenuItem,
            this.HelpToolStripMenuItem});
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(242, 24);
            this.MenuStrip.TabIndex = 0;
            this.MenuStrip.Text = "MenuStrip";
            // 
            // InitialsToolStripMenuItem
            // 
            this.InitialsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UniqueLettersToolStripMenuItem,
            this.UniqueWordsToolStripMenuItem,
            this.AllWordsToolStripMenuItem});
            this.InitialsToolStripMenuItem.Name = "InitialsToolStripMenuItem";
            this.InitialsToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.InitialsToolStripMenuItem.Text = "&Initials";
            // 
            // UniqueLettersToolStripMenuItem
            // 
            this.UniqueLettersToolStripMenuItem.Name = "UniqueLettersToolStripMenuItem";
            this.UniqueLettersToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.UniqueLettersToolStripMenuItem.Text = "Unique &Letters";
            this.UniqueLettersToolStripMenuItem.Click += new System.EventHandler(this.InitialsUniqueLettersToolStripMenuItem_Click);
            // 
            // UniqueWordsToolStripMenuItem
            // 
            this.UniqueWordsToolStripMenuItem.Name = "UniqueWordsToolStripMenuItem";
            this.UniqueWordsToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.UniqueWordsToolStripMenuItem.Text = "Unique &Words";
            this.UniqueWordsToolStripMenuItem.Click += new System.EventHandler(this.InitialsUniqueWordsToolStripMenuItem_Click);
            // 
            // AllWordsToolStripMenuItem
            // 
            this.AllWordsToolStripMenuItem.Name = "AllWordsToolStripMenuItem";
            this.AllWordsToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.AllWordsToolStripMenuItem.Text = "&All Words";
            this.AllWordsToolStripMenuItem.Click += new System.EventHandler(this.InitialsAllWordsToolStripMenuItem_Click);
            // 
            // RunToolStripMenuItem
            // 
            this.RunToolStripMenuItem.Name = "RunToolStripMenuItem";
            this.RunToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.RunToolStripMenuItem.Text = "&Run";
            this.RunToolStripMenuItem.ToolTipText = "Generate sentences";
            this.RunToolStripMenuItem.Click += new System.EventHandler(this.RunToolStripMenuItem_Click);
            // 
            // OpenToolStripMenuItem
            // 
            this.OpenToolStripMenuItem.Enabled = false;
            this.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
            this.OpenToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.OpenToolStripMenuItem.Text = "&Open";
            this.OpenToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // HelpToolStripMenuItem
            // 
            this.HelpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InitialLettersToolStripMenuItem,
            this.ToolStripSeparator1,
            this.AboutToolStripMenuItem});
            this.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem";
            this.HelpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.HelpToolStripMenuItem.Text = "&Help";
            // 
            // InitialLettersToolStripMenuItem
            // 
            this.InitialLettersToolStripMenuItem.Name = "InitialLettersToolStripMenuItem";
            this.InitialLettersToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.InitialLettersToolStripMenuItem.Text = "&Initial Letters";
            this.InitialLettersToolStripMenuItem.Click += new System.EventHandler(this.InitialLettersToolStripMenuItem_Click);
            // 
            // ToolStripSeparator1
            // 
            this.ToolStripSeparator1.Name = "ToolStripSeparator1";
            this.ToolStripSeparator1.Size = new System.Drawing.Size(138, 6);
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold); this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.AboutToolStripMenuItem.Text = "&About...";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // Timer
            // 
            this.Timer.Interval = 1000;
            this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // ListView
            // 
            this.ListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader});
            this.ListView.Font = new System.Drawing.Font("Lucida Console", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListView.Location = new System.Drawing.Point(0, 24);
            this.ListView.Name = "ListView";
            this.ListView.Size = new System.Drawing.Size(242, 301);
            this.ListView.TabIndex = 1;
            this.ListView.TabStop = false;
            this.ListView.UseCompatibleStateImageBehavior = false;
            this.ListView.View = System.Windows.Forms.View.Details;
            this.ListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_SortColumnClick);
            this.ListView.SelectedIndexChanged += new System.EventHandler(this.ListView_SelectedIndexChanged);
            this.ListView.Resize += new System.EventHandler(this.ListView_Resize);
            // 
            // ColumnHeader
            // 
            this.ColumnHeader.Text = "Sentences";
            this.ColumnHeader.Width = 220;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ProgressBar.Location = new System.Drawing.Point(0, 370);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(242, 12);
            this.ProgressBar.Step = 1;
            this.ProgressBar.TabIndex = 0;
            this.ProgressBar.Tag = "http://heliwave.com";
            this.ToolTip.SetToolTip(this.ProgressBar, "Ali");
            this.ProgressBar.Click += new System.EventHandler(this.LinkLabel_Click);
            this.ProgressBar.MouseHover += new System.EventHandler(this.ProgressBar_MouseHover);
            // 
            // LettersTextBox
            // 
            this.LettersTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LettersTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LettersTextBox.Location = new System.Drawing.Point(0, 347);
            this.LettersTextBox.Name = "LettersTextBox";
            this.LettersTextBox.Size = new System.Drawing.Size(242, 23);
            this.LettersTextBox.TabIndex = 3;
            this.LettersTextBox.TextChanged += new System.EventHandler(this.LettersTextBox_TextChanged);
            this.LettersTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LettersTextBox_KeyPress);
            // 
            // ResultPanel
            // 
            this.ResultPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResultPanel.Controls.Add(this.GenerateSentencesDuplicateLettersCheckBox);
            this.ResultPanel.Controls.Add(this.ResultLabel);
            this.ResultPanel.Controls.Add(this.ElapsedTimeLabel);
            this.ResultPanel.Location = new System.Drawing.Point(0, 325);
            this.ResultPanel.Name = "ResultPanel";
            this.ResultPanel.Size = new System.Drawing.Size(242, 21);
            this.ResultPanel.TabIndex = 20;
            // 
            // GenerateSentencesDuplicateLettersCheckBox
            // 
            this.GenerateSentencesDuplicateLettersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GenerateSentencesDuplicateLettersCheckBox.AutoSize = true;
            this.GenerateSentencesDuplicateLettersCheckBox.Checked = true;
            this.GenerateSentencesDuplicateLettersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GenerateSentencesDuplicateLettersCheckBox.Location = new System.Drawing.Point(119, 3);
            this.GenerateSentencesDuplicateLettersCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.GenerateSentencesDuplicateLettersCheckBox.Name = "GenerateSentencesDuplicateLettersCheckBox";
            this.GenerateSentencesDuplicateLettersCheckBox.Size = new System.Drawing.Size(74, 17);
            this.GenerateSentencesDuplicateLettersCheckBox.TabIndex = 2;
            this.GenerateSentencesDuplicateLettersCheckBox.Text = "duplicates";
            this.GenerateSentencesDuplicateLettersCheckBox.UseVisualStyleBackColor = true;
            this.GenerateSentencesDuplicateLettersCheckBox.CheckedChanged += new System.EventHandler(this.GenerateSentencesDuplicateLettersCheckBox_CheckedChanged);
            // 
            // ResultLabel
            // 
            this.ResultLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ResultLabel.AutoSize = true;
            this.ResultLabel.Location = new System.Drawing.Point(1, 4);
            this.ResultLabel.Name = "ResultLabel";
            this.ResultLabel.Size = new System.Drawing.Size(30, 13);
            this.ResultLabel.TabIndex = 0;
            this.ResultLabel.Text = "0 / 0";
            // 
            // ElapsedTimeLabel
            // 
            this.ElapsedTimeLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.ElapsedTimeLabel.AutoSize = true;
            this.ElapsedTimeLabel.Location = new System.Drawing.Point(192, 4);
            this.ElapsedTimeLabel.Name = "ElapsedTimeLabel";
            this.ElapsedTimeLabel.Size = new System.Drawing.Size(49, 13);
            this.ElapsedTimeLabel.TabIndex = 0;
            this.ElapsedTimeLabel.Text = "00:00:00";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(242, 382);
            this.Controls.Add(this.ResultPanel);
            this.Controls.Add(this.LettersTextBox);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.ListView);
            this.Controls.Add(this.MenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.MenuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Initial Letters";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.ResultPanel.ResumeLayout(false);
            this.ResultPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem HelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
        private System.Windows.Forms.Timer Timer;
        private System.Windows.Forms.ToolStripMenuItem InitialsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem UniqueLettersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem UniqueWordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AllWordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RunToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OpenToolStripMenuItem;
        private System.Windows.Forms.ListView ListView;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.TextBox LettersTextBox;
        private System.Windows.Forms.Panel ResultPanel;
        private System.Windows.Forms.Label ResultLabel;
        private System.Windows.Forms.Label ElapsedTimeLabel;
        private System.Windows.Forms.ToolStripMenuItem InitialLettersToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
        private System.Windows.Forms.ColumnHeader ColumnHeader;
        private System.Windows.Forms.CheckBox GenerateSentencesDuplicateLettersCheckBox;
        private System.Windows.Forms.ToolTip ToolTip;
    }
}
