using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;

public partial class MainForm : Form
{
    private int m_radix = Numbers.DEFAULT_RADIX;
    private int m_divisor = Numbers.DEFAULT_DIVISOR;

    private void FixMicrosoft(object sender, KeyPressEventArgs e)
    {
        // stop annoying beep due to parent not having an AcceptButton
        if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Escape))
        {
            e.Handled = true;
        }
        // enable Ctrl+A to SelectAll in TextBox and RichTextBox
        if ((ModifierKeys == Keys.Control) && (e.KeyChar == 1))
        {
            TextBoxBase control = (sender as TextBoxBase);
            if (control != null)
            {
                control.SelectAll();
                e.Handled = true;
            }
        }
    }

    private float m_dpi = 96.0F;
    public MainForm()
    {
        using (Graphics graphics = this.CreateGraphics())
        {
            m_dpi = graphics.DpiX;    // 100% = 96.0F,   125% = 120.0F,   150% = 144.0F
        }

        if (m_dpi == 96.0F)
        {
            InitializeComponent();
        }
        else
        {
            InitializeComponent125();
        }

        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

        AboutToolStripMenuItem.Font = new Font(AboutToolStripMenuItem.Font, AboutToolStripMenuItem.Font.Style | FontStyle.Bold);

        m_ini_filename = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".ini");
        LoadSettings();
    }

    private string m_ini_filename = null;
    private void LoadSettings()
    {
        if (File.Exists(m_ini_filename))
        {
            using (StreamReader reader = File.OpenText(m_ini_filename))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            string[] parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                switch (parts[0])
                                {
                                    case "Top":
                                        {
                                            this.Top = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Left":
                                        {
                                            this.Left = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Width":
                                        {
                                            this.Width = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Height":
                                        {
                                            this.Height = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Count":
                                        {
                                            string[] sub_parts = parts[1].Split('\t');
                                            if (sub_parts.Length == 2)
                                            {
                                                int count = int.Parse(sub_parts[0]);
                                                string[] sub_sub_parts = sub_parts[1].Split(',');
                                                if (sub_sub_parts.Length == count)
                                                {
                                                    foreach (string item in sub_sub_parts)
                                                    {
                                                        m_history_items.Add(item);
                                                        m_history_index++;
                                                    }
                                                    ValueTextBox.Text = m_history_items[m_history_index];
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    RestoreLocation();
                }
            }
        }
        else // first start
        {
            RestoreLocation();
        }
    }
    private void SaveSettings()
    {
        try
        {
            using (StreamWriter writer = File.CreateText(m_ini_filename))
            {
                writer.WriteLine("[Window]");
                writer.WriteLine("Top=" + this.Top);
                writer.WriteLine("Left=" + this.Left);
                writer.WriteLine("Width=" + this.Width);
                writer.WriteLine("Height=" + this.Height);

                writer.WriteLine("[History]");
                StringBuilder str = new StringBuilder("Count=" + this.m_history_items.Count + "\t");
                foreach (string item in m_history_items)
                {
                    str.Append(item + ",");
                }
                str.Remove(str.Length - 1, 1);
                writer.WriteLine(str.ToString());
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    private void RestoreLocation()
    {
        this.Top = Screen.PrimaryScreen.WorkingArea.Top;
        this.Left = Screen.PrimaryScreen.WorkingArea.Left;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        m_worker = null;
        m_worker_thread = null;

        VersionLabel.Text = Globals.SHORT_VERSION;

        if (this.Top < 0)
        {
            RestoreLocation();
        }

        NthNumberTextBox.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[3];
        NthAdditiveNumberTextBox.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[3];
        NthNonAdditiveNumberTextBox.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[6];

        PLabel.ForeColor = Numbers.NUMBER_TYPE_COLORS[2];
        APLabel.ForeColor = Numbers.NUMBER_TYPE_COLORS[3];
        XPLabel.ForeColor = Numbers.NUMBER_TYPE_COLORS[4];
        CLabel.ForeColor = Numbers.NUMBER_TYPE_COLORS[5];
        ACLabel.ForeColor = Numbers.NUMBER_TYPE_COLORS[6];
        XCLabel.ForeColor = Numbers.NUMBER_TYPE_COLORS[7];
        //DFLabel.ForeColor = Numbers.NUMBER_KIND_COLORS[0];
        //ABLabel.ForeColor = Numbers.NUMBER_KIND_COLORS[2];
        DeficientNumbersLabel.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[0];
        PerfectNumbersLabel.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[1];
        AbundantNumbersLabel.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[2];
        DFTextBox.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[0];
        ABTextBox.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[2];
    }
    private void MainForm_Shown(object sender, EventArgs e)
    {
        NotifyIcon.Visible = true;

        EnableEntryControls();

        ToolTip.SetToolTip(this.DigitsLabel,
                            "Edit interesting numbers ..." + "\r\n" +
                            "\r\n" +
                            "Up                 = next number" + "\r\n" +
                            "Ctrl+Up        = next prime" + "\r\n" +
                            "Shift+Up      = next history" + "\r\n" +
                            "Down            = previous number" + "\r\n" +
                            "Ctrl+Down   = previous prime" + "\r\n" +
                            "Shift+Down = previous history"
                           );

        if (this.Tag != null)
        {
            ValueTextBox.Text = this.Tag.ToString();
            ValueTextBox.Refresh();
        }

        CallRun();
    }
    private bool m_was_maximized = false;
    private void MainForm_Resize(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Visible = false; // send to system try instead of minimize
            this.WindowState = FormWindowState.Normal;
        }
        else
        {
            m_was_maximized = this.WindowState == FormWindowState.Maximized;
        }

        if (m_dpi == 96.0F)
        {
            int left = 1;
            int expandable = (this.Width - 10 - 6 - 30 - 13);
            int width = (int)(expandable * 0.666);
            PrimeFactorsTextBox.Left = left;
            PrimeFactorsTextBox.Width = width;
            left += width;
            width = (int)(expandable * 0.333);
            PrimeNumbersSumTextBox.Left = left;
            PrimeNumbersSumTextBox.Width = width;

            left = 1 + 27;
            expandable = (this.Width - 10 - 6 - 27);
            width = (int)(expandable * 0.25);
            NthANumberDimensionTextBox.Left = left;
            NthANumberDimensionTextBox.Width = width;
            NthANumberDimensionLabel.Left = left + width - 10;
            left += width;
            width = (int)(expandable * 0.25);
            NthUNumberDimensionTextBox.Left = left;
            NthUNumberDimensionTextBox.Width = width;
            NthUNumberDimensionLabel.Left = left + width - 10;
            left += width;
            width = (int)(expandable * 0.25);
            NthDNumberDimensionTextBox.Left = left;
            NthDNumberDimensionTextBox.Width = width;
            NthDNumberDimensionLabel.Left = left + width - 10;
            left += width;
            width = (int)(expandable * 0.25);
            NthSimilarPowersTextBox.Left = left;
            NthSimilarPowersTextBox.Width = width;

            left = 2 + 7;
            expandable = (this.Width - 10 - 6 - 7);
            width = (int)(expandable * 0.333);
            NthNumberTextBox.Left = left;
            NthNumberTextBox.Width = width;
            left += width;
            width = (int)(expandable * 0.333);
            NthAdditiveNumberTextBox.Left = left;
            NthAdditiveNumberTextBox.Width = width;
            left += width;
            width = (int)(expandable * 0.333);
            NthNonAdditiveNumberTextBox.Left = left;
            NthNonAdditiveNumberTextBox.Width = width;

            left = 7;
            int small_width = (int)((this.Width - 25) * 0.304);
            int large_width = (int)((this.Width - 25) * 0.405);
            SumOfNumbersTextBox.Left = left;
            SumOfNumbersTextBox.Width = large_width;
            left += large_width - 1;
            SumOfDigitSumsTextBox.Left = left;
            SumOfDigitSumsTextBox.Width = small_width;
            left += small_width - 1;
            SumOfDigitalRootsTextBox.Left = left;
            SumOfDigitalRootsTextBox.Width = small_width;

            left = 7;
            small_width = (int)((this.Width - 25) * 0.19);
            large_width = (int)((this.Width - 25) * 0.27);
            NumberKindIndexTextBox.Left = left;
            NumberKindIndexTextBox.Width = small_width;
            left += small_width - 1;
            SumOfProperDivisorsTextBox.Left = left;
            SumOfProperDivisorsTextBox.Width = small_width;
            left += small_width - 1;
            SumOfDivisorsTextBox.Left = left;
            SumOfDivisorsTextBox.Width = large_width;
            left += large_width - 1;
            SumOfDivisorDigitSumsTextBox.Left = left;
            SumOfDivisorDigitSumsTextBox.Width = small_width;
            left += small_width - 1;
            SumOfDivisorDigitalRootsTextBox.Left = left;
            SumOfDivisorDigitalRootsTextBox.Width = small_width;

            left = 7;
            large_width = (int)((this.Width - 25) * 0.19);
            small_width = (int)((this.Width - 25) * 0.09);
            PCIndexChainL2RTextBox.Left = left;
            PCIndexChainL2RTextBox.Width = large_width;
            left += large_width - 1;
            PCIndexChainR2LTextBox.Left = left;
            PCIndexChainR2LTextBox.Width = large_width;
            left += large_width - 1;
            CPIndexChainL2RTextBox.Left = left;
            CPIndexChainL2RTextBox.Width = large_width;
            left += large_width - 1;
            CPIndexChainR2LTextBox.Left = left;
            CPIndexChainR2LTextBox.Width = large_width;
            left += large_width - 1;
            IndexChainSumTextBox.Left = left;
            IndexChainSumTextBox.Width = large_width;
            left += large_width - 1;
            IndexChainLengthTextBox.Left = left;
            IndexChainLengthTextBox.Width = small_width;
        }
        else
        {
            int left = 3;
            int expandable = (this.Width - 10 - 6 - 30 - 13);
            int width = (int)(expandable * 0.666);
            PrimeFactorsTextBox.Left = left;
            PrimeFactorsTextBox.Width = width;
            left += width;
            width = (int)(expandable * 0.333);
            PrimeNumbersSumTextBox.Left = left;
            PrimeNumbersSumTextBox.Width = width;

            left = 3 + 27;
            expandable = (this.Width - 10 - 6 - 27);
            width = (int)(expandable * 0.333);
            NthANumberDimensionTextBox.Left = left;
            NthANumberDimensionTextBox.Width = width;
            NthANumberDimensionLabel.Left = left + width - 10;
            left += width;
            width = (int)(expandable * 0.333);
            NthUNumberDimensionTextBox.Left = left;
            NthUNumberDimensionTextBox.Width = width;
            NthUNumberDimensionLabel.Left = left + width - 10;
            left += width;
            width = (int)(expandable * 0.333);
            NthDNumberDimensionTextBox.Left = left;
            NthDNumberDimensionTextBox.Width = width;
            NthDNumberDimensionLabel.Left = left + width - 10;

            left = 2 + 10;
            expandable = (this.Width - 10 - 6 - 7);
            width = (int)(expandable * 0.333);
            NthNumberTextBox.Left = left;
            NthNumberTextBox.Width = width;
            left += width;
            width = (int)(expandable * 0.333);
            NthAdditiveNumberTextBox.Left = left;
            NthAdditiveNumberTextBox.Width = width;
            left += width;
            width = (int)(expandable * 0.333);
            NthNonAdditiveNumberTextBox.Left = left;
            NthNonAdditiveNumberTextBox.Width = width;

            left = 10;
            int small_width = (int)((this.Width - 25) * 0.304);
            int large_width = (int)((this.Width - 25) * 0.405);
            SumOfNumbersTextBox.Left = left;
            SumOfNumbersTextBox.Width = large_width;
            left += large_width - 1;
            SumOfDigitSumsTextBox.Left = left;
            SumOfDigitSumsTextBox.Width = small_width;
            left += small_width - 1;
            SumOfDigitalRootsTextBox.Left = left;
            SumOfDigitalRootsTextBox.Width = small_width;

            left = 10;
            small_width = (int)((this.Width - 25) * 0.19);
            large_width = (int)((this.Width - 25) * 0.27);
            NumberKindIndexTextBox.Left = left;
            NumberKindIndexTextBox.Width = small_width;
            left += small_width - 1;
            SumOfProperDivisorsTextBox.Left = left;
            SumOfProperDivisorsTextBox.Width = small_width;
            left += small_width - 1;
            SumOfDivisorsTextBox.Left = left;
            SumOfDivisorsTextBox.Width = large_width;
            left += large_width - 1;
            SumOfDivisorDigitSumsTextBox.Left = left;
            SumOfDivisorDigitSumsTextBox.Width = small_width;
            left += small_width - 1;
            SumOfDivisorDigitalRootsTextBox.Left = left;
            SumOfDivisorDigitalRootsTextBox.Width = small_width;

            left = 7;
            large_width = (int)((this.Width - 25) * 0.19);
            small_width = (int)((this.Width - 25) * 0.09);
            PCIndexChainL2RTextBox.Left = left;
            PCIndexChainL2RTextBox.Width = large_width;
            left += large_width - 1;
            PCIndexChainR2LTextBox.Left = left;
            PCIndexChainR2LTextBox.Width = large_width;
            left += large_width - 1;
            CPIndexChainL2RTextBox.Left = left;
            CPIndexChainL2RTextBox.Width = large_width;
            left += large_width - 1;
            CPIndexChainR2LTextBox.Left = left;
            CPIndexChainR2LTextBox.Width = large_width;
            left += large_width - 1;
            IndexChainSumTextBox.Left = left;
            IndexChainSumTextBox.Width = large_width;
            left += large_width - 1;
            IndexChainLengthTextBox.Left = left;
            IndexChainLengthTextBox.Width = small_width;
        }
    }
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
            {
                if (MessageBox.Show("Cancel all progress?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    Cancel();
                }
            }
            e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
        }
    }
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        //// prevent user from closing from the X close button
        //if (e.CloseReason == CloseReason.UserClosing)
        //{
        //    e.Cancel = true;
        //    this.Visible = false;
        //}
        Cancel();
    }
    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        CloseApplication();
    }
    private void CloseApplication()
    {
        if (m_worker_thread != null)
        {
            m_worker_thread.Join(); // wait for worker_thread to terminate
            m_worker_thread = null;
        }

        // remove icon from tray
        if (NotifyIcon != null)
        {
            NotifyIcon.Visible = false;
            NotifyIcon.Dispose();
        }

        SaveSettings();
    }
    private void Control_MouseHover(object sender, EventArgs e)
    {
        Control control = sender as Control;
        if (control != null)
        {
            string text = control.Text;
            if (!String.IsNullOrEmpty(text))
            {
                double value;
                if (double.TryParse(text, out value))
                {
                    string factors_str = Numbers.FactorizeToString((long)value);
                    ToolTip.SetToolTip(control, factors_str);
                }
                else
                {
                    ToolTip.SetToolTip(control, null);
                }
            }
            else
            {
                ToolTip.SetToolTip(control, null);
            }
        }
    }
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (e.KeyCode == Keys.A)
            {
                if (sender is TextBoxBase)
                {
                    (sender as TextBoxBase).SelectAll();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Z)
            {
            }
            else if (e.KeyCode == Keys.Y)
            {
            }
            else if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                CallRun();
            }
            else
            {
            }
        }
        else if (ModifierKeys == Keys.Shift)
        {
            if (e.KeyCode == Keys.Up)
            {
            }
            else if (e.KeyCode == Keys.Down)
            {
            }
            else if (e.KeyCode == Keys.Enter)
            {
            }
            else
            {
            }
        }
        else
        {
            if (e.KeyCode == Keys.Up)
            {
            }
            else if (e.KeyCode == Keys.Down)
            {
            }
            else if (e.KeyCode == Keys.Enter)
            {
            }
            else
            {
            }
        }
    }

    private void ValueTextBox_TextChanged(object sender, EventArgs e)
    {
        string text = ValueTextBox.Text.Replace(" ", "");
        int digits = Numbers.DigitCount(text);
        DigitsLabel.Text = digits.ToString() + " " + ((digits < 1000) ? "digit" + ((digits == 1) ? "" : "s") : "dgs");
        DigitsLabel.ForeColor = Numbers.GetNumberForeColor(digits);
        DigitsLabel.Refresh();

        if (!String.IsNullOrEmpty(text))
        {
            long value;
            if (long.TryParse(text, out value))
            {
                ValueTextBox.ForeColor = Numbers.GetNumberForeColor(value);
                ValueTextBox.BackColor = Numbers.GetNumberBackColor(value, m_divisor, SystemColors.Window);
                ToolTip.SetToolTip(this.ValueTextBox, "Divisors" + " " + Numbers.GetNumberToolTipText(value));
                ValueTextBox.Refresh();

                FactorizeValue(value);
            }
            else // big integer or Math expression
            {
                if (text.Length >= 19)  // big number
                {
                    TabControl.SelectedIndex = 0;   // FactorsTabPage
                }
                else // Math expression or invalid input
                {
                    TabControl.SelectedIndex = 1;   // IndexTabPage
                }
            }
        }
        else // empty
        {
            ClearNumberFields();
        }
    }
    private void ValueTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            if (e.KeyCode == Keys.A)
            {
                if (sender is TextBoxBase)
                {
                    (sender as TextBoxBase).SelectAll();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Z)
            {
                if (sender is TextBoxBase)
                {
                    (sender as TextBoxBase).Undo();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Y)
            {
                if (sender is TextBoxBase)
                {
                    //(sender as TextBoxBase).Redo();
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                NextPrimeNumber();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                PreviousPrimeNumber();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                CallRun();
            }
            else
            {
            }
        }
        else if (ModifierKeys == Keys.Shift)
        {
            if (e.KeyCode == Keys.Up)
            {
                NextHistoryItem();
            }
            else if (e.KeyCode == Keys.Down)
            {
                PreviousHistoryItem();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                CallRun();
            }
            else
            {
            }
        }
        else
        {
            if (e.KeyCode == Keys.Up)
            {
                IncrementValue();
            }
            else if (e.KeyCode == Keys.Down)
            {
                DecrementValue();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                CallRun();
            }
            else
            {
            }
        }
    }
    private void IncrementValue()
    {
        if (ValueTextBox.Text == "")
        {
            ValueTextBox.Text = "1";
        }
        else
        {
            long value;
            string input = ValueTextBox.Text.Replace(" ", "");
            if (long.TryParse(input, out value))
            {
                if (value < long.MaxValue)
                {
                    value++;
                    ValueTextBox.Text = value.ToString();
                    ValueTextBox.Refresh();
                }
            }
        }
    }
    private void DecrementValue()
    {
        if (ValueTextBox.Text == "")
        {
            ValueTextBox.Text = "1";
        }
        else
        {
            long value;
            string input = ValueTextBox.Text.Replace(" ", "");
            if (long.TryParse(input, out value))
            {
                if (value > 1L)
                {
                    value--;
                    ValueTextBox.Text = value.ToString();
                    ValueTextBox.Refresh();
                }
            }
        }
    }
    private void FactorizeValue(long value)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            // if there is a math expression, add to it, don't overwrite it
            if (
                (ValueTextBox.Text.EndsWith("+")) ||
                (ValueTextBox.Text.EndsWith("-")) ||
                (ValueTextBox.Text.EndsWith("*")) ||
                (ValueTextBox.Text.EndsWith("/")) ||
                (ValueTextBox.Text.EndsWith("^")) ||
                (ValueTextBox.Text.EndsWith("%"))
               )
            {
                ValueTextBox.Text += value.ToString();

                // focus so user can continue with +, -, *, /, ^, %, Enter
                ValueTextBox.Focus();
                ValueTextBox.SelectionStart = ValueTextBox.Text.Length;
                ValueTextBox.SelectionLength = 0;
            }
            else
            {
                // leave it as it is
                //ValueTextBox.Text = value.ToString();
            }

            ValueTextBox.ForeColor = Numbers.GetNumberForeColor(value);
            ValueTextBox.BackColor = Numbers.GetNumberBackColor(value, m_divisor, SystemColors.Window);
            ValueTextBox.Refresh();

            int digit_sum = Numbers.DigitSum(value);
            DigitSumTextBox.Text = (digit_sum == 0) ? "" : digit_sum.ToString();
            DigitSumTextBox.ForeColor = Numbers.GetNumberForeColor(digit_sum);
            DigitSumTextBox.BackColor = Numbers.GetNumberBackColor(digit_sum, m_divisor, SystemColors.ControlLight);
            ToolTip.SetToolTip(this.DigitSumTextBox, "Digit sum" + "\r\n" + /* "Divisors" + ": " + */ Numbers.GetNumberToolTipText(digit_sum));
            DigitSumTextBox.Refresh();

            int digital_root = Numbers.DigitalRoot(digit_sum);
            DigitalRootTextBox.Text = (digital_root == 0) ? "" : digital_root.ToString();
            DigitalRootTextBox.ForeColor = Numbers.GetNumberForeColor(digital_root);
            DigitalRootTextBox.BackColor = Numbers.GetNumberBackColor(digital_root, m_divisor, SystemColors.ControlLight);
            ToolTip.SetToolTip(this.DigitalRootTextBox, "Digital root" + "\r\n" + /* "Divisors" + ": " + */ Numbers.GetNumberToolTipText(digital_root));
            DigitalRootTextBox.Refresh();

            if (Math.Abs(value) > 1000000000000L)
            {
                TabControl.SelectedIndex = 0;   // Factorization
                Run();
            }
            else
            {
                TabControl.SelectedIndex = 1;   // Index

                List<long> factors = Numbers.Factorize(value);
                if (factors != null)
                {
                    m_number_dimension = factors.Count;
                    if (value < -1)
                        m_number_dimension--;
                    else if (value < 2)
                        m_number_dimension = 0;

                    int nth_number_dimension_a_index = Numbers.NumberDimensionIndexOf(m_number_dimension, value) + 1;
                    int nth_number_dimension_u_index = Numbers.UniqueNumberDimensionIndexOf(m_number_dimension, value) + 1;
                    int nth_number_dimension_d_index = Numbers.DuplicateNumberDimensionIndexOf(m_number_dimension, value) + 1;
                    int nth_similar_powers_index = Numbers.GetSimilarPowersIndex(value) + 1;

                    NumberDimensionTextBox.Text = m_number_dimension.ToString() + "D";
                    //NumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(m_number_dimension);
                    NumberDimensionTextBox.BackColor = Numbers.GetNumberBackColor(m_number_dimension, m_divisor, SystemColors.ControlLight);
                    ToolTip.SetToolTip(this.NumberDimensionTextBox, "Number dimension" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(m_number_dimension));
                    NumberDimensionTextBox.Refresh();

                    NthANumberDimensionTextBox.Text = (nth_number_dimension_a_index > 0) ? nth_number_dimension_a_index.ToString() : "";
                    NthANumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(nth_number_dimension_a_index);
                    NthANumberDimensionTextBox.BackColor = Numbers.GetNumberBackColor(nth_number_dimension_a_index, m_divisor, SystemColors.ControlLight);
                    ToolTip.SetToolTip(this.NthANumberDimensionTextBox, "Any prime factors index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_dimension_a_index));
                    NthANumberDimensionTextBox.Refresh();

                    NthUNumberDimensionTextBox.Text = (nth_number_dimension_u_index > 0) ? nth_number_dimension_u_index.ToString() : "";
                    NthUNumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(nth_number_dimension_u_index);
                    NthUNumberDimensionTextBox.BackColor = Numbers.GetNumberBackColor(nth_number_dimension_u_index, m_divisor, SystemColors.ControlLight);
                    ToolTip.SetToolTip(this.NthUNumberDimensionTextBox, "Unique prime factors index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_dimension_u_index));
                    NthUNumberDimensionTextBox.Refresh();

                    NthDNumberDimensionTextBox.Text = (nth_number_dimension_d_index > 0) ? nth_number_dimension_d_index.ToString() : "";
                    NthDNumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(nth_number_dimension_d_index);
                    NthDNumberDimensionTextBox.BackColor = Numbers.GetNumberBackColor(nth_number_dimension_d_index, m_divisor, SystemColors.ControlLight);
                    ToolTip.SetToolTip(this.NthDNumberDimensionTextBox, "Duplicate prime factors index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_dimension_d_index));
                    NthDNumberDimensionTextBox.Refresh();

                    NthSimilarPowersTextBox.Text = (nth_similar_powers_index > 0) ? nth_similar_powers_index.ToString() : "";
                    NthSimilarPowersTextBox.ForeColor = Numbers.GetNumberForeColor(nth_similar_powers_index);
                    NthSimilarPowersTextBox.BackColor = Numbers.GetNumberBackColor(nth_similar_powers_index, m_divisor, SystemColors.ControlLight);
                    ToolTip.SetToolTip(this.NthSimilarPowersTextBox, "Similar prime factor powers index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_similar_powers_index));
                    NthSimilarPowersTextBox.Refresh();
                }

                string factors_str = Numbers.FactorizeToString(value);
                PrimeFactorsTextBox.Text = factors_str;
                PrimeFactorsTextBox.Refresh();

                long prime_numbers_sum = 0L;
                if (value == 1L)
                {
                    prime_numbers_sum = value;
                }
                else if ((value >= 2L) && (Numbers.IsPrime(value))) // if prime, sum up primes from 2 to p
                {
                    int index = Numbers.PrimeIndexOf(value);
                    for (int i = 0; i <= index; i++)
                    {
                        prime_numbers_sum += Numbers.Primes[i];
                    }
                }
                else if ((value >= 4L) && (Numbers.IsComposite(value)))  // if composite, sum up composites from 4 to c
                {
                    int index = Numbers.CompositeIndexOf(value);
                    for (int i = 0; i <= index; i++)
                    {
                        prime_numbers_sum += Numbers.Composites[i];
                    }
                }

                PrimeNumbersSumTextBox.Text = prime_numbers_sum.ToString();
                PrimeNumbersSumTextBox.ForeColor = Numbers.GetNumberForeColor(prime_numbers_sum);
                PrimeNumbersSumTextBox.BackColor = Numbers.GetNumberBackColor(prime_numbers_sum, m_divisor, SystemColors.Window);
                if (value == 1L)
                {
                    ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "1 is neither prime nor composite");
                }
                else if ((value >= 2L) && (Numbers.IsPrime(value))) // if prime, sum up primes from 2 to p
                {
                    ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "Sum of prime numbers from 2 to" + " " + value.ToString() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(prime_numbers_sum));
                }
                else if ((value >= 4L) && (Numbers.IsComposite(value)))  // if composite, sum up composites from 4 to c
                {
                    ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "Sum of composite numbers from 4 to" + " " + value.ToString() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(prime_numbers_sum));
                }
                PrimeNumbersSumTextBox.Refresh();

                m_index_type = IndexType.Any;
                int nth_number_index = -1;
                int nth_additive_number_index = -1;
                int nth_non_additive_number_index = -1;
                if (Numbers.IsUnit(value))
                {
                    m_index_type = IndexType.Unit;
                    nth_number_index = 0;
                    nth_additive_number_index = 0;
                    nth_non_additive_number_index = 0;

                    Color PBackColor = (nth_additive_number_index > 0) ? Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.AdditivePrime] : Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.NonAdditivePrime];
                    NthNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_number_index, m_divisor, PBackColor);
                    ToolTip.SetToolTip(this.NthNumberTextBox, "The unit index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_index));

                    Color APBackColor = Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.AdditivePrime];
                    NthAdditiveNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_additive_number_index, m_divisor, APBackColor);
                    ToolTip.SetToolTip(this.NthAdditiveNumberTextBox, "The unit index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_index));

                    Color XPBackColor = Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.NonAdditivePrime];
                    NthNonAdditiveNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_non_additive_number_index, m_divisor, XPBackColor);
                    ToolTip.SetToolTip(this.NthNonAdditiveNumberTextBox, "The unit index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_index));
                }
                else if (Numbers.IsPrime(value))
                {
                    m_index_type = IndexType.Prime;
                    nth_number_index = Numbers.PrimeIndexOf(value) + 1;
                    nth_additive_number_index = Numbers.AdditivePrimeIndexOf(value) + 1;
                    nth_non_additive_number_index = Numbers.NonAdditivePrimeIndexOf(value) + 1;

                    Color PBackColor = (nth_additive_number_index > 0) ? Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.AdditivePrime] : Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.NonAdditivePrime];
                    NthNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_number_index, m_divisor, PBackColor);
                    ToolTip.SetToolTip(this.NthNumberTextBox, "Prime index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_index));

                    Color APBackColor = Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.AdditivePrime];
                    NthAdditiveNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_additive_number_index, m_divisor, APBackColor);
                    ToolTip.SetToolTip(this.NthAdditiveNumberTextBox, "Additive prime index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_additive_number_index));

                    Color XPBackColor = Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.NonAdditivePrime];
                    NthNonAdditiveNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_non_additive_number_index, m_divisor, XPBackColor);
                    ToolTip.SetToolTip(this.NthNonAdditiveNumberTextBox, "Non-additive prime index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_non_additive_number_index));
                }
                else if (Numbers.IsComposite(value))
                {
                    m_index_type = IndexType.Composite;
                    nth_number_index = Numbers.CompositeIndexOf(value) + 1;
                    nth_additive_number_index = Numbers.AdditiveCompositeIndexOf(value) + 1;
                    nth_non_additive_number_index = Numbers.NonAdditiveCompositeIndexOf(value) + 1;

                    Color CBackColor = (nth_additive_number_index > 0) ? Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.AdditiveComposite] : Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.NonAdditiveComposite];
                    NthNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_number_index, m_divisor, CBackColor);
                    ToolTip.SetToolTip(this.NthNumberTextBox, "Composite index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_number_index));

                    Color ACBackColor = Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.AdditiveComposite];
                    NthAdditiveNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_additive_number_index, m_divisor, ACBackColor);
                    ToolTip.SetToolTip(this.NthAdditiveNumberTextBox, "Additive composite index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_additive_number_index));

                    Color XCBackColor = Numbers.NUMBER_TYPE_BACKCOLORS[(int)NumberType.NonAdditiveComposite];
                    NthNonAdditiveNumberTextBox.BackColor = Numbers.GetNumberBackColor(nth_non_additive_number_index, m_divisor, XCBackColor);
                    ToolTip.SetToolTip(this.NthNonAdditiveNumberTextBox, "Non-additive composite index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(nth_non_additive_number_index));
                }
                else // big number
                {
                    m_index_type = IndexType.Any;
                    nth_number_index = -1;
                    nth_additive_number_index = -1;
                    nth_non_additive_number_index = -1;

                    NthNumberTextBox.BackColor = SystemColors.ControlLight;
                    ToolTip.SetToolTip(this.NthNumberTextBox, null);

                    NthAdditiveNumberTextBox.BackColor = SystemColors.ControlLight;
                    ToolTip.SetToolTip(this.NthAdditiveNumberTextBox, null);

                    NthNonAdditiveNumberTextBox.BackColor = SystemColors.ControlLight;
                    ToolTip.SetToolTip(this.NthNonAdditiveNumberTextBox, null);
                }

                NthNumberTextBox.Text = (nth_number_index > 0) ? nth_number_index.ToString() : "";
                NthAdditiveNumberTextBox.Text = (nth_additive_number_index > 0) ? nth_additive_number_index.ToString() : "";
                NthNonAdditiveNumberTextBox.Text = (nth_non_additive_number_index > 0) ? nth_non_additive_number_index.ToString() : "";

                NthNumberTextBox.ForeColor = Numbers.GetNumberForeColor(nth_number_index);
                NthAdditiveNumberTextBox.ForeColor = Numbers.GetNumberForeColor(nth_additive_number_index);
                NthNonAdditiveNumberTextBox.ForeColor = Numbers.GetNumberForeColor(nth_non_additive_number_index);

                NthNumberTextBox.Refresh();
                NthAdditiveNumberTextBox.Refresh();
                NthNonAdditiveNumberTextBox.Refresh();

                long n = 0L;
                int plus1_index = -1;
                int minus1_index = -1;
                string plus1_str = "";
                string minus1_str = "";
                if (Numbers.IsUnit(value))
                {
                    plus1_index = 0;
                    minus1_index = 0;
                    plus1_str = "0";
                    minus1_str = "0";
                }
                else if (Numbers.IsPrime(value))
                {
                    plus1_index = Numbers.Prime4nPlus1IndexOf(value) + 1;
                    if (plus1_index > 0)
                    {
                        n = (value - 1L) / 4L;
                        if (n > 0L)
                        {
                            string plus_summed_squares = Numbers.Get4nPlus1EqualsSumOfTwoSquares(value);
                            string plus_diffed_squares = Numbers.Get4nPlus1EqualsDiffOfTwoSquares(value);
                            string plus_summed_cubes = Numbers.Get4nPlus1EqualsSumOfTwoCubes(value);
                            string plus_diffed_cubes = Numbers.Get4nPlus1EqualsDiffOfTwoCubes(value);
                            plus1_str = "4×" + n.ToString() + " + 1"
                                + ((plus_summed_squares != "") ? (" = " + plus_summed_squares) : "")
                                + ((plus_diffed_squares != "") ? (" = " + plus_diffed_squares) : "")
                                + ((plus_summed_cubes != "") ? (" = " + plus_summed_cubes) : "")
                                + ((plus_diffed_cubes != "") ? (" = " + plus_diffed_cubes) : "")
                                ;
                        }
                    }

                    minus1_index = Numbers.Prime4nMinus1IndexOf(value) + 1;
                    if (minus1_index > 0)
                    {
                        n = (value + 1L) / 4L;
                        if (n > 0L)
                        {
                            string minus_summed_squares = Numbers.Get4nMinus1EqualsSumOfTwoSquares(value);
                            string minus_diffed_squares = Numbers.Get4nMinus1EqualsDiffOfTwoSquares(value);
                            string minus_summed_cubes = Numbers.Get4nMinus1EqualsSumOfTwoCubes(value);
                            string minus_diffed_cubes = Numbers.Get4nMinus1EqualsDiffOfTwoCubes(value);
                            minus1_str = "4×" + n.ToString() + " - 1"
                                + ((minus_summed_squares != "") ? (" = " + minus_summed_squares) : "")
                                + ((minus_diffed_squares != "") ? (" = " + minus_diffed_squares) : "")
                                + ((minus_summed_cubes != "") ? (" = " + minus_summed_cubes) : "")
                                + ((minus_diffed_cubes != "") ? (" = " + minus_diffed_cubes) : "")
                                ;
                        }
                    }
                }
                else if (Numbers.IsComposite(value))
                {
                    plus1_index = Numbers.Composite4nPlus1IndexOf(value) + 1;
                    if (plus1_index > 0)
                    {
                        n = (value - 1L) / 4L;
                        if (n > 0L)
                        {
                            string plus_summed_squares = Numbers.Get4nPlus1EqualsSumOfTwoSquares(value);
                            string plus_diffed_squares = Numbers.Get4nPlus1EqualsDiffOfTwoSquares(value);
                            string plus_summed_cubes = Numbers.Get4nPlus1EqualsSumOfTwoCubes(value);
                            string plus_diffed_cubes = Numbers.Get4nPlus1EqualsDiffOfTwoCubes(value);
                            plus1_str = "4×" + n.ToString() + " + 1"
                                + ((plus_summed_squares != "") ? (" = " + plus_summed_squares) : "")
                                + ((plus_diffed_squares != "") ? (" = " + plus_diffed_squares) : "")
                                + ((plus_summed_cubes != "") ? (" = " + plus_summed_cubes) : "")
                                + ((plus_diffed_cubes != "") ? (" = " + plus_diffed_cubes) : "")
                                ;
                        }
                    }

                    minus1_index = Numbers.Composite4nMinus1IndexOf(value) + 1;
                    if (minus1_index > 0)
                    {
                        n = (value + 1L) / 4L;
                        if (n > 0L)
                        {
                            string minus_summed_squares = Numbers.Get4nMinus1EqualsSumOfTwoSquares(value);
                            string minus_diffed_squares = Numbers.Get4nMinus1EqualsDiffOfTwoSquares(value);
                            string minus_summed_cubes = Numbers.Get4nMinus1EqualsSumOfTwoCubes(value);
                            string minus_diffed_cubes = Numbers.Get4nMinus1EqualsDiffOfTwoCubes(value);
                            minus1_str = "4×" + n.ToString() + " - 1"
                                + ((minus_summed_squares != "") ? (" = " + minus_summed_squares) : "")
                                + ((minus_diffed_squares != "") ? (" = " + minus_diffed_squares) : "")
                                + ((minus_summed_cubes != "") ? (" = " + minus_summed_cubes) : "")
                                + ((minus_diffed_cubes != "") ? (" = " + minus_diffed_cubes) : "")
                                ;
                        }
                    }
                }
                else // big number
                {
                    plus1_index = -1;
                    minus1_index = -1;
                    plus1_str = "-1";
                    minus1_str = "-1";
                }
                SquareSumTextBox.Text = (plus1_index > 0) ? plus1_str : (minus1_index > 0) ? minus1_str : "";

                if (plus1_str.StartsWith("4×")) // 4n+1
                {
                    int start = "4×".Length;
                    int end = plus1_str.IndexOf("+");
                    if ((start >= 0) && (end >= start))
                    {
                        string text = plus1_str.Substring(start, end - start);
                        n = long.Parse(text);
                    }
                }
                else if (minus1_str.StartsWith("4×")) // 4n-1
                {
                    int start = "4×".Length;
                    int end = minus1_str.IndexOf("-");
                    if ((start >= 0) && (end >= start))
                    {
                        string text = minus1_str.Substring(start, end - start);
                        n = long.Parse(text);
                    }
                }
                Color n_color = Numbers.GetNumberForeColor(n);
                //double scale = 0.8D;
                //n_color = Color.FromArgb((int)(n_color.R * scale), (int)(n_color.G * scale), (int)(n_color.B * scale));
                SquareSumTextBox.ForeColor = n_color;
                //if (Numbers.IsPrime(n))
                //{
                //    SquareSumTextBox.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[3];
                //}
                //else if (Numbers.IsComposite(n))
                //{
                //    SquareSumTextBox.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[6];
                //}
                //else
                //{
                //    SquareSumTextBox.BackColor = Color.Lavender;
                //}
                //SquareSumTextBox.BackColor = Numbers.GetNumberBackColor(n, m_divisor, SquareSumTextBox.BackColor);
                SquareSumTextBox.BackColor = Numbers.GetNumberBackColor(n, m_divisor, Color.Lavender);
                ToolTip.SetToolTip(this.SquareSumTextBox, "Divisors" + " " + Numbers.GetNumberToolTipText(n) + ((plus1_index > 0) ? plus1_str : (minus1_index > 0) ? minus1_str : ""));
                SquareSumTextBox.Refresh();

                int _4n1_index = (plus1_index > 0) ? plus1_index : (minus1_index > 0) ? minus1_index : 0;
                Nth4n1NumberTextBox.Text = (_4n1_index > 0) ? _4n1_index.ToString() : "";
                Nth4n1NumberTextBox.ForeColor = Numbers.GetNumberForeColor(_4n1_index);
                //if (Numbers.IsPrime(n))
                //{
                //    Nth4n1NumberTextBox.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[3];
                //}
                //else if (Numbers.IsComposite(n))
                //{
                //    Nth4n1NumberTextBox.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[6];
                //}
                //else
                //{
                //    Nth4n1NumberTextBox.BackColor = Color.Lavender;
                //}
                //Nth4n1NumberTextBox.BackColor = Numbers.GetNumberBackColor(_4n1_index, m_divisor, Nth4n1NumberTextBox.BackColor);
                Nth4n1NumberTextBox.BackColor = Numbers.GetNumberBackColor(_4n1_index, m_divisor, Color.Lavender);
                UpdateToolTipNth4n1NumberTextBox();
            }

            UpdateNumberKind(value);
            UpdateSumOfDivisors(value);
            UpdateSumOfNumbers(value);
            UpdatePCIndexChains(value);

            IndexTextBox.TextChanged -= this.ValueTextBox_TextChanged;
            IndexTextBox.Text = value.ToString();
            IndexTextBox.TextChanged += this.ValueTextBox_TextChanged;
            IndexTextBox.ForeColor = Numbers.GetNumberForeColor(value);
            IndexTextBox.BackColor = Numbers.GetNumberBackColor(value, m_divisor, SystemColors.Window);
            ToolTip.SetToolTip(this.IndexTextBox, "Divisors" + " " + Numbers.GetNumberToolTipText(value));
            IndexTextBox.Refresh();

            AfterProcessing();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void NthNumberTextBox_Enter(object sender, EventArgs e)
    {
        if (sender == NthNumberTextBox)
        {
            m_index_subtype = IndexSubType.Any;
        }
        else if (sender == NthAdditiveNumberTextBox)
        {
            m_index_subtype = IndexSubType.Additive;
        }
        else if (sender == NthNonAdditiveNumberTextBox)
        {
            m_index_subtype = IndexSubType.NonAdditive;
        }
        else
        {
            // ignore
        }
    }
    private void DigitsLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (ModifierKeys == Keys.None)
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    // open file for live editing using ISubscriber
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + Globals.INTERESTING_NUMBERS_FILENAME;
                    FileHelper.DisplayFile(path);
                }
            }
            else
            {
                Control_CtrlOrShiftClick(sender, e);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private int m_number_dimension = 1;
    private FactorsType m_factros_type = FactorsType.Any;
    private void NthNumberDimensionTextBox_Enter(object sender, EventArgs e)
    {
        Control control = sender as Control;
        if (control == NthANumberDimensionTextBox)
        {
            m_factros_type = FactorsType.Any;
        }
        else if (control == NthUNumberDimensionTextBox)
        {
            m_factros_type = FactorsType.Unique;
        }
        else if (control == NthDNumberDimensionTextBox)
        {
            m_factros_type = FactorsType.Duplicate;
        }
        else if (control == NthSimilarPowersTextBox)
        {
            m_factros_type = FactorsType.SimilarPowers;
        }

        UpdateNthNumberDimensionLabels();
    }
    private void NthNumberDimensionTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        Control control = sender as Control;
        if (control != null)
        {
            if (e.KeyCode == Keys.Up)
            {
                IncrementNumberDimension(control);
            }
            else if (e.KeyCode == Keys.Down)
            {
                DecrementNumberDimension(control);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                int index = -1;

                int number;
                string text = control.Text;
                if (int.TryParse(text, out number))
                {
                    index = number - 1;
                }

                long value = GetNumberDimensionValue(m_number_dimension, index);
                if (value > -1L)
                {
                    ValueTextBox.Text = value.ToString();
                }
            }
        }
    }
    private void UpdateNthNumberDimensionLabels()
    {
        NthANumberDimensionLabel.BackColor = Color.LightGray;
        NthUNumberDimensionLabel.BackColor = Color.LightGray;
        NthDNumberDimensionLabel.BackColor = Color.LightGray;
        NthSimilarPowersLabel.BackColor = Color.LightGray;

        if (m_factros_type == FactorsType.Any)
        {
            NthANumberDimensionLabel.BackColor = SystemColors.ControlLightLight;
        }
        else if (m_factros_type == FactorsType.Unique)
        {
            NthUNumberDimensionLabel.BackColor = SystemColors.ControlLightLight;
        }
        else if (m_factros_type == FactorsType.Duplicate)
        {
            NthDNumberDimensionLabel.BackColor = SystemColors.ControlLightLight;
        }
        else if (m_factros_type == FactorsType.SimilarPowers)
        {
            NthSimilarPowersLabel.BackColor = SystemColors.ControlLightLight;
        }
        else
        {
        }
    }
    private void IncrementNumberDimension(Control control)
    {
        if (control == NthSimilarPowersTextBox)
        {
            string text = ValueTextBox.Text;
            if (!String.IsNullOrEmpty(text))
            {
                int value;
                if (int.TryParse(text, out value))
                {
                    long number = Numbers.GetSimilarPowersNextNumber(value);
                    if (number > 0L)
                    {
                        ValueTextBox.Text = number.ToString();
                    }
                }
            }
        }
        else
        {
            int index = -1;
            if (control == NumberDimensionTextBox)
            {
                if (m_factros_type == FactorsType.SimilarPowers)
                {
                    m_factros_type = FactorsType.Any;
                }

                if (m_number_dimension < 19)
                {
                    m_number_dimension++;
                    NumberDimensionTextBox.Text = m_number_dimension.ToString() + "D";
                    //NumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(m_number_dimension);
                    NumberDimensionTextBox.Refresh();

                    int number;
                    string text = "";
                    if (m_factros_type == FactorsType.Any)
                    {
                        text = NthANumberDimensionTextBox.Text;
                    }
                    else if (m_factros_type == FactorsType.Unique)
                    {
                        text = NthUNumberDimensionTextBox.Text;
                    }
                    else if (m_factros_type == FactorsType.Duplicate)
                    {
                        text = NthDNumberDimensionTextBox.Text;
                    }

                    if (String.IsNullOrEmpty(text))
                    {
                        text = "1";
                    }
                    if (int.TryParse(text, out number))
                    {
                        index = number - 1;
                    }
                }
            }
            else
            {
                int number;
                string text = control.Text;
                if (String.IsNullOrEmpty(text))
                {
                    text = "0";
                }
                if (int.TryParse(text, out number))
                {
                    number++;
                    index = number - 1;
                }
            }

            long value = GetNumberDimensionValue(m_number_dimension, index);
            if (value > -1L)
            {
                ValueTextBox.Text = value.ToString();
            }
        }
    }
    private void DecrementNumberDimension(Control control)
    {
        if (control == NthSimilarPowersTextBox)
        {
            string text = ValueTextBox.Text;
            if (!String.IsNullOrEmpty(text))
            {
                int value;
                if (int.TryParse(text, out value))
                {
                    long number = Numbers.GetSimilarPowersPriorNumber(value);
                    if (number > 0L)
                    {
                        ValueTextBox.Text = number.ToString();
                    }
                }
            }
        }
        else
        {
            int index = -1;
            if (control == NumberDimensionTextBox)
            {
                if (m_factros_type == FactorsType.SimilarPowers)
                {
                    m_factros_type = FactorsType.Any;
                }

                if (m_number_dimension > 1)
                {
                    m_number_dimension--;
                    NumberDimensionTextBox.Text = m_number_dimension.ToString() + "D";
                    //NumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(m_number_dimension);
                    NumberDimensionTextBox.Refresh();

                    int number;
                    string text = "";
                    if (m_factros_type == FactorsType.Any)
                    {
                        text = NthANumberDimensionTextBox.Text;
                    }
                    else if (m_factros_type == FactorsType.Unique)
                    {
                        text = NthUNumberDimensionTextBox.Text;
                    }
                    else if (m_factros_type == FactorsType.Duplicate)
                    {
                        text = NthDNumberDimensionTextBox.Text;
                    }

                    if (String.IsNullOrEmpty(text))
                    {
                        text = "1";
                    }
                    if (int.TryParse(text, out number))
                    {
                        index = number - 1;
                    }
                }
            }
            else
            {
                int number;
                string text = control.Text;
                if (String.IsNullOrEmpty(text))
                {
                    text = "1";
                }
                if (int.TryParse(text, out number))
                {
                    number--;
                    index = number - 1;
                }
            }

            long value = GetNumberDimensionValue(m_number_dimension, index);
            if (value > -1L)
            {
                ValueTextBox.Text = value.ToString();
            }
        }
    }
    private long GetNumberDimensionValue(int dimension, int index)
    {
        long value = -1L;
        if (m_factros_type == FactorsType.Any)
        {
            if ((dimension >= 1) && (dimension <= Numbers.NumberDimensions.Count))
            {
                if ((index >= 0) && (index < Numbers.NumberDimensions[dimension - 1].Count))
                {
                    value = Numbers.NumberDimensions[dimension - 1][index];
                }
            }
        }
        else if (m_factros_type == FactorsType.Duplicate)
        {
            if (dimension == 1)
                dimension = 2;
            if ((dimension >= 2) && (dimension <= Numbers.DuplicateNumberDimensions.Count + 1))
            {
                if ((index >= 0) && (index < Numbers.DuplicateNumberDimensions[dimension - 1].Count))
                {
                    value = Numbers.DuplicateNumberDimensions[dimension - 1][index];
                }
            }
        }
        else if (m_factros_type == FactorsType.Unique)
        {
            if (dimension == 1)
                dimension = 2;
            if ((dimension >= 2) && (dimension <= Numbers.UniqueNumberDimensions.Count + 1))
            {
                if ((index >= 0) && (index < Numbers.UniqueNumberDimensions[dimension - 1].Count))
                {
                    value = Numbers.UniqueNumberDimensions[dimension - 1][index];
                }
            }
        }
        else if (m_factros_type == FactorsType.SimilarPowers)
        {
            long current_value;
            if (long.TryParse(ValueTextBox.Text, out current_value))
            {
                value = Numbers.GetSimilarPowersNumber(current_value, index);
            }

        }
        return value;
    }
    private void NthNumberDimensionLabel_Click(object sender, EventArgs e)
    {
        try
        {
            if (m_number_dimension > 0)
            {
                Control control = sender as Control;

                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + m_number_dimension.ToString();
                    FileHelper.DisplayFile(path);
                    if (control == NthANumberDimensionLabel)
                    {
                        path += "a.txt";
                    }
                    else if (control == NthUNumberDimensionLabel)
                    {
                        path += "u.txt";
                    }
                    else if (control == NthDNumberDimensionLabel)
                    {
                        path += "d.txt";
                    }
                    FileHelper.DisplayFile(path);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
    }
    private void NthSimilarPowersLabel_Click(object sender, EventArgs e)
    {
        try
        {
            string number_str = ValueTextBox.Text;
            if (!String.IsNullOrEmpty(number_str))
            {
                long number;
                if (long.TryParse(number_str, out number))
                {

                    if (number > 0L)
                    {
                        string filename = "powers" + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".txt";
                        if (Directory.Exists(Globals.NUMBERS_FOLDER))
                        {
                            string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + filename;

                            StringBuilder str = new StringBuilder();
                            List<long> numbers_with_similar_powers = Numbers.GetSimilarPowersNumbers(number);
                            int count = 0;
                            foreach (long n in numbers_with_similar_powers)
                            {
                                str.AppendLine(++count + "\t" + n + "\t" + Numbers.FactorizeToString(n));
                            }
                            FileHelper.SaveText(path, str.ToString());
                            FileHelper.DisplayFile(path);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
    }

    private void PreviousPrimeNumber()
    {
        // guard against multiple runs on multiple ENTER key presses
        if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
            return;

        this.Cursor = Cursors.WaitCursor;
        try
        {
            string value = ValueTextBox.Text + ",0"; // previousprime = nextprime(n,0), thanks to Benjamin Buhrow (bbuhrow@gmail.com) for telling me that.

            BeforeProcessing();
            try
            {
                m_worker = new Factorizer(this, "nextprime", value, m_multithreading);
                if (m_worker != null)
                {
                    m_worker_thread = new Thread(new ThreadStart(m_worker.Run));
                    m_worker_thread.Priority = ThreadPriority.Highest;
                    m_worker_thread.IsBackground = false;
                    m_worker_thread.Start();
                }
            }
            catch
            {
                Cancel();
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void NextPrimeNumber()
    {
        // guard against multiple runs on multiple ENTER key presses
        if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
            return;

        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (ValueTextBox.Text.Length == 0)
                ValueTextBox.Text = "1";
            string value = ValueTextBox.Text;

            BeforeProcessing();
            try
            {
                m_worker = new Factorizer(this, "nextprime", value, m_multithreading);
                if (m_worker != null)
                {
                    m_worker_thread = new Thread(new ThreadStart(m_worker.Run));
                    m_worker_thread.Priority = ThreadPriority.Highest;
                    m_worker_thread.IsBackground = false;
                    m_worker_thread.Start();
                }
            }
            catch
            {
                Cancel();
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private List<string> m_history_items = new List<string>();
    private int m_history_index = -1;
    private void PreviousHistoryItem()
    {
        if ((m_history_index > 0) && (m_history_index < m_history_items.Count))
        {
            m_history_index--;
            ValueTextBox.Text = m_history_items[m_history_index];

            ClearProgress();

            CallRun();
        }
    }
    private void NextHistoryItem()
    {
        if ((m_history_index >= 0) && (m_history_index < m_history_items.Count - 1))
        {
            m_history_index++;
            ValueTextBox.Text = m_history_items[m_history_index];

            ClearProgress();

            CallRun();
        }
    }
    private void PreviousHistoryLabel_Click(object sender, EventArgs e)
    {
        PreviousHistoryItem();
    }
    private void NextHistoryLabel_Click(object sender, EventArgs e)
    {
        NextHistoryItem();
    }

    private void UpdateToolTipNth4n1NumberTextBox()
    {
        long value = Radix.Decode(ValueTextBox.Text, m_radix);

        int plus1_index = -1;
        int minus1_index = -1;
        Nth4nPlus1PrimeNumberLabel.BackColor = SystemColors.ControlLight;
        Nth4nMinus1PrimeNumberLabel.BackColor = SystemColors.ControlLight;
        Nth4nPlus1CompositeNumberLabel.BackColor = SystemColors.ControlLight;
        Nth4nMinus1CompositeNumberLabel.BackColor = SystemColors.ControlLight;
        if (Numbers.IsUnit(value))
        {
            plus1_index = 0;
            minus1_index = 0;
        }
        else if (Numbers.IsPrime(value))
        {
            plus1_index = Numbers.Prime4nPlus1IndexOf(value) + 1;
            minus1_index = Numbers.Prime4nMinus1IndexOf(value) + 1;
        }
        else if (Numbers.IsComposite(value))
        {
            plus1_index = Numbers.Composite4nPlus1IndexOf(value) + 1;
            minus1_index = Numbers.Composite4nMinus1IndexOf(value) + 1;
        }
        else // big number
        {
            plus1_index = -1;
            minus1_index = -1;
        }

        m_plus1_index = (plus1_index > 0);
        m_minus1_index = (minus1_index > 0);
        if (!m_plus1_index && !m_minus1_index)
        {
            m_plus1_index = false; // use default
            m_minus1_index = false; // use default
        }

        if (m_plus1_index || m_minus1_index)
        {
            if (Numbers.IsPrime(value))
            {
                if (m_plus1_index)
                {
                    Nth4nPlus1PrimeNumberLabel.BackColor = SystemColors.ControlLightLight;
                    ToolTip.SetToolTip(this.Nth4n1NumberTextBox, "4n+1 prime index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(plus1_index));
                }
                else if (m_minus1_index)
                {
                    Nth4nMinus1PrimeNumberLabel.BackColor = SystemColors.ControlLightLight;
                    ToolTip.SetToolTip(this.Nth4n1NumberTextBox, "4n-1 prime index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(minus1_index));
                }
                else
                {
                    ToolTip.SetToolTip(this.Nth4n1NumberTextBox, null);
                }
            }
            else // any other index type will be treated as IndexNumberType.Composite
            {
                if (m_plus1_index)
                {
                    Nth4nPlus1CompositeNumberLabel.BackColor = SystemColors.ControlLightLight;
                    ToolTip.SetToolTip(this.Nth4n1NumberTextBox, "4n+1 composite index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(plus1_index));
                }
                else if (m_minus1_index)
                {
                    Nth4nMinus1CompositeNumberLabel.BackColor = SystemColors.ControlLightLight;
                    ToolTip.SetToolTip(this.Nth4n1NumberTextBox, "4n-1 composite index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(minus1_index));
                }
                else
                {
                    ToolTip.SetToolTip(this.Nth4n1NumberTextBox, null);
                }
            }
        }
        else // big number
        {
            ToolTip.SetToolTip(this.Nth4n1NumberTextBox, null);
        }
        Nth4n1NumberTextBox.Refresh();
    }
    private void Nth4nPlus1PrimeNumberLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "primes_4n+1.txt";
                    FileHelper.DisplayFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_index_type = IndexType.Prime;
            m_plus1_index = true;
            m_minus1_index = false;
            FactorizeValue(Nth4n1NumberTextBox);
        }
    }
    private void Nth4nMinus1PrimeNumberLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "primes_4n-1.txt";
                    FileHelper.DisplayFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_index_type = IndexType.Prime;
            m_plus1_index = false;
            m_minus1_index = true;
            FactorizeValue(Nth4n1NumberTextBox);
        }
    }
    private void Nth4nPlus1CompositeNumberLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "composites_4n+1.txt";
                    FileHelper.DisplayFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_index_type = IndexType.Composite;
            m_plus1_index = true;
            m_minus1_index = false;
            FactorizeValue(Nth4n1NumberTextBox);
        }
    }
    private void Nth4nMinus1CompositeNumberLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "composites_4n-1.txt";
                    FileHelper.DisplayFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_index_type = IndexType.Composite;
            m_plus1_index = false;
            m_minus1_index = true;
            FactorizeValue(Nth4n1NumberTextBox);
        }
    }
    private void PrimeNumbersLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = null;
                    switch (m_index_subtype)
                    {
                        case IndexSubType.Any:
                            {
                                path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "primes.txt";
                            }
                            break;
                        case IndexSubType.Additive:
                            {
                                path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "additive_primes.txt";
                            }
                            break;
                        case IndexSubType.NonAdditive:
                            {
                                path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "non_additive_primes.txt";
                            }
                            break;
                    }
                    if (!String.IsNullOrEmpty(path))
                    {
                        FileHelper.DisplayFile(path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_index_type = IndexType.Prime;
            switch (m_index_subtype)
            {
                case IndexSubType.Any:
                    {
                        FactorizeValue(NthNumberTextBox);
                    }
                    break;
                case IndexSubType.Additive:
                    {
                        FactorizeValue(NthAdditiveNumberTextBox);
                    }
                    break;
                case IndexSubType.NonAdditive:
                    {
                        FactorizeValue(NthNonAdditiveNumberTextBox);
                    }
                    break;
            }
        }
    }
    private void CompositeNumbersLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string path = null;
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    switch (m_index_subtype)
                    {
                        case IndexSubType.Any:
                            {
                                path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "composites.txt";
                            }
                            break;
                        case IndexSubType.Additive:
                            {
                                path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "additive_composites.txt";
                            }
                            break;
                        case IndexSubType.NonAdditive:
                            {
                                path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "non_additive_composites.txt";
                            }
                            break;
                    }
                    if (!String.IsNullOrEmpty(path))
                    {
                        FileHelper.DisplayFile(path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_index_type = IndexType.Composite;
            switch (m_index_subtype)
            {
                case IndexSubType.Any:
                    {
                        FactorizeValue(NthNumberTextBox);
                    }
                    break;
                case IndexSubType.Additive:
                    {
                        FactorizeValue(NthAdditiveNumberTextBox);
                    }
                    break;
                case IndexSubType.NonAdditive:
                    {
                        FactorizeValue(NthNonAdditiveNumberTextBox);
                    }
                    break;
            }
        }
    }

    private void UpdateNumberKind(long value)
    {
        if (value < 0L) value *= -1L;
        if (value > 1000000000000L) value = 0L;

        m_number_kind = Numbers.GetNumberKind(value);
        int number_kind_index = 0;
        switch (m_number_kind)
        {
            case NumberKind.Deficient:
                {
                    number_kind_index = Numbers.DeficientNumberIndexOf(value) + 1;
                    NumberKindIndexTextBox.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[0];
                }
                break;
            case NumberKind.Perfect:
                {
                    number_kind_index = Numbers.PerfectNumberIndexOf(value) + 1;
                    NumberKindIndexTextBox.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[1];
                }
                break;
            case NumberKind.Abundant:
                {
                    number_kind_index = Numbers.AbundantNumberIndexOf(value) + 1;
                    NumberKindIndexTextBox.BackColor = Numbers.NUMBER_KIND_BACKCOLORS[2];
                }
                break;
            default:
                {
                    number_kind_index = 0;
                    NumberKindIndexTextBox.BackColor = SystemColors.Control;
                }
                break;
        }

        NumberKindIndexTextBox.Text = number_kind_index.ToString();
        NumberKindIndexTextBox.ForeColor = Numbers.GetNumberForeColor(number_kind_index);
        NumberKindIndexTextBox.BackColor = Numbers.GetNumberBackColor(number_kind_index, m_divisor, NumberKindIndexTextBox.BackColor);
        ToolTip.SetToolTip(this.NumberKindIndexTextBox, m_number_kind.ToString() + " number index" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(number_kind_index));
        NumberKindIndexTextBox.Refresh();
    }
    private void UpdateNumberKind(int index)
    {
        long value = 0L;
        switch (m_number_kind)
        {
            case NumberKind.Perfect:
                {
                    if ((index > 0) && (index < Numbers.PerfectNumbers.Count))
                    {
                        value = Numbers.PerfectNumbers[index - 1];
                    }
                }
                break;
            case NumberKind.Abundant:
                {
                    if ((index > 0) && (index < Numbers.AbundantNumbers.Count))
                    {
                        value = Numbers.AbundantNumbers[index - 1];
                    }
                }
                break;
            case NumberKind.Deficient:
                {
                    if ((index > 0) && (index < Numbers.DeficientNumbers.Count))
                    {
                        value = Numbers.DeficientNumbers[index - 1];
                    }
                }
                break;
        }
        ValueTextBox.Text = value.ToString();
    }
    private void PerfectNumbersLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "perfect_numbers.txt";
                    FileHelper.DisplayFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_number_kind = NumberKind.Perfect;
            int index = 0;
            if (int.TryParse(NumberKindIndexTextBox.Text, out index))
            {
                UpdateNumberKind(index);
            }
        }
    }
    private void AbundantNumbersLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "abundant_numbers.txt";
                    FileHelper.DisplayFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_number_kind = NumberKind.Abundant;
            int index = 0;
            if (int.TryParse(NumberKindIndexTextBox.Text, out index))
            {
                UpdateNumberKind(index);
            }
        }
    }
    private void DeficientNumbersLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (Directory.Exists(Globals.NUMBERS_FOLDER))
                {
                    string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + "deficient_numbers.txt";
                    FileHelper.DisplayFile(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else
        {
            m_number_kind = NumberKind.Deficient;
            int index = 0;
            if (int.TryParse(NumberKindIndexTextBox.Text, out index))
            {
                UpdateNumberKind(index);
            }
        }
    }
    private NumberKind m_number_kind = NumberKind.Deficient;
    private IndexType m_index_type = IndexType.Unit;
    private IndexSubType m_index_subtype = IndexSubType.Any;

    private bool m_plus1_index = false;
    private bool m_minus1_index = false;
    private void FactorizeValue(Control control)
    {
        if (control is TextBoxBase)
        {
            long value = -1L;
            int number;
            if (int.TryParse(control.Text, out number))
            {
                if (number < 0) number *= -1;
                int index = number - 1;
                if (index < 0) return; // needed here

                control.ForeColor = Numbers.GetNumberForeColor(index + 1);

                if (control == NthNumberTextBox)
                {
                    if (m_index_type == IndexType.Prime)
                    {
                        if (index < Numbers.Primes.Count)
                        {
                            value = Numbers.Primes[index];
                        }
                    }
                    else // any other index type will be treated as IndexNumberType.Composite
                    {
                        if (index < Numbers.Composites.Count)
                        {
                            value = Numbers.Composites[index];
                        }
                    }
                }
                else if (control == NthAdditiveNumberTextBox)
                {
                    if (m_index_type == IndexType.Prime)
                    {
                        if (index < Numbers.AdditivePrimes.Count)
                        {
                            value = Numbers.AdditivePrimes[index];
                        }
                    }
                    else // any other index type will be treated as IndexNumberType.Composite
                    {
                        if (index < Numbers.AdditiveComposites.Count)
                        {
                            value = Numbers.AdditiveComposites[index];
                        }
                    }
                }
                else if (control == NthNonAdditiveNumberTextBox)
                {
                    if (m_index_type == IndexType.Prime)
                    {
                        if (index < Numbers.NonAdditivePrimes.Count)
                        {
                            value = Numbers.NonAdditivePrimes[index];
                        }
                    }
                    else // any other index type will be treated as IndexNumberType.Composite
                    {
                        if (index < Numbers.NonAdditiveComposites.Count)
                        {
                            value = Numbers.NonAdditiveComposites[index];
                        }
                    }
                }
                else if (control == Nth4n1NumberTextBox)
                {
                    if (m_index_type == IndexType.Prime)
                    {
                        if (m_plus1_index)
                        {
                            if (index < Numbers.Primes4nPlus1.Count)
                            {
                                value = Numbers.Primes4nPlus1[index];
                            }
                        }
                        else if (m_minus1_index)
                        {
                            if (index < Numbers.Primes4nMinus1.Count)
                            {
                                value = Numbers.Primes4nMinus1[index];
                            }
                        }
                        else
                        {
                            // do nothing
                        }
                    }
                    else // any other index type will be treated as IndexNumberType.Composite
                    {
                        if (m_plus1_index)
                        {
                            if (index < Numbers.Composites4nPlus1.Count)
                            {
                                value = Numbers.Composites4nPlus1[index];
                            }
                        }
                        else if (m_minus1_index)
                        {
                            if (index < Numbers.Composites4nMinus1.Count)
                            {
                                value = Numbers.Composites4nMinus1[index];
                            }
                        }
                        else
                        {
                            // do nothing
                        }
                    }

                    UpdateToolTipNth4n1NumberTextBox();
                }
                else if (control == NumberKindIndexTextBox)
                {
                    switch (m_number_kind)
                    {
                        case NumberKind.Deficient:
                            {
                                if (index < Numbers.DeficientNumbers.Count)
                                {
                                    value = Numbers.DeficientNumbers[index];
                                }
                            }
                            break;
                        case NumberKind.Perfect:
                            {
                                if (index < Numbers.PerfectNumbers.Count)
                                {
                                    value = Numbers.PerfectNumbers[index];
                                }
                            }
                            break;
                        case NumberKind.Abundant:
                            {
                                if (index < Numbers.AbundantNumbers.Count)
                                {
                                    value = Numbers.AbundantNumbers[index];
                                }
                            }
                            break;
                        default:
                            {
                            }
                            break;
                    }
                }
                else if (control == NumberDimensionTextBox)
                {
                    value = m_number_dimension;
                }
                else if ((control == NthANumberDimensionTextBox) ||
                         (control == NthUNumberDimensionTextBox) ||
                         (control == NthDNumberDimensionTextBox))
                {
                    value = GetNumberDimensionValue(m_number_dimension, index);
                }
                else
                {
                    value = number;
                }

                FactorizeValue(value);
            }
            else
            {
                ValueTextBox.Text = "0";
            }
        }
    }
    private void IncrementValue(Control control)
    {
        if (control is TextBoxBase)
        {
            control.Text = control.Text.Replace(" ", "");
            if (control.Text == "")
            {
                control.Text = "1";
            }
            else
            {
                long number;
                if (long.TryParse(control.Text, out number))
                {
                    if (number < long.MaxValue)
                    {
                        number++;
                        control.Text = number.ToString();
                        FactorizeValue(control);
                    }
                }
            }
        }
    }
    private void DecrementValue(Control control)
    {
        if (control is TextBoxBase)
        {
            control.Text = control.Text.Replace(" ", "");
            if (control.Text == "")
            {
                control.Text = "1";
            }
            else
            {
                long number;
                if (long.TryParse(control.Text, out number))
                {
                    if (number > 1L)
                    {
                        number--;
                        control.Text = number.ToString();
                        FactorizeValue(control);
                    }
                }
            }
        }
    }

    private long GetValue(Label control)
    {
        long value = 0L;

        if (control != null)
        {
            try
            {
                string text = control.Text;
                if (!String.IsNullOrEmpty(text))
                {
                    if (text.EndsWith("digit"))
                    {
                        text = text.Substring(0, text.Length - 6);
                    }
                    else if (text.EndsWith("digits"))
                    {
                        text = text.Substring(0, text.Length - 7);
                    }

                    value = Math.Abs((long)double.Parse(text));
                }
            }
            catch
            {
                value = -1L; // error
            }
        }

        return value;
    }
    private long GetValue(TextBox control)
    {
        long value = 0L;

        if (control != null)
        {
            if (control != ValueTextBox)
            {
                try
                {
                    string text = control.Text;
                    if (!String.IsNullOrEmpty(text))
                    {
                        if (control.Name.StartsWith("LetterFrequency"))
                        {
                            value = Math.Abs((long)double.Parse(text));
                        }
                        else if (control.Name.StartsWith("Decimal"))
                        {
                            value = Radix.Decode(text, Numbers.DEFAULT_RADIX);
                        }
                        else if (text.StartsWith("4×")) // 4n+1 or 4n-1
                        {
                            int start = "4×".Length;
                            int end = text.IndexOf("+");
                            if (end == -1)
                                end = text.IndexOf("-");
                            if ((start >= 0) && (end >= start))
                            {
                                text = text.Substring(start, end - start);
                                value = Radix.Decode(text, Numbers.DEFAULT_RADIX);
                            }
                        }
                        else if (text.Contains("×")) // Prime factors
                        {
                            text = text.Replace("×", ""); // concatenate left-to-right
                            value = Radix.Decode(text, Numbers.DEFAULT_RADIX);
                        }
                        else if (text.Contains("*"))
                        {
                            text = text.Replace("*", "");
                            value = Radix.Decode(text, Numbers.DEFAULT_RADIX);
                        }
                        else if (text.Contains("D"))
                        {
                            text = text.Substring(0, text.Length - 1);
                            value = Radix.Decode(text, Numbers.DEFAULT_RADIX);
                        }
                        else
                        {
                            value = Radix.Decode(text, Numbers.DEFAULT_RADIX);
                        }
                    }
                }
                catch
                {
                    value = -1L; // error
                }
            }
        }

        return value;
    }
    private void Control_CtrlOrShiftClick(object sender, EventArgs e)
    {
        long value = 0L;
        if (sender is Label)
        {
            value = GetValue(sender as Label);
        }
        else if (sender is TextBox)
        {
            value = GetValue(sender as TextBox);
        }
        else
        {
            value = -1L;
        }

        if (ModifierKeys == Keys.Control)
        {
            ValueTextBox.Text = value.ToString();
        }
        else if (ModifierKeys == Keys.Shift)
        {
            PrimeCalculator_Launch(value);
        }
    }
    private void PrimeCalculator_Launch(long number)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (File.Exists("PrimeCalculator.exe"))
            {
                System.Diagnostics.Process.Start("PrimeCalculator.exe", number.ToString());
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private void ValueInspectLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            InspectValueCalculations();
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void InspectValueCalculations()
    {
        StringBuilder str = new StringBuilder();

        str.AppendLine("Number\t\t\t=\t" + ValueTextBox.Text);
        str.AppendLine("Digits               " + "\t=\t" + DigitsLabel.Text.Substring(0, DigitsLabel.Text.IndexOf(" ")));
        str.AppendLine("Digit Sum            " + "\t=\t" + DigitSumTextBox.Text);
        str.AppendLine("Digital Root         " + "\t=\t" + DigitalRootTextBox.Text);

        str.AppendLine();
        str.AppendLine("Prime Factors\t\t=\t" + PrimeFactorsTextBox.Text);
        str.AppendLine("Prime Factors Sum\t=\t" + PrimeNumbersSumTextBox.Text);

        str.AppendLine();
        str.AppendLine("Dimension        \t=\t" + NumberDimensionTextBox.Text);
        str.AppendLine("Dimension A Index\t=\t" + NthANumberDimensionTextBox.Text);
        str.AppendLine("Dimension U Index\t=\t" + NthUNumberDimensionTextBox.Text);
        str.AppendLine("Dimension D Index\t=\t" + NthDNumberDimensionTextBox.Text);

        string value_str = ValueTextBox.Text.Replace(" ", "");
        long value = 0L;
        if (long.TryParse(value_str, out value))
        {
            str.AppendLine();
            if (m_index_type == IndexType.Prime)
            {
                str.AppendLine("P  Index\t\t=\t" + NthNumberTextBox.Text);
                str.AppendLine("AP Index\t\t=\t" + NthAdditiveNumberTextBox.Text);
                str.AppendLine("XP Index\t\t=\t" + NthNonAdditiveNumberTextBox.Text);
            }
            else
            {
                str.AppendLine("C  Index\t\t=\t" + NthNumberTextBox.Text);
                str.AppendLine("AC Index\t\t=\t" + NthAdditiveNumberTextBox.Text);
                str.AppendLine("XC Index\t\t=\t" + NthNonAdditiveNumberTextBox.Text);
            }

            str.AppendLine();
            long n = 0L;
            int plus1_index = -1;
            int minus1_index = -1;
            string plus1_str = "";
            string minus1_str = "";
            if (Numbers.IsUnit(value))
            {
                plus1_index = 0;
                minus1_index = 0;
                plus1_str = "0";
                minus1_str = "0";
            }
            else if (Numbers.IsPrime(value))
            {
                plus1_index = Numbers.Prime4nPlus1IndexOf(value) + 1;
                if (plus1_index > 0)
                {
                    n = (value - 1L) / 4L;
                    if (n > 0L)
                    {
                        string plus_summed_squares = Numbers.Get4nPlus1EqualsSumOfTwoSquares(value);
                        string plus_diffed_squares = Numbers.Get4nPlus1EqualsDiffOfTwoSquares(value);
                        string plus_summed_cubes = Numbers.Get4nPlus1EqualsSumOfTwoCubes(value);
                        string plus_diffed_cubes = Numbers.Get4nPlus1EqualsDiffOfTwoCubes(value);
                        plus1_str = "4×" + n.ToString() + " + 1"
                            + ((plus_summed_squares != "") ? (" = " + plus_summed_squares) : "")
                            + ((plus_diffed_squares != "") ? (" = " + plus_diffed_squares) : "")
                            + ((plus_summed_cubes != "") ? (" = " + plus_summed_cubes) : "")
                            + ((plus_diffed_cubes != "") ? (" = " + plus_diffed_cubes) : "")
                            ;
                    }
                }

                minus1_index = Numbers.Prime4nMinus1IndexOf(value) + 1;
                if (minus1_index > 0)
                {
                    n = (value + 1L) / 4L;
                    if (n > 0L)
                    {
                        string minus_summed_squares = Numbers.Get4nMinus1EqualsSumOfTwoSquares(value);
                        string minus_diffed_squares = Numbers.Get4nMinus1EqualsDiffOfTwoSquares(value);
                        string minus_summed_cubes = Numbers.Get4nMinus1EqualsSumOfTwoCubes(value);
                        string minus_diffed_cubes = Numbers.Get4nMinus1EqualsDiffOfTwoCubes(value);
                        minus1_str = "4×" + n.ToString() + " - 1"
                            + ((minus_summed_squares != "") ? (" = " + minus_summed_squares) : "")
                            + ((minus_diffed_squares != "") ? (" = " + minus_diffed_squares) : "")
                            + ((minus_summed_cubes != "") ? (" = " + minus_summed_cubes) : "")
                            + ((minus_diffed_cubes != "") ? (" = " + minus_diffed_cubes) : "")
                            ;
                    }
                }
            }
            else if (Numbers.IsComposite(value))
            {
                plus1_index = Numbers.Composite4nPlus1IndexOf(value) + 1;
                if (plus1_index > 0)
                {
                    n = (value - 1L) / 4L;
                    if (n > 0L)
                    {
                        string plus_summed_squares = Numbers.Get4nPlus1EqualsSumOfTwoSquares(value);
                        string plus_diffed_squares = Numbers.Get4nPlus1EqualsDiffOfTwoSquares(value);
                        string plus_summed_cubes = Numbers.Get4nPlus1EqualsSumOfTwoCubes(value);
                        string plus_diffed_cubes = Numbers.Get4nPlus1EqualsDiffOfTwoCubes(value);
                        plus1_str = "4×" + n.ToString() + " + 1"
                            + ((plus_summed_squares != "") ? (" = " + plus_summed_squares) : "")
                            + ((plus_diffed_squares != "") ? (" = " + plus_diffed_squares) : "")
                            + ((plus_summed_cubes != "") ? (" = " + plus_summed_cubes) : "")
                            + ((plus_diffed_cubes != "") ? (" = " + plus_diffed_cubes) : "")
                            ;
                    }
                }

                minus1_index = Numbers.Composite4nMinus1IndexOf(value) + 1;
                if (minus1_index > 0)
                {
                    n = (value + 1L) / 4L;
                    if (n > 0L)
                    {
                        string minus_summed_squares = Numbers.Get4nMinus1EqualsSumOfTwoSquares(value);
                        string minus_diffed_squares = Numbers.Get4nMinus1EqualsDiffOfTwoSquares(value);
                        string minus_summed_cubes = Numbers.Get4nMinus1EqualsSumOfTwoCubes(value);
                        string minus_diffed_cubes = Numbers.Get4nMinus1EqualsDiffOfTwoCubes(value);
                        minus1_str = "4×" + n.ToString() + " - 1"
                            + ((minus_summed_squares != "") ? (" = " + minus_summed_squares) : "")
                            + ((minus_diffed_squares != "") ? (" = " + minus_diffed_squares) : "")
                            + ((minus_summed_cubes != "") ? (" = " + minus_summed_cubes) : "")
                            + ((minus_diffed_cubes != "") ? (" = " + minus_diffed_cubes) : "")
                            ;
                    }
                }
            }
            else // big number
            {
                plus1_index = -1;
                minus1_index = -1;
                plus1_str = "-1";
                minus1_str = "-1";
            }
            str.AppendLine((plus1_index > 0) ? ("4n+1 Value\t\t=\t" + plus1_str) : (minus1_index > 0) ? ("4n-1 Value\t\t=\t" + minus1_str) : "");
            str.AppendLine(
                            (plus1_index > 0) ? ("4n+1 " + (Numbers.IsComposite(value) ? "Composite" : "Prime") + " Index\t=\t" + plus1_index.ToString()) :
                            (minus1_index > 0) ? ("4n-1 " + (Numbers.IsComposite(value) ? "Composite" : "Prime") + " Index\t=\t" + minus1_index.ToString()) : ""
                          );
            str.AppendLine(" n Divisors\t\t=\t" + Numbers.GetDivisorsString(n));
            str.AppendLine(" n Factors\t\t=\t" + Numbers.FactorizeToString(n));

            str.AppendLine();
            string interesting_equations = value.FindInterestingSums(Numbers.INTERESTINGX_COUNT);
            if (!String.IsNullOrEmpty(interesting_equations))
            {
                str.AppendLine("Interesting numbers\t=\t" + interesting_equations.Replace("\r\n", "\r\n\t\t\t\t"));
            }

            long sum_of_numbers = Numbers.SumOfNumbers(value);
            str.AppendLine("Sum of Numbers\t\t=\t" + sum_of_numbers + ((ModifierKeys == Keys.Control) ? (" = " + Numbers.GetNumbersString(value)) : ""));
            long sum_of_number_digit_sums = Numbers.SumOfNumberDigitSums(value);
            str.AppendLine("Sum of Number DSums\t=\t" + sum_of_number_digit_sums + ((ModifierKeys == Keys.Control) ? (" = " + Numbers.GetNumberDigitSumsString(value)) : ""));
            long sum_of_number_digital_roots = Numbers.SumNumberDigitalRoots(value);
            str.AppendLine("Sum of Number DRoots\t=\t" + sum_of_number_digital_roots + ((ModifierKeys == Keys.Control) ? (" = " + Numbers.GetNumberDigitalRootsString(value)) : ""));

            str.AppendLine();
            m_number_kind = Numbers.GetNumberKind(value);
            int number_kind_index = 0;
            switch (m_number_kind)
            {
                case NumberKind.Deficient:
                    {
                        number_kind_index = Numbers.DeficientNumberIndexOf(value) + 1;
                    }
                    break;
                case NumberKind.Perfect:
                    {
                        number_kind_index = Numbers.PerfectNumberIndexOf(value) + 1;
                    }
                    break;
                case NumberKind.Abundant:
                    {
                        number_kind_index = Numbers.AbundantNumberIndexOf(value) + 1;
                    }
                    break;
                default:
                    {
                        number_kind_index = 0;
                    }
                    break;
            }
            str.AppendLine(m_number_kind.ToString() + " Index\t\t=\t" + number_kind_index);
            str.AppendLine();

            str.AppendLine("Divisors\t\t=\t" + Numbers.GetDivisorsString(value));
            long sum_of_proper_divisors = Numbers.SumOfProperDivisors(value);
            str.AppendLine("Sum of Proper Divisors\t=\t" + sum_of_proper_divisors + ((ModifierKeys == Keys.Control) ? (" = " + Numbers.GetProperDivisorsString(value)) : ""));
            long sum_of_divisors = Numbers.SumOfDivisors(value);
            str.AppendLine("Sum of Divisors\t\t=\t" + sum_of_divisors + ((ModifierKeys == Keys.Control) ? (" = " + Numbers.GetDivisorsString(value)) : ""));
            long sum_of_divisor_digit_sums = Numbers.SumOfDivisorDigitSums(value);
            str.AppendLine("Sum of Divisor DSums\t=\t" + sum_of_divisor_digit_sums + ((ModifierKeys == Keys.Control) ? (" = " + Numbers.GetDivisorDigitSumsString(value)) : ""));
            long sum_of_divisor_digital_roots = Numbers.SumOfDivisorDigitalRoots(value);
            str.AppendLine("Sum of Divisor DRoots\t=\t" + sum_of_divisor_digital_roots + ((ModifierKeys == Keys.Control) ? (" = " + Numbers.GetDivisorDigitalRootsString(value)) : ""));

            str.AppendLine();
            str.AppendLine("PC Index Chain       \t=\t" + PCIndexChainL2RDashString());
            str.AppendLine("PC Index Chain Length\t=\t" + IndexChainLength());
            str.AppendLine("PC Index Chain Sum   \t=\t" + IndexChainSum() + ((ModifierKeys == Keys.Control) ? (" = " + IndexChainSumString()) : ""));
            string l2r_text = IndexChainL2RString();
            long l2r_number = 0L;
            if (long.TryParse(l2r_text, out l2r_number))
            {
                str.AppendLine("PC Index Chain L2R\t=\t" + l2r_text + " = " + Numbers.FactorizeToString(l2r_number));
            }
            else
            {
                str.AppendLine("PC Index Chain L2R\t=\t" + l2r_text);
            }
            string r2l_text = IndexChainR2LString();
            long r2l_number = 0L;
            if (long.TryParse(r2l_text, out r2l_number))
            {
                str.AppendLine("PC Index Chain R2L\t=\t" + r2l_text + " = " + Numbers.FactorizeToString(r2l_number));
            }
            else
            {
                str.AppendLine("PC Index Chain R2L\t=\t" + r2l_text);
            }
            str.AppendLine("PC Index Chain P0C1>\t=\t" + PCIndexChainL2RBinaryString() + " = " + PCIndexChainL2RBinaryValue());
            str.AppendLine("PC Index Chain P1C0>\t=\t" + CPIndexChainL2RBinaryString() + " = " + CPIndexChainL2RBinaryValue());
            str.AppendLine("PC Index Chain P0C1<\t=\t" + PCIndexChainR2LBinaryString() + " = " + PCIndexChainR2LBinaryValue());
            str.AppendLine("PC Index Chain P1C0<\t=\t" + CPIndexChainR2LBinaryString() + " = " + CPIndexChainR2LBinaryValue());
        }

        string filename = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".txt";
        if (Directory.Exists(Globals.NUMBERS_FOLDER))
        {
            string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + filename;
            FileHelper.SaveText(path, str.ToString());

            // show file content after save
            FileHelper.DisplayFile(path);
        }
    }
    private void UpdateSumOfDivisors(long value)
    {
        if (value < 0L) value *= -1L;
        if (value > 1000000000000L) value = 0L;

        Color number_kind_backcolor = SystemColors.Control;
        switch (m_number_kind)
        {
            case NumberKind.Deficient:
                {
                    number_kind_backcolor = Numbers.NUMBER_KIND_BACKCOLORS[0];
                }
                break;
            case NumberKind.Perfect:
                {
                    number_kind_backcolor = Numbers.NUMBER_KIND_BACKCOLORS[1];
                }
                break;
            case NumberKind.Abundant:
                {
                    number_kind_backcolor = Numbers.NUMBER_KIND_BACKCOLORS[2];
                }
                break;
            default:
                {
                }
                break;
        }
        SumOfProperDivisorsTextBox.BackColor = number_kind_backcolor;
        SumOfDivisorsTextBox.BackColor = number_kind_backcolor;
        SumOfDivisorDigitSumsTextBox.BackColor = number_kind_backcolor;
        SumOfDivisorDigitalRootsTextBox.BackColor = number_kind_backcolor;

        long sum_of_divisors = Numbers.SumOfDivisors(value);
        long sum_of_divisor_digit_sums = Numbers.SumOfDivisorDigitSums(value);
        long sum_of_divisor_digital_roots = Numbers.SumOfDivisorDigitalRoots(value);
        string divisors = Numbers.GetDivisorsString(value);
        string divisor_digit_sums = Numbers.GetDivisorDigitSumsString(value);
        string divisor_digital_roots = Numbers.GetDivisorDigitalRootsString(value);

        SumOfDivisorsTextBox.Text = sum_of_divisors.ToString();
        SumOfDivisorsTextBox.ForeColor = Numbers.GetNumberForeColor(sum_of_divisors);
        SumOfDivisorsTextBox.BackColor = Numbers.GetNumberBackColor(sum_of_divisors, m_divisor, number_kind_backcolor);
        ToolTip.SetToolTip(this.SumOfDivisorsTextBox, "Sum of divisors" + "\r\n" + divisors + " = " + sum_of_divisors + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum_of_divisors));
        SumOfDivisorsTextBox.Refresh();

        SumOfDivisorDigitSumsTextBox.Text = sum_of_divisor_digit_sums.ToString();
        SumOfDivisorDigitSumsTextBox.ForeColor = Numbers.GetNumberForeColor(sum_of_divisor_digit_sums);
        SumOfDivisorDigitSumsTextBox.BackColor = Numbers.GetNumberBackColor(sum_of_divisor_digit_sums, m_divisor, number_kind_backcolor);
        ToolTip.SetToolTip(this.SumOfDivisorDigitSumsTextBox, "Sum of divisor digit sums" + "\r\n" + divisor_digit_sums + " = " + sum_of_divisor_digit_sums + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum_of_divisor_digit_sums));
        SumOfDivisorDigitSumsTextBox.Refresh();

        SumOfDivisorDigitalRootsTextBox.Text = sum_of_divisor_digital_roots.ToString();
        SumOfDivisorDigitalRootsTextBox.ForeColor = Numbers.GetNumberForeColor(sum_of_divisor_digital_roots);
        SumOfDivisorDigitalRootsTextBox.BackColor = Numbers.GetNumberBackColor(sum_of_divisor_digital_roots, m_divisor, number_kind_backcolor);
        ToolTip.SetToolTip(this.SumOfDivisorDigitalRootsTextBox, "Sum of divisor digital roots" + "\r\n" + divisor_digital_roots + " = " + sum_of_divisor_digital_roots + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum_of_divisor_digital_roots));
        SumOfDivisorDigitalRootsTextBox.Refresh();

        long sum_of_proper_divisors = Numbers.SumOfProperDivisors(value);
        string proper_divisors = Numbers.GetProperDivisorsString(value);
        SumOfProperDivisorsTextBox.Text = sum_of_proper_divisors.ToString();
        SumOfProperDivisorsTextBox.ForeColor = Numbers.GetNumberForeColor(sum_of_proper_divisors);
        SumOfProperDivisorsTextBox.BackColor = Numbers.GetNumberBackColor(sum_of_proper_divisors, m_divisor, number_kind_backcolor);
        ToolTip.SetToolTip(this.SumOfProperDivisorsTextBox, "Sum of proper divisors" + "\r\n" + proper_divisors + " = " + sum_of_proper_divisors + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum_of_proper_divisors));
        SumOfProperDivisorsTextBox.Refresh();
    }
    private void UpdateSumOfNumbers(long value)
    {
        if (value < 0L) value *= -1L;
        if (value > 1000000000000L) value = 0L;

        SumOfNumbersTextBox.BackColor = SystemColors.ControlLight;
        SumOfDigitSumsTextBox.BackColor = SystemColors.ControlLight;
        SumOfDigitalRootsTextBox.BackColor = SystemColors.ControlLight;

        long sum_of_numbers = Numbers.SumOfNumbers(value);
        SumOfNumbersTextBox.Text = (sum_of_numbers == 0) ? "" : sum_of_numbers.ToString();
        SumOfNumbersTextBox.ForeColor = Numbers.GetNumberForeColor(sum_of_numbers);
        SumOfNumbersTextBox.BackColor = Numbers.GetNumberBackColor(sum_of_numbers, m_divisor, SystemColors.ControlLight);
        ToolTip.SetToolTip(this.SumOfNumbersTextBox, "Sum of numbers from 1 to" + " " + value.ToString() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum_of_numbers));
        SumOfNumbersTextBox.Refresh();

        long sum_of_digit_sums = Numbers.SumOfNumberDigitSums(value);
        SumOfDigitSumsTextBox.Text = (sum_of_digit_sums == 0) ? "" : sum_of_digit_sums.ToString();
        SumOfDigitSumsTextBox.ForeColor = Numbers.GetNumberForeColor(sum_of_digit_sums);
        SumOfDigitSumsTextBox.BackColor = Numbers.GetNumberBackColor(sum_of_digit_sums, m_divisor, SystemColors.ControlLight);
        ToolTip.SetToolTip(this.SumOfDigitSumsTextBox, "Sum of digit sums of numbers from 1 to" + " " + value.ToString() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum_of_digit_sums));
        SumOfDigitSumsTextBox.Refresh();

        long sum_of_digital_roots = Numbers.SumNumberDigitalRoots(value);
        SumOfDigitalRootsTextBox.Text = (sum_of_digital_roots == 0) ? "" : sum_of_digital_roots.ToString();
        SumOfDigitalRootsTextBox.ForeColor = Numbers.GetNumberForeColor(sum_of_digital_roots);
        SumOfDigitalRootsTextBox.BackColor = Numbers.GetNumberBackColor(sum_of_digital_roots, m_divisor, SystemColors.ControlLight);
        ToolTip.SetToolTip(this.SumOfDigitalRootsTextBox, "Sum of digital roots of numbers from 1 to" + " " + value.ToString() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum_of_digital_roots));
        SumOfDigitalRootsTextBox.Refresh();
    }

    private void PrimeNumbersSumTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (
            (e.KeyChar != (char)Keys.Back) &&
            (e.KeyChar != (char)Keys.Home) &&
            (e.KeyChar != (char)Keys.End) &&
            (e.KeyChar != (char)Keys.Delete) &&
            (e.KeyChar != (char)Keys.Up) &&
            (e.KeyChar != (char)Keys.Down) &&
            (e.KeyChar != (char)Keys.Left) &&
            (e.KeyChar != (char)Keys.Right) &&
            (e.KeyChar != '+') &&  // plus
            (e.KeyChar != '-') &&  // minus
            (!Char.IsDigit(e.KeyChar)) &&
            (ModifierKeys != Keys.Control) &&
            (ModifierKeys != Keys.Shift) &&
            (ModifierKeys != Keys.Alt)
           )
        {
            e.Handled = true;
        }
    }
    private void PrimeNumbersSumTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementTargetSum(sender as Control);
            ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, null);
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementTargetSum(sender as Control);
            ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, null);
        }
        else if (e.KeyCode == Keys.Enter)
        {
            FindRangeSumsToTarget();
        }
        else if ((ModifierKeys == Keys.Control) && (e.KeyCode == Keys.N))
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                FindRangeableNumbers();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else if ((ModifierKeys == Keys.Control) && (e.KeyCode == Keys.P))
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                FindPrimeRangeableNumbers();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else if ((ModifierKeys == Keys.Control) && (e.KeyCode == Keys.Q))
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                FindCompositeRangeableNumbers();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else if ((ModifierKeys == Keys.Shift) && (e.KeyCode == Keys.N))
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                FindNonRangeableNumbers();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else if ((ModifierKeys == Keys.Shift) && (e.KeyCode == Keys.P))
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                FindPrimeNonRangeableNumbers();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        else if ((ModifierKeys == Keys.Shift) && (e.KeyCode == Keys.Q))
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                FindCompositeNonRangeableNumbers();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
    }
    private void PrimeNumbersSumTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        long prime_numbers_sum = 0;
        if (long.TryParse(PrimeNumbersSumTextBox.Text, out prime_numbers_sum))
        {
            PrimeNumbersSumTextBox.ForeColor = Numbers.GetNumberForeColor(prime_numbers_sum);
            PrimeNumbersSumTextBox.BackColor = Numbers.GetNumberBackColor(prime_numbers_sum, m_divisor, SystemColors.Window);
            if (prime_numbers_sum == 1L)
            {
                ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "1 is neither prime nor composite");
            }
            else if ((prime_numbers_sum >= 2L) && (Numbers.IsPrime(prime_numbers_sum))) // if prime, sum up primes from 2 to p
            {
                ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "Sum of prime numbers from 2 to" + " " + prime_numbers_sum.ToString() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(prime_numbers_sum));
            }
            else if ((prime_numbers_sum >= 4L) && (Numbers.IsComposite(prime_numbers_sum)))  // if composite, sum up composites from 4 to c
            {
                ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "Sum of composite numbers from 4 to" + " " + prime_numbers_sum.ToString() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(prime_numbers_sum));
            }
            PrimeNumbersSumTextBox.Refresh();
        }
    }
    private void IncrementTargetSum(Control control)
    {
        if (control == PrimeNumbersSumTextBox)
        {
            int target = 0;

            string text = PrimeNumbersSumTextBox.Text;
            if (text.Contains("-"))
            {
                FindRangeSumsToTarget();
                text = PrimeNumbersSumTextBox.Text;
            }

            if (int.TryParse(text, out target))
            {
                if (target < int.MaxValue)
                {
                    target++;
                }
            }

            PrimeNumbersSumTextBox.Text = target.ToString();
        }
    }
    private void DecrementTargetSum(Control control)
    {
        if (control == PrimeNumbersSumTextBox)
        {
            int target = 0;

            string text = PrimeNumbersSumTextBox.Text;
            if (text.Contains("-"))
            {
                FindRangeSumsToTarget();
                text = PrimeNumbersSumTextBox.Text;
            }

            if (int.TryParse(text, out target))
            {
                if (target > 0)
                {
                    target--;
                }
            }

            PrimeNumbersSumTextBox.Text = target.ToString();
        }
    }
    private void FindRangeSumsToTarget()
    {
        string text = PrimeNumbersSumTextBox.Text;
        if (text.Contains("-"))
        {
            CalculateAndDisplaySumOfTypeRange(text);
        }
        else // given target sum, find from_value and to_value
        {
            long target = 0L;
            if (long.TryParse(text, out target))
            {
                if (target > 0L)
                {
                    long from_value = 0L;
                    long to_value = 0L;

                    string find_range_type = "Prime";
                    bool found_prime_range = FindPrimeRangeSumsToTarget(target, ref from_value, ref to_value);
                    if (found_prime_range)
                    {
                        PrimeNumbersSumTextBox.Text = from_value.ToString() + "-" + to_value.ToString();
                        ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, find_range_type + " " + "range" + " " + "sum" + " = " + target.ToString());
                        PrimeNumbersSumTextBox.ForeColor = Color.Black;
                        PrimeNumbersSumTextBox.BackColor = SystemColors.Window;
                        PrimeNumbersSumTextBox.Refresh();
                    }

                    find_range_type = "Composite";
                    bool found_composite_range = FindCompositeRangeSumsToTarget(target, ref from_value, ref to_value);
                    if (found_composite_range)
                    {
                        // keep time to view prime range, then display composite range
                        if (found_prime_range)
                        {
                            //this.Cursor = Cursors.WaitCursor;
                            try
                            {
                                // 7 days per week, 12 months per year, 30 days per month
                                // 7*12*30 = 2520 is divisible by all numberrs from 1 to 10
                                Thread.Sleep(2520);
                            }
                            finally
                            {
                                //this.Cursor = Cursors.Default;
                            }
                        }

                        PrimeNumbersSumTextBox.Text = from_value.ToString() + "-" + to_value.ToString();
                        ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, find_range_type + " " + "range" + " " + "sum" + " = " + target.ToString());
                        PrimeNumbersSumTextBox.ForeColor = Color.Black;
                        PrimeNumbersSumTextBox.BackColor = SystemColors.Window;
                        PrimeNumbersSumTextBox.Refresh();
                    }
                }
            }
        }
    }
    private void CalculateAndDisplaySumOfTypeRange(string text)
    {
        string[] separators = { "-" };
        string[] parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            long from_value = 0L;
            long to_value = 0L;
            if ((long.TryParse(parts[0], out from_value)) && (long.TryParse(parts[1], out to_value)))
            {
                if ((from_value >= 0L) && (to_value > from_value))
                {
                    long sum = 0L;
                    if (from_value <= 1L)
                    {
                        if (to_value == 1L)
                        {
                            sum = to_value;
                            ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "1 is neither prime nor composite");
                        }
                        else if (Numbers.IsPrime(to_value))
                        {
                            from_value = 2L;
                        }
                        else if (Numbers.IsComposite(to_value))
                        {
                            from_value = 4L;
                        }
                    }

                    if (Numbers.IsPrime(from_value)) // if prime, sum up primes from_value to p <= to_value
                    {
                        int from_index = Numbers.PrimeIndexOf(from_value);
                        int to_index = Numbers.PrimeIndexOf(to_value);
                        while (to_index == -1)
                        {
                            to_value--;
                            to_index = Numbers.PrimeIndexOf(to_value);
                        }
                        if (from_value <= to_value)
                        {
                            for (int i = from_index; i <= to_index; i++)
                            {
                                sum += Numbers.Primes[i];
                            }
                            ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "Sum of prime numbers from" + " " + from_value.ToString() + " " + "to" + " " + to_value.ToString());
                        }
                    }
                    else if (Numbers.IsComposite(from_value)) // if composite, sum up composite from_value to c <= to_value
                    {
                        int from_index = Numbers.CompositeIndexOf(from_value);
                        int to_index = Numbers.CompositeIndexOf(to_value);
                        while (to_index == -1)
                        {
                            to_value--;
                            to_index = Numbers.CompositeIndexOf(to_value);
                        }
                        if (from_value <= to_value)
                        {
                            for (int i = from_index; i <= to_index; i++)
                            {
                                sum += Numbers.Composites[i];
                            }
                            ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "Sum of composite numbers from" + " " + from_value.ToString() + " " + "to" + " " + to_value.ToString());
                        }
                    }
                    PrimeNumbersSumTextBox.Text = sum.ToString();
                    PrimeNumbersSumTextBox.ForeColor = Numbers.GetNumberForeColor(sum);
                    PrimeNumbersSumTextBox.BackColor = Numbers.GetNumberBackColor(sum, m_divisor, SystemColors.Window);
                    PrimeNumbersSumTextBox.Refresh();
                }
            }
        }
    }
    private bool FindPrimeRangeSumsToTarget(long target, ref long from_value, ref long to_value)
    {
        long sum = 0L;
        int start_index = 0;
        long start_p = 2L;
        bool found = false;
        do
        {
            int index = start_index;
            if ((index >= 0) && (index < Numbers.Primes.Count))
            {
                sum = Numbers.Primes[index];
                from_value = sum;
                do
                {
                    index++;
                    if ((index >= 0) && (index < Numbers.Primes.Count))
                    {
                        long p = Numbers.Primes[index];

                        sum += p;
                        if (sum == target)
                        {
                            to_value = p;
                            found = true;
                            break;
                        }
                    }
                    else
                        break;
                } while (sum < target);
                if (found)
                    break;
            }
            else
                break;

            start_index++;
            if ((start_index >= 0) && (start_index < Numbers.Primes.Count))
            {
                start_p = Numbers.Primes[start_index];
            }
            else
                break;
        } while (start_p < target);

        return found;
    }
    private bool FindCompositeRangeSumsToTarget(long target, ref long from_value, ref long to_value)
    {
        long sum = 0L;
        int start_index = 0;
        long start_c = 2L;
        bool found = false;
        do
        {
            int index = start_index;
            if ((index >= 0) && (index < Numbers.Composites.Count))
            {
                sum = Numbers.Composites[index];
                from_value = sum;
                do
                {
                    index++;
                    if ((index >= 0) && (index < Numbers.Composites.Count))
                    {
                        long c = Numbers.Composites[index];

                        sum += c;
                        if (sum == target)
                        {
                            to_value = c;
                            found = true;
                            break;
                        }
                    }
                    else
                        break;
                } while (sum < target);
                if (found)
                    break;
            }
            else
                break;

            start_index++;
            if ((start_index >= 0) && (start_index < Numbers.Composites.Count))
            {
                start_c = Numbers.Composites[start_index];
            }
            else
                break;
        } while (start_c < target);

        return found;
    }
    private void FindRangeableNumbers()
    {
        List<string> result = new List<string>();

        long from_value = 0L;
        long to_value = 0L;
        int limit = (Globals.EDITION == Edition.Standard) ? 1000 :
                    (Globals.EDITION == Edition.Research) ? 10000 :
                    (Globals.EDITION == Edition.Ultimate) ? 100000 :
                    (Globals.EDITION == Edition.BigNumbers) ? 1000000 : 0;
        for (int target = 1; target <= limit; target++)
        {
            string line = null;

            bool found_prime_range = false;
            if (FindPrimeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                line = target.ToString() + "\t" + from_value.ToString() + "-" + to_value.ToString();
                found_prime_range = true;
            }

            if (FindCompositeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                if (found_prime_range && (!String.IsNullOrEmpty(line)))
                {
                    line += "\t" + from_value.ToString() + "-" + to_value.ToString();
                }
                else
                {
                    line = target.ToString() + "\t" + from_value.ToString() + "-" + to_value.ToString();
                }
            }

            if (!String.IsNullOrEmpty(line))
            {
                result.Add(line);
            }
        }

        string path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + "rangeable" + ".txt";
        FileHelper.SaveLines(path, result);
        FileHelper.DisplayFile(path);
    }
    private void FindPrimeRangeableNumbers()
    {
        List<string> result = new List<string>();

        long from_value = 0L;
        long to_value = 0L;
        int limit = (Globals.EDITION == Edition.Standard) ? 1000 :
                    (Globals.EDITION == Edition.Research) ? 10000 :
                    (Globals.EDITION == Edition.Ultimate) ? 100000 :
                    (Globals.EDITION == Edition.BigNumbers) ? 1000000 : 0;
        for (int target = 1; target <= limit; target++)
        {
            string line = null;
            if (FindPrimeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                line = target.ToString() + "\t" + from_value.ToString() + "-" + to_value.ToString();
                result.Add(line);
            }
        }

        string path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + "prime_rangeable" + ".txt";
        FileHelper.SaveLines(path, result);
        FileHelper.DisplayFile(path);
    }
    private void FindCompositeRangeableNumbers()
    {
        List<string> result = new List<string>();

        long from_value = 0L;
        long to_value = 0L;
        int limit = (Globals.EDITION == Edition.Standard) ? 1000 :
                    (Globals.EDITION == Edition.Research) ? 10000 :
                    (Globals.EDITION == Edition.Ultimate) ? 100000 :
                    (Globals.EDITION == Edition.BigNumbers) ? 1000000 : 0;
        for (int target = 1; target <= limit; target++)
        {
            string line = null;
            if (FindCompositeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                line = target.ToString() + "\t" + from_value.ToString() + "-" + to_value.ToString();
                result.Add(line);
            }
        }

        string path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + "composite_rangeable" + ".txt";
        FileHelper.SaveLines(path, result);
        FileHelper.DisplayFile(path);
    }
    private void FindNonRangeableNumbers()
    {
        List<long> result = new List<long>();

        long from_value = 0L;
        long to_value = 0L;
        int limit = (Globals.EDITION == Edition.Standard) ? 1000 :
                    (Globals.EDITION == Edition.Research) ? 10000 :
                    (Globals.EDITION == Edition.Ultimate) ? 100000 :
                    (Globals.EDITION == Edition.BigNumbers) ? 1000000 : 0;
        for (int target = 1; target <= limit; target++)
        {
            if (FindPrimeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                continue;
            }
            if (FindCompositeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                continue;
            }
            result.Add(target);
        }

        string path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + "nonrangeable" + ".txt";
        FileHelper.SaveValues(path, result);
        FileHelper.DisplayFile(path);
    }
    private void FindPrimeNonRangeableNumbers()
    {
        List<long> result = new List<long>();

        long from_value = 0L;
        long to_value = 0L;
        int limit = (Globals.EDITION == Edition.Standard) ? 1000 :
                    (Globals.EDITION == Edition.Research) ? 10000 :
                    (Globals.EDITION == Edition.Ultimate) ? 100000 :
                    (Globals.EDITION == Edition.BigNumbers) ? 1000000 : 0;
        for (int target = 1; target <= limit; target++)
        {
            if (!FindPrimeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                result.Add(target);
            }
        }

        string path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + "prime_nonrangeable" + ".txt";
        FileHelper.SaveValues(path, result);
        FileHelper.DisplayFile(path);
    }
    private void FindCompositeNonRangeableNumbers()
    {
        List<long> result = new List<long>();

        long from_value = 0L;
        long to_value = 0L;
        int limit = (Globals.EDITION == Edition.Standard) ? 1000 :
                    (Globals.EDITION == Edition.Research) ? 10000 :
                    (Globals.EDITION == Edition.Ultimate) ? 100000 :
                    (Globals.EDITION == Edition.BigNumbers) ? 1000000 : 0;
        for (int target = 1; target <= limit; target++)
        {
            if (!FindCompositeRangeSumsToTarget(target, ref from_value, ref to_value))
            {
                result.Add(target);
            }
        }

        string path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + "composite_nonrangeable" + ".txt";
        FileHelper.SaveValues(path, result);
        FileHelper.DisplayFile(path);
    }

    private Dictionary<int, string> m_index_chain = null;
    private void GenerateIndexChain(long number)
    {
        if (number < 0L) number *= -1L;
        if (number > 1000000L) number = 0L;

        if (m_index_chain == null)
        {
            m_index_chain = new Dictionary<int, string>();
        }
        else
        {
            m_index_chain.Clear();
        }

        if (m_index_chain != null)
        {
            while (number > 1L)
            {
                int index = 0;
                if ((index = (Numbers.PrimeIndexOf(number) + 1)) > 0)
                {
                    m_index_chain.Add(index, "P");
                }
                else if ((index = (Numbers.CompositeIndexOf(number) + 1)) > 0)
                {
                    m_index_chain.Add(index, "C");
                }
                else // number is too large
                {
                    return;
                }
                number = index;
            }
        }
    }
    private int IndexChainLength()
    {
        if (m_index_chain != null)
        {
            return m_index_chain.Count;
        }
        return 0;
    }
    private int IndexChainSum()
    {
        int result = 0;
        if (m_index_chain != null)
        {
            foreach (int index in m_index_chain.Keys)
            {
                result += index;
            }
        }
        return result;
    }
    private string IndexChainSumString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            foreach (int key in m_index_chain.Keys)
            {
                str.Append(key + "+");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1); // "+"
            }
        }
        return str.ToString();
    }
    private string IndexChainL2RString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            foreach (int key in m_index_chain.Keys)
            {
                str.Append(key);
            }
        }
        return str.ToString();
    }
    private string IndexChainR2LString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            foreach (int key in m_index_chain.Keys)
            {
                str.Insert(0, key);
            }
        }
        return str.ToString();
    }
    private long IndexChainL2RValue()
    {
        long result = 0L;
        string text = IndexChainL2RString();
        if (!String.IsNullOrEmpty(text))
        {
            if (long.TryParse(text, out result))
            {
                // success
            }
        }
        return result;
    }
    private long IndexChainR2LValue()
    {
        long result = 0L;
        string text = IndexChainR2LString();
        if (!String.IsNullOrEmpty(text))
        {
            if (long.TryParse(text, out result))
            {
                // success
            }
        }
        return result;
    }
    private string PCIndexChainL2RBinaryString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            if (m_index_chain.Count > 0)
            {
                foreach (int key in m_index_chain.Keys)
                {
                    if (m_index_chain[key] == "P")
                    {
                        str.Append("0");
                    }
                    else if (m_index_chain[key] == "C")
                    {
                        str.Append("1");
                    }
                    else
                    {
                        // do nothing
                    }
                }
            }
        }
        return str.ToString();
    }
    private string PCIndexChainR2LBinaryString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            if (m_index_chain.Count > 0)
            {
                foreach (int key in m_index_chain.Keys)
                {
                    if (m_index_chain[key] == "P")
                    {
                        str.Insert(0, "0");
                    }
                    else if (m_index_chain[key] == "C")
                    {
                        str.Insert(0, "1");
                    }
                    else
                    {
                        // do nothing
                    }
                }
            }
        }
        return str.ToString();
    }
    private string CPIndexChainL2RBinaryString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            if (m_index_chain.Count > 0)
            {
                foreach (int key in m_index_chain.Keys)
                {
                    if (m_index_chain[key] == "P")
                    {
                        str.Append("1");
                    }
                    else if (m_index_chain[key] == "C")
                    {
                        str.Append("0");
                    }
                    else
                    {
                        // do nothing
                    }
                }
            }
        }
        return str.ToString();
    }
    private string CPIndexChainR2LBinaryString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            if (m_index_chain.Count > 0)
            {
                foreach (int key in m_index_chain.Keys)
                {
                    if (m_index_chain[key] == "P")
                    {
                        str.Insert(0, "1");
                    }
                    else if (m_index_chain[key] == "C")
                    {
                        str.Insert(0, "0");
                    }
                    else
                    {
                        // do nothing
                    }
                }
            }
        }
        return str.ToString();
    }
    private long PCIndexChainL2RBinaryValue()
    {
        string binary_chain = PCIndexChainL2RBinaryString();
        if (!String.IsNullOrEmpty(binary_chain))
        {
            return Convert.ToInt64(binary_chain, 2);
        }
        else
        {
            return -1L;
        }
    }
    private long PCIndexChainR2LBinaryValue()
    {
        string binary_chain = PCIndexChainR2LBinaryString();
        if (!String.IsNullOrEmpty(binary_chain))
        {
            return Convert.ToInt64(binary_chain, 2);
        }
        else
        {
            return -1L;
        }
    }
    private long CPIndexChainL2RBinaryValue()
    {
        string binary_chain = CPIndexChainL2RBinaryString();
        if (!String.IsNullOrEmpty(binary_chain))
        {
            return Convert.ToInt64(binary_chain, 2);
        }
        else
        {
            return -1L;
        }
    }
    private long CPIndexChainR2LBinaryValue()
    {
        string binary_chain = CPIndexChainR2LBinaryString();
        if (!String.IsNullOrEmpty(binary_chain))
        {
            return Convert.ToInt64(binary_chain, 2);
        }
        else
        {
            return -1L;
        }
    }
    private string PCIndexChainL2RDashString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            if (m_index_chain.Count > 0)
            {
                foreach (int key in m_index_chain.Keys)
                {
                    str.Append(m_index_chain[key] + key.ToString() + "-");
                }
                if (str.Length > 0)
                {
                    str.Remove(str.Length - 1, 1); // "-"
                }
            }
        }
        return str.ToString();
    }
    private string PCIndexChainR2LDashString()
    {
        StringBuilder str = new StringBuilder();
        if (m_index_chain != null)
        {
            if (m_index_chain.Count > 0)
            {
                foreach (int key in m_index_chain.Keys)
                {
                    str.Insert(0, m_index_chain[key] + key.ToString() + "-");
                }
                if (str.Length > 0)
                {
                    str.Remove(str.Length - 1, 1); // "-"
                }
            }
        }
        return str.ToString();
    }
    private string CPIndexChainL2RDashString()
    {
        return PCIndexChainL2RDashString();
    }
    private string CPIndexChainR2LDashString()
    {
        return PCIndexChainR2LDashString();
    }
    private void PCIndexChainL2RTextBox_TextChanged(object sender, EventArgs e)
    {
        ToolTip.SetToolTip(this.PCIndexChainL2RTextBox, PCIndexChainL2RDashString() + "\r\n" + "P=0 C=1:\t" + PCIndexChainL2RBinaryString() + " = " + PCIndexChainL2RBinaryValue());
    }
    private void PCIndexChainR2LTextBox_TextChanged(object sender, EventArgs e)
    {
        ToolTip.SetToolTip(this.PCIndexChainR2LTextBox, PCIndexChainR2LDashString() + "\r\n" + "P=0 C=1:\t" + PCIndexChainR2LBinaryString() + " = " + PCIndexChainR2LBinaryValue());
    }
    private void CPIndexChainL2RTextBox_TextChanged(object sender, EventArgs e)
    {
        ToolTip.SetToolTip(this.CPIndexChainL2RTextBox, CPIndexChainL2RDashString() + "\r\n" + "C=0 P=1:\t" + CPIndexChainL2RBinaryString() + " = " + CPIndexChainL2RBinaryValue());
    }
    private void CPIndexChainR2LTextBox_TextChanged(object sender, EventArgs e)
    {
        ToolTip.SetToolTip(this.CPIndexChainR2LTextBox, CPIndexChainR2LDashString() + "\r\n" + "C=0 P=1:\t" + CPIndexChainR2LBinaryString() + " = " + CPIndexChainR2LBinaryValue());
    }
    private void IndexChainSumTextBox_TextChanged(object sender, EventArgs e)
    {
        ToolTip.SetToolTip(this.IndexChainSumTextBox, "Index chain sum = " + IndexChainSumString() + " = " + IndexChainSum());
    }
    private void IndexChainLengthTextBox_TextChanged(object sender, EventArgs e)
    {
        // SLOW CODE
        //StringBuilder str = new StringBuilder();
        //str.AppendLine("Index Chain Sum              \t\t= " + IndexChainSum() + " = " + IndexChainSumString());
        //string l2r_text = IndexChainL2RString();
        //long l2r_number = 0L;
        //if (long.TryParse(l2r_text, out l2r_number))
        //{
        //    str.AppendLine("Index Chain L2R Concatenation\t= " + l2r_text + " = " + Numbers.FactorizeToString(l2r_number));
        //}
        //else
        //{
        //    str.AppendLine("Index Chain L2R Concatenation\t= " + l2r_text);
        //}
        //string r2l_text = IndexChainR2LString();
        //long r2l_number = 0L;
        //if (long.TryParse(r2l_text, out r2l_number))
        //{
        //    str.AppendLine("Index Chain R2L Concatenation\t= " + r2l_text + " = " + Numbers.FactorizeToString(r2l_number));
        //}
        //else
        //{
        //    str.AppendLine("Index Chain R2L Concatenation\t= " + r2l_text);
        //}
        //ToolTip.SetToolTip(this.IndexChainLengthTextBox, str.ToString());
    }
    private void IndexChainLengthTextBox_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            Control_CtrlOrShiftClick(sender, e);
        }
        else
        {
            int length = 0;
            if (int.TryParse(IndexChainLengthTextBox.Text, out length))
            {
                GenerateAllNumbersOfIndexChainLength(length);
            }
        }
    }
    private void GenerateAllNumbersOfIndexChainLength(int length)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            string path = "PIndexChainLength" + "_" + length + ".txt";
            StringBuilder str = new StringBuilder();
            long running_total = 0L;
            for (int i = 0; i < Numbers.Primes.Count; i++)
            {
                long number = Numbers.Primes[i];
                GenerateIndexChain(number);
                int len = IndexChainLength();
                if (len == length)
                {
                    running_total += number;
                    str.AppendLine(number.ToString() + "\t" + running_total.ToString() + "\t" + PCIndexChainL2RBinaryValue().ToString() + "\t" + PCIndexChainR2LBinaryValue().ToString() + "\t" + CPIndexChainL2RBinaryValue().ToString() + "\t" + CPIndexChainR2LBinaryValue().ToString() + "\t" + PCIndexChainL2RDashString());
                }
            }

            path = "APIndexChainLength" + "_" + length + ".txt";
            str = new StringBuilder();
            running_total = 0L;
            for (int i = 0; i < Numbers.AdditivePrimes.Count; i++)
            {
                long number = Numbers.AdditivePrimes[i];
                GenerateIndexChain(number);
                int len = IndexChainLength();
                if (len == length)
                {
                    running_total += number;
                    str.AppendLine(number.ToString() + "\t" + running_total.ToString() + "\t" + PCIndexChainL2RBinaryValue().ToString() + "\t" + PCIndexChainR2LBinaryValue().ToString() + "\t" + CPIndexChainL2RBinaryValue().ToString() + "\t" + CPIndexChainR2LBinaryValue().ToString() + "\t" + PCIndexChainL2RDashString());
                }
            }

            path = "XPIndexChainLength" + "_" + length + ".txt";
            str = new StringBuilder();
            running_total = 0L;
            for (int i = 0; i < Numbers.NonAdditivePrimes.Count; i++)
            {
                long number = Numbers.NonAdditivePrimes[i];
                GenerateIndexChain(number);
                int len = IndexChainLength();
                if (len == length)
                {
                    running_total += number;
                    str.AppendLine(number.ToString() + "\t" + running_total.ToString() + "\t" + PCIndexChainL2RBinaryValue().ToString() + "\t" + PCIndexChainR2LBinaryValue().ToString() + "\t" + CPIndexChainL2RBinaryValue().ToString() + "\t" + CPIndexChainR2LBinaryValue().ToString() + "\t" + PCIndexChainL2RDashString());
                }
            }

            path = "CIndexChainLength" + "_" + length + ".txt";
            str = new StringBuilder();
            running_total = 0L;
            for (int i = 0; i < Numbers.Composites.Count; i++)
            {
                long number = Numbers.Composites[i];
                GenerateIndexChain(number);
                int len = IndexChainLength();
                if (len == length)
                {
                    running_total += number;
                    str.AppendLine(number.ToString() + "\t" + running_total.ToString() + "\t" + PCIndexChainL2RBinaryValue().ToString() + "\t" + PCIndexChainR2LBinaryValue().ToString() + "\t" + CPIndexChainL2RBinaryValue().ToString() + "\t" + CPIndexChainR2LBinaryValue().ToString() + "\t" + PCIndexChainL2RDashString());
                }
            }

            path = "ACIndexChainLength" + "_" + length + ".txt";
            str = new StringBuilder();
            running_total = 0L;
            for (int i = 0; i < Numbers.AdditiveComposites.Count; i++)
            {
                long number = Numbers.AdditiveComposites[i];
                GenerateIndexChain(number);
                int len = IndexChainLength();
                if (len == length)
                {
                    running_total += number;
                    str.AppendLine(number.ToString() + "\t" + running_total.ToString() + "\t" + PCIndexChainL2RBinaryValue().ToString() + "\t" + PCIndexChainR2LBinaryValue().ToString() + "\t" + CPIndexChainL2RBinaryValue().ToString() + "\t" + CPIndexChainR2LBinaryValue().ToString() + "\t" + PCIndexChainL2RDashString());
                }
            }

            path = "XCIndexChainLength" + "_" + length + ".txt";
            str = new StringBuilder();
            running_total = 0L;
            for (int i = 0; i < Numbers.NonAdditiveComposites.Count; i++)
            {
                long number = Numbers.NonAdditiveComposites[i];
                GenerateIndexChain(number);
                int len = IndexChainLength();
                if (len == length)
                {
                    running_total += number;
                    str.AppendLine(number.ToString() + "\t" + running_total.ToString() + "\t" + PCIndexChainL2RBinaryValue().ToString() + "\t" + PCIndexChainR2LBinaryValue().ToString() + "\t" + CPIndexChainL2RBinaryValue().ToString() + "\t" + CPIndexChainR2LBinaryValue().ToString() + "\t" + PCIndexChainL2RDashString());
                }
            }

            path = "NIndexChainLength" + "_" + length + ".txt";
            str = new StringBuilder();
            running_total = 0L;
            for (int i = 0; i < int.MaxValue / 1024; i++)
            {
                GenerateIndexChain(i);
                int len = IndexChainLength();
                if (len == length)
                {
                    long number = i;
                    running_total += number;
                    str.AppendLine(number.ToString() + "\t" + running_total.ToString() + "\t" + PCIndexChainL2RBinaryValue().ToString() + "\t" + PCIndexChainR2LBinaryValue().ToString() + "\t" + CPIndexChainL2RBinaryValue().ToString() + "\t" + CPIndexChainR2LBinaryValue().ToString() + "\t" + PCIndexChainL2RDashString());
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
        finally
        {
            // restore m_index_chain
            long value;
            if (long.TryParse(ValueTextBox.Text, out value))
            {
                GenerateIndexChain(value);
            }

            this.Cursor = Cursors.Default;
        }
    }
    private void UpdatePCIndexChains(long value)
    {
        GenerateIndexChain(value);

        CPIndexChainL2RTextBox.BackColor = Color.MistyRose;
        PCIndexChainR2LTextBox.BackColor = Color.MistyRose;
        PCIndexChainL2RTextBox.BackColor = Color.MistyRose;
        CPIndexChainR2LTextBox.BackColor = Color.MistyRose;
        IndexChainSumTextBox.BackColor = Color.Pink;
        IndexChainLengthTextBox.BackColor = Color.Pink;

        long index = PCIndexChainL2RBinaryValue();
        PCIndexChainL2RTextBox.Text = index.ToString();
        PCIndexChainL2RTextBox.ForeColor = Numbers.GetNumberForeColor(index);
        PCIndexChainL2RTextBox.BackColor = Numbers.GetNumberBackColor(index, m_divisor, Color.MistyRose);
        ToolTip.SetToolTip(this.PCIndexChainL2RTextBox, PCIndexChainL2RDashString() + "\r\n" + "P=0 C=1:\t" + PCIndexChainL2RBinaryString() + " = " + PCIndexChainL2RBinaryValue() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(index));
        PCIndexChainL2RTextBox.Refresh();

        index = PCIndexChainR2LBinaryValue();
        PCIndexChainR2LTextBox.Text = index.ToString();
        PCIndexChainR2LTextBox.ForeColor = Numbers.GetNumberForeColor(index);
        PCIndexChainR2LTextBox.BackColor = Numbers.GetNumberBackColor(index, m_divisor, Color.MistyRose);
        ToolTip.SetToolTip(this.PCIndexChainR2LTextBox, PCIndexChainR2LDashString() + "\r\n" + "P=0 C=1:\t" + PCIndexChainR2LBinaryString() + " = " + PCIndexChainR2LBinaryValue() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(index));
        PCIndexChainR2LTextBox.Refresh();

        index = CPIndexChainL2RBinaryValue();
        CPIndexChainL2RTextBox.Text = index.ToString();
        CPIndexChainL2RTextBox.ForeColor = Numbers.GetNumberForeColor(index);
        CPIndexChainL2RTextBox.BackColor = Numbers.GetNumberBackColor(index, m_divisor, Color.MistyRose);
        ToolTip.SetToolTip(this.CPIndexChainL2RTextBox, CPIndexChainL2RDashString() + "\r\n" + "P=1 C=0:\t" + CPIndexChainL2RBinaryString() + " = " + CPIndexChainL2RBinaryValue() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(index));
        CPIndexChainL2RTextBox.Refresh();

        index = CPIndexChainR2LBinaryValue();
        CPIndexChainR2LTextBox.Text = index.ToString();
        CPIndexChainR2LTextBox.ForeColor = Numbers.GetNumberForeColor(index);
        CPIndexChainR2LTextBox.BackColor = Numbers.GetNumberBackColor(index, m_divisor, Color.MistyRose);
        ToolTip.SetToolTip(this.CPIndexChainR2LTextBox, CPIndexChainR2LDashString() + "\r\n" + "P=1 C=0:\t" + CPIndexChainR2LBinaryString() + " = " + CPIndexChainR2LBinaryValue() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(index));
        CPIndexChainR2LTextBox.Refresh();

        int sum = IndexChainSum();
        IndexChainSumTextBox.Text = sum.ToString();
        IndexChainSumTextBox.ForeColor = Numbers.GetNumberForeColor(sum);
        IndexChainSumTextBox.BackColor = Numbers.GetNumberBackColor(sum, m_divisor, Color.Pink);
        ToolTip.SetToolTip(this.IndexChainSumTextBox, "Prime-composite index chain sum" + " = " + IndexChainSumString() + " = " + IndexChainSum() + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(sum));
        IndexChainSumTextBox.Refresh();

        int length = IndexChainLength();
        IndexChainLengthTextBox.Text = length.ToString();
        IndexChainLengthTextBox.ForeColor = Numbers.GetNumberForeColor(length);
        IndexChainLengthTextBox.BackColor = Numbers.GetNumberBackColor(length, m_divisor, Color.Pink);
        ToolTip.SetToolTip(this.IndexChainLengthTextBox, "Prime-composite index chain length" + "\r\n" + "Divisors" + " " + Numbers.GetNumberToolTipText(length));
        IndexChainLengthTextBox.Refresh();
    }
    private void SaveNumberIndexChain(string path, long value, int chain_length, string text)
    {
        if (value < 0L) value *= -1L;

        if (Directory.Exists(Globals.NUMBERS_FOLDER))
        {
            path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + path;
            try
            {
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                {
                    writer.WriteLine("-----------------------------------------------------");
                    writer.WriteLine(value.ToString() + " with IndexChainLength = " + chain_length.ToString());
                    writer.WriteLine("-----------------------------------------------------");
                    writer.WriteLine("Number\tTotal\tPC_L2R\tPC_R2L\tCP_L2R\tCP_R2L\tChain");
                    writer.WriteLine("-----------------------------------------------------");
                    writer.Write(text);
                    writer.WriteLine("-----------------------------------------------------");
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }

            // show file content after save
            FileHelper.DisplayFile(path);
        }
    }
    private void SaveIndexChainLength(string path, NumberType number_type, int chain_length, string text)
    {
        if (Directory.Exists(Globals.NUMBERS_FOLDER))
        {
            path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + path;
            try
            {
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                {
                    writer.WriteLine("-----------------------------------------------------");
                    writer.WriteLine(number_type.ToString() + " numbers with IndexChainLength = " + chain_length.ToString());
                    writer.WriteLine("-----------------------------------------------------");
                    writer.WriteLine("Number\tTotal\tPC_L2R\tPC_R2L\tCP_L2R\tCP_R2L\tChain");
                    writer.WriteLine("-----------------------------------------------------");
                    writer.Write(text);
                    writer.WriteLine("-----------------------------------------------------");
                }
            }
            catch
            {
                // silence IO error in case running from read-only media (CD/DVD)
            }

            // show file content after save
            FileHelper.DisplayFile(path);
        }
    }

    private Factorizer m_worker = null;
    private Thread m_worker_thread = null;
    private enum TimeDisplayMode { Elapsed, Remaining }
    private TimeDisplayMode m_time_display_mode = TimeDisplayMode.Elapsed;
    private void ElapsedTimeLabel_Click(object sender, EventArgs e)
    {
        if (m_time_display_mode == TimeDisplayMode.Elapsed)
        {
            m_time_display_mode = TimeDisplayMode.Remaining;
        }
        else
        {
            m_time_display_mode = TimeDisplayMode.Elapsed;
        }
        ElapsedTimeLabel.Text = m_time_display_mode.ToString();
    }

    private int m_old_progress = -1;
    public delegate void UpdateProgressBar(int progress);
    public void UpdateProgressBarMethod(int progress)
    {
        if (progress > m_old_progress)
        {
            m_old_progress = progress;
            if (m_worker != null)
            {
                if (m_worker.Cancel) // interrupted by user
                {
                    AfterCancelled();
                }
                else
                {
                    if ((progress >= 0) && (progress < 100))
                    {
                        ProgressBar.Value = progress;
                        ProgressBar.Refresh();
                        ProgressValueLabel.Text = ProgressBar.Value + "%";
                        ProgressValueLabel.Refresh();

                        UpdateTimer(progress);
                    }

                    if (progress == 100) // finished naturally 
                    {
                        AfterProcessing();
                    }
                }
            }
        }
        else // no new progress
        {
            if (m_time_display_mode == TimeDisplayMode.Elapsed)
            {
                if (m_worker != null)
                {
                    UpdateTimer(progress);
                }
            }
            else //if (m_time_display_mode == TimeDisplayMode.Remaining)
            {
                // don't update remaining as it goes up wrongly with time passage and no progress
                // if progree was double not int then ok but now
                // it keeps going up up up till it there is progress so t goes down to the real remaining time and then
                // it keeps going up up up again and so on, like a seesaw
            }
        }
    }
    private void UpdateTimer(int progress)
    {
        TimeSpan timespan = m_worker.Duration;
        if (m_time_display_mode == TimeDisplayMode.Remaining)
        {
            if (progress > 0)
            {
                long ticks = (long)((double)timespan.Ticks * ((double)(100 - progress) / (double)progress));
                timespan = new TimeSpan(ticks);
            }
            else
            {
                timespan = new TimeSpan(0, 0, 0);
            }
        }
        ElapsedTimeValueLabel.Text = String.Format("{0:00}:{1:00}:{2:00}", timespan.Hours, timespan.Minutes, timespan.Seconds);
        ElapsedTimeValueLabel.Refresh();
        ElapsedTimeMillisecondsValueLabel.Text = String.Format(".{0:000}", timespan.Milliseconds);
        ElapsedTimeMillisecondsValueLabel.Refresh();
    }
    public delegate void UpdateOutputTextBox(string output);
    public void UpdateOutputTextBoxMethod(string output)
    {
        if (m_worker != null)
        {
            if (!m_worker.Cancel)
            {
                if (m_worker.IsPrime == false)
                {
                    ProgressLabel.Text = "Factoring ...";
                    ProgressLabel.Refresh();
                }

                // FIX: prevent race condition
                Thread.Sleep(10);

                OutputTextBox.Text = output;
                OutputTextBox.Refresh();
                OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
                OutputTextBox.ScrollToCaret();

                int pos = output.LastIndexOf("\r\n");
                if (pos > -1)
                {
                    string result = output.Substring(pos);
                    if ((result.Length > 0) && (result != "\r\n"))
                    {
                        pos = result.IndexOf("=");
                        if (pos > -1)
                        {
                            PrimeFactorsTextBox.Text = result.Substring(pos + 2).Replace("*", "×");
                            PrimeFactorsTextBox.Refresh();

                            string[] parts = PrimeFactorsTextBox.Text.Split('×');
                            long factors_sum = 0L;
                            foreach (string part in parts)
                            {
                                long factor;
                                if (long.TryParse(part, out factor))
                                {
                                    factors_sum += factor;
                                }
                                else
                                {
                                    factors_sum = 0L;
                                    break;
                                }
                            }
                            PrimeNumbersSumTextBox.Text = (factors_sum > 0L) ? factors_sum.ToString() : "";
                            PrimeNumbersSumTextBox.ForeColor = Numbers.GetNumberForeColor(factors_sum);
                            ToolTip.SetToolTip(this.PrimeNumbersSumTextBox, "Sum of prime factors");
                            PrimeNumbersSumTextBox.Refresh();

                            m_number_dimension = 1;
                            foreach (char c in result)
                            {
                                if (c == '*')
                                    m_number_dimension++;
                            }

                            long value = 0L;
                            if (long.TryParse(ValueTextBox.Text, out value))
                            {
                                if (value < -1)
                                    m_number_dimension--;
                                else if (value < 2)
                                    m_number_dimension = 0;

                                int nth_number_dimension_a_index = Numbers.NumberDimensionIndexOf(m_number_dimension, value) + 1;
                                int nth_number_dimension_u_index = Numbers.UniqueNumberDimensionIndexOf(m_number_dimension, value) + 1;
                                int nth_number_dimension_d_index = Numbers.DuplicateNumberDimensionIndexOf(m_number_dimension, value) + 1;
                                NumberDimensionTextBox.Text = m_number_dimension.ToString() + "D";
                                NthANumberDimensionTextBox.Text = (nth_number_dimension_a_index > 0) ? nth_number_dimension_a_index.ToString() : "";
                                NthUNumberDimensionTextBox.Text = (nth_number_dimension_u_index > 0) ? nth_number_dimension_u_index.ToString() : "";
                                NthDNumberDimensionTextBox.Text = (nth_number_dimension_d_index > 0) ? nth_number_dimension_d_index.ToString() : "";
                                //NumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(m_number_dimension);
                                NthANumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(nth_number_dimension_a_index);
                                NthUNumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(nth_number_dimension_u_index);
                                NthDNumberDimensionTextBox.ForeColor = Numbers.GetNumberForeColor(nth_number_dimension_d_index);
                                NumberDimensionTextBox.Refresh();
                                NthANumberDimensionTextBox.Refresh();
                                NthUNumberDimensionTextBox.Refresh();
                                NthDNumberDimensionTextBox.Refresh();
                            }
                        }
                    }
                }

                if (!output.Contains(Environment.NewLine))
                {
                    if (output.Length == 0)
                        return;
                    //while (output.StartsWith("-"))
                    //    output = output.Substring(1);
                    while (output.StartsWith("0"))
                        output = output.Substring(1);
                    if (output.Length == 0)
                        return; // check again, yes!

                    if (Char.IsLetter(output[0]))
                        return;
                    if (output == "1")
                        return;

                    BeforeProcessing();

                    long value = -1L;
                    if (long.TryParse(output, out value))
                    {
                        ValueTextBox.Text = value.ToString();
                    }
                }
            }
        }
    }

    private void EnableEntryControls()
    {
        MultithreadingCheckBox.Enabled = true;

        //ValueTextBox.Enabled = true;
        ValueTextBox.Focus();
        ValueTextBox.Refresh();
    }
    private void DisableEntryControls()
    {
        MultithreadingCheckBox.Enabled = false;

        //ValueTextBox.Enabled = false;
        ValueTextBox.Focus();
        ValueTextBox.Refresh();
    }
    private void ClearNumberFields()
    {
        PrimeFactorsTextBox.Text = "";
        PrimeNumbersSumTextBox.Text = "";
        DigitSumTextBox.Text = "";
        DigitalRootTextBox.Text = "";
        PrimeFactorsTextBox.BackColor = SystemColors.ControlLight;
        PrimeNumbersSumTextBox.BackColor = SystemColors.Window;
        DigitSumTextBox.BackColor = SystemColors.ControlLight;
        DigitalRootTextBox.BackColor = SystemColors.ControlLight;
        PrimeFactorsTextBox.Refresh();
        PrimeNumbersSumTextBox.Refresh();
        DigitSumTextBox.Refresh();
        DigitalRootTextBox.Refresh();

        NumberDimensionTextBox.Text = "";
        NthANumberDimensionTextBox.Text = "";
        NthUNumberDimensionTextBox.Text = "";
        NthDNumberDimensionTextBox.Text = "";
        NthSimilarPowersTextBox.Text = "";
        NumberDimensionTextBox.BackColor = SystemColors.ControlLight;
        NthANumberDimensionTextBox.BackColor = SystemColors.ControlLight;
        NthUNumberDimensionTextBox.BackColor = SystemColors.ControlLight;
        NthDNumberDimensionTextBox.BackColor = SystemColors.ControlLight;
        NthSimilarPowersTextBox.BackColor = SystemColors.ControlLight;
        NumberDimensionTextBox.Refresh();
        NthANumberDimensionTextBox.Refresh();
        NthUNumberDimensionTextBox.Refresh();
        NthDNumberDimensionTextBox.Refresh();
        NthSimilarPowersTextBox.Refresh();

        NthNumberTextBox.Text = "";
        NthAdditiveNumberTextBox.Text = "";
        NthNonAdditiveNumberTextBox.Text = "";
        NthNumberTextBox.BackColor = Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(255)))));
        NthAdditiveNumberTextBox.BackColor = Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(255)))));
        NthNonAdditiveNumberTextBox.BackColor = Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(255)))));
        NthNumberTextBox.Refresh();
        NthAdditiveNumberTextBox.Refresh();
        NthNonAdditiveNumberTextBox.Refresh();

        SquareSumTextBox.Text = "";
        Nth4n1NumberTextBox.Text = "";
        SquareSumTextBox.BackColor = Color.Lavender;
        Nth4n1NumberTextBox.BackColor = Color.Lavender;
        SquareSumTextBox.Refresh();
        Nth4n1NumberTextBox.Refresh();

        SumOfNumbersTextBox.Text = "";
        SumOfDigitSumsTextBox.Text = "";
        SumOfDigitalRootsTextBox.Text = "";
        SumOfNumbersTextBox.BackColor = SystemColors.ControlLight;
        SumOfDigitSumsTextBox.BackColor = SystemColors.ControlLight;
        SumOfDigitalRootsTextBox.BackColor = SystemColors.ControlLight;
        SumOfNumbersTextBox.Refresh();
        SumOfDigitSumsTextBox.Refresh();
        SumOfDigitalRootsTextBox.Refresh();

        NumberKindIndexTextBox.Text = "";
        SumOfProperDivisorsTextBox.Text = "";
        SumOfDivisorsTextBox.Text = "";
        SumOfDivisorDigitSumsTextBox.Text = "";
        SumOfDivisorDigitalRootsTextBox.Text = "";
        NumberKindIndexTextBox.BackColor = SystemColors.Control;
        SumOfProperDivisorsTextBox.BackColor = SystemColors.Control;
        SumOfDivisorsTextBox.BackColor = SystemColors.Control;
        SumOfDivisorDigitSumsTextBox.BackColor = SystemColors.Control;
        SumOfDivisorDigitalRootsTextBox.BackColor = SystemColors.Control;
        NumberKindIndexTextBox.Refresh();
        SumOfProperDivisorsTextBox.Refresh();
        SumOfDivisorsTextBox.Refresh();
        SumOfDivisorDigitSumsTextBox.Refresh();
        SumOfDivisorDigitalRootsTextBox.Refresh();

        PCIndexChainL2RTextBox.Text = "";
        PCIndexChainR2LTextBox.Text = "";
        CPIndexChainL2RTextBox.Text = "";
        CPIndexChainR2LTextBox.Text = "";
        IndexChainSumTextBox.Text = "";
        IndexChainLengthTextBox.Text = "";
        CPIndexChainL2RTextBox.BackColor = Color.MistyRose;
        PCIndexChainR2LTextBox.BackColor = Color.MistyRose;
        PCIndexChainL2RTextBox.BackColor = Color.MistyRose;
        CPIndexChainR2LTextBox.BackColor = Color.MistyRose;
        IndexChainSumTextBox.BackColor = Color.Pink;
        IndexChainLengthTextBox.BackColor = Color.Pink;
        PCIndexChainL2RTextBox.Refresh();
        PCIndexChainR2LTextBox.Refresh();
        CPIndexChainL2RTextBox.Refresh();
        CPIndexChainR2LTextBox.Refresh();
        IndexChainSumTextBox.Refresh();
        IndexChainLengthTextBox.Refresh();

        IndexTextBox.TextChanged -= this.IndexTextBox_TextChanged;
        IndexTextBox.Text = "";
        IndexTextBox.TextChanged += this.IndexTextBox_TextChanged;
        IndexTextBox.BackColor = SystemColors.Window;
        IndexTextBox.Refresh();

        PTextBox.Text = "";
        APTextBox.Text = "";
        XPTextBox.Text = "";
        CTextBox.Text = "";
        ACTextBox.Text = "";
        XCTextBox.Text = "";
        DFTextBox.Text = "";
        ABTextBox.Text = "";
        PTextBox.Refresh();
        APTextBox.Refresh();
        XPTextBox.Refresh();
        CTextBox.Refresh();
        ACTextBox.Refresh();
        XCTextBox.Refresh();
        DFTextBox.Refresh();
        ABTextBox.Refresh();

        OutputTextBox.Text = "\r\n\r\n\r\n" +
                             "Large number" + "\r\n" +
                             "factorization progress" + "\r\n" +
                             "will be displayed here";
        OutputTextBox.Refresh();
    }
    private void ClearProgress()
    {
        ElapsedTimeValueLabel.Text = "00:00:00";
        ElapsedTimeValueLabel.Refresh();
        ElapsedTimeMillisecondsValueLabel.Text = ".000";
        ElapsedTimeMillisecondsValueLabel.Refresh();
        ProgressLabel.Text = "Working...";
        ProgressLabel.Refresh();
        ProgressBar.Value = 0;
        ProgressBar.Refresh();
        ProgressValueLabel.Text = ProgressBar.Value + "%";
        ProgressValueLabel.Refresh();
    }
    private void BeforeProcessing()
    {
        DisableEntryControls();
        ClearProgress();
    }
    private bool m_multithreading = true;
    private void MultithreadingCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_multithreading = MultithreadingCheckBox.Checked;
    }
    private void CallRun()
    {
        ValueTextBox.Text = ValueTextBox.Text.Replace(" ", "");

        if (ValueTextBox.Text.Length >= 19)
        {
            TabControl.SelectedIndex = 0;   // Factorization
            Run();
        }
        else
        {
            TabControl.SelectedIndex = 1;   // Index
            EvaluateExpression();
        }
    }
    private void Run()
    {
        // guard against multiple runs on multiple ENTER key presses
        if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
            return;

        this.Cursor = Cursors.WaitCursor;
        try
        {
            string arguments = ValueTextBox.Text;
            if (arguments.Length == 0) return;

            BeforeProcessing();
            try
            {
                if (Numbers.IsDigitsOnly(arguments))
                {
                    m_worker = new Factorizer(this, "factor", arguments, m_multithreading);
                }
                else
                {
                    m_worker = new Factorizer(this, null, arguments, false);
                }

                if (m_worker != null)
                {
                    m_worker_thread = new Thread(new ThreadStart(m_worker.Run));
                    m_worker_thread.Priority = ThreadPriority.Highest;
                    m_worker_thread.IsBackground = false;
                    m_worker_thread.Start();
                }
            }
            catch
            {
                Cancel();
            }
        }
        finally
        {
            //this.Cursor = Cursors.Default;
            // will be done by AfterCancelled OR AfterProcessing
        }
    }
    public void Cancel()
    {
        if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
        {
            if (m_worker != null)
            {
                m_worker.Cancel = true;

                if (m_worker_thread != null)
                {
                    m_worker_thread.Join();
                    m_worker_thread = null;
                }
            }
        }
    }
    private void AfterCancelled()
    {
        this.Cursor = Cursors.Default;

        m_old_progress = -1;
        ProgressLabel.Text = "Cancelled";
        ProgressLabel.Refresh();

        EnableEntryControls();
    }
    private void AfterProcessing()
    {
        Cursor.Current = Cursors.Default;

        string item = ValueTextBox.Text;
        if (!m_history_items.Contains(item))
        {
            m_history_items.Insert(m_history_index + 1, item);
            m_history_index++;
        }
        PreviousHistoryLabel.Enabled = (m_history_index != 0);
        NextHistoryLabel.Enabled = (m_history_index != (m_history_items.Count - 1));

        if (m_worker != null)
        {
            m_old_progress = -1;
            ProgressLabel.Text = "Finished";
            ProgressLabel.Refresh();

            EnableEntryControls();
        }
    }
    private double m_double_value = 0.0D;
    private void EvaluateExpression()
    {
        BigInteger big_integer = 0;
        long value = 0L;
        m_double_value = value;

        string input = ValueTextBox.Text;

        if (long.TryParse(input, out value))
        {
            m_double_value = value;
            FactorizeValue(value);
        }
        else if ((m_radix == Numbers.DEFAULT_RADIX) && (input.ContainsInside("P")))
        {
            string[] parts = input.ToUpper().Split('P'); // Permutations
            if (parts.Length == 2)
            {
                int n = 0;
                if (int.TryParse(parts[0], out n))
                {
                    int k = 0;
                    if (int.TryParse(parts[1], out k))
                    {
                        big_integer = Numbers.nPk(n, k);
                        FactorizeValue((long)big_integer);
                    }
                }
            }
        }
        else if ((m_radix == Numbers.DEFAULT_RADIX) && (input.ContainsInside("C"))) // Combinations
        {
            string[] parts = input.ToUpper().Split('C');
            if (parts.Length == 2)
            {
                int n = 0;
                if (int.TryParse(parts[0], out n))
                {
                    int k = 0;
                    if (int.TryParse(parts[1], out k))
                    {
                        big_integer = Numbers.nCk(n, k);
                        FactorizeValue((long)big_integer);
                    }
                }
            }
        }
        else // expression or big number
        {
            m_double_value = DoCalculateExpression(input, Numbers.DEFAULT_RADIX);
            if (Math.Abs(m_double_value) < Numbers.ERROR_MARGIN) m_double_value = 0.0D;

            value = (long)Math.Round(m_double_value + Numbers.ERROR_MARGIN);

            ValueTextBox.TextChanged -= ValueTextBox_TextChanged;
            ValueTextBox.Text = value.ToString();
            ValueTextBox.TextChanged += ValueTextBox_TextChanged;

            FactorizeValue(value);
        }

        // result has a big decimal fraction
        if (Math.Abs((m_double_value - value)) >= Numbers.ERROR_MARGIN)
        {
            PrimeFactorsTextBox.Text = m_double_value.ToString();
            PrimeFactorsTextBox.Refresh();
        }
    }
    private double DoCalculateExpression(string expression, int radix)
    {
        string output = RadixEvaluator.Evaluate(expression, radix);

        double result;
        if (double.TryParse(output, out result))
        {
            return result;
        }

        return 0.0D;
    }

    private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.AcceptButton = null;

        if (TabControl.SelectedIndex == 0)      // Factorization
        {
            ValueTextBox.Focus();
            ValueTextBox.SelectionStart = ValueTextBox.Text.Length;
            ValueTextBox.SelectionLength = 0;
        }
        else if (TabControl.SelectedIndex == 1) // Index
        {
            ValueTextBox.Focus();
            ValueTextBox.SelectionStart = ValueTextBox.Text.Length;
            ValueTextBox.SelectionLength = 0;
        }
        else if (TabControl.SelectedIndex == 2) // Triangle
        {
            this.AcceptButton = CalculateTriangleButton;

            aTriangleTextBox.Focus();
            aTriangleTextBox.SelectionStart = aTriangleTextBox.Text.Length;
            aTriangleTextBox.SelectionLength = 0;
        }
        else if (TabControl.SelectedIndex == 3) // Circle
        {
            rCircleTextBox.Focus();
            rCircleTextBox.SelectionStart = rCircleTextBox.Text.Length;
            rCircleTextBox.SelectionLength = 0;
        }
        else if (TabControl.SelectedIndex == 4) // Sphere
        {
            rSphereTextBox.Focus();
            rSphereTextBox.SelectionStart = rSphereTextBox.Text.Length;
            rSphereTextBox.SelectionLength = 0;
        }
    }

    private void IndexTextBox_TextChanged(object sender, EventArgs e)
    {
        string input = IndexTextBox.Text.Replace(" ", "");
        if (!String.IsNullOrEmpty(input))
        {
            while (input.StartsWith("0"))
            {
                input = input.Remove(0, 1);

                IndexTextBox.TextChanged -= this.IndexTextBox_TextChanged;
                IndexTextBox.Text = input;
                IndexTextBox.TextChanged += this.IndexTextBox_TextChanged;
                IndexTextBox.Refresh();
            }

            if (input.Length > 0)
            {
                if (input.EndsWith("11"))
                {
                    NthLabel.Text = "th";
                }
                else if (input.EndsWith("12"))
                {
                    NthLabel.Text = "th";
                }
                else if (input.EndsWith("13"))
                {
                    NthLabel.Text = "th";
                }
                else if (input.EndsWith("1"))
                {
                    NthLabel.Text = "st";
                }
                else if (input.EndsWith("2"))
                {
                    NthLabel.Text = "nd";
                }
                else if (input.EndsWith("3"))
                {
                    NthLabel.Text = "rd";
                }
                else
                {
                    NthLabel.Text = "th";
                }

                int index;
                if (int.TryParse(input, out index))
                {
                    ValueTextBox.Text = index.ToString();
                    ValueTextBox.ForeColor = Numbers.GetNumberForeColor(index);
                    ValueTextBox.BackColor = Numbers.GetNumberBackColor(index, m_divisor, SystemColors.Window);
                    ToolTip.SetToolTip(this.ValueTextBox, "Divisors" + " " + Numbers.GetNumberToolTipText(index));
                    ValueTextBox.Refresh();

                    if (index < 0) index *= -1;
                    index--;

                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        long p = Numbers.Primes[index];
                        long ap = Numbers.AdditivePrimes[index];
                        long xp = Numbers.NonAdditivePrimes[index];
                        long c = Numbers.Composites[index];
                        long ac = Numbers.AdditiveComposites[index];
                        long xc = Numbers.NonAdditiveComposites[index];

                        PTextBox.Text = p.ToString();
                        APTextBox.Text = ap.ToString();
                        XPTextBox.Text = xp.ToString();
                        CTextBox.Text = c.ToString();
                        ACTextBox.Text = ac.ToString();
                        XCTextBox.Text = xc.ToString();

                        PTextBox.ForeColor = Numbers.GetNumberForeColor(p);
                        APTextBox.ForeColor = Numbers.GetNumberForeColor(ap);
                        XPTextBox.ForeColor = Numbers.GetNumberForeColor(xp);
                        CTextBox.ForeColor = Numbers.GetNumberForeColor(c);
                        ACTextBox.ForeColor = Numbers.GetNumberForeColor(ac);
                        XCTextBox.ForeColor = Numbers.GetNumberForeColor(xc);
                    }
                    catch
                    {
                        PTextBox.Text = "";
                        APTextBox.Text = "";
                        XPTextBox.Text = "";
                        CTextBox.Text = "";
                        ACTextBox.Text = "";
                        XCTextBox.Text = "";
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }

                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        long df = Numbers.DeficientNumbers[index];
                        long ab = Numbers.AbundantNumbers[index];

                        DFTextBox.Text = df.ToString();
                        ABTextBox.Text = ab.ToString();

                        DFTextBox.ForeColor = Numbers.GetNumberForeColor(df);
                        ABTextBox.ForeColor = Numbers.GetNumberForeColor(ab);
                    }
                    catch
                    {
                        DFTextBox.Text = "";
                        ABTextBox.Text = "";
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    PTextBox.Text = "";
                    APTextBox.Text = "";
                    XPTextBox.Text = "";
                    CTextBox.Text = "";
                    ACTextBox.Text = "";
                    XCTextBox.Text = "";
                    DFTextBox.Text = "";
                    ABTextBox.Text = "";
                }
            }
            else // empty
            {
                ValueTextBox.Text = "";
                ValueTextBox.ForeColor = SystemColors.WindowText;
                ValueTextBox.BackColor = SystemColors.Window;
                ToolTip.SetToolTip(this.ValueTextBox, null);
                ValueTextBox.Refresh();
            }
        }
    }
    private void IndexTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        Control control = (sender as TextBoxBase);
        if (control != null)
        {
            if (e.KeyCode == Keys.Up)
            {
                IncrementValue(control);
            }
            else if (e.KeyCode == Keys.Down)
            {
                DecrementValue(control);
            }
            else if (e.KeyCode == Keys.Enter)
            {
                FactorizeValue(control);
            }
        }
    }
    private void IncrementIndex()
    {
        if (IndexTextBox.Text == "")
        {
            IndexTextBox.Text = "1";
        }
        else
        {
            int index;
            if (int.TryParse(IndexTextBox.Text, out index))
            {
                if (index < int.MaxValue)
                    index++;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void DecrementIndex()
    {
        if (IndexTextBox.Text == "")
        {
            IndexTextBox.Text = "1";
        }
        else
        {
            int index;
            if (int.TryParse(IndexTextBox.Text, out index))
            {
                if (index > 1)
                    index--;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void PTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.PrimeIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void APTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.AdditivePrimeIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void XPTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.NonAdditivePrimeIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void CTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.CompositeIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void ACTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.AdditiveCompositeIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void XCTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.NonAdditiveCompositeIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void DFTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.DeficientNumberIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }
    private void ABTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex();
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex();
        }
        else if (e.KeyCode == Keys.Enter)
        {
            long value = 0L;
            if (long.TryParse((sender as TextBox).Text, out value))
            {
                int index = Numbers.AbundantNumberIndexOf(value) + 1;
                IndexTextBox.Text = index.ToString();
            }
        }
    }

    private void CircleTextBox_TextChanged(object sender, EventArgs e)
    {
        CircleCalculations(sender);
    }
    private void CircleTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementCircleParameter(sender);
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementCircleParameter(sender);
        }
    }
    private void IncrementCircleParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value < double.MaxValue)
                value++;
            (sender as TextBox).Text = value.ToString("0.0");
        }
    }
    private void DecrementCircleParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value > 0.0D)
                value--;
            (sender as TextBox).Text = value.ToString("0.0");
        }
    }
    private void CircleCalculations(object sender)
    {
        double r = 0.0D;
        double d = 0.0D;
        double c = 0.0D;
        double a = 0.0D;

        try
        {
            rCircleTextBox.TextChanged -= CircleTextBox_TextChanged;
            dCircleTextBox.TextChanged -= CircleTextBox_TextChanged;
            cCircleTextBox.TextChanged -= CircleTextBox_TextChanged;
            aCircleTextBox.TextChanged -= CircleTextBox_TextChanged;

            if (sender == rCircleTextBox)
            {
                r = double.Parse(rCircleTextBox.Text);
                d = 2.0D * r;
                c = 2.0D * Math.PI * r;
                a = Math.PI * r * r;
                dCircleTextBox.Text = d.ToString("0.0");
                cCircleTextBox.Text = c.ToString("0.0");
                aCircleTextBox.Text = a.ToString("0.0");
            }
            else if (sender == dCircleTextBox)
            {
                d = double.Parse(dCircleTextBox.Text);
                r = 0.5D * d;
                c = 2.0D * Math.PI * r;
                a = Math.PI * r * r;
                rCircleTextBox.Text = r.ToString("0.0");
                cCircleTextBox.Text = c.ToString("0.0");
                aCircleTextBox.Text = a.ToString("0.0");
            }
            else if (sender == cCircleTextBox)
            {
                c = double.Parse(cCircleTextBox.Text);
                r = c / (2.0D * Math.PI);
                d = 2.0D * r;
                a = Math.PI * r * r;
                rCircleTextBox.Text = r.ToString("0.0");
                dCircleTextBox.Text = d.ToString("0.0");
                aCircleTextBox.Text = a.ToString("0.0");
            }
            else if (sender == aCircleTextBox)
            {
                a = double.Parse(aCircleTextBox.Text);
                r = Math.Sqrt(a / Math.PI);
                d = 2.0D * r;
                c = 2.0D * Math.PI * r;
                rCircleTextBox.Text = r.ToString("0.0");
                dCircleTextBox.Text = d.ToString("0.0");
                cCircleTextBox.Text = c.ToString("0.0");
            }
        }
        catch
        {
            // skip error
        }
        finally
        {
            rCircleTextBox.TextChanged += CircleTextBox_TextChanged;
            dCircleTextBox.TextChanged += CircleTextBox_TextChanged;
            cCircleTextBox.TextChanged += CircleTextBox_TextChanged;
            aCircleTextBox.TextChanged += CircleTextBox_TextChanged;
        }
    }

    private void PiDigitsLabel_Click(object sender, EventArgs e)
    {
        Clipboard.SetData(DataFormats.Text, piCircleTextBox.Text);
    }

    private void SphereTextBox_TextChanged(object sender, EventArgs e)
    {
        SphereCalculations(sender);
    }
    private void SphereTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementSphereParameter(sender);
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementSphereParameter(sender);
        }
    }
    private void IncrementSphereParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value < double.MaxValue)
                value++;
            (sender as TextBox).Text = value.ToString("0.0");
        }
    }
    private void DecrementSphereParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value > 0.0D)
                value--;
            (sender as TextBox).Text = value.ToString("0.0");
        }
    }
    private void SphereCalculations(object sender)
    {
        double r = 0.0D;
        double d = 0.0D;
        double s = 0.0D;
        double v = 0.0D;

        try
        {
            rSphereTextBox.TextChanged -= SphereTextBox_TextChanged;
            dSphereTextBox.TextChanged -= SphereTextBox_TextChanged;
            sSphereTextBox.TextChanged -= SphereTextBox_TextChanged;
            vSphereTextBox.TextChanged -= SphereTextBox_TextChanged;

            if (sender == rSphereTextBox)
            {
                r = double.Parse(rSphereTextBox.Text);
                d = 2.0D * r;
                s = 4.0D * Math.PI * r * r;
                v = (4.0D / 3.0D) * Math.PI * r * r * r;
                dSphereTextBox.Text = d.ToString("0.0");
                sSphereTextBox.Text = s.ToString("0.0");
                vSphereTextBox.Text = v.ToString("0.0");
            }
            else if (sender == dSphereTextBox)
            {
                d = double.Parse(dSphereTextBox.Text);
                r = 0.5D * d;
                s = 4.0D * Math.PI * r * r;
                v = (4.0D / 3.0D) * Math.PI * r * r * r;
                rSphereTextBox.Text = r.ToString("0.0");
                sSphereTextBox.Text = s.ToString("0.0");
                vSphereTextBox.Text = v.ToString("0.0");
            }
            else if (sender == sSphereTextBox)
            {
                s = double.Parse(sSphereTextBox.Text);
                r = Math.Sqrt(s / (4.0D * Math.PI));
                d = 2.0D * r;
                v = (4.0D / 3.0D) * Math.PI * r * r * r;
                rSphereTextBox.Text = r.ToString("0.0");
                dSphereTextBox.Text = d.ToString("0.0");
                vSphereTextBox.Text = v.ToString("0.0");
            }
            else if (sender == vSphereTextBox)
            {
                v = double.Parse(vSphereTextBox.Text);
                r = Math.Pow((v / ((4.0D / 3.0D) * Math.PI)), 1.0D / 3.0D);
                d = 2.0D * r;
                s = 4.0D * Math.PI * r * r;
                rSphereTextBox.Text = r.ToString("0.0");
                dSphereTextBox.Text = d.ToString("0.0");
                sSphereTextBox.Text = s.ToString("0.0");
            }
        }
        catch
        {
            // skip error
        }
        finally
        {
            rSphereTextBox.TextChanged += SphereTextBox_TextChanged;
            dSphereTextBox.TextChanged += SphereTextBox_TextChanged;
            sSphereTextBox.TextChanged += SphereTextBox_TextChanged;
            vSphereTextBox.TextChanged += SphereTextBox_TextChanged;
        }
    }

    private void CalculateTriangleButton_Click(object sender, EventArgs e)
    {
        TriangleCalculations(sender);
    }
    private void ClearTriangleButton_Click(object sender, EventArgs e)
    {
        //aTriangleTextBox.Text = "";
        //bTriangleTextBox.Text = "";
        //cTriangleTextBox.Text = "";

        alphaTriangleTextBox.Text = "";
        betaTriangleTextBox.Text = "";
        gammaTriangleTextBox.Text = "";

        pTriangleTextBox.Text = "";
        tTriangleTextBox.Text = "";
        h1TriangleTextBox.Text = "";
        h2TriangleTextBox.Text = "";
        h3TriangleTextBox.Text = "";

        aTriangleTextBox.Focus();
    }
    private void TriangleTextBox_TextChanged(object sender, EventArgs e)
    {
        //TriangleCalculations(sender);
    }
    private void TriangleTextBox_KeyDown(object sender, KeyEventArgs e)
    {

    }
    private void IncrementTriangleParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value < double.MaxValue)
                value++;
            value = Math.Floor(value);
            (sender as TextBox).Text = value.ToString("0");
        }
        TriangleCalculations(sender);
    }
    private void DecrementTriangleParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value > 0.0D)
                value--;
            value = Math.Floor(value);
            (sender as TextBox).Text = value.ToString("0");
        }
        TriangleCalculations(sender);
    }
    private void AngleTriangleTextBox_TextChanged(object sender, EventArgs e)
    {
        //TriangleCalculations(sender);
    }
    private void AngleTriangleTextBox_KeyDown(object sender, KeyEventArgs e)
    {

    }
    private void AngleTriangleTextBox_Leave(object sender, EventArgs e)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value > 180.0D)
            {
                value = 180.0D;
            }
            else if (value < 0.0D)
            {
                value = 0.0D;
            }
        }
        (sender as TextBox).Text = value.ToString("0");
    }
    private void IncrementAngleTriangleParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            double alpha = 0.0D;
            double beta = 0.0D;
            double gamma = 0.0D;
            if (alphaTriangleTextBox.Text.Length > 0)
                alpha = double.Parse(alphaTriangleTextBox.Text) / (180.0D / Math.PI);
            if (betaTriangleTextBox.Text.Length > 0)
                beta = double.Parse(betaTriangleTextBox.Text) / (180.0D / Math.PI);
            if (gammaTriangleTextBox.Text.Length > 0)
                gamma = double.Parse(gammaTriangleTextBox.Text) / (180.0D / Math.PI);
            if ((alpha + beta + gamma) < Math.PI)
            {
                if (value > 180.0D)
                {
                    value = 180.0D;
                }
                else if (value < 0.0D)
                {
                    value = -1.0D;
                }
                if (value < 180.0D)
                    value++;
                value = Math.Floor(value);
                (sender as TextBox).Text = value.ToString("0");
            }
        }
    }
    private void DecrementAngleTriangleParameter(object sender)
    {
        double value = 0.0D;
        if (double.TryParse((sender as TextBox).Text, out value))
        {
            if (value > 180.0D)
            {
                value = 180.0D;
            }
            else if (value < 0.0D)
            {
                value = 0.0D;
            }

            if (value > 0.0D)
                value--;
            value = Math.Floor(value);
            (sender as TextBox).Text = value.ToString("0");
        }
    }
    private void TriangleCalculations(object sender)
    {
        // https://en.wikipedia.org/wiki/Solution_of_triangles

        double a = 0.0D;
        double b = 0.0D;
        double c = 0.0D;
        double alpha = 0.0D;
        double beta = 0.0D;
        double gamma = 0.0D;
        double p = 0.0D;
        double t = 0.0D;
        double h1 = 0.0D;
        double h2 = 0.0D;
        double h3 = 0.0D;

        try
        {
            aTriangleTextBox.TextChanged -= TriangleTextBox_TextChanged;
            bTriangleTextBox.TextChanged -= TriangleTextBox_TextChanged;
            cTriangleTextBox.TextChanged -= TriangleTextBox_TextChanged;
            alphaTriangleTextBox.TextChanged -= AngleTriangleTextBox_TextChanged;
            betaTriangleTextBox.TextChanged -= AngleTriangleTextBox_TextChanged;
            gammaTriangleTextBox.TextChanged -= AngleTriangleTextBox_TextChanged;

            if (aTriangleTextBox.Text.Length > 0)
                a = double.Parse(aTriangleTextBox.Text);
            if (bTriangleTextBox.Text.Length > 0)
                b = double.Parse(bTriangleTextBox.Text);
            if (cTriangleTextBox.Text.Length > 0)
                c = double.Parse(cTriangleTextBox.Text);
            if (alphaTriangleTextBox.Text.Length > 0)
                alpha = double.Parse(alphaTriangleTextBox.Text) / (180.0D / Math.PI);
            if (betaTriangleTextBox.Text.Length > 0)
                beta = double.Parse(betaTriangleTextBox.Text) / (180.0D / Math.PI);
            if (gammaTriangleTextBox.Text.Length > 0)
                gamma = double.Parse(gammaTriangleTextBox.Text) / (180.0D / Math.PI);


            // SSS
            if ((a > 0.0D) && (b > 0.0D) && (c > 0.0D))
            {
                alpha = Math.Acos(((b * b) + (c * c) - (a * a)) / (2 * b * c));
                beta = Math.Acos(((a * a) + (c * c) - (b * b)) / (2 * a * c));
                gamma = Math.Acos(((a * a) + (b * b) - (c * c)) / (2 * a * b));
            }


            // SAS
            else if ((a > 0.0D) && (b > 0.0D) && (gamma > 0.0D))
            {
                c = Math.Sqrt((a * a) + (b * b) - (2 * a * b * Math.Cos((gamma))));
                alpha = Math.Acos(((b * b) + (c * c) - (a * a)) / (2 * b * c));
                beta = Math.Acos(((a * a) + (c * c) - (b * b)) / (2 * a * c));
            }
            else if ((a > 0.0D) && (c > 0.0D) && (beta > 0.0D))
            {
                b = Math.Sqrt((a * a) + (c * c) - (2 * a * c * Math.Cos((beta))));
                alpha = Math.Acos(((b * b) + (c * c) - (a * a)) / (2 * b * c));
                gamma = Math.Acos(((a * a) + (b * b) - (c * c)) / (2 * a * b));
            }
            else if ((b > 0.0D) && (c > 0.0D) && (alpha > 0.0D))
            {
                a = Math.Sqrt((b * b) + (c * c) - (2 * b * c * Math.Cos((alpha))));
                beta = Math.Acos(((a * a) + (c * c) - (b * b)) / (2 * a * c));
                gamma = Math.Acos(((a * a) + (b * b) - (c * c)) / (2 * a * b));
            }


            // SSA
            else if ((a > 0.0D) && (b > 0.0D) && (alpha > 0.0D))
            {
                double D = b * Math.Sin(alpha) / a;
                if (D > 1)
                    beta = double.NaN;
                else if (D == 1)
                    beta = Math.PI / 2.0D;
                else
                    if (a < b)
                    beta = Math.Asin(D);
                else
                    beta = Math.PI - Math.Asin(D);
                gamma = Math.PI - alpha - beta;
                if (gamma < 0.0D)
                {
                    beta = Math.Asin(D);
                    gamma = Math.PI - alpha - beta;
                }
                c = b * (Math.Sin(gamma) / Math.Sin(beta));
            }
            else if ((a > 0.0D) && (b > 0.0D) && (beta > 0.0D))
            {
                double D = a * Math.Sin(beta) / b;
                if (D > 1)
                    alpha = double.NaN;
                else if (D == 1)
                    alpha = Math.PI / 2.0D;
                else
                    if (a < b)
                    alpha = Math.Asin(D);
                else
                    alpha = Math.PI - Math.Asin(D);
                gamma = Math.PI - alpha - beta;
                if (gamma < 0.0D)
                {
                    alpha = Math.Asin(D);
                    gamma = Math.PI - alpha - beta;
                }
                c = b * (Math.Sin(gamma) / Math.Sin(beta));
            }
            else if ((a > 0.0D) && (c > 0.0D) && (alpha > 0.0D))
            {
                double D = c * Math.Sin(alpha) / a;
                if (D > 1)
                    gamma = double.NaN;
                else if (D == 1)
                    gamma = Math.PI / 2.0D;
                else
                    if (a < b)
                    gamma = Math.Asin(D);
                else
                    gamma = Math.PI - Math.Asin(D);
                beta = Math.PI - alpha - gamma;
                if (beta < 0.0D)
                {
                    gamma = Math.Asin(D);
                    beta = Math.PI - alpha - gamma;
                }
                b = a * (Math.Sin(beta) / Math.Sin(alpha));
            }
            else if ((a > 0.0D) && (c > 0.0D) && (gamma > 0.0D))
            {
                double D = a * Math.Sin(gamma) / c;
                if (D > 1)
                    alpha = double.NaN;
                else if (D == 1)
                    alpha = Math.PI / 2.0D;
                else
                    if (a < b)
                    alpha = Math.Asin(D);
                else
                    alpha = Math.PI - Math.Asin(D);
                beta = Math.PI - alpha - gamma;
                if (beta < 0.0D)
                {
                    alpha = Math.Asin(D);
                    beta = Math.PI - alpha - gamma;
                }
                b = c * (Math.Sin(beta) / Math.Sin(gamma));
            }
            else if ((b > 0.0D) && (c > 0.0D) && (beta > 0.0D))
            {
                double D = c * Math.Sin(beta) / b;
                if (D > 1)
                    gamma = double.NaN;
                else if (D == 1)
                    gamma = Math.PI / 2.0D;
                else
                    if (a < b)
                    gamma = Math.Asin(D);
                else
                    gamma = Math.PI - Math.Asin(D);
                alpha = Math.PI - beta - gamma;
                if (alpha < 0.0D)
                {
                    gamma = Math.Asin(D);
                    alpha = Math.PI - beta - gamma;
                }
                a = b * (Math.Sin(alpha) / Math.Sin(beta));
            }
            else if ((b > 0.0D) && (c > 0.0D) && (gamma > 0.0D))
            {
                double D = b * Math.Sin(gamma) / c;
                if (D > 1)
                    beta = double.NaN;
                else if (D == 1)
                    beta = Math.PI / 2.0D;
                else
                    if (a < b)
                    beta = Math.Asin(D);
                else
                    beta = Math.PI - Math.Asin(D);
                alpha = Math.PI - beta - gamma;
                if (alpha < 0.0D)
                {
                    beta = Math.Asin(D);
                    alpha = Math.PI - beta - gamma;
                }
                a = c * (Math.Sin(alpha) / Math.Sin(gamma));
            }


            // ASA
            else if ((a > 0.0D) && (beta > 0.0D) && (gamma > 0.0D))
            {
                alpha = Math.PI - beta - gamma;
                b = a * (Math.Sin(beta) / Math.Sin(alpha));
                c = a * (Math.Sin(gamma) / Math.Sin(alpha));
            }
            else if ((b > 0.0D) && (alpha > 0.0D) && (gamma > 0.0D))
            {
                beta = Math.PI - alpha - gamma;
                a = b * (Math.Sin(alpha) / Math.Sin(beta));
                c = b * (Math.Sin(gamma) / Math.Sin(beta));
            }
            else if ((c > 0.0D) && (alpha > 0.0D) && (beta > 0.0D))
            {
                gamma = Math.PI - alpha - beta;
                a = c * (Math.Sin(alpha) / Math.Sin(gamma));
                b = c * (Math.Sin(beta) / Math.Sin(gamma));
            }


            // SAA
            else if ((a > 0.0D) && (alpha > 0.0D) && (beta > 0.0D))
            {
                gamma = Math.PI - alpha - beta;
                b = a * (Math.Sin(beta) / Math.Sin(alpha));
                c = a * (Math.Sin(gamma) / Math.Sin(alpha));
            }
            else if ((a > 0.0D) && (alpha > 0.0D) && (gamma > 0.0D))
            {
                beta = Math.PI - alpha - gamma;
                b = a * (Math.Sin(beta) / Math.Sin(alpha));
                c = a * (Math.Sin(gamma) / Math.Sin(alpha));
            }
            else if ((b > 0.0D) && (beta > 0.0D) && (alpha > 0.0D))
            {
                gamma = Math.PI - alpha - beta;
                a = b * (Math.Sin(alpha) / Math.Sin(beta));
                c = b * (Math.Sin(gamma) / Math.Sin(beta));
            }
            else if ((b > 0.0D) && (beta > 0.0D) && (gamma > 0.0D))
            {
                alpha = Math.PI - beta - gamma;
                a = b * (Math.Sin(alpha) / Math.Sin(beta));
                c = b * (Math.Sin(gamma) / Math.Sin(beta));
            }
            else if ((c > 0.0D) && (gamma > 0.0D) && (alpha > 0.0D))
            {
                beta = Math.PI - alpha - gamma;
                a = c * (Math.Sin(alpha) / Math.Sin(gamma));
                b = c * (Math.Sin(beta) / Math.Sin(gamma));
            }
            else if ((c > 0.0D) && (gamma > 0.0D) && (beta > 0.0D))
            {
                alpha = Math.PI - beta - gamma;
                a = c * (Math.Sin(alpha) / Math.Sin(gamma));
                b = c * (Math.Sin(beta) / Math.Sin(gamma));
            }


            // AAA
            else if ((alpha > 0.0D) && (beta > 0.0D) && (gamma > 0.0D))
            {
                if ((a == 0.0D) && (b == 0.0D) && (c == 0.0D))
                {
                    a = 1; // The Unit
                    b = a * (Math.Sin(beta) / Math.Sin(alpha));
                    c = b * (Math.Sin(gamma) / Math.Sin(beta));
                }
                else if (a > 0.0D)
                {
                    b = a * (Math.Sin(beta) / Math.Sin(alpha));
                    c = b * (Math.Sin(gamma) / Math.Sin(beta));
                }
                else if (b > 0.0D)
                {
                    a = b * (Math.Sin(alpha) / Math.Sin(beta));
                    c = b * (Math.Sin(gamma) / Math.Sin(beta));
                }
                else if (c > 0.0D)
                {
                    a = c * (Math.Sin(alpha) / Math.Sin(gamma));
                    b = a * (Math.Sin(beta) / Math.Sin(alpha));
                }
            }



            p = a + b + c;
            t = 0.25D * Math.Sqrt((a + b + c) * (-a + b + c) * (a - b + c) * (a + b - c));
            h1 = 2 * t / a;
            h2 = 2 * t / b;
            h3 = 2 * t / c;

            aTriangleTextBox.Text = a.ToString("0.0");
            bTriangleTextBox.Text = b.ToString("0.0");
            cTriangleTextBox.Text = c.ToString("0.0");
            alphaTriangleTextBox.Text = (alpha * (180.0D / Math.PI)).ToString("0.0");
            betaTriangleTextBox.Text = (beta * (180.0D / Math.PI)).ToString("0.0");
            gammaTriangleTextBox.Text = (gamma * (180.0D / Math.PI)).ToString("0.0");
            pTriangleTextBox.Text = p.ToString("0.0");
            tTriangleTextBox.Text = t.ToString("0.0");
            h1TriangleTextBox.Text = h1.ToString("0.0");
            h2TriangleTextBox.Text = h2.ToString("0.0");
            h3TriangleTextBox.Text = h3.ToString("0.0");
        }
        catch
        {
            // skip error
        }
        finally
        {
            aTriangleTextBox.TextChanged += TriangleTextBox_TextChanged;
            bTriangleTextBox.TextChanged += TriangleTextBox_TextChanged;
            cTriangleTextBox.TextChanged += TriangleTextBox_TextChanged;
            alphaTriangleTextBox.TextChanged += AngleTriangleTextBox_TextChanged;
            betaTriangleTextBox.TextChanged += AngleTriangleTextBox_TextChanged;
            gammaTriangleTextBox.TextChanged += AngleTriangleTextBox_TextChanged;
        }
    }

    private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if ((this.Visible == true) && (139 == 319))
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
                if (this.WindowState == FormWindowState.Minimized)
                {
                    if (m_was_maximized)
                    {
                        this.WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Normal;
                    }
                }
                this.Activate();    // bring to foreground
                this.BringToFront();
            }
        }
    }
    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show
        (
            Application.ProductName + "  v" + Application.ProductVersion + "\r\n" +
            "©2009-2026 Ali Adams - علي عبد الرزاق عبد الكريم القره غولي" + "\r\n" + "\r\n" +
            "Powered by Yet Another Factoring Utility\r\nBenjamin Buhrow <bbuhrow@gmail.com>" + "\r\n" + "\r\n" +
            "http://qurancode.com" + "\r\n" +
            "God > ∞    الله أكبر",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1
        );
    }
    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CloseApplication();
        Environment.Exit(0); // close Console and WinForms applications immediately without errors
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
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
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
    private void VersionLabel_MouseHover(object sender, EventArgs e)
    {
        ToolTip.SetToolTip(this.VersionLabel, "Version " + Globals.RELEASE_EDITION + "\r\n" + "©2009-2026 Ali Adams");
    }
}
