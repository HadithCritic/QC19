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
    private string m_numerical_system_name = null;
    private List<Letter> m_fatiha_letters = null;
    private string m_infallible_letters = null;
    private bool m_use_ya_husein = true;
    //private string m_ya_husein_letters = null;
    private long[] m_ya_husein_letter_values = null;
    private List<string> m_generated_lines = null;

    public MainForm()
    {
        InitializeComponent();

        using (Graphics graphics = this.CreateGraphics())
        {
            // 100% = 96.0F,   125% = 120.0F,   150% = 144.0F
            if (graphics.DpiX == 96.0F)
            {
                this.AutoGenerateWordsButton.Size = new System.Drawing.Size(25, 23);
            }
            else if (graphics.DpiX == 120.0F)
            {
                this.AutoGenerateWordsButton.Size = new System.Drawing.Size(27, 25);
            }
            else if (graphics.DpiX == 144.0F)
            {
                //this.AutoGenerateWordsButton.Size = new System.Drawing.Size(27, 25);
            }
        }
    }
    private void MainForm_Load(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            m_client = new Client(NumericalSystem.DEFAULT_NAME);
            if (m_client != null)
            {
                if (m_client.NumericalSystem != null)
                {
                    m_numerical_system_name = m_client.NumericalSystem.Name;

                    string text_mode = m_client.NumericalSystem.TextMode;
                    PopulateTextModeComboBox();
                    if (TextModeComboBox.Items.Count > 0)
                    {
                        if (TextModeComboBox.Items.Contains(text_mode))
                        {
                            TextModeComboBox.SelectedItem = text_mode;
                        }
                        else
                        {
                            TextModeComboBox.SelectedIndex = 0;
                        }
                    }

                    m_client.BuildSimplifiedBook(text_mode, false, true, false, false, true, false, false, false, false);
                    if (m_client.Book != null)
                    {
                        if (m_client.Book.Chapters != null)
                        {
                            if (m_client.Book.Chapters.Count > 0)
                            {
                                List<Verse> fatiha_verses = m_client.Book.Chapters[0].Verses;
                                if (fatiha_verses != null)
                                {
                                    List<Word> fatiha_words = new List<Word>();
                                    foreach (Verse fatiha_verse in fatiha_verses)
                                    {
                                        fatiha_words.AddRange(fatiha_verse.Words);
                                    }

                                    if (fatiha_words != null)
                                    {
                                        m_fatiha_letters = new List<Letter>();
                                        foreach (Word fatiha_word in fatiha_words)
                                        {
                                            m_fatiha_letters.AddRange(fatiha_word.Letters);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    m_infallible_letters = "محمدالمصطفىعليالمرتضىفاطمةالزهراءحسنالمجتبىحسينالشهيدعليالسجادمحمدالباقرجعفرالصادقموسىالكاظمعليالرضامحمدالجوادعليالهاديحسنالعسكريمحمدالمهدي";

                    //m_ya_husein_letters = "يييييييييياححححححححسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسسيييييييييينننننننننننننننننننننننننننننننننننننننننننننننننن";
                    m_ya_husein_letter_values = new long[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 1, 8, 8, 8, 8, 8, 8, 8, 8, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60 };

                    m_generated_lines = new List<string>();
                }
            }

            m_number_type = NumberType.Prime;
            NumberTypeLabel.Text = "P";
            NumberTypeLabel.ForeColor = Numbers.GetNumberForeColor(73L);
            ToolTip.SetToolTip(this.ValueInterlaceLabel, "concatenate letter values");
            ToolTip.SetToolTip(this.ValueCombinationDirectionLabel, "combine letter values right to left");
            ToolTip.SetToolTip(this.NumberTypeLabel, "allow prime combined letter values only");
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
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
                                if ((text_mode == "Original") || (text_mode == "SimplifiedMarks")) continue;

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
    private void YaHuseinCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_use_ya_husein = YaHuseinCheckBox.Checked;
        label7.Visible = m_use_ya_husein;
        label8.Visible = m_use_ya_husein;
        label9.Visible = m_use_ya_husein;
    }

    private enum WritingDirection { RightToLeft, LeftToRight };
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
            // at the start of running
            ProgressBar.Value = 0;
            ProgressBar.Refresh();
            WordCountLabel.Text = "0 Lines";
            WordCountLabel.ForeColor = Numbers.GetNumberForeColor(0L);
            WordCountLabel.Refresh();

            if (m_generated_lines == null)
            {
                m_generated_lines = new List<string>();
            }
            if (m_generated_lines != null)
            {
                m_generated_lines.Clear();
            }

            if (ModifierKeys == Keys.Shift)
            {
                int loops = 0;
                foreach (string text_mode in TextModeComboBox.Items)
                {
                    TextModeComboBox.SelectedItem = text_mode;
                    TextModeComboBox.Refresh();

                    loops += NumericalSystemComboBox.Items.Count;
                }

                int i = 0;
                foreach (string text_mode in TextModeComboBox.Items)
                {
                    TextModeComboBox.SelectedItem = text_mode;
                    TextModeComboBox.Refresh();

                    foreach (string numerical_system in NumericalSystemComboBox.Items)
                    {
                        NumericalSystemComboBox.SelectedItem = numerical_system;
                        NumericalSystemComboBox.Refresh();

                        GenerateLine();

                        // display progress
                        i++;
                        ProgressBar.Value = (i * 100) / loops;
                        if (m_generated_lines != null)
                        {
                            WordCountLabel.Text = m_generated_lines.Count + " Line" + (m_generated_lines.Count == 1 ? "" : "s");
                            WordCountLabel.ForeColor = Numbers.GetNumberForeColor(m_generated_lines.Count);
                            WordCountLabel.Refresh();
                        }
                        Application.DoEvents();

                    } // foreach NumericalSystem

                } // foreach TextMode
            }
            else
            {
                GenerateLine();
            }

            // at the end of running
            ProgressBar.Value = 50;
            ProgressBar.Refresh();
            if (m_generated_lines != null)
            {
                WordCountLabel.Text = m_generated_lines.Count + " Line" + (m_generated_lines.Count == 1 ? "" : "s");
                WordCountLabel.ForeColor = Numbers.GetNumberForeColor(m_generated_lines.Count);
                WordCountLabel.Refresh();
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
            // at the start of running
            ProgressBar.Value = 0;
            ProgressBar.Refresh();
            WordCountLabel.Text = "0 Lines";
            WordCountLabel.ForeColor = Numbers.GetNumberForeColor(0L);
            WordCountLabel.Refresh();

            if (m_generated_lines == null)
            {
                m_generated_lines = new List<string>();
            }
            if (m_generated_lines != null)
            {
                m_generated_lines.Clear();
            }

            if (ModifierKeys == Keys.Shift)
            {
                int loops = 0;
                foreach (string text_mode in TextModeComboBox.Items)
                {
                    TextModeComboBox.SelectedItem = text_mode;
                    TextModeComboBox.Refresh();

                    loops += NumericalSystemComboBox.Items.Count;
                }

                int i = 0;
                foreach (string text_mode in TextModeComboBox.Items)
                {
                    TextModeComboBox.SelectedItem = text_mode;
                    TextModeComboBox.Refresh();

                    foreach (string numerical_system in NumericalSystemComboBox.Items)
                    {
                        NumericalSystemComboBox.SelectedItem = numerical_system;
                        NumericalSystemComboBox.Refresh();

                        ProcessNumericalSystem();

                        // display progress
                        i++;
                        ProgressBar.Value = ((i + 1) * 100) / loops;
                        if (m_generated_lines != null)
                        {
                            WordCountLabel.Text = m_generated_lines.Count + " Line" + (m_generated_lines.Count == 1 ? "" : "s");
                            WordCountLabel.ForeColor = Numbers.GetNumberForeColor(m_generated_lines.Count);
                            WordCountLabel.Refresh();
                        }
                        Application.DoEvents();

                    } // foreach NumericalSystem

                } // foreach TextMode
            }
            else
            {
                ProcessNumericalSystem();
            }

            // at the end of running
            ProgressBar.Value = 50;
            ProgressBar.Refresh();
            if (m_generated_lines != null)
            {
                WordCountLabel.Text = m_generated_lines.Count + " Line" + (m_generated_lines.Count == 1 ? "" : "s");
                WordCountLabel.ForeColor = Numbers.GetNumberForeColor(m_generated_lines.Count);
                WordCountLabel.Refresh();
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
        if (m_generated_lines != null)
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

                                                GenerateLine();

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
    private void GenerateLine()
    {
        if (m_client != null)
        {
            if (m_generated_lines != null)
            {
                if (m_fatiha_letters != null)
                {
                    List<long> fatiha_letter_values = new List<long>();
                    List<long> infallible_letter_values = new List<long>();
                    for (int i = 0; i < m_fatiha_letters.Count; i++)
                    {
                        long value = m_client.CalculateValue(m_fatiha_letters[i]);
                        if (m_add_verse_and_word_values_to_letter_value)
                        {
                            value += m_client.CalculateValue(m_fatiha_letters[i].Word);
                            value += m_client.CalculateValue(m_fatiha_letters[i].Word.Verse);
                        }
                        if (m_use_ya_husein) value += m_ya_husein_letter_values[i];
                        fatiha_letter_values.Add(value);

                        value = m_client.CalculateValue(m_infallible_letters[i]);
                        if (m_use_ya_husein) value -= m_ya_husein_letter_values[i];
                        infallible_letter_values.Add(Math.Abs(value));
                    }

                    StringBuilder str = new StringBuilder();
                    if (m_client.NumericalSystem != null)
                    {
                        string generated_line = "";
                        if (fatiha_letter_values != null)
                        {
                            for (int j = 0; j < fatiha_letter_values.Count; j++)
                            {
                                long number = 0L;
                                long AAA = fatiha_letter_values[j];
                                long BBB = infallible_letter_values[j];
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
                                    if (Numbers.IsNumberType(number, m_number_type))
                                    {
                                        // mod 29 to select letter
                                        int i = (int)((long)number % (long)m_client.NumericalSystem.Count);
                                        char[] letters = new char[m_client.NumericalSystem.LetterValues.Count];
                                        m_client.NumericalSystem.LetterValues.Keys.CopyTo(letters, 0);
                                        generated_line += letters[i] + " ";
                                    }
                                    else
                                    {
                                        generated_line += "  ";
                                    }
                                }
                                else
                                {
                                    generated_line += "  ";
                                }
                                generated_line.Remove(generated_line.Length - 1, 1);
                            }

                            Line1Label.Text = generated_line.Substring(0, 96);
                            Line2Label.Text = generated_line.Substring(96, 96);
                            Line3Label.Text = generated_line.Substring(192);

                            string parameters =
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
                            ;
                            m_generated_lines.Add(parameters + "\t" + generated_line);
                        }
                    }
                }
            }
        }
    }
    private void InspectButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            StringBuilder str = new StringBuilder();
            if (m_generated_lines != null)
            {
                if (m_generated_lines.Count > 0)
                {
                    for (int i = 0; i < m_generated_lines.Count; i++)
                    {
                        string xxx = m_generated_lines[i];
                        if (xxx != null)
                        {
                            while (xxx.Contains("  "))
                            {
                                xxx = xxx.Replace("  ", " ");
                            }
                            str.AppendLine(xxx);
                        }
                    }
                    str.AppendLine();
                }
            }

            string filename = "AlFatiha_AhlulBayt" + (m_use_ya_husein ? "_YaHusein" : "") + ".txt";
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
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
}
