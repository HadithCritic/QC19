using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using Model;

public partial class MainForm : Form
{
    private const string INFO = "القرءان الكريم مكنون رقمياً في قرءاننا العظيم" + "\r\n"
                            + "114 ÷ 2 = 57" + "\r\n"
                            + "mid(57) = 29" + "\r\n"
                            //+ "سورة الحديد = 57" + "\r\n"
                            //+ "ءايات الحديد = 29" + "\r\n"
                            + "57×29 = 1653" + "\r\n"
                            + "Prime 16 = 53" + "\r\n"
                            + "16 APs to 114" + "\r\n"
                            + "53 ACs to 114" + "\r\n"
                            + "God > ∞    الله أكبر";

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

    public MainForm()
    {
        InitializeComponent();
    }

    private string m_numerical_system_name = "Original_Alphabet_Primes1";

    private Client m_client = null;

    private int m_max_chapter_count = 0;
    private int m_max_chapter_sum = 0;
    private int m_max_verse_sum = 0;
    private int m_max_word_sum = 0;
    private int m_max_letter_sum = 0;
    private long m_max_value_sum = 0L;
    private int m_max_chapter_number = 0;
    private int m_min_chapter_number = 0;
    private int m_max_chapter_verses = 0;
    private int m_min_chapter_verses = 0;
    private int m_max_chapter_words = 0;
    private int m_min_chapter_words = 0;
    private int m_max_chapter_letters = 0;
    private int m_min_chapter_letters = 0;
    private void UpdateNumericUpDownMinMax()
    {
        if (m_client != null)
        {
            if (m_client.Book != null)
            {
                m_max_chapter_count = m_client.Book.Chapters.Count;
                m_max_chapter_sum = (m_max_chapter_count * (m_max_chapter_count + 1)) / 2;
                m_max_verse_sum = m_client.Book.Verses.Count;
                m_max_word_sum = m_client.Book.Words.Count;
                m_max_letter_sum = m_client.Book.Letters.Count;
                m_max_value_sum = 0L;
                foreach (Chapter chapter in m_client.Book.Chapters)
                {
                    m_max_value_sum += chapter.Value;
                }
                m_max_chapter_number = m_client.Book.Chapters.Count;
                m_min_chapter_number = 1;
                foreach (Chapter chapter in m_client.Book.Chapters)
                {
                    if (m_max_chapter_verses < chapter.Verses.Count)
                    {
                        m_max_chapter_verses = chapter.Verses.Count;
                    }
                    if (m_min_chapter_verses > chapter.Verses.Count)
                    {
                        m_min_chapter_verses = chapter.Verses.Count;
                    }

                    if (m_max_chapter_words < chapter.Words.Count)
                    {
                        m_max_chapter_words = chapter.Words.Count;
                    }
                    if (m_min_chapter_words > chapter.Words.Count)
                    {
                        m_min_chapter_words = chapter.Words.Count;
                    }

                    if (m_max_chapter_letters < chapter.Letters.Count)
                    {
                        m_max_chapter_letters = chapter.Letters.Count;
                    }
                    if (m_min_chapter_letters > chapter.Letters.Count)
                    {
                        m_min_chapter_letters = chapter.Letters.Count;
                    }
                }

                ChaptersNumericUpDown.Minimum = 1;
                ChaptersNumericUpDown.Maximum = m_max_chapter_count;
                ChaptersNumericUpDown.Value = 1;

                ChapterSumNumericUpDown.Minimum = -1;
                ChapterSumNumericUpDown.Maximum = m_max_chapter_sum;
                ChapterSumNumericUpDown.Value = -1;

                VerseSumNumericUpDown.Minimum = -1;
                VerseSumNumericUpDown.Maximum = m_max_verse_sum;
                VerseSumNumericUpDown.Value = -1;

                WordSumNumericUpDown.Minimum = -1;
                WordSumNumericUpDown.Maximum = m_max_word_sum;
                WordSumNumericUpDown.Value = -1;

                LetterSumNumericUpDown.Minimum = -1;
                LetterSumNumericUpDown.Maximum = m_max_letter_sum;
                LetterSumNumericUpDown.Value = -1;

                ValueSumNumericUpDown.Minimum = -1;
                ValueSumNumericUpDown.Maximum = m_max_value_sum;
                ValueSumNumericUpDown.Value = -1;

                CPlusVSumNumericUpDown.Minimum = -1;
                CPlusVSumNumericUpDown.Maximum = m_max_chapter_sum + m_max_verse_sum;
                CPlusVSumNumericUpDown.Value = -1;

                CMinusVSumNumericUpDown.Minimum = -1;
                CMinusVSumNumericUpDown.Maximum = m_max_chapter_sum - m_max_verse_sum;
                CMinusVSumNumericUpDown.Value = -1;

                CTimesVSumNumericUpDown.Minimum = -1;
                CTimesVSumNumericUpDown.Maximum = m_max_chapter_sum * m_max_verse_sum;
                CTimesVSumNumericUpDown.Value = -1;

                C2MinusC1NumericUpDown.Minimum = -1;
                C2MinusC1NumericUpDown.Maximum = m_max_chapter_number - m_min_chapter_number;
                C2MinusC1NumericUpDown.Value = -1;

                V2MinusV1NumericUpDown.Minimum = -1;
                V2MinusV1NumericUpDown.Maximum = m_max_chapter_verses - m_min_chapter_verses;
                V2MinusV1NumericUpDown.Value = -1;

                W2MinusW1NumericUpDown.Minimum = -1;
                W2MinusW1NumericUpDown.Maximum = m_max_chapter_words - m_min_chapter_words;
                W2MinusW1NumericUpDown.Value = -1;

                L2MinusL1NumericUpDown.Minimum = -1;
                L2MinusL1NumericUpDown.Maximum = m_max_chapter_letters - m_min_chapter_letters;
                L2MinusL1NumericUpDown.Value = -1;

                NumericUpDown_ValueChanged(null, null);
            }
        }
    }

    private void ResetMatchFields()
    {
        MatchChaptersTextBox.Text = m_max_chapter_count.ToString();
        MatchChapterSumTextBox.Text = m_max_chapter_sum.ToString();
        MatchVerseSumTextBox.Text = m_max_verse_sum.ToString();
        MatchWordSumTextBox.Text = m_max_word_sum.ToString();
        MatchLetterSumTextBox.Text = m_max_letter_sum.ToString();
        MatchValueSumTextBox.Text = m_max_value_sum.ToString();

        MatchChaptersTextBox.ForeColor = Numbers.GetNumberForeColor(m_max_chapter_count);
        MatchChapterSumTextBox.ForeColor = Numbers.GetNumberForeColor(m_max_chapter_sum);
        MatchVerseSumTextBox.ForeColor = Numbers.GetNumberForeColor(m_max_verse_sum);
        MatchWordSumTextBox.ForeColor = Numbers.GetNumberForeColor(m_max_word_sum);
        MatchLetterSumTextBox.ForeColor = Numbers.GetNumberForeColor(m_max_letter_sum);
        MatchValueSumTextBox.ForeColor = Numbers.GetNumberForeColor(m_max_value_sum);

        MatchChaptersTextBox.BackColor = MatchChaptersTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
        MatchChapterSumTextBox.BackColor = MatchChapterSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
        MatchVerseSumTextBox.BackColor = MatchVerseSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
        MatchWordSumTextBox.BackColor = MatchWordSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
        MatchLetterSumTextBox.BackColor = MatchLetterSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
        MatchValueSumTextBox.BackColor = MatchValueSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly

        MatchChapterListTextBox.Text = INFO;
        MatchChapterListTextBox.ForeColor = Color.Black;
        ToolTip.SetToolTip(this.MatchChapterListTextBox, "Al-Quran Al-Kareem is hidden within our Quran Adheem.\r\n©2009-2026 Ali Adams");
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        ShowVersion(false);

        this.Cursor = Cursors.WaitCursor;
        try
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

                    SaveMatchesButton.Enabled = false;
                    ChaptersNumericUpDown.Focus();

                    PopulateOutputFormatComboBox();
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

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (s_running)
        {
            e.Cancel = true;
        }
    }
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            if (s_running)
            {
                if (DialogResult.Yes == MessageBox.Show(
                    "Stop the search?",
                    Application.ProductName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2))
                {
                    Finish(true);
                }
            }
        }
        else if (ModifierKeys == Keys.Control)
        {
            m_with_revision = !m_with_revision;
            ShowVersion(m_with_revision);
        }
    }
    private bool m_with_revision = false;
    private void ShowVersion(bool with_revision)
    {
        m_with_revision = with_revision;

        int major = Assembly.GetEntryAssembly().GetName().Version.Major;
        int minor = Assembly.GetEntryAssembly().GetName().Version.Minor;
        int build = Assembly.GetEntryAssembly().GetName().Version.Build;
        int revision = Assembly.GetEntryAssembly().GetName().Version.Revision;
        //int major_revision = Assembly.GetEntryAssembly().GetName().Version.MajorRevision;
        //int minor_revision = Assembly.GetEntryAssembly().GetName().Version.MinorRevision;
        //this.Text = Application.ProductName + " - " + major + "." + minor + "." + build + (m_with_revision ? ("." + revision.ToString("0000")) : "");
        this.Text = Application.ProductName + " - " + major + "." + minor + "." + build + (m_with_revision ? ("." + Globals.RELEASE_EDITION) : "");
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
        // force close up of TextModeComboBox immeidately
        this.Refresh();

        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (TextModeComboBox.SelectedItem != null)
            {
                // and then simplify text
                m_with_bism_Allah = (
                                        (TextModeComboBox.Text == "SimplifiedMarks") ||
                                        (TextModeComboBox.Text == "Simplified29") ||
                                        (TextModeComboBox.Text == "Simplified31") ||
                                        (TextModeComboBox.Text == "Simplified36") ||
                                        (TextModeComboBox.Text == "SimplifiedDots")
                                    );
                m_hamza_above_horizontal_line_as_letter = m_with_bism_Allah;

                BuildSimplifiedBookAndDisplaySelection();
                UpdateTextModeOptions();

                // populate new numerical systems
                string backup_valuation_system = null;
                if (NumericalSystemComboBox.SelectedItem != null)
                {
                    backup_valuation_system = NumericalSystemComboBox.SelectedItem.ToString();
                }
                PopulateNumericalSystemComboBox();
                if (NumericalSystemComboBox.Items.Count > 0)
                {
                    if (backup_valuation_system != null)
                    {
                        if (NumericalSystemComboBox.Items.Contains(backup_valuation_system))
                        {
                            NumericalSystemComboBox.SelectedItem = backup_valuation_system;
                        }
                        else
                        {
                            NumericalSystemComboBox.SelectedIndex = 0;
                        }
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
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void NumericalSystemComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        m_numerical_system_name = TextModeComboBox.SelectedItem.ToString() + "_" + NumericalSystemComboBox.SelectedItem.ToString();
        if (m_client != null)
        {
            m_client.LoadNumericalSystem(m_numerical_system_name);
            NumericalSystemComboBox_MouseHover(null, null);

            // update chapter values
            foreach (Chapter chapter in m_client.Book.Chapters)
            {
                m_client.CalculateValue(chapter);
            }

            UpdateNumericUpDownMinMax();
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
    private void NumericalSystemComboBox_DropDown(object sender, EventArgs e)
    {
        NumericalSystemComboBox.DropDownHeight = this.Height - NumericalSystemComboBox.Top - NumericalSystemComboBox.Height - 1;
        //NumericalSystemComboBox.DropDownWidth = this.Width - NumericalSystemComboBox.Left - 1;
    }

    private bool m_with_bism_Allah = true;
    private bool m_waw_as_word = false;
    private bool m_shadda_as_letter = false;
    private bool m_hamza_above_horizontal_line_as_letter = false;
    private bool m_elf_above_horizontal_line_as_letter = false;
    private bool m_yaa_above_horizontal_line_as_letter = false;
    private bool m_noon_above_horizontal_line_as_letter = false;
    private void WithBismAllahCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_with_bism_Allah = WithBismAllahCheckBox.Checked;
        BuildSimplifiedBookAndDisplaySelection();
    }
    private void WawAsWordCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_waw_as_word = WawAsWordCheckBox.Checked;
        BuildSimplifiedBookAndDisplaySelection();
    }
    private void ShaddaAsLetterCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_shadda_as_letter = ShaddaAsLetterCheckBox.Checked;
        BuildSimplifiedBookAndDisplaySelection();
    }
    private void HamzaAboveHorizontalLineAsLetterCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_hamza_above_horizontal_line_as_letter = HamzaAboveHorizontalLineAsLetterCheckBox.Checked;
        BuildSimplifiedBookAndDisplaySelection();
    }
    private void ElfAboveHorizontalLineAsLetterCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_elf_above_horizontal_line_as_letter = ElfAboveHorizontalLineAsLetterCheckBox.Checked;
        BuildSimplifiedBookAndDisplaySelection();
    }
    private void YaaAboveHorizontalLineAsLetterCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_yaa_above_horizontal_line_as_letter = YaaAboveHorizontalLineAsLetterCheckBox.Checked;
        BuildSimplifiedBookAndDisplaySelection();
    }
    private void NoonAboveHorizontalLineAsLetterCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_noon_above_horizontal_line_as_letter = NoonAboveHorizontalLineAsLetterCheckBox.Checked;
        BuildSimplifiedBookAndDisplaySelection();
    }
    private void BuildSimplifiedBookAndDisplaySelection()
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (TextModeComboBox.SelectedItem != null)
            {
                string text_mode = TextModeComboBox.SelectedItem.ToString();

                if (m_client != null)
                {
                    if (m_client.Book != null)
                    {
                        // ALWAYS rebuild book to support dynamic editing of SimplificationRules
                        // uncomment to ONLY rebuild ON CHANGE to these variables
                        //if ((m_client.Book.TextMode != text_mode) ||
                        //    (m_client.Book.WithBismAllah != m_with_bism_Allah) ||
                        //    (m_client.Book.WawAsWord != m_waw_as_word) ||
                        //    (m_client.Book.ShaddaAsLetter != m_shadda_as_letter)
                        //    (m_client.Book.HamzaAboveHorizontalLineAsLetter != m_hamza_above_horizontal_line_as_letter)
                        //    (m_client.Book.ElfAboveHorizontalLineAsLetter != m_elf_above_horizontal_line_as_letter)
                        //    (m_client.Book.YaaAboveHorizontalLineAsLetter != m_yaa_above_horizontal_line_as_letter)
                        //    (m_client.Book.NoonAboveHorizontalLineAsLetter != m_noon_above_horizontal_line_as_letter)
                        //   )
                        {
                            if (text_mode == "Original")
                            {
                                m_with_bism_Allah = true;
                                m_waw_as_word = false;
                                m_shadda_as_letter = false;
                                m_hamza_above_horizontal_line_as_letter = false;
                                m_elf_above_horizontal_line_as_letter = false;
                                m_yaa_above_horizontal_line_as_letter = false;
                                m_noon_above_horizontal_line_as_letter = false;
                            }

                            m_client.BuildSimplifiedBook(text_mode, false, m_with_bism_Allah, m_waw_as_word, m_shadda_as_letter, m_hamza_above_horizontal_line_as_letter, m_elf_above_horizontal_line_as_letter, m_yaa_above_horizontal_line_as_letter, m_noon_above_horizontal_line_as_letter, false);
                        }
                    }
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

    private int m_chapter_count = -1;
    private int m_chapter_sum = -1;
    private int m_verse_sum = -1;
    private int m_word_sum = -1;
    private int m_letter_sum = -1;
    private long m_value_sum = -1L;
    private int m_cplusv_sum = -1;
    private int m_cminusv_sum = -1;
    private int m_ctimesv_sum = -1;
    private int m_c2minusc1 = -1;
    private int m_v2minusv1 = -1;
    private int m_w2minusw1 = -1;
    private int m_l2minusl1 = -1;
    private BigInteger m_combinations = 1; // prevent null and div by 0
    private void NumericUpDown_TextChanged(object sender, EventArgs e)
    {
        if (sender is NumericUpDown)
        {
            (sender as NumericUpDown).ForeColor = Numbers.GetNumberForeColor(((long)(sender as NumericUpDown).Value));
        }
    }
    private void NumericUpDown_ValueChanged(object sender, EventArgs e)
    {
        ResetMatchFields();

        //ChaptersLabel.Text = (ChaptersNumericUpDown.Value <= 1) ? "Chapter" : ((ChaptersNumericUpDown.Value <= 2) ? "Chapters" : "n Chapters");
        ChapterSumLabel.Text = (ChaptersNumericUpDown.Value <= 1) ? "Number" : ((ChaptersNumericUpDown.Value <= 2) ? "C1 + C2" : "C1+...+Cn");
        VerseSumLabel.Text = (ChaptersNumericUpDown.Value <= 1) ? "Verses" : ((ChaptersNumericUpDown.Value <= 2) ? "V1 + V2" : "V1+...+Vn");
        WordSumLabel.Text = (ChaptersNumericUpDown.Value <= 1) ? "Words" : ((ChaptersNumericUpDown.Value <= 2) ? "W1 + W2" : "W1+...+Wn");
        LetterSumLabel.Text = (ChaptersNumericUpDown.Value <= 1) ? "Letters" : ((ChaptersNumericUpDown.Value <= 2) ? "L1 + L2" : "L1+...+Ln");
        ValueSumLabel.Text = (ChaptersNumericUpDown.Value <= 1) ? "Value" : ((ChaptersNumericUpDown.Value <= 2) ? "N1 + N2" : "N1+...+Nn");

        C2MinusC1NumericUpDown.Enabled = (C2MinusC1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
        if (!C2MinusC1NumericUpDown.Enabled) C2MinusC1NumericUpDown.Value = -1;
        C2MinusC1NumberTypeLabel.BackColor = (ChaptersNumericUpDown.Value == 2) ? SystemColors.Window : SystemColors.Control;

        V2MinusV1NumericUpDown.Enabled = (V2MinusV1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
        if (!V2MinusV1NumericUpDown.Enabled) V2MinusV1NumericUpDown.Value = -1;
        V2MinusV1NumberTypeLabel.BackColor = (ChaptersNumericUpDown.Value == 2) ? SystemColors.Window : SystemColors.Control;

        W2MinusW1NumericUpDown.Enabled = (W2MinusW1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
        if (!W2MinusW1NumericUpDown.Enabled) W2MinusW1NumericUpDown.Value = -1;
        W2MinusW1NumberTypeLabel.BackColor = (ChaptersNumericUpDown.Value == 2) ? SystemColors.Window : SystemColors.Control;

        L2MinusL1NumericUpDown.Enabled = (L2MinusL1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
        if (!L2MinusL1NumericUpDown.Enabled) L2MinusL1NumericUpDown.Value = -1;
        L2MinusL1NumberTypeLabel.BackColor = (ChaptersNumericUpDown.Value == 2) ? SystemColors.Window : SystemColors.Control;

        if (sender is NumericUpDown)
        {
            (sender as NumericUpDown).ForeColor = Numbers.GetNumberForeColor(((long)(sender as NumericUpDown).Value));
        }

        if (m_chapter_count > 0)
        {
            int count = (int)ChaptersNumericUpDown.Value;
            m_combinations = Numbers.nCk(m_client.Book.Chapters.Count, count);
            MatchesTextBox.Text = m_combinations.ToString();
            MatchesTextBox.ForeColor = Numbers.GetNumberForeColor((long)m_combinations);
            MatchesTextBox.BackColor = MatchesTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
        }

        UpdateToolTipsAndColors();
    }
    private void TextBox_TextChanged(object sender, EventArgs e)
    {
        UpdateToolTipsAndColors();
    }
    private void UpdateToolTipsAndColors()
    {
        try
        {
            ChaptersNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)ChaptersNumericUpDown.Value);
            ChapterSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)ChapterSumNumericUpDown.Value);
            VerseSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)VerseSumNumericUpDown.Value);
            WordSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)WordSumNumericUpDown.Value);
            LetterSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)LetterSumNumericUpDown.Value);
            ValueSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)ValueSumNumericUpDown.Value);
            CPlusVSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)CPlusVSumNumericUpDown.Value);
            CMinusVSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)CMinusVSumNumericUpDown.Value);
            CTimesVSumNumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)CTimesVSumNumericUpDown.Value);
            C2MinusC1NumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)C2MinusC1NumericUpDown.Value);
            V2MinusV1NumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)V2MinusV1NumericUpDown.Value);
            W2MinusW1NumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)W2MinusW1NumericUpDown.Value);
            L2MinusL1NumericUpDown.ForeColor = Numbers.GetNumberForeColor((int)L2MinusL1NumericUpDown.Value);
            MatchChaptersTextBox.ForeColor = Numbers.GetNumberForeColor(int.Parse(MatchChaptersTextBox.Text));
            MatchChapterSumTextBox.ForeColor = Numbers.GetNumberForeColor(int.Parse(MatchChapterSumTextBox.Text));
            MatchVerseSumTextBox.ForeColor = Numbers.GetNumberForeColor(int.Parse(MatchVerseSumTextBox.Text));
            MatchWordSumTextBox.ForeColor = Numbers.GetNumberForeColor(int.Parse(MatchWordSumTextBox.Text));
            MatchLetterSumTextBox.ForeColor = Numbers.GetNumberForeColor(int.Parse(MatchLetterSumTextBox.Text));
            MatchValueSumTextBox.ForeColor = Numbers.GetNumberForeColor(int.Parse(MatchValueSumTextBox.Text));

            ChaptersNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)ChaptersNumericUpDown.Value, 19, SystemColors.Window);
            ChapterSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)ChapterSumNumericUpDown.Value, 19, SystemColors.Window);
            VerseSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)VerseSumNumericUpDown.Value, 19, SystemColors.Window);
            WordSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)WordSumNumericUpDown.Value, 19, SystemColors.Window);
            LetterSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)LetterSumNumericUpDown.Value, 19, SystemColors.Window);
            ValueSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)ValueSumNumericUpDown.Value, 19, SystemColors.Window);
            CPlusVSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)CPlusVSumNumericUpDown.Value, 19, SystemColors.Window);
            CMinusVSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)CMinusVSumNumericUpDown.Value, 19, SystemColors.Window);
            CTimesVSumNumericUpDown.BackColor = Numbers.GetNumberBackColor((int)CTimesVSumNumericUpDown.Value, 19, SystemColors.Window);
            C2MinusC1NumericUpDown.BackColor = Numbers.GetNumberBackColor((int)C2MinusC1NumericUpDown.Value, 19, SystemColors.Window);
            V2MinusV1NumericUpDown.BackColor = Numbers.GetNumberBackColor((int)V2MinusV1NumericUpDown.Value, 19, SystemColors.Window);
            W2MinusW1NumericUpDown.BackColor = Numbers.GetNumberBackColor((int)W2MinusW1NumericUpDown.Value, 19, SystemColors.Window);
            L2MinusL1NumericUpDown.BackColor = Numbers.GetNumberBackColor((int)L2MinusL1NumericUpDown.Value, 19, SystemColors.Window);
            MatchChaptersTextBox.BackColor = Numbers.GetNumberBackColor(int.Parse(MatchChaptersTextBox.Text), 19, SystemColors.Window);
            MatchChapterSumTextBox.BackColor = Numbers.GetNumberBackColor(int.Parse(MatchChapterSumTextBox.Text), 19, SystemColors.Window);
            MatchVerseSumTextBox.BackColor = Numbers.GetNumberBackColor(int.Parse(MatchVerseSumTextBox.Text), 19, SystemColors.Window);
            MatchWordSumTextBox.BackColor = Numbers.GetNumberBackColor(int.Parse(MatchWordSumTextBox.Text), 19, SystemColors.Window);
            MatchLetterSumTextBox.BackColor = Numbers.GetNumberBackColor(int.Parse(MatchLetterSumTextBox.Text), 19, SystemColors.Window);
            MatchValueSumTextBox.BackColor = Numbers.GetNumberBackColor(int.Parse(MatchValueSumTextBox.Text), 19, SystemColors.Window);

            ToolTip.SetToolTip(this.ChaptersNumericUpDown, Numbers.GetNumberToolTipText((int)ChaptersNumericUpDown.Value));
            ToolTip.SetToolTip(this.ChapterSumNumericUpDown, Numbers.GetNumberToolTipText((int)ChapterSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.VerseSumNumericUpDown, Numbers.GetNumberToolTipText((int)VerseSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.WordSumNumericUpDown, Numbers.GetNumberToolTipText((int)WordSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.LetterSumNumericUpDown, Numbers.GetNumberToolTipText((int)LetterSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.ValueSumNumericUpDown, Numbers.GetNumberToolTipText((int)ValueSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.CPlusVSumNumericUpDown, Numbers.GetNumberToolTipText((int)CPlusVSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.CMinusVSumNumericUpDown, Numbers.GetNumberToolTipText((int)CMinusVSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.CTimesVSumNumericUpDown, Numbers.GetNumberToolTipText((int)CTimesVSumNumericUpDown.Value));
            ToolTip.SetToolTip(this.C2MinusC1NumericUpDown, Numbers.GetNumberToolTipText((int)C2MinusC1NumericUpDown.Value));
            ToolTip.SetToolTip(this.V2MinusV1NumericUpDown, Numbers.GetNumberToolTipText((int)V2MinusV1NumericUpDown.Value));
            ToolTip.SetToolTip(this.W2MinusW1NumericUpDown, Numbers.GetNumberToolTipText((int)W2MinusW1NumericUpDown.Value));
            ToolTip.SetToolTip(this.L2MinusL1NumericUpDown, Numbers.GetNumberToolTipText((int)L2MinusL1NumericUpDown.Value));
            ToolTip.SetToolTip(this.MatchChaptersTextBox, Numbers.GetNumberToolTipText(int.Parse(MatchChaptersTextBox.Text)));
            ToolTip.SetToolTip(this.MatchChapterSumTextBox, Numbers.GetNumberToolTipText(int.Parse(MatchChapterSumTextBox.Text)));
            ToolTip.SetToolTip(this.MatchVerseSumTextBox, Numbers.GetNumberToolTipText(int.Parse(MatchVerseSumTextBox.Text)));
            ToolTip.SetToolTip(this.MatchWordSumTextBox, Numbers.GetNumberToolTipText(int.Parse(MatchWordSumTextBox.Text)));
            ToolTip.SetToolTip(this.MatchLetterSumTextBox, Numbers.GetNumberToolTipText(int.Parse(MatchLetterSumTextBox.Text)));
            ToolTip.SetToolTip(this.MatchValueSumTextBox, Numbers.GetNumberToolTipText(int.Parse(MatchValueSumTextBox.Text)));

            ChaptersNumericUpDown.Refresh();
            ChapterSumNumericUpDown.Refresh();
            VerseSumNumericUpDown.Refresh();
            WordSumNumericUpDown.Refresh();
            LetterSumNumericUpDown.Refresh();
            ValueSumNumericUpDown.Refresh();
            CPlusVSumNumericUpDown.Refresh();
            CMinusVSumNumericUpDown.Refresh();
            CTimesVSumNumericUpDown.Refresh();
            C2MinusC1NumericUpDown.Refresh();
            V2MinusV1NumericUpDown.Refresh();
            W2MinusW1NumericUpDown.Refresh();
            L2MinusL1NumericUpDown.Refresh();
            MatchChaptersTextBox.Refresh();
            MatchChapterSumTextBox.Refresh();
            MatchVerseSumTextBox.Refresh();
            MatchWordSumTextBox.Refresh();
            MatchLetterSumTextBox.Refresh();
            MatchValueSumTextBox.Refresh();
        }
        catch
        {
            // silence error
        }
    }
    private void UpdateTextModeOptions()
    {
        if (TextModeComboBox.SelectedItem != null)
        {
            try
            {
                for (int i = 0; i < 3; i++)
                    WithBismAllahCheckBox.CheckedChanged -= new System.EventHandler(WithBismAllahCheckBox_CheckedChanged);
                for (int i = 0; i < 3; i++)
                    WawAsWordCheckBox.CheckedChanged -= new System.EventHandler(WawAsWordCheckBox_CheckedChanged);
                for (int i = 0; i < 3; i++)
                    ShaddaAsLetterCheckBox.CheckedChanged -= new System.EventHandler(ShaddaAsLetterCheckBox_CheckedChanged);
                for (int i = 0; i < 3; i++)
                    HamzaAboveHorizontalLineAsLetterCheckBox.CheckedChanged -= new System.EventHandler(HamzaAboveHorizontalLineAsLetterCheckBox_CheckedChanged);
                for (int i = 0; i < 3; i++)
                    ElfAboveHorizontalLineAsLetterCheckBox.CheckedChanged -= new System.EventHandler(ElfAboveHorizontalLineAsLetterCheckBox_CheckedChanged);
                for (int i = 0; i < 3; i++)
                    YaaAboveHorizontalLineAsLetterCheckBox.CheckedChanged -= new System.EventHandler(YaaAboveHorizontalLineAsLetterCheckBox_CheckedChanged);
                for (int i = 0; i < 3; i++)
                    NoonAboveHorizontalLineAsLetterCheckBox.CheckedChanged -= new System.EventHandler(NoonAboveHorizontalLineAsLetterCheckBox_CheckedChanged);

                string text_mode = TextModeComboBox.SelectedItem.ToString();
                if ((text_mode == "Original") || (text_mode == "SimplifiedMarks"))
                {
                    m_with_bism_Allah = true;
                    m_waw_as_word = false;
                    m_shadda_as_letter = false;
                    m_hamza_above_horizontal_line_as_letter = false;
                }

                WithBismAllahCheckBox.Checked = m_with_bism_Allah;
                WawAsWordCheckBox.Checked = m_waw_as_word;
                ShaddaAsLetterCheckBox.Checked = m_shadda_as_letter;
                HamzaAboveHorizontalLineAsLetterCheckBox.Checked = m_hamza_above_horizontal_line_as_letter;
                ElfAboveHorizontalLineAsLetterCheckBox.Checked = m_elf_above_horizontal_line_as_letter;
                YaaAboveHorizontalLineAsLetterCheckBox.Checked = m_yaa_above_horizontal_line_as_letter;
                NoonAboveHorizontalLineAsLetterCheckBox.Checked = m_noon_above_horizontal_line_as_letter;

                WithBismAllahCheckBox.Enabled = (text_mode != "Original");
                WawAsWordCheckBox.Enabled = (text_mode != "Original");
                ShaddaAsLetterCheckBox.Enabled = (text_mode != "Original");
                HamzaAboveHorizontalLineAsLetterCheckBox.Enabled = ((text_mode != "Original") && (text_mode != "Simplified28") && (text_mode != "Simplified30"));
                ElfAboveHorizontalLineAsLetterCheckBox.Enabled = (text_mode != "Original");
                YaaAboveHorizontalLineAsLetterCheckBox.Enabled = (text_mode != "Original");
                NoonAboveHorizontalLineAsLetterCheckBox.Enabled = (text_mode != "Original");

                WithBismAllahCheckBox.Refresh();
                WawAsWordCheckBox.Refresh();
                ShaddaAsLetterCheckBox.Refresh();
                HamzaAboveHorizontalLineAsLetterCheckBox.Refresh();
                ElfAboveHorizontalLineAsLetterCheckBox.Refresh();
                YaaAboveHorizontalLineAsLetterCheckBox.Refresh();
                NoonAboveHorizontalLineAsLetterCheckBox.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                WithBismAllahCheckBox.CheckedChanged += new System.EventHandler(WithBismAllahCheckBox_CheckedChanged);
                WawAsWordCheckBox.CheckedChanged += new System.EventHandler(WawAsWordCheckBox_CheckedChanged);
                ShaddaAsLetterCheckBox.CheckedChanged += new System.EventHandler(ShaddaAsLetterCheckBox_CheckedChanged);
                HamzaAboveHorizontalLineAsLetterCheckBox.CheckedChanged += new System.EventHandler(HamzaAboveHorizontalLineAsLetterCheckBox_CheckedChanged);
                ElfAboveHorizontalLineAsLetterCheckBox.CheckedChanged += new System.EventHandler(ElfAboveHorizontalLineAsLetterCheckBox_CheckedChanged);
                YaaAboveHorizontalLineAsLetterCheckBox.CheckedChanged += new System.EventHandler(YaaAboveHorizontalLineAsLetterCheckBox_CheckedChanged);
                NoonAboveHorizontalLineAsLetterCheckBox.CheckedChanged += new System.EventHandler(NoonAboveHorizontalLineAsLetterCheckBox_CheckedChanged);
            }
        }
    }

    private void NumericUpDown_Enter(object sender, EventArgs e)
    {
        NumericUpDown control = sender as NumericUpDown;
        if (control != null)
        {
            control.Select(0, control.Text.Length);
        }
    }
    private void NumericUpDown_Leave(object sender, EventArgs e)
    {
        NumericUpDown control = sender as NumericUpDown;
        if (control != null)
        {
            if (String.IsNullOrEmpty(control.Text))
            {
                if (control == ChaptersNumericUpDown)
                {
                    ChaptersNumericUpDown.Value = 2;
                    ChaptersNumericUpDown.Text = "2";
                }
                else
                {
                    control.Value = -1;
                    control.Text = "-1";
                }
            }
        }
    }
    private void CaptureQueryParameters()
    {
        m_chapter_count = (int)ChaptersNumericUpDown.Value;
        m_chapter_sum = (int)ChapterSumNumericUpDown.Value;
        m_verse_sum = (int)VerseSumNumericUpDown.Value;
        m_word_sum = (int)WordSumNumericUpDown.Value;
        m_letter_sum = (int)LetterSumNumericUpDown.Value;
        m_value_sum = (long)ValueSumNumericUpDown.Value;
        m_cplusv_sum = (int)CPlusVSumNumericUpDown.Value;
        m_cminusv_sum = (int)CMinusVSumNumericUpDown.Value;
        m_ctimesv_sum = (int)CTimesVSumNumericUpDown.Value;
        m_c2minusc1 = (int)C2MinusC1NumericUpDown.Value;
        m_v2minusv1 = (int)V2MinusV1NumericUpDown.Value;
        m_w2minusw1 = (int)W2MinusW1NumericUpDown.Value;
        m_l2minusl1 = (int)L2MinusL1NumericUpDown.Value;
    }

    private void NumberTypeLabel_Click(object sender, EventArgs e)
    {
        ResetMatchFields();

        Control control = sender as Control;
        if (control != null)
        {
            UpdateNumberType(control);

            if (control == ChapterSumNumberTypeLabel)
            {
                ChapterSumNumericUpDown.Enabled = (control.Text == "");
                ChapterSumNumericUpDown.Focus();
            }
            else if (control == VersesNumberTypeLabel)
            {
                VerseSumNumericUpDown.Enabled = (control.Text == "");
                VerseSumNumericUpDown.Focus();
            }
            else if (control == WordsNumberTypeLabel)
            {
                WordSumNumericUpDown.Enabled = (control.Text == "");
                WordSumNumericUpDown.Focus();
            }
            else if (control == LettersNumberTypeLabel)
            {
                LetterSumNumericUpDown.Enabled = (control.Text == "");
                LetterSumNumericUpDown.Focus();
            }
            else if (control == ValueNumberTypeLabel)
            {
                ValueSumNumericUpDown.Enabled = (control.Text == "");
                ValueSumNumericUpDown.Focus();
            }
            else if (control == CPlusVSumNumberTypeLabel)
            {
                CPlusVSumNumericUpDown.Enabled = (control.Text == "");
                CPlusVSumNumericUpDown.Focus();
            }
            else if (control == CMinusVSumNumberTypeLabel)
            {
                CMinusVSumNumericUpDown.Enabled = (control.Text == "");
                CMinusVSumNumericUpDown.Focus();
            }
            else if (control == CTimesVSumNumberTypeLabel)
            {
                CTimesVSumNumericUpDown.Enabled = (control.Text == "");
                CTimesVSumNumericUpDown.Focus();
            }
            else if (control == C2MinusC1NumberTypeLabel)
            {
                C2MinusC1NumericUpDown.Enabled = (C2MinusC1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
                C2MinusC1NumericUpDown.Focus();
            }
            else if (control == V2MinusV1NumberTypeLabel)
            {
                V2MinusV1NumericUpDown.Enabled = (V2MinusV1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
                V2MinusV1NumericUpDown.Focus();
            }
            else if (control == W2MinusW1NumberTypeLabel)
            {
                W2MinusW1NumericUpDown.Enabled = (W2MinusW1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
                W2MinusW1NumericUpDown.Focus();
            }
            else if (control == L2MinusL1NumberTypeLabel)
            {
                L2MinusL1NumericUpDown.Enabled = (L2MinusL1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
                L2MinusL1NumericUpDown.Focus();
            }
            else
            {
                // do nothing
            }
        }

        NumericUpDown_ValueChanged(sender, e);
    }
    private void UpdateNumberType(Control control)
    {
        if (control == null) return;

        if (ModifierKeys != Keys.Shift)
        {
            if (control.Text == "")
            {
                control.Text = "P";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[2];
                ToolTip.SetToolTip(control, "prime = divisible by itself only");
            }
            else if (control.Text == "P")
            {
                control.Text = "AP";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[3];
                ToolTip.SetToolTip(control, "additive prime = prime with a prime digit sum");
            }
            else if (control.Text == "AP")
            {
                control.Text = "XP";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[4];
                ToolTip.SetToolTip(control, "non-additive prime = prime with a composite digit sum");
            }
            else if (control.Text == "XP")
            {
                control.Text = "C";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[5];
                ToolTip.SetToolTip(control, "composite = divisible by prime(s) below it");
            }
            else if (control.Text == "C")
            {
                control.Text = "AC";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[6];
                ToolTip.SetToolTip(control, "additive composite = composite with a composite digit sum");
            }
            else if (control.Text == "AC")
            {
                control.Text = "XC";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[7];
                ToolTip.SetToolTip(control, "non-additive composite = composite with a prime digit sum");
            }
            else if (control.Text == "XC")
            {
                control.Text = "O";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[8];
                ToolTip.SetToolTip(control, "odd number");
            }
            else if (control.Text == "O")
            {
                control.Text = "E";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[9];
                ToolTip.SetToolTip(control, "even number");
            }
            else if (control.Text == "E")
            {
                control.Text = "^2";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[10];
                ToolTip.SetToolTip(control, "square number");
            }
            else if (control.Text == "^2")
            {
                control.Text = "^3";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[11];
                ToolTip.SetToolTip(control, "cubic number");
            }
            else if (control.Text == "^3")
            {
                control.Text = "^4";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[12];
                ToolTip.SetToolTip(control, "quartic number");
            }
            else if (control.Text == "^4")
            {
                control.Text = "^5";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[13];
                ToolTip.SetToolTip(control, "quintic number");
            }
            else if (control.Text == "^5")
            {
                control.Text = "^6";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[14];
                ToolTip.SetToolTip(control, "sextic number");
            }
            else if (control.Text == "^6")
            {
                control.Text = "^7";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[15];
                ToolTip.SetToolTip(control, "septic number");
            }
            else if (control.Text == "^7")
            {
                control.Text = "";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[0];
                ToolTip.SetToolTip(control, "");
            }
        }
        else // if (ModifierKeys == Keys.Shift)
        {
            if (control.Text == "")
            {
                control.Text = "^7";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[15];
                ToolTip.SetToolTip(control, "septic number");
            }
            else if (control.Text == "^7")
            {
                control.Text = "^6";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[14];
                ToolTip.SetToolTip(control, "sextic number");
            }
            else if (control.Text == "^6")
            {
                control.Text = "^5";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[13];
                ToolTip.SetToolTip(control, "quartic number");
            }
            else if (control.Text == "^5")
            {
                control.Text = "^4";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[12];
                ToolTip.SetToolTip(control, "quartic number");
            }
            else if (control.Text == "^4")
            {
                control.Text = "^3";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[11];
                ToolTip.SetToolTip(control, "cubic number");
            }
            else if (control.Text == "^3")
            {
                control.Text = "^2";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[10];
                ToolTip.SetToolTip(control, "square number");
            }
            else if (control.Text == "^2")
            {
                control.Text = "E";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[9];
                ToolTip.SetToolTip(control, "even number");
            }
            else if (control.Text == "E")
            {
                control.Text = "O";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[8];
                ToolTip.SetToolTip(control, "odd number");
            }
            else if (control.Text == "O")
            {
                control.Text = "XC";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[7];
                ToolTip.SetToolTip(control, "non-additive composite = composite with a prime digit sum");
            }
            else if (control.Text == "XC")
            {
                control.Text = "AC";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[6];
                ToolTip.SetToolTip(control, "additive composite = composite with a composite digit sum");
            }
            else if (control.Text == "AC")
            {
                control.Text = "C";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[5];
                ToolTip.SetToolTip(control, "composite = divisible by prime(s) below it");
            }
            else if (control.Text == "C")
            {
                control.Text = "XP";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[4];
                ToolTip.SetToolTip(control, "non-additive prime = prime with a composite digit sum");
            }
            else if (control.Text == "XP")
            {
                control.Text = "AP";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[3];
                ToolTip.SetToolTip(control, "additive prime = prime with a prime digit sum");
            }
            else if (control.Text == "AP")
            {
                control.Text = "P";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[2];
                ToolTip.SetToolTip(control, "prime = divisible by itself only");
            }
            else if (control.Text == "P")
            {
                control.Text = "";
                control.ForeColor = Numbers.NUMBER_TYPE_COLORS[0];
                ToolTip.SetToolTip(control, "");
            }
        }
    }
    private NumberType m_chapter_sum_number_type;
    private NumberType m_verses_number_type;
    private NumberType m_words_number_type;
    private NumberType m_letters_number_type;
    private NumberType m_value_sum_number_type;
    private NumberType m_cplusv_sum_number_type;
    private NumberType m_cminusv_sum_number_type;
    private NumberType m_ctimesv_sum_number_type;
    private NumberType m_c2minusc1_number_type;
    private NumberType m_v2minusv1_number_type;
    private NumberType m_w2minusw1_number_type;
    private NumberType m_l2minusl1_number_type;
    private void CaptureNumberTypes()
    {
        string chapter_sum_symbol = ChapterSumNumberTypeLabel.Enabled ? ChapterSumNumberTypeLabel.Text : "";
        m_chapter_sum_number_type =
           (chapter_sum_symbol == "P") ? NumberType.Prime :
           (chapter_sum_symbol == "AP") ? NumberType.AdditivePrime :
           (chapter_sum_symbol == "XP") ? NumberType.NonAdditivePrime :
           (chapter_sum_symbol == "C") ? NumberType.Composite :
           (chapter_sum_symbol == "AC") ? NumberType.AdditiveComposite :
           (chapter_sum_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (chapter_sum_symbol == "O") ? NumberType.Odd :
           (chapter_sum_symbol == "E") ? NumberType.Even :
           (chapter_sum_symbol == "^2") ? NumberType.Square :
           (chapter_sum_symbol == "^3") ? NumberType.Cubic :
           (chapter_sum_symbol == "^4") ? NumberType.Quartic :
           (chapter_sum_symbol == "^5") ? NumberType.Quintic :
           (chapter_sum_symbol == "^6") ? NumberType.Sextic :
           (chapter_sum_symbol == "^7") ? NumberType.Septic :
           (chapter_sum_symbol == "") ? NumberType.None :
                                        NumberType.Natural;
        string verses_symbol = VersesNumberTypeLabel.Enabled ? VersesNumberTypeLabel.Text : "";
        m_verses_number_type =
           (verses_symbol == "P") ? NumberType.Prime :
           (verses_symbol == "AP") ? NumberType.AdditivePrime :
           (verses_symbol == "XP") ? NumberType.NonAdditivePrime :
           (verses_symbol == "C") ? NumberType.Composite :
           (verses_symbol == "AC") ? NumberType.AdditiveComposite :
           (verses_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (verses_symbol == "O") ? NumberType.Odd :
           (verses_symbol == "E") ? NumberType.Even :
           (verses_symbol == "^2") ? NumberType.Square :
           (verses_symbol == "^3") ? NumberType.Cubic :
           (verses_symbol == "^4") ? NumberType.Quartic :
           (verses_symbol == "^5") ? NumberType.Quintic :
           (verses_symbol == "^6") ? NumberType.Sextic :
           (verses_symbol == "^7") ? NumberType.Septic :
           (verses_symbol == "") ? NumberType.None :
                                   NumberType.Natural;
        string words_symbol = WordsNumberTypeLabel.Enabled ? WordsNumberTypeLabel.Text : "";
        m_words_number_type =
           (words_symbol == "P") ? NumberType.Prime :
           (words_symbol == "AP") ? NumberType.AdditivePrime :
           (words_symbol == "XP") ? NumberType.NonAdditivePrime :
           (words_symbol == "C") ? NumberType.Composite :
           (words_symbol == "AC") ? NumberType.AdditiveComposite :
           (words_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (words_symbol == "O") ? NumberType.Odd :
           (words_symbol == "E") ? NumberType.Even :
           (words_symbol == "^2") ? NumberType.Square :
           (words_symbol == "^3") ? NumberType.Cubic :
           (words_symbol == "^4") ? NumberType.Quartic :
           (words_symbol == "^5") ? NumberType.Quintic :
           (words_symbol == "^6") ? NumberType.Sextic :
           (words_symbol == "^7") ? NumberType.Septic :
           (words_symbol == "") ? NumberType.None :
                                  NumberType.Natural;
        string letters_symbol = LettersNumberTypeLabel.Enabled ? LettersNumberTypeLabel.Text : "";
        m_letters_number_type =
           (letters_symbol == "P") ? NumberType.Prime :
           (letters_symbol == "AP") ? NumberType.AdditivePrime :
           (letters_symbol == "XP") ? NumberType.NonAdditivePrime :
           (letters_symbol == "C") ? NumberType.Composite :
           (letters_symbol == "AC") ? NumberType.AdditiveComposite :
           (letters_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (letters_symbol == "O") ? NumberType.Odd :
           (letters_symbol == "E") ? NumberType.Even :
           (letters_symbol == "^2") ? NumberType.Square :
           (letters_symbol == "^3") ? NumberType.Cubic :
           (letters_symbol == "^4") ? NumberType.Quartic :
           (letters_symbol == "^5") ? NumberType.Quintic :
           (letters_symbol == "^6") ? NumberType.Sextic :
           (letters_symbol == "^7") ? NumberType.Septic :
           (letters_symbol == "") ? NumberType.None :
                                    NumberType.Natural;
        string value_sum_symbol = ValueNumberTypeLabel.Enabled ? ValueNumberTypeLabel.Text : "";
        m_value_sum_number_type =
           (value_sum_symbol == "P") ? NumberType.Prime :
           (value_sum_symbol == "AP") ? NumberType.AdditivePrime :
           (value_sum_symbol == "XP") ? NumberType.NonAdditivePrime :
           (value_sum_symbol == "C") ? NumberType.Composite :
           (value_sum_symbol == "AC") ? NumberType.AdditiveComposite :
           (value_sum_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (value_sum_symbol == "O") ? NumberType.Odd :
           (value_sum_symbol == "E") ? NumberType.Even :
           (value_sum_symbol == "^2") ? NumberType.Square :
           (value_sum_symbol == "^3") ? NumberType.Cubic :
           (value_sum_symbol == "^4") ? NumberType.Quartic :
           (value_sum_symbol == "^5") ? NumberType.Quintic :
           (value_sum_symbol == "^6") ? NumberType.Sextic :
           (value_sum_symbol == "^7") ? NumberType.Septic :
           (value_sum_symbol == "") ? NumberType.None :
                                      NumberType.Natural;
        string cplusv_sum_symbol = CPlusVSumNumberTypeLabel.Enabled ? CPlusVSumNumberTypeLabel.Text : "";
        m_cplusv_sum_number_type =
           (cplusv_sum_symbol == "P") ? NumberType.Prime :
           (cplusv_sum_symbol == "AP") ? NumberType.AdditivePrime :
           (cplusv_sum_symbol == "XP") ? NumberType.NonAdditivePrime :
           (cplusv_sum_symbol == "C") ? NumberType.Composite :
           (cplusv_sum_symbol == "AC") ? NumberType.AdditiveComposite :
           (cplusv_sum_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (cplusv_sum_symbol == "O") ? NumberType.Odd :
           (cplusv_sum_symbol == "E") ? NumberType.Even :
           (cplusv_sum_symbol == "^2") ? NumberType.Square :
           (cplusv_sum_symbol == "^3") ? NumberType.Cubic :
           (cplusv_sum_symbol == "^4") ? NumberType.Quartic :
           (cplusv_sum_symbol == "^5") ? NumberType.Quintic :
           (cplusv_sum_symbol == "^6") ? NumberType.Sextic :
           (cplusv_sum_symbol == "^7") ? NumberType.Septic :
           (cplusv_sum_symbol == "") ? NumberType.None :
                                       NumberType.Natural;
        string cminusv_sum_symbol = CMinusVSumNumberTypeLabel.Enabled ? CMinusVSumNumberTypeLabel.Text : "";
        m_cminusv_sum_number_type =
           (cminusv_sum_symbol == "P") ? NumberType.Prime :
           (cminusv_sum_symbol == "AP") ? NumberType.AdditivePrime :
           (cminusv_sum_symbol == "XP") ? NumberType.NonAdditivePrime :
           (cminusv_sum_symbol == "C") ? NumberType.Composite :
           (cminusv_sum_symbol == "AC") ? NumberType.AdditiveComposite :
           (cminusv_sum_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (cminusv_sum_symbol == "O") ? NumberType.Odd :
           (cminusv_sum_symbol == "E") ? NumberType.Even :
           (cminusv_sum_symbol == "^2") ? NumberType.Square :
           (cminusv_sum_symbol == "^3") ? NumberType.Cubic :
           (cminusv_sum_symbol == "^4") ? NumberType.Quartic :
           (cminusv_sum_symbol == "^5") ? NumberType.Quintic :
           (cminusv_sum_symbol == "^6") ? NumberType.Sextic :
           (cminusv_sum_symbol == "^7") ? NumberType.Septic :
           (cminusv_sum_symbol == "") ? NumberType.None :
                                        NumberType.Natural;
        string ctimesv_sum_symbol = CTimesVSumNumberTypeLabel.Enabled ? CTimesVSumNumberTypeLabel.Text : "";
        m_ctimesv_sum_number_type =
           (ctimesv_sum_symbol == "P") ? NumberType.Prime :
           (ctimesv_sum_symbol == "AP") ? NumberType.AdditivePrime :
           (ctimesv_sum_symbol == "XP") ? NumberType.NonAdditivePrime :
           (ctimesv_sum_symbol == "C") ? NumberType.Composite :
           (ctimesv_sum_symbol == "AC") ? NumberType.AdditiveComposite :
           (ctimesv_sum_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (ctimesv_sum_symbol == "O") ? NumberType.Odd :
           (ctimesv_sum_symbol == "E") ? NumberType.Even :
           (ctimesv_sum_symbol == "^2") ? NumberType.Square :
           (ctimesv_sum_symbol == "^3") ? NumberType.Cubic :
           (ctimesv_sum_symbol == "^4") ? NumberType.Quartic :
           (ctimesv_sum_symbol == "^5") ? NumberType.Quintic :
           (ctimesv_sum_symbol == "^6") ? NumberType.Sextic :
           (ctimesv_sum_symbol == "^7") ? NumberType.Septic :
           (ctimesv_sum_symbol == "") ? NumberType.None :
                                        NumberType.Natural;
        string c2minusc1_symbol = C2MinusC1NumberTypeLabel.Enabled ? C2MinusC1NumberTypeLabel.Text : "";
        m_c2minusc1_number_type =
           (c2minusc1_symbol == "P") ? NumberType.Prime :
           (c2minusc1_symbol == "AP") ? NumberType.AdditivePrime :
           (c2minusc1_symbol == "XP") ? NumberType.NonAdditivePrime :
           (c2minusc1_symbol == "C") ? NumberType.Composite :
           (c2minusc1_symbol == "AC") ? NumberType.AdditiveComposite :
           (c2minusc1_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (c2minusc1_symbol == "O") ? NumberType.Odd :
           (c2minusc1_symbol == "E") ? NumberType.Even :
           (c2minusc1_symbol == "^2") ? NumberType.Square :
           (c2minusc1_symbol == "^3") ? NumberType.Cubic :
           (c2minusc1_symbol == "^4") ? NumberType.Quartic :
           (c2minusc1_symbol == "^5") ? NumberType.Quintic :
           (c2minusc1_symbol == "^6") ? NumberType.Sextic :
           (c2minusc1_symbol == "^7") ? NumberType.Septic :
           (c2minusc1_symbol == "") ? NumberType.None :
                                      NumberType.Natural;
        string v2minusv1_symbol = V2MinusV1NumberTypeLabel.Enabled ? V2MinusV1NumberTypeLabel.Text : "";
        m_v2minusv1_number_type =
           (v2minusv1_symbol == "P") ? NumberType.Prime :
           (v2minusv1_symbol == "AP") ? NumberType.AdditivePrime :
           (v2minusv1_symbol == "XP") ? NumberType.NonAdditivePrime :
           (v2minusv1_symbol == "C") ? NumberType.Composite :
           (v2minusv1_symbol == "AC") ? NumberType.AdditiveComposite :
           (v2minusv1_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (v2minusv1_symbol == "O") ? NumberType.Odd :
           (v2minusv1_symbol == "E") ? NumberType.Even :
           (v2minusv1_symbol == "^2") ? NumberType.Square :
           (v2minusv1_symbol == "^3") ? NumberType.Cubic :
           (v2minusv1_symbol == "^4") ? NumberType.Quartic :
           (v2minusv1_symbol == "^5") ? NumberType.Quintic :
           (v2minusv1_symbol == "^6") ? NumberType.Sextic :
           (v2minusv1_symbol == "^7") ? NumberType.Septic :
           (v2minusv1_symbol == "") ? NumberType.None :
                                      NumberType.Natural;
        string w2minusw1_symbol = W2MinusW1NumberTypeLabel.Enabled ? W2MinusW1NumberTypeLabel.Text : "";
        m_w2minusw1_number_type =
           (w2minusw1_symbol == "P") ? NumberType.Prime :
           (w2minusw1_symbol == "AP") ? NumberType.AdditivePrime :
           (w2minusw1_symbol == "XP") ? NumberType.NonAdditivePrime :
           (w2minusw1_symbol == "C") ? NumberType.Composite :
           (w2minusw1_symbol == "AC") ? NumberType.AdditiveComposite :
           (w2minusw1_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (w2minusw1_symbol == "O") ? NumberType.Odd :
           (w2minusw1_symbol == "E") ? NumberType.Even :
           (w2minusw1_symbol == "^2") ? NumberType.Square :
           (w2minusw1_symbol == "^3") ? NumberType.Cubic :
           (w2minusw1_symbol == "^4") ? NumberType.Quartic :
           (w2minusw1_symbol == "^5") ? NumberType.Quintic :
           (w2minusw1_symbol == "^6") ? NumberType.Sextic :
           (w2minusw1_symbol == "^7") ? NumberType.Septic :
           (w2minusw1_symbol == "") ? NumberType.None :
                                      NumberType.Natural;
        string l2minusl1_symbol = L2MinusL1NumberTypeLabel.Enabled ? L2MinusL1NumberTypeLabel.Text : "";
        m_l2minusl1_number_type =
           (l2minusl1_symbol == "P") ? NumberType.Prime :
           (l2minusl1_symbol == "AP") ? NumberType.AdditivePrime :
           (l2minusl1_symbol == "XP") ? NumberType.NonAdditivePrime :
           (l2minusl1_symbol == "C") ? NumberType.Composite :
           (l2minusl1_symbol == "AC") ? NumberType.AdditiveComposite :
           (l2minusl1_symbol == "XC") ? NumberType.NonAdditiveComposite :
           (l2minusl1_symbol == "O") ? NumberType.Odd :
           (l2minusl1_symbol == "E") ? NumberType.Even :
           (l2minusl1_symbol == "^2") ? NumberType.Square :
           (l2minusl1_symbol == "^3") ? NumberType.Cubic :
           (l2minusl1_symbol == "^4") ? NumberType.Quartic :
           (l2minusl1_symbol == "^5") ? NumberType.Quintic :
           (l2minusl1_symbol == "^6") ? NumberType.Sextic :
           (l2minusl1_symbol == "^7") ? NumberType.Septic :
           (l2minusl1_symbol == "") ? NumberType.None :
                                      NumberType.Natural;

        // ignore given values if number type is given
        if (m_chapter_sum_number_type != NumberType.None) m_chapter_sum = -1;
        if (m_verses_number_type != NumberType.None) m_verse_sum = -1;
        if (m_words_number_type != NumberType.None) m_word_sum = -1;
        if (m_letters_number_type != NumberType.None) m_letter_sum = -1;
        if (m_value_sum_number_type != NumberType.None) m_value_sum = -1L;
        if (m_cplusv_sum_number_type != NumberType.None) m_cplusv_sum = -1;
        if (m_cminusv_sum_number_type != NumberType.None) m_cminusv_sum = -1;
        if (m_ctimesv_sum_number_type != NumberType.None) m_ctimesv_sum = -1;
        if (m_c2minusc1_number_type != NumberType.None) m_c2minusc1 = -1;
        if (m_v2minusv1_number_type != NumberType.None) m_v2minusv1 = -1;
        if (m_w2minusw1_number_type != NumberType.None) m_w2minusw1 = -1;
        if (m_l2minusl1_number_type != NumberType.None) m_l2minusl1 = -1;
    }

    private static bool s_abs_c_minus_v_sum = false;
    public static bool AbsCMinusVSum
    {
        get { return s_abs_c_minus_v_sum; }
    }
    private void CMinusVSumLabel_Click(object sender, EventArgs e)
    {
        s_abs_c_minus_v_sum = !s_abs_c_minus_v_sum;
        CMinusVSumLabel.Text = s_abs_c_minus_v_sum ? "∑(|C - V|)" : "∑(C - V)";
    }
    private static bool s_abs_c2_minus_c1 = false;
    public static bool AbsC2MinusC1
    {
        get { return s_abs_c2_minus_c1; }
    }
    private void C2MinusC1Label_Click(object sender, EventArgs e)
    {
        s_abs_c2_minus_c1 = !s_abs_c2_minus_c1;
        C2MinusC1Label.Text = s_abs_c2_minus_c1 ? "|C2 - C1|" : "C2 - C1";
    }
    private static bool s_abs_v2_minus_v1 = false;
    public static bool AbsV2MinusV1
    {
        get { return s_abs_v2_minus_v1; }
    }
    private void V2MinusV1Label_Click(object sender, EventArgs e)
    {
        s_abs_v2_minus_v1 = !s_abs_v2_minus_v1;
        V2MinusV1Label.Text = s_abs_v2_minus_v1 ? "|V2 - V1|" : "V2 - V1";
    }
    private static bool s_abs_w2_minus_w1 = false;
    public static bool AbsW2MinusW1
    {
        get { return s_abs_w2_minus_w1; }
    }
    private void W2MinusW1Label_Click(object sender, EventArgs e)
    {
        s_abs_w2_minus_w1 = !s_abs_w2_minus_w1;
        W2MinusW1Label.Text = s_abs_w2_minus_w1 ? "|W2 - W1|" : "W2 - W1";
    }
    private static bool s_abs_l2_minus_l1 = false;
    public static bool AbsL2MinusL1
    {
        get { return s_abs_l2_minus_l1; }
    }
    private void L2MinusL1Label_Click(object sender, EventArgs e)
    {
        s_abs_l2_minus_l1 = !s_abs_l2_minus_l1;
        L2MinusL1Label.Text = s_abs_l2_minus_l1 ? "|L2 - L1|" : "L2 - L1";
    }

    private bool m_count_only = true;
    private void CountOnlyCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_count_only = CountOnlyCheckBox.Checked;

        ChapterOutputFieldCheckBox.Enabled = !m_count_only;
        VersesOutputFieldCheckBox.Enabled = !m_count_only;
        WordsOutputFieldCheckBox.Enabled = !m_count_only;
        LettersOutputFieldCheckBox.Enabled = !m_count_only;
        ValueOutputFieldCheckBox.Enabled = !m_count_only;
        OutputFormatZeroPadComboBox.Enabled = !m_count_only;
        OutputFormatFieldSeparatorComboBox.Enabled = !m_count_only;
        OutputFormatChapterSeparatorComboBox.Enabled = !m_count_only;

        FindButton.Text = m_count_only ? "&Count" : "&Find";
        ToolTip.SetToolTip(this.FindButton, (m_count_only ? "عد المجموعات المطابقة فقط" : "جد وأعرض المجموعات المطابقة"));
    }
    private void FindButton_Click(object sender, EventArgs e)
    {
        if (s_running) // stop
        {
            Finish(true);
        }
        else // start
        {
            Start();
        }
    }
    private SubsetFinder m_subset_finder = null;
    private long m_subsets = 0L;
    private DateTime m_start_time;
    private Control m_focused_control = null;
    private static long s_progress = 0L;
    public static long Progress
    {
        get { return s_progress; }
        set { s_progress = value; }
    }
    private static bool s_running = false;
    public static bool Running
    {
        get { return s_running; }
    }
    private void Start()
    {
        m_focused_control = this.ActiveControl;
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (!s_running)
            {
                m_start_time = DateTime.Now;

                s_running = true;
                FindButton.Text = "&Cancel";
                ToolTip.SetToolTip(this.FindButton, "إلغاء");
                FindButton.Refresh();

                CountOnlyCheckBox.Visible = false;
                CountOnlyCheckBox.Enabled = false;
                CountOnlyCheckBox.Refresh();

                ChapterOutputFieldCheckBox.Enabled = false;
                VersesOutputFieldCheckBox.Enabled = false;
                WordsOutputFieldCheckBox.Enabled = false;
                LettersOutputFieldCheckBox.Enabled = false;
                ValueOutputFieldCheckBox.Enabled = false;
                OutputFormatZeroPadComboBox.Enabled = false;
                OutputFormatFieldSeparatorComboBox.Enabled = false;
                OutputFormatChapterSeparatorComboBox.Enabled = false;

                SaveMatchesButton.Enabled = false;

                ChaptersNumericUpDown.Enabled = false;
                ChapterSumNumericUpDown.Enabled = false;
                VerseSumNumericUpDown.Enabled = false;
                WordSumNumericUpDown.Enabled = false;
                LetterSumNumericUpDown.Enabled = false;
                ValueSumNumericUpDown.Enabled = false;
                CPlusVSumNumericUpDown.Enabled = false;
                CMinusVSumNumericUpDown.Enabled = false;
                CTimesVSumNumericUpDown.Enabled = false;
                C2MinusC1NumericUpDown.Enabled = false;
                V2MinusV1NumericUpDown.Enabled = false;
                W2MinusW1NumericUpDown.Enabled = false;
                L2MinusL1NumericUpDown.Enabled = false;

                ChapterSumNumberTypeLabel.BackColor = SystemColors.Control;
                VersesNumberTypeLabel.BackColor = SystemColors.Control;
                WordsNumberTypeLabel.BackColor = SystemColors.Control;
                LettersNumberTypeLabel.BackColor = SystemColors.Control;
                ValueNumberTypeLabel.BackColor = SystemColors.Control;
                CPlusVSumNumberTypeLabel.BackColor = SystemColors.Control;
                CMinusVSumNumberTypeLabel.BackColor = SystemColors.Control;
                CTimesVSumNumberTypeLabel.BackColor = SystemColors.Control;
                C2MinusC1NumberTypeLabel.BackColor = SystemColors.Control;
                V2MinusV1NumberTypeLabel.BackColor = SystemColors.Control;
                W2MinusW1NumberTypeLabel.BackColor = SystemColors.Control;
                L2MinusL1NumberTypeLabel.BackColor = SystemColors.Control;

                MatchesTextBox.Text = "";
                MatchesTextBox.ForeColor = Numbers.GetNumberForeColor(0L);
                MatchesTextBox.BackColor = MatchesTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
                MatchesTextBox.Refresh();

                ProgressBar.Value = 0;
                ElapsedTimeValueLabel.Text = "0d 00h 00m 00s 000ms";

                // Just before Run
                CaptureQueryParameters();
                CaptureNumberTypes();

                NumberQuery query = new NumberQuery();
                query.Chapters = m_chapter_count;
                query.ChapterSum = m_chapter_sum;
                query.Verses = m_verse_sum;
                query.Words = m_word_sum;
                query.Letters = m_letter_sum;
                query.Value = m_value_sum;
                query.CPlusVSum = m_cplusv_sum;
                query.CMinusVSum = m_cminusv_sum;
                query.CTimesVSum = m_ctimesv_sum;
                query.C2MinusC1 = m_c2minusc1;
                query.V2MinusV1 = m_v2minusv1;
                query.W2MinusW1 = m_w2minusw1;
                query.L2MinusL1 = m_l2minusl1;

                query.ChapterSumNumberType = m_chapter_sum_number_type;
                query.VersesNumberType = m_verses_number_type;
                query.WordsNumberType = m_words_number_type;
                query.LettersNumberType = m_letters_number_type;
                query.ValueNumberType = m_value_sum_number_type;
                query.CPlusVSumNumberType = m_cplusv_sum_number_type;
                query.CMinusVSumNumberType = m_cminusv_sum_number_type;
                query.CTimesVSumNumberType = m_ctimesv_sum_number_type;
                query.C2MinusC1NumberType = m_c2minusc1_number_type;
                query.V2MinusV1NumberType = m_v2minusv1_number_type;
                query.W2MinusW1NumberType = m_w2minusw1_number_type;
                query.L2MinusL1NumberType = m_l2minusl1_number_type;

                // Run
                m_subsets = 0L;
                m_matches_str = new StringBuilder();
                m_subset_finder = new SubsetFinder(m_client, query);
                if (m_subset_finder != null)
                {
                    if (m_count_only)
                    {
                        m_subsets = m_subset_finder.Count(m_chapter_count, m_chapter_sum);
                    }
                    else
                    {
                        m_subset_finder.Find(m_chapter_count, m_chapter_sum, OnFound);
                    }
                }

                if (s_running) // if finished normally
                {
                    Finish(false);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            Finish(true);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void Finish(bool cancelled)
    {
        if (s_running)
        {
            s_running = false;

            FindButton.Text = m_count_only ? "&Count" : "&Find";
            ToolTip.SetToolTip(this.FindButton, (m_count_only ? "عد المجموعات المطابقة فقط" : "جد وأعرض المجموعات المطابقة"));
            FindButton.Refresh();

            CountOnlyCheckBox.Visible = true;
            CountOnlyCheckBox.Enabled = true;
            CountOnlyCheckBox.Refresh();

            ChapterOutputFieldCheckBox.Enabled = !m_count_only;
            VersesOutputFieldCheckBox.Enabled = !m_count_only;
            WordsOutputFieldCheckBox.Enabled = !m_count_only;
            LettersOutputFieldCheckBox.Enabled = !m_count_only;
            ValueOutputFieldCheckBox.Enabled = !m_count_only;
            OutputFormatZeroPadComboBox.Enabled = !m_count_only;
            OutputFormatFieldSeparatorComboBox.Enabled = !m_count_only;
            OutputFormatChapterSeparatorComboBox.Enabled = !m_count_only;

            ChaptersNumericUpDown.Enabled = true;
            ChapterSumNumericUpDown.Enabled = (ChapterSumNumberTypeLabel.Text == "");
            VerseSumNumericUpDown.Enabled = (VersesNumberTypeLabel.Text == "");
            WordSumNumericUpDown.Enabled = (WordsNumberTypeLabel.Text == "");
            LetterSumNumericUpDown.Enabled = (LettersNumberTypeLabel.Text == "");
            ValueSumNumericUpDown.Enabled = (ValueNumberTypeLabel.Text == "");
            CPlusVSumNumericUpDown.Enabled = (CPlusVSumNumberTypeLabel.Text == "");
            CMinusVSumNumericUpDown.Enabled = (CMinusVSumNumberTypeLabel.Text == "");
            CTimesVSumNumericUpDown.Enabled = (CTimesVSumNumberTypeLabel.Text == "");
            C2MinusC1NumericUpDown.Enabled = (C2MinusC1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
            V2MinusV1NumericUpDown.Enabled = (V2MinusV1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
            W2MinusW1NumericUpDown.Enabled = (W2MinusW1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);
            L2MinusL1NumericUpDown.Enabled = (L2MinusL1NumberTypeLabel.Text == "") && (ChaptersNumericUpDown.Value == 2);

            ChapterSumNumberTypeLabel.BackColor = SystemColors.Window;
            VersesNumberTypeLabel.BackColor = SystemColors.Window;
            WordsNumberTypeLabel.BackColor = SystemColors.Window;
            LettersNumberTypeLabel.BackColor = SystemColors.Window;
            ValueNumberTypeLabel.BackColor = SystemColors.Window;
            CPlusVSumNumberTypeLabel.BackColor = SystemColors.Window;
            CMinusVSumNumberTypeLabel.BackColor = SystemColors.Window;
            CTimesVSumNumberTypeLabel.BackColor = SystemColors.Window;
            if (ChaptersNumericUpDown.Value == 2) C2MinusC1NumberTypeLabel.BackColor = SystemColors.Window;
            if (ChaptersNumericUpDown.Value == 2) V2MinusV1NumberTypeLabel.BackColor = SystemColors.Window;
            if (ChaptersNumericUpDown.Value == 2) W2MinusW1NumberTypeLabel.BackColor = SystemColors.Window;
            if (ChaptersNumericUpDown.Value == 2) L2MinusL1NumberTypeLabel.BackColor = SystemColors.Window;

            if (m_focused_control != null)
            {
                m_focused_control.Focus();
            }

            if (m_count_only)
            {
                MatchesTextBox.Text = m_subsets.ToString();
                MatchesTextBox.ForeColor = Numbers.GetNumberForeColor(m_subsets);
                MatchesTextBox.BackColor = MatchesTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly

                UpdateMatchResults();

                SaveMatchesButton.Enabled = false;
            }
            else // save matches
            {
                if (m_matches_str == null)
                {
                    SaveMatchesButton.Enabled = false;
                }
                else
                {
                    if (m_matches_str.Length > 0)
                    {
                        SaveMatchesButton.Enabled = true;
                        if (!cancelled)
                        {
                            SaveMatchesButton_Click(null, null);
                        }
                    }
                    else
                    {
                        SaveMatchesButton.Enabled = false;
                    }
                }
            }

            UpdateElapsedTime(m_subsets);
        }
    }
    private void UpdateElapsedTime(long progress)
    {
        if (s_running)
        {
            ProgressBar.Value = (int)((s_progress * ProgressBar.Maximum) / m_combinations);
        }
        else
        {
            ProgressBar.Value = ProgressBar.Maximum;
        }
        ProgressBar.Refresh();

        TimeSpan elapsed_time = DateTime.Now - m_start_time;
        ElapsedTimeValueLabel.Text = String.Format("{0:d1}d {1:d2}h {2:d2}m {3:d2}s {4:d3}ms", elapsed_time.Days, elapsed_time.Hours, elapsed_time.Minutes, elapsed_time.Seconds, elapsed_time.Milliseconds);
        ElapsedTimeValueLabel.Refresh();
    }
    private StringBuilder m_matches_str = null;
    private void OnFound(Chapter[] chapter_set)
    {
        if (chapter_set != null)
        {
            if (m_subset_finder != null)
            {
                m_subsets++;

                if (!m_count_only)
                {
                    MatchesTextBox.Text = m_subsets.ToString();
                    MatchesTextBox.ForeColor = Numbers.GetNumberForeColor(m_subsets);
                    MatchesTextBox.BackColor = MatchesTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
                    MatchesTextBox.Refresh();

                    UpdateMatchResults();

                    StringBuilder str = new StringBuilder();
                    for (int i = chapter_set.Length - 1; i >= 0; i--)
                    {
                        str.Append((m_include_chapters_output_field ? (chapter_set[i].Number.ToString(m_chapter_number_format)) : "") +
                                   (m_include_verses_output_field ? ((m_include_chapters_output_field ? m_field_seperator : "") + chapter_set[i].Verses.Count.ToString(m_verses_number_format)) : "") +
                                   (m_include_words_output_field ? ((m_include_chapters_output_field || m_include_verses_output_field ? m_field_seperator : "") + chapter_set[i].Words.Count.ToString(m_words_number_format)) : "") +
                                   (m_include_letters_output_field ? ((m_include_chapters_output_field || m_include_verses_output_field || m_include_words_output_field ? m_field_seperator : "") + chapter_set[i].Letters.Count.ToString(m_letters_number_format)) : "") +
                                   (m_include_value_output_field ? ((m_include_chapters_output_field || m_include_verses_output_field || m_include_words_output_field || m_include_letters_output_field ? m_field_seperator : "") + chapter_set[i].Value.ToString(m_value_number_format)) : "") +
                                   m_chapter_seperator);
                    }
                    if (str.Length > 0)
                    {
                        if (m_chapter_seperator != "\r\n")
                        {
                            str.Remove(str.Length - m_chapter_seperator.Length, m_chapter_seperator.Length);
                        }
                    }
                    m_matches_str.Insert(0, str.ToString() + "\r\n");

                    UpdateElapsedTime(m_subsets);
                }
            }
        }
    }
    private void UpdateMatchResults()
    {
        if (m_subset_finder != null)
        {
            List<Chapter> chapters = m_subset_finder.UniqueMatchChapters;
            if (chapters != null)
            {
                chapters.Sort();

                int match_chapters = 0;
                int match_chapter_sum = 0;
                int match_verses = 0;
                int match_words = 0;
                int match_letters = 0;
                long match_value = 0;

                StringBuilder str = new StringBuilder();
                match_chapters = chapters.Count;
                foreach (Chapter chapter in chapters)
                {
                    match_chapter_sum += chapter.SortedNumber;
                    match_verses += chapter.Verses.Count;
                    match_words += chapter.Words.Count;
                    match_letters += chapter.Letters.Count;
                    match_value += chapter.Value;
                    str.Append(chapter.SortedNumber.ToString() + ", ");
                }
                if (str.Length > 0)
                {
                    str.Remove(str.Length - 2, 2); // ", "
                    MatchChapterListTextBox.Text = str.ToString();
                    MatchChapterListTextBox.ForeColor = Numbers.GetNumberForeColor(match_chapters);
                    ToolTip.SetToolTip(this.MatchChapterListTextBox, "Copy and paste into the\r\nChapter field of QuranCode\r\nto study further ...");
                }

                MatchChaptersTextBox.Text = match_chapters.ToString();
                MatchChaptersTextBox.ForeColor = Numbers.GetNumberForeColor(match_chapters);
                MatchChaptersTextBox.BackColor = MatchChaptersTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly

                MatchChapterSumTextBox.Text = match_chapter_sum.ToString();
                MatchChapterSumTextBox.ForeColor = Numbers.GetNumberForeColor(match_chapter_sum);
                MatchChapterSumTextBox.BackColor = MatchChapterSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly

                MatchVerseSumTextBox.Text = match_verses.ToString();
                MatchVerseSumTextBox.ForeColor = Numbers.GetNumberForeColor(match_verses);
                MatchVerseSumTextBox.BackColor = MatchVerseSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly

                MatchWordSumTextBox.Text = match_words.ToString();
                MatchWordSumTextBox.ForeColor = Numbers.GetNumberForeColor(match_words);
                MatchWordSumTextBox.BackColor = MatchWordSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly

                MatchLetterSumTextBox.Text = match_letters.ToString();
                MatchLetterSumTextBox.ForeColor = Numbers.GetNumberForeColor(match_letters);
                MatchLetterSumTextBox.BackColor = MatchLetterSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly

                MatchValueSumTextBox.Text = match_value.ToString();
                MatchValueSumTextBox.ForeColor = Numbers.GetNumberForeColor(match_value);
                MatchValueSumTextBox.BackColor = MatchValueSumTextBox.BackColor; // FIX to allow change of ForeColor while ReadOnly
            }
        }
    }

    private bool m_zero_padded_format = true;
    private string m_field_seperator = ".";
    private string m_chapter_seperator = "\t";
    private string m_chapter_number_format;
    private string m_verses_number_format;
    private string m_words_number_format;
    private string m_letters_number_format;
    private string m_value_number_format;
    private void PopulateOutputFormatComboBox()
    {
        OutputFormatZeroPadComboBox.Items.Clear();
        OutputFormatZeroPadComboBox.Items.Add("000");
        OutputFormatZeroPadComboBox.Items.Add("0");
        OutputFormatZeroPadComboBox.SelectedIndex = 0;

        OutputFormatFieldSeparatorComboBox.Items.Clear();
        OutputFormatFieldSeparatorComboBox.Items.Add("Dot");
        OutputFormatFieldSeparatorComboBox.Items.Add("Space");
        OutputFormatFieldSeparatorComboBox.Items.Add("Comma");
        OutputFormatFieldSeparatorComboBox.Items.Add("Dash");
        OutputFormatFieldSeparatorComboBox.Items.Add("Tab");
        OutputFormatFieldSeparatorComboBox.SelectedIndex = 0;

        OutputFormatChapterSeparatorComboBox.Items.Clear();
        OutputFormatChapterSeparatorComboBox.Items.Add("Tab");
        OutputFormatChapterSeparatorComboBox.Items.Add("TabTab");
        OutputFormatChapterSeparatorComboBox.Items.Add("NewLine");
        OutputFormatChapterSeparatorComboBox.SelectedIndex = 0;
    }
    private void OutputFormatZeroPadComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (OutputFormatZeroPadComboBox.SelectedIndex)
        {
            case 0:
                m_zero_padded_format = true;
                break;
            case 1:
                m_zero_padded_format = false;
                break;
            default:
                break;
        }

        m_chapter_number_format = m_zero_padded_format ? "000" : "";
        m_verses_number_format = m_zero_padded_format ? "000" : "";
        m_words_number_format = m_zero_padded_format ? "0000" : "";
        m_letters_number_format = m_zero_padded_format ? "00000" : "";
        m_value_number_format = m_zero_padded_format ? "000000" : "";
    }
    private void OutputFormatFieldSeparatorComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (OutputFormatFieldSeparatorComboBox.SelectedIndex)
        {
            case 0:
                m_field_seperator = ".";
                break;
            case 1:
                m_field_seperator = " ";
                break;
            case 2:
                m_field_seperator = ",";
                break;
            case 3:
                m_field_seperator = "-";
                break;
            case 4:
                m_field_seperator = "\t";
                break;
            default:
                break;
        }
    }
    private void OutputFormatChapterSeparatorComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (OutputFormatChapterSeparatorComboBox.SelectedIndex)
        {
            case 0:
                m_chapter_seperator = "\t";
                break;
            case 1:
                m_chapter_seperator = "\t\t";
                break;
            case 2:
                m_chapter_seperator = "\r\n";
                break;
            default:
                break;
        }
    }
    private bool m_include_chapters_output_field = true;
    private bool m_include_verses_output_field = true;
    private bool m_include_words_output_field = true;
    private bool m_include_letters_output_field = true;
    private bool m_include_value_output_field = true;
    private void ChapterOutputFieldCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_include_chapters_output_field = ChapterOutputFieldCheckBox.Checked;
    }
    private void VersesOutputFieldCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_include_verses_output_field = VersesOutputFieldCheckBox.Checked;
    }
    private void WordsOutputFieldCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_include_words_output_field = WordsOutputFieldCheckBox.Checked;
    }
    private void LettersOutputFieldCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_include_letters_output_field = LettersOutputFieldCheckBox.Checked;
    }
    private void ValueOutputFieldCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        m_include_value_output_field = ValueOutputFieldCheckBox.Checked;
    }
    private const string STATISTIC_FOLDER = "Statistics";
    private void SaveMatchesButton_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (m_matches_str != null)
            {
                if ((MatchesTextBox.Text != "") && (MatchesTextBox.Text != "0"))
                {
                    string filename = "QuranLab" + "_" +
                                      ((m_chapter_count != -1) ? ("Cs=" + m_chapter_count.ToString() + "_") : "") +
                                      ((m_chapter_sum != -1) ? ("Csum=" + m_chapter_sum.ToString() + "_") : "") +
                                      ((m_cplusv_sum != -1) ? ("C+V=" + m_cplusv_sum.ToString() + "_") : "") +
                                      ((m_cminusv_sum != -1) ? ("C-V=" + m_cminusv_sum.ToString() + "_") : "") +
                                      ((m_ctimesv_sum != -1) ? ("CxV=" + m_ctimesv_sum.ToString() + "_") : "") +
                                      ((m_verse_sum != -1) ? ("Vs=" + m_verse_sum.ToString() + "_") : "") +
                                      ((m_word_sum != -1) ? ("Ws=" + m_word_sum.ToString() + "_") : "") +
                                      ((m_letter_sum != -1) ? ("Ls=" + m_letter_sum.ToString() + "_") : "") +
                                      ((m_value_sum != -1) ? ("N=" + m_value_sum.ToString() + "_") : "") +
                                      ".txt";

                    if (!Directory.Exists(STATISTIC_FOLDER))
                    {
                        Directory.CreateDirectory(STATISTIC_FOLDER);
                    }
                    string path = STATISTIC_FOLDER + Path.DirectorySeparatorChar + filename;

                    FileHelper.SaveText(path, m_matches_str.ToString());
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
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void DataLabel_Click(object sender, EventArgs e)
    {
        try
        {
            if (m_client != null)
            {
                if (m_client.Book != null)
                {
                    StringBuilder str = new StringBuilder();
                    if (str != null)
                    {
                        str.AppendLine("Chapter" + "\t" + "Verses" + "\t" + "Words" + "\t" + "Letters" + "\t" + "Value" + "\t" + "Factors");
                        foreach (Chapter chapter in m_client.Book.Chapters)
                        {
                            str.Append(chapter.SortedNumber + "\t");
                            str.Append(chapter.Verses.Count + "\t");
                            str.Append(chapter.Words.Count + "\t");
                            str.Append(chapter.Letters.Count + "\t");
                            str.Append(chapter.Value + "\t");
                            str.Append(Numbers.FactorizeToString(chapter.Value) + "\r\n");
                        }
                        str.AppendLine();

                        string path = Globals.DATA_FOLDER + Path.DirectorySeparatorChar + "quran-lab.txt";
                        FileHelper.SaveText(path, str.ToString());
                        if (File.Exists(path))
                        {
                            System.Diagnostics.Process.Start(path);
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
    private void HelpLabel_Click(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start("Help");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
        }
    }
}
