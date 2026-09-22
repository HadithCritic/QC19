using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.IO;
using Model;

public partial class MainForm : Form
{
    private Client m_client = null;
    private string m_numerical_system_name = "Original_Abjad_Gematria";
    private List<string> m_found_words = null;

    private string m_ini_filename = null;
    private void Initialize()
    {
        this.Top = Screen.PrimaryScreen.WorkingArea.Top;
        this.Left = Screen.PrimaryScreen.WorkingArea.Left;
        this.Width = (m_dpi == 96.0F) ? 478 : 600;
        this.Height = (m_dpi == 96.0F) ? 400 : 500;
    }
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
                                    case "NumericalSystem":
                                        {
                                            m_numerical_system_name = parts[1];
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Initialize();
                }
            }
        }
        else // first start
        {
            Initialize();
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

                writer.WriteLine("[Calculations]");
                writer.WriteLine("NumericalSystem=" + this.m_numerical_system_name);
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
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
            InitializeComponent();
        }

        AboutToolStripMenuItem.Font = new Font(AboutToolStripMenuItem.Font, AboutToolStripMenuItem.Font.Style | FontStyle.Bold);

        m_ini_filename = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".ini");
        LoadSettings();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            m_client = new Client(m_numerical_system_name);
            if (m_client != null)
            {
                if (m_client.NumericalSystem != null)
                {
                    string default_text_mode = m_client.NumericalSystem.TextMode;
                    m_client.BuildSimplifiedBook(default_text_mode, false, true, false, false, true, false, false, false, false);

                    PopulateTextModeComboBox();
                    if (TextModeComboBox.Items.Count > 0)
                    {
                        if (TextModeComboBox.Items.Contains(default_text_mode))
                        {
                            TextModeComboBox.SelectedItem = default_text_mode;
                        }
                        else
                        {
                            TextModeComboBox.SelectedIndex = 0;
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
    private void MainForm_Shown(object sender, EventArgs e)
    {
        NotifyIcon.Visible = true;

        SizeNumericUpDown.Focus();
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

        ResultsColumnHeader3.Width = this.ClientRectangle.Width - ResultsListView.Left - ResultsColumnHeader1.Width - ResultsColumnHeader2.Width - 27; // 27 Scrollbar width
    }
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
        }
        else if ((ModifierKeys == Keys.Control) && (e.KeyCode == Keys.S))
        {
            ResultsSaveButton_Click(null, null);
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
    }
    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        CloseApplication();
    }
    private void CloseApplication()
    {
        // remove icon from tray
        if (NotifyIcon != null)
        {
            NotifyIcon.Visible = false;
            NotifyIcon.Dispose();
        }

        SaveSettings();
    }

    private Dictionary<long, char> m_dictionary = null;
    private void PopulateTextModeComboBox()
    {
        try
        {
            TextModeComboBox.SelectedIndexChanged -= new EventHandler(TextModeComboBox_SelectedIndexChanged);

            if (m_client != null)
            {
                if (m_client.NumericalSystem != null)
                {
                    if (m_client.LoadedNumericalSystems != null)
                    {
                        TextModeComboBox.BeginUpdate();

                        TextModeComboBox.Items.Clear();
                        foreach (NumericalSystem ns in m_client.LoadedNumericalSystems.Values)
                        {
                            string[] parts = ns.Name.Split('_');
                            if (parts != null)
                            {
                                if (parts.Length == 3)
                                {
                                    string text_mode = parts[0];
                                    if (!TextModeComboBox.Items.Contains(text_mode))
                                    {
                                        TextModeComboBox.Items.Add(text_mode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            TextModeComboBox.EndUpdate();
            TextModeComboBox.SelectedIndexChanged += new EventHandler(TextModeComboBox_SelectedIndexChanged);
        }
    }
    private void PopulateNumericalSystemComboBox()
    {
        try
        {
            NumericalSystemComboBox.SelectedIndexChanged -= new EventHandler(NumericalSystemComboBox_SelectedIndexChanged);

            if (m_client != null)
            {
                if (m_client.LoadedNumericalSystems != null)
                {
                    NumericalSystemComboBox.BeginUpdate();

                    if (TextModeComboBox.SelectedItem != null)
                    {
                        string text_mode = TextModeComboBox.SelectedItem.ToString();

                        NumericalSystemComboBox.Items.Clear();
                        foreach (NumericalSystem ns in m_client.LoadedNumericalSystems.Values)
                        {
                            string[] parts = ns.Name.Split('_');
                            if (parts != null)
                            {
                                if (parts.Length == 3)
                                {
                                    if (parts[0] == text_mode)
                                    {
                                        string valuation_system = parts[1] + "_" + parts[2];
                                        if (!NumericalSystemComboBox.Items.Contains(valuation_system))
                                        {
                                            NumericalSystemComboBox.Items.Add(valuation_system);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            NumericalSystemComboBox.EndUpdate();
            NumericalSystemComboBox.SelectedIndexChanged += new EventHandler(NumericalSystemComboBox_SelectedIndexChanged);
        }
    }
    private void TextModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (m_client.NumericalSystem != null)
            {
                PopulateNumericalSystemComboBox();
                if (NumericalSystemComboBox.Items.Count > 0)
                {
                    int pos = m_client.NumericalSystem.Name.IndexOf("_");
                    string default_letter_valuation = m_client.NumericalSystem.Name.Substring(pos + 1);
                    if (NumericalSystemComboBox.Items.Contains(default_letter_valuation))
                    {
                        NumericalSystemComboBox.SelectedItem = default_letter_valuation;
                    }
                    else
                    {
                        NumericalSystemComboBox.SelectedIndex = 0;
                    }
                }
                else
                {
                    NumericalSystemComboBox.SelectedIndex = -1;
                }
            }
        }
    }
    private void NumericalSystemComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        m_numerical_system_name = TextModeComboBox.SelectedItem.ToString() + "_" + NumericalSystemComboBox.SelectedItem.ToString();
        if (m_client != null)
        {
            m_client.LoadNumericalSystem(m_numerical_system_name);

            // update tooltip with new numerical_system values
            NumericalSystemComboBox_MouseHover(null, null);

            if (m_client.NumericalSystem != null)
            {
                m_dictionary = new Dictionary<long, char>();
                foreach (char key in m_client.NumericalSystem.Keys)
                {
                    if (!m_dictionary.ContainsKey(m_client.NumericalSystem[key]))
                    {
                        m_dictionary.Add(m_client.NumericalSystem[key], key);
                    }
                }
            }

            LoadLetterValuesButton_Click(null, null);
        }
    }
    private void NumericalSystemComboBox_MouseHover(object sender, EventArgs e)
    {
        if (m_client != null)
        {
            if (m_client.NumericalSystem != null)
            {
                StringBuilder str = new StringBuilder();
                foreach (char key in m_client.NumericalSystem.Keys)
                {
                    str.AppendLine(key.ToString() + "\t" + m_client.NumericalSystem[key].ToString());
                }
                ToolTip.SetToolTip(this.NumericalSystemComboBox, str.ToString());
            }
        }
    }

    private void NumbersTextBox_TextChanged(object sender, EventArgs e)
    {
        EnableDisableControls();
    }
    private void NumbersTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && (e.KeyChar != ',');
    }
    private void NumbersTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        TextBox control = sender as TextBox;
        if (ModifierKeys == Keys.Control)
        {
            if (e.KeyCode == Keys.A)
            {
                control.SelectAll();
                e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
            }
            else
            {
                // don't e.SuppressKeyPress = true;
            }
        }
        else
        {
            if (e.KeyCode == Keys.Enter)
            {
                control.Text.Insert(control.SelectionStart, "\n");
            }
            else if (e.KeyCode == Keys.Tab)
            {
                control.Text.Insert(control.SelectionStart, "\t");
            }
            else if (e.KeyCode == Keys.Escape)
            {
                control.SelectionLength = 0;
            }
            else // Keys.Up || Keys.Down || Keys.Left || Keys.Right || Keys.PageUp ...
            {
            }
        }
    }
    private void LoadLetterValuesButton_Click(object sender, EventArgs e)
    {
        if (m_dictionary != null)
        {
            StringBuilder str = new StringBuilder();
            foreach (long key in m_dictionary.Keys)
            {
                str.Append(key.ToString() + ",");
            }
            foreach (long key in m_dictionary.Keys)
            {
                str.Append(key.ToString() + ",");
            }
            foreach (long key in m_dictionary.Keys)
            {
                str.Append(key.ToString() + ",");
            }
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }
            NumbersTextBox.Text = str.ToString();
        }
    }
    private void SizeNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        m_subset_size = (int)SizeNumericUpDown.Value;
        EnableDisableControls();
    }
    private void SizeNumericUpDown_TextChanged(object sender, EventArgs e)
    {
        SizeNumericUpDown_ValueChanged(null, null);

        if (SizeNumericUpDown.Focused)
        {
            if (m_autorun)
            {
                FindWordsButton_Click(null, null);
            }
        }
    }
    private void SumNumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        m_subset_sum = (long)SumNumericUpDown.Value;
        EnableDisableControls();
    }
    private void SumNumericUpDown_TextChanged(object sender, EventArgs e)
    {
        SumNumericUpDown_ValueChanged(null, null);

        if (SumNumericUpDown.Focused)
        {
            if (m_autorun)
            {
                FindWordsButton_Click(null, null);
            }
        }
    }
    private void ResultsListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
        EnableDisableControls();
    }
    private void AutoRunCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_autorun = AutoRunCheckBox.Checked;
    }

    private long[] m_numbers = null;
    private int m_subset_size = 0;
    private long m_subset_sum = 0L;
    private DateTime m_start;
    private bool m_autorun = false;

    private void FindWordsButton_Click(object sender, EventArgs e)
    {
        BeforeProcessing();

        if (m_found_words != null)
        {
            List<long> numbers = new List<long>();
            char[] separators = new char[] { ',' };
            string[] parts = NumbersTextBox.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                numbers.Add(long.Parse(part));
            }
            m_numbers = numbers.ToArray();

            m_found_words.Clear();
            ResultsListView.Items.Clear();
            ResultsSaveButton.Enabled = false;

            FindWords();

            AfterProcessing();
        }
    }
    private void BeforeProcessing()
    {
        ClearResults();

        this.Cursor = Cursors.WaitCursor;
    }
    private void ClearResults()
    {
        ElapsedTimeValueLabel.Text = "00:00:00";
        ElapsedTimeValueLabel.Refresh();
        ToolTip.SetToolTip(this.ProgressLabel, "Searching ...");
        ProgressLabel.Refresh();
        ProgressBar.Value = 0;
        ProgressBar.Refresh();

        if (m_found_words != null)
        {
            m_found_words.Clear();
        }
        else
        {
            m_found_words = new List<string>();
        }

        ResultsListView.Items.Clear();
        ResultsCountLabel.Text = "0";
        ResultsSaveButton.Enabled = false;
    }
    private void EnableDisableControls()
    {
        FindWordsButton.Enabled = (
                                        (NumbersTextBox.Text.Length > 2) &&
                                        (NumbersTextBox.Text.Contains(",")) &&
                                        (m_subset_size > 0) &&
                                        (m_subset_sum > 0L) &&
                                        (m_subset_sum > m_subset_size)
                                    );
        FindWordsButton.Refresh();

        ResultsSaveButton.Enabled = (
                                        (ResultsListView.Items.Count > 0) &&
                                        (m_subset_size > 0) &&
                                        (m_subset_sum > 0L) &&
                                        (m_subset_sum > m_subset_size)
                                     );
        ResultsSaveButton.Refresh();
    }
    private void FindWords()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            FindWordsButton.Enabled = false;

            m_start = DateTime.Now;

            Subsets subsets = new Subsets(m_numbers);
            subsets.Find(m_subset_size, m_subset_sum, OnFound);
        }
        catch
        {
            // ignore
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void OnFound(Subsets.Item[] subset)
    {
        if (m_found_words != null)
        {
            string[] item_parts = new string[3];
            StringBuilder str = new StringBuilder();
            foreach (Subsets.Item item in subset)
            {
                if (m_dictionary != null)
                {
                    if (m_dictionary.ContainsKey(item.Value))
                    {
                        str.Insert(0, " " + m_dictionary[item.Value]);
                    }
                }
            }
            if (str.Length > 0)
            {
                str.Remove(0, 1);
            }
            item_parts[1] = str.ToString();

            if (!m_found_words.Contains(item_parts[1]))
            {
                m_found_words.Add(item_parts[1]);
                item_parts[0] = m_found_words.Count.ToString();

                char[] letters = item_parts[1].Replace(" ", "").ToCharArray();
                Permutations<char> sets = new Permutations<char>(letters, GenerateOption.WithoutRepetition);
                StringBuilder sss = new StringBuilder();
                foreach (List<char> set in sets)
                {
                    foreach (char c in set)
                    {
                        sss.Append(c);
                    }
                    sss.Append("   ");
                }
                item_parts[2] = sss.ToString();

                ResultsListView.Items.Add(new ListViewItem(item_parts));
                ResultsListView.Items[ResultsListView.Items.Count - 1].Selected = true;
                ResultsListView.Items[ResultsListView.Items.Count - 1].EnsureVisible();
                ResultsListView.Refresh();

                ResultsCountLabel.Text = ResultsListView.Items.Count.ToString();
                ResultsCountLabel.Refresh();

                TimeSpan timespan = DateTime.Now - m_start;
                if (timespan.Days > 0)
                {
                    ElapsedTimeValueLabel.Text = String.Format("{0:0}d {1:00}:{2:00}:{3:00}", timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds);
                    ElapsedTimeValueLabel.Refresh();
                }
                else
                {
                    ElapsedTimeValueLabel.Text = String.Format("{0:00}:{1:00}:{2:00}.{3:000}", timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                    ElapsedTimeValueLabel.Refresh();
                }

                ProgressBar.Value = (ResultsListView.Items.Count <= 100) ? ResultsListView.Items.Count : 100;
                ProgressBar.Refresh();
            }
        }
    }
    private void AfterProcessing()
    {
        this.Cursor = Cursors.Default;

        ToolTip.SetToolTip(this.ProgressLabel, "Finished");
        ProgressLabel.Refresh();

        ProgressBar.Value = 50;
        ProgressBar.Refresh();

        EnableDisableControls();
    }
    private void ResultsSaveButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_found_words != null)
            {
                if (m_found_words.Count > 0)
                {
                    StringBuilder str = new StringBuilder();
                    int count = 0;
                    foreach (string word in m_found_words)
                    {
                        char[] letters = word.Replace(" ", "").ToCharArray();
                        Permutations<char> sets = new Permutations<char>(letters, GenerateOption.WithoutRepetition);
                        StringBuilder sss = new StringBuilder();
                        foreach (List<char> set in sets)
                        {
                            foreach (char c in set)
                            {
                                sss.Append(c);
                            }
                            sss.Append("   ");
                        }

                        count++;
                        str.AppendLine(count.ToString() + "\t" + word + "\t" + sss);
                    }

                    string path = Application.ProductName + "_" + m_numerical_system_name + "_" + m_subset_sum.ToString() + "_" + m_subset_size.ToString() + ".txt";
                    FileHelper.SaveText(path, str.ToString());
                    FileHelper.DisplayFile(path);
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
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
            "\r\n" +
            "©2009-2026 Ali Adams - علي عبد الرزاق عبد الكريم القره غولي" + "\r\n" +
            "http://qurancode.com" + "\r\n" +
            "God > ∞   الله أكبر",
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
        ToolTip.SetToolTip(this.VersionLabel, "Version " + Globals.SHORT_VERSION + "\r\n" + "©2009-2026 Ali Adams");
    }
}
