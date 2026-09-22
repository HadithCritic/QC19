using System;
using System.Drawing;
using System.Windows.Forms;


namespace Divisibility
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            NumberTextBox.Focus();
            StatusLabel.Text = "Ready";
            StatusLabel.ForeColor = SystemColors.WindowText;
        }

        private void NumberTextBox_TextChanged(object sender, EventArgs e)
        {
            Calculate();
        }
        private void NumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (NumberTextBox.Text == "Number")
            {
                NumberTextBox.Text = "";
                NumberTextBox.ForeColor = SystemColors.WindowText;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private void NumberTextBox_Leave(object sender, EventArgs e)
        {
            if (NumberTextBox.Text.Length == 0)
            {
                NumberTextBox.Text = "Number";
                NumberTextBox.ForeColor = SystemColors.ControlDark;

                Clear();
            }
        }

        private void PrimeTextBox_TextChanged(object sender, EventArgs e)
        {
            Calculate();
        }
        private void PrimeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (PrimeTextBox.Text == "Prime")
            {
                PrimeTextBox.Text = "";
                PrimeTextBox.ForeColor = SystemColors.WindowText;
            }

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            if (PrimeTextBox.Text.Length == 0)
            {
                Clear();
            }
        }
        private void PrimeTextBox_Leave(object sender, EventArgs e)
        {
            if (PrimeTextBox.Text.Length == 0)
            {
                PrimeTextBox.Text = "Prime";
                PrimeTextBox.ForeColor = SystemColors.ControlDark;

                Clear();
            }
        }
        private void Clear()
        {
            PrimeMultiplierTextBox.Text = "";
            PrimeMultiplierTextBox.ForeColor = SystemColors.ControlDark;
            ModifiedPrimeTextBox.Text = "";
            ModifiedPrimeTextBox.ForeColor = SystemColors.ControlDark;
            UnitMultiplierTextBox.Text = "";
            UnitMultiplierTextBox.ForeColor = SystemColors.ControlDark;
        }

        private void Calculate()
        {
            CalculationTextBox.Text = "Is Number divisible by Prime?";
            CalculationTextBox.ForeColor = SystemColors.WindowText;
            CalculationTextBox.BackColor = Color.LightSlateGray;
            StatusLabel.Text = "Ready";
            StatusLabel.ForeColor = SystemColors.WindowText;

            long number;
            if (long.TryParse(NumberTextBox.Text, out number))
            {
                long p;
                if (long.TryParse(PrimeTextBox.Text, out p))
                {
                    if (p.IsPrime())
                    {
                        PrimeTextBox.ForeColor = SystemColors.WindowText;

                        long pr = p % 10L;
                        int m = 0;
                        if (pr == 2L)
                        {
                            if ((number % 2L) == 0L)
                            {
                                CalculationTextBox.Text = number.ToString() + " is divisible by 2.";
                                CalculationTextBox.BackColor = Color.FromArgb(240, 255, 240);
                                CalculationTextBox.ForeColor = Color.Green;
                            }
                            else
                            {
                                CalculationTextBox.Text = number.ToString() + " is NOT divisible by 2.";
                                CalculationTextBox.BackColor = Color.FromArgb(255, 240, 240);
                                CalculationTextBox.ForeColor = Color.Red;
                            }
                            StatusLabel.Text = number.ToString() + " / " + p.ToString() + " = " + ((double)number / p).ToString();
                            StatusLabel.ForeColor = SystemColors.WindowText;

                            return;
                        }
                        else if (pr == 5L)
                        {
                            if ((number % 5L) == 0L)
                            {
                                CalculationTextBox.Text = number.ToString() + " is divisible by 5";
                                CalculationTextBox.BackColor = Color.FromArgb(240, 255, 240);
                                CalculationTextBox.ForeColor = Color.Green;
                            }
                            else
                            {
                                CalculationTextBox.Text = number.ToString() + " is NOT divisible by 5.";
                                CalculationTextBox.BackColor = Color.FromArgb(255, 240, 240);
                                CalculationTextBox.ForeColor = Color.Red;
                            }
                            StatusLabel.Text = number.ToString() + " / " + p.ToString() + " = " + ((double)number / p).ToString();
                            StatusLabel.ForeColor = SystemColors.WindowText;

                            return;
                        }

                        else if ((pr == 1L) || (pr == 9L))
                        {
                            m = 1;
                        }
                        else if ((pr == 3L) || (pr == 7L))
                        {
                            m = 3;
                        }
                        else
                        {
                            m = 0;
                            StatusLabel.Text = "Prime numbers end in 1, 3, 7, 9.";
                            StatusLabel.ForeColor = Color.Red;
                        }
                        PrimeMultiplierTextBox.Text = m.ToString();
                        long mp = m * p;
                        ModifiedPrimeTextBox.Text = mp.ToString();
                        long mpf = mp / 10L;
                        long mpr = mp % 10L;

                        // find x for mpr = 1 or 9
                        long x = 0;
                        if (mpr == 1L)
                        {
                            x = -mpf;
                        }
                        else //if (mpr == 9L)
                        {
                            x = mpf + 1L;
                        }
                        UnitMultiplierTextBox.Text = x.ToString();
                        UnitMultiplierTextBox.ForeColor = SystemColors.WindowText;

                        // loop while number >= prime
                        if (number < 0) number = -number;
                        long n = number;
                        long nf = 0L;
                        long nr = 0L;
                        long old = 0L;
                        CalculationTextBox.Text = number.ToString() + Environment.NewLine;
                        do
                        {
                            old = n;
                            nf = n / 10L;
                            nr = n % 10L;
                            n = nf + (nr * x);
                            if (old == n) break;

                            CalculationTextBox.Text +=
                                nf.ToString() + " + (" +
                                nr.ToString() + " * " + x.ToString() + ")" +
                                " = " + n.ToString() +
                                //n.ToString() + " = " +
                                //nf.ToString() + " + (" +
                                //nr.ToString() + " * " + x.ToString() + ")" +
                                Environment.NewLine;
                        } while (n > p);

                        CalculationTextBox.Text += Environment.NewLine;
                        if ((n % p) == 0L)
                        {
                            CalculationTextBox.Text += NumberTextBox.Text + " is divisible by " + PrimeTextBox.Text;
                            CalculationTextBox.BackColor = Color.FromArgb(240, 255, 240);
                            CalculationTextBox.ForeColor = Color.Green;
                        }
                        else
                        {
                            CalculationTextBox.Text += NumberTextBox.Text + " is NOT divisible by " + PrimeTextBox.Text;
                            CalculationTextBox.BackColor = Color.FromArgb(255, 240, 240);
                            CalculationTextBox.ForeColor = Color.Red;
                        }

                        StatusLabel.Text =
                            number.ToString() + " / " + p.ToString() +
                            " = " + ((double)number / (double)p).ToString();
                        StatusLabel.ForeColor = SystemColors.WindowText;
                    }
                    else
                    {
                        PrimeTextBox.ForeColor = Color.Red;
                        StatusLabel.Text = PrimeTextBox.Text + " is NOT prime!";
                        StatusLabel.ForeColor = Color.Red;

                        Clear();
                    }
                }

                NumberTextBox.ForeColor = SystemColors.WindowText;
            }
            else
            {
                NumberTextBox.ForeColor = Color.Red;
            }
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
                            System.Diagnostics.Process.Start(control.Tag.ToString());
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
