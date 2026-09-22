using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Model;

namespace QuranNet
{
    public partial class MainForm : Form
    {
        // https://www.alt-codes.net/arrow_alt_codes.php
        private const int CHAPTERS = 114;
        private const string ASC = " ▲";   // ˄ ▲ ↑
        private const string DESC = " ▼";  // ˅ ▼ ↓
        private const string DATA_FOLDER = "3dQuran";

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
        private string m_text_mode = "Original";
        private string m_numerology_system_name = NumerologySystem.DEFAULT_NAME;

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

            m_ini_filename = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".ini");
            LoadApplicationOptions();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = Application.ProductName + " " + Globals.SHORT_VERSION;

            this.Cursor = Cursors.WaitCursor;
            try
            {
                m_client = new Client(m_numerology_system_name);
                if (m_client != null)
                {
                    if (m_client.NumerologySystem != null)
                    {
                        m_text_mode = m_client.NumerologySystem.TextMode;

                        PopulateTextModeComboBox();
                        if (TextModeComboBox.Items.Count > 0)
                        {
                            if (TextModeComboBox.Items.Contains(m_text_mode))
                            {
                                TextModeComboBox.SelectedItem = m_text_mode;
                            }
                            else
                            {
                                TextModeComboBox.SelectedIndex = 0;
                            }
                        }
                    }
                }

                LoadSpecialChapterNumbers();
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            ChaptersComboBox.Items.Clear();
            ChaptersComboBox.Items.Add("All");  // all chapters
            for (int i = 1; i <= CHAPTERS; i++)
            {
                ChaptersComboBox.Items.Add(i.ToString());
            }
            ChaptersComboBox.Items.Add("Few");      // speical chapters
            ChaptersComboBox.Items.Add("1by1");     // chapter by chapter each in its own filepath
            ChaptersComboBox.SelectedIndex = CHAPTERS + 1;


            LinkBy[] link_bys = (LinkBy[])Enum.GetValues(typeof(LinkBy));
            LinkByComboBox.Items.Clear();
            foreach (LinkBy link_by in link_bys)
            {
                LinkByComboBox.Items.Add(link_by.ToString());
            }
            LinkByComboBox.SelectedIndex = 0;

            LinkTo[] link_tos = (LinkTo[])Enum.GetValues(typeof(LinkTo));
            LinkToComboBox.Items.Clear();
            foreach (LinkTo link_to in link_tos)
            {
                LinkToComboBox.Items.Add(link_to.ToString());
            }
            LinkToComboBox.SelectedIndex = 2;

            MinRangeComboBox.Items.Clear();
            for (int i = 1; i <= 28; i++) //??? hard-coded :(
            {
                MinRangeComboBox.Items.Add(i.ToString());
            }
            MinRangeComboBox.SelectedIndex = 0;
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveApplicationOptions();
        }
        private bool m_show_full_version = false;
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                m_show_full_version = !m_show_full_version;
                if (m_show_full_version)
                {
                    this.Text = Application.ProductName + " " + Globals.LONG_VERSION + " " + Globals.RELEASE;
                }
                else
                {
                    this.Text = Application.ProductName + " " + Globals.SHORT_VERSION;
                }
            }
            this.Refresh();
        }

        private void PopulateTextModeComboBox()
        {
            try
            {
                TextModeComboBox.SelectedIndexChanged -= new EventHandler(TextModeComboBox_SelectedIndexChanged);

                if (m_client != null)
                {
                    if (m_client.NumerologySystem != null)
                    {
                        if (m_client.LoadedNumerologySystems != null)
                        {
                            TextModeComboBox.BeginUpdate();

                            TextModeComboBox.Items.Clear();
                            foreach (NumerologySystem numerology_system in m_client.LoadedNumerologySystems.Values)
                            {
                                string[] parts = numerology_system.Name.Split('_');
                                if (parts != null)
                                {
                                    if (parts.Length == 3)
                                    {
                                        if (parts[0].Contains("Dots")) continue;
                                        if (!TextModeComboBox.Items.Contains(parts[0]))
                                        {
                                            TextModeComboBox.Items.Add(parts[0]);
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
        private void TextModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                if (m_client != null)
                {
                    if (TextModeComboBox.SelectedItem != null)
                    {
                        m_text_mode = TextModeComboBox.SelectedItem.ToString();
                        m_client.BuildSimplifiedBook(m_text_mode, false, true, false, false, false, false, false, false, false);

                        PopulateNumerologySystemComboBox();
                        if (NumerologySystemComboBox.Items.Count > 0)
                        {
                            if (m_client.NumerologySystem != null)
                            {
                                int pos = m_client.NumerologySystem.Name.IndexOf("_");
                                string letter_valuation = m_client.NumerologySystem.Name.Substring(pos + 1);
                                if (NumerologySystemComboBox.Items.Contains(letter_valuation))
                                {
                                    NumerologySystemComboBox.SelectedItem = letter_valuation;
                                }
                                else
                                {
                                    NumerologySystemComboBox.SelectedIndex = 0;
                                }
                            }
                            else
                            {
                                NumerologySystemComboBox.SelectedIndex = -1;
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
        private void PopulateNumerologySystemComboBox()
        {
            try
            {
                NumerologySystemComboBox.SelectedIndexChanged -= new EventHandler(NumerologySystemComboBox_SelectedIndexChanged);

                if (m_client != null)
                {
                    if (m_client.LoadedNumerologySystems != null)
                    {
                        NumerologySystemComboBox.BeginUpdate();

                        if (TextModeComboBox.SelectedItem != null)
                        {
                            NumerologySystemComboBox.Items.Clear();
                            foreach (NumerologySystem numerology_system in m_client.LoadedNumerologySystems.Values)
                            {
                                string[] parts = numerology_system.Name.Split('_');
                                if (parts != null)
                                {
                                    if (parts.Length == 3)
                                    {
                                        if (parts[0] == m_text_mode)
                                        {
                                            string valuation_system = parts[1] + "_" + parts[2];
                                            if (!NumerologySystemComboBox.Items.Contains(valuation_system))
                                            {
                                                NumerologySystemComboBox.Items.Add(valuation_system);
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
                NumerologySystemComboBox.EndUpdate();
                NumerologySystemComboBox.SelectedIndexChanged += new EventHandler(NumerologySystemComboBox_SelectedIndexChanged);
            }
        }
        private void NumerologySystemComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_client != null)
            {
                if (TextModeComboBox.SelectedItem != null)
                {
                    if (NumerologySystemComboBox.SelectedItem != null)
                    {
                        string valuation_system = NumerologySystemComboBox.SelectedItem.ToString();
                        m_numerology_system_name = m_text_mode + "_" + valuation_system;
                        m_client.LoadNumerologySystem(m_numerology_system_name);
                        NumerologySystemComboBox_MouseHover(null, null);
                    }
                }
            }
        }
        private void NumerologySystemComboBox_MouseHover(object sender, EventArgs e)
        {
            if (m_client != null)
            {
                if (m_client.NumerologySystem != null)
                {
                    StringBuilder str = new StringBuilder();
                    foreach (char key in m_client.NumerologySystem.Keys)
                    {
                        str.AppendLine(key.ToString() + "\t" + m_client.NumerologySystem[key].ToString());
                    }
                    ToolTip.SetToolTip(NumerologySystemComboBox, str.ToString());
                }
            }
        }

        private List<int> m_special_chapter_numbers = null;
        private void LoadSpecialChapterNumbers()
        {
            m_special_chapter_numbers = new List<int>();
            string special_chapters_filename = "3dQuran" + Path.DirectorySeparatorChar +"chapters.txt";
            if (File.Exists(special_chapters_filename))
            {
                List<string> lines = FileHelper.LoadLines(special_chapters_filename);
                foreach (string line in lines)
                {
                    int special_chapter_number;
                    if (int.TryParse(line, out special_chapter_number))
                    {
                        m_special_chapter_numbers.Add(special_chapter_number);
                    }
                }
            }
        }
        private List<int> m_selected_chapter_numbers = null;
        private enum LinkBy { Text, Root, Value };
        private enum LinkTo { Nearest, Furthest, Widest, All };
        private LinkBy m_link_by = LinkBy.Text;
        private LinkTo m_link_to = LinkTo.Widest;
        private int m_min_range = 1;
        private bool m_build_network_chapters_1by1 = false;
        private int m_selected_chapter_index = -1;
        private void ChaptersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_selected_chapter_index = ChaptersComboBox.SelectedIndex;
            if (m_selected_chapter_index > -1)
            {
                if (m_selected_chapter_index == ChaptersComboBox.Items.Count - 1) // chapter by chapter each in its own filepath
                {
                    m_build_network_chapters_1by1 = true;
                    m_selected_chapter_numbers = new List<int>();
                    ToolTip.SetToolTip(this.ChaptersComboBox, "Build network chapter by chapter each in its own filepath");
                }
                else if (m_selected_chapter_index == CHAPTERS + 1) // few chapters
                {
                    m_build_network_chapters_1by1 = false;
                    m_selected_chapter_numbers = m_special_chapter_numbers;
                    ToolTip.SetToolTip(this.ChaptersComboBox, "Build network for special chapters");
                }
                else if (m_selected_chapter_index == 0)                                 // all chapters
                {
                    m_build_network_chapters_1by1 = false;
                    m_selected_chapter_numbers = new List<int>();
                    for (int i = 1; i <= CHAPTERS; i++)
                    {
                        m_selected_chapter_numbers.Add(i);
                    }
                    ToolTip.SetToolTip(this.ChaptersComboBox, "Build network for all chapters");
                }
                else if ((m_selected_chapter_index > 0) && (m_selected_chapter_index <= CHAPTERS))   // a single chapter
                {
                    m_build_network_chapters_1by1 = false;
                    m_selected_chapter_numbers = new List<int>() { m_selected_chapter_index };
                    ToolTip.SetToolTip(this.ChaptersComboBox, "Build network for chapter " + m_selected_chapter_index.ToString());
                }
            }
        }
        private void LinkByComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_link_by = (LinkBy)LinkByComboBox.SelectedIndex;
        }
        private void LinkToComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_link_to = (LinkTo)LinkToComboBox.SelectedIndex;
        }
        private void MinRangeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_min_range = MinRangeComboBox.SelectedIndex + 1;
        }

        private string m_ini_filename = null;
        private const int WINDOW_TOP = 0;
        private const int WINDOW_LEFT = 0;
        private const int WINDOW_WIDTH = 533;
        private const int WINDOW_HEIGHT = 609;
        private const int NODESLISTVIEW_COLUMN_0 = 40;
        private const int NODESLISTVIEW_COLUMN_1 = 36;
        private const int NODESLISTVIEW_COLUMN_2 = 36;
        private const int NODESLISTVIEW_COLUMN_3 = 36;
        private const int NODESLISTVIEW_COLUMN_4 = 73;
        private const int NODESLISTVIEW_COLUMN_5 = 144;
        private const int NODESLISTVIEW_COLUMN_6 = 57;
        private const int NODESLISTVIEW_COLUMN_7 = 66;
        private const int EDGESLISTVIEW_COLUMN_0 = 60;
        private const int EDGESLISTVIEW_COLUMN_1 = 80;
        private const int EDGESLISTVIEW_COLUMN_2 = 80;
        private const int EDGESLISTVIEW_COLUMN_3 = 148;
        private const int EDGESLISTVIEW_COLUMN_4 = 60;
        private const int EDGESLISTVIEW_COLUMN_5 = 60;
        private void LoadApplicationOptions()
        {
            try
            {
                if (AppDomain.CurrentDomain != null)
                {
                    m_ini_filename = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".ini");
                    if (File.Exists(m_ini_filename))
                    {
                        using (StreamReader reader = File.OpenText(m_ini_filename))
                        {
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                if (!String.IsNullOrEmpty(line))
                                {
                                    if (line.StartsWith("#")) continue;

                                    string[] parts = line.Split('=');
                                    if (parts.Length >= 2)
                                    {
                                        switch (parts[0])
                                        {
                                            // [Window]
                                            case "Top":
                                                {
                                                    try
                                                    {
                                                        this.Top = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        this.Top = 100;
                                                    }
                                                }
                                                break;
                                            case "Left":
                                                {
                                                    try
                                                    {
                                                        this.Left = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        this.Left = 100;
                                                    }
                                                }
                                                break;
                                            case "Width":
                                                {
                                                    try
                                                    {
                                                        this.Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        this.Width = WINDOW_WIDTH;
                                                    }
                                                }
                                                break;
                                            case "Height":
                                                {
                                                    try
                                                    {
                                                        this.Height = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        this.Height = WINDOW_HEIGHT;
                                                    }
                                                }
                                                break;
                                            // [ListViews]
                                            case "NodesListView_Column0":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[0].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[0].Width = NODESLISTVIEW_COLUMN_0;
                                                    }
                                                }
                                                break;
                                            case "NodesListView_Column1":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[1].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[1].Width = NODESLISTVIEW_COLUMN_1;
                                                    }
                                                }
                                                break;
                                            case "NodesListView_Column2":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[2].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[2].Width = NODESLISTVIEW_COLUMN_2;
                                                    }
                                                }
                                                break;
                                            case "NodesListView_Column3":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[3].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[3].Width = NODESLISTVIEW_COLUMN_3;
                                                    }
                                                }
                                                break;
                                            case "NodesListView_Column4":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[4].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[4].Width = NODESLISTVIEW_COLUMN_4;
                                                    }
                                                }
                                                break;
                                            case "NodesListView_Column5":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[5].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[5].Width = NODESLISTVIEW_COLUMN_5;
                                                    }
                                                }
                                                break;
                                            case "NodesListView_Column6":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[6].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[6].Width = NODESLISTVIEW_COLUMN_6;
                                                    }
                                                }
                                                break;
                                            case "NodesListView_Column7":
                                                {
                                                    try
                                                    {
                                                        NodesListView.Columns[7].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        NodesListView.Columns[7].Width = NODESLISTVIEW_COLUMN_7;
                                                    }
                                                }
                                                break;
                                            case "LinksListView_Column0":
                                                {
                                                    try
                                                    {
                                                        LinksListView.Columns[0].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        LinksListView.Columns[0].Width = EDGESLISTVIEW_COLUMN_0;
                                                    }
                                                }
                                                break;
                                            case "LinksListView_Column1":
                                                {
                                                    try
                                                    {
                                                        LinksListView.Columns[1].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        LinksListView.Columns[1].Width = EDGESLISTVIEW_COLUMN_1;
                                                    }
                                                }
                                                break;
                                            case "LinksListView_Column2":
                                                {
                                                    try
                                                    {
                                                        LinksListView.Columns[2].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        LinksListView.Columns[2].Width = EDGESLISTVIEW_COLUMN_2;
                                                    }
                                                }
                                                break;
                                            case "LinksListView_Column3":
                                                {
                                                    try
                                                    {
                                                        LinksListView.Columns[3].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        LinksListView.Columns[3].Width = EDGESLISTVIEW_COLUMN_3;
                                                    }
                                                }
                                                break;
                                            case "LinksListView_Column4":
                                                {
                                                    try
                                                    {
                                                        LinksListView.Columns[4].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        LinksListView.Columns[4].Width = EDGESLISTVIEW_COLUMN_4;
                                                    }
                                                }
                                                break;
                                            case "LinksListView_Column5":
                                                {
                                                    try
                                                    {
                                                        LinksListView.Columns[5].Width = int.Parse(parts[1].Trim());
                                                    }
                                                    catch
                                                    {
                                                        LinksListView.Columns[5].Width = EDGESLISTVIEW_COLUMN_5;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else // first launch
                {
                    this.Width = WINDOW_WIDTH;
                    this.Height = WINDOW_HEIGHT;
                    this.CenterToScreen();
                    this.Refresh();
                }
            }
            catch
            {
                // continue with next INI entry
            }
        }
        private void SaveApplicationOptions()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(m_ini_filename, false, Encoding.Unicode))
                {
                    if (this.WindowState == FormWindowState.Maximized)
                    {
                        this.WindowState = FormWindowState.Normal;
                    }

                    writer.WriteLine("[Window]");
                    writer.WriteLine("Top" + "=" + this.Top);
                    writer.WriteLine("Left" + "=" + this.Left);
                    writer.WriteLine("Width" + "=" + this.Width);
                    writer.WriteLine("Height" + "=" + this.Height);

                    writer.WriteLine("[ListViews]");
                    foreach (ColumnHeader column in NodesListView.Columns)
                    {
                        writer.WriteLine("NodesListView_Column" + column.Index.ToString() + "=" + column.Width);
                    }
                    foreach (ColumnHeader column in LinksListView.Columns)
                    {
                        writer.WriteLine("LinksListView_Column" + column.Index.ToString() + "=" + column.Width);
                    }

                    writer.WriteLine();
                }
            }
            catch
            {
                // silence IO errors in case running from read-only media (CD/DVD)
            }
        }

        private string m_index_filename = "index.html";
        private string m_save_filename = null;
        private List<Node> m_nodes = null;
        private void BuildNodes()
        {
            try
            {
                if (m_selected_chapter_numbers != null)
                {
                    m_nodes = new List<Node>();
                    if (m_nodes != null)
                    {
                        NetworkTextBox.Text = "";
                        NetworkTextBox.Refresh();

                        LinksTabPage.Text = "Links";
                        LinksTabPage.Refresh();
                        LinksListView.Items.Clear();
                        LinksListView.Refresh();

                        TabControl.SelectedTab = NodesTabPage;
                        NodesTabPage.Text = "Nodes ...";
                        NodesTabPage.Refresh();
                        NodesListView.Items.Clear();
                        NodesListView.Refresh();
                        TabControl.Refresh();

                        m_nodes.Clear();
                        if (m_client != null)
                        {
                            if (m_client.Book != null)
                            {
                                foreach (Chapter chapter in m_client.Book.Chapters)
                                {
                                    if (m_selected_chapter_numbers.Contains(chapter.SortedNumber))
                                    {
                                        foreach (Verse verse in chapter.Verses)
                                        {
                                            foreach (Word word in verse.Words)
                                            {
                                                Node node = new Node();

                                                node.Id = word.Number;
                                                node.X = chapter.SortedNumber;
                                                node.Y = verse.NumberInChapter;
                                                node.Z = word.NumberInVerse;
                                                node.Text = word.Text;
                                                node.SimplifiedText = word.Text.Simplify(m_text_mode);
                                                node.Meaning = word.Meaning;
                                                node.Root = word.Root;
                                                node.Value = word.Value;
                                                node.InPorts = 0;
                                                node.OutPorts = 0;

                                                m_nodes.Add(node);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace.ToString(), Application.ProductName);
            }
        }
        private void DisplayNodes()
        {
            if (m_nodes != null)
            {
                if (m_nodes.Count > 0)
                {
                    TabControl.SelectedTab = NodesTabPage;
                    NodesTabPage.Text = "Nodes [" + m_nodes.Count + "]";
                    NodesTabPage.Refresh();
                    NodesListView.Items.Clear();
                    NodesListView.Refresh();
                    TabControl.Refresh();

                    foreach (Node node in m_nodes)
                    {
                        string[] node_data = new string[]
                            { 
                                node.Id.ToString(),
                                node.X.ToString(),
                                node.Y.ToString(),
                                node.Z.ToString(),
                                node.Text,
                                node.Meaning,
                                node.Root,
                                node.Value.ToString()
                            };
                        NodesListView.Items.Add(new ListViewItem(node_data));
                    }
                    NodesListView.Refresh();
                }
            }
        }
        private void ConnectSimilarVerses()
        {
            if (m_client != null)
            {
                if (m_client.Book != null)
                {
                    List<Verse> verses = new List<Verse>();
                    foreach (Chapter chapter in m_client.Book.Chapters)
                    {
                        if (m_selected_chapter_numbers.Contains(chapter.SortedNumber))
                        {
                            verses.AddRange(chapter.Verses);
                        }
                    }

                    int count = 0;
                    for (int i = 0; i < verses.Count - 1; i++)
                    {
                        for (int j = i + 1; j < verses.Count; j++)
                        {
                            if (verses[i].Words.Count == verses[j].Words.Count)
                            {
                                bool similar = true;
                                for (int w = 0; w < verses[i].Words.Count; w++)
                                {
                                    if (m_link_by == LinkBy.Text)
                                    {
                                        if (verses[i].Words[w].Text.Simplify(m_text_mode) != verses[j].Words[w].Text.Simplify(m_text_mode))
                                        {
                                            similar = false;
                                            break; // w and try next j
                                        }
                                    }
                                    else if (m_link_by == LinkBy.Root)
                                    {
                                        if (verses[i].Words[w].Root != verses[j].Words[w].Root)
                                        {
                                            similar = false;
                                            break; // w and try next j
                                        }
                                    }
                                    else if (m_link_by == LinkBy.Value)
                                    {
                                        if (verses[i].Words[w].Value != verses[j].Words[w].Value)
                                        {
                                            similar = false;
                                            break; // w and try next j
                                        }
                                    }
                                }

                                if (similar)
                                {
                                    for (int w = 0; w < verses[i].Words.Count; w++)
                                    {
                                        Node node_i = GetNodeById(verses[i].Words[w].Number);
                                        Node node_j = GetNodeById(verses[j].Words[w].Number);
                                        if ((node_i.OutPorts == 0) && (node_j.InPorts == 0))
                                        {
                                            Link link = new Link();
                                            if (link != null)
                                            {
                                                link.Id = ++count;
                                                link.Source = node_i;
                                                link.Target = node_j;
                                                link.Label = node_i.Text + " = " + node_i.Value.ToString();
                                                link.Range = verses[i].Words.Count;
                                                link.NumberInRange = w + 1;
                                                link.VerseLink = true;
                                                link.Source.OutPorts++;
                                                link.Target.InPorts++;
                                                m_links.Add(link);
                                            }
                                            else
                                            {
                                                MessageBox.Show("Cannot create Link.\r\nOut of memory!", Application.ProductName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private Node GetNodeById(long id)
        {
            foreach (Node node in m_nodes)
            {
                if (node.Id == id) return node;
            }
            return null;
        }

        private bool m_include_similar_words = false;
        private void IncludeSimilarWordsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_include_similar_words = IncludeSimilarWordsCheckBox.Checked;
        }
        private List<Link> m_links = null;
        private void BuildLinks_Nearest()
        {
            m_links = new List<Link>();
            if (m_links != null)
            {
                ConnectSimilarVerses();

                if (m_include_similar_words)
                {
                    if (m_nodes != null)
                    {
                        switch (m_link_by)
                        {
                            case LinkBy.Text:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].SimplifiedText == m_nodes[j].SimplifiedText)
                                            {
                                                Link link = new Link();
                                                if (link != null)
                                                {
                                                    link.Id = m_links.Count + 1;
                                                    link.Source = m_nodes[i];
                                                    link.Target = m_nodes[j];
                                                    link.Label = m_nodes[j].Text;
                                                    link.VerseLink = false;
                                                    m_links.Add(link);
                                                }
                                                break;
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Root:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Root == m_nodes[j].Root)
                                            {
                                                Link link = new Link();
                                                if (link != null)
                                                {
                                                    link.Id = m_links.Count + 1;
                                                    link.Source = m_nodes[i];
                                                    link.Target = m_nodes[j];
                                                    link.Label = m_nodes[j].Text + " = " + m_nodes[j].Root;
                                                    link.VerseLink = false;
                                                    m_links.Add(link);
                                                }
                                                break;
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Value:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Value == m_nodes[j].Value)
                                            {
                                                Link link = new Link();
                                                if (link != null)
                                                {
                                                    link.Id = m_links.Count + 1;
                                                    link.Source = m_nodes[i];
                                                    link.Target = m_nodes[j];
                                                    link.Label = m_nodes[j].Text + " = " + m_nodes[j].Value.ToString();
                                                    link.VerseLink = false;
                                                    m_links.Add(link);
                                                }
                                                break;
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
        private void BuildLinks_Furthest()
        {
            m_links = new List<Link>();
            if (m_links != null)
            {
                ConnectSimilarVerses();

                if (m_include_similar_words)
                {
                    if (m_nodes != null)
                    {
                        switch (m_link_by)
                        {
                            case LinkBy.Text:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        Node last_j_node = null;
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].SimplifiedText == m_nodes[j].SimplifiedText)
                                            {
                                                last_j_node = m_nodes[j];
                                            }
                                        }
                                        if (last_j_node != null)
                                        {
                                            Link link = new Link();
                                            if (link != null)
                                            {
                                                link.Id = m_links.Count + 1;
                                                link.Source = m_nodes[i];
                                                link.Target = last_j_node;
                                                link.Label = last_j_node.Text;
                                                link.VerseLink = false;
                                                m_links.Add(link);
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Root:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        Node last_j_node = null;
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Root == m_nodes[j].Root)
                                            {
                                                last_j_node = m_nodes[j];
                                            }
                                        }
                                        if (last_j_node != null)
                                        {
                                            Link link = new Link();
                                            if (link != null)
                                            {
                                                link.Id = m_links.Count + 1;
                                                link.Source = m_nodes[i];
                                                link.Target = last_j_node;
                                                link.Label = last_j_node.Text + " = " + last_j_node.Root;
                                                link.VerseLink = false;
                                                m_links.Add(link);
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Value:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        Node last_j_node = null;
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Value == m_nodes[j].Value)
                                            {
                                                last_j_node = m_nodes[j];
                                            }
                                        }
                                        if (last_j_node != null)
                                        {
                                            Link link = new Link();
                                            if (link != null)
                                            {
                                                link.Id = m_links.Count + 1;
                                                link.Source = m_nodes[i];
                                                link.Target = last_j_node;
                                                link.Label = last_j_node.Text + " = " + last_j_node.Value.ToString();
                                                link.VerseLink = false;
                                                m_links.Add(link);
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
        private void BuildLinks_Widest()
        {
            m_links = new List<Link>();
            if (m_links != null)
            {
                ConnectSimilarVerses();

                if (m_include_similar_words)
                {
                    if (m_nodes != null)
                    {
                        switch (m_link_by)
                        {
                            case LinkBy.Text:
                                {
                                    int max_range = 0;
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].SimplifiedText == m_nodes[j].SimplifiedText)
                                            {
                                                int k = 0;
                                                while (m_nodes[i + k].SimplifiedText == m_nodes[j + k].SimplifiedText)
                                                {
                                                    k++;
                                                    if ((j + k) == m_nodes.Count) break;
                                                }

                                                if (max_range < k)
                                                {
                                                    max_range = k;
                                                }
                                            }
                                        }
                                        Application.DoEvents();
                                    }

                                    bool skip_range = false;
                                    for (int r = max_range; r >= m_min_range; r--)
                                    {
                                        for (int i = 0; i < m_nodes.Count - 1; i++)
                                        {
                                            for (int j = i + 1; j < m_nodes.Count; j++)
                                            {
                                                if (m_nodes[i].SimplifiedText == m_nodes[j].SimplifiedText)
                                                {
                                                    int first_i = i;
                                                    int first_j = j;

                                                    int k = 0;
                                                    do
                                                    {
                                                        if ((m_nodes[first_i + k].OutPorts > 0) || (m_nodes[first_j + k].InPorts > 0)) break;

                                                        k++;
                                                        if ((j + k) >= m_nodes.Count) break;

                                                    } while (m_nodes[i + k].SimplifiedText == m_nodes[j + k].SimplifiedText);

                                                    if (k == r)
                                                    {
                                                        for (int x = 0; x < r; x++)
                                                        {
                                                            if ((m_nodes[first_i + x].OutPorts > 0) || (m_nodes[first_j + x].InPorts > 0))
                                                            {
                                                                skip_range = true; // skip r completely
                                                                continue;
                                                            }

                                                            Link link = new Link();
                                                            if (link != null)
                                                            {
                                                                link.Id = m_links.Count + 1;
                                                                link.Source = m_nodes[first_i + x];
                                                                link.Target = m_nodes[first_j + x];
                                                                link.Label = m_nodes[first_i + x].Text + " = " + m_nodes[first_i + x].Value.ToString();
                                                                link.Range = r;
                                                                link.NumberInRange = x + 1;
                                                                link.VerseLink = false;
                                                                link.Source.OutPorts++;
                                                                link.Target.InPorts++;
                                                                m_links.Add(link);
                                                            }
                                                            else
                                                            {
                                                                MessageBox.Show("Cannot create Link.\r\nOut of memory!", Application.ProductName);
                                                            }
                                                        }

                                                        i += r - 1; // -1 for ; i++)
                                                        j += r - 1; // -1 for ; j++)
                                                    }
                                                }
                                                if (skip_range)
                                                {
                                                    continue; // skip r completely
                                                }
                                            }
                                            if (skip_range)
                                            {
                                                continue; // skip r completely
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Root:
                                {
                                    int max_range = 0;
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Root == m_nodes[j].Root)
                                            {
                                                int k = 0;
                                                while (m_nodes[i + k].Root == m_nodes[j + k].Root)
                                                {
                                                    k++;
                                                    if ((j + k) == m_nodes.Count) break;
                                                }

                                                if (max_range < k)
                                                {
                                                    max_range = k;
                                                }
                                            }
                                        }
                                        Application.DoEvents();
                                    }

                                    bool skip_range = false;
                                    for (int r = max_range; r >= m_min_range; r--)
                                    {
                                        for (int i = 0; i < m_nodes.Count - 1; i++)
                                        {
                                            for (int j = i + 1; j < m_nodes.Count; j++)
                                            {
                                                if (m_nodes[i].Root == m_nodes[j].Root)
                                                {
                                                    int first_i = i;
                                                    int first_j = j;

                                                    int k = 0;
                                                    do
                                                    {
                                                        if ((m_nodes[first_i + k].OutPorts > 0) || (m_nodes[first_j + k].InPorts > 0)) break;

                                                        k++;
                                                        if ((j + k) >= m_nodes.Count) break;

                                                    } while (m_nodes[i + k].Root == m_nodes[j + k].Root);

                                                    if (k == r)
                                                    {
                                                        for (int x = 0; x < r; x++)
                                                        {
                                                            if ((m_nodes[first_i + x].OutPorts > 0) || (m_nodes[first_j + x].InPorts > 0))
                                                            {
                                                                skip_range = true; // skip r completely
                                                                continue;
                                                            }

                                                            Link link = new Link();
                                                            if (link != null)
                                                            {
                                                                link.Id = m_links.Count + 1;
                                                                link.Source = m_nodes[first_i + x];
                                                                link.Target = m_nodes[first_j + x];
                                                                link.Label = m_nodes[first_i + x].Text + " and " + m_nodes[first_j + x].Text + ":" + " Root = " + m_nodes[first_i + x].Root;
                                                                link.Range = r;
                                                                link.NumberInRange = x + 1;
                                                                link.VerseLink = false;
                                                                link.Source.OutPorts++;
                                                                link.Target.InPorts++;
                                                                m_links.Add(link);
                                                            }
                                                            else
                                                            {
                                                                MessageBox.Show("Cannot create Link.\r\nOut of memory!", Application.ProductName);
                                                            }
                                                        }

                                                        i += r - 1; // -1 for ; i++)
                                                        j += r - 1; // -1 for ; j++)
                                                    }
                                                }
                                                if (skip_range)
                                                {
                                                    continue; // skip r completely
                                                }
                                            }
                                            if (skip_range)
                                            {
                                                continue; // skip r completely
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Value:
                                {
                                    int max_range = 0;
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Value == m_nodes[j].Value)
                                            {
                                                int k = 0;
                                                while (m_nodes[i + k].Value == m_nodes[j + k].Value)
                                                {
                                                    k++;
                                                    if ((j + k) == m_nodes.Count) break;
                                                }

                                                if (max_range < k)
                                                {
                                                    max_range = k;
                                                }
                                            }
                                        }
                                        Application.DoEvents();
                                    }

                                    bool skip_range = false;
                                    for (int r = max_range; r >= m_min_range; r--)
                                    {
                                        for (int i = 0; i < m_nodes.Count - 1; i++)
                                        {
                                            for (int j = i + 1; j < m_nodes.Count; j++)
                                            {
                                                if (m_nodes[i].Value == m_nodes[j].Value)
                                                {
                                                    int first_i = i;
                                                    int first_j = j;

                                                    int k = 0;
                                                    do
                                                    {
                                                        if ((m_nodes[first_i + k].OutPorts > 0) || (m_nodes[first_j + k].InPorts > 0)) break;

                                                        k++;
                                                        if ((j + k) >= m_nodes.Count) break;

                                                    } while (m_nodes[i + k].Value == m_nodes[j + k].Value);

                                                    if (k == r)
                                                    {
                                                        for (int x = 0; x < r; x++)
                                                        {
                                                            if ((m_nodes[first_i + x].OutPorts > 0) || (m_nodes[first_j + x].InPorts > 0))
                                                            {
                                                                skip_range = true; // skip r completely
                                                                continue;
                                                            }

                                                            Link link = new Link();
                                                            if (link != null)
                                                            {
                                                                link.Id = m_links.Count + 1;
                                                                link.Source = m_nodes[first_i + x];
                                                                link.Target = m_nodes[first_j + x];
                                                                link.Label = m_nodes[first_i + x].Text + " = " + m_nodes[first_j + x].Text + " = " + m_nodes[first_i + x].Value.ToString();
                                                                link.Range = r;
                                                                link.NumberInRange = x + 1;
                                                                link.VerseLink = false;
                                                                link.Source.OutPorts++;
                                                                link.Target.InPorts++;
                                                                m_links.Add(link);
                                                            }
                                                            else
                                                            {
                                                                MessageBox.Show("Cannot create Link.\r\nOut of memory!", Application.ProductName);
                                                            }
                                                        }

                                                        i += r - 1; // -1 for ; i++)
                                                        j += r - 1; // -1 for ; j++)
                                                    }
                                                }
                                                if (skip_range)
                                                {
                                                    continue; // skip r completely
                                                }
                                            }
                                            if (skip_range)
                                            {
                                                continue; // skip r completely
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
        private void BuildLinks_All()
        {
            m_links = new List<Link>();
            if (m_links != null)
            {
                ConnectSimilarVerses();

                if (m_include_similar_words)
                {
                    if (m_nodes != null)
                    {
                        switch (m_link_by)
                        {
                            case LinkBy.Text:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].SimplifiedText == m_nodes[j].SimplifiedText)
                                            {
                                                Link link = new Link();
                                                if (link != null)
                                                {
                                                    link.Id = m_links.Count + 1;
                                                    link.Source = m_nodes[i];
                                                    link.Target = m_nodes[j];
                                                    link.Label = m_nodes[j].Text;
                                                    link.VerseLink = false;
                                                    m_links.Add(link);
                                                }
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Root:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Root == m_nodes[j].Root)
                                            {
                                                Link link = new Link();
                                                if (link != null)
                                                {
                                                    link.Id = m_links.Count + 1;
                                                    link.Source = m_nodes[i];
                                                    link.Target = m_nodes[j];
                                                    link.Label = m_nodes[j].Text + " = " + m_nodes[j].Root;
                                                    link.VerseLink = false;
                                                    m_links.Add(link);
                                                }
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                            case LinkBy.Value:
                                {
                                    for (int i = 0; i < m_nodes.Count - 1; i++)
                                    {
                                        for (int j = i + 1; j < m_nodes.Count; j++)
                                        {
                                            if (m_nodes[i].Value == m_nodes[j].Value)
                                            {
                                                Link link = new Link();
                                                if (link != null)
                                                {
                                                    link.Id = m_links.Count + 1;
                                                    link.Source = m_nodes[i];
                                                    link.Target = m_nodes[j];
                                                    link.Label = m_nodes[j].Text + " = " + m_nodes[j].Value.ToString();
                                                    link.VerseLink = false;
                                                    m_links.Add(link);
                                                }
                                            }
                                        }
                                        Application.DoEvents();
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
        private void SaveLinks()
        {
            if (m_links != null)
            {
                StringBuilder str = new StringBuilder();
                if (m_links.Count > 0)
                {
                    str.AppendLine("Id" + "\t" + "Source" + "\t" + "Target" + "\t" + "Label" + "\t" + "Range" + "\t" + "#" + "\t" + "VerseLink");
                    foreach (Link link in m_links)
                    {
                        str.AppendLine(
                            link.Id.ToString() + "\t" +
                            link.Source.Id.ToString() + "\t" +
                            link.Target.Id.ToString() + "\t" +
                            link.Label + "\t" +
                            link.Range.ToString() + "\t" +
                            link.NumberInRange.ToString() + "\t" +
                            link.VerseLink.ToString()
                        );
                    }
                }

                if (Directory.Exists(DATA_FOLDER))
                {
                    string folder_name = DATA_FOLDER;
                    if (m_selected_chapter_index == 0)
                    {
                        folder_name += "/" + "all";                             // sub filepath: all chapters
                        Directory.CreateDirectory(folder_name);
                        SaveIndexFile(folder_name);
                    }
                    else if ((m_selected_chapter_index >= 1) && (m_selected_chapter_index <= CHAPTERS))
                    {
                        folder_name += "/" + m_selected_chapter_index.ToString();     // sub filepath: a single chapter
                        Directory.CreateDirectory(folder_name);
                        SaveIndexFile(folder_name);
                    }
                    else if (m_selected_chapter_index == CHAPTERS + 1)  // few chapters
                    {
                    }
                    else if (m_selected_chapter_index == CHAPTERS + 2)  // chapter by chapter each in its own filepath
                    {
                    }

                    if (m_selected_chapter_index == -1)
                    {
                        m_save_filename = folder_name + "/" + m_numerology_system_name + "_" + m_link_by.ToString() + "_" + m_link_to.ToString() + ".tsv";
                    }
                    else
                    {
                        m_save_filename = folder_name + "/" + "links" + ".tsv";
                    }

                    if (!String.IsNullOrEmpty(m_save_filename))
                    {
                        FileHelper.SaveText(m_save_filename, str.ToString(), Encoding.UTF8);
                    }
                    else
                    {
                        MessageBox.Show("Cannot save:\r\n" + m_save_filename, Application.ProductName);
                    }
                }
            }
        }
        private void SaveIndexFile(string folder_name)
        {
            if (File.Exists(DATA_FOLDER + "/" + m_index_filename))
            {
                if (Directory.Exists(folder_name))
                {
                    File.Copy(DATA_FOLDER + "/" + m_index_filename, folder_name + "/" + m_index_filename, true);
                }
            }
        }
        private void DisplayLinks()
        {
            if (m_links != null)
            {
                TabControl.SelectedTab = LinksTabPage;
                LinksTabPage.Text = "Links [" + m_links.Count + "]";
                LinksTabPage.Refresh();
                LinksListView.Items.Clear();
                LinksListView.Refresh();
                TabControl.Refresh();

                foreach (Link link in m_links)
                {
                    string[] link_data = new string[]
                        { 
                            link.Id.ToString(),
                            link.Source.Id.ToString(),
                            link.Target.Id.ToString(),
                            link.Label,
                            (link.Range > 0) ? link.Range.ToString() : "",
                            (link.Range > 0) ? link.NumberInRange.ToString() : "",
                            (link.Range > 0) ? link.VerseLink.ToString() : ""
                        };
                    LinksListView.Items.Add(new ListViewItem(link_data));
                }
                LinksListView.Refresh();
            }
        }

        private string m_network_text = null;
        private void BuildNetworkButton_Click(object sender, EventArgs e)
        {
            this.UseWaitCursor = true;
            Application.DoEvents();
            try
            {
                if (m_build_network_chapters_1by1)
                {
                    BuildNetworkForEachChapters();
                }
                else
                {
                    BuildNetwork();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace.ToString(), Application.ProductName);
            }
            finally
            {
                this.UseWaitCursor = false;
                Application.DoEvents();
            }
        }
        private void BuildNetwork()
        {
            BuildNodes();
            DisplayNodes();

            NetworkTextBox.Text = "";
            NetworkTextBox.Refresh();

            TabControl.SelectedTab = LinksTabPage;
            LinksTabPage.Text = "Links ...";
            LinksTabPage.Refresh();
            LinksListView.Items.Clear();
            LinksListView.Refresh();
            TabControl.Refresh();

            switch (m_link_to)
            {
                case LinkTo.Nearest:
                    {
                        BuildLinks_Nearest();
                    }
                    break;
                case LinkTo.Furthest:
                    {
                        BuildLinks_Furthest();
                    }
                    break;
                case LinkTo.Widest:
                    {
                        BuildLinks_Widest();
                    }
                    break;
                case LinkTo.All:
                    {
                        BuildLinks_All();
                    }
                    break;
                default:
                    {
                        BuildLinks_Nearest();
                    }
                    break;
            }

            if (m_selected_chapter_index != -1)
            {
                SaveLinks();
            }
            DisplayLinks();

            SaveNetwork();
            DisplayNetwork();
        }
        private void BuildNetworkForEachChapters()
        {
            for (int i = 1; i <= CHAPTERS; i++)
            {
                m_selected_chapter_index = i;
                m_selected_chapter_numbers = new List<int>() { m_selected_chapter_index };

                BuildNodes();
                DisplayNodes();

                NetworkTextBox.Text = "";
                NetworkTextBox.Refresh();

                TabControl.SelectedTab = LinksTabPage;
                LinksTabPage.Text = "Links ...";
                LinksTabPage.Refresh();
                LinksListView.Items.Clear();
                LinksListView.Refresh();
                TabControl.Refresh();

                switch (m_link_to)
                {
                    case LinkTo.Nearest:
                        {
                            BuildLinks_Nearest();
                        }
                        break;
                    case LinkTo.Furthest:
                        {
                            BuildLinks_Furthest();
                        }
                        break;
                    case LinkTo.Widest:
                        {
                            BuildLinks_Widest();
                        }
                        break;
                    case LinkTo.All:
                        {
                            BuildLinks_All();
                        }
                        break;
                    default:
                        {
                            BuildLinks_Nearest();
                        }
                        break;
                }

                SaveLinks();
                DisplayLinks();

                SaveNetwork();
                DisplayNetwork();
            }
        }
        private void DisplayNetwork()
        {
            TabControl.SelectedTab = NetworkTabPage;
            NetworkTextBox.Text = "";
            NetworkTextBox.Refresh();
            TabControl.Refresh();

            NetworkTextBox.Text = m_network_text.ToString();
            NetworkTextBox.Refresh();
        }
        private void SaveNetwork()
        {
            TabControl.SelectedTab = NetworkTabPage;
            NetworkTextBox.Text = "";
            NetworkTextBox.Refresh();
            TabControl.Refresh();

            // save network in .json format
            //{
            //  "nodes": [
            //    {"id": "Myriel", "group": 1},
            //    {"id": "Napoleon", "group": 1},
            //    {"id": "Mlle.Baptistine", "group": 1},
            //    {"id": "Mme.Hucheloup", "group": 8}
            //  ],
            //  "links": [
            //    {"source": "Napoleon", "target": "Myriel", "value": 1},
            //    {"source": "Mlle.Baptistine", "target": "Myriel", "value": 8},
            //    {"source": "Mme.Magloire", "target": "Myriel", "value": 10},
            //    {"source": "Mme.Hucheloup", "target": "Enjolras", "value": 1}
            //  ]
            //}
            StringBuilder str = new StringBuilder();
            if (m_nodes != null)
            {
                str.AppendLine("{");

                str.AppendLine("\t\"nodes\": [");
                if (m_nodes.Count > 0)
                {
                    // (m_selected_chapter_index == 0) // all chapters
                    foreach (Node node in m_nodes)
                    {
                        str.Append("\t\t{");
                        str.Append("\"id\":" + " " + node.Id.ToString() + ", ");
                        str.Append("\"fx\":" + " " + ((m_build_network_chapters_1by1 ? 0 : node.X) * ((m_selected_chapter_index == 0) ? 40 : 10) - 600).ToString() + ", ");
                        str.Append("\"fy\":" + " " + (node.Y * ((m_selected_chapter_index == 0) ? 40 : 10) - 400).ToString() + ", ");
                        str.Append("\"fz\":" + " " + (node.Z * ((m_selected_chapter_index == 0) ? 40 : 10) + 800).ToString() + ", ");
                        str.Append("\"l\":" + " \"" + node.Text + "\", ");
                        str.Append("\"r\":" + " \"" + node.Root + "\", ");
                        str.Append("\"v\":" + " " + node.Value + "");
                        str.Append("},");
                        str.AppendLine();
                    }
                    str.Remove(str.Length - 3, 1); // last ","
                }
                str.AppendLine("\t],");
            }

            if (m_links != null)
            {

                str.AppendLine("\t\"links\": [");
                if (m_links.Count > 0)
                {
                    foreach (Link link in m_links)
                    {
                        str.Append("\t\t{");
                        str.Append("\"s\":" + " " + link.Source.Id.ToString() + ", ");
                        str.Append("\"t\":" + " " + link.Target.Id.ToString() + ", ");
                        str.Append("\"w\":" + " " + link.Range + ", ");
                        str.Append("\"n\":" + " " + link.NumberInRange + ", ");
                        str.Append("\"o\":" + " " + (link.VerseLink ? 2.0 : 0.2) + "");
                        str.Append("},");
                        str.AppendLine();
                    }
                    str.Remove(str.Length - 3, 1); // last ","
                }
                str.AppendLine("\t]");

                str.AppendLine("}");
            }

            if (str.Length > 0)
            {
                if (Directory.Exists(DATA_FOLDER))
                {
                    if (!String.IsNullOrEmpty(m_save_filename))
                    {
                        string json_filename = null;
                        if (m_save_filename.Contains("links"))
                        {
                            json_filename = m_save_filename.Replace("links.tsv", "network.json");
                        }
                        else
                        {
                            json_filename = m_save_filename.Replace(".tsv", ".json");
                        }

                        if (!String.IsNullOrEmpty(json_filename))
                        {
                            FileHelper.SaveText(json_filename, str.ToString(), Encoding.UTF8);
                        }
                        else
                        {
                            MessageBox.Show("Cannot save:\r\n" + json_filename, Application.ProductName);
                        }
                    }
                }
            }

            m_network_text = str.ToString();
        }
        private void OpenNetworkFolderButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Directory.Exists(DATA_FOLDER))
                {
                    System.Diagnostics.Process.Start(DATA_FOLDER);
                }
            }
            catch
            {
                // ignore errors
            }
        }

        private void NodesListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (m_nodes != null)
            {
                SortNodes((NodeCompareBy)e.Column);
                DisplayNodes();

                // choose marker type
                string sort_marker = (Node.CompareOrder == NodeCompareOrder.Ascending) ? ASC : DESC;

                // clear markers
                foreach (ColumnHeader column in NodesListView.Columns)
                {
                    column.Text = column.Text.Replace(ASC, "");
                    column.Text = column.Text.Replace(DESC, "");
                }

                // display marker
                NodesListView.Columns[e.Column].Text = NodesListView.Columns[e.Column].Text + sort_marker;
            }
        }
        private void NodesListView_DoubleClick(object sender, EventArgs e)
        {

        }
        private void LinksListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (m_links != null)
            {
                SortLinks((LinkCompareBy)e.Column);

                if (m_selected_chapter_index != -1)
                {
                    SaveLinks();
                }
                DisplayLinks();

                // choose marker type
                string sort_marker = (Link.CompareOrder == LinkCompareOrder.Ascending) ? ASC : DESC;

                // clear markers
                foreach (ColumnHeader column in LinksListView.Columns)
                {
                    column.Text = column.Text.Replace(ASC, "");
                    column.Text = column.Text.Replace(DESC, "");
                }

                // display marker
                LinksListView.Columns[e.Column].Text = LinksListView.Columns[e.Column].Text + sort_marker;
            }
        }
        private void LinksListView_DoubleClick(object sender, EventArgs e)
        {

        }
        public void SortNodes(NodeCompareBy compare_by)
        {
            if (Node.CompareOrder == NodeCompareOrder.Ascending)
            {
                Node.CompareOrder = NodeCompareOrder.Descending;
            }
            else
            {
                Node.CompareOrder = NodeCompareOrder.Ascending;
            }

            Node.CompareBy = compare_by;
            m_nodes.Sort();
        }
        public void SortLinks(LinkCompareBy compare_by)
        {
            if (Link.CompareOrder == LinkCompareOrder.Ascending)
            {
                Link.CompareOrder = LinkCompareOrder.Descending;
            }
            else
            {
                Link.CompareOrder = LinkCompareOrder.Ascending;
            }

            //if ((compare_by == LinkCompareBy.Range) && (compare_by == LinkCompareBy.NumberInRange))
            if (compare_by == LinkCompareBy.Range)
            {
                // use IComparer for multi-column sort
                m_links.Sort(new LinkRangeComparer());
            }
            else
            {
                // use IComparable for single-column sort
                Link.CompareBy = compare_by;
                m_links.Sort();
            }
        }
    }
}
