namespace Divisibility
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.NumberTextBox = new System.Windows.Forms.TextBox();
            this.PrimeTextBox = new System.Windows.Forms.TextBox();
            this.PrimeMultiplierTextBox = new System.Windows.Forms.TextBox();
            this.CalculationTextBox = new System.Windows.Forms.TextBox();
            this.UnitMultiplierTextBox = new System.Windows.Forms.TextBox();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.DivisionLabel = new System.Windows.Forms.Label();
            this.UnitMultiplierNoteLabel = new System.Windows.Forms.Label();
            this.PrimeMultiplierNoteLabel = new System.Windows.Forms.Label();
            this.CalculationNoteLabel = new System.Windows.Forms.Label();
            this.PrimeMultiplierLabel = new System.Windows.Forms.Label();
            this.UnitMultiplierLabel = new System.Windows.Forms.Label();
            this.ModifiedPrimeTextBox = new System.Windows.Forms.TextBox();
            this.ModifiedPrimeNoteLabel = new System.Windows.Forms.Label();
            this.CopyrightLabel = new System.Windows.Forms.Label();
            this.DeveloperLabel = new System.Windows.Forms.Label();
            this.StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // NumberTextBox
            // 
            this.NumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumberTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.NumberTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.NumberTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NumberTextBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.NumberTextBox.Location = new System.Drawing.Point(7, 7);
            this.NumberTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.NumberTextBox.Name = "NumberTextBox";
            this.NumberTextBox.Size = new System.Drawing.Size(163, 16);
            this.NumberTextBox.TabIndex = 1;
            this.NumberTextBox.Text = "Number";
            this.NumberTextBox.TextChanged += new System.EventHandler(this.NumberTextBox_TextChanged);
            this.NumberTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumberTextBox_KeyPress);
            this.NumberTextBox.Leave += new System.EventHandler(this.NumberTextBox_Leave);
            // 
            // PrimeTextBox
            // 
            this.PrimeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PrimeTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.PrimeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PrimeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PrimeTextBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.PrimeTextBox.Location = new System.Drawing.Point(201, 7);
            this.PrimeTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.PrimeTextBox.Name = "PrimeTextBox";
            this.PrimeTextBox.Size = new System.Drawing.Size(76, 16);
            this.PrimeTextBox.TabIndex = 2;
            this.PrimeTextBox.Text = "Prime";
            this.PrimeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PrimeTextBox.TextChanged += new System.EventHandler(this.PrimeTextBox_TextChanged);
            this.PrimeTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PrimeTextBox_KeyPress);
            this.PrimeTextBox.Leave += new System.EventHandler(this.PrimeTextBox_Leave);
            // 
            // PrimeMultiplierTextBox
            // 
            this.PrimeMultiplierTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PrimeMultiplierTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.PrimeMultiplierTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PrimeMultiplierTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PrimeMultiplierTextBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.PrimeMultiplierTextBox.Location = new System.Drawing.Point(201, 36);
            this.PrimeMultiplierTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.PrimeMultiplierTextBox.Name = "PrimeMultiplierTextBox";
            this.PrimeMultiplierTextBox.ReadOnly = true;
            this.PrimeMultiplierTextBox.Size = new System.Drawing.Size(76, 16);
            this.PrimeMultiplierTextBox.TabIndex = 3;
            this.PrimeMultiplierTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // CalculationTextBox
            // 
            this.CalculationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CalculationTextBox.BackColor = System.Drawing.Color.LightSlateGray;
            this.CalculationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CalculationTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CalculationTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.CalculationTextBox.Location = new System.Drawing.Point(7, 185);
            this.CalculationTextBox.Multiline = true;
            this.CalculationTextBox.Name = "CalculationTextBox";
            this.CalculationTextBox.ReadOnly = true;
            this.CalculationTextBox.Size = new System.Drawing.Size(272, 250);
            this.CalculationTextBox.TabIndex = 5;
            this.CalculationTextBox.Text = "Is Number divisible by Prime?";
            this.CalculationTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // UnitMultiplierTextBox
            // 
            this.UnitMultiplierTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UnitMultiplierTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.UnitMultiplierTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UnitMultiplierTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UnitMultiplierTextBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.UnitMultiplierTextBox.Location = new System.Drawing.Point(201, 91);
            this.UnitMultiplierTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.UnitMultiplierTextBox.Name = "UnitMultiplierTextBox";
            this.UnitMultiplierTextBox.ReadOnly = true;
            this.UnitMultiplierTextBox.Size = new System.Drawing.Size(76, 16);
            this.UnitMultiplierTextBox.TabIndex = 4;
            this.UnitMultiplierTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // StatusStrip
            // 
            this.StatusStrip.BackColor = System.Drawing.SystemColors.Control;
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 438);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(284, 22);
            this.StatusStrip.TabIndex = 99;
            this.StatusStrip.Tag = "";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // DivisionLabel
            // 
            this.DivisionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DivisionLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DivisionLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.DivisionLabel.Location = new System.Drawing.Point(178, 8);
            this.DivisionLabel.Name = "DivisionLabel";
            this.DivisionLabel.Size = new System.Drawing.Size(12, 13);
            this.DivisionLabel.TabIndex = 0;
            this.DivisionLabel.Text = "/";
            // 
            // UnitMultiplierNoteLabel
            // 
            this.UnitMultiplierNoteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UnitMultiplierNoteLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UnitMultiplierNoteLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.UnitMultiplierNoteLabel.Location = new System.Drawing.Point(7, 84);
            this.UnitMultiplierNoteLabel.Name = "UnitMultiplierNoteLabel";
            this.UnitMultiplierNoteLabel.Size = new System.Drawing.Size(165, 30);
            this.UnitMultiplierNoteLabel.TabIndex = 0;
            this.UnitMultiplierNoteLabel.Text = "if r = 1, x = -fff\r\nif r = 9, x = fff + 1";
            this.UnitMultiplierNoteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PrimeMultiplierNoteLabel
            // 
            this.PrimeMultiplierNoteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PrimeMultiplierNoteLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PrimeMultiplierNoteLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.PrimeMultiplierNoteLabel.Location = new System.Drawing.Point(7, 32);
            this.PrimeMultiplierNoteLabel.Name = "PrimeMultiplierNoteLabel";
            this.PrimeMultiplierNoteLabel.Size = new System.Drawing.Size(158, 46);
            this.PrimeMultiplierNoteLabel.TabIndex = 0;
            this.PrimeMultiplierNoteLabel.Text = "multiplier to end\r\nPrime in 1 or 9\r\nm * Prime =";
            this.PrimeMultiplierNoteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CalculationNoteLabel
            // 
            this.CalculationNoteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CalculationNoteLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CalculationNoteLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.CalculationNoteLabel.Location = new System.Drawing.Point(7, 122);
            this.CalculationNoteLabel.Name = "CalculationNoteLabel";
            this.CalculationNoteLabel.Size = new System.Drawing.Size(178, 59);
            this.CalculationNoteLabel.TabIndex = 100;
            this.CalculationNoteLabel.Text = "Number = FFFR\r\nloop until\r\n  FFFR = FFF + (R * x)\r\nis divisible or not";
            this.CalculationNoteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PrimeMultiplierLabel
            // 
            this.PrimeMultiplierLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PrimeMultiplierLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PrimeMultiplierLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.PrimeMultiplierLabel.Location = new System.Drawing.Point(183, 35);
            this.PrimeMultiplierLabel.Name = "PrimeMultiplierLabel";
            this.PrimeMultiplierLabel.Size = new System.Drawing.Size(12, 13);
            this.PrimeMultiplierLabel.TabIndex = 0;
            this.PrimeMultiplierLabel.Text = "m";
            // 
            // UnitMultiplierLabel
            // 
            this.UnitMultiplierLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UnitMultiplierLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UnitMultiplierLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.UnitMultiplierLabel.Location = new System.Drawing.Point(183, 90);
            this.UnitMultiplierLabel.Name = "UnitMultiplierLabel";
            this.UnitMultiplierLabel.Size = new System.Drawing.Size(12, 13);
            this.UnitMultiplierLabel.TabIndex = 0;
            this.UnitMultiplierLabel.Text = "x";
            // 
            // ModifiedPrimeTextBox
            // 
            this.ModifiedPrimeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ModifiedPrimeTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ModifiedPrimeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ModifiedPrimeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModifiedPrimeTextBox.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.ModifiedPrimeTextBox.Location = new System.Drawing.Point(201, 60);
            this.ModifiedPrimeTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.ModifiedPrimeTextBox.Name = "ModifiedPrimeTextBox";
            this.ModifiedPrimeTextBox.ReadOnly = true;
            this.ModifiedPrimeTextBox.Size = new System.Drawing.Size(76, 16);
            this.ModifiedPrimeTextBox.TabIndex = 101;
            this.ModifiedPrimeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ModifiedPrimeNoteLabel
            // 
            this.ModifiedPrimeNoteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ModifiedPrimeNoteLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModifiedPrimeNoteLabel.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ModifiedPrimeNoteLabel.Location = new System.Drawing.Point(162, 61);
            this.ModifiedPrimeNoteLabel.Name = "ModifiedPrimeNoteLabel";
            this.ModifiedPrimeNoteLabel.Size = new System.Drawing.Size(35, 13);
            this.ModifiedPrimeNoteLabel.TabIndex = 102;
            this.ModifiedPrimeNoteLabel.Text = "fffr";
            // 
            // CopyrightLabel
            // 
            this.CopyrightLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyrightLabel.BackColor = System.Drawing.SystemColors.Control;
            this.CopyrightLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CopyrightLabel.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CopyrightLabel.ForeColor = System.Drawing.Color.LightSteelBlue;
            this.CopyrightLabel.Location = new System.Drawing.Point(201, 125);
            this.CopyrightLabel.Name = "CopyrightLabel";
            this.CopyrightLabel.Size = new System.Drawing.Size(76, 37);
            this.CopyrightLabel.TabIndex = 103;
            this.CopyrightLabel.Tag = "https://savory.de/maths1.htm";
            this.CopyrightLabel.Text = "Stu\r\nSavory";
            this.CopyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.CopyrightLabel.Click += new System.EventHandler(this.LinkLabel_Click);
            // 
            // DeveloperLabel
            // 
            this.DeveloperLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeveloperLabel.BackColor = System.Drawing.SystemColors.Control;
            this.DeveloperLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DeveloperLabel.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeveloperLabel.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.DeveloperLabel.Location = new System.Drawing.Point(201, 162);
            this.DeveloperLabel.Name = "DeveloperLabel";
            this.DeveloperLabel.Size = new System.Drawing.Size(76, 18);
            this.DeveloperLabel.TabIndex = 104;
            this.DeveloperLabel.Tag = "http://heliwave.com";
            this.DeveloperLabel.Text = "Ali Adams";
            this.DeveloperLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DeveloperLabel.Click += new System.EventHandler(this.LinkLabel_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(284, 460);
            this.Controls.Add(this.DeveloperLabel);
            this.Controls.Add(this.CopyrightLabel);
            this.Controls.Add(this.ModifiedPrimeNoteLabel);
            this.Controls.Add(this.ModifiedPrimeTextBox);
            this.Controls.Add(this.UnitMultiplierLabel);
            this.Controls.Add(this.PrimeMultiplierLabel);
            this.Controls.Add(this.CalculationNoteLabel);
            this.Controls.Add(this.UnitMultiplierTextBox);
            this.Controls.Add(this.PrimeMultiplierTextBox);
            this.Controls.Add(this.PrimeMultiplierNoteLabel);
            this.Controls.Add(this.UnitMultiplierNoteLabel);
            this.Controls.Add(this.DivisionLabel);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.CalculationTextBox);
            this.Controls.Add(this.PrimeTextBox);
            this.Controls.Add(this.NumberTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Divisibility Rules";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox NumberTextBox;
        private System.Windows.Forms.TextBox PrimeTextBox;
        private System.Windows.Forms.TextBox PrimeMultiplierTextBox;
        private System.Windows.Forms.TextBox CalculationTextBox;
        private System.Windows.Forms.TextBox UnitMultiplierTextBox;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.Label DivisionLabel;
        private System.Windows.Forms.Label UnitMultiplierNoteLabel;
        private System.Windows.Forms.Label PrimeMultiplierNoteLabel;
        private System.Windows.Forms.Label CalculationNoteLabel;
        private System.Windows.Forms.Label PrimeMultiplierLabel;
        private System.Windows.Forms.Label UnitMultiplierLabel;
        private System.Windows.Forms.TextBox ModifiedPrimeTextBox;
        private System.Windows.Forms.Label ModifiedPrimeNoteLabel;
        private System.Windows.Forms.Label DeveloperLabel;
        private System.Windows.Forms.Label CopyrightLabel;
    }
}

