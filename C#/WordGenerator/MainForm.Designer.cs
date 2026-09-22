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
        this.GenerateWordsButton = new System.Windows.Forms.Button();
        this.ListView = new System.Windows.Forms.ListView();
        this.NumberColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.SentenceColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.ValueColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.WordColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.AddPositionsCheckBox = new System.Windows.Forms.CheckBox();
        this.WordCountLabel = new System.Windows.Forms.Label();
        this.NumberTypeLabel = new System.Windows.Forms.Label();
        ToolTip = new System.Windows.Forms.ToolTip(this.components);
        this.ValueCombinationDirectionLabel = new System.Windows.Forms.Label();
        this.AddDistancesToPreviousCheckBox = new System.Windows.Forms.CheckBox();
        this.AddDistancesToNextCheckBox = new System.Windows.Forms.CheckBox();
        this.ValueInterlaceLabel = new System.Windows.Forms.Label();
        this.AutoGenerateWordsButton = new System.Windows.Forms.Button();
        this.AddVerseAndWordValuesCheckBox = new System.Windows.Forms.CheckBox();
        this.InspectButton = new System.Windows.Forms.Button();
        this.ProgressBar = new System.Windows.Forms.ProgressBar();
        this.NumericalSystemComboBox = new System.Windows.Forms.ComboBox();
        this.TextModeComboBox = new System.Windows.Forms.ComboBox();
        this.SuspendLayout();
        // 
        // GenerateWordsButton
        // 
        this.GenerateWordsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.GenerateWordsButton.Cursor = System.Windows.Forms.Cursors.Hand;
        this.GenerateWordsButton.Image = ((System.Drawing.Image)(resources.GetObject("GenerateWordsButton.Image")));
        this.GenerateWordsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.GenerateWordsButton.Location = new System.Drawing.Point(426, 358);
        this.GenerateWordsButton.Name = "GenerateWordsButton";
        this.GenerateWordsButton.Size = new System.Drawing.Size(110, 21);
        this.GenerateWordsButton.TabIndex = 19;
        this.GenerateWordsButton.Text = "&Generate words";
        this.GenerateWordsButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.ToolTip.SetToolTip(this.GenerateWordsButton, resources.GetString("GenerateWordsButton.ToolTip"));
        this.GenerateWordsButton.UseVisualStyleBackColor = true;
        this.GenerateWordsButton.Click += new System.EventHandler(this.GenerateWordsButton_Click);
        // 
        // ListView
        // 
        this.ListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NumberColumnHeader,
            this.SentenceColumnHeader,
            this.ValueColumnHeader,
            this.WordColumnHeader});
        this.ListView.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ListView.Location = new System.Drawing.Point(0, 0);
        this.ListView.Name = "ListView";
        this.ListView.Size = new System.Drawing.Size(665, 342);
        this.ListView.TabIndex = 1;
        this.ListView.UseCompatibleStateImageBehavior = false;
        this.ListView.View = System.Windows.Forms.View.Details;
        this.ListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListView_ColumnClick);
        // 
        // NumberColumnHeader
        // 
        this.NumberColumnHeader.Text = "# ▲";
        this.NumberColumnHeader.Width = 55;
        // 
        // SentenceColumnHeader
        // 
        this.SentenceColumnHeader.Text = "Sentence  ";
        this.SentenceColumnHeader.Width = 385;
        // 
        // ValueColumnHeader
        // 
        this.ValueColumnHeader.Text = "Value  ";
        this.ValueColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        this.ValueColumnHeader.Width = 94;
        // 
        // WordColumnHeader
        // 
        this.WordColumnHeader.Text = "Word  ";
        this.WordColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
        this.WordColumnHeader.Width = 110;
        // 
        // AddPositionsCheckBox
        // 
        this.AddPositionsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.AddPositionsCheckBox.AutoSize = true;
        this.AddPositionsCheckBox.Location = new System.Drawing.Point(4, 367);
        this.AddPositionsCheckBox.Name = "AddPositionsCheckBox";
        this.AddPositionsCheckBox.Size = new System.Drawing.Size(130, 17);
        this.AddPositionsCheckBox.TabIndex = 4;
        this.AddPositionsCheckBox.Text = "Add positions to value";
        this.ToolTip.SetToolTip(this.AddPositionsCheckBox, "Add letter, word and verse positions to each letter value");
        this.AddPositionsCheckBox.UseVisualStyleBackColor = true;
        this.AddPositionsCheckBox.CheckedChanged += new System.EventHandler(this.AddPositionsCheckBox_CheckedChanged);
        // 
        // WordCountLabel
        // 
        this.WordCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.WordCountLabel.AutoSize = true;
        this.WordCountLabel.ForeColor = System.Drawing.SystemColors.WindowText;
        this.WordCountLabel.Location = new System.Drawing.Point(537, 362);
        this.WordCountLabel.Name = "WordCountLabel";
        this.WordCountLabel.Size = new System.Drawing.Size(101, 13);
        this.WordCountLabel.TabIndex = 23;
        this.WordCountLabel.Text = "00000 (0000) words";
        this.WordCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.ToolTip.SetToolTip(this.WordCountLabel, "Quran words");
        // 
        // NumberTypeLabel
        // 
        this.NumberTypeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.NumberTypeLabel.BackColor = System.Drawing.Color.Silver;
        this.NumberTypeLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.NumberTypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.NumberTypeLabel.ForeColor = System.Drawing.Color.Green;
        this.NumberTypeLabel.Location = new System.Drawing.Point(399, 360);
        this.NumberTypeLabel.Name = "NumberTypeLabel";
        this.NumberTypeLabel.Size = new System.Drawing.Size(25, 17);
        this.NumberTypeLabel.TabIndex = 12;
        this.NumberTypeLabel.Text = "P";
        this.NumberTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.ToolTip.SetToolTip(this.NumberTypeLabel, "allow prime concatenated letter values only");
        this.NumberTypeLabel.Click += new System.EventHandler(this.NumberTypeLabel_Click);
        // 
        // ValueCombinationDirectionLabel
        // 
        this.ValueCombinationDirectionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ValueCombinationDirectionLabel.BackColor = System.Drawing.Color.Silver;
        this.ValueCombinationDirectionLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ValueCombinationDirectionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ValueCombinationDirectionLabel.ForeColor = System.Drawing.SystemColors.WindowText;
        this.ValueCombinationDirectionLabel.Location = new System.Drawing.Point(372, 360);
        this.ValueCombinationDirectionLabel.Name = "ValueCombinationDirectionLabel";
        this.ValueCombinationDirectionLabel.Size = new System.Drawing.Size(25, 17);
        this.ValueCombinationDirectionLabel.TabIndex = 11;
        this.ValueCombinationDirectionLabel.Text = "←";
        this.ValueCombinationDirectionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.ToolTip.SetToolTip(this.ValueCombinationDirectionLabel, "combine right to left");
        this.ValueCombinationDirectionLabel.Click += new System.EventHandler(this.ValueCombinationDirectionLabel_Click);
        // 
        // AddDistancesToPreviousCheckBox
        // 
        this.AddDistancesToPreviousCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.AddDistancesToPreviousCheckBox.AutoSize = true;
        this.AddDistancesToPreviousCheckBox.Location = new System.Drawing.Point(159, 353);
        this.AddDistancesToPreviousCheckBox.Name = "AddDistancesToPreviousCheckBox";
        this.AddDistancesToPreviousCheckBox.Size = new System.Drawing.Size(184, 17);
        this.AddDistancesToPreviousCheckBox.TabIndex = 5;
        this.AddDistancesToPreviousCheckBox.Text = "Add backward distances to value";
        this.ToolTip.SetToolTip(this.AddDistancesToPreviousCheckBox, "Add letter and word distances to each letter value\r\nbackward to the previous same" +
    " letter and word");
        this.AddDistancesToPreviousCheckBox.UseVisualStyleBackColor = true;
        this.AddDistancesToPreviousCheckBox.CheckedChanged += new System.EventHandler(this.AddDistancesToPreviousCheckBox_CheckedChanged);
        // 
        // AddDistancesToNextCheckBox
        // 
        this.AddDistancesToNextCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.AddDistancesToNextCheckBox.AutoSize = true;
        this.AddDistancesToNextCheckBox.Location = new System.Drawing.Point(159, 367);
        this.AddDistancesToNextCheckBox.Name = "AddDistancesToNextCheckBox";
        this.AddDistancesToNextCheckBox.Size = new System.Drawing.Size(172, 17);
        this.AddDistancesToNextCheckBox.TabIndex = 6;
        this.AddDistancesToNextCheckBox.Text = "Add forward distances to value";
        this.ToolTip.SetToolTip(this.AddDistancesToNextCheckBox, "Add letter and word distances to each letter value\r\nforward to the next same lett" +
    "er and word");
        this.AddDistancesToNextCheckBox.UseVisualStyleBackColor = true;
        this.AddDistancesToNextCheckBox.CheckedChanged += new System.EventHandler(this.AddDistancesToNextCheckBox_CheckedChanged);
        // 
        // ValueInterlaceLabel
        // 
        this.ValueInterlaceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ValueInterlaceLabel.BackColor = System.Drawing.Color.Silver;
        this.ValueInterlaceLabel.Cursor = System.Windows.Forms.Cursors.Hand;
        this.ValueInterlaceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ValueInterlaceLabel.ForeColor = System.Drawing.SystemColors.WindowText;
        this.ValueInterlaceLabel.Location = new System.Drawing.Point(343, 360);
        this.ValueInterlaceLabel.Name = "ValueInterlaceLabel";
        this.ValueInterlaceLabel.Size = new System.Drawing.Size(27, 17);
        this.ValueInterlaceLabel.TabIndex = 9;
        this.ValueInterlaceLabel.Text = "- -";
        this.ValueInterlaceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        this.ToolTip.SetToolTip(this.ValueInterlaceLabel, "concatenate letter values");
        this.ValueInterlaceLabel.Click += new System.EventHandler(this.ValueInterlaceLabel_Click);
        // 
        // AutoGenerateWordsButton
        // 
        this.AutoGenerateWordsButton.Cursor = System.Windows.Forms.Cursors.Hand;
        this.AutoGenerateWordsButton.Image = ((System.Drawing.Image)(resources.GetObject("AutoGenerateWordsButton.Image")));
        this.AutoGenerateWordsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.AutoGenerateWordsButton.Location = new System.Drawing.Point(416, 1);
        this.AutoGenerateWordsButton.Name = "AutoGenerateWordsButton";
        this.AutoGenerateWordsButton.Size = new System.Drawing.Size(25, 23);
        this.AutoGenerateWordsButton.TabIndex = 89;
        this.AutoGenerateWordsButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.ToolTip.SetToolTip(this.AutoGenerateWordsButton, resources.GetString("AutoGenerateWordsButton.ToolTip"));
        this.AutoGenerateWordsButton.UseVisualStyleBackColor = true;
        this.AutoGenerateWordsButton.Click += new System.EventHandler(this.AutoGenerateWordsButton_Click);
        // 
        // AddVerseAndWordValuesCheckBox
        // 
        this.AddVerseAndWordValuesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.AddVerseAndWordValuesCheckBox.AutoSize = true;
        this.AddVerseAndWordValuesCheckBox.Location = new System.Drawing.Point(4, 353);
        this.AddVerseAndWordValuesCheckBox.Name = "AddVerseAndWordValuesCheckBox";
        this.AddVerseAndWordValuesCheckBox.Size = new System.Drawing.Size(155, 17);
        this.AddVerseAndWordValuesCheckBox.TabIndex = 3;
        this.AddVerseAndWordValuesCheckBox.Text = "Add verse and word values";
        this.ToolTip.SetToolTip(this.AddVerseAndWordValuesCheckBox, "Add letter\'s verse and word values to each letter value");
        this.AddVerseAndWordValuesCheckBox.UseVisualStyleBackColor = true;
        this.AddVerseAndWordValuesCheckBox.CheckedChanged += new System.EventHandler(this.AddVerseAndWordValuesCheckBox_CheckedChanged);
        // 
        // InspectButton
        // 
        this.InspectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.InspectButton.Cursor = System.Windows.Forms.Cursors.Hand;
        this.InspectButton.Image = ((System.Drawing.Image)(resources.GetObject("InspectButton.Image")));
        this.InspectButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.InspectButton.Location = new System.Drawing.Point(638, 358);
        this.InspectButton.Name = "InspectButton";
        this.InspectButton.Size = new System.Drawing.Size(23, 21);
        this.InspectButton.TabIndex = 91;
        this.InspectButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.ToolTip.SetToolTip(this.InspectButton, "View");
        this.InspectButton.UseVisualStyleBackColor = true;
        this.InspectButton.Click += new System.EventHandler(this.InspectButton_Click);
        // 
        // ProgressBar
        // 
        this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ProgressBar.Location = new System.Drawing.Point(-1, 342);
        this.ProgressBar.Name = "ProgressBar";
        this.ProgressBar.Size = new System.Drawing.Size(666, 11);
        this.ProgressBar.TabIndex = 2;
        // 
        // NumericalSystemComboBox
        // 
        this.NumericalSystemComboBox.BackColor = System.Drawing.Color.MistyRose;
        this.NumericalSystemComboBox.DropDownHeight = 1024;
        this.NumericalSystemComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.NumericalSystemComboBox.FormattingEnabled = true;
        this.NumericalSystemComboBox.IntegralHeight = false;
        this.NumericalSystemComboBox.Location = new System.Drawing.Point(242, 2);
        this.NumericalSystemComboBox.Name = "NumericalSystemComboBox";
        this.NumericalSystemComboBox.Size = new System.Drawing.Size(176, 21);
        this.NumericalSystemComboBox.TabIndex = 0;
        this.NumericalSystemComboBox.SelectedIndexChanged += new System.EventHandler(this.NumericalSystemComboBox_SelectedIndexChanged);
        this.NumericalSystemComboBox.MouseHover += new System.EventHandler(this.NumericalSystemComboBox_MouseHover);
        // 
        // TextModeComboBox
        // 
        this.TextModeComboBox.BackColor = System.Drawing.Color.MistyRose;
        this.TextModeComboBox.DropDownHeight = 1024;
        this.TextModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.TextModeComboBox.FormattingEnabled = true;
        this.TextModeComboBox.IntegralHeight = false;
        this.TextModeComboBox.Items.AddRange(new object[] {
            "Simplified29"});
        this.TextModeComboBox.Location = new System.Drawing.Point(160, 2);
        this.TextModeComboBox.Name = "TextModeComboBox";
        this.TextModeComboBox.Size = new System.Drawing.Size(84, 21);
        this.TextModeComboBox.TabIndex = 90;
        this.TextModeComboBox.SelectedIndexChanged += new System.EventHandler(this.TextModeComboBox_SelectedIndexChanged);
        // 
        // MainForm
        // 
        this.AcceptButton = this.GenerateWordsButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(664, 382);
        this.Controls.Add(this.InspectButton);
        this.Controls.Add(this.AutoGenerateWordsButton);
        this.Controls.Add(this.NumericalSystemComboBox);
        this.Controls.Add(this.TextModeComboBox);
        this.Controls.Add(this.ValueInterlaceLabel);
        this.Controls.Add(this.AddDistancesToNextCheckBox);
        this.Controls.Add(this.AddPositionsCheckBox);
        this.Controls.Add(this.AddDistancesToPreviousCheckBox);
        this.Controls.Add(this.NumberTypeLabel);
        this.Controls.Add(this.ValueCombinationDirectionLabel);
        this.Controls.Add(this.AddVerseAndWordValuesCheckBox);
        this.Controls.Add(this.ProgressBar);
        this.Controls.Add(this.GenerateWordsButton);
        this.Controls.Add(this.ListView);
        this.Controls.Add(this.WordCountLabel);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximumSize = new System.Drawing.Size(680, 1599);
        this.MinimumSize = new System.Drawing.Size(680, 419);
        this.Name = "MainForm";
        this.Text = "WordGenerator | Primalogy value of أُمُّ ٱلْكِتَٰبِ = letters+diacritics of سورة " +
"لفاتحة";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button GenerateWordsButton;
    private System.Windows.Forms.ListView ListView;
    private System.Windows.Forms.ColumnHeader SentenceColumnHeader;
    private System.Windows.Forms.ColumnHeader WordColumnHeader;
    private System.Windows.Forms.ColumnHeader ValueColumnHeader;
    private System.Windows.Forms.CheckBox AddPositionsCheckBox;
    private System.Windows.Forms.Label WordCountLabel;
    private System.Windows.Forms.Label NumberTypeLabel;
    private System.Windows.Forms.ToolTip ToolTip;
    private System.Windows.Forms.Label ValueCombinationDirectionLabel;
    private System.Windows.Forms.ColumnHeader NumberColumnHeader;
    private System.Windows.Forms.CheckBox AddDistancesToPreviousCheckBox;
    private System.Windows.Forms.ProgressBar ProgressBar;
    private System.Windows.Forms.CheckBox AddDistancesToNextCheckBox;
    private System.Windows.Forms.ComboBox NumericalSystemComboBox;
    private System.Windows.Forms.Label ValueInterlaceLabel;
    private System.Windows.Forms.Button AutoGenerateWordsButton;
    private System.Windows.Forms.CheckBox AddVerseAndWordValuesCheckBox;
    private System.Windows.Forms.ComboBox TextModeComboBox;
    private System.Windows.Forms.Button InspectButton;
}
