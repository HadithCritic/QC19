namespace DayOfWeek
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
            this.Year1TextBox = new System.Windows.Forms.TextBox();
            this.Month1TextBox = new System.Windows.Forms.TextBox();
            this.Day1TextBox = new System.Windows.Forms.TextBox();
            this.Weekday1TextBox = new System.Windows.Forms.TextBox();
            this.Date1Label = new System.Windows.Forms.Label();
            this.Date2Label = new System.Windows.Forms.Label();
            this.Weekday2TextBox = new System.Windows.Forms.TextBox();
            this.Day2TextBox = new System.Windows.Forms.TextBox();
            this.Month2TextBox = new System.Windows.Forms.TextBox();
            this.Year2TextBox = new System.Windows.Forms.TextBox();
            this.DaysTextBox = new System.Windows.Forms.TextBox();
            this.DaysLabel = new System.Windows.Forms.Label();
            this.LeapYearsLabel = new System.Windows.Forms.Label();
            this.LeapYearsTextBox = new System.Windows.Forms.TextBox();
            this.Years53Label = new System.Windows.Forms.Label();
            this.Years53TextBox = new System.Windows.Forms.TextBox();
            this.Years53ComboBox = new System.Windows.Forms.ComboBox();
            this.DifferenceLabel = new System.Windows.Forms.Label();
            this.DayDiffTextBox = new System.Windows.Forms.TextBox();
            this.MonthDiffTextBox = new System.Windows.Forms.TextBox();
            this.YearDiffTextBox = new System.Windows.Forms.TextBox();
            this.YearsMonthsDaysLabel = new System.Windows.Forms.Label();
            this.StatusPanel = new System.Windows.Forms.Panel();
            this.OffsetNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.TagLinkLabel = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.Weekday0TextBox = new System.Windows.Forms.TextBox();
            this.Day0TextBox = new System.Windows.Forms.TextBox();
            this.Month0TextBox = new System.Windows.Forms.TextBox();
            this.Year0TextBox = new System.Windows.Forms.TextBox();
            this.TodayLabel = new System.Windows.Forms.Label();
            this.Years53ListBox = new System.Windows.Forms.ListBox();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.LeapYearRulesButton = new System.Windows.Forms.Button();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.RamanujanLabel = new System.Windows.Forms.Label();
            this.WeekdaysTextBox = new System.Windows.Forms.TextBox();
            this.WeeksTextBox = new System.Windows.Forms.TextBox();
            this.WeeksLabel = new System.Windows.Forms.Label();
            this.WeekdaysLabel = new System.Windows.Forms.Label();
            this.StatusPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OffsetNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // Year1TextBox
            // 
            this.Year1TextBox.BackColor = System.Drawing.Color.LavenderBlush;
            this.Year1TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Year1TextBox.Location = new System.Drawing.Point(83, 33);
            this.Year1TextBox.MaxLength = 4;
            this.Year1TextBox.Name = "Year1TextBox";
            this.Year1TextBox.Size = new System.Drawing.Size(48, 26);
            this.Year1TextBox.TabIndex = 4;
            this.Year1TextBox.Text = "00001";
            this.Year1TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Year1TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.YearTextBox_KeyDown);
            this.Year1TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.Year1TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // Month1TextBox
            // 
            this.Month1TextBox.BackColor = System.Drawing.Color.LavenderBlush;
            this.Month1TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Month1TextBox.Location = new System.Drawing.Point(132, 33);
            this.Month1TextBox.MaxLength = 2;
            this.Month1TextBox.Name = "Month1TextBox";
            this.Month1TextBox.Size = new System.Drawing.Size(28, 26);
            this.Month1TextBox.TabIndex = 5;
            this.Month1TextBox.Text = "01";
            this.Month1TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Month1TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MonthTextBox_KeyDown);
            this.Month1TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.Month1TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // Day1TextBox
            // 
            this.Day1TextBox.BackColor = System.Drawing.Color.LavenderBlush;
            this.Day1TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Day1TextBox.Location = new System.Drawing.Point(161, 33);
            this.Day1TextBox.MaxLength = 2;
            this.Day1TextBox.Name = "Day1TextBox";
            this.Day1TextBox.Size = new System.Drawing.Size(28, 26);
            this.Day1TextBox.TabIndex = 6;
            this.Day1TextBox.Text = "01";
            this.Day1TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Day1TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DayTextBox_KeyDown);
            this.Day1TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.Day1TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // Weekday1TextBox
            // 
            this.Weekday1TextBox.BackColor = System.Drawing.SystemColors.Control;
            this.Weekday1TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Weekday1TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Weekday1TextBox.Location = new System.Drawing.Point(198, 36);
            this.Weekday1TextBox.Name = "Weekday1TextBox";
            this.Weekday1TextBox.ReadOnly = true;
            this.Weekday1TextBox.Size = new System.Drawing.Size(90, 19);
            this.Weekday1TextBox.TabIndex = 7;
            this.Weekday1TextBox.TabStop = false;
            this.Weekday1TextBox.Text = "Wednesday";
            // 
            // Date1Label
            // 
            this.Date1Label.Location = new System.Drawing.Point(5, 34);
            this.Date1Label.Name = "Date1Label";
            this.Date1Label.Size = new System.Drawing.Size(72, 23);
            this.Date1Label.TabIndex = 0;
            this.Date1Label.Text = "From";
            this.Date1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Date2Label
            // 
            this.Date2Label.Location = new System.Drawing.Point(5, 62);
            this.Date2Label.Name = "Date2Label";
            this.Date2Label.Size = new System.Drawing.Size(72, 23);
            this.Date2Label.TabIndex = 0;
            this.Date2Label.Text = "To";
            this.Date2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Weekday2TextBox
            // 
            this.Weekday2TextBox.BackColor = System.Drawing.SystemColors.Control;
            this.Weekday2TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Weekday2TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Weekday2TextBox.Location = new System.Drawing.Point(198, 64);
            this.Weekday2TextBox.Name = "Weekday2TextBox";
            this.Weekday2TextBox.ReadOnly = true;
            this.Weekday2TextBox.Size = new System.Drawing.Size(90, 19);
            this.Weekday2TextBox.TabIndex = 11;
            this.Weekday2TextBox.TabStop = false;
            this.Weekday2TextBox.Text = "Wednesday";
            // 
            // Day2TextBox
            // 
            this.Day2TextBox.BackColor = System.Drawing.Color.LavenderBlush;
            this.Day2TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Day2TextBox.Location = new System.Drawing.Point(161, 61);
            this.Day2TextBox.MaxLength = 2;
            this.Day2TextBox.Name = "Day2TextBox";
            this.Day2TextBox.Size = new System.Drawing.Size(28, 26);
            this.Day2TextBox.TabIndex = 10;
            this.Day2TextBox.Text = "31";
            this.Day2TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Day2TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DayTextBox_KeyDown);
            this.Day2TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.Day2TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // Month2TextBox
            // 
            this.Month2TextBox.BackColor = System.Drawing.Color.LavenderBlush;
            this.Month2TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Month2TextBox.Location = new System.Drawing.Point(132, 61);
            this.Month2TextBox.MaxLength = 2;
            this.Month2TextBox.Name = "Month2TextBox";
            this.Month2TextBox.Size = new System.Drawing.Size(28, 26);
            this.Month2TextBox.TabIndex = 9;
            this.Month2TextBox.Text = "12";
            this.Month2TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Month2TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MonthTextBox_KeyDown);
            this.Month2TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.Month2TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // Year2TextBox
            // 
            this.Year2TextBox.BackColor = System.Drawing.Color.LavenderBlush;
            this.Year2TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Year2TextBox.Location = new System.Drawing.Point(83, 61);
            this.Year2TextBox.MaxLength = 4;
            this.Year2TextBox.Name = "Year2TextBox";
            this.Year2TextBox.Size = new System.Drawing.Size(48, 26);
            this.Year2TextBox.TabIndex = 8;
            this.Year2TextBox.Text = "9999";
            this.Year2TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Year2TextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.YearTextBox_KeyDown);
            this.Year2TextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_KeyPress);
            this.Year2TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // DaysTextBox
            // 
            this.DaysTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.DaysTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DaysTextBox.Location = new System.Drawing.Point(83, 119);
            this.DaysTextBox.Name = "DaysTextBox";
            this.DaysTextBox.ReadOnly = true;
            this.DaysTextBox.Size = new System.Drawing.Size(106, 26);
            this.DaysTextBox.TabIndex = 14;
            this.DaysTextBox.Text = "0";
            this.DaysTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // DaysLabel
            // 
            this.DaysLabel.Location = new System.Drawing.Point(5, 120);
            this.DaysLabel.Name = "DaysLabel";
            this.DaysLabel.Size = new System.Drawing.Size(72, 23);
            this.DaysLabel.TabIndex = 0;
            this.DaysLabel.Text = "Days";
            this.DaysLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LeapYearsLabel
            // 
            this.LeapYearsLabel.Location = new System.Drawing.Point(5, 91);
            this.LeapYearsLabel.Name = "LeapYearsLabel";
            this.LeapYearsLabel.Size = new System.Drawing.Size(72, 23);
            this.LeapYearsLabel.TabIndex = 0;
            this.LeapYearsLabel.Text = "Leap years";
            this.LeapYearsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LeapYearsTextBox
            // 
            this.LeapYearsTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.LeapYearsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LeapYearsTextBox.Location = new System.Drawing.Point(83, 90);
            this.LeapYearsTextBox.Name = "LeapYearsTextBox";
            this.LeapYearsTextBox.ReadOnly = true;
            this.LeapYearsTextBox.Size = new System.Drawing.Size(106, 26);
            this.LeapYearsTextBox.TabIndex = 12;
            this.LeapYearsTextBox.Text = "0";
            this.LeapYearsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Years53Label
            // 
            this.Years53Label.Location = new System.Drawing.Point(5, 6);
            this.Years53Label.Name = "Years53Label";
            this.Years53Label.Size = new System.Drawing.Size(72, 23);
            this.Years53Label.TabIndex = 0;
            this.Years53Label.Text = "Years with 53";
            this.Years53Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Years53TextBox
            // 
            this.Years53TextBox.BackColor = System.Drawing.Color.LavenderBlush;
            this.Years53TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Years53TextBox.Location = new System.Drawing.Point(306, 33);
            this.Years53TextBox.Name = "Years53TextBox";
            this.Years53TextBox.ReadOnly = true;
            this.Years53TextBox.Size = new System.Drawing.Size(53, 26);
            this.Years53TextBox.TabIndex = 11;
            this.Years53TextBox.Text = "0";
            this.Years53TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Years53ComboBox
            // 
            this.Years53ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Years53ComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Years53ComboBox.FormattingEnabled = true;
            this.Years53ComboBox.Location = new System.Drawing.Point(83, 4);
            this.Years53ComboBox.Name = "Years53ComboBox";
            this.Years53ComboBox.Size = new System.Drawing.Size(276, 26);
            this.Years53ComboBox.TabIndex = 1;
            this.Years53ComboBox.SelectedIndexChanged += new System.EventHandler(this.Years53ComboBox_SelectedIndexChanged);
            // 
            // DifferenceLabel
            // 
            this.DifferenceLabel.Location = new System.Drawing.Point(5, 207);
            this.DifferenceLabel.Name = "DifferenceLabel";
            this.DifferenceLabel.Size = new System.Drawing.Size(72, 23);
            this.DifferenceLabel.TabIndex = 0;
            this.DifferenceLabel.Text = "Difference";
            this.DifferenceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DayDiffTextBox
            // 
            this.DayDiffTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.DayDiffTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DayDiffTextBox.Location = new System.Drawing.Point(161, 206);
            this.DayDiffTextBox.Name = "DayDiffTextBox";
            this.DayDiffTextBox.ReadOnly = true;
            this.DayDiffTextBox.Size = new System.Drawing.Size(28, 26);
            this.DayDiffTextBox.TabIndex = 19;
            this.DayDiffTextBox.Text = "0";
            this.DayDiffTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // MonthDiffTextBox
            // 
            this.MonthDiffTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.MonthDiffTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MonthDiffTextBox.Location = new System.Drawing.Point(132, 206);
            this.MonthDiffTextBox.Name = "MonthDiffTextBox";
            this.MonthDiffTextBox.ReadOnly = true;
            this.MonthDiffTextBox.Size = new System.Drawing.Size(28, 26);
            this.MonthDiffTextBox.TabIndex = 18;
            this.MonthDiffTextBox.Text = "0";
            this.MonthDiffTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // YearDiffTextBox
            // 
            this.YearDiffTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.YearDiffTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YearDiffTextBox.Location = new System.Drawing.Point(83, 206);
            this.YearDiffTextBox.Name = "YearDiffTextBox";
            this.YearDiffTextBox.ReadOnly = true;
            this.YearDiffTextBox.Size = new System.Drawing.Size(48, 26);
            this.YearDiffTextBox.TabIndex = 17;
            this.YearDiffTextBox.Text = "0";
            this.YearDiffTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // YearsMonthsDaysLabel
            // 
            this.YearsMonthsDaysLabel.Location = new System.Drawing.Point(83, 230);
            this.YearsMonthsDaysLabel.Name = "YearsMonthsDaysLabel";
            this.YearsMonthsDaysLabel.Size = new System.Drawing.Size(114, 16);
            this.YearsMonthsDaysLabel.TabIndex = 0;
            this.YearsMonthsDaysLabel.Text = "  years   months  days";
            this.YearsMonthsDaysLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusPanel
            // 
            this.StatusPanel.Controls.Add(this.OffsetNumericUpDown);
            this.StatusPanel.Controls.Add(this.TagLinkLabel);
            this.StatusPanel.Controls.Add(this.SaveButton);
            this.StatusPanel.Controls.Add(this.Weekday0TextBox);
            this.StatusPanel.Controls.Add(this.Day0TextBox);
            this.StatusPanel.Controls.Add(this.Month0TextBox);
            this.StatusPanel.Controls.Add(this.Year0TextBox);
            this.StatusPanel.Controls.Add(this.TodayLabel);
            this.StatusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.StatusPanel.Location = new System.Drawing.Point(0, 250);
            this.StatusPanel.Name = "StatusPanel";
            this.StatusPanel.Size = new System.Drawing.Size(366, 25);
            this.StatusPanel.TabIndex = 33;
            // 
            // OffsetNumericUpDown
            // 
            this.OffsetNumericUpDown.BackColor = System.Drawing.Color.LightGreen;
            this.OffsetNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OffsetNumericUpDown.ForeColor = System.Drawing.SystemColors.WindowText;
            this.OffsetNumericUpDown.Location = new System.Drawing.Point(2, 0);
            this.OffsetNumericUpDown.Maximum = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.OffsetNumericUpDown.Minimum = new decimal(new int[] {
            7,
            0,
            0,
            -2147483648});
            this.OffsetNumericUpDown.Name = "OffsetNumericUpDown";
            this.OffsetNumericUpDown.ReadOnly = true;
            this.OffsetNumericUpDown.Size = new System.Drawing.Size(80, 26);
            this.OffsetNumericUpDown.TabIndex = 20;
            this.OffsetNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ToolTip.SetToolTip(this.OffsetNumericUpDown, "Adjust day of week");
            this.OffsetNumericUpDown.ValueChanged += new System.EventHandler(this.OffsetNumericUpDown_ValueChanged);
            // 
            // TagLinkLabel
            // 
            this.TagLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TagLinkLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.TagLinkLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TagLinkLabel.Image = ((System.Drawing.Image)(resources.GetObject("TagLinkLabel.Image")));
            this.TagLinkLabel.Location = new System.Drawing.Point(306, 0);
            this.TagLinkLabel.Margin = new System.Windows.Forms.Padding(0);
            this.TagLinkLabel.Name = "TagLinkLabel";
            this.TagLinkLabel.Size = new System.Drawing.Size(29, 25);
            this.TagLinkLabel.TabIndex = 101;
            this.TagLinkLabel.Tag = "http://heliwave.com";
            this.TagLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolTip.SetToolTip(this.TagLinkLabel, "Website");
            this.TagLinkLabel.UseVisualStyleBackColor = true;
            this.TagLinkLabel.Click += new System.EventHandler(this.LinkLabel_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SaveButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.SaveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveButton.Image")));
            this.SaveButton.Location = new System.Drawing.Point(335, 0);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(0);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(31, 25);
            this.SaveButton.TabIndex = 102;
            this.SaveButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolTip.SetToolTip(this.SaveButton, "Save");
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // Weekday0TextBox
            // 
            this.Weekday0TextBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.Weekday0TextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.Weekday0TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Weekday0TextBox.Location = new System.Drawing.Point(189, 0);
            this.Weekday0TextBox.Name = "Weekday0TextBox";
            this.Weekday0TextBox.ReadOnly = true;
            this.Weekday0TextBox.Size = new System.Drawing.Size(121, 24);
            this.Weekday0TextBox.TabIndex = 54;
            this.Weekday0TextBox.TabStop = false;
            this.Weekday0TextBox.Text = "Wednesday";
            this.Weekday0TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Day0TextBox
            // 
            this.Day0TextBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.Day0TextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.Day0TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Day0TextBox.Location = new System.Drawing.Point(160, 0);
            this.Day0TextBox.MaxLength = 2;
            this.Day0TextBox.Name = "Day0TextBox";
            this.Day0TextBox.ReadOnly = true;
            this.Day0TextBox.Size = new System.Drawing.Size(29, 24);
            this.Day0TextBox.TabIndex = 53;
            this.Day0TextBox.TabStop = false;
            this.Day0TextBox.Text = "99";
            this.Day0TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Month0TextBox
            // 
            this.Month0TextBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.Month0TextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.Month0TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Month0TextBox.Location = new System.Drawing.Point(131, 0);
            this.Month0TextBox.MaxLength = 2;
            this.Month0TextBox.Name = "Month0TextBox";
            this.Month0TextBox.ReadOnly = true;
            this.Month0TextBox.Size = new System.Drawing.Size(29, 24);
            this.Month0TextBox.TabIndex = 52;
            this.Month0TextBox.TabStop = false;
            this.Month0TextBox.Text = "99";
            this.Month0TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Year0TextBox
            // 
            this.Year0TextBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.Year0TextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.Year0TextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Year0TextBox.Location = new System.Drawing.Point(83, 0);
            this.Year0TextBox.MaxLength = 4;
            this.Year0TextBox.Name = "Year0TextBox";
            this.Year0TextBox.ReadOnly = true;
            this.Year0TextBox.Size = new System.Drawing.Size(48, 24);
            this.Year0TextBox.TabIndex = 51;
            this.Year0TextBox.TabStop = false;
            this.Year0TextBox.Text = "9999";
            this.Year0TextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TodayLabel
            // 
            this.TodayLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TodayLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.TodayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TodayLabel.ForeColor = System.Drawing.Color.Crimson;
            this.TodayLabel.Location = new System.Drawing.Point(0, 0);
            this.TodayLabel.Name = "TodayLabel";
            this.TodayLabel.Size = new System.Drawing.Size(83, 25);
            this.TodayLabel.TabIndex = 0;
            this.TodayLabel.Tag = "http://qurancode.com";
            this.TodayLabel.Text = "v3.1.9";
            this.TodayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.TodayLabel.Click += new System.EventHandler(this.LinkLabel_Click);
            // 
            // Years53ListBox
            // 
            this.Years53ListBox.FormattingEnabled = true;
            this.Years53ListBox.Location = new System.Drawing.Point(306, 60);
            this.Years53ListBox.Name = "Years53ListBox";
            this.Years53ListBox.Size = new System.Drawing.Size(53, 186);
            this.Years53ListBox.TabIndex = 0;
            // 
            // LeapYearRulesButton
            // 
            this.LeapYearRulesButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.LeapYearRulesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LeapYearRulesButton.Image = ((System.Drawing.Image)(resources.GetObject("LeapYearRulesButton.Image")));
            this.LeapYearRulesButton.Location = new System.Drawing.Point(197, 90);
            this.LeapYearRulesButton.Margin = new System.Windows.Forms.Padding(0);
            this.LeapYearRulesButton.Name = "LeapYearRulesButton";
            this.LeapYearRulesButton.Size = new System.Drawing.Size(30, 27);
            this.LeapYearRulesButton.TabIndex = 13;
            this.LeapYearRulesButton.Tag = "";
            this.LeapYearRulesButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolTip.SetToolTip(this.LeapYearRulesButton, "Leap year calculation rules");
            this.LeapYearRulesButton.UseVisualStyleBackColor = true;
            this.LeapYearRulesButton.Click += new System.EventHandler(this.LeapYearRulesButton_Click);
            // 
            // PictureBox
            // 
            this.PictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox.Image")));
            this.PictureBox.Location = new System.Drawing.Point(198, 119);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(100, 100);
            this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox.TabIndex = 104;
            this.PictureBox.TabStop = false;
            this.PictureBox.Tag = "Ramanujan.pdf";
            this.ToolTip.SetToolTip(this.PictureBox, "Ramanujan\'s birthday 22-12-1887\r\nAny row, column, diagonal, 2x2 sum to 139\r\nWhat " +
        "is the sum chapter verses of these numbers?");
            this.PictureBox.Click += new System.EventHandler(this.LinkLabel_Click);
            // 
            // RamanujanLabel
            // 
            this.RamanujanLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RamanujanLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RamanujanLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.RamanujanLabel.Location = new System.Drawing.Point(198, 219);
            this.RamanujanLabel.Name = "RamanujanLabel";
            this.RamanujanLabel.Size = new System.Drawing.Size(100, 29);
            this.RamanujanLabel.TabIndex = 22;
            this.RamanujanLabel.Tag = "Ramanujan.pdf";
            this.RamanujanLabel.Text = "Srinivasa Ramanujan\r\nMagic Square && 139";
            this.RamanujanLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ToolTip.SetToolTip(this.RamanujanLabel, "Make your own magic square :)");
            this.RamanujanLabel.Click += new System.EventHandler(this.LinkLabel_Click);
            // 
            // WeekdaysTextBox
            // 
            this.WeekdaysTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.WeekdaysTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WeekdaysTextBox.Location = new System.Drawing.Point(83, 177);
            this.WeekdaysTextBox.Name = "WeekdaysTextBox";
            this.WeekdaysTextBox.ReadOnly = true;
            this.WeekdaysTextBox.Size = new System.Drawing.Size(106, 26);
            this.WeekdaysTextBox.TabIndex = 16;
            this.WeekdaysTextBox.Text = "0";
            this.WeekdaysTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // WeeksTextBox
            // 
            this.WeeksTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.WeeksTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WeeksTextBox.Location = new System.Drawing.Point(83, 148);
            this.WeeksTextBox.Name = "WeeksTextBox";
            this.WeeksTextBox.ReadOnly = true;
            this.WeeksTextBox.Size = new System.Drawing.Size(106, 26);
            this.WeeksTextBox.TabIndex = 15;
            this.WeeksTextBox.Text = "0";
            this.WeeksTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // WeeksLabel
            // 
            this.WeeksLabel.Location = new System.Drawing.Point(5, 150);
            this.WeeksLabel.Name = "WeeksLabel";
            this.WeeksLabel.Size = new System.Drawing.Size(72, 23);
            this.WeeksLabel.TabIndex = 0;
            this.WeeksLabel.Text = "Weeks";
            this.WeeksLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // WeekdaysLabel
            // 
            this.WeekdaysLabel.Location = new System.Drawing.Point(5, 179);
            this.WeekdaysLabel.Name = "WeekdaysLabel";
            this.WeekdaysLabel.Size = new System.Drawing.Size(72, 23);
            this.WeekdaysLabel.TabIndex = 0;
            this.WeekdaysLabel.Text = "Weekdays";
            this.WeekdaysLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(366, 275);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.RamanujanLabel);
            this.Controls.Add(this.WeeksTextBox);
            this.Controls.Add(this.WeeksLabel);
            this.Controls.Add(this.WeekdaysTextBox);
            this.Controls.Add(this.WeekdaysLabel);
            this.Controls.Add(this.LeapYearRulesButton);
            this.Controls.Add(this.StatusPanel);
            this.Controls.Add(this.DayDiffTextBox);
            this.Controls.Add(this.MonthDiffTextBox);
            this.Controls.Add(this.YearDiffTextBox);
            this.Controls.Add(this.DifferenceLabel);
            this.Controls.Add(this.Years53ComboBox);
            this.Controls.Add(this.Years53TextBox);
            this.Controls.Add(this.LeapYearsTextBox);
            this.Controls.Add(this.DaysTextBox);
            this.Controls.Add(this.Weekday2TextBox);
            this.Controls.Add(this.Day2TextBox);
            this.Controls.Add(this.Month2TextBox);
            this.Controls.Add(this.Year2TextBox);
            this.Controls.Add(this.Weekday1TextBox);
            this.Controls.Add(this.Day1TextBox);
            this.Controls.Add(this.Month1TextBox);
            this.Controls.Add(this.Year1TextBox);
            this.Controls.Add(this.Years53Label);
            this.Controls.Add(this.LeapYearsLabel);
            this.Controls.Add(this.DaysLabel);
            this.Controls.Add(this.Date2Label);
            this.Controls.Add(this.Date1Label);
            this.Controls.Add(this.Years53ListBox);
            this.Controls.Add(this.YearsMonthsDaysLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Day of Week";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.StatusPanel.ResumeLayout(false);
            this.StatusPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OffsetNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Year1TextBox;
        private System.Windows.Forms.TextBox Month1TextBox;
        private System.Windows.Forms.TextBox Day1TextBox;
        private System.Windows.Forms.TextBox Weekday1TextBox;
        private System.Windows.Forms.Label Date1Label;
        private System.Windows.Forms.Label Date2Label;
        private System.Windows.Forms.TextBox Weekday2TextBox;
        private System.Windows.Forms.TextBox Day2TextBox;
        private System.Windows.Forms.TextBox Month2TextBox;
        private System.Windows.Forms.TextBox Year2TextBox;
        private System.Windows.Forms.TextBox DaysTextBox;
        private System.Windows.Forms.Label DaysLabel;
        private System.Windows.Forms.Label LeapYearsLabel;
        private System.Windows.Forms.TextBox LeapYearsTextBox;
        private System.Windows.Forms.Label Years53Label;
        private System.Windows.Forms.TextBox Years53TextBox;
        private System.Windows.Forms.ComboBox Years53ComboBox;
        private System.Windows.Forms.Label DifferenceLabel;
        private System.Windows.Forms.TextBox DayDiffTextBox;
        private System.Windows.Forms.TextBox MonthDiffTextBox;
        private System.Windows.Forms.TextBox YearDiffTextBox;
        private System.Windows.Forms.Label YearsMonthsDaysLabel;
        private System.Windows.Forms.Panel StatusPanel;
        private System.Windows.Forms.TextBox Weekday0TextBox;
        private System.Windows.Forms.TextBox Day0TextBox;
        private System.Windows.Forms.TextBox Month0TextBox;
        private System.Windows.Forms.TextBox Year0TextBox;
        private System.Windows.Forms.Label TodayLabel;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button TagLinkLabel;
        private System.Windows.Forms.ListBox Years53ListBox;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.TextBox WeekdaysTextBox;
        private System.Windows.Forms.Button LeapYearRulesButton;
        private System.Windows.Forms.TextBox WeeksTextBox;
        private System.Windows.Forms.Label WeeksLabel;
        private System.Windows.Forms.Label WeekdaysLabel;
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.Label RamanujanLabel;
        private System.Windows.Forms.NumericUpDown OffsetNumericUpDown;
    }
}