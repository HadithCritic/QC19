using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Model;

public class Client : IPublisher, ISubscriber
{
    private static Server s_server = null;

    #region Interfaces
    ///////////////////////////////////////////////////////////////////////////////
    // IPublisher methods
    private Dictionary<Subject, List<ISubscriber>> m_subscribers = null;
    public void Subscribe(Subject subject, ISubscriber subscriber)
    {
        if (m_subscribers == null)
        {
            m_subscribers = new Dictionary<Subject, List<ISubscriber>>();
        }

        if (m_subscribers != null)
        {
            if (!m_subscribers.ContainsKey(subject))
            {
                m_subscribers.Add(subject, new List<ISubscriber>());
            }

            if (m_subscribers.ContainsKey(subject))
            {
                if (m_subscribers[subject] != null)
                {
                    m_subscribers[subject].Add(subscriber);
                }
            }
        }
    }
    public void Unsubscribe(Subject subject, ISubscriber subscriber)
    {
        if (m_subscribers != null)
        {
            if (m_subscribers.ContainsKey(subject))
            {
                if (m_subscribers[subject] != null)
                {
                    m_subscribers[subject].Remove(subscriber);
                }
            }
        }
    }
    // ISubscriber methods
    public void Notify(Subject subject, FileSystemEventArgs e)
    {
        // notify subscribers
        NotifySubscribers(subject, e);
    }
    private void NotifySubscribers(Subject subject, FileSystemEventArgs e)
    {
        if (m_subscribers != null)
        {
            if (m_subscribers.ContainsKey(subject))
            {
                foreach (ISubscriber item in m_subscribers[subject])
                {
                    if (item != null)
                    {
                        try
                        {
                            item.Notify(subject, e);
                        }
                        catch
                        {
                            // handle exception if desired... }
                        }
                    }
                }
            }
        }
    }
    // folder watcher
    public void WatchFolder(string folder_name, string filter)
    {
        if (s_server != null)
        {
            s_server.WatchFolder(folder_name, filter);
        }
    }
    public void UnwatchFolder(string folder_name, string filter)
    {
        try
        {
            if (s_server != null)
            {
                s_server.UnwatchFolder(folder_name, filter);
            }

            //FileHelper.WaitForFolderReady(Globals.LANGUAGES_FOLDER, "*.txt");
            //FileHelper.WaitForFolderReady(Globals.RULES_FOLDER, "*.txt");
            //FileHelper.WaitForFolderReady(Globals.VALUES_FOLDER, "*.txt");
            //FileHelper.WaitForFolderReady(Globals.NUMBERS_FOLDER, Globals.INTERESTING_NUMBERS_FILENAME);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ".\r\n\r\n" + ex.StackTrace);
        }
    }
    ///////////////////////////////////////////////////////////////////////////////
    #endregion

    public const int DEFAULT_WORD_COUNT_METHOD = Server.DEFAULT_WORD_COUNT_METHOD;
    public const string DEFAULT_RECITATION = Server.DEFAULT_RECITATION;
    public const string DEFAULT_EMLAAEI_TEXT = Server.DEFAULT_EMLAAEI_TEXT;
    public const string DEFAULT_TRANSLATION = Server.DEFAULT_TRANSLATION;
    public const string DEFAULT_TRANSLITERATION = Server.DEFAULT_TRANSLITERATION;
    public const string DEFAULT_WORD_MEANINGS = Server.DEFAULT_WORD_MEANINGS;
    public const string DEFAULT_TRANSLATION_1 = Server.DEFAULT_TRANSLATION_1;
    public const string DEFAULT_TRANSLATION_2 = Server.DEFAULT_TRANSLATION_2;
    public const string DEFAULT_TRANSLATION_3 = Server.DEFAULT_TRANSLATION_3;
    public const string DEFAULT_TRANSLATION_4 = Server.DEFAULT_TRANSLATION_4;
    public const string DEFAULT_TRANSLATION_5 = Server.DEFAULT_TRANSLATION_5;
    public const string DEFAULT_TRANSLATION_6 = Server.DEFAULT_TRANSLATION_6;
    public const string DEFAULT_TRANSLATION_7 = Server.DEFAULT_TRANSLATION_7;
    public const string DEFAULT_TRANSLATION_8 = Server.DEFAULT_TRANSLATION_8;
    public const string DEFAULT_TRANSLATION_9 = Server.DEFAULT_TRANSLATION_9;

    public Client(string numerical_system_name)
    {
        if (!Directory.Exists(Globals.BOOKMARKS_FOLDER))
        {
            Directory.CreateDirectory(Globals.BOOKMARKS_FOLDER);
        }

        if (!Directory.Exists(Globals.HISTORY_FOLDER))
        {
            Directory.CreateDirectory(Globals.HISTORY_FOLDER);
        }

        // notify me of all event types
        s_server = new Server();
        if (s_server != null)
        {
            s_server.Subscribe(Subject.LanguageSystem, this);
            s_server.Subscribe(Subject.SimplificationSystem, this);
            s_server.Subscribe(Subject.NumericalSystem, this);
            s_server.Subscribe(Subject.DNASequenceSystem, this);
            s_server.Subscribe(Subject.InterestingNumbers, this);
        }

        // load and set initial NumericalSystem
        LoadNumericalSystem(numerical_system_name);
    }

    public static int WordCountMethod
    {
        set
        {
            Server.WordCountMethod = value;
        }
        get
        {
            return Server.WordCountMethod;
        }
    }
    public static List<int> WordCountMethods
    {
        get { return Server.WordCountMethods; }
    }

    // current book
    public Book Book
    {
        get { return Server.Book; }
    }

    // current simplification system
    public SimplificationSystem SimplificationSystem
    {
        get { return Server.SimplificationSystem; }
    }
    // all loaded simplification systems
    public static Dictionary<string, SimplificationSystem> LoadedSimplificationSystems
    {
        get { return Server.LoadedSimplificationSystems; }
    }
    public void BuildSimplifiedBook(string text_mode, bool with_diacritics, bool with_bism_Allah, bool waw_as_word, bool shadda_as_letter, bool hamza_above_horizontal_line_as_letter, bool elf_above_horizontal_line_as_letter, bool yaa_above_horizontal_line_as_letter, bool noon_above_horizontal_line_as_letter, bool emlaaei_text)
    {
        Server.BuildSimplifiedBook(text_mode, with_diacritics, with_bism_Allah, waw_as_word, shadda_as_letter, hamza_above_horizontal_line_as_letter, elf_above_horizontal_line_as_letter, yaa_above_horizontal_line_as_letter, noon_above_horizontal_line_as_letter, emlaaei_text);
        UpdatePhrasePositionsAndLengths(text_mode);

        // supports FrequencyByValue and OccurrencesByValue
        if (Globals.EDITION != Edition.Standard)
        {
            if (Book != null)
            {
                CalculateValue(Book.Words);
                Book.SetupWordFrequenciesAndOccurrences(with_diacritics);
            }
        }
    }

    // current numerical system
    public NumericalSystem NumericalSystem
    {
        get { return Server.NumericalSystem; }
        set { Server.NumericalSystem = value; }
    }
    // all loaded numerical systems
    public Dictionary<string, NumericalSystem> LoadedNumericalSystems
    {
        get { return Server.LoadedNumericalSystems; }
    }
    // client-side Numerical system scope (not needed by NumericalSystem class) 
    private NumericalSystemScope m_numerical_system_scope = NumericalSystemScope.Book;
    public NumericalSystemScope NumericalSystemScope
    {
        get { return m_numerical_system_scope; }
        set { m_numerical_system_scope = value; }
    }
    // update current numerical system
    public void UpdateNumericalSystem(string text, bool? include_diacritics)
    {
        Server.UpdateNumericalSystem(text, include_diacritics);
    }
    private void UpdatePhrasePositionsAndLengths(string text_mode)
    {
        if (Book != null)
        {
            // update Selection to point at new book object
            if (m_selection != null)
            {
                m_selection = new Selection(Book, m_selection.Scope, m_selection.Indexes);
            }

            if (m_selection != null)
            {
                if (NumericalSystem != null)
                {
                    // update FoundVerses to point at new book object
                    if (Book.Verses != null)
                    {
                        if (m_found_verses != null)
                        {
                            List<Verse> verses = new List<Verse>();
                            foreach (Verse verse in m_found_verses)
                            {
                                int index = verse.Number - 1;
                                if ((index >= 0) && (index < Book.Verses.Count))
                                {
                                    verses.Add(Book.Verses[index]);
                                }
                            }
                            m_found_verses = verses;
                        }
                    }

                    // update FoundPhrases to point at new book object
                    if (Book.Verses != null)
                    {
                        if (m_found_phrases != null)
                        {
                            for (int i = 0; i < m_found_phrases.Count; i++)
                            {
                                Phrase phrase = m_found_phrases[i];
                                if (phrase != null)
                                {
                                    int index = phrase.Verse.Number - 1;
                                    if ((index >= 0) && (index < Book.Verses.Count))
                                    {
                                        phrase = new Phrase(Book.Verses[index], phrase.Position, phrase.Text);
                                        m_found_phrases[i] = Server.SimplifyPhrase(phrase, text_mode);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // ALSO should update these less used collections as they are already held by FoundVerses
            // update FoundVerseRanges to point at new book object
            // update FoundVerseSets to point at new book object
            // update FoundChapters to point at new book object
            // update FoundChapterRanges to point at new book object
            // update FoundChapterSets to point at new book object
        }
    }
    // load and replace current numerical system
    public void LoadNumericalSystem(string numerical_system_name)
    {
        Server.LoadNumericalSystem(numerical_system_name);
    }
    public void ReloadNumericalSystem()
    {
        Server.LoadNumericalSystem(NumericalSystem.Name);
    }

    // current dna sequence system
    public DNASequenceSystem DNASequenceSystem
    {
        get { return Server.DNASequenceSystem; }
    }
    // all loaded dna sequence systems
    public Dictionary<string, DNASequenceSystem> LoadedDNASequenceSystems
    {
        get { return Server.LoadedDNASequenceSystems; }
    }
    public void LoadDNASequenceSystem(string dna_sequence_system_name)
    {
        Server.LoadDNASequenceSystem(dna_sequence_system_name);
    }

    // translations
    public string GetTranslationKey(string translation)
    {
        return Server.GetTranslationKey(translation);
    }
    public void LoadTranslation(string translation)
    {
        Server.LoadTranslation(translation);
    }
    public void UnloadTranslation(string translation)
    {
        Server.UnloadTranslation(translation);
    }
    public void SaveTranslation(string translation)
    {
        Server.SaveTranslation(translation);
    }

    public CalculationMode CalculationMode
    {
        get { return Server.CalculationMode; }
        set { Server.CalculationMode = value; }
    }
    public bool AlternateLetterValues
    {
        get { return Server.AlternateLetterValues; }
        set { Server.AlternateLetterValues = value; }
    }
    public bool AlternateWordValues
    {
        get { return Server.AlternateWordValues; }
        set { Server.AlternateWordValues = value; }
    }
    public bool AlternateVerseValues
    {
        get { return Server.AlternateVerseValues; }
        set { Server.AlternateVerseValues = value; }
    }
    public bool AlternateChapterValues
    {
        get { return Server.AlternateChapterValues; }
        set { Server.AlternateChapterValues = value; }
    }

    public bool Logging = false;
    // used for user text or Quran highlighted text in CalculationMode.SumOfUniqueLetterValues
    public long CalculateValue(char character)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(character, Logging);
    }
    public long CalculateValue(string text)
    {
        if (NumericalSystem != null)
        {
            text = text.Replace("\t", "");
            text = text.Replace("_", "");
            text = text.Replace(Constants.ORNATE_RIGHT_PARENTHESIS, "");
            text = text.Replace(Constants.ORNATE_LEFT_PARENTHESIS, "");
            foreach (char character in Constants.INDIAN_DIGITS)
            {
                text = text.Replace(character.ToString(), "");
            }
            foreach (char character in Constants.ARABIC_DIGITS)
            {
                text = text.Replace(character.ToString(), "");
            }
            foreach (char character in Constants.SYMBOLS)
            {
                text = text.Replace(character.ToString(), "");
            }
        }

        if (Logging) Server.ClearLog();

        return Server.CalculateValue(text, Logging);
    }
    // used for Quran text for non-CalculationMode.SumOfUniqueLetterValues
    public long CalculateValue(Letter letter)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(letter, Logging);
    }
    public long CalculateValue(Word word)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(word, Logging);
    }
    public long CalculateValue(List<Word> words)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(words, Logging);
    }
    public long CalculateValue(Verse verse)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(verse, Logging);
    }
    public long CalculateValue(List<Verse> verses)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(verses, Logging);
    }
    public long CalculateValue(Chapter chapter)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(chapter, Logging);
    }
    public long CalculateValue(List<Chapter> chapters)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(chapters, Logging);
    }
    public long CalculateValue(Book book)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(book, Logging);
    }
    public long CalculateValue(List<Verse> verses, Letter start_letter, Letter end_letter)
    {
        if (Logging) Server.ClearLog();

        return Server.CalculateValue(verses, start_letter, end_letter, Logging);
    }
    public List<long> CalculateVerseValues(List<Verse> verses)
    {
        if (Logging) Server.ClearLog();

        List<long> result = new List<long>();
        foreach (Verse verse in verses)
        {
            result.Add(Server.CalculateValue(verse, Logging));
        }
        return result;
    }
    public List<long> CalculateWordValues(List<Verse> verses)
    {
        if (Logging) Server.ClearLog();

        List<long> result = new List<long>();
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                result.Add(Server.CalculateValue(word, Logging));
            }
        }
        return result;
    }
    public List<long> CalculateLetterValues(List<Verse> verses)
    {
        if (Logging) Server.ClearLog();

        List<long> result = new List<long>();
        foreach (Verse verse in verses)
        {
            foreach (Word word in verse.Words)
            {
                foreach (Letter letter in word.Letters)
                {
                    result.Add(Server.CalculateValue(letter, Logging));
                }
            }
        }
        return result;
    }
    public long MaximumVerseValue
    {
        get
        {
            long result = 0L;
            foreach (Verse verse in Book.Verses)
            {
                long value = Server.CalculateValue(verse, false);
                if (result < value)
                {
                    result = value;
                }
            }
            return result;
        }
    }
    public long MaximumWordValue
    {
        get
        {
            long result = 0L;
            foreach (Verse verse in Book.Verses)
            {
                foreach (Word word in verse.Words)
                {
                    long value = Server.CalculateValue(word, false);
                    if (result < value)
                    {
                        result = value;
                    }
                }
            }
            return result;
        }
    }
    public long MaximumLetterValue
    {
        get
        {
            long result = 0L;
            if (NumericalSystem != null)
            {
                foreach (long value in NumericalSystem.Values)
                {
                    if (result < value)
                    {
                        result = value;
                    }
                }
            }
            return result;
        }
    }
    public void SaveValueCalculations(string path, string text, bool is_value)
    {
        if (Directory.Exists(Globals.STATISTICS_FOLDER))
        {
            path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + path;
            try
            {
                if (NumericalSystem != null)
                {
                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                    {
                        StringBuilder numbers = new StringBuilder();
                        foreach (int index in Selection.Indexes)
                        {
                            numbers.Append((index + 1).ToString() + ", ");
                        }
                        if (numbers.Length > 0)
                        {
                            numbers.Remove(numbers.Length - 2, 2);
                        }

                        if (is_value)
                        {
                            writer.WriteLine("---------------------------------------------------------------------------------------------------------------------------");
                            writer.WriteLine(NumericalSystem.Name);
                            writer.WriteLine("Selection = " + Selection.Scope.ToString() + " " + numbers.ToString());
                            writer.WriteLine("CalculationMode = " + CalculationMode);
                            writer.WriteLine("---------------------------------------------------------------------------------------------------------------------------");
                            writer.WriteLine(NumericalSystem.ToOverview());
                            writer.WriteLine();
                        }
                        writer.WriteLine(is_value ? "Text" : "Number Analysis");
                        writer.WriteLine("---------------------------------------------------------------------------------------------------------------------------");
                        writer.WriteLine(text);

                        if (is_value)
                        {
                            writer.WriteLine("------------------------------------------------------------------------------------------------------------------------------------");
                            writer.WriteLine("Letter" + "\t" + "Value" + "\t" + "\t" + "\t" + "\t" + "L" + "\t" + "W" + "\t" + "V" + "\t" + "C" + "\t" + "←∆L" + "\t" + "←∆W" + "\t" + "←∆V" + "\t" + "←∆C" + "\t" + "∆L→" + "\t" + "∆W→" + "\t" + "∆V→" + "\t" + "∆C→");
                            writer.WriteLine("------------------------------------------------------------------------------------------------------------------------------------");
                            writer.Write(Server.Log.ToString());
                            writer.WriteLine("------------------------------------------------------------------------------------------------------------------------------------");
                            writer.WriteLine("Sum" + "\t" + Server.Log_Value + "\t" + "\t" + "\t" + "\t" + Server.Log_LSum + "\t" + Server.Log_WSum + "\t" + Server.Log_VSum + "\t" + Server.Log_CSum + "\t" + Server.Log_pLSum + "\t" + Server.Log_pWSum + "\t" + Server.Log_pVSum + "\t" + Server.Log_pCSum + "\t" + Server.Log_nLSum + "\t" + Server.Log_nWSum + "\t" + Server.Log_nVSum + "\t" + Server.Log_nCSum);
                            writer.WriteLine("------------------------------------------------------------------------------------------------------------------------------------");
                            writer.WriteLine("Total" + "\t" + Server.Log_Total);
                            writer.WriteLine("------------------------------------------------------------------------------------------------------------------------------------");
                        }
                    }
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
    public void SaveNumberIndexChain(string path, long number, int chain_length, string text)
    {
        if (Directory.Exists(Globals.STATISTICS_FOLDER))
        {
            path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + path;
            try
            {
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                {
                    writer.WriteLine("-----------------------------------------------------");
                    writer.WriteLine(number.ToString() + " with IndexChainLength = " + chain_length.ToString());
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
    public void SaveIndexChainLength(string path, NumberType number_type, int chain_length, string text)
    {
        if (Directory.Exists(Globals.STATISTICS_FOLDER))
        {
            path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + path;
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


    private List<Chapter> m_filter_chapters = null;
    public List<Chapter> FilterChapters
    {
        set { m_filter_chapters = value; }
        get { return m_filter_chapters; }
    }
    private Selection m_selection = null;
    public Selection Selection
    {
        get
        {
            if (Book != null)
            {
                return m_selection;
            }
            return null;
        }
        set
        {
            if (Book != null)
            {
                m_selection = value;
            }
        }
    }
    private void ClearSelection()
    {
        if (Book != null)
        {
            if (m_selection != null)
            {
                m_selection = new Selection(Book, SelectionScope.Chapter, new List<int>() { 0 });
            }
        }
    }
    private SearchScope m_search_scope = SearchScope.Book;
    public SearchScope SearchScope
    {
        set { m_search_scope = value; }
        get { return m_search_scope; }
    }

    private List<Phrase> m_found_phrases = null;
    public List<Phrase> FoundPhrases
    {
        set { m_found_phrases = value; }
        get
        {
            if (m_found_phrases == null) return null;
            if (m_filter_chapters == null) return m_found_phrases;

            List<Phrase> filtered_found_phrases = new List<Phrase>();
            foreach (Phrase phrase in m_found_phrases)
            {
                if (phrase != null)
                {
                    if (phrase.Verse != null)
                    {
                        if (phrase.Verse.Chapter != null)
                        {
                            if (m_filter_chapters.Contains(phrase.Verse.Chapter))
                            {
                                filtered_found_phrases.Add(phrase);
                            }
                        }
                    }
                }
            }
            return filtered_found_phrases;
        }
    }

    private List<Letter> m_found_letters = null;
    public List<Letter> FoundLetters
    {
        set { m_found_letters = value; }
        get
        {
            if (m_found_letters == null) return null;
            if (m_filter_chapters == null) return m_found_letters;

            List<Letter> filtered_found_letters = new List<Letter>();
            foreach (Letter letter in m_found_letters)
            {
                if (letter != null)
                {
                    if (letter.Word != null)
                    {
                        if (letter.Word.Verse != null)
                        {
                            if (letter.Word.Verse.Chapter != null)
                            {
                                if (m_filter_chapters.Contains(letter.Word.Verse.Chapter))
                                {
                                    filtered_found_letters.Add(letter);
                                }
                            }
                        }
                    }
                }
            }
            return filtered_found_letters;
        }
    }
    private List<List<Letter>> m_found_letter_ranges = null;
    public List<List<Letter>> FoundLetterRanges
    {
        set { m_found_letter_ranges = value; }
        get
        {
            if (m_found_letter_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_letter_ranges;

            List<List<Letter>> filtered_found_letter_ranges = new List<List<Letter>>();
            foreach (List<Letter> range in m_found_letter_ranges)
            {
                bool valid_range = true;
                foreach (Letter letter in range)
                {
                    if (letter != null)
                    {
                        if (letter.Word != null)
                        {
                            if (letter.Word.Verse != null)
                            {
                                if (letter.Word.Verse.Chapter != null)
                                {
                                    if (!m_filter_chapters.Contains(letter.Word.Verse.Chapter))
                                    {
                                        valid_range = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_letter_ranges.Add(range);
                }
            }
            return filtered_found_letter_ranges;
        }
    }
    private List<List<Letter>> m_found_letter_sets = null;
    public List<List<Letter>> FoundLetterSets
    {
        set { m_found_letter_sets = value; }
        get
        {
            if (m_found_letter_sets == null) return null;
            if (m_filter_chapters == null) return m_found_letter_sets;

            List<List<Letter>> filtered_found_letter_sets = new List<List<Letter>>();
            foreach (List<Letter> set in m_found_letter_sets)
            {
                bool valid_set = true;
                foreach (Letter letter in set)
                {
                    if (letter != null)
                    {
                        if (letter.Word != null)
                        {
                            if (letter.Word.Verse != null)
                            {
                                if (letter.Word.Verse.Chapter != null)
                                {
                                    if (!m_filter_chapters.Contains(letter.Word.Verse.Chapter))
                                    {
                                        valid_set = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_letter_sets.Add(set);
                }
            }
            return filtered_found_letter_sets;
        }
    }

    private List<Word> m_found_words = null;
    public List<Word> FoundWords
    {
        set { m_found_words = value; }
        get
        {
            if (m_found_words == null) return null;
            if (m_filter_chapters == null) return m_found_words;

            List<Word> filtered_found_words = new List<Word>();
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        if (word.Verse.Chapter != null)
                        {
                            if (m_filter_chapters.Contains(word.Verse.Chapter))
                            {
                                filtered_found_words.Add(word);
                            }
                        }
                    }
                }
            }
            return filtered_found_words;
        }
    }
    private List<List<Word>> m_found_word_ranges = null;
    public List<List<Word>> FoundWordRanges
    {
        set { m_found_word_ranges = value; }
        get
        {
            if (m_found_word_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_word_ranges;

            List<List<Word>> filtered_found_word_ranges = new List<List<Word>>();
            foreach (List<Word> range in m_found_word_ranges)
            {
                bool valid_range = true;
                foreach (Word word in range)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            if (word.Verse.Chapter != null)
                            {
                                if (!m_filter_chapters.Contains(word.Verse.Chapter))
                                {
                                    valid_range = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_word_ranges.Add(range);
                }
            }
            return filtered_found_word_ranges;
        }
    }
    private List<List<Word>> m_found_word_sets = null;
    public List<List<Word>> FoundWordSets
    {
        set { m_found_word_sets = value; }
        get
        {
            if (m_found_word_sets == null) return null;
            if (m_filter_chapters == null) return m_found_word_sets;

            List<List<Word>> filtered_found_word_sets = new List<List<Word>>();
            foreach (List<Word> set in m_found_word_sets)
            {
                bool valid_set = true;
                foreach (Word word in set)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            if (word.Verse.Chapter != null)
                            {
                                if (!m_filter_chapters.Contains(word.Verse.Chapter))
                                {
                                    valid_set = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_word_sets.Add(set);
                }
            }
            return filtered_found_word_sets;
        }
    }

    private List<Sentence> m_found_sentences = null;
    public List<Sentence> FoundSentences
    {
        set { m_found_sentences = value; }
        get
        {
            if (m_found_sentences == null) return null;
            if (m_filter_chapters == null) return m_found_sentences;

            List<Sentence> filtered_found_sentences = new List<Sentence>();
            foreach (Sentence sentence in m_found_sentences)
            {
                if (sentence != null)
                {
                    if (sentence.FirstVerse != null)
                    {
                        if (sentence.FirstVerse.Chapter != null)
                        {
                            if (m_filter_chapters.Contains(sentence.FirstVerse.Chapter))
                            {
                                filtered_found_sentences.Add(sentence);
                            }
                        }
                    }
                }
            }
            return filtered_found_sentences;
        }
    }

    private List<Verse> m_found_verses = null;
    public List<Verse> FoundVerses
    {
        set { m_found_verses = value; }
        get
        {
            if (m_found_verses == null) return null;
            if (m_filter_chapters == null) return m_found_verses;

            List<Verse> filtered_found_verses = new List<Verse>();
            foreach (Verse verse in m_found_verses)
            {
                if (verse != null)
                {
                    if (verse.Chapter != null)
                    {
                        if (m_filter_chapters.Contains(verse.Chapter))
                        {
                            filtered_found_verses.Add(verse);
                        }
                    }
                }
            }
            return filtered_found_verses;
        }
    }
    private List<List<Verse>> m_found_verse_ranges = null;
    public List<List<Verse>> FoundVerseRanges
    {
        set { m_found_verse_ranges = value; }
        get
        {
            if (m_found_verse_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_verse_ranges;

            List<List<Verse>> filtered_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Verse> range in m_found_verse_ranges)
            {
                bool valid_range = true;
                foreach (Verse verse in range)
                {
                    if (verse != null)
                    {
                        if (verse.Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(verse.Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_verse_ranges.Add(range);
                }
            }
            return filtered_found_verse_ranges;
        }
    }
    private List<List<Verse>> m_found_verse_sets = null;
    public List<List<Verse>> FoundVerseSets
    {
        set { m_found_verse_sets = value; }
        get
        {
            if (m_found_verse_sets == null) return null;
            if (m_filter_chapters == null) return m_found_verse_sets;

            List<List<Verse>> filtered_found_verse_sets = new List<List<Verse>>();
            foreach (List<Verse> set in m_found_verse_sets)
            {
                bool valid_set = true;
                foreach (Verse verse in set)
                {
                    if (verse != null)
                    {
                        if (verse.Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(verse.Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_verse_sets.Add(set);
                }
            }
            return filtered_found_verse_sets;
        }
    }

    private List<Chapter> m_found_chapters = null;
    public List<Chapter> FoundChapters
    {
        set { m_found_chapters = value; }
        get
        {
            if (m_found_chapters == null) return null;
            if (m_filter_chapters == null) return m_found_chapters;

            List<Chapter> filtered_found_chapters = new List<Chapter>();
            foreach (Chapter chapter in m_found_chapters)
            {
                if (chapter != null)
                {
                    if (m_filter_chapters.Contains(chapter))
                    {
                        filtered_found_chapters.Add(chapter);
                    }
                }
            }
            return filtered_found_chapters;
        }
    }
    private List<List<Chapter>> m_found_chapter_ranges = null;
    public List<List<Chapter>> FoundChapterRanges
    {
        set { m_found_chapter_ranges = value; }
        get
        {
            if (m_found_chapter_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_chapter_ranges;

            List<List<Chapter>> filtered_found_chapter_ranges = new List<List<Chapter>>();
            foreach (List<Chapter> range in m_found_chapter_ranges)
            {
                bool valid_range = true;
                foreach (Chapter chapter in range)
                {
                    if (chapter != null)
                    {
                        if (!m_filter_chapters.Contains(chapter))
                        {
                            valid_range = false;
                            break;
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_chapter_ranges.Add(range);
                }
            }
            return filtered_found_chapter_ranges;
        }
    }
    private List<List<Chapter>> m_found_chapter_sets = null;
    public List<List<Chapter>> FoundChapterSets
    {
        set { m_found_chapter_sets = value; }
        get
        {
            if (m_found_chapter_sets == null) return null;
            if (m_filter_chapters == null) return m_found_chapter_sets;

            List<List<Chapter>> filtered_found_chapter_sets = new List<List<Chapter>>();
            foreach (List<Chapter> set in m_found_chapter_sets)
            {
                bool valid_set = true;
                foreach (Chapter chapter in set)
                {
                    if (chapter != null)
                    {
                        if (!m_filter_chapters.Contains(chapter))
                        {
                            valid_set = false;
                            break;
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_chapter_sets.Add(set);
                }
            }
            return filtered_found_chapter_sets;
        }
    }

    private List<Page> m_found_pages = null;
    public List<Page> FoundPages
    {
        set { m_found_pages = value; }
        get
        {
            if (m_found_pages == null) return null;
            if (m_filter_chapters == null) return m_found_pages;

            List<Page> filtered_found_pages = new List<Page>();
            foreach (Page page in m_found_pages)
            {
                if (page != null)
                {
                    if (page.Verses[0].Chapter != null)
                    {
                        if (m_filter_chapters.Contains(page.Verses[0].Chapter))
                        {
                            filtered_found_pages.Add(page);
                        }
                    }
                }
            }
            return filtered_found_pages;
        }
    }
    private List<List<Page>> m_found_page_ranges = null;
    public List<List<Page>> FoundPageRanges
    {
        set { m_found_page_ranges = value; }
        get
        {
            if (m_found_page_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_page_ranges;

            List<List<Page>> filtered_found_page_ranges = new List<List<Page>>();
            foreach (List<Page> range in m_found_page_ranges)
            {
                bool valid_range = true;
                foreach (Page page in range)
                {
                    if (page != null)
                    {
                        if (page.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(page.Verses[0].Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_page_ranges.Add(range);
                }
            }
            return filtered_found_page_ranges;
        }
    }
    private List<List<Page>> m_found_page_sets = null;
    public List<List<Page>> FoundPageSets
    {
        set { m_found_page_sets = value; }
        get
        {
            if (m_found_page_sets == null) return null;
            if (m_filter_chapters == null) return m_found_page_sets;

            List<List<Page>> filtered_found_page_sets = new List<List<Page>>();
            foreach (List<Page> set in m_found_page_sets)
            {
                bool valid_set = true;
                foreach (Page page in set)
                {
                    if (page != null)
                    {
                        if (page.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(page.Verses[0].Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_page_sets.Add(set);
                }
            }
            return filtered_found_page_sets;
        }
    }

    private List<Station> m_found_stations = null;
    public List<Station> FoundStations
    {
        set { m_found_stations = value; }
        get
        {
            if (m_found_stations == null) return null;
            if (m_filter_chapters == null) return m_found_stations;

            List<Station> filtered_found_stations = new List<Station>();
            foreach (Station station in m_found_stations)
            {
                if (station != null)
                {
                    if (station.Verses[0].Chapter != null)
                    {
                        if (m_filter_chapters.Contains(station.Verses[0].Chapter))
                        {
                            filtered_found_stations.Add(station);
                        }
                    }
                }
            }
            return filtered_found_stations;
        }
    }
    private List<List<Station>> m_found_station_ranges = null;
    public List<List<Station>> FoundStationRanges
    {
        set { m_found_station_ranges = value; }
        get
        {
            if (m_found_station_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_station_ranges;

            List<List<Station>> filtered_found_station_ranges = new List<List<Station>>();
            foreach (List<Station> range in m_found_station_ranges)
            {
                bool valid_range = true;
                foreach (Station station in range)
                {
                    if (station != null)
                    {
                        if (station.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(station.Verses[0].Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_station_ranges.Add(range);
                }
            }
            return filtered_found_station_ranges;
        }
    }
    private List<List<Station>> m_found_station_sets = null;
    public List<List<Station>> FoundStationSets
    {
        set { m_found_station_sets = value; }
        get
        {
            if (m_found_station_sets == null) return null;
            if (m_filter_chapters == null) return m_found_station_sets;

            List<List<Station>> filtered_found_station_sets = new List<List<Station>>();
            foreach (List<Station> set in m_found_station_sets)
            {
                bool valid_set = true;
                foreach (Station station in set)
                {
                    if (station != null)
                    {
                        if (station.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(station.Verses[0].Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_station_sets.Add(set);
                }
            }
            return filtered_found_station_sets;
        }
    }

    private List<Part> m_found_parts = null;
    public List<Part> FoundParts
    {
        set { m_found_parts = value; }
        get
        {
            if (m_found_parts == null) return null;
            if (m_filter_chapters == null) return m_found_parts;

            List<Part> filtered_found_parts = new List<Part>();
            foreach (Part part in m_found_parts)
            {
                if (part != null)
                {
                    if (part.Verses[0].Chapter != null)
                    {
                        if (m_filter_chapters.Contains(part.Verses[0].Chapter))
                        {
                            filtered_found_parts.Add(part);
                        }
                    }
                }
            }
            return filtered_found_parts;
        }
    }
    private List<List<Part>> m_found_part_ranges = null;
    public List<List<Part>> FoundPartRanges
    {
        set { m_found_part_ranges = value; }
        get
        {
            if (m_found_part_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_part_ranges;

            List<List<Part>> filtered_found_part_ranges = new List<List<Part>>();
            foreach (List<Part> range in m_found_part_ranges)
            {
                bool valid_range = true;
                foreach (Part part in range)
                {
                    if (part != null)
                    {
                        if (part.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(part.Verses[0].Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_part_ranges.Add(range);
                }
            }
            return filtered_found_part_ranges;
        }
    }
    private List<List<Part>> m_found_part_sets = null;
    public List<List<Part>> FoundPartSets
    {
        set { m_found_part_sets = value; }
        get
        {
            if (m_found_part_sets == null) return null;
            if (m_filter_chapters == null) return m_found_part_sets;

            List<List<Part>> filtered_found_part_sets = new List<List<Part>>();
            foreach (List<Part> set in m_found_part_sets)
            {
                bool valid_set = true;
                foreach (Part part in set)
                {
                    if (part != null)
                    {
                        if (part.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(part.Verses[0].Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_part_sets.Add(set);
                }
            }
            return filtered_found_part_sets;
        }
    }

    private List<Group> m_found_groups = null;
    public List<Group> FoundGroups
    {
        set { m_found_groups = value; }
        get
        {
            if (m_found_groups == null) return null;
            if (m_filter_chapters == null) return m_found_groups;

            List<Group> filtered_found_groups = new List<Group>();
            foreach (Group group in m_found_groups)
            {
                if (group != null)
                {
                    if (group.Verses[0].Chapter != null)
                    {
                        if (m_filter_chapters.Contains(group.Verses[0].Chapter))
                        {
                            filtered_found_groups.Add(group);
                        }
                    }
                }
            }
            return filtered_found_groups;
        }
    }
    private List<List<Group>> m_found_group_ranges = null;
    public List<List<Group>> FoundGroupRanges
    {
        set { m_found_group_ranges = value; }
        get
        {
            if (m_found_group_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_group_ranges;

            List<List<Group>> filtered_found_group_ranges = new List<List<Group>>();
            foreach (List<Group> range in m_found_group_ranges)
            {
                bool valid_range = true;
                foreach (Group group in range)
                {
                    if (group != null)
                    {
                        if (group.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(group.Verses[0].Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_group_ranges.Add(range);
                }
            }
            return filtered_found_group_ranges;
        }
    }
    private List<List<Group>> m_found_group_sets = null;
    public List<List<Group>> FoundGroupSets
    {
        set { m_found_group_sets = value; }
        get
        {
            if (m_found_group_sets == null) return null;
            if (m_filter_chapters == null) return m_found_group_sets;

            List<List<Group>> filtered_found_group_sets = new List<List<Group>>();
            foreach (List<Group> set in m_found_group_sets)
            {
                bool valid_set = true;
                foreach (Group group in set)
                {
                    if (group != null)
                    {
                        if (group.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(group.Verses[0].Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_group_sets.Add(set);
                }
            }
            return filtered_found_group_sets;
        }
    }

    private List<Half> m_found_halfs = null;
    public List<Half> FoundHalfs
    {
        set { m_found_halfs = value; }
        get
        {
            if (m_found_halfs == null) return null;
            if (m_filter_chapters == null) return m_found_halfs;

            List<Half> filtered_found_halfs = new List<Half>();
            foreach (Half half in m_found_halfs)
            {
                if (half != null)
                {
                    if (half.Verses[0].Chapter != null)
                    {
                        if (m_filter_chapters.Contains(half.Verses[0].Chapter))
                        {
                            filtered_found_halfs.Add(half);
                        }
                    }
                }
            }
            return filtered_found_halfs;
        }
    }
    private List<List<Half>> m_found_half_ranges = null;
    public List<List<Half>> FoundHalfRanges
    {
        set { m_found_half_ranges = value; }
        get
        {
            if (m_found_half_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_half_ranges;

            List<List<Half>> filtered_found_half_ranges = new List<List<Half>>();
            foreach (List<Half> range in m_found_half_ranges)
            {
                bool valid_range = true;
                foreach (Half half in range)
                {
                    if (half != null)
                    {
                        if (half.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(half.Verses[0].Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_half_ranges.Add(range);
                }
            }
            return filtered_found_half_ranges;
        }
    }
    private List<List<Half>> m_found_half_sets = null;
    public List<List<Half>> FoundHalfSets
    {
        set { m_found_half_sets = value; }
        get
        {
            if (m_found_half_sets == null) return null;
            if (m_filter_chapters == null) return m_found_half_sets;

            List<List<Half>> filtered_found_half_sets = new List<List<Half>>();
            foreach (List<Half> set in m_found_half_sets)
            {
                bool valid_set = true;
                foreach (Half half in set)
                {
                    if (half != null)
                    {
                        if (half.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(half.Verses[0].Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_half_sets.Add(set);
                }
            }
            return filtered_found_half_sets;
        }
    }

    private List<Quarter> m_found_quarters = null;
    public List<Quarter> FoundQuarters
    {
        set { m_found_quarters = value; }
        get
        {
            if (m_found_quarters == null) return null;
            if (m_filter_chapters == null) return m_found_quarters;

            List<Quarter> filtered_found_quarters = new List<Quarter>();
            foreach (Quarter quarter in m_found_quarters)
            {
                if (quarter != null)
                {
                    if (quarter.Verses[0].Chapter != null)
                    {
                        if (m_filter_chapters.Contains(quarter.Verses[0].Chapter))
                        {
                            filtered_found_quarters.Add(quarter);
                        }
                    }
                }
            }
            return filtered_found_quarters;
        }
    }
    private List<List<Quarter>> m_found_quarter_ranges = null;
    public List<List<Quarter>> FoundQuarterRanges
    {
        set { m_found_quarter_ranges = value; }
        get
        {
            if (m_found_quarter_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_quarter_ranges;

            List<List<Quarter>> filtered_found_quarter_ranges = new List<List<Quarter>>();
            foreach (List<Quarter> range in m_found_quarter_ranges)
            {
                bool valid_range = true;
                foreach (Quarter quarter in range)
                {
                    if (quarter != null)
                    {
                        if (quarter.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(quarter.Verses[0].Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_quarter_ranges.Add(range);
                }
            }
            return filtered_found_quarter_ranges;
        }
    }
    private List<List<Quarter>> m_found_quarter_sets = null;
    public List<List<Quarter>> FoundQuarterSets
    {
        set { m_found_quarter_sets = value; }
        get
        {
            if (m_found_quarter_sets == null) return null;
            if (m_filter_chapters == null) return m_found_quarter_sets;

            List<List<Quarter>> filtered_found_quarter_sets = new List<List<Quarter>>();
            foreach (List<Quarter> set in m_found_quarter_sets)
            {
                bool valid_set = true;
                foreach (Quarter quarter in set)
                {
                    if (quarter != null)
                    {
                        if (quarter.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(quarter.Verses[0].Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_quarter_sets.Add(set);
                }
            }
            return filtered_found_quarter_sets;
        }
    }

    private List<Bowing> m_found_bowings = null;
    public List<Bowing> FoundBowings
    {
        set { m_found_bowings = value; }
        get
        {
            if (m_found_bowings == null) return null;
            if (m_filter_chapters == null) return m_found_bowings;

            List<Bowing> filtered_found_bowings = new List<Bowing>();
            foreach (Bowing bowing in m_found_bowings)
            {
                if (bowing != null)
                {
                    if (bowing.Verses[0].Chapter != null)
                    {
                        if (m_filter_chapters.Contains(bowing.Verses[0].Chapter))
                        {
                            filtered_found_bowings.Add(bowing);
                        }
                    }
                }
            }
            return filtered_found_bowings;
        }
    }
    private List<List<Bowing>> m_found_bowing_ranges = null;
    public List<List<Bowing>> FoundBowingRanges
    {
        set { m_found_bowing_ranges = value; }
        get
        {
            if (m_found_bowing_ranges == null) return null;
            if (m_filter_chapters == null) return m_found_bowing_ranges;

            List<List<Bowing>> filtered_found_bowing_ranges = new List<List<Bowing>>();
            foreach (List<Bowing> range in m_found_bowing_ranges)
            {
                bool valid_range = true;
                foreach (Bowing bowing in range)
                {
                    if (bowing != null)
                    {
                        if (bowing.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(bowing.Verses[0].Chapter))
                            {
                                valid_range = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_range)
                {
                    filtered_found_bowing_ranges.Add(range);
                }
            }
            return filtered_found_bowing_ranges;
        }
    }
    private List<List<Bowing>> m_found_bowing_sets = null;
    public List<List<Bowing>> FoundBowingSets
    {
        set { m_found_bowing_sets = value; }
        get
        {
            if (m_found_bowing_sets == null) return null;
            if (m_filter_chapters == null) return m_found_bowing_sets;

            List<List<Bowing>> filtered_found_bowing_sets = new List<List<Bowing>>();
            foreach (List<Bowing> set in m_found_bowing_sets)
            {
                bool valid_set = true;
                foreach (Bowing bowing in set)
                {
                    if (bowing != null)
                    {
                        if (bowing.Verses[0].Chapter != null)
                        {
                            if (!m_filter_chapters.Contains(bowing.Verses[0].Chapter))
                            {
                                valid_set = false;
                                break;
                            }
                        }
                    }
                }
                if (valid_set)
                {
                    filtered_found_bowing_sets.Add(set);
                }
            }
            return filtered_found_bowing_sets;
        }
    }

    //public void NewSearchResults()
    //{
    //    m_filter_chapters = new List<Chapter>();

    //    m_found_phrases = new List<Phrase>();

    //    m_found_letters = new List<Letter>();
    //    m_found_letter_ranges = new List<List<Letter>>();
    //    m_found_letter_sets = new List<List<Letter>>();

    //    m_found_words = new List<Word>();
    //    m_found_word_ranges = new List<List<Word>>();
    //    m_found_word_sets = new List<List<Word>>();

    //    m_found_sentences = new List<Sentence>();

    //    // m_found_verses are needed in nested searches
    //    if (m_search_scope != SearchScope.Result)
    //    {
    //        m_found_verses = new List<Verse>();
    //    }
    //    m_found_verse_ranges = new List<List<Verse>>();
    //    m_found_verse_sets = new List<List<Verse>>();

    //    m_found_chapters = new List<Chapter>();
    //    m_found_chapter_ranges = new List<List<Chapter>>();
    //    m_found_chapter_sets = new List<List<Chapter>>();

    //    m_found_pages = new List<Page>();
    //    m_found_page_ranges = new List<List<Page>>();
    //    m_found_page_sets = new List<List<Page>>();

    //    m_found_stations = new List<Station>();
    //    m_found_station_ranges = new List<List<Station>>();
    //    m_found_station_sets = new List<List<Station>>();

    //    m_found_parts = new List<Part>();
    //    m_found_part_ranges = new List<List<Part>>();
    //    m_found_part_sets = new List<List<Part>>();

    //    m_found_groups = new List<Group>();
    //    m_found_group_ranges = new List<List<Group>>();
    //    m_found_group_sets = new List<List<Group>>();

    //    m_found_halfs = new List<Half>();
    //    m_found_half_ranges = new List<List<Half>>();
    //    m_found_half_sets = new List<List<Half>>();

    //    m_found_quarters = new List<Quarter>();
    //    m_found_quarter_ranges = new List<List<Quarter>>();
    //    m_found_quarter_sets = new List<List<Quarter>>();

    //    m_found_bowings = new List<Bowing>();
    //    m_found_bowing_ranges = new List<List<Bowing>>();
    //    m_found_bowing_sets = new List<List<Bowing>>();
    //}
    public void ClearSearchResults()
    {
        m_filter_chapters = null;

        m_found_phrases = null;

        m_found_letters = null;
        m_found_letter_ranges = null;
        m_found_letter_sets = null;

        m_found_words = null;
        m_found_word_ranges = null;
        m_found_word_sets = null;

        m_found_sentences = null;

        // m_found_verses are needed in nested searches
        if (m_search_scope != SearchScope.Result)
        {
            m_found_verses = null;
        }
        m_found_verse_ranges = null;
        m_found_verse_sets = null;

        m_found_chapters = null;
        m_found_chapter_ranges = null;
        m_found_chapter_sets = null;

        m_found_pages = null;
        m_found_page_ranges = null;
        m_found_page_sets = null;

        m_found_stations = null;
        m_found_station_ranges = null;
        m_found_station_sets = null;

        m_found_parts = null;
        m_found_part_ranges = null;
        m_found_part_sets = null;

        m_found_groups = null;
        m_found_group_ranges = null;
        m_found_group_sets = null;

        m_found_halfs = null;
        m_found_half_ranges = null;
        m_found_half_sets = null;

        m_found_quarters = null;
        m_found_quarter_ranges = null;
        m_found_quarter_sets = null;

        m_found_bowings = null;
        m_found_bowing_ranges = null;
        m_found_bowing_sets = null;
    }

    // helper methods with GetSourceVerses (not entire book verses)
    public Dictionary<string, int> GetCurrentWords(string text, TextLocationInChapter text_location_in_chapter, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        List<Verse> source = Server.GetSourceVerses(m_search_scope, m_selection, m_found_verses, text_location_in_chapter);
        if (Book != null)
        {
            result = Book.GetCurrentWords(source, text, text_location_in_verse, text_location_in_word, text_wordness, with_diacritics);
        }
        return result;
    }
    public Dictionary<string, int> GetNextWords(string text, TextLocationInChapter text_location_in_chapter, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        List<Verse> source = Server.GetSourceVerses(m_search_scope, m_selection, m_found_verses, text_location_in_chapter);
        if (Book != null)
        {
            result = Book.GetNextWords(source, text, text_location_in_verse, text_location_in_word, text_wordness, with_diacritics);
        }
        return result;
    }

    // helper method with GetSourceVerses (not entire book verses)
    public Dictionary<string, int> GetWordRoots(string text, TextLocationInWord text_location_in_word)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        List<Verse> source = Server.GetSourceVerses(m_search_scope, m_selection, m_found_verses, TextLocationInChapter.Any);
        if (Book != null)
        {
            result = Book.GetRoots(source, text, text_location_in_word);
        }
        return result;
    }

    // helper methods
    public List<string> GetSimplifiedWords()
    {
        List<string> result = null;

        if (this.NumericalSystem != null)
        {
            if (this.Book != null)
            {
                if (this.Book.Verses != null)
                {
                    SortedDictionary<string, int> word_frequencies = new SortedDictionary<string, int>();
                    if (word_frequencies != null)
                    {
                        List<Word> quran_words = new List<Word>();
                        if (quran_words != null)
                        {
                            foreach (Verse verse in this.Book.Verses)
                            {
                                quran_words.AddRange(verse.Words);
                            }
                        }

                        foreach (Word quran_word in quran_words)
                        {
                            string simplified_quran_word_text = quran_word.Text.Simplify(NumericalSystem.TextMode);
                            if (word_frequencies.ContainsKey(simplified_quran_word_text))
                            {
                                word_frequencies[simplified_quran_word_text]++;
                            }
                            else
                            {
                                word_frequencies.Add(simplified_quran_word_text, 1);
                            }
                        }

                        result = new List<string>(word_frequencies.Keys);
                    }
                }
            }
        }

        return result;
    }
    public SortedDictionary<string, int> GetSimplifiedWordFrequencies()
    {
        SortedDictionary<string, int> result = new SortedDictionary<string, int>();

        if (this.NumericalSystem != null)
        {
            if (this.Book != null)
            {
                if (this.Book.Verses != null)
                {
                    if (result != null)
                    {
                        List<Word> quran_words = new List<Word>();
                        if (quran_words != null)
                        {
                            foreach (Verse verse in this.Book.Verses)
                            {
                                quran_words.AddRange(verse.Words);
                            }
                        }

                        foreach (Word quran_word in quran_words)
                        {
                            if (result.ContainsKey(quran_word.Text))
                            {
                                result[quran_word.Text]++;
                            }
                            else
                            {
                                result.Add(quran_word.Text, 1);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private void FillFoundPartitionsWithFoundWords()
    {
        m_found_verses = new List<Verse>();
        m_found_chapters = new List<Chapter>();
        m_found_pages = new List<Page>();
        m_found_stations = new List<Station>();
        m_found_parts = new List<Part>();
        m_found_groups = new List<Group>();
        m_found_halfs = new List<Half>();
        m_found_quarters = new List<Quarter>();
        m_found_bowings = new List<Bowing>();

        if (m_found_words != null)
        {
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        if (!m_found_verses.Contains(word.Verse))
                        {
                            m_found_verses.Add(word.Verse);
                        }

                        if (!m_found_chapters.Contains(word.Verse.Chapter))
                        {
                            m_found_chapters.Add(word.Verse.Chapter);
                        }

                        if (!m_found_pages.Contains(word.Verse.Page))
                        {
                            m_found_pages.Add(word.Verse.Page);
                        }

                        if (!m_found_stations.Contains(word.Verse.Station))
                        {
                            m_found_stations.Add(word.Verse.Station);
                        }

                        if (!m_found_parts.Contains(word.Verse.Part))
                        {
                            m_found_parts.Add(word.Verse.Part);
                        }

                        if (!m_found_groups.Contains(word.Verse.Group))
                        {
                            m_found_groups.Add(word.Verse.Group);
                        }

                        if (!m_found_halfs.Contains(word.Verse.Half))
                        {
                            m_found_halfs.Add(word.Verse.Half);
                        }

                        if (!m_found_quarters.Contains(word.Verse.Quarter))
                        {
                            m_found_quarters.Add(word.Verse.Quarter);
                        }

                        if (!m_found_bowings.Contains(word.Verse.Bowing))
                        {
                            m_found_bowings.Add(word.Verse.Bowing);
                        }
                    }
                }
            }
        }
    }
    private void FillFoundPartitionsWithFoundPhrases()
    {
        //TODO m_found_letters = new List<Letter>();
        //TODO m_found_words = new List<Word>();
        m_found_verses = new List<Verse>();
        m_found_chapters = new List<Chapter>();
        m_found_pages = new List<Page>();
        m_found_stations = new List<Station>();
        m_found_parts = new List<Part>();
        m_found_groups = new List<Group>();
        m_found_halfs = new List<Half>();
        m_found_quarters = new List<Quarter>();
        m_found_bowings = new List<Bowing>();

        if (m_found_phrases != null)
        {
            foreach (Phrase phrase in m_found_phrases)
            {
                if (phrase != null)
                {
                    if (phrase.Verse != null)
                    {
                        //TODO m_found_letters
                        //TODO m_found_words

                        if (!m_found_verses.Contains(phrase.Verse))
                        {
                            m_found_verses.Add(phrase.Verse);
                        }

                        if (!m_found_chapters.Contains(phrase.Verse.Chapter))
                        {
                            m_found_chapters.Add(phrase.Verse.Chapter);
                        }

                        if (!m_found_pages.Contains(phrase.Verse.Page))
                        {
                            m_found_pages.Add(phrase.Verse.Page);
                        }

                        if (!m_found_stations.Contains(phrase.Verse.Station))
                        {
                            m_found_stations.Add(phrase.Verse.Station);
                        }

                        if (!m_found_parts.Contains(phrase.Verse.Part))
                        {
                            m_found_parts.Add(phrase.Verse.Part);
                        }

                        if (!m_found_groups.Contains(phrase.Verse.Group))
                        {
                            m_found_groups.Add(phrase.Verse.Group);
                        }

                        if (!m_found_halfs.Contains(phrase.Verse.Half))
                        {
                            m_found_halfs.Add(phrase.Verse.Half);
                        }

                        if (!m_found_quarters.Contains(phrase.Verse.Quarter))
                        {
                            m_found_quarters.Add(phrase.Verse.Quarter);
                        }

                        if (!m_found_bowings.Contains(phrase.Verse.Bowing))
                        {
                            m_found_bowings.Add(phrase.Verse.Bowing);
                        }
                    }
                }
            }
        }
    }
    // find by text - Exact
    /// <summary>
    /// Find phrases for given exact text that meet all parameters.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="language_type"></param>
    /// <param name="translation"></param>
    /// <param name="text_location_in_verse"></param>
    /// <param name="text_location_in_word"></param>
    /// <param name="case_sensitive"></param>
    /// <param name="text_wordness"></param>
    /// <param name="multiplicity"></param>
    /// <param name="at_word_start"></param>
    /// <returns>Number of found phrases. Result is stored in FoundPhrases.</returns>
    public int FindPhrases(TextSearchBlockSize text_search_block_size, string text, LanguageType language_type, string translation, TextLocationInChapter text_location_in_chapter, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        ClearSearchResults();
        m_found_phrases = Server.FindPhrases(m_search_scope, m_selection, m_found_verses, text_search_block_size, text, language_type, translation, text_location_in_chapter, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
        if (m_found_phrases != null)
        {
            FillFoundPartitionsWithFoundPhrases();

            Verse.CompareOrder = VerseCompareOrder.Ascending;
            Verse.CompareBy = VerseCompareBy.Number;
            m_found_verses.Sort();

            return m_found_phrases.Count;
        }
        return 0;
    }
    // find by text - Proximity
    /// <summary>
    /// Find phrases for given text by proximity that meet all parameters.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="language_type"></param>
    /// <param name="translation"></param>
    /// <param name="text_location_in_verse"></param>
    /// <param name="text_location_in_word"></param>
    /// <param name="case_sensitive"></param>
    /// <param name="text_wordness"></param>
    /// <param name="multiplicity"></param>
    /// <param name="at_word_start"></param>
    /// <returns>Number of found phrases. Result is stored in FoundPhrases.</returns>
    public int FindWords(TextSearchBlockSize text_search_block_size, string text, LanguageType language_type, string translation, TextWordGrouping text_word_grouping, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        ClearSearchResults();
        m_found_words = Server.FindWords(m_search_scope, m_selection, m_found_verses, text_search_block_size, text, language_type, translation, text_word_grouping, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
        if (m_found_words != null)
        {
            FillFoundPartitionsWithFoundWords();

            Verse.CompareOrder = VerseCompareOrder.Ascending;
            Verse.CompareBy = VerseCompareBy.Number;
            m_found_verses.Sort();

            return m_found_words.Count;
        }
        return 0;
    }
    // find by text - Root
    /// <summary>
    /// Find phrases for given text( or space separate roots) that meet all parameters.
    /// </summary>
    /// <param name="roots"></param>
    /// <param name="multiplicity"></param>
    /// <returns>Number of found phrases. Result is stored in FoundPhrases.</returns>
    public int FindWords(TextSearchBlockSize text_search_block_size, string roots, TextWordGrouping text_word_grouping, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        ClearSearchResults();
        m_found_words = Server.FindWords(m_search_scope, m_selection, m_found_verses, text_search_block_size, roots, text_word_grouping, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
        if (m_found_words != null)
        {
            FillFoundPartitionsWithFoundWords();

            Verse.CompareOrder = VerseCompareOrder.Ascending;
            Verse.CompareBy = VerseCompareBy.Number;
            m_found_verses.Sort();

            return m_found_words.Count;
        }
        return 0;
    }
    // find by text - Related words
    /// <summary>
    /// Find verses with related words from the same root
    /// </summary>
    /// <param name="verse"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindRelatedVerses(Verse verse)
    {
        ClearSearchResults();
        m_found_verses = Server.FindRelatedVerses(m_search_scope, m_selection, m_found_verses, verse);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }
    // find by text - Repetition
    public int FindConsecutivelyRepeatedWords(int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool with_diacritics)
    {
        ClearSearchResults();
        m_found_words = Server.FindConsecutivelyRepeatedWords(m_search_scope, m_selection, m_found_verses, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, with_diacritics);
        if (m_found_words != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (!m_found_verses.Contains(word.Verse))
                    {
                        m_found_verses.Add(word.Verse);
                    }
                }
            }
            return m_found_words.Count;
        }
        return 0;
    }
    public int FindConsecutivelyRepeatedRoots(int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        ClearSearchResults();
        m_found_words = Server.FindConsecutivelyRepeatedRoots(m_search_scope, m_selection, m_found_verses, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
        if (m_found_words != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (!m_found_verses.Contains(word.Verse))
                    {
                        m_found_verses.Add(word.Verse);
                    }
                }
            }
            return m_found_words.Count;
        }
        return 0;
    }
    public int FindConsecutivelyRepeatedValues(int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        ClearSearchResults();
        m_found_words = Server.FindConsecutivelyRepeatedValues(m_search_scope, m_selection, m_found_verses, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
        if (m_found_words != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (!m_found_verses.Contains(word.Verse))
                    {
                        m_found_verses.Add(word.Verse);
                    }
                }
            }
            return m_found_words.Count;
        }
        return 0;
    }


    // find by numbers - Letters
    /// <summary>
    /// Find letters that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found letters. Result is stored in FoundLetters.</returns>
    public int FindLetters(NumberQuery query)
    {
        ClearSearchResults();
        m_found_letters = Server.FindLetters(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_letters != null)
        {
            m_found_words = new List<Word>();
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (Letter letter in m_found_letters)
            {
                if (letter != null)
                {
                    if (letter.Word != null)
                    {
                        if (!m_found_words.Contains(letter.Word))
                        {
                            m_found_words.Add(letter.Word);
                        }

                        Verse verse = letter.Word.Verse;
                        if (!m_found_verses.Contains(verse))
                        {
                            m_found_verses.Add(verse);
                        }
                    }
                }

                Phrase phrase = new Phrase(letter.Word.Verse, letter.Word.Position + letter.NumberInWord - 1, letter.Text);
                if ((NumericalSystem.TextMode == "Original") || (NumericalSystem.TextMode == "SimplifiedMarks"))
                {
                    Server.SimplifyPhrase(phrase, NumericalSystem.TextMode);
                }
                m_found_phrases.Add(phrase);
            }
            return m_found_letters.Count;
        }
        return 0;
    }
    // find by numbers - LetterRanges
    /// <summary>
    /// Find letter ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found letter ranges. Result is stored in FoundLetterRanges.</returns>
    public int FindLetterRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_letter_ranges = Server.FindLetterRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_letter_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (List<Letter> range in m_found_letter_ranges)
            {
                if (range != null)
                {
                    if (range.Count > 0)
                    {
                        string range_text = null;
                        Verse previous_verse = null;
                        foreach (Letter letter in range)
                        {
                            if (letter != null)
                            {
                                if (letter.Word != null)
                                {
                                    // prepare found phrase verse
                                    Verse verse = letter.Word.Verse;

                                    // build found verses // prevent duplicate verses in case more than 1 range is found in same verse
                                    if (!m_found_verses.Contains(verse))
                                    {
                                        m_found_verses.Add(verse);
                                    }

                                    // prepare found phrase text
                                    if (previous_verse != null)
                                    {
                                        //    first letter in word     &&  word is not first in verse
                                        if ((letter.NumberInWord == 1) && (letter.Word.NumberInVerse > 1))
                                        {
                                            range_text += " " + letter.Text;
                                        }
                                        // first letter in new verse
                                        else if (letter.NumberInVerse == 1)
                                        {
                                            range_text += previous_verse.Endmark + verse.Address + "\t" + letter.Text;
                                        }
                                        else
                                        {
                                            range_text += letter.Text;
                                        }
                                    }
                                    else
                                    {
                                        range_text += letter.Text;
                                    }

                                    if ((NumericalSystem.TextMode == "Original") || (NumericalSystem.TextMode == "SimplifiedMarks"))
                                    {
                                        if (letter.Word.Stopmark != Stopmark.None)
                                        {
                                            range_text += StopmarkHelper.GetStopmarkText(letter.Word.Stopmark) + " ";
                                        }
                                    }

                                    previous_verse = verse;
                                }
                            }
                        }

                        // build found phrases // allow multiple phrases even if overlapping inside same verse
                        Phrase phrase = new Phrase(range[0].Word.Verse, range[0].Word.Position + range[0].NumberInWord - 1, range_text);
                        if (phrase != null)
                        {
                            m_found_phrases.Add(phrase);
                        }
                    }
                }
            }
            return m_found_letter_ranges.Count;
        }
        return 0;
    }
    // find by numbers - LetterSets
    /// <summary>
    /// Find letter sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found letter sets. Result is stored in FoundLetterSets.</returns>
    public int FindLetterSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_letter_sets = Server.FindLetterSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_letter_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (List<Letter> set in m_found_letter_sets)
            {
                if (set != null)
                {
                    if (set.Count > 0)
                    {
                        string set_text = null;
                        foreach (Letter letter in set)
                        {
                            if (letter != null)
                            {
                                if (letter.Word != null)
                                {
                                    // prepare found phrase verse
                                    Verse verse = letter.Word.Verse;

                                    // build found verses // prevent duplicate verses in case more than 1 set is found in same verse
                                    if (!m_found_verses.Contains(verse))
                                    {
                                        m_found_verses.Add(verse);
                                    }

                                    // prepare found phrase text
                                    set_text = letter.Text;

                                    if ((NumericalSystem.TextMode == "Original") || (NumericalSystem.TextMode == "SimplifiedMarks"))
                                    {
                                        if (letter.Word.Stopmark != Stopmark.None)
                                        {
                                            set_text += StopmarkHelper.GetStopmarkText(letter.Word.Stopmark) + " ";
                                        }
                                    }

                                    // build found phrases // allow multiple phrases even if overlapping inside same verse
                                    Phrase phrase = new Phrase(letter.Word.Verse, letter.Word.Position + letter.NumberInWord - 1, set_text);
                                    if (phrase != null)
                                    {
                                        m_found_phrases.Add(phrase);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return m_found_letter_sets.Count;
        }
        return 0;
    }

    // find by numbers - Words
    /// <summary>
    /// Find words that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found words. Result is stored in FoundWords.</returns>
    public int FindWords(NumberQuery query)
    {
        ClearSearchResults();
        m_found_words = Server.FindWords(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_words != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (!m_found_verses.Contains(word.Verse))
                    {
                        m_found_verses.Add(word.Verse);
                    }
                }
            }
            return m_found_words.Count;
        }
        return 0;
    }
    // find by numbers - WordRanges
    /// <summary>
    /// Find word ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found word ranges. Result is stored in FoundWordRanges.</returns>
    public int FindWordRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_word_ranges = Server.FindWordRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_word_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (List<Word> range in m_found_word_ranges)
            {
                if (range != null)
                {
                    if (range.Count > 0)
                    {
                        string range_text = null;
                        foreach (Word word in range)
                        {
                            if (word != null)
                            {
                                // prepare found phrase verse
                                Verse verse = word.Verse;

                                // build found verses // prevent duplicate verses in case more than 1 range is found in same verse
                                if (!m_found_verses.Contains(verse))
                                {
                                    m_found_verses.Add(verse);
                                }

                                // prepare found phrase text
                                range_text += word.Text + " ";
                                if ((NumericalSystem.TextMode == "Original") || (NumericalSystem.TextMode == "SimplifiedMarks"))
                                {
                                    if (word.Stopmark != Stopmark.None)
                                    {
                                        range_text += StopmarkHelper.GetStopmarkText(word.Stopmark) + " ";
                                    }
                                }
                            }
                        }
                        range_text = range_text.Remove(range_text.Length - 1, 1);

                        // build found phrases // allow multiple phrases even if overlapping inside same verse
                        Phrase phrase = new Phrase(range[0].Verse, range[0].Position, range_text);
                        if (phrase != null)
                        {
                            m_found_phrases.Add(phrase);
                        }
                    }
                }
            }
            return m_found_word_ranges.Count;
        }
        return 0;
    }
    // find by numbers - WordSets
    /// <summary>
    /// Find word sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found word sets. Result is stored in FoundWordSets.</returns>
    public int FindWordSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_word_sets = Server.FindWordSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_word_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (List<Word> set in m_found_word_sets)
            {
                if (set != null)
                {
                    if (set.Count > 0)
                    {
                        string set_text = null;
                        foreach (Word word in set)
                        {
                            if (word != null)
                            {
                                // prepare found phrase verse
                                Verse verse = word.Verse;

                                // build found verses // prevent duplicate verses in case more than 1 set is found in same verse
                                if (!m_found_verses.Contains(verse))
                                {
                                    m_found_verses.Add(verse);
                                }

                                // prepare found phrase text
                                set_text += word.Text + " ";
                                if ((NumericalSystem.TextMode == "Original") || (NumericalSystem.TextMode == "SimplifiedMarks"))
                                {
                                    if (word.Stopmark != Stopmark.None)
                                    {
                                        set_text += StopmarkHelper.GetStopmarkText(word.Stopmark) + " ";
                                    }
                                }
                            }
                        }
                        set_text = set_text.Remove(set_text.Length - 1, 1);

                        // build found phrases // allow multiple phrases even if overlapping inside same verse
                        Phrase phrase = new Phrase(set[0].Verse, set[0].Position, set_text);
                        if (phrase != null)
                        {
                            m_found_phrases.Add(phrase);
                        }
                    }
                }
            }
            return m_found_word_sets.Count;
        }
        return 0;
    }

    // find by numbers - Sentences
    /// <summary>
    /// Find sentences across verses that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found sentences. Result is stored in FoundSentences.</returns>
    public int FindSentences(NumberQuery query)
    {
        ClearSearchResults();
        m_found_sentences = Server.FindSentences(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_sentences != null)
        {
            BuildSentencePhrases();

            return m_found_sentences.Count;
        }
        return 0;
    }
    private void BuildSentencePhrases()
    {
        if (m_found_sentences != null)
        {
            m_found_verses = new List<Verse>();
            m_found_phrases = new List<Phrase>();
            foreach (Sentence sentence in m_found_sentences)
            {
                if (sentence != null)
                {
                    Verse first_verse = sentence.FirstVerse;
                    Verse last_verse = sentence.LastVerse;
                    if ((first_verse != null) && (last_verse != null))
                    {
                        int start = first_verse.Number - 1;
                        int end = last_verse.Number - 1;
                        if (end >= start)
                        {
                            // add unique verses
                            for (int i = start; i <= end; i++)
                            {
                                if (!m_found_verses.Contains(Book.Verses[i]))
                                {
                                    m_found_verses.Add(Book.Verses[i]);
                                }
                            }

                            // build phrases for colorization
                            if (start == end) // sentence within verse
                            {
                                Phrase sentence_phrase = new Phrase(first_verse, sentence.StartPosition, sentence.Text);
                                m_found_phrases.Add(sentence_phrase);
                            }
                            else // sentence across verses
                            {
                                // first verse
                                string start_text = first_verse.Text.Substring(sentence.StartPosition);
                                Phrase start_phrase = new Phrase(sentence.FirstVerse, sentence.StartPosition, start_text);
                                m_found_phrases.Add(start_phrase);

                                // middle verses
                                for (int i = start + 1; i < end; i++)
                                {
                                    Verse verse = Book.Verses[i];
                                    if (verse != null)
                                    {
                                        Phrase middle_phrase = new Phrase(verse, 0, verse.Text);
                                        m_found_phrases.Add(middle_phrase);
                                    }
                                }

                                // last verse
                                string end_text = last_verse.Text.Substring(0, sentence.EndPosition);
                                Phrase end_phrase = new Phrase(last_verse, 0, end_text);
                                m_found_phrases.Add(end_phrase);
                            }
                        }
                    }
                }
            }
        }
    }

    // find by numbers - Verses
    /// <summary>
    /// Find verses that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindVerses(NumberQuery query)
    {
        ClearSearchResults();
        m_found_verses = Server.FindVerses(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }
    // find by numbers - VerseRanges
    /// <summary>
    /// Find verse ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found verse ranges. Result is stored in FoundVerseRanges.</returns>
    public int FindVerseRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_verse_ranges = Server.FindVerseRanges(m_search_scope, m_selection, m_found_verses, query);
        m_found_verses = new List<Verse>();
        if (m_found_verse_ranges != null)
        {
            foreach (List<Verse> range in m_found_verse_ranges)
            {
                m_found_verses.AddRange(range);
            }
            return m_found_verse_ranges.Count;
        }
        return 0;
    }
    // find by numbers - VerseSets
    /// <summary>
    /// Find verse sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found verse sets. Result is stored in FoundVerseSets.</returns>
    public int FindVerseSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_verse_sets = Server.FindVerseSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_verse_sets != null)
        {
            m_found_verses = new List<Verse>();
            foreach (List<Verse> set in m_found_verse_sets)
            {
                m_found_verses.AddRange(set);
            }
            return m_found_verse_sets.Count;
        }
        return 0;
    }

    // find by numbers - Chapters
    /// <summary>
    /// Find chapters that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found chapters. Result is stored in FoundChapters.</returns>
    public int FindChapters(NumberQuery query)
    {
        ClearSearchResults();
        m_found_chapters = Server.FindChapters(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_chapters != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Chapter chapter in m_found_chapters)
            {
                if (chapter != null)
                {
                    m_found_verses.AddRange(chapter.Verses);
                    m_found_verse_ranges.Add(chapter.Verses);
                }
            }
            return m_found_chapters.Count;
        }
        return 0;
    }
    // find by numbers - ChapterRanges
    /// <summary>
    /// Find chapter ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found chapter ranges. Result is stored in FoundChapterRanges.</returns>
    public int FindChapterRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_chapter_ranges = Server.FindChapterRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_chapter_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Chapter> range in m_found_chapter_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Chapter chapter in range)
                {
                    if (chapter != null)
                    {
                        m_found_verses.AddRange(chapter.Verses);
                        verses.AddRange(chapter.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_chapter_ranges.Count;
        }
        return 0;
    }
    // find by numbers - ChapterSets
    /// <summary>
    /// Find chapter sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found chapter sets. Result is stored in FoundChapterSets.</returns>
    public int FindChapterSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_chapter_sets = Server.FindChapterSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_chapter_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Chapter> set in m_found_chapter_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Chapter chapter in set)
                {
                    if (chapter != null)
                    {
                        m_found_verses.AddRange(chapter.Verses);
                        verses.AddRange(chapter.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_chapter_sets.Count;
        }
        return 0;
    }

    // find by numbers - Pages
    /// <summary>
    /// Find pages that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found pages. Result is stored in FoundPages.</returns>
    public int FindPages(NumberQuery query)
    {
        ClearSearchResults();
        m_found_pages = Server.FindPages(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_pages != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Page page in m_found_pages)
            {
                if (page != null)
                {
                    m_found_verses.AddRange(page.Verses);
                    m_found_verse_ranges.Add(page.Verses);
                }
            }
            return m_found_pages.Count;
        }
        return 0;
    }
    // find by numbers - PageRanges
    /// <summary>
    /// Find page ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found page ranges. Result is stored in FoundPageRanges.</returns>
    public int FindPageRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_page_ranges = Server.FindPageRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_page_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Page> range in m_found_page_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Page page in range)
                {
                    if (page != null)
                    {
                        m_found_verses.AddRange(page.Verses);
                        verses.AddRange(page.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_page_ranges.Count;
        }
        return 0;
    }
    // find by numbers - PageSets
    /// <summary>
    /// Find page sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found page sets. Result is stored in FoundPageSets.</returns>
    public int FindPageSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_page_sets = Server.FindPageSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_page_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Page> set in m_found_page_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Page page in set)
                {
                    if (page != null)
                    {
                        m_found_verses.AddRange(page.Verses);
                        verses.AddRange(page.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_page_sets.Count;
        }
        return 0;
    }

    // find by numbers - Stations
    /// <summary>
    /// Find stations that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found stations. Result is stored in FoundStations.</returns>
    public int FindStations(NumberQuery query)
    {
        ClearSearchResults();
        m_found_stations = Server.FindStations(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_stations != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Station station in m_found_stations)
            {
                if (station != null)
                {
                    m_found_verses.AddRange(station.Verses);
                    m_found_verse_ranges.Add(station.Verses);
                }
            }
            return m_found_stations.Count;
        }
        return 0;
    }
    // find by numbers - StationRanges
    /// <summary>
    /// Find station ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found station ranges. Result is stored in FoundStationRanges.</returns>
    public int FindStationRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_station_ranges = Server.FindStationRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_station_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Station> range in m_found_station_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Station station in range)
                {
                    if (station != null)
                    {
                        m_found_verses.AddRange(station.Verses);
                        verses.AddRange(station.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_station_ranges.Count;
        }
        return 0;
    }
    // find by numbers - StationSets
    /// <summary>
    /// Find station sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found station sets. Result is stored in FoundStationSets.</returns>
    public int FindStationSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_station_sets = Server.FindStationSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_station_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Station> set in m_found_station_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Station station in set)
                {
                    if (station != null)
                    {
                        m_found_verses.AddRange(station.Verses);
                        verses.AddRange(station.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_station_sets.Count;
        }
        return 0;
    }

    // find by numbers - Parts
    /// <summary>
    /// Find parts that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found parts. Result is stored in FoundParts.</returns>
    public int FindParts(NumberQuery query)
    {
        ClearSearchResults();
        m_found_parts = Server.FindParts(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_parts != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Part part in m_found_parts)
            {
                if (part != null)
                {
                    m_found_verses.AddRange(part.Verses);
                    m_found_verse_ranges.Add(part.Verses);
                }
            }
            return m_found_parts.Count;
        }
        return 0;
    }
    // find by numbers - PartRanges
    /// <summary>
    /// Find part ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found part ranges. Result is stored in FoundPartRanges.</returns>
    public int FindPartRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_part_ranges = Server.FindPartRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_part_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Part> range in m_found_part_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Part part in range)
                {
                    if (part != null)
                    {
                        m_found_verses.AddRange(part.Verses);
                        verses.AddRange(part.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_part_ranges.Count;
        }
        return 0;
    }
    // find by numbers - PartSets
    /// <summary>
    /// Find part sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found part sets. Result is stored in FoundPartSets.</returns>
    public int FindPartSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_part_sets = Server.FindPartSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_part_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Part> set in m_found_part_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Part part in set)
                {
                    if (part != null)
                    {
                        m_found_verses.AddRange(part.Verses);
                        verses.AddRange(part.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_part_sets.Count;
        }
        return 0;
    }

    // find by numbers - Groups
    /// <summary>
    /// Find groups that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found groups. Result is stored in FoundGroups.</returns>
    public int FindGroups(NumberQuery query)
    {
        ClearSearchResults();
        m_found_groups = Server.FindGroups(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_groups != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Group group in m_found_groups)
            {
                if (group != null)
                {
                    m_found_verses.AddRange(group.Verses);
                    m_found_verse_ranges.Add(group.Verses);
                }
            }
            return m_found_groups.Count;
        }
        return 0;
    }
    // find by numbers - GroupRanges
    /// <summary>
    /// Find group ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found group ranges. Result is stored in FoundGroupRanges.</returns>
    public int FindGroupRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_group_ranges = Server.FindGroupRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_group_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Group> range in m_found_group_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Group group in range)
                {
                    if (group != null)
                    {
                        m_found_verses.AddRange(group.Verses);
                        verses.AddRange(group.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_group_ranges.Count;
        }
        return 0;
    }
    // find by numbers - GroupSets
    /// <summary>
    /// Find group sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found group sets. Result is stored in FoundGroupSets.</returns>
    public int FindGroupSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_group_sets = Server.FindGroupSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_group_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Group> set in m_found_group_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Group group in set)
                {
                    if (group != null)
                    {
                        m_found_verses.AddRange(group.Verses);
                        verses.AddRange(group.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_group_sets.Count;
        }
        return 0;
    }

    // find by numbers - Halfs
    /// <summary>
    /// Find halfs that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found halfs. Result is stored in FoundHalfs.</returns>
    public int FindHalfs(NumberQuery query)
    {
        ClearSearchResults();
        m_found_halfs = Server.FindHalfs(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_halfs != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Half half in m_found_halfs)
            {
                if (half != null)
                {
                    m_found_verses.AddRange(half.Verses);
                    m_found_verse_ranges.Add(half.Verses);
                }
            }
            return m_found_halfs.Count;
        }
        return 0;
    }
    // find by numbers - HalfRanges
    /// <summary>
    /// Find half ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found half ranges. Result is stored in FoundHalfRanges.</returns>
    public int FindHalfRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_half_ranges = Server.FindHalfRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_half_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Half> range in m_found_half_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Half half in range)
                {
                    if (half != null)
                    {
                        m_found_verses.AddRange(half.Verses);
                        verses.AddRange(half.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_half_ranges.Count;
        }
        return 0;
    }
    // find by numbers - HalfSets
    /// <summary>
    /// Find half sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found half sets. Result is stored in FoundHalfSets.</returns>
    public int FindHalfSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_half_sets = Server.FindHalfSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_half_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Half> set in m_found_half_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Half half in set)
                {
                    if (half != null)
                    {
                        m_found_verses.AddRange(half.Verses);
                        verses.AddRange(half.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_half_sets.Count;
        }
        return 0;
    }

    // find by numbers - Quarters
    /// <summary>
    /// Find quarters that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found quarters. Result is stored in FoundQuarters.</returns>
    public int FindQuarters(NumberQuery query)
    {
        ClearSearchResults();
        m_found_quarters = Server.FindQuarters(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_quarters != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Quarter quarter in m_found_quarters)
            {
                if (quarter != null)
                {
                    m_found_verses.AddRange(quarter.Verses);
                    m_found_verse_ranges.Add(quarter.Verses);
                }
            }
            return m_found_quarters.Count;
        }
        return 0;
    }
    // find by numbers - QuarterRanges
    /// <summary>
    /// Find quarter ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found quarter ranges. Result is stored in FoundQuarterRanges.</returns>
    public int FindQuarterRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_quarter_ranges = Server.FindQuarterRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_quarter_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Quarter> range in m_found_quarter_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Quarter quarter in range)
                {
                    if (quarter != null)
                    {
                        m_found_verses.AddRange(quarter.Verses);
                        verses.AddRange(quarter.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_quarter_ranges.Count;
        }
        return 0;
    }
    // find by numbers - QuarterSets
    /// <summary>
    /// Find quarter sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found quarter sets. Result is stored in FoundQuarterSets.</returns>
    public int FindQuarterSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_quarter_sets = Server.FindQuarterSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_quarter_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Quarter> set in m_found_quarter_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Quarter quarter in set)
                {
                    if (quarter != null)
                    {
                        m_found_verses.AddRange(quarter.Verses);
                        verses.AddRange(quarter.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_quarter_sets.Count;
        }
        return 0;
    }

    // find by numbers - Bowings
    /// <summary>
    /// Find bowings that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found bowings. Result is stored in FoundBowings.</returns>
    public int FindBowings(NumberQuery query)
    {
        ClearSearchResults();
        m_found_bowings = Server.FindBowings(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_bowings != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (Bowing bowing in m_found_bowings)
            {
                if (bowing != null)
                {
                    m_found_verses.AddRange(bowing.Verses);
                    m_found_verse_ranges.Add(bowing.Verses);
                }
            }
            return m_found_bowings.Count;
        }
        return 0;
    }
    // find by numbers - BowingRanges
    /// <summary>
    /// Find bowing ranges that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found bowing ranges. Result is stored in FoundBowingRanges.</returns>
    public int FindBowingRanges(NumberQuery query)
    {
        ClearSearchResults();
        m_found_bowing_ranges = Server.FindBowingRanges(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_bowing_ranges != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_ranges = new List<List<Verse>>();
            foreach (List<Bowing> range in m_found_bowing_ranges)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Bowing bowing in range)
                {
                    if (bowing != null)
                    {
                        m_found_verses.AddRange(bowing.Verses);
                        verses.AddRange(bowing.Verses);
                    }
                }
                m_found_verse_ranges.Add(verses);
            }
            return m_found_bowing_ranges.Count;
        }
        return 0;
    }
    // find by numbers - BowingSets
    /// <summary>
    /// Find bowing sets that meet query criteria.
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found bowing sets. Result is stored in FoundBowingSets.</returns>
    public int FindBowingSets(NumberQuery query)
    {
        ClearSearchResults();
        m_found_bowing_sets = Server.FindBowingSets(m_search_scope, m_selection, m_found_verses, query);
        if (m_found_bowing_sets != null)
        {
            m_found_verses = new List<Verse>();
            m_found_verse_sets = new List<List<Verse>>();
            foreach (List<Bowing> set in m_found_bowing_sets)
            {
                List<Verse> verses = new List<Verse>();
                foreach (Bowing bowing in set)
                {
                    if (bowing != null)
                    {
                        m_found_verses.AddRange(bowing.Verses);
                        verses.AddRange(bowing.Verses);
                    }
                }
                m_found_verse_sets.Add(verses);
            }
            return m_found_bowing_sets.Count;
        }
        return 0;
    }



    // find by similarity - verses similar to given verse
    /// <summary>
    /// Find verses with similar text to verse text with given similarity percentage or more using given similarity method
    /// </summary>
    /// <param name="verse"></param>
    /// <param name="similarity_method"></param>
    /// <param name="similarity_percentage"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindVerses(Verse verse, SimilarityMethod similarity_method, double similarity_percentage)
    {
        ClearSearchResults();
        m_found_verses = Server.FindVerses(m_search_scope, m_selection, m_found_verses, verse, similarity_method, similarity_percentage);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }
    // find by similarity - all similar verses to each other throughout the book
    /// <summary>
    /// Find verse ranges with similar text to each other with given similarity percentage or more.
    /// </summary>
    /// <param name="similarity_method"></param>
    /// <param name="similarity_percentage"></param>
    /// <returns>Number of found verse ranges. Result is stored in FoundVerseRanges.</returns>
    public int FindVerses(SimilarityMethod similarity_method, double similarity_percentage)
    {
        ClearSearchResults();
        m_found_verse_ranges = Server.FindVersess(m_search_scope, m_selection, m_found_verses, similarity_method, similarity_percentage);
        if (m_found_verse_ranges != null)
        {
            m_found_verses = new List<Verse>();
            foreach (List<Verse> verse_range in m_found_verse_ranges)
            {
                m_found_verses.AddRange(verse_range);
            }
            return m_found_verse_ranges.Count;
        }
        return 0;
    }


    // find by sum - Verses
    /// <summary>
    /// Find verses with C+V+Ws+Ls = c_v_ws_ls_sum
    /// </summary>
    /// <param name="query"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindVerses(CVWLSumType cvwl_sum_type, long cvwl_sum)
    {
        ClearSearchResults();
        m_found_verses = Server.FindVerses(m_search_scope, m_selection, m_found_verses, cvwl_sum_type, cvwl_sum);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }



    // find by prostration type
    /// <summary>
    /// Find verses with given prostration type.
    /// </summary>
    /// <param name="prostration_type"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindVerses(ProstrationType prostration_type)
    {
        ClearSearchResults();
        m_found_verses = Server.FindVerses(m_search_scope, m_selection, m_found_verses, prostration_type);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }


    // find by revelation place
    /// <summary>
    /// Find chapters that were revealed at given revelation place.
    /// </summary>
    /// <param name="revelation_place"></param>
    /// <returns>Number of found chapters. Result is stored in FoundChapters.</returns>
    public int FindChapters(RevelationPlace revelation_place)
    {
        ClearSearchResults();
        m_found_chapters = Server.FindChapters(m_search_scope, m_selection, m_found_verses, revelation_place);
        if (m_found_chapters != null)
        {
            if (m_found_chapters != null)
            {
                m_found_verses = new List<Verse>();
                foreach (Chapter chapter in m_found_chapters)
                {
                    if (chapter != null)
                    {
                        m_found_verses.AddRange(chapter.Verses);
                    }
                }
                return m_found_chapters.Count;
            }
        }
        return 0;
    }


    // find by initialization type
    /// <summary>
    /// Find verses with given initialization type.
    /// </summary>
    /// <param name="initialization_type"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindVerses(InitializationType initialization_type)
    {
        ClearSearchResults();
        m_found_verses = Server.FindVerses(m_search_scope, m_selection, m_found_verses, initialization_type);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }


    // find by letter frequency sum
    /// <summary>
    /// Find words with required letter frequency sum in their text of the given phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="sum"></param>
    /// <param name="frequency_search_type"></param>
    /// <returns>Number of found words. Result is stored in FoundWords.</returns>
    public int FindWords(string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_words = Server.FindWords(m_search_scope, m_selection, m_found_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
        if (m_found_words != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (!m_found_verses.Contains(word.Verse))
                    {
                        m_found_verses.Add(word.Verse);
                    }
                }
            }
            return m_found_words.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find word ranges with required letter frequency sum in their text of the given phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="sum"></param>
    /// <param name="frequency_search_type"></param>
    /// <returns>Number of found word ranges. Result is stored in FoundWordRanges.</returns>
    public int FindWordRanges(string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_word_ranges = Server.FindWordRanges(m_search_scope, m_selection, m_found_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
        if (m_found_word_ranges != null)
        {
            m_found_verses = new List<Verse>();
            foreach (List<Word> range in m_found_word_ranges)
            {
                if (range != null)
                {
                    if (range.Count > 0)
                    {
                        foreach (Word word in range)
                        {
                            if (word != null)
                            {
                                // build found verses // prevent duplicate verses in case more than 1 range is found in same verse
                                Verse verse = word.Verse;
                                if (!m_found_verses.Contains(verse))
                                {
                                    m_found_verses.Add(verse);
                                }
                            }
                        }
                    }
                }
            }
            return m_found_word_ranges.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find sentences across verses with required letter frequency sum in their text of the given phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="sum"></param>
    /// <param name="frequency_search_type"></param>
    /// <returns>Number of found sentences. Result is stored in FoundSentences.</returns>
    public int FindSentences(string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_sentences = Server.FindSentences(m_search_scope, m_selection, m_found_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
        if (m_found_sentences != null)
        {
            BuildSentencePhrases();

            return m_found_sentences.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find verses with required letter frequency sum in their text of the given phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="sum"></param>
    /// <param name="frequency_search_type"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindVerses(string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_verses = Server.FindVerses(m_search_scope, m_selection, m_found_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find verse ranges with required letter frequency sum in their text of the given phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="sum"></param>
    /// <param name="frequency_search_type"></param>
    /// <returns>Number of found verse ranges. Result is stored in FoundVerseRanges.</returns>
    public int FindVerseRanges(string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_verse_ranges = Server.FindVerseRanges(m_search_scope, m_selection, m_found_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
        if (m_found_verse_ranges != null)
        {
            m_found_verses = new List<Verse>();
            foreach (List<Verse> range in m_found_verse_ranges)
            {
                if (range != null)
                {
                    if (range.Count > 0)
                    {
                        foreach (Verse verse in range)
                        {
                            if (verse != null)
                            {
                                // build found verses // prevent duplicate verses in case more than 1 range is found in same verse
                                if (!m_found_verses.Contains(verse))
                                {
                                    m_found_verses.Add(verse);
                                }
                            }
                        }
                    }
                }
            }
            return m_found_verse_ranges.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find chapters with required letter frequency sum in their text of the given phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="sum"></param>
    /// <param name="frequency_search_type"></param>
    /// <returns>Number of found chapters. Result is stored in FoundChapters.</returns>
    public int FindChapters(string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_chapters = Server.FindChapters(m_search_scope, m_selection, m_found_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
        if (m_found_chapters != null)
        {
            if (m_found_chapters != null)
            {
                m_found_verses = new List<Verse>();
                foreach (Chapter chapter in m_found_chapters)
                {
                    if (chapter != null)
                    {
                        m_found_verses.AddRange(chapter.Verses);
                    }
                }
                return m_found_chapters.Count;
            }
        }
        return 0;
    }
    /// <summary>
    /// Find chapter ranges with required letter frequency sum in their text of the given phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="sum"></param>
    /// <param name="frequency_search_type"></param>
    /// <returns>Number of found verse ranges. Result is stored in FoundChapterRanges.</returns>
    public int FindChapterRanges(string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_chapter_ranges = Server.FindChapterRanges(m_search_scope, m_selection, m_found_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
        if (m_found_chapter_ranges != null)
        {
            m_found_chapters = new List<Chapter>();
            foreach (List<Chapter> range in m_found_chapter_ranges)
            {
                if (range != null)
                {
                    if (range.Count > 0)
                    {
                        foreach (Chapter chapter in range)
                        {
                            if (chapter != null)
                            {
                                // build found chapters // prevent duplicate verses in case more than 1 range is found in same verse
                                if (!m_found_chapters.Contains(chapter))
                                {
                                    m_found_chapters.Add(chapter);
                                }
                            }
                        }
                    }
                }
            }
            return m_found_chapter_ranges.Count;
        }
        return 0;
    }
    public int CalculateLetterFrequencySum(string text, string phrase, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return Server.CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
    }

    /// <summary>
    /// Find words with required letter matching with optional frequency sum in phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="frequency_matching_type"></param>
    /// <param name="frequency_search_type"></param>
    /// <param name="include_diacritics"></param>
    /// <returns>Number of found words. Result is stored in FoundWords.</returns>
    public int FindWords(string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_words = Server.FindWords(m_search_scope, m_selection, m_found_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
        if (m_found_words != null)
        {
            m_found_verses = new List<Verse>();
            foreach (Word word in m_found_words)
            {
                if (word != null)
                {
                    if (!m_found_verses.Contains(word.Verse))
                    {
                        m_found_verses.Add(word.Verse);
                    }
                }
            }
            return m_found_words.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find sentences across verses with required letter matching with optional frequency sum in phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="frequency_matching_type"></param>
    /// <param name="frequency_search_type"></param>
    /// <param name="include_diacritics"></param>
    /// <returns>Number of found sentences. Result is stored in FoundSentences.</returns>
    public int FindSentences(string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_sentences = Server.FindSentences(m_search_scope, m_selection, m_found_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
        if (m_found_sentences != null)
        {
            BuildSentencePhrases();

            return m_found_sentences.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find verses verses with required letter matching with optional frequency sum in phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="frequency_matching_type"></param>
    /// <param name="frequency_search_type"></param>
    /// <param name="include_diacritics"></param>
    /// <returns>Number of found verses. Result is stored in FoundVerses.</returns>
    public int FindVerses(string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_verses = Server.FindVerses(m_search_scope, m_selection, m_found_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
        if (m_found_verses != null)
        {
            return m_found_verses.Count;
        }
        return 0;
    }
    /// <summary>
    /// Find chapters verses with required letter matching with optional frequency sum in phrase.
    /// </summary>
    /// <param name="phrase"></param>
    /// <param name="frequency_matching_type"></param>
    /// <param name="frequency_search_type"></param>
    /// <param name="include_diacritics"></param>
    /// <returns>Number of found chapters. Result is stored in FoundChapters.</returns>
    public int FindChapters(string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        ClearSearchResults();
        m_found_chapters = Server.FindChapters(m_search_scope, m_selection, m_found_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
        if (m_found_chapters != null)
        {
            if (m_found_chapters != null)
            {
                m_found_verses = new List<Verse>();
                foreach (Chapter chapter in m_found_chapters)
                {
                    if (chapter != null)
                    {
                        m_found_verses.AddRange(chapter.Verses);
                    }
                }
                return m_found_chapters.Count;
            }
        }
        return 0;
    }


    // browse history
    private List<object> m_history_items = new List<object>();
    public List<object> HistoryItems
    {
        get { return m_history_items; }
    }
    private int m_history_item_index = -1;
    public int HistoryItemIndex
    {
        get { return m_history_item_index; }
    }
    public object CurrentHistoryItem
    {
        get
        {
            if (m_history_items != null)
            {
                if ((m_history_item_index >= 0) && (m_history_item_index < m_history_items.Count))
                {
                    return m_history_items[m_history_item_index];
                }
            }
            return null;
        }
    }
    public void AddHistoryItem(object item)
    {
        if (m_history_items != null)
        {
            m_history_items.Add(item);
            m_history_item_index = m_history_items.Count - 1;
        }
    }
    public void DeleteHistoryItem(object item)
    {
        if (m_history_items != null)
        {
            m_history_items.Remove(item);
            m_history_item_index = m_history_items.Count - 1;
        }
    }
    public void DeleteCurrentHistoryItem()
    {
        if (m_history_items != null)
        {
            if ((m_history_item_index >= 0) && (m_history_item_index < m_history_items.Count))
            {
                object item = m_history_items[m_history_item_index];
                m_history_items.Remove(item);
                m_history_item_index = m_history_items.Count - 1;
            }

            if (m_history_items.Count == 0) // all items deleted
            {
                m_history_item_index = -1;
            }
            else // there are still some item(s)
            {
                // if index becomes outside list, move back into list
                if (m_history_item_index == m_history_items.Count)
                {
                    m_history_item_index = m_history_items.Count - 1;
                }
            }
        }
    }
    public void ClearHistoryItems()
    {
        if (m_history_items != null)
        {
            m_history_items.Clear();
            m_history_item_index = -1;
        }
    }
    public object GotoPreviousHistoryItem()
    {
        object result = null;
        if (m_history_items != null)
        {
            if ((m_history_item_index > 0) && (m_history_item_index < m_history_items.Count))
            {
                m_history_item_index--;
                result = m_history_items[m_history_item_index];
            }
        }
        return result;
    }
    public object GotoNextHistoryItem()
    {
        object result = null;
        if (m_history_items != null)
        {
            if ((m_history_item_index >= -1) && (m_history_item_index < m_history_items.Count - 1))
            {
                m_history_item_index++;
                result = m_history_items[m_history_item_index];
            }
        }
        return result;
    }
    public void SaveHistoryItems()
    {
        if (m_history_items != null)
        {
            if (Directory.Exists(Globals.HISTORY_FOLDER))
            {
                string path = Globals.HISTORY_FOLDER + Path.DirectorySeparatorChar + "History.txt";
                try
                {
                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                    {
                        StringBuilder str = new StringBuilder();

                        foreach (object history_item in m_history_items)
                        {
                            if (history_item is BrowseHistoryItem)
                            {
                                BrowseHistoryItem item = history_item as BrowseHistoryItem;
                                if (item != null)
                                {
                                    str.AppendLine("BrowseHistoryItem");
                                    str.AppendLine(item.Scope.ToString());
                                    if (item.Indexes.Count > 0)
                                    {
                                        foreach (int index in item.Indexes)
                                        {
                                            str.Append(index.ToString() + ",");
                                        }
                                        if (str.Length > 1)
                                        {
                                            str.Remove(str.Length - 1, 1);
                                        }
                                    }
                                    str.AppendLine();
                                    str.AppendLine(END_OF_HISTORY_ITME_MARKER);
                                }
                            }
                            else if (history_item is SearchHistoryItem)
                            {
                                SearchHistoryItem item = history_item as SearchHistoryItem;
                                if (item != null)
                                {
                                    str.AppendLine("SearchHistoryItem");
                                    str.AppendLine("SearchType" + "\t" + item.SearchType);
                                    str.AppendLine("NumbersResultType" + "\t" + item.NumbersResultType);
                                    str.AppendLine("Text" + "\t" + item.Text);
                                    str.AppendLine("Header" + "\t" + item.Header);
                                    str.AppendLine("Language" + "\t" + item.LanguageType);
                                    str.AppendLine("Translation" + "\t" + item.Translation);

                                    if ((item.Phrases != null) && (item.Phrases.Count > 0))
                                    {
                                        foreach (Phrase phrase in item.Phrases)
                                        {
                                            if (phrase != null)
                                            {
                                                str.AppendLine(phrase.Verse.Number.ToString() + "," + phrase.Text + "," + phrase.Position.ToString());
                                            }
                                        }
                                    }
                                    else // verse.Number
                                    {
                                        //TODO Save NumberQuery with each search result

                                        foreach (Verse verse in item.Verses)
                                        {
                                            str.AppendLine(verse.Number.ToString());
                                        }
                                    }
                                    str.AppendLine(END_OF_HISTORY_ITME_MARKER);
                                }
                            }
                        }
                        writer.Write(str.ToString());
                    }
                }
                catch
                {
                    // silence IO error in case running from read-only media (CD/DVD)
                }
            }
        }
    }
    public void LoadHistoryItems()
    {
        if (Book != null)
        {
            string path = Globals.HISTORY_FOLDER + Path.DirectorySeparatorChar + "History.txt";
            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    try
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (!String.IsNullOrEmpty(line))
                            {
                                string[] parts = null;

                                if (line == "")
                                {
                                    continue;
                                }
                                else if (line == "BrowseHistoryItem")
                                {
                                    line = reader.ReadLine();
                                    if (!String.IsNullOrEmpty(line))
                                    {
                                        SelectionScope scope = (SelectionScope)Enum.Parse(typeof(SelectionScope), line);
                                        List<int> indexes = new List<int>();

                                        line = reader.ReadLine();
                                        if (!String.IsNullOrEmpty(line))
                                        {
                                            parts = line.Split(',');
                                            foreach (string part in parts)
                                            {
                                                int index = -1;
                                                if (int.TryParse(part, out index))
                                                {
                                                    indexes.Add(index);
                                                }
                                            }

                                            BrowseHistoryItem item = new BrowseHistoryItem(Book, scope, indexes);
                                            AddHistoryItem(item);
                                        }
                                        else if (line == "SearchHistoryItem")
                                        {
                                            SearchHistoryItem item = new SearchHistoryItem();

                                            line = reader.ReadLine();
                                            if (!String.IsNullOrEmpty(line))
                                            {
                                                parts = line.Split('\t');
                                                if ((parts.Length == 2) && (parts[0].Trim() == "SearchType"))
                                                {
                                                    item.SearchType = (SearchType)Enum.Parse(typeof(SearchType), parts[1].Trim());
                                                }
                                            }

                                            line = reader.ReadLine();
                                            if (!String.IsNullOrEmpty(line))
                                            {
                                                parts = line.Split('\t');
                                                if ((parts.Length == 2) && (parts[0].Trim() == "NumbersResultType"))
                                                {
                                                    item.NumbersResultType = (NumbersResultType)Enum.Parse(typeof(NumbersResultType), parts[1].Trim());
                                                }
                                            }

                                            line = reader.ReadLine();
                                            if (!String.IsNullOrEmpty(line))
                                            {
                                                parts = line.Split('\t');
                                                if ((parts.Length == 2) && (parts[0].Trim() == "Text"))
                                                {
                                                    item.Text = parts[1].Trim();
                                                }
                                            }

                                            line = reader.ReadLine();
                                            if (!String.IsNullOrEmpty(line))
                                            {
                                                parts = line.Split('\t');
                                                if ((parts.Length == 2) && (parts[0].Trim() == "Header"))
                                                {
                                                    item.Header = parts[1].Trim();
                                                }
                                            }

                                            line = reader.ReadLine();
                                            if (!String.IsNullOrEmpty(line))
                                            {
                                                parts = line.Split('\t');
                                                if ((parts.Length == 2) && (parts[0].Trim() == "Language"))
                                                {
                                                    item.LanguageType = (LanguageType)Enum.Parse(typeof(LanguageType), parts[1].Trim());
                                                }
                                            }

                                            line = reader.ReadLine();
                                            if (!String.IsNullOrEmpty(line))
                                            {
                                                parts = line.Split('\t');
                                                if ((parts.Length == 2) && (parts[0].Trim() == "Translation"))
                                                {
                                                    item.Translation = parts[1].Trim();
                                                }
                                            }

                                            // CSV: Phrase.Verse.Number, Phrase.Text, Phrase.Position
                                            while (true)
                                            {
                                                line = reader.ReadLine();
                                                if (!String.IsNullOrEmpty(line))
                                                {
                                                    if (line == END_OF_HISTORY_ITME_MARKER)
                                                    {
                                                        break;
                                                    }
                                                    parts = line.Split(',');
                                                    if (parts.Length == 1) // verse.Number
                                                    {
                                                        //TODO Load NumberQuery with each search result

                                                        int verse_index = int.Parse(parts[0].Trim()) - 1;
                                                        if ((verse_index >= 0) && (verse_index < Book.Verses.Count))
                                                        {
                                                            Verse verse = Book.Verses[verse_index];
                                                            if (!item.Verses.Contains(verse))
                                                            {
                                                                item.Verses.Add(verse);
                                                            }
                                                        }
                                                    }
                                                    else if (parts.Length == 3) // phrase.Verse.Number,phrase.Text,phrase.Position
                                                    {
                                                        int verse_index = int.Parse(parts[0].Trim()) - 1;
                                                        if ((verse_index >= 0) && (verse_index < Book.Verses.Count))
                                                        {
                                                            Verse verse = Book.Verses[verse_index];
                                                            if (!item.Verses.Contains(verse))
                                                            {
                                                                item.Verses.Add(verse);
                                                            }

                                                            string phrase_text = parts[1].Trim();
                                                            if (phrase_text.Length > 0)
                                                            {
                                                                int phrase_position = int.Parse(parts[2].Trim());
                                                                Phrase phrase = new Phrase(verse, phrase_position, phrase_text);
                                                                item.Phrases.Add(phrase);
                                                            }
                                                        }
                                                    }
                                                }
                                            } // while

                                            AddHistoryItem(item);
                                        }
                                    } // while
                                }
                            }
                        }
                    }
                    catch
                    {
                        throw new Exception("Invalid " + path + " format");
                    }
                }
            }
        }
    }
    private string END_OF_HISTORY_ITME_MARKER = "-------------------";


    // letter statistics
    private List<LetterStatistic> m_letter_statistics = new List<LetterStatistic>();
    public List<LetterStatistic> LetterStatistics
    {
        get { return m_letter_statistics; }
    }
    public void SortLetterStatistics(StatisticCompareBy compare_by)
    {
        LetterStatistic.CompareBy = compare_by;

        if (LetterStatistic.CompareOrder == StatisticCompareOrder.Ascending)
        {
            LetterStatistic.CompareOrder = StatisticCompareOrder.Descending;
        }
        else
        {
            LetterStatistic.CompareOrder = StatisticCompareOrder.Ascending;
        }

        m_letter_statistics.Sort();
    }
    // letter frequency in given text
    public void BuildLetterStatistics(string text, bool? include_diacritics)
    {
        m_letter_statistics = DoBuildLetterStatistics(text, include_diacritics);
    }
    private List<LetterStatistic> DoBuildLetterStatistics(string text, bool? include_diacritics)
    {
        if (String.IsNullOrEmpty(text)) return new List<LetterStatistic>();

        List<LetterStatistic> result = new List<LetterStatistic>();
        if (result != null)
        {
            if (NumericalSystem != null)
            {
                if (include_diacritics == true) { /* do nothing */ }
                else if (include_diacritics == null) { text = text.GetDiacritics(); }
                else if (include_diacritics == false) { text = text.Simplify(NumericalSystem.TextMode); }

                text = text.Replace("\r", "");
                text = text.Replace("\n", "");
                text = text.Replace("\t", "");
                text = text.Replace("_", "");
                text = text.Replace(" ", "");
                text = text.Replace(Constants.ORNATE_RIGHT_PARENTHESIS, "");
                text = text.Replace(Constants.ORNATE_LEFT_PARENTHESIS, "");
                foreach (char character in Constants.INDIAN_DIGITS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.ARABIC_DIGITS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.SYMBOLS)
                {
                    text = text.Replace(character.ToString(), "");
                }

                result.Clear();
                if (!String.IsNullOrEmpty(text))
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        bool is_found = false;
                        for (int j = 0; j < result.Count; j++)
                        {
                            if (text[i] == result[j].Letter)
                            {
                                is_found = true;
                                result[j].Frequency++;
                                long position = i + 1L;
                                long previous_position = result[j].Positions[result[j].Positions.Count - 1];
                                result[j].Positions.Add(position);
                                result[j].PositionSum += position;
                                long distance = position - previous_position;
                                result[j].Distances.Add(distance);
                                result[j].DistanceSum += distance;
                            }
                        }
                        if (!is_found)
                        {
                            LetterStatistic letter_statistic = new LetterStatistic();

                            letter_statistic.Order = result.Count + 1;
                            letter_statistic.Letter = text[i];
                            letter_statistic.Frequency = 1;
                            long position = i + 1L;
                            letter_statistic.Positions.Add(position);
                            letter_statistic.PositionSum += position;
                            letter_statistic.Distances.Clear();
                            letter_statistic.DistanceSum = 0;

                            result.Add(letter_statistic);
                        }
                    }
                }
            }
        }

        return result;
    }
    public void SaveLetterStatistics(string path, string text, bool verbose)
    {
        if (String.IsNullOrEmpty(path)) return;
        if (String.IsNullOrEmpty(text)) return;
        if (m_letter_statistics == null) return;

        if (Directory.Exists(Globals.STATISTICS_FOLDER))
        {
            path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + path;
            try
            {
                if (NumericalSystem != null)
                {
                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                    {
                        writer.WriteLine("-------------------------------------------------------------------");
                        StringBuilder numbers = new StringBuilder();
                        foreach (int index in Selection.Indexes)
                        {
                            numbers.Append((index + 1).ToString() + ", ");
                        }
                        if (numbers.Length > 0)
                        {
                            numbers.Remove(numbers.Length - 2, 2);
                        }
                        writer.WriteLine(NumericalSystem.Name);
                        writer.WriteLine("Selection = " + Selection.Scope.ToString() + " " + numbers.ToString());
                        writer.WriteLine("-------------------------------------------------------------------");
                        writer.WriteLine();
                        writer.WriteLine("-------------------------------------------------------------------");
                        writer.WriteLine("Text");
                        writer.WriteLine("-------------------------------------------------------------------");
                        writer.WriteLine(text);
                        writer.WriteLine("-------------------------------------------------------------------");
                        writer.WriteLine();
                        writer.WriteLine("-------------------------------------------------------------------");
                        if (verbose) writer.WriteLine("Order" + "\t" + "Letter" + "\t" + "Freq" + "\t" + "ΣPos" + "\t" + "\t" + "Σ∆" + "\t");
                        else writer.WriteLine("Order" + "\t" + "Letter" + "\t" + "Freq" + "\t" + "ΣPos" + "\t" + "Σ∆");
                        writer.WriteLine("-------------------------------------------------------------------");
                        int count = 0;
                        long running_sum = 0L;
                        long frequence_sum = 0L;
                        long positionsum_sum = 0L;
                        long distancesum_sum = 0L;
                        foreach (LetterStatistic letter_statistic in m_letter_statistics)
                        {
                            if (verbose)
                            {
                                StringBuilder positions = new StringBuilder();
                                foreach (int value in letter_statistic.Positions)
                                {
                                    positions.Append(value.ToString() + "+");
                                }
                                if (positions.Length > 0)
                                {
                                    positions.Remove(positions.Length - 1, 1);
                                }

                                StringBuilder distances = new StringBuilder();
                                foreach (int value in letter_statistic.Distances)
                                {
                                    distances.Append(value.ToString() + "+");
                                }
                                if (distances.Length > 0)
                                {
                                    distances.Remove(distances.Length - 1, 1);
                                }

                                writer.WriteLine(letter_statistic.Order.ToString() + "\t" +
                                                 letter_statistic.Letter.ToString() + '\t' +
                                                 letter_statistic.Frequency.ToString() + "\t" +
                                                 letter_statistic.PositionSum.ToString() + "\t" + positions.ToString() + "\t" +
                                                 letter_statistic.DistanceSum.ToString() + "\t" + distances.ToString()
                                                 );
                            }
                            else
                            {
                                writer.WriteLine(letter_statistic.Order.ToString() + "\t" +
                                                letter_statistic.Letter.ToString() + '\t' +
                                                letter_statistic.Frequency.ToString() + "\t" +
                                                letter_statistic.PositionSum.ToString() + "\t" +
                                                letter_statistic.DistanceSum.ToString()
                                                );
                            }
                            count++;
                            running_sum += count;
                            frequence_sum += letter_statistic.Frequency;
                            positionsum_sum += letter_statistic.PositionSum;
                            distancesum_sum += letter_statistic.DistanceSum;
                        }
                        writer.WriteLine("-------------------------------------------------------------------");
                        if (verbose) writer.WriteLine(running_sum + "\t" + "Sum" + "\t" + frequence_sum.ToString() + "\t" + positionsum_sum.ToString() + "\t" + "\t" + distancesum_sum.ToString() + "\t");
                        else writer.WriteLine(running_sum + "\t" + "Sum" + "\t" + frequence_sum.ToString() + "\t" + positionsum_sum.ToString() + "\t" + distancesum_sum.ToString());
                        writer.WriteLine("-------------------------------------------------------------------");
                    }
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
    // phrase letter frequency in given text
    public void BuildLetterStatistics(string text, string phrase, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        m_letter_statistics = DoBuildLetterStatistics(text, phrase, frequency_search_type, include_diacritics);
    }
    private List<LetterStatistic> DoBuildLetterStatistics(string text, string phrase, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        if (String.IsNullOrEmpty(text)) return new List<LetterStatistic>();
        if (String.IsNullOrEmpty(phrase)) return new List<LetterStatistic>();

        List<LetterStatistic> result = new List<LetterStatistic>();
        if (result != null)
        {
            if (NumericalSystem != null)
            {
                if (include_diacritics == true) { /* do nothing */ }
                else if (include_diacritics == null) { text = text.GetDiacritics(); }
                else if (include_diacritics == false) { text = text.Simplify(NumericalSystem.TextMode); }

                text = text.Replace("\r", "");
                text = text.Replace("\n", "");
                text = text.Replace("\t", "");
                text = text.Replace("_", "");
                text = text.Replace(" ", "");
                text = text.Replace(Constants.ORNATE_RIGHT_PARENTHESIS, "");
                text = text.Replace(Constants.ORNATE_LEFT_PARENTHESIS, "");
                foreach (char character in Constants.INDIAN_DIGITS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.ARABIC_DIGITS)
                {
                    text = text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.SYMBOLS)
                {
                    text = text.Replace(character.ToString(), "");
                }

                if (include_diacritics == true) { /* do nothing */ }
                else if (include_diacritics == null) { phrase = phrase.GetDiacritics(); }
                else if (include_diacritics == false) { phrase = phrase.Simplify(NumericalSystem.TextMode); }

                phrase = phrase.Replace("\r", "");
                phrase = phrase.Replace("\n", "");
                phrase = phrase.Replace("\t", "");
                phrase = phrase.Replace("_", "");
                phrase = phrase.Replace(" ", "");
                phrase = phrase.Replace(Constants.ORNATE_RIGHT_PARENTHESIS, "");
                phrase = phrase.Replace(Constants.ORNATE_LEFT_PARENTHESIS, "");
                foreach (char character in Constants.INDIAN_DIGITS)
                {
                    phrase = phrase.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.ARABIC_DIGITS)
                {
                    phrase = phrase.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.SYMBOLS)
                {
                    phrase = phrase.Replace(character.ToString(), "");
                }

                if (frequency_search_type == FrequencySearchType.UniqueLetters)
                {
                    phrase = phrase.RemoveDuplicates();
                }

                result.Clear();
                if (!String.IsNullOrEmpty(phrase))
                {
                    for (int i = 0; i < phrase.Length; i++)
                    {
                        int frequency = 0;
                        for (int j = 0; j < text.Length; j++)
                        {
                            if (phrase[i] == text[j])
                            {
                                frequency++;
                            }
                        }

                        if (frequency > 0)
                        {
                            LetterStatistic phrase_letter_statistic = new LetterStatistic();
                            phrase_letter_statistic.Order = result.Count + 1;
                            phrase_letter_statistic.Letter = phrase[i];
                            phrase_letter_statistic.Frequency = frequency;
                            result.Add(phrase_letter_statistic);
                        }
                    }
                }
            }
        }

        return result;
    }
    public void SaveLetterStatistics(string path, string text, string phrase)
    {
        if (String.IsNullOrEmpty(path)) return;
        if (String.IsNullOrEmpty(text)) return;
        if (m_letter_statistics == null) return;

        if (Directory.Exists(Globals.STATISTICS_FOLDER))
        {
            path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + "Phrase_" + path;
            try
            {
                if (NumericalSystem != null)
                {
                    if (m_letter_statistics != null)
                    {
                        using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                        {
                            writer.WriteLine("-------------------------------------------------------------------");
                            StringBuilder numbers = new StringBuilder();
                            foreach (int index in Selection.Indexes)
                            {
                                numbers.Append((index + 1).ToString() + ", ");
                            }
                            if (numbers.Length > 0)
                            {
                                numbers.Remove(numbers.Length - 2, 2);
                            }
                            writer.WriteLine(NumericalSystem.Name);
                            writer.WriteLine("Selection = " + Selection.Scope.ToString() + " " + numbers.ToString());
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine();
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine("Text");
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine(text);
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine();
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine("Phrase");
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine(phrase);
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine();
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine("Order" + "\t" + "Letter" + "\t" + "Frequency");
                            writer.WriteLine("-------------------------------------------------------------------");
                            int count = m_letter_statistics.Count;
                            long sum = 0L;
                            for (int i = 0; i < count; i++)
                            {
                                writer.WriteLine(m_letter_statistics[i].Order.ToString() + "\t" + m_letter_statistics[i].Letter.ToString() + '\t' + m_letter_statistics[i].Frequency.ToString());
                                sum += m_letter_statistics[i].Frequency;
                            }
                            writer.WriteLine("-------------------------------------------------------------------");
                            writer.WriteLine("Total" + "\t" + count.ToString() + "\t" + sum.ToString());
                            writer.WriteLine("-------------------------------------------------------------------");
                        }
                    }
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

    public void BuildLetterStatistics(List<Letter> letters, NumberScope number_scope)
    {
        if (letters == null) return;
        if (letters.Count == 0) return;

        if (m_letter_statistics == null)
        {
            m_letter_statistics = new List<LetterStatistic>();
        }
        else
        {
            m_letter_statistics.Clear();
        }

        List<LetterStatistic> result = DoBuildLetterStatistics(letters, number_scope);
        if (result != null)
        {
            for (int i = 0; i < result.Count; i++)
            {
                bool is_found = false;
                for (int j = 0; j < m_letter_statistics.Count; j++)
                {
                    if (result[i].Letter == m_letter_statistics[j].Letter)
                    {
                        is_found = true;
                        m_letter_statistics[j].Frequency += result[i].Frequency;
                        m_letter_statistics[j].Positions.AddRange(result[i].Positions);
                        m_letter_statistics[j].PositionSum += result[i].PositionSum;
                        m_letter_statistics[j].Distances.AddRange(result[i].Distances);
                        m_letter_statistics[j].DistanceSum += result[i].DistanceSum;
                    }
                }
                if (!is_found)
                {
                    LetterStatistic letter_statistic = new LetterStatistic();

                    letter_statistic.Order = m_letter_statistics.Count + 1;
                    letter_statistic.Letter = result[i].Letter;
                    letter_statistic.Frequency = result[i].Frequency;
                    letter_statistic.Positions = result[i].Positions;
                    letter_statistic.PositionSum = result[i].PositionSum;
                    letter_statistic.Distances = result[i].Distances;
                    letter_statistic.DistanceSum += result[i].DistanceSum;

                    m_letter_statistics.Add(letter_statistic);
                }
            }
        }
    }
    public void BuildLetterStatistics(List<Word> words, NumberScope number_scope)
    {
        if (words == null) return;
        if (words.Count == 0) return;

        if (m_letter_statistics == null)
        {
            m_letter_statistics = new List<LetterStatistic>();
        }
        else
        {
            m_letter_statistics.Clear();
        }

        List<LetterStatistic> result = DoBuildLetterStatistics(words, number_scope);
        if (result != null)
        {
            for (int i = 0; i < result.Count; i++)
            {
                bool is_found = false;
                for (int j = 0; j < m_letter_statistics.Count; j++)
                {
                    if (result[i].Letter == m_letter_statistics[j].Letter)
                    {
                        is_found = true;
                        m_letter_statistics[j].Frequency += result[i].Frequency;
                        m_letter_statistics[j].Positions.AddRange(result[i].Positions);
                        m_letter_statistics[j].PositionSum += result[i].PositionSum;
                        m_letter_statistics[j].Distances.AddRange(result[i].Distances);
                        m_letter_statistics[j].DistanceSum += result[i].DistanceSum;
                    }
                }
                if (!is_found)
                {
                    LetterStatistic letter_statistic = new LetterStatistic();

                    letter_statistic.Order = m_letter_statistics.Count + 1;
                    letter_statistic.Letter = result[i].Letter;
                    letter_statistic.Frequency = result[i].Frequency;
                    letter_statistic.Positions = result[i].Positions;
                    letter_statistic.PositionSum = result[i].PositionSum;
                    letter_statistic.Distances = result[i].Distances;
                    letter_statistic.DistanceSum += result[i].DistanceSum;

                    m_letter_statistics.Add(letter_statistic);
                }
            }
        }
    }
    public void BuildLetterStatistics(List<Verse> verses, NumberScope number_scope)
    {
        if (verses == null) return;
        if (verses.Count == 0) return;

        if (m_letter_statistics == null)
        {
            m_letter_statistics = new List<LetterStatistic>();
        }
        else
        {
            m_letter_statistics.Clear();
        }

        List<LetterStatistic> result = DoBuildLetterStatistics(verses, number_scope);
        if (result != null)
        {
            for (int i = 0; i < result.Count; i++)
            {
                bool is_found = false;
                for (int j = 0; j < m_letter_statistics.Count; j++)
                {
                    if (result[i].Letter == m_letter_statistics[j].Letter)
                    {
                        is_found = true;
                        m_letter_statistics[j].Frequency += result[i].Frequency;
                        m_letter_statistics[j].Positions.AddRange(result[i].Positions);
                        m_letter_statistics[j].PositionSum += result[i].PositionSum;
                        m_letter_statistics[j].Distances.AddRange(result[i].Distances);
                        m_letter_statistics[j].DistanceSum += result[i].DistanceSum;
                    }
                }
                if (!is_found)
                {
                    LetterStatistic letter_statistic = new LetterStatistic();

                    letter_statistic.Order = m_letter_statistics.Count + 1;
                    letter_statistic.Letter = result[i].Letter;
                    letter_statistic.Frequency = result[i].Frequency;
                    letter_statistic.Positions = result[i].Positions;
                    letter_statistic.PositionSum = result[i].PositionSum;
                    letter_statistic.Distances = result[i].Distances;
                    letter_statistic.DistanceSum += result[i].DistanceSum;

                    m_letter_statistics.Add(letter_statistic);
                }
            }
        }
    }
    private List<LetterStatistic> DoBuildLetterStatistics(List<Letter> letters, NumberScope number_scope)
    {
        if (letters == null) return new List<LetterStatistic>();
        if (letters.Count == 0) return new List<LetterStatistic>();

        List<LetterStatistic> result = new List<LetterStatistic>();
        if (result != null)
        {
            if (NumericalSystem != null)
            {
                for (int i = 0; i < letters.Count; i++)
                {
                    long letter_number = 0;
                    if (number_scope == NumberScope.Number)
                    {
                        letter_number = letters[i].Number;
                    }
                    else if (number_scope == NumberScope.NumberInWord)
                    {
                        letter_number = letters[i].NumberInWord;
                    }
                    else if (number_scope == NumberScope.NumberInVerse)
                    {
                        letter_number = letters[i].NumberInVerse;
                    }
                    else if (number_scope == NumberScope.NumberInChapter)
                    {
                        letter_number = letters[i].NumberInChapter;
                    }

                    bool is_found = false;
                    for (int j = 0; j < result.Count; j++)
                    {
                        if (letters[i].Character == result[j].Letter)
                        {
                            is_found = true;
                            result[j].Frequency++;
                            long position = letter_number;
                            long previous_position = result[j].Positions[result[j].Positions.Count - 1];
                            result[j].Positions.Add(position);
                            result[j].PositionSum += position;
                            long distance = position - previous_position;
                            result[j].Distances.Add(distance);
                            result[j].DistanceSum += distance;
                        }
                    }
                    if (!is_found)
                    {
                        LetterStatistic letter_statistic = new LetterStatistic();

                        letter_statistic.Order = result.Count + 1;
                        letter_statistic.Letter = letters[i].Character;
                        letter_statistic.Frequency = 1;
                        long position = letter_number;
                        letter_statistic.Positions.Add(position);
                        letter_statistic.PositionSum += position;
                        letter_statistic.Distances.Clear();
                        letter_statistic.DistanceSum = 0;

                        result.Add(letter_statistic);
                    }
                }
            }
        }

        return result;
    }
    private List<LetterStatistic> DoBuildLetterStatistics(List<Word> words, NumberScope number_scope)
    {
        if (words == null) return new List<LetterStatistic>();
        if (words.Count == 0) return new List<LetterStatistic>();

        List<LetterStatistic> result = new List<LetterStatistic>();
        if (result != null)
        {
            if (NumericalSystem != null)
            {
                foreach (Word word in words)
                {
                    if (word != null)
                    {
                        if (word.Letters != null)
                        {
                            for (int i = 0; i < word.Letters.Count; i++)
                            {
                                long letter_number = 0;
                                if (number_scope == NumberScope.Number)
                                {
                                    letter_number = word.Letters[i].Number;
                                }
                                else if (number_scope == NumberScope.NumberInWord)
                                {
                                    letter_number = word.Letters[i].NumberInWord;
                                }
                                else if (number_scope == NumberScope.NumberInVerse)
                                {
                                    letter_number = word.Letters[i].NumberInVerse;
                                }
                                else if (number_scope == NumberScope.NumberInChapter)
                                {
                                    letter_number = word.Letters[i].NumberInChapter;
                                }

                                bool is_found = false;
                                for (int j = 0; j < result.Count; j++)
                                {
                                    if (word.Letters[i].Character == result[j].Letter)
                                    {
                                        is_found = true;
                                        result[j].Frequency++;
                                        long position = letter_number;
                                        long previous_position = result[j].Positions[result[j].Positions.Count - 1];
                                        result[j].Positions.Add(position);
                                        result[j].PositionSum += position;
                                        long distance = position - previous_position;
                                        result[j].Distances.Add(distance);
                                        result[j].DistanceSum += distance;
                                    }
                                }
                                if (!is_found)
                                {
                                    LetterStatistic letter_statistic = new LetterStatistic();

                                    letter_statistic.Order = result.Count + 1;
                                    letter_statistic.Letter = word.Letters[i].Character;
                                    letter_statistic.Frequency = 1;
                                    long position = letter_number;
                                    letter_statistic.Positions.Add(position);
                                    letter_statistic.PositionSum += position;
                                    letter_statistic.Distances.Clear();
                                    letter_statistic.DistanceSum = 0;

                                    result.Add(letter_statistic);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private List<LetterStatistic> DoBuildLetterStatistics(List<Verse> verses, NumberScope number_scope)
    {
        if (Book == null) return new List<LetterStatistic>();
        if (verses == null) return new List<LetterStatistic>();
        if (verses.Count == 0) return new List<LetterStatistic>();

        List<LetterStatistic> result = new List<LetterStatistic>();
        if (result != null)
        {
            if (NumericalSystem != null)
            {
                long letter_number = 0;

                if (number_scope == NumberScope.Number)
                {
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            if (verse.Letters != null)
                            {
                                for (int i = 0; i < verse.Letters.Count; i++)
                                {
                                    letter_number = verse.Letters[i].Number;

                                    bool is_found = false;
                                    for (int j = 0; j < result.Count; j++)
                                    {
                                        if (verse.Letters[i].Character == result[j].Letter)
                                        {
                                            is_found = true;
                                            result[j].Frequency++;
                                            long position = letter_number;
                                            long distance = position - result[j].Positions[result[j].Positions.Count - 1];
                                            result[j].Positions.Add(position);
                                            result[j].PositionSum += position;
                                            result[j].Distances.Add(distance);
                                            result[j].DistanceSum += distance;
                                        }
                                    }
                                    if (!is_found)
                                    {
                                        LetterStatistic letter_statistic = new LetterStatistic();
                                        letter_statistic.Order = result.Count + 1;
                                        letter_statistic.Letter = verse.Letters[i].Character;
                                        letter_statistic.Frequency = 1;
                                        long position = letter_number;
                                        letter_statistic.Positions.Add(position);
                                        letter_statistic.PositionSum += position;
                                        letter_statistic.Distances.Clear();
                                        letter_statistic.DistanceSum = 0;
                                        result.Add(letter_statistic);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (number_scope == NumberScope.NumberInChapter)
                {
                    if (Book != null)
                    {
                        List<Chapter> chapters = Book.GetChapters(verses);
                        if (chapters != null)
                        {
                            foreach (Chapter chapter in chapters)
                            {
                                if (chapter != null)
                                {
                                    List<LetterStatistic> chapter_result = new List<LetterStatistic>();

                                    foreach (Verse verse in chapter.Verses)
                                    {
                                        if (verses.Contains(verse))
                                        {
                                            if (verse.Letters != null)
                                            {
                                                for (int i = 0; i < verse.Letters.Count; i++)
                                                {
                                                    letter_number = verse.Letters[i].NumberInChapter;

                                                    bool is_found = false;
                                                    for (int j = 0; j < chapter_result.Count; j++)
                                                    {
                                                        if (verse.Letters[i].Character == chapter_result[j].Letter)
                                                        {
                                                            is_found = true;
                                                            chapter_result[j].Frequency++;
                                                            long position = letter_number;
                                                            long distance = position - chapter_result[j].Positions[chapter_result[j].Positions.Count - 1];
                                                            chapter_result[j].Positions.Add(position);
                                                            chapter_result[j].PositionSum += position;
                                                            chapter_result[j].Distances.Add(distance);
                                                            chapter_result[j].DistanceSum += distance;
                                                        }
                                                    }
                                                    if (!is_found)
                                                    {
                                                        LetterStatistic letter_statistic = new LetterStatistic();
                                                        letter_statistic.Order = chapter_result.Count + 1;
                                                        letter_statistic.Letter = verse.Letters[i].Character;
                                                        letter_statistic.Frequency = 1;
                                                        long position = letter_number;
                                                        letter_statistic.Positions.Add(position);
                                                        letter_statistic.PositionSum += position;
                                                        letter_statistic.Distances.Clear();
                                                        letter_statistic.DistanceSum = 0;
                                                        chapter_result.Add(letter_statistic);
                                                    }
                                                }
                                            }
                                        }
                                    } // next verse

                                    // merge chapter_result into result
                                    for (int i = 0; i < chapter_result.Count; i++)
                                    {
                                        bool is_found = false;
                                        for (int j = 0; j < result.Count; j++)
                                        {
                                            if (result[j].Letter == chapter_result[i].Letter)
                                            {
                                                is_found = true;
                                                result[j].Frequency += chapter_result[i].Frequency;
                                                result[j].Positions.AddRange(chapter_result[i].Positions);
                                                result[j].PositionSum += chapter_result[i].PositionSum;
                                                result[j].Distances.AddRange(chapter_result[i].Distances);
                                                result[j].DistanceSum += chapter_result[i].DistanceSum;
                                            }
                                        }
                                        if (!is_found)
                                        {
                                            LetterStatistic letter_statistic = new LetterStatistic();
                                            letter_statistic.Order = result.Count + 1;
                                            letter_statistic.Letter = chapter_result[i].Letter;
                                            letter_statistic.Frequency += chapter_result[i].Frequency;
                                            letter_statistic.Positions.AddRange(chapter_result[i].Positions);
                                            letter_statistic.PositionSum += chapter_result[i].PositionSum;
                                            letter_statistic.Distances.AddRange(chapter_result[i].Distances);
                                            letter_statistic.DistanceSum += chapter_result[i].DistanceSum;
                                            result.Add(letter_statistic);
                                        }
                                    }
                                }
                            } // next chapter
                        }
                    }
                }
                else if (number_scope == NumberScope.NumberInVerse)
                {
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            List<LetterStatistic> verse_result = new List<LetterStatistic>();

                            if (verse.Letters != null)
                            {
                                for (int i = 0; i < verse.Letters.Count; i++)
                                {
                                    letter_number = verse.Letters[i].NumberInVerse;

                                    bool is_found = false;
                                    for (int j = 0; j < verse_result.Count; j++)
                                    {
                                        if (verse.Letters[i].Character == verse_result[j].Letter)
                                        {
                                            is_found = true;
                                            verse_result[j].Frequency++;
                                            long position = letter_number;
                                            long distance = position - verse_result[j].Positions[verse_result[j].Positions.Count - 1];
                                            verse_result[j].Positions.Add(position);
                                            verse_result[j].PositionSum += position;
                                            verse_result[j].Distances.Add(distance);
                                            verse_result[j].DistanceSum += distance;
                                        }
                                    }
                                    if (!is_found)
                                    {
                                        LetterStatistic letter_statistic = new LetterStatistic();
                                        letter_statistic.Order = verse_result.Count + 1;
                                        letter_statistic.Letter = verse.Letters[i].Character;
                                        letter_statistic.Frequency = 1;
                                        long position = letter_number;
                                        letter_statistic.Positions.Add(position);
                                        letter_statistic.PositionSum += position;
                                        letter_statistic.Distances.Clear();
                                        letter_statistic.DistanceSum = 0;
                                        verse_result.Add(letter_statistic);
                                    }
                                }

                                // merge verse_result into result
                                for (int i = 0; i < verse_result.Count; i++)
                                {
                                    bool is_found = false;
                                    for (int j = 0; j < result.Count; j++)
                                    {
                                        if (result[j].Letter == verse_result[i].Letter)
                                        {
                                            is_found = true;
                                            result[j].Frequency += verse_result[i].Frequency;
                                            result[j].Positions.AddRange(verse_result[i].Positions);
                                            result[j].PositionSum += verse_result[i].PositionSum;
                                            result[j].Distances.AddRange(verse_result[i].Distances);
                                            result[j].DistanceSum += verse_result[i].DistanceSum;
                                        }
                                    }
                                    if (!is_found)
                                    {
                                        LetterStatistic letter_statistic = new LetterStatistic();
                                        letter_statistic.Order = result.Count + 1;
                                        letter_statistic.Letter = verse_result[i].Letter;
                                        letter_statistic.Frequency += verse_result[i].Frequency;
                                        letter_statistic.Positions.AddRange(verse_result[i].Positions);
                                        letter_statistic.PositionSum += verse_result[i].PositionSum;
                                        letter_statistic.Distances.AddRange(verse_result[i].Distances);
                                        letter_statistic.DistanceSum += verse_result[i].DistanceSum;
                                        result.Add(letter_statistic);
                                    }
                                }
                            }
                        }
                    } // next verse
                }
                else if (number_scope == NumberScope.NumberInWord)
                {
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            foreach (Word word in verse.Words)
                            {
                                List<LetterStatistic> word_result = new List<LetterStatistic>();

                                if (word.Letters != null)
                                {
                                    for (int i = 0; i < word.Letters.Count; i++)
                                    {
                                        letter_number = word.Letters[i].NumberInWord;

                                        bool is_found = false;
                                        for (int j = 0; j < word_result.Count; j++)
                                        {
                                            if (word.Letters[i].Character == word_result[j].Letter)
                                            {
                                                is_found = true;
                                                word_result[j].Frequency++;
                                                long position = letter_number;
                                                long distance = position - word_result[j].Positions[word_result[j].Positions.Count - 1];
                                                word_result[j].Positions.Add(position);
                                                word_result[j].PositionSum += position;
                                                word_result[j].Distances.Add(distance);
                                                word_result[j].DistanceSum += distance;
                                            }
                                        }
                                        if (!is_found)
                                        {
                                            LetterStatistic letter_statistic = new LetterStatistic();
                                            letter_statistic.Order = word_result.Count + 1;
                                            letter_statistic.Letter = word.Letters[i].Character;
                                            letter_statistic.Frequency = 1;
                                            long position = letter_number;
                                            letter_statistic.Positions.Add(position);
                                            letter_statistic.PositionSum += position;
                                            letter_statistic.Distances.Clear();
                                            letter_statistic.DistanceSum = 0;
                                            word_result.Add(letter_statistic);
                                        }
                                    }

                                    // merge verse_result into result
                                    for (int i = 0; i < word_result.Count; i++)
                                    {
                                        bool is_found = false;
                                        for (int j = 0; j < result.Count; j++)
                                        {
                                            if (result[j].Letter == word_result[i].Letter)
                                            {
                                                is_found = true;
                                                result[j].Frequency += word_result[i].Frequency;
                                                result[j].Positions.AddRange(word_result[i].Positions);
                                                result[j].PositionSum += word_result[i].PositionSum;
                                                result[j].Distances.AddRange(word_result[i].Distances);
                                                result[j].DistanceSum += word_result[i].DistanceSum;
                                            }
                                        }
                                        if (!is_found)
                                        {
                                            LetterStatistic letter_statistic = new LetterStatistic();
                                            letter_statistic.Order = result.Count + 1;
                                            letter_statistic.Letter = word_result[i].Letter;
                                            letter_statistic.Frequency += word_result[i].Frequency;
                                            letter_statistic.Positions.AddRange(word_result[i].Positions);
                                            letter_statistic.PositionSum += word_result[i].PositionSum;
                                            letter_statistic.Distances.AddRange(word_result[i].Distances);
                                            letter_statistic.DistanceSum += word_result[i].DistanceSum;
                                            result.Add(letter_statistic);
                                        }
                                    }
                                }
                            } // next word
                        }
                    }
                }
            }
        }

        return result;
    }


    // word symmetry
    private StringBuilder m_word_symmetry_str = null;
    public void CalculateWordSymmetry(SelectionScope selection_scope, bool include_boundary_cases)
    {
        if (Book != null)
        {
            m_word_symmetry_str = new StringBuilder();
            //m_word_symmetry_str.AppendLine("----------------------------------------------------------------");
            //m_word_symmetry_str.AppendLine(Book.TextMode.ToString() +
            //                                ((Book.WithBismAllah) ? "" : "_WithoutBismAllah") +
            //                                ((Book.WawAsWord) ? "_WawAsWord" : "") +
            //                                ((Book.ShaddaAsLetter) ? "_ShaddaAsLetter" : ""));
            //m_word_symmetry_str.AppendLine((include_boundary_cases ? "Full " : "") + selection_scope.ToString() + " Word Symmetry");
            //m_word_symmetry_str.AppendLine("----------------------------------------------------------------");
            //m_word_symmetry_str.AppendLine();
            //m_word_symmetry_str.AppendLine();
            //m_word_symmetry_str.AppendLine("----------------------------------------------------------");
            m_word_symmetry_str.AppendLine(((selection_scope == SelectionScope.Book) ? "BOOK" : selection_scope.ToString().ToUpper()) + "\tWORDS\t#\tWords\tLetters\tWSum\tLSum\tSymmetry");
            //m_word_symmetry_str.AppendLine("----------------------------------------------------------");

            List<Word> words = new List<Word>();
            switch (selection_scope)
            {
                case SelectionScope.Book:
                    {
                        words.Clear();
                        foreach (Verse verse in Book.Verses)
                        {
                            words.AddRange(verse.Words);
                        }
                        DoCalculateWordSymmetry(1, words, include_boundary_cases);
                    }
                    break;
                case SelectionScope.Chapter:
                    {
                        foreach (Chapter chapter in Book.Chapters)
                        {
                            words.Clear();
                            foreach (Verse verse in chapter.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(chapter.SortedNumber, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Page:
                    {
                        foreach (Page page in Book.Pages)
                        {
                            words.Clear();
                            foreach (Verse verse in page.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(page.Number, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Station:
                    {
                        foreach (Station station in Book.Stations)
                        {
                            words.Clear();
                            foreach (Verse verse in station.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(station.Number, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Part:
                    {
                        foreach (Part part in Book.Parts)
                        {
                            words.Clear();
                            foreach (Verse verse in part.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(part.Number, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Group:
                    {
                        foreach (Group group in Book.Groups)
                        {
                            words.Clear();
                            foreach (Verse verse in group.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(group.Number, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Half:
                    {
                        foreach (Half half in Book.Halfs)
                        {
                            words.Clear();
                            foreach (Verse verse in half.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(half.Number, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Quarter:
                    {
                        foreach (Quarter quarter in Book.Quarters)
                        {
                            words.Clear();
                            foreach (Verse verse in quarter.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(quarter.Number, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Bowing:
                    {
                        foreach (Bowing bowing in Book.Bowings)
                        {
                            words.Clear();
                            foreach (Verse verse in bowing.Verses)
                            {
                                words.AddRange(verse.Words);
                            }
                            DoCalculateWordSymmetry(bowing.Number, words, include_boundary_cases);
                        }
                    }
                    break;
                case SelectionScope.Verse:
                default:
                    {
                        foreach (Verse verse in Book.Verses)
                        {
                            DoCalculateWordSymmetry(verse.Number, verse.Words, include_boundary_cases);
                        }
                    }
                    break;
            }

            //m_word_symmetry_str.AppendLine("----------------------------------------------------------");

            SaveWordSymmetry(selection_scope, m_word_symmetry_str.ToString(), include_boundary_cases);
        }
    }
    private void DoCalculateWordSymmetry(int selection_number, List<Word> words, bool include_boundary_cases)
    {
        if (m_word_symmetry_str == null) return;

        int count = 0;
        int w_sum = 0;
        int l_sum = 0;
        int max = words.Count - 1;
        if (include_boundary_cases)
        {
            // 0, 0
            count++;
            w_sum += 0;
            l_sum += 0;
            //                             selection_number                     WORDS                           #                         Words        Letters      WSum         LSum         Symmetry
            m_word_symmetry_str.AppendLine(selection_number.ToString() + "\t" + words.Count.ToString() + "\t" + count.ToString() + "\t" + "0" + "\t" + "0" + "\t" + "0" + "\t" + "0" + "\t" + ((double)(count * 100.0D) / (double)words.Count).ToString("0.000") + "%");

            // all words, all letters
            max++;
        }

        int a = 0;
        int z = 0;
        for (int i = 0; i < max; i++)
        {
            int j = words.Count - 1 - i;

            a += words[i].Letters.Count;
            z += words[j].Letters.Count;
            if (a == z)
            {
                count++;
                w_sum += (i + 1);
                l_sum += a;
                //                             selection_number                     WORDS                           #                         Words                       Letters               WSum                      LSum                      Symmetry
                m_word_symmetry_str.AppendLine(selection_number.ToString() + "\t" + words.Count.ToString() + "\t" + count.ToString() + "\t" + (i + 1).ToString() + "\t" + a.ToString() + "\t" + w_sum.ToString() + "\t" + l_sum.ToString() + "\t" + ((double)((count - (include_boundary_cases ? 2 : 0)) * 100.0D) / (double)words.Count).ToString("0.000") + "%");
            }
        }
    }
    private void SaveWordSymmetry(SelectionScope selection_scope, string word_symmetry_text, bool include_boundary_cases)
    {
        if (Book != null)
        {
            if (Directory.Exists(Globals.STATISTICS_FOLDER))
            {
                string path = Globals.STATISTICS_FOLDER + Path.DirectorySeparatorChar + (include_boundary_cases ? "Full" : "") + selection_scope.ToString() + "WordSymmetry" + "_" +
                                                                    Book.TextMode.ToString() +
                                                                    ((Book.WithBismAllah) ? "" : "_WithoutBismAllah") +
                                                                    ((Book.WawAsWord) ? "_WawAsWord" : "") +
                                                                    ((Book.ShaddaAsLetter) ? "_ShaddaAsLetter" : "") +
                                                                    ((Book.HamzaAboveHorizontalLineAsLetter) ? "_HamzaAboveHorizontalLineAsLetter" : "") +
                                                                    ((Book.ElfAboveHorizontalLineAsLetter) ? "_ElfAboveHorizontalLineAsLetter" : "") +
                                                                    ((Book.YaaAboveHorizontalLineAsLetter) ? "_YaaAboveHorizontalLineAsLetter" : "") +
                                                                    ((Book.NoonAboveHorizontalLineAsLetter) ? "_NoonAboveHorizontalLineAsLetter" : "") +
                                                                    ".txt";
                try
                {
                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                    {
                        writer.WriteLine(word_symmetry_text);
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
    }


    // bookmarks
    private List<Bookmark> m_bookmarks = new List<Bookmark>();
    public List<Bookmark> Bookmarks
    {
        get { return m_bookmarks; }
    }
    private Bookmark m_current_bookmark;
    public Bookmark CurrentBookmark
    {
        get { return m_current_bookmark; }
    }
    private int m_current_bookmark_index = -1;
    public int CurrentBookmarkIndex
    {
        get
        {
            if (m_bookmarks != null)
            {
                for (int i = 0; i < m_bookmarks.Count; i++)
                {
                    if (m_bookmarks[i] == m_current_bookmark)
                    {
                        if (i == m_current_bookmark_index)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
    }
    public int GetBookmarkIndex(Bookmark bookmark)
    {
        if (m_bookmarks != null)
        {
            for (int i = 0; i < m_bookmarks.Count; i++)
            {
                if (m_bookmarks[i] == bookmark)
                {
                    return i;
                }
            }
        }
        return -1;
    }
    public Bookmark GetBookmark(Selection selection)
    {
        if (selection != null)
        {
            // selection is mutable so we cannot use ==
            //foreach (Bookmark bookmark in m_bookmarks)
            //{
            //    if (bookmark.Selection == selection)
            //    {
            //        return bookmark;
            //    }
            //}
            return GetBookmark(selection.Scope, selection.Indexes);
        }
        return null;
    }
    public Bookmark GetBookmark(SelectionScope scope, List<int> indexes)
    {
        if (m_bookmarks != null)
        {
            foreach (Bookmark bookmark in m_bookmarks)
            {
                if (bookmark.Selection.Scope == scope)
                {
                    if (bookmark.Selection.Indexes.Count == indexes.Count)
                    {
                        int matching_indexes = 0;
                        for (int i = 0; i < bookmark.Selection.Indexes.Count; i++)
                        {
                            if (bookmark.Selection.Indexes[i] == indexes[i])
                            {
                                matching_indexes++;
                            }
                        }
                        if (indexes.Count == matching_indexes)
                        {
                            return bookmark;
                        }
                    }
                }
            }
        }
        return null;
    }
    public Bookmark GotoBookmark(Selection selection)
    {
        Bookmark bookmark = null;
        if (selection != null)
        {
            bookmark = GetBookmark(selection.Scope, selection.Indexes);
            if (bookmark != null)
            {
                m_current_bookmark = bookmark;
                m_current_bookmark_index = GetBookmarkIndex(bookmark);
            }
        }
        return bookmark;
    }
    public Bookmark GotoBookmark(SelectionScope scope, List<int> indexes)
    {
        Bookmark bookmark = GetBookmark(scope, indexes);
        if (bookmark != null)
        {
            m_current_bookmark = bookmark;
            m_current_bookmark_index = GetBookmarkIndex(bookmark);
        }
        return bookmark;
    }
    public Bookmark GotoNextBookmark()
    {
        if (m_bookmarks != null)
        {
            if (m_bookmarks.Count > 0)
            {
                if (m_current_bookmark_index < m_bookmarks.Count - 1)
                {
                    m_current_bookmark_index++;
                    m_current_bookmark = m_bookmarks[m_current_bookmark_index];
                }
            }
        }
        return m_current_bookmark;
    }
    public Bookmark GotoPreviousBookmark()
    {
        if (m_bookmarks != null)
        {
            if (m_bookmarks.Count > 0)
            {
                if (m_current_bookmark_index > 0)
                {
                    m_current_bookmark_index--;
                    m_current_bookmark = m_bookmarks[m_current_bookmark_index];
                }
            }
        }
        return m_current_bookmark;
    }
    public Bookmark CreateBookmark(Selection selection, string note)
    {
        Bookmark bookmark = GetBookmark(selection.Scope, selection.Indexes);
        if (bookmark != null) // overwrite existing bookmark
        {
            bookmark.Note = note;
            bookmark.LastModifiedTime = DateTime.Now;
            m_current_bookmark = bookmark;
        }
        else // create a new bookmark
        {
            bookmark = new Bookmark(selection, note);
            m_bookmarks.Insert(m_current_bookmark_index + 1, bookmark);
            m_current_bookmark_index++;
            m_current_bookmark = m_bookmarks[m_current_bookmark_index];
        }
        return m_current_bookmark;
    }
    public void AddBookmark(Selection selection, string note, DateTime created_time, DateTime last_modified_time)
    {
        if (m_bookmarks != null)
        {
            Bookmark bookmark = CreateBookmark(selection, note);
            if (bookmark != null)
            {
                bookmark.CreatedTime = created_time;
                bookmark.LastModifiedTime = last_modified_time;
            }
        }
    }
    public void DeleteCurrentBookmark()
    {
        Bookmark current_bookmark = CurrentBookmark;
        if (current_bookmark != null)
        {
            if (m_bookmarks != null)
            {
                m_bookmarks.Remove(current_bookmark);
                if (m_bookmarks.Count == 0) // no bookmark to display
                {
                    m_current_bookmark_index = -1;
                    m_current_bookmark = null;
                }
                else // there are bookmarks still
                {
                    // if index becomes outside list, move back into list
                    if (m_current_bookmark_index == m_bookmarks.Count)
                    {
                        m_current_bookmark_index = m_bookmarks.Count - 1;
                    }
                    m_current_bookmark = m_bookmarks[m_current_bookmark_index];
                }
            }
        }
    }
    public void ClearBookmarks()
    {
        if (m_bookmarks != null)
        {
            m_bookmarks.Clear();
            m_current_bookmark_index = -1;
            m_current_bookmark = null;
        }
    }
    public void SaveBookmarks()
    {
        if (Book != null)
        {
            if (Directory.Exists(Globals.BOOKMARKS_FOLDER))
            {
                string path = Globals.BOOKMARKS_FOLDER + Path.DirectorySeparatorChar + Book.TextMode + ".txt";
                try
                {
                    using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                    {
                        if (m_bookmarks != null)
                        {
                            foreach (Bookmark bookmark in m_bookmarks)
                            {
                                if (bookmark.Note.Length > 0)
                                {
                                    string scope_str = bookmark.Selection.Scope.ToString();

                                    StringBuilder str = new StringBuilder();
                                    if (bookmark.Selection.Indexes.Count > 0)
                                    {
                                        for (int i = 0; i < bookmark.Selection.Indexes.Count; i++)
                                        {
                                            str.Append((bookmark.Selection.Indexes[i] + 1).ToString() + "+");
                                        }
                                        if (str.Length > 1)
                                        {
                                            str.Remove(str.Length - 1, 1);
                                        }
                                    }

                                    string created_time = bookmark.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss");
                                    string last_modified_time = bookmark.LastModifiedTime.ToString("yyyy-MM-dd HH:mm:ss");
                                    string note = bookmark.Note;

                                    string line = scope_str + "," + str.ToString() + "," + created_time + "," + last_modified_time + "," + note;
                                    writer.WriteLine(line);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // silence IO error in case running from read-only media (CD/DVD)
                }
            }
        }
    }
    public void LoadBookmarks()
    {
        if (Book != null)
        {
            string path = Globals.BOOKMARKS_FOLDER + Path.DirectorySeparatorChar + Book.TextMode + ".txt";
            if (File.Exists(path))
            {
                using (StreamReader reader = File.OpenText(path))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            string[] parts = line.Split(',');
                            if (parts.Length == 5)
                            {
                                try
                                {
                                    SelectionScope scope = (SelectionScope)Enum.Parse(typeof(SelectionScope), parts[0]);

                                    string part = parts[1].Trim();
                                    string[] sub_parts = part.Split('+');
                                    List<int> indexes = new List<int>();
                                    foreach (string sub_part in sub_parts)
                                    {
                                        indexes.Add(int.Parse(sub_part.Trim()) - 1);
                                    }
                                    Selection selection = new Selection(Book, scope, indexes);

                                    DateTime created_time = DateTime.ParseExact(parts[2], "yyyy-MM-dd HH:mm:ss", null);
                                    DateTime last_modified_time = DateTime.ParseExact(parts[3], "yyyy-MM-dd HH:mm:ss", null);
                                    string note = parts[4];

                                    AddBookmark(selection, note, created_time, last_modified_time);
                                }
                                catch
                                {
                                    throw new Exception("Invalid data format in " + path);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // help
    public List<string> HelpMessages
    {
        get { return Server.HelpMessages; }
    }
}
