using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.IO;
using Model;

public partial class MainForm : Form
{
    // Use Simplified29 ONLY because Surat Al-Fatiha is built upon pattern 7-29
    private string m_numerical_system_name = "Original_Alphabet_Primes1";

    private Client m_client = null;
    private List<List<Word>> m_word_subsets = null;
    private List<Line> m_lines = null;

    public MainForm()
    {
        InitializeComponent();

        using (Graphics graphics = this.CreateGraphics())
        {
            // 100% = 96.0F,   125% = 120.0F,   150% = 144.0F
            if (graphics.DpiX == 96.0F)
            {
                this.NumberColumnHeader.Width = 55;
                this.SentenceColumnHeader.Width = 385;
                this.ValueColumnHeader.Width = 94;
                this.WordColumnHeader.Width = 110;
                this.AutoGenerateWordsButton.Size = new System.Drawing.Size(25, 23);
            }
            else if (graphics.DpiX == 120.0F)
            {
                this.NumberColumnHeader.Width = 70;
                this.SentenceColumnHeader.Width = 510;
                this.ValueColumnHeader.Width = 114;
                this.WordColumnHeader.Width = 165;
                this.AutoGenerateWordsButton.Size = new System.Drawing.Size(27, 25);
            }
            else if (graphics.DpiX == 144.0F)
            {
                //this.NumberColumnHeader.Width = 70;
                //this.SentenceColumnHeader.Width = 510;
                //this.ValueColumnHeader.Width = 114;
                //this.WordColumnHeader.Width = 165;
                //this.AutoGenerateWordsButton.Size = new System.Drawing.Size(27, 25);
            }
        }
    }
    private void MainForm_Load(object sender, EventArgs e)
    {
        if (m_client == null)
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

        m_lines = new List<Line>();
        m_generated_words = new SortedDictionary<string, int>();

        m_number_type = NumberType.Prime;
        NumberTypeLabel.Text = "P";
        NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(73L);
        ToolTip.SetToolTip(this.ValueInterlaceLabel, "concatenate letter values");
        ToolTip.SetToolTip(this.ValueCombinationDirectionLabel, "combine letter values right to left");
        ToolTip.SetToolTip(this.NumberTypeLabel, "allow prime combined letter values only");
    }
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Environment.Exit(0); // close Console and WinForms applications immediately without errors
    }
    private void PopulateTextModeComboBox()
    {
        try
        {
            TextModeComboBox.SelectedIndexChanged -= new EventHandler(TextModeComboBox_SelectedIndexChanged);

            if (m_client != null)
            {
                if (m_client.NumericalSystem != null)
                {
                    string default_text_mode = m_client.NumericalSystem.TextMode;

                    if (m_client.LoadedNumericalSystems != null)
                    {
                        TextModeComboBox.BeginUpdate();

                        TextModeComboBox.Items.Clear();
                        foreach (NumericalSystem ns in m_client.LoadedNumericalSystems.Values)
                        {
                            if (!ns.Name.StartsWith(default_text_mode)) continue;

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
            UpdateNumericalSystem();
            NumericalSystemComboBox_MouseHover(null, null);
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
    private void UpdateNumericalSystem()
    {
        if (m_client != null)
        {
            if (m_client.NumericalSystem != null)
            {
                m_client.NumericalSystem.AddToLetterLNumber = m_add_positions_to_letter_value;
                m_client.NumericalSystem.AddToLetterWNumber = m_add_positions_to_letter_value;
                m_client.NumericalSystem.AddToLetterVNumber = m_add_positions_to_letter_value;
                m_client.NumericalSystem.AddToLetterCNumber = false;
                m_client.NumericalSystem.AddToLetterLDistance = true;
                m_client.NumericalSystem.AddToLetterWDistance = true;
                m_client.NumericalSystem.AddToLetterVDistance = true;
                m_client.NumericalSystem.AddToLetterCDistance = false;
                m_client.NumericalSystem.AddToWordWNumber = m_add_positions_to_letter_value;
                m_client.NumericalSystem.AddToWordVNumber = m_add_positions_to_letter_value;
                m_client.NumericalSystem.AddToWordCNumber = false;
                m_client.NumericalSystem.AddToWordWDistance = true;
                m_client.NumericalSystem.AddToWordVDistance = true;
                m_client.NumericalSystem.AddToWordCDistance = false;
                m_client.NumericalSystem.AddToVerseVNumber = m_add_positions_to_letter_value;
                m_client.NumericalSystem.AddToVerseCNumber = false;
                m_client.NumericalSystem.AddToVerseVDistance = true;
                m_client.NumericalSystem.AddToVerseCDistance = false;
                m_client.NumericalSystem.AddToChapterCNumber = false;

                m_client.NumericalSystem.AddPositions = m_add_positions_to_letter_value;
                m_client.NumericalSystem.AddDistancesToPrevious = m_add_distances_to_previous_to_letter_value;
                m_client.NumericalSystem.AddDistancesToNext = m_add_distances_to_next_to_letter_value;
                m_client.NumericalSystem.AddDistancesWithinChapters = true;
                if (m_client.Book != null)
                {
                    m_client.Book.SetupDistances(m_client.NumericalSystem.AddDistancesWithinChapters, false);
                }
            }
        }
    }
    private bool m_add_verse_and_word_values_to_letter_value = false;
    private bool m_add_positions_to_letter_value = false;
    private bool m_add_distances_to_previous_to_letter_value = false;
    private bool m_add_distances_to_next_to_letter_value = false;
    private void AddVerseAndWordValuesCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_add_verse_and_word_values_to_letter_value = AddVerseAndWordValuesCheckBox.Checked;
    }
    private void AddPositionsCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_add_positions_to_letter_value = AddPositionsCheckBox.Checked;
        UpdateNumericalSystem();
    }
    private void AddDistancesToPreviousCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_add_distances_to_previous_to_letter_value = AddDistancesToPreviousCheckBox.Checked;
        UpdateNumericalSystem();
    }
    private void AddDistancesToNextCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_add_distances_to_next_to_letter_value = AddDistancesToNextCheckBox.Checked;
        UpdateNumericalSystem();
    }

    private enum CombinationMethod { Concatenate, InterlaceAB, InterlaceBA, CrossOverAB, CrossOverBA };
    private CombinationMethod m_combination_method = CombinationMethod.Concatenate;
    private bool m_left_to_right = false;
    private NumberType m_number_type = NumberType.Prime;
    private void ValueInterlaceLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Shift)
        {
            GoToPreviousValueCombinationType();
        }
        else
        {
            GoToNextValueCombinationType();
        }
    }
    private void GoToPreviousValueCombinationType()
    {
        switch (m_combination_method)
        {
            case CombinationMethod.CrossOverBA:
                {
                    m_combination_method = CombinationMethod.CrossOverAB;
                    ValueInterlaceLabel.Text = "aXb";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "corssover digits of letter values, aabbaaabb");
                }
                break;
            case CombinationMethod.CrossOverAB:
                {
                    m_combination_method = CombinationMethod.InterlaceBA;
                    ValueInterlaceLabel.Text = "b§a";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "interlace digits of letter values, babababaa");
                }
                break;
            case CombinationMethod.InterlaceBA:
                {
                    m_combination_method = CombinationMethod.InterlaceAB;
                    ValueInterlaceLabel.Text = "a§b";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "interlace digits of letter values, ababababa");
                }
                break;
            case CombinationMethod.InterlaceAB:
                {
                    m_combination_method = CombinationMethod.Concatenate;
                    ValueInterlaceLabel.Text = "- -";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "concatenate letter values, aaaaabbbb");
                }
                break;
            case CombinationMethod.Concatenate:
                {
                    m_combination_method = CombinationMethod.CrossOverBA;
                    ValueInterlaceLabel.Text = "bXa";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "crossover digits of letter values, bbaabbaaa");
                }
                break;
        }
        ValueInterlaceLabel.Refresh();
    }
    private void GoToNextValueCombinationType()
    {
        switch (m_combination_method)
        {
            case CombinationMethod.Concatenate:
                {
                    m_combination_method = CombinationMethod.InterlaceAB;
                    ValueInterlaceLabel.Text = "a§b";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "interlace digits of letter values, ababababa");
                }
                break;
            case CombinationMethod.InterlaceAB:
                {
                    m_combination_method = CombinationMethod.InterlaceBA;
                    ValueInterlaceLabel.Text = "b§a";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "interlace digits of letter values, babababaa");
                }
                break;
            case CombinationMethod.InterlaceBA:
                {
                    m_combination_method = CombinationMethod.CrossOverAB;
                    ValueInterlaceLabel.Text = "aXb";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "corssover digits of letter values, aabbaaabb");
                }
                break;
            case CombinationMethod.CrossOverAB:
                {
                    m_combination_method = CombinationMethod.CrossOverBA;
                    ValueInterlaceLabel.Text = "bXa";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "crossover digits of letter values, bbaabbaaa");
                }
                break;
            case CombinationMethod.CrossOverBA:
                {
                    m_combination_method = CombinationMethod.Concatenate;
                    ValueInterlaceLabel.Text = "- -";
                    ToolTip.SetToolTip(this.ValueInterlaceLabel, "concatenate letter values, aaaaabbbb");
                }
                break;
        }
        ValueInterlaceLabel.Refresh();
    }
    private void ValueCombinationDirectionLabel_Click(object sender, EventArgs e)
    {
        m_left_to_right = !m_left_to_right;
        if (m_left_to_right)
        {
            ValueCombinationDirectionLabel.Text = "→";
            ToolTip.SetToolTip(this.ValueCombinationDirectionLabel, "combine letter values left to right");
        }
        else // right_to_left
        {
            ValueCombinationDirectionLabel.Text = "←";
            ToolTip.SetToolTip(this.ValueCombinationDirectionLabel, "combine letter values right to left");
        }
        ValueCombinationDirectionLabel.Refresh();
    }
    private void NumberTypeLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Shift)
        {
            GotoPreviousNumberType();
        }
        else
        {
            GotoNextNumberType();
        }
    }
    private void GotoPreviousNumberType()
    {
        switch (m_number_type)
        {
            case NumberType.NonAdditiveComposite:
                {
                    m_number_type = NumberType.AdditiveComposite;
                    NumberTypeLabel.Text = "AC";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(114L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow additive composite combined letter values only");
                }
                break;
            case NumberType.AdditiveComposite:
                {
                    m_number_type = NumberType.Composite;
                    NumberTypeLabel.Text = "C";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(14L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow composite combined letter values only");
                }
                break;
            case NumberType.Composite:
                {
                    m_number_type = NumberType.NonAdditivePrime;
                    NumberTypeLabel.Text = "XP";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(73L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow non-additive prime combined letter values only");
                }
                break;
            case NumberType.NonAdditivePrime:
                {
                    m_number_type = NumberType.AdditivePrime;
                    NumberTypeLabel.Text = "AP";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(47L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow additive prime combined letter values only");
                }
                break;
            case NumberType.AdditivePrime:
                {
                    m_number_type = NumberType.Prime;
                    NumberTypeLabel.Text = "P";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(73L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow prime combined letter values only");
                }
                break;
            case NumberType.Prime:
                {
                    m_number_type = NumberType.Natural;
                    NumberTypeLabel.Text = "";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(0L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow all combined letter values only");
                }
                break;
            case NumberType.Natural:
                {
                    m_number_type = NumberType.NonAdditiveComposite;
                    NumberTypeLabel.Text = "XC";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(12L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow non-additive composite combined letter values only");
                }
                break;
        }
        NumberTypeLabel.Refresh();
    }
    private void GotoNextNumberType()
    {
        switch (m_number_type)
        {
            case NumberType.Natural:
                {
                    m_number_type = NumberType.Prime;
                    NumberTypeLabel.Text = "P";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(73L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow prime combined letter values only");
                }
                break;
            case NumberType.Prime:
                {
                    m_number_type = NumberType.AdditivePrime;
                    NumberTypeLabel.Text = "AP";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(47L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow additive prime combined letter values only");
                }
                break;
            case NumberType.AdditivePrime:
                {
                    m_number_type = NumberType.NonAdditivePrime;
                    NumberTypeLabel.Text = "XP";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(73L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow non-additive prime combined letter values only");
                }
                break;
            case NumberType.NonAdditivePrime:
                {
                    m_number_type = NumberType.Composite;
                    NumberTypeLabel.Text = "C";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(14L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow composite combined letter values only");
                }
                break;
            case NumberType.Composite:
                {
                    m_number_type = NumberType.AdditiveComposite;
                    NumberTypeLabel.Text = "AC";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(114L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow additive composite combined letter values only");
                }
                break;
            case NumberType.AdditiveComposite:
                {
                    m_number_type = NumberType.NonAdditiveComposite;
                    NumberTypeLabel.Text = "XC";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(25L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow non-additive composite combined letter values only");
                }
                break;
            case NumberType.NonAdditiveComposite:
                {
                    m_number_type = NumberType.Natural;
                    NumberTypeLabel.Text = "";
                    NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(0L);
                    ToolTip.SetToolTip(this.NumberTypeLabel, "allow all combined letter values only");
                }
                break;
        }
        NumberTypeLabel.Refresh();
    }

    public enum LineCompareBy { Number, Sentence, Value, Word }
    public enum LineCompareOrder { Ascending, Descending }
    public class Line : IComparable<Line>
    {
        public int Number;
        public string Sentence;
        public long Value;
        public string Word;

        public static LineCompareBy CompareBy;
        public static LineCompareOrder CompareOrder;
        public int CompareTo(Line obj)
        {
            if (CompareOrder == LineCompareOrder.Ascending)
            {
                if (CompareBy == LineCompareBy.Number)
                {
                    return this.Number.CompareTo(obj.Number);
                }
                else if (CompareBy == LineCompareBy.Sentence)
                {
                    return this.Sentence.CompareTo(obj.Sentence);
                }
                else if (CompareBy == LineCompareBy.Value)
                {
                    return this.Value.CompareTo(obj.Value);
                }
                else if (CompareBy == LineCompareBy.Word)
                {
                    return this.Word.CompareTo(obj.Word);
                }
                else
                {
                    return this.Number.CompareTo(obj.Number);
                }
            }
            else
            {
                if (CompareBy == LineCompareBy.Number)
                {
                    return obj.Number.CompareTo(this.Number);
                }
                else if (CompareBy == LineCompareBy.Sentence)
                {
                    return obj.Sentence.CompareTo(this.Sentence);
                }
                else if (CompareBy == LineCompareBy.Value)
                {
                    return obj.Value.CompareTo(this.Value);
                }
                else if (CompareBy == LineCompareBy.Word)
                {
                    return obj.Word.CompareTo(this.Word);
                }
                else
                {
                    return obj.Number.CompareTo(this.Number);
                }
            }
        }
    }
    private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        if (ListView != null)
        {
            if (ListView.Columns != null)
            {
                // compare method
                Line.CompareBy = (LineCompareBy)e.Column;

                // compare order
                if (Line.CompareOrder == LineCompareOrder.Ascending)
                {
                    Line.CompareOrder = LineCompareOrder.Descending;
                }
                else
                {
                    Line.CompareOrder = LineCompareOrder.Ascending;
                }

                // order marker
                string sort_marker = (Line.CompareOrder == LineCompareOrder.Ascending) ? "▲" : "▼";
                foreach (ColumnHeader column in ListView.Columns)
                {
                    column.Text = column.Text.Replace("▲", " ");
                    column.Text = column.Text.Replace("▼", " ");
                }
                ListView.Columns[e.Column].Text = ListView.Columns[e.Column].Text.Replace("  ", " " + sort_marker);
                ListView.Refresh();

                // sort items
                m_lines.Sort();

                // display items
                UpdateListView();
            }
        }
    }
    private void ClearListView()
    {
        if (ListView != null)
        {
            if (ListView.Items != null)
            {
                ListView.Items.Clear();
                Line.CompareBy = (LineCompareBy)0;
                Line.CompareOrder = LineCompareOrder.Ascending;
                foreach (ColumnHeader column in ListView.Columns)
                {
                    column.Text = column.Text.Replace("▲", " ");
                    column.Text = column.Text.Replace("▼", " ");
                }
                ListView.Columns[0].Text = ListView.Columns[0].Text = "# ▲";
                ListView.Refresh();
            }
        }
    }
    private void UpdateListView()
    {
        if (ListView != null)
        {
            if (ListView.Items != null)
            {
                ListView.Items.Clear();
                for (int i = 0; i < m_lines.Count; i++)
                {
                    string[] parts = new string[4];
                    parts[0] = m_lines[i].Number.ToString();
                    parts[1] = m_lines[i].Sentence.ToString();
                    parts[2] = m_lines[i].Value.ToString();
                    parts[3] = m_lines[i].Word;
                    ListView.Items.Add(new ListViewItem(parts, i));
                }
                ListView.Refresh();
            }
        }
    }

    private SortedDictionary<string, int> m_generated_words = null;
    private void GenerateWordsButton_Click(object sender, EventArgs e)
    {
        TextModeComboBox.Enabled = false;
        NumericalSystemComboBox.Enabled = false;
        AddVerseAndWordValuesCheckBox.Enabled = false;
        AddPositionsCheckBox.Enabled = false;
        AddDistancesToPreviousCheckBox.Enabled = false;
        AddDistancesToNextCheckBox.Enabled = false;
        ValueCombinationDirectionLabel.Enabled = false;
        NumberTypeLabel.Enabled = false;
        AutoGenerateWordsButton.Enabled = false;
        GenerateWordsButton.Enabled = false;
        InspectButton.Enabled = false;
        TextModeComboBox.Refresh();
        NumericalSystemComboBox.Refresh();
        AddVerseAndWordValuesCheckBox.Refresh();
        AddPositionsCheckBox.Refresh();
        AddDistancesToPreviousCheckBox.Refresh();
        AddDistancesToNextCheckBox.Refresh();
        ValueCombinationDirectionLabel.Refresh();
        NumberTypeLabel.Refresh();
        AutoGenerateWordsButton.Refresh();
        GenerateWordsButton.Refresh();
        InspectButton.Refresh();

        this.Cursor = Cursors.WaitCursor;
        try
        {
            ClearListView();

            if (m_client != null)
            {
                if (m_client.Book != null)
                {
                    if (m_client.Book.Chapters[0] != null)
                    {
                        List<Verse> fatiha_verses = m_client.Book.Chapters[0].Verses;
                        if (fatiha_verses != null)
                        {
                            List<Word> fatiha_words = new List<Word>();
                            foreach (Verse fatiha_verse in fatiha_verses)
                            {
                                fatiha_words.AddRange(fatiha_verse.Words);
                            }


                            // find all 7-word 29-letter word subsets
                            WordSubsetFinder word_subset_finder = new WordSubsetFinder(fatiha_words);
                            m_word_subsets = word_subset_finder.Find(fatiha_verses.Count, fatiha_words.Count);
                            if (m_word_subsets != null)
                            {
                                if (ModifierKeys == Keys.Shift)
                                {
                                    foreach (string numerical_system_name in NumericalSystemComboBox.Items)
                                    {
                                        NumericalSystemComboBox.SelectedItem = numerical_system_name;
                                        NumericalSystemComboBox.Refresh();

                                        DoGenerateWords(false);
                                    } // foreach NumericalSystem
                                }
                                else
                                {
                                    DoGenerateWords(true);
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            TextModeComboBox.Enabled = true;
            NumericalSystemComboBox.Enabled = true;
            AddVerseAndWordValuesCheckBox.Enabled = true;
            AddPositionsCheckBox.Enabled = true;
            AddDistancesToPreviousCheckBox.Enabled = true;
            AddDistancesToNextCheckBox.Enabled = true;
            ValueCombinationDirectionLabel.Enabled = true;
            NumberTypeLabel.Enabled = true;
            AutoGenerateWordsButton.Enabled = true;
            GenerateWordsButton.Enabled = true;
            InspectButton.Enabled = true;

            this.Cursor = Cursors.Default;
        }
    }
    private void AutoGenerateWordsButton_Click(object sender, EventArgs e)
    {
        TextModeComboBox.Enabled = false;
        NumericalSystemComboBox.Enabled = false;
        AddVerseAndWordValuesCheckBox.Enabled = false;
        AddPositionsCheckBox.Enabled = false;
        AddDistancesToPreviousCheckBox.Enabled = false;
        AddDistancesToNextCheckBox.Enabled = false;
        ValueCombinationDirectionLabel.Enabled = false;
        NumberTypeLabel.Enabled = false;
        AutoGenerateWordsButton.Enabled = false;
        GenerateWordsButton.Enabled = false;
        InspectButton.Enabled = false;
        TextModeComboBox.Refresh();
        NumericalSystemComboBox.Refresh();
        AddVerseAndWordValuesCheckBox.Refresh();
        AddPositionsCheckBox.Refresh();
        AddDistancesToPreviousCheckBox.Refresh();
        AddDistancesToNextCheckBox.Refresh();
        ValueCombinationDirectionLabel.Refresh();
        NumberTypeLabel.Refresh();
        AutoGenerateWordsButton.Refresh();
        GenerateWordsButton.Refresh();
        InspectButton.Refresh();

        this.Cursor = Cursors.WaitCursor;
        try
        {
            ClearListView();

            if (m_client != null)
            {
                if (m_client.Book != null)
                {
                    if (m_client.Book.Chapters[0] != null)
                    {
                        List<Verse> fatiha_verses = m_client.Book.Chapters[0].Verses;
                        if (fatiha_verses != null)
                        {
                            // setup all quran words from quran verses
                            List<Word> fatiha_words = new List<Word>();
                            foreach (Verse fatiha_verse in fatiha_verses)
                            {
                                fatiha_words.AddRange(fatiha_verse.Words);
                            }

                            // find all 7-word 29-letter word subsets
                            WordSubsetFinder word_subset_finder = new WordSubsetFinder(fatiha_words);
                            m_word_subsets = word_subset_finder.Find(fatiha_verses.Count, fatiha_words.Count);
                            if (m_word_subsets != null)
                            {
                                if (ModifierKeys == Keys.Shift)
                                {
                                    foreach (string numerical_system_name in NumericalSystemComboBox.Items)
                                    {
                                        NumericalSystemComboBox.SelectedItem = numerical_system_name;
                                        NumericalSystemComboBox.Refresh();

                                        ProcessNumericalSystem();
                                    } // foreach NumericalSystem
                                }
                                else
                                {
                                    ProcessNumericalSystem();
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            TextModeComboBox.Enabled = true;
            NumericalSystemComboBox.Enabled = true;
            AddVerseAndWordValuesCheckBox.Enabled = true;
            AddPositionsCheckBox.Enabled = true;
            AddDistancesToPreviousCheckBox.Enabled = true;
            AddDistancesToNextCheckBox.Enabled = true;
            ValueCombinationDirectionLabel.Enabled = true;
            NumberTypeLabel.Enabled = true;
            AutoGenerateWordsButton.Enabled = true;
            GenerateWordsButton.Enabled = true;
            InspectButton.Enabled = true;

            this.Cursor = Cursors.Default;
        }
    }
    private void ProcessNumericalSystem()
    {
        if (m_generated_words != null)
        {
            if (m_client != null)
            {
                if (m_client.NumericalSystem != null)
                {
                    // prepare for next state
                    m_combination_method = CombinationMethod.CrossOverBA;
                    m_left_to_right = true;
                    m_number_type = NumberType.NonAdditiveComposite;

                    for (int h = 0; h < 2; h++)
                    {
                        AddVerseAndWordValuesCheckBox.Checked = (h == 1);
                        for (int k = 0; k < 2; k++)
                        {
                            AddPositionsCheckBox.Checked = (k == 1);
                            for (int l = 0; l < 2; l++)
                            {
                                AddDistancesToPreviousCheckBox.Checked = (l == 1);
                                for (int m = 0; m < 2; m++)
                                {
                                    AddDistancesToNextCheckBox.Checked = (m == 1);
                                    for (int n = 0; n < 5; n++)
                                    {
                                        ValueInterlaceLabel_Click(null, null);
                                        for (int o = 0; o < 2; o++)
                                        {
                                            ValueCombinationDirectionLabel_Click(null, null);
                                            for (int p = 0; p < 6; p++)
                                            {
                                                GotoNextNumberType();
                                                // skip Natural type
                                                if (m_number_type == NumberType.Natural) // skip natural type
                                                {
                                                    GotoNextNumberType();
                                                }

                                                DoGenerateWords(false);

                                            } // for NumberType
                                        } // for Direction
                                    } // for Combination
                                } // for AddDistancesToNext
                            } // for AddDistancesToPrevious
                        } // for AddPositions
                    } // for AddVerseAndWordValues
                }
            }
        }
    }
    private void DoGenerateWords(bool display_progress)
    {
        if (m_lines != null)
        {
            m_lines.Clear();
            if (m_generated_words != null)
            {
                m_generated_words.Clear();

                // get unique quran words
                List<string> quran_word_texts = m_client.GetSimplifiedWords();
                if (quran_word_texts != null)
                {

                    if (m_word_subsets != null)
                    {
                        for (int i = 0; i < m_word_subsets.Count; i++)
                        {
                            // calculate word values
                            long sentence_word_value = 0L;
                            foreach (Word word in m_word_subsets[i])
                            {
                                sentence_word_value += m_client.CalculateValue(word);
                            }

                            // calculate letter values
                            List<long> sentence_letter_values = new List<long>();
                            foreach (Word word in m_word_subsets[i])
                            {
                                foreach (Letter letter in word.Letters)
                                {
                                    long letter_value = m_client.CalculateValue(letter);
                                    if (m_add_verse_and_word_values_to_letter_value)
                                    {
                                        letter_value += m_client.CalculateValue(letter.Word);
                                        letter_value += m_client.CalculateValue(letter.Word.Verse);
                                    }
                                    sentence_letter_values.Add(letter_value);
                                }
                            }

                            // build sentence from word subset
                            StringBuilder str = new StringBuilder();
                            foreach (Word word in m_word_subsets[i])
                            {
                                str.Append(word.Text + " ");
                            }
                            if (str.Length > 1)
                            {
                                str.Remove(str.Length - 1, 1);
                            }

                            // generate Quran words
                            string generated_word = "";
                            if (m_client.NumericalSystem != null)
                            {
                                Dictionary<char, long> letter_dictionary = m_client.NumericalSystem.LetterValues;
                                if (letter_dictionary != null)
                                {
                                    List<char> numerical_letters = new List<char>(letter_dictionary.Keys);
                                    List<long> numerical_letter_values = new List<long>(letter_dictionary.Values);

                                    // interlace or concatenate values of numerical letters with sentence letters
                                    for (int j = 0; j < numerical_letters.Count; j++)
                                    {
                                        long number = 0L;
                                        long AAA = numerical_letter_values[j];
                                        long BBB = sentence_letter_values[j];
                                        switch (m_combination_method)
                                        {
                                            case CombinationMethod.Concatenate:
                                                number = Numbers.Concatenate(AAA, BBB, m_left_to_right);
                                                break;
                                            case CombinationMethod.InterlaceAB:
                                                number = Numbers.Interlace(AAA, BBB, true, m_left_to_right);
                                                break;
                                            case CombinationMethod.InterlaceBA:
                                                number = Numbers.Interlace(AAA, BBB, false, m_left_to_right);
                                                break;
                                            case CombinationMethod.CrossOverAB:
                                                number = Numbers.CrossOver(AAA, BBB, true, m_left_to_right);
                                                break;
                                            case CombinationMethod.CrossOverBA:
                                                number = Numbers.CrossOver(AAA, BBB, false, m_left_to_right);
                                                break;
                                        }

                                        if (number != -1)
                                        {
                                            // generate word from letter value combinations matching the number type
                                            if (Numbers.IsNumberType(number, m_number_type))
                                            {
                                                // mod 29 to select letter
                                                int index = (int)((long)number % (long)numerical_letters.Count);
                                                generated_word += numerical_letters[index];
                                            }
                                        }
                                    }
                                }
                            }

                            // add sentence if it generates a valid quran word
                            if (quran_word_texts.Contains(generated_word))
                            {
                                Line line = new Line();
                                if (line != null)
                                {
                                    line.Number = m_lines.Count + 1;
                                    line.Sentence = str.ToString();
                                    line.Value = sentence_word_value;
                                    line.Word = generated_word;
                                    m_lines.Add(line);

                                    if (m_generated_words.ContainsKey(generated_word))
                                    {
                                        m_generated_words[generated_word]++;
                                    }
                                    else
                                    {
                                        m_generated_words.Add(generated_word, 1);
                                    }
                                }
                            }

                            if (display_progress)
                            {
                                // display progress
                                this.Text = "WordGenerator | Primalogy value of أُمُّ ٱلْكِتَٰبِ = letters+diacritics of سورة الفاتحة | Sentence " + (i + 1) + "/" + m_word_subsets.Count;
                                ProgressBar.Value = ((i + 1) * 100) / m_word_subsets.Count;
                                WordCountLabel.Text = m_lines.Count + " (" + m_generated_words.Count + ") words";
                                WordCountLabel.ForeColor = Numbers.GetNumberForeColor(m_lines.Count);
                                WordCountLabel.Refresh();

                                Application.DoEvents();
                            }
                        } // for m_word_subsets
                    }

                    // at the end of running
                    this.Text = "WordGenerator | Primalogy value of أُمُّ ٱلْكِتَٰبِ = letters+diacritics of سورة الفاتحة | Sentences = " + m_word_subsets.Count;
                    ProgressBar.Value = 50;
                    ProgressBar.Refresh();
                    WordCountLabel.Text = m_lines.Count + " (" + m_generated_words.Count + ") words";
                    WordCountLabel.ForeColor = Numbers.GetNumberForeColor(m_lines.Count);
                    WordCountLabel.Refresh();

                    UpdateListView();
                    InspectButton_Click(null, null);

                    Application.DoEvents();
                }
            }
        }
    }
    private void InspectButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_lines != null)
            {
                StringBuilder str = new StringBuilder();

                if (sender == InspectButton)
                {
                    if (m_lines.Count > 0)
                    {
                        for (int i = 0; i < m_lines.Count; i++)
                        {
                            if (m_lines[i] != null)
                            {
                                str.AppendLine(m_lines[i].Number + "\t" + m_lines[i].Sentence + "\t" + m_lines[i].Value + "\t" + m_lines[i].Word);
                            }
                        }
                        str.AppendLine();
                    }
                }

                if (m_generated_words != null)
                {
                    str.AppendLine("#" + "\t" + "Word" + "\t" + "Freq" + "\t" + "Length" + "\t" + "Value");

                    int count = 0;
                    int frequency_sum = 0;
                    int length_sum = 0;
                    long value_sum = 0L;
                    foreach (string key in m_generated_words.Keys)
                    {
                        count++;

                        int frequency = m_generated_words[key];
                        frequency_sum += frequency;

                        int length = key.Length;
                        length_sum += length;

                        long value = m_client.CalculateValue(key);
                        value_sum += value;

                        str.AppendLine(count + "\t" + key + "\t" + frequency + "\t" + length + "\t" + value);
                    }

                    str.AppendLine();
                    str.AppendLine(count + "\t" + "Sum" + "\t" + frequency_sum + "\t" + length_sum + "\t" + value_sum);
                }

                string filename = null;
                if (sender == InspectButton)
                {
                    string compare_by = Line.CompareBy.ToString().Substring(2);
                    string compare_order = (Line.CompareOrder == LineCompareOrder.Ascending) ? "asc" : "desc";

                    filename =
                           m_numerical_system_name + "_"
                        + (m_add_verse_and_word_values_to_letter_value ? "vw" : "__")
                        + (m_add_positions_to_letter_value ? "_n" : "__")
                        + (m_add_distances_to_previous_to_letter_value ? "_-d" : "_-_")
                        + (m_add_distances_to_next_to_letter_value ? "_d-" : "__-")
                        + ("_" + m_combination_method.ToString().ToLower())
                        + ((m_left_to_right) ? "_l" : "_r")
                        + ((m_number_type != NumberType.None) ? "_" : "")
                        + (
                            (m_number_type == NumberType.Prime) ? "P" :
                            (m_number_type == NumberType.AdditivePrime) ? "AP" :
                            (m_number_type == NumberType.NonAdditivePrime) ? "XP" :
                            (m_number_type == NumberType.Composite) ? "C" :
                            (m_number_type == NumberType.AdditiveComposite) ? "AC" :
                            (m_number_type == NumberType.NonAdditiveComposite) ? "XC" : ""
                            )
                        + "_" + m_generated_words.Count.ToString()
                        + "_" + compare_by + "_" + compare_order
                        + ".txt";
                }
                else
                {
                    filename =
                           m_numerical_system_name + "_"
                        + (m_add_verse_and_word_values_to_letter_value ? "vw" : "__")
                        + (m_add_positions_to_letter_value ? "_n" : "__")
                        + (m_add_distances_to_previous_to_letter_value ? "_-d" : "_-_")
                        + (m_add_distances_to_next_to_letter_value ? "_d-" : "__-")
                        + ("_" + m_combination_method.ToString().ToLower())
                        + ((m_left_to_right) ? "_l" : "_r")
                        + ((m_number_type != NumberType.None) ? "_" : "")
                        + (
                            (m_number_type == NumberType.Prime) ? "P" :
                            (m_number_type == NumberType.AdditivePrime) ? "AP" :
                            (m_number_type == NumberType.NonAdditivePrime) ? "XP" :
                            (m_number_type == NumberType.Composite) ? "C" :
                            (m_number_type == NumberType.AdditiveComposite) ? "AC" :
                            (m_number_type == NumberType.NonAdditiveComposite) ? "XC" : ""
                            )
                        + "_" + m_generated_words.Count.ToString()
                        + ".txt";
                }

                if (!Directory.Exists(Globals.STATISTICS_FOLDER))
                {
                    Directory.CreateDirectory(Globals.STATISTICS_FOLDER);
                }
                string folder = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + m_numerical_system_name;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                if (Directory.Exists(Globals.STATISTICS_FOLDER))
                {
                    string path = folder + Path.DirectorySeparatorChar + filename;
                    FileHelper.SaveText(path, str.ToString());

                    if (sender == InspectButton)
                    {
                        FileHelper.DisplayFile(path);
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
