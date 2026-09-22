using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using System.IO;
using Model;

public partial class MainForm : Form
{
    public const string DATA_FOLDER = "Data";
    public const string DICTIONARY_FILE = "dictionary.txt";
    public const string STATISTICS_FOLDER = "Statistics";

    private void FixMicrosoft(object sender, KeyPressEventArgs e)
    {
        // stop annoying beep due to parent not having an AcceptButton
        if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Escape))
        {
            e.Handled = true;
        }
        // enable Ctrl+A to SelectAll
        if ((ModifierKeys == Keys.Control) && (e.KeyChar == 'A'))
        {
            TextBoxBase control = (sender as TextBoxBase);
            if (control != null)
            {
                control.SelectAll();
                e.Handled = true;
            }
        }
    }
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBoxBase)
        {
            TextBoxBase control = (sender as TextBoxBase);
            if (control != null)
            {
                if (control.Focused)
                {
                    if (ModifierKeys == Keys.Control)
                    {
                        if (e.KeyCode == Keys.A)
                        {
                            control.SelectAll();
                            e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
                        }
                        else if (e.KeyCode == Keys.F)
                        {
                            // Find dialog
                            e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
                        }
                        else if (e.KeyCode == Keys.H)
                        {
                            // Replace dialog
                            e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
                        }
                        else if (e.KeyCode == Keys.S)
                        {
                            // SaveAs dialog
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
            }
        }
    }

    private Client m_client = null;
    private string m_numerical_system_name = "Original_Abjad_Gematria";

    private string m_ini_filename = null;
    private void Initialize()
    {
        this.Top = Screen.PrimaryScreen.WorkingArea.Top;
        this.Left = Screen.PrimaryScreen.WorkingArea.Left;
        this.Width = (m_dpi == 96.0F) ? 300 : 375;
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

            LoadDictionary();
            SearchInDictionaryCheckBox.Checked = true;
            m_search_in_dictionary = true;
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void MainForm_Shown(object sender, EventArgs e)
    {
        SaveResultsLabel.Enabled = ((m_found_words != null) && (m_found_words.Count > 0));

        ColorizeDirectionsLabels();

        NotifyIcon.Visible = true;

        CipherTextBox.Focus();
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
    }
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
            {
                //if (MessageBox.Show("Cancel all progress?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    Cancel();
                }
            }
            e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
        }
        else if ((ModifierKeys == Keys.Control) && (e.KeyCode == Keys.Q))
        {
            m_neighbours_only = !m_neighbours_only;
            SearchButton.Width = m_neighbours_only ? 70 : 90;
            SearchInDictionaryCheckBox.Visible = !SearchInDictionaryCheckBox.Visible;
            e.SuppressKeyPress = true; // suppress annoying beep due to parent not having an AcceptButton
        }
        else if ((ModifierKeys == Keys.Control) && (e.KeyCode == Keys.S))
        {
            SaveResultsLabel_Click(null, null);
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

    private bool m_left_to_right = false;
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

            if (m_numerical_system_name.Contains("English"))
            {
                m_left_to_right = true;
                MessageTextBox.RightToLeft = RightToLeft.No;
                ResultsListBox.RightToLeft = RightToLeft.No;
            }
            else
            {
                m_left_to_right = false;
                MessageTextBox.RightToLeft = RightToLeft.Yes;
                ResultsListBox.RightToLeft = RightToLeft.Yes;
            }

            BuildMap();

            MessageTextBox.Text = Decode();

            CTextBox.Text = Encode(MTextBox.Text);
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
    private void CipherTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && (e.KeyChar != m_space);
    }
    private void CipherTextBox_TextChanged(object sender, EventArgs e)
    {
        if (TextModeComboBox.Items.Count > 0)
        {
            if (NumericalSystemComboBox.Items.Count > 0)
            {
                SearchButton.Enabled = !String.IsNullOrEmpty(CipherTextBox.Text);
                MessageTextBox.Text = Decode();
            }
            else
            {
                SearchButton.Enabled = false;
            }
        }
        else
        {
            SearchButton.Enabled = false;
        }
    }

    private List<string> m_words = null;
    private List<string> m_found_words = null;
    private void LoadDictionary()
    {
        string path = DATA_FOLDER + Path.DirectorySeparatorChar + DICTIONARY_FILE;
        if (File.Exists(path))
        {
            m_words = FileHelper.LoadLines(path);
            for (int i = 0; i < m_words.Count; i++)
            {
                m_words[i] = m_words[i].ToUpper();
            }
        }
    }

    private Dictionary<int, List<char>> m_map = null; // digit-to-letters map for each digit from 0 to 9
    private int m_rows = 0;
    private int m_cols = 0;
    private char m_space = ' ';
    private char m_placeholder = '_';
    private void BuildMap()
    {
        m_map = new Dictionary<int, List<char>>();
        if (m_map != null)
        {
            for (int n = 0; n < 10; n++)
            {
                m_map.Add(n, new List<char>());
                if (m_map[n] != null)
                {
                    int count = 0;
                    foreach (char key in m_client.NumericalSystem.Keys)
                    {
                        if ((m_numerical_system_name.Contains("English")) && (Constants.SMALL_ENGLISH_LETTERS.Contains(key))) continue;

                        int dr = Numbers.DigitalRoot(m_client.NumericalSystem[key]);
                        if (dr == n)
                        {
                            m_map[n].Add(key);
                            count++;
                        }
                    }

                    // if no chars found for n, add m_placehoder
                    if (m_map[n].Count == 0)
                    {
                        m_map[n].Add(m_placeholder);
                        count = 1;
                    }
                }
            }

            m_rows = 0;
            foreach (int key in m_map.Keys)
            {
                int count = 0;
                foreach (char c in m_map[key])
                {
                    if (c == m_placeholder) break;

                    count++;
                    if (m_rows < count)
                    {
                        m_rows = count;
                    }
                }
            }
        }

        PadMap();
    }
    private void PadMap()
    {
        if (m_map != null)
        {
            foreach (int key in m_map.Keys)
            {
                List<char> chars = m_map[key];
                if (chars != null)
                {
                    while (chars.Count < m_rows)
                    {
                        chars.Add(m_placeholder);
                    }
                }
            }
        }
    }
    private List<int> m_digits = null; // user digits
    private void BuildDigits()
    {
        m_digits = new List<int>();
        if (m_digits != null)
        {
            if (!String.IsNullOrEmpty(CipherTextBox.Text))
            {
                foreach (char c in CipherTextBox.Text)
                {
                    if (c == m_space) // space between decoded words
                    {
                        m_digits.Add(-1); // space between decoded words
                    }
                    else
                    {
                        int digit;
                        if (int.TryParse(c.ToString(), out digit))
                        {
                            m_digits.Add(digit);
                        }
                    }
                }
            }

            m_cols = m_digits.Count;
        }
    }
    private char[,] m_board = null; // all user digits to their letterss
    private void BuildBoard()
    {
        if (m_map != null)
        {
            if (m_digits != null)
            {
                m_board = new char[m_rows, m_cols];
                if (m_board != null)
                {
                    for (int i = 0; i < m_rows; i++)
                    {
                        for (int j = 0; j < m_cols; j++)
                        {
                            if (m_digits[j] == -1) // space between decoded words
                            {
                                m_board[i, j] = m_space;
                            }
                            else
                            {
                                if (m_map[m_digits[j]] != null)
                                {
                                    m_board[i, j] = m_map[m_digits[j]][i];
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private string Decode()
    {
        BuildDigits();
        BuildBoard();

        ClearResults();
        StringBuilder str = new StringBuilder();
        if (m_board != null)
        {
            if (m_left_to_right)
            {
                str.AppendLine();
                for (int i = 0; i < m_rows; i++)
                {
                    for (int j = 0; j < m_cols; j++)
                    {
                        str.Append(m_board[i, j]);
                    }
                    str.AppendLine();
                }
            }
            else // right_to_left
            {
                for (int i = m_rows - 1; i >= 0; i--)
                {
                    for (int j = 0; j < m_cols; j++)
                    {
                        str.Insert(0, m_board[i, j]);
                    }
                    str.Insert(0, "\r\n");
                }
            }
        }
        return str.ToString();
    }

    private Searcher m_worker = null;
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
    private void SearchButton_Click(object sender, EventArgs e)
    {
        if ((m_worker_thread != null) && (m_worker_thread.IsAlive))
        {
            //if (MessageBox.Show("Cancel all progress?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Cancel();
            }
        }
        else
        {
            Run();
        }
    }
    private bool m_search_in_dictionary = true;
    private void SearchInDictionaryCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_search_in_dictionary = SearchInDictionaryCheckBox.Checked;
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
                    if ((progress >= 0) && (progress <= 100))
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
        if (progress > 0)
        {
            if (m_time_display_mode == TimeDisplayMode.Elapsed)
            {
                long ticks = timespan.Ticks;
                timespan = new TimeSpan(ticks);
            }
            else if (m_time_display_mode == TimeDisplayMode.Remaining)
            {
                long ticks = (long)((double)timespan.Ticks * ((double)(100 - progress) / (double)progress));
                timespan = new TimeSpan(ticks);
            }
        }
        else
        {
            timespan = new TimeSpan(0, 0, 0);
        }

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
    }
    public delegate void UpdateResultsListBox(string result);
    public void UpdateResultsListBoxMethod(string result)
    {
        if (m_worker != null)
        {
            if (!m_worker.Cancel)
            {
                // FIX: prevent race condition
                Thread.Sleep(10);

                if (result.Length == 0) return;

                if (m_found_words != null)
                {
                    if (!m_found_words.Contains(result))
                    {
                        m_found_words.Add(result);
                        ResultsListBox.Items.Add(result);
                        ResultsCountLabel.Text = ResultsListBox.Items.Count.ToString();
                        SaveResultsLabel.Enabled = true;
                    }
                }
            }
        }
    }

    private void BeforeProcessing()
    {
        ClearResults();
        DisableEntryControls();

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
        ProgressValueLabel.Text = ProgressBar.Value + "%";
        ProgressValueLabel.Refresh();

        if (m_found_words != null)
        {
            m_found_words.Clear();
        }
        else
        {
            m_found_words = new List<string>();
        }
        ResultsListBox.Items.Clear();
        ResultsCountLabel.Text = "0";
        SaveResultsLabel.Enabled = false;
    }
    private void DisableEntryControls()
    {
        //SearchButton.Enabled = false;
        SearchButton.Text = "Cancel";

        TextModeComboBox.Enabled = false;
        NumericalSystemComboBox.Enabled = false;
        CipherTextBox.Enabled = false;
        MessageTextBox.Enabled = false;
        MTextBox.Enabled = false;
        CTextBox.Enabled = false;
        CLabel.Enabled = false;
        NLabel.Enabled = false;
        SLabel.Enabled = false;
        ELabel.Enabled = false;
        WLabel.Enabled = false;
        NELabel.Enabled = false;
        NWLabel.Enabled = false;
        SELabel.Enabled = false;
        SWLabel.Enabled = false;

        SearchButton.Refresh();
        TextModeComboBox.Refresh();
        NumericalSystemComboBox.Refresh();
        CipherTextBox.Refresh();
        MessageTextBox.Refresh();
        MTextBox.Refresh();
        CTextBox.Refresh();
        CLabel.Refresh();
        NLabel.Refresh();
        SLabel.Refresh();
        ELabel.Refresh();
        WLabel.Refresh();
        NELabel.Refresh();
        NWLabel.Refresh();
        SELabel.Refresh();
        SWLabel.Refresh();
    }
    private void EnableEntryControls()
    {
        //SearchButton.Enabled = true;
        SearchButton.Text = "Search";

        TextModeComboBox.Enabled = true;
        NumericalSystemComboBox.Enabled = true;
        CipherTextBox.Enabled = true;
        MessageTextBox.Enabled = true;
        MTextBox.Enabled = true;
        CTextBox.Enabled = true;
        CLabel.Enabled = true;
        NLabel.Enabled = true;
        SLabel.Enabled = true;
        ELabel.Enabled = true;
        WLabel.Enabled = true;
        NELabel.Enabled = true;
        NWLabel.Enabled = true;
        SELabel.Enabled = true;
        SWLabel.Enabled = true;

        TextModeComboBox.Refresh();
        NumericalSystemComboBox.Refresh();
        CipherTextBox.Refresh();
        MessageTextBox.Refresh();
        MTextBox.Refresh();
        CTextBox.Refresh();
        SearchButton.Refresh();
        CLabel.Refresh();
        NLabel.Refresh();
        SLabel.Refresh();
        ELabel.Refresh();
        WLabel.Refresh();
        NELabel.Refresh();
        NWLabel.Refresh();
        SELabel.Refresh();
        SWLabel.Refresh();

        CipherTextBox.Focus();
    }

    // hidden Boggle game. Ctrl+Q
    private bool m_neighbours_only = false;
    private Directions m_directions = Directions.All;
    private Color m_inactive_color = SystemColors.ControlDark;
    private Color m_active_color = Color.Yellow;
    private void DirectionsLabel_Click(object sender, EventArgs e)
    {
        Control control = sender as Label;
        if (control != null)
        {
            if (control == CLabel)
                if (m_directions == Directions.None)
                    m_directions = Directions.All;
                else
                    m_directions = Directions.None;

            else if (control == NLabel)
                if ((m_directions & Directions.North) == Directions.North)    // has
                    m_directions &= ~Directions.North;                        // remove
                else
                    m_directions |= Directions.North;                         // add

            else if (control == SLabel)
                if ((m_directions & Directions.South) == Directions.South)    // has
                    m_directions &= ~Directions.South;                        // remove
                else
                    m_directions |= Directions.South;                         // add

            else if (control == ELabel)
                if ((m_directions & Directions.East) == Directions.East)      // has
                    m_directions &= ~Directions.East;                         // remove
                else
                    m_directions |= Directions.East;                          // add

            else if (control == WLabel)
                if ((m_directions & Directions.West) == Directions.West)      // has
                    m_directions &= ~Directions.West;                         // remove
                else
                    m_directions |= Directions.West;                          // add

            else if (control == NELabel)
                if ((m_directions & Directions.NorthEast) == Directions.NorthEast)    // has
                    m_directions &= ~Directions.NorthEast;                            // remove
                else
                    m_directions |= Directions.NorthEast;                             // add

            else if (control == NWLabel)
                if ((m_directions & Directions.NorthWest) == Directions.NorthWest)    // has
                    m_directions &= ~Directions.NorthWest;                            // remove
                else
                    m_directions |= Directions.NorthWest;                             // add

            else if (control == SELabel)
                if ((m_directions & Directions.SouthEast) == Directions.SouthEast)    // has
                    m_directions &= ~Directions.SouthEast;                            // remove
                else
                    m_directions |= Directions.SouthEast;                             // add

            else if (control == SWLabel)
                if ((m_directions & Directions.SouthWest) == Directions.SouthWest)    // has
                    m_directions &= ~Directions.SouthWest;                            // remove
                else
                    m_directions |= Directions.SouthWest;                             // add

            ColorizeDirectionsLabels();
            SetTooltips();
        }
    }
    private void ColorizeDirectionsLabels()
    {
        NLabel.BackColor = ((m_directions & Directions.North) == Directions.North) ? m_active_color : m_inactive_color;
        SLabel.BackColor = ((m_directions & Directions.South) == Directions.South) ? m_active_color : m_inactive_color;
        ELabel.BackColor = ((m_directions & Directions.East) == Directions.East) ? m_active_color : m_inactive_color;
        WLabel.BackColor = ((m_directions & Directions.West) == Directions.West) ? m_active_color : m_inactive_color;
        NELabel.BackColor = ((m_directions & Directions.NorthEast) == Directions.NorthEast) ? m_active_color : m_inactive_color;
        NWLabel.BackColor = ((m_directions & Directions.NorthWest) == Directions.NorthWest) ? m_active_color : m_inactive_color;
        SELabel.BackColor = ((m_directions & Directions.SouthEast) == Directions.SouthEast) ? m_active_color : m_inactive_color;
        SWLabel.BackColor = ((m_directions & Directions.SouthWest) == Directions.SouthWest) ? m_active_color : m_inactive_color;
    }
    private void SetTooltips()
    {
        if (m_directions == Directions.None) ToolTip.SetToolTip(this.CLabel, "Select All");
        else ToolTip.SetToolTip(this.CLabel, "Deselect All");

        ToolTip.SetToolTip(this.NLabel, Directions.North.ToString());
        ToolTip.SetToolTip(this.SLabel, Directions.South.ToString());
        ToolTip.SetToolTip(this.ELabel, Directions.East.ToString());
        ToolTip.SetToolTip(this.WLabel, Directions.West.ToString());
        ToolTip.SetToolTip(this.NELabel, Directions.NorthEast.ToString());
        ToolTip.SetToolTip(this.NWLabel, Directions.NorthWest.ToString());
        ToolTip.SetToolTip(this.SELabel, Directions.SouthEast.ToString());
        ToolTip.SetToolTip(this.SWLabel, Directions.SouthWest.ToString());
    }

    private void Run()
    {
        // guard against multiple runs on multiple ENTER key presses
        if ((m_worker_thread != null) && (m_worker_thread.IsAlive)) return;

        BeforeProcessing();
        try
        {
            m_worker = new Searcher(this, m_words, m_board);
            if (m_worker != null)
            {
                m_worker_thread = new Thread(() => m_worker.Run(m_left_to_right, m_directions, m_neighbours_only, m_search_in_dictionary, m_placeholder));
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
        ProgressLabel.Text = "";
        ToolTip.SetToolTip(this.ProgressLabel, "Cancelled!");
        ProgressLabel.Refresh();

        EnableEntryControls();
    }
    private void AfterProcessing()
    {
        this.Cursor = Cursors.Default;

        if (m_worker != null)
        {
            m_old_progress = -1;
            ToolTip.SetToolTip(this.ProgressLabel, "Finished");
            ProgressLabel.Refresh();

            EnableEntryControls();
        }
    }
    private void SaveResultsLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_found_words != null)
            {
                if (m_found_words.Count > 0)
                {
                    string path = STATISTICS_FOLDER + Path.DirectorySeparatorChar + Application.ProductName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + ".txt";
                    FileHelper.SaveWords(path, m_found_words);
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
    private void ViewDictionaryLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            string path = DATA_FOLDER + Path.DirectorySeparatorChar + DICTIONARY_FILE;
            FileHelper.DisplayFile(path);
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

    private void MTextBox_TextChanged(object sender, EventArgs e)
    {
        CTextBox.Text = Encode(MTextBox.Text);
    }
    private string Encode(string text)
    {
        StringBuilder str = new StringBuilder();

        if (String.IsNullOrEmpty(text))
        {
            VersionLabel.Text = "w w w . q u r a n c o d e . c o m";
        }
        else
        {
            text = text.Simplify29();

            string word_value_text = "";
            long word_values_sum = 0L;
            long letter_values_sum = 0L;
            foreach (char c in text)
            {
                if (c == m_space)
                {
                    if (m_left_to_right)
                    {
                        str.Append(m_space);
                    }
                    else // right_to_left
                    {
                        str.Insert(0, m_space);
                    }

                    if (!String.IsNullOrEmpty(word_value_text))
                    {
                        word_values_sum += long.Parse(word_value_text);
                    }
                    word_value_text = "";
                }
                else // digit
                {
                    long value = m_client.CalculateValue(c);
                    int dr = Numbers.DigitalRoot(value);
                    letter_values_sum += dr;

                    if (m_left_to_right)
                    {
                        str.Append(dr.ToString());
                        word_value_text += (dr.ToString());
                    }
                    else // right_to_left
                    {
                        str.Insert(0, dr.ToString());
                        word_value_text = word_value_text.Insert(0, dr.ToString());
                    }
                }
            }
            if (!String.IsNullOrEmpty(word_value_text))
            {
                word_values_sum += long.Parse(word_value_text);
                VersionLabel.Text = "مجموع الحروف = " + letter_values_sum.ToString() + " | " + "مجموع الكلمات = " + word_values_sum.ToString();
                VersionLabel.ForeColor = Numbers.GetNumberForeColor(word_values_sum);
                VersionLabel.Refresh();
            }
            else
            {
                VersionLabel.Text = "w w w . q u r a n c o d e . c o m";
            }
        }

        return str.ToString();
    }
}
