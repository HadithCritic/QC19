using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.IO;

namespace DayOfWeek
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Date date0 = null;
        private Date date1 = null;
        private Date date2 = null;
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = Application.ProductName + " - " + Globals.SHORT_VERSION;
            DateTime today = DateTime.Today;

            date0 = new Date(today.Year, today.Month, today.Day);
            date1 = new Date(1, 1, 1);
            date2 = new Date(today.Year, today.Month, today.Day);
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            Years53ListBox.Items.Clear();

            Years53ComboBox.Items.Clear();
            Years53ComboBox.Items.Add("Sundays");
            Years53ComboBox.Items.Add("Mondays");
            Years53ComboBox.Items.Add("Tuesdays");
            Years53ComboBox.Items.Add("Wednesdays");
            Years53ComboBox.Items.Add("Thursdays");
            Years53ComboBox.Items.Add("Fridays");
            Years53ComboBox.Items.Add("Saturdays");
            Years53ComboBox.SelectedIndex = 5; // Fridays :)
            Years53ComboBox.Focus();
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if ((ModifierKeys == Keys.Control) && (e.KeyCode == Keys.S))
            {
                SaveButton_Click(null, null);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private int year = 1;
        private int month = 1;
        private int day = 1;
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void YearTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox control = sender as TextBox;
            if (control != null)
            {
                if (int.TryParse(control.Text, out year))
                {
                    if (
                        (e.KeyCode == Keys.Enter)
                        || (e.KeyCode == Keys.Up)
                        || (e.KeyCode == Keys.Down)
                       )
                    {
                        if (e.KeyCode == Keys.Up)
                        {
                            year++;
                            if (year > 9999)
                            {
                                year = 9999;
                            }
                        }
                        else if (e.KeyCode == Keys.Down)
                        {
                            year--;
                            if (year < 1)
                            {
                                year = 1;
                            }
                        }

                        if (control.Name.Contains("1"))
                        {
                            month = date1.Month;
                            LimitDayToMonthDays(control, year, month);
                            day = date1.Day;
                            date1 = new Date(year, month, day);
                        }
                        else
                        {
                            if (year != date2.Year)
                            {
                                month = date2.Month;
                                LimitDayToMonthDays(control, year, month);
                                day = date2.Day;
                                date2 = new Date(year, month, day);
                            }
                        }

                        UpdateGUI();
                    }
                }
            }
        }
        private void MonthTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox control = sender as TextBox;
            if (control != null)
            {
                if (int.TryParse(control.Text, out month))
                {
                    if (
                        (e.KeyCode == Keys.Enter)
                        || (e.KeyCode == Keys.Up)
                        || (e.KeyCode == Keys.Down)
                       )
                    {
                        if (e.KeyCode == Keys.Up)
                        {
                            month++;
                            if (month > 12)
                            {
                                month = 12;
                            }
                        }
                        else if (e.KeyCode == Keys.Down)
                        {
                            month--;
                            if (month < 1)
                            {
                                month = 1;
                            }
                        }

                        if (control.Name.Contains("1"))
                        {
                            year = date1.Year;
                            LimitDayToMonthDays(control, year, month);
                            day = date1.Day;
                            date1 = new Date(year, month, day);
                        }
                        else
                        {
                            year = date2.Year;
                            LimitDayToMonthDays(control, year, month);
                            day = date2.Day;
                            date2 = new Date(year, month, day);
                        }

                        UpdateGUI();
                    }
                }
            }
        }
        private void DayTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            int month_days = Date.DaysInMonth(year, month);

            TextBox control = sender as TextBox;
            if (control != null)
            {
                if (int.TryParse(control.Text, out day))
                {
                    if (
                        (e.KeyCode == Keys.Enter)
                        || (e.KeyCode == Keys.Up)
                        || (e.KeyCode == Keys.Down)
                       )
                    {
                        if (e.KeyCode == Keys.Up)
                        {
                            day++;
                            if (day > month_days)
                            {
                                day = month_days;
                            }
                        }
                        else if (e.KeyCode == Keys.Down)
                        {
                            day--;
                            if (day < 1)
                            {
                                day = 1;
                            }
                        }

                        if (control.Name.Contains("1"))
                        {
                            year = date1.Year;
                            month = date1.Month;
                            date1 = new Date(year, month, day);
                        }
                        else
                        {
                            year = date2.Year;
                            month = date2.Month;
                            date2 = new Date(year, month, day);
                        }

                        UpdateGUI();
                    }
                }
            }
        }
        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox control = sender as TextBox;
            if (control != null)
            {
                if (control.Name.StartsWith("Year"))
                {
                    if (int.TryParse(control.Text, out year))
                    {
                        if (year < 1) year = 1;
                        if (year > 9999) year = 9999;

                        if (control.Name.Contains("1"))
                        {
                            if (year != date1.Year)
                            {
                                month = date1.Month;
                                LimitDayToMonthDays(control, year, month);
                                day = date1.Day;
                                date1 = new Date(year, month, day);
                                UpdateGUI();
                            }
                        }
                        else
                        {
                            if (year != date2.Year)
                            {
                                month = date2.Month;
                                LimitDayToMonthDays(control, year, month);
                                day = date2.Day;
                                date2 = new Date(year, month, day);
                                UpdateGUI();
                            }
                        }
                    }
                }
                else if (control.Name.StartsWith("Month"))
                {
                    if (int.TryParse(control.Text, out month))
                    {
                        if (month < 1) month = 1;
                        if (month > 12) month = 12;

                        if (control.Name.Contains("1"))
                        {
                            if (month != date1.Month)
                            {
                                year = date1.Year;
                                LimitDayToMonthDays(control, year, month);
                                day = date1.Day;
                                date1 = new Date(year, month, day);
                                UpdateGUI();
                            }
                        }
                        else
                        {
                            if (month != date2.Month)
                            {
                                year = date2.Year;
                                LimitDayToMonthDays(control, year, month);
                                day = date2.Day;
                                date2 = new Date(year, month, day);
                                UpdateGUI();
                            }
                        }
                    }
                }
                else if (control.Name.StartsWith("Day"))
                {
                    if (int.TryParse(control.Text, out day))
                    {
                        int month_days = Date.DaysInMonth(year, month);
                        if (day < 1) day = 1;
                        if (day > month_days) day = month_days;

                        if (control.Name.Contains("1"))
                        {
                            if (day != date1.Day)
                            {
                                year = date1.Year;
                                month = date1.Month;
                                date1 = new Date(year, month, day);
                                UpdateGUI();
                            }
                        }
                        else
                        {
                            if (day != date2.Day)
                            {
                                year = date2.Year;
                                month = date2.Month;
                                date2 = new Date(year, month, day);
                                UpdateGUI();
                            }
                        }
                    }
                }
            }
        }
        private void LimitDayToMonthDays(TextBox control, int year, int month)
        {
            if (day > Date.DaysInMonth(this.year, this.month))
            {
                day = Date.DaysInMonth(this.year, this.month);
                if (control.Name.Contains("1"))
                {
                    date1 = new Date(this.year, this.month, day);
                }
                else
                {
                    date2 = new Date(this.year, this.month, day);
                }
            }
        }
        private void UpdateGUI()
        {
            Year0TextBox.Text = date0.Year.ToString("0000");
            Month0TextBox.Text = date0.Month.ToString("00");
            Day0TextBox.Text = date0.Day.ToString("00");
            Weekday0TextBox.Text = Date.WeekdayName(date0.Weekday);

            Year1TextBox.Text = date1.Year.ToString("0000");
            Month1TextBox.Text = date1.Month.ToString("00");
            Day1TextBox.Text = date1.Day.ToString("00");
            Weekday1TextBox.Text = Date.WeekdayName(date1.Weekday);

            Year2TextBox.Text = date2.Year.ToString("0000");
            Month2TextBox.Text = date2.Month.ToString("00");
            Day2TextBox.Text = date2.Day.ToString("00");
            Weekday2TextBox.Text = Date.WeekdayName(date2.Weekday);

            LeapYearsTextBox.Text = Date.LeapYearsBetweenDates(date1, date2).ToString();

            DaysTextBox.Text = Date.DaysBetweenDates(date1, date2).ToString();

            WeeksTextBox.Text = Date.WeeksBetweenDates(date1, date2).ToString();

            target_weekday = Years53ComboBox.SelectedIndex + 1;
            WeekdaysTextBox.Text = Date.WeekdaysBetweenDates(date1, date2, target_weekday).ToString();

            Date date_diff = Date.SubtractDates(date1, date2);
            if (date_diff != null)
            {
                YearDiffTextBox.Text = date_diff.Year.ToString();
                MonthDiffTextBox.Text = date_diff.Month.ToString();
                DayDiffTextBox.Text = date_diff.Day.ToString();
            }

            years53_list = Date.YearsWith53WeekdaysList(date1.Year, date2.Year, target_weekday);
            Years53TextBox.Text = years53_list.Count.ToString();
            Years53TextBox.Refresh();

            Years53ListBox.SuspendLayout();
            Years53ListBox.Items.Clear();
            foreach (int i in years53_list)
            {
                Years53ListBox.Items.Add(i.ToString("0000"));
            }
            Years53ListBox.ResumeLayout();
        }

        private int target_weekday = 0;
        private List<int> years53_list = null;
        private void Years53ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            target_weekday = Years53ComboBox.SelectedIndex + 1;
            WeekdaysLabel.Text = Years53ComboBox.SelectedItem.ToString();
            years53_list = Date.YearsWith53WeekdaysList(date1.Year, date2.Year, target_weekday);
            Years53TextBox.Text = years53_list.Count.ToString();
            Years53TextBox.Refresh();

            UpdateGUI();
        }

        private int offset = 0;
        private int previous_value = 0;
        private void OffsetNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            offset = (int)OffsetNumericUpDown.Value;

            if (previous_value < offset)
            {
                Date.FirstDayOf0001 = Date.FirstDayOf0001 + 1;
            }
            else if (previous_value > offset)
            {
                Date.FirstDayOf0001 = Date.FirstDayOf0001 - 1;
            }
            else
            {
                // do nothing
            }
            previous_value = offset;

            OffsetNumericUpDown.BackColor = (offset == 0) ? Color.LightGreen : Color.Red;
            OffsetNumericUpDown.ForeColor = (offset == 0) ? Color.Black : Color.White;
            OffsetNumericUpDown.Refresh();

            UpdateGUI();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            int backup = Years53ComboBox.SelectedIndex;
            Years53ComboBox.SuspendLayout();

            string filename = "YearsWith53" + Years53ComboBox.SelectedItem.ToString()
                            + "_" + date1.Year.ToString()
                            + "_" + date2.Year.ToString()
                            + "_" + Years53TextBox.Text
                            + ".txt";

            StringBuilder str = new StringBuilder();
            if (offset != 0)
            {
                str.AppendLine("WARNING: Adjusted by " + (offset > 0 ? "+" : "") + offset + " days");
                str.AppendLine();
            }
            str.AppendLine("Today\t" + date0.ToMediumString());
            str.AppendLine();
            str.AppendLine("From \t" + date1.ToMediumString());
            str.AppendLine("To   \t" + date2.ToMediumString());
            str.AppendLine();
            str.AppendLine("Leap Years\t\t" + LeapYearsTextBox.Text);
            str.AppendLine("Days\t\t\t" + DaysTextBox.Text);
            str.AppendLine("Weeks\t\t\t" + WeeksTextBox.Text);
            string tabs = (Years53ComboBox.SelectedItem.ToString().Length > 8) ? tabs = "\t\t" : "\t\t\t";
            str.AppendLine(Years53ComboBox.SelectedItem.ToString() + tabs + WeekdaysTextBox.Text);
            str.AppendLine("Difference\t\t" +
                YearDiffTextBox.Text + " years " +
                MonthDiffTextBox.Text + " months " +
                DayDiffTextBox.Text + " days");
            str.AppendLine();
            str.AppendLine("Years with 53 " + Years53ComboBox.SelectedItem.ToString() + "\t" + Years53TextBox.Text);
            foreach (string year in Years53ListBox.Items)
            {
                str.Append(year + ",");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }

            FileHelper.SaveText(filename, str.ToString());
            FileHelper.DisplayFile(filename);

            Years53ComboBox.ResumeLayout();
            Years53ComboBox.SelectedIndex = backup;
        }
        private void LeapYearRulesButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("A normal year will have 53 Fridays" + "\r\n" +
                            "if it starts with a Friday." + "\r\n" +
                            "A leap year wil have 53 Fridays" + "\r\n" +
                            "if it starts with a Thursday or Friday." + "\r\n" +
                            "\r\n" +
                            "Julian Calendar:" + "\r\n" +
                            "A leap year repeats every four years" + "\r\n" +
                            "from 45BC to 1582AD." + "\r\n" +
                            "Gregorian Calendar:" + "\r\n" +
                            "A leap year repeats every four years" + "\r\n" +
                            "except multiples of 100 but not 400" + "\r\n" +
                            "from 1582 onwards." + "\r\n" +
                            "\r\n" +
                            "October 1582 skipped 10 days from" + "\r\n" +
                            "Monday 4th to Friday 15th. 1582 started" + "\r\n" +
                            "on Monday and ended on Friday.", "Leap Year Rules");
        }
        private void LinkLabel_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                Control control = (sender as Control);
                if (control != null)
                {
                    if (control.Tag != null)
                    {
                        if (!String.IsNullOrEmpty(control.Tag.ToString()))
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(control.Tag.ToString());
                            }
                            catch
                            {
                                try
                                {
                                    if (Directory.Exists(Globals.HELP_FOLDER))
                                    {
                                        System.Diagnostics.Process.Start(Globals.HELP_FOLDER + Path.DirectorySeparatorChar + control.Tag.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
}
