using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Model;

public class Server : IPublisher
{
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
    private FileSystemWatcher m_file_system_watcher = null;
    public void WatchFolder(string folder_name, string filter)
    {
        if (m_file_system_watcher == null)
        {
            m_file_system_watcher = new FileSystemWatcher();
        }

        if (m_file_system_watcher != null)
        {
            if (Directory.Exists(folder_name))
            {
                m_file_system_watcher.Filter = filter;
                m_file_system_watcher.Path = folder_name;
                m_file_system_watcher.IncludeSubdirectories = true;
                m_file_system_watcher.NotifyFilter =
                    NotifyFilters.FileName |
                    NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Security |
                    NotifyFilters.Size |
                    NotifyFilters.DirectoryName;

                m_file_system_watcher.Created += new FileSystemEventHandler(OnFolderChanged);
                m_file_system_watcher.Changed += new FileSystemEventHandler(OnFileChanged);
                m_file_system_watcher.Deleted += new FileSystemEventHandler(OnFolderChanged);
                m_file_system_watcher.Renamed += new RenamedEventHandler(OnFolderChanged);

                m_file_system_watcher.EnableRaisingEvents = true;
            }
        }
    }
    public void UnwatchFolder(string folder_name, string filter)
    {
        if (m_file_system_watcher != null)
        {
            if (Directory.Exists(folder_name))
            {
                // wait for files in folder to be unlocked
                FileHelper.WaitForFolderReady(folder_name, filter);
            }

            m_file_system_watcher.EnableRaisingEvents = false;

            m_file_system_watcher.Created -= new FileSystemEventHandler(OnFolderChanged);
            m_file_system_watcher.Changed -= new FileSystemEventHandler(OnFileChanged);
            m_file_system_watcher.Deleted -= new FileSystemEventHandler(OnFolderChanged);
            m_file_system_watcher.Renamed -= new RenamedEventHandler(OnFolderChanged);
        }
    }
    private void OnFolderChanged(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.StartsWith(Globals.LANGUAGES_FOLDER))
        {
            NotifySubscribers(Subject.LanguageSystem, e);
        }
        else if (e.FullPath.StartsWith(Globals.RULES_FOLDER))
        {
            LoadSimplificationSystems();
            NotifySubscribers(Subject.SimplificationSystem, e);
        }
        else if (e.FullPath.StartsWith(Globals.VALUES_FOLDER))
        {
            if (e.Name.Contains("DNA") || (e.Name.Contains("ConvertTo")))
            {
                LoadDNASequenceSystems();
                NotifySubscribers(Subject.DNASequenceSystem, e);
            }
            else
            {
                LoadNumericalSystems();
                NotifySubscribers(Subject.NumericalSystem, e);
            }
        }
        else if (e.FullPath.StartsWith(Globals.NUMBERS_FOLDER))
        {
            if (e.Name == Globals.INTERESTING_NUMBERS_FILENAME)
            {
                Numbers.LoadInterestingNumbers();
                NotifySubscribers(Subject.InterestingNumbers, e);
            }
        }
    }
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.StartsWith("Languages"))
        {
            NotifySubscribers(Subject.LanguageSystem, e);
        }
        else
        {
            if (e.Name == Globals.INTERESTING_NUMBERS_FILENAME)
            {
                Numbers.LoadInterestingNumbers();
                NotifySubscribers(Subject.InterestingNumbers, e);
            }
            //else if (e.Name.Contains("DNA"))   // e.g. Simplified29_A29T29C19G23_DNA.txt
            else if (e.Name.Contains("DNA") || (e.Name.Contains("ConvertTo")))
            {
                LoadDNASequenceSystem(s_dna_sequence_system.Name);
                NotifySubscribers(Subject.DNASequenceSystem, e);
            }
            else if (e.Name.Contains("_")) // e.g. Original_Alphabet_Primes1.txt
            {
                if (s_numerical_system != null)
                {
                    LoadNumericalSystem(s_numerical_system.Name);
                    NotifySubscribers(Subject.NumericalSystem, e);
                }
            }
            else                           // e.g. Simplified29.txt
            {
                if (s_simplification_system != null)
                {
                    LoadSimplificationSystem(s_simplification_system.Name);
                    NotifySubscribers(Subject.SimplificationSystem, e);
                }
            }
        }
    }
    //private void Dispose()
    //{
    //    if (m_file_system_watcher != null)
    //    {
    //        m_file_system_watcher.Dispose();
    //        m_file_system_watcher = null;
    //    }
    //} ///////////////////////////////////////////////////////////////////////////////
    #endregion

    public const int DEFAULT_WORD_COUNT_METHOD = 77878;
    public const string DEFAULT_RECITATION = "Alafasy_64kbps";
    public const string DEFAULT_QURAN_TEXT = "quran-uthmani";
    public const string DEFAULT_EMLAAEI_TEXT = "ar.emlaaei";
    public const string DEFAULT_TRANSLATION = "en.qarai";
    public const string DEFAULT_TRANSLITERATION = "en.transliteration";
    public const string DEFAULT_WORD_MEANINGS = "en.wordbyword";
    public const string DEFAULT_TRANSLATION_1 = "en.pickthall";
    public const string DEFAULT_TRANSLATION_2 = "es.garcia";
    public const string DEFAULT_TRANSLATION_3 = "fa.makarem";
    public const string DEFAULT_TRANSLATION_4 = "hi.hindi";
    public const string DEFAULT_TRANSLATION_5 = "id.muntakhab";
    public const string DEFAULT_TRANSLATION_6 = "ja.japanese";
    public const string DEFAULT_TRANSLATION_7 = "tr.yazir";
    public const string DEFAULT_TRANSLATION_8 = "ur.jawadi";
    public const string DEFAULT_TRANSLATION_9 = "zh.jian";

    static Server()
    {
        if (!Directory.Exists(Globals.STATISTICS_FOLDER))
        {
            Directory.CreateDirectory(Globals.STATISTICS_FOLDER);
        }

        if (!Directory.Exists(Globals.RULES_FOLDER))
        {
            Directory.CreateDirectory(Globals.RULES_FOLDER);
        }

        if (!Directory.Exists(Globals.VALUES_FOLDER))
        {
            Directory.CreateDirectory(Globals.VALUES_FOLDER);
        }

        if (!Directory.Exists(Globals.HELP_FOLDER))
        {
            Directory.CreateDirectory(Globals.HELP_FOLDER);
        }

        if (!Directory.Exists(Globals.RESEARCH_FOLDER))
        {
            Directory.CreateDirectory(Globals.RESEARCH_FOLDER);
        }

        // load word count methods names and and default method's files
        LoadWordCountMethods();
        LoadDefaultWordCountMethodFiles();

        // load simplification systems
        LoadSimplificationSystems();

        // load numerical systems
        LoadNumericalSystems();

        // load dna sequence systems
        LoadDNASequenceSystems();

        // load help messages
        LoadHelpMessages();
    }

    private static List<int> s_word_count_methods = null;
    public static List<int> WordCountMethods
    {
        get { return s_word_count_methods; }
    }
    private static void LoadWordCountMethods()
    {
        if (s_word_count_methods == null)
        {
            s_word_count_methods = new List<int>();
        }

        if (s_word_count_methods != null)
        {
            s_word_count_methods.Clear();

            string path = Globals.RULES_FOLDER;
            try
            {
                DirectoryInfo folder = new DirectoryInfo(path);
                if (folder != null)
                {
                    DirectoryInfo[] sub_folders = folder.GetDirectories();
                    if ((sub_folders != null) && (sub_folders.Length > 0))
                    {
                        foreach (DirectoryInfo sub_folder in sub_folders)
                        {
                            if (sub_folder != null)
                            {
                                int word_count_method;
                                if (int.TryParse(sub_folder.Name, out word_count_method))
                                {
                                    s_word_count_methods.Add(word_count_method);
                                }
                            }
                        }

                        // start with default word_count_method
                        if (!s_word_count_methods.Contains(DEFAULT_WORD_COUNT_METHOD))
                        {
                            //TODO Cannot carry exception over Interface notification
                            throw new Exception("ERROR: No default word count method was found.");
                        }
                    }
                }
            }
            catch
            {
                //TODO Cannot carry exception over Interface notification
                throw new Exception("ERROR: No " + path.ToString() + " word count method was found.");
            }
        }
    }
    private static void LoadDefaultWordCountMethodFiles()
    {
        try
        {
            if (s_word_count_methods != null)
            {
                if (Directory.Exists(Globals.RULES_FOLDER))
                {
                    // bring files online
                    string rules_source = Globals.RULES_FOLDER + Path.DirectorySeparatorChar + DEFAULT_WORD_COUNT_METHOD.ToString();
                    string rules_target = Globals.RULES_FOLDER;
                    DirectoryInfo rules_source_folder = new DirectoryInfo(rules_source);
                    if (rules_source_folder != null)
                    {
                        FileInfo[] files = rules_source_folder.GetFiles("*.txt");
                        foreach (FileInfo file in files)
                        {
                            if (file != null)
                            {
                                file.CopyTo(rules_target + Path.DirectorySeparatorChar + file.Name, true);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ".\r\n\r\n" + ex.StackTrace);
        }
    }

    private static int s_word_count_method = DEFAULT_WORD_COUNT_METHOD;
    public static int WordCountMethod
    {
        set
        {
            s_word_count_method = value;
            UpdateWordCountMethodFiles();
        }
        get
        {
            return s_word_count_method;
        }
    }
    public static void UpdateWordCountMethodFiles()
    {
        try
        {
            if (s_word_count_methods != null)
            {
                // if invalid method, use default method
                if (!s_word_count_methods.Contains(s_word_count_method))
                {
                    s_word_count_method = DEFAULT_WORD_COUNT_METHOD;
                }

                if (Directory.Exists(Globals.RULES_FOLDER))
                {
                    // bring files online
                    string rules_source = Globals.RULES_FOLDER + Path.DirectorySeparatorChar + s_word_count_method.ToString();
                    string rules_target = Globals.RULES_FOLDER;
                    DirectoryInfo rules_source_folder = new DirectoryInfo(rules_source);
                    if (rules_source_folder != null)
                    {
                        FileInfo[] files = rules_source_folder.GetFiles("*.txt");
                        foreach (FileInfo file in files)
                        {
                            if (file != null)
                            {
                                try
                                {
                                    file.CopyTo(rules_target + Path.DirectorySeparatorChar + file.Name, true);
                                }
                                catch
                                {
                                    // slience exception
                                }
                            }
                        }
                    }
                }

                if (Directory.Exists(Globals.DATA_FOLDER))
                {
                    string data_source = Globals.DATA_FOLDER + Path.DirectorySeparatorChar + s_word_count_method.ToString();
                    string data_target = Globals.DATA_FOLDER;
                    DirectoryInfo data_source_folder = new DirectoryInfo(data_source);
                    if (data_source_folder != null)
                    {
                        FileInfo[] files = data_source_folder.GetFiles("*.txt");
                        foreach (FileInfo file in files)
                        {
                            if (file != null)
                            {
                                try
                                {
                                    file.CopyTo(data_target + Path.DirectorySeparatorChar + file.Name, true);
                                }
                                catch
                                {
                                    // slience exception
                                }
                            }
                        }
                    }
                }

                if (Directory.Exists(Globals.TRANSLATIONS_FOLDER))
                {
                    string translations_source = Globals.TRANSLATIONS_FOLDER + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar + s_word_count_method.ToString();
                    string translations_target = Globals.TRANSLATIONS_FOLDER;
                    string translations_target_offline = Globals.TRANSLATIONS_FOLDER + Path.DirectorySeparatorChar + "Offline";
                    DirectoryInfo translations_source_folder = new DirectoryInfo(translations_source);
                    if (translations_source_folder != null)
                    {
                        FileInfo[] files = translations_source_folder.GetFiles("*.txt");
                        foreach (FileInfo file in files)
                        {
                            if (file != null)
                            {
                                try
                                {
                                    file.CopyTo(translations_target_offline + Path.DirectorySeparatorChar + file.Name, true);

                                    // bring online
                                    file.CopyTo(translations_target + Path.DirectorySeparatorChar + file.Name, true);
                                }
                                catch
                                {
                                    // slience exception
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ".\r\n\r\n" + ex.StackTrace);
        }
    }

    // the book [DYNAMIC]
    private static Book s_book = null;
    public static Book Book
    {
        get { return s_book; }
    }

    // loaded simplification systems [STATIC]
    private static Dictionary<string, SimplificationSystem> s_loaded_simplification_systems = null;
    public static Dictionary<string, SimplificationSystem> LoadedSimplificationSystems
    {
        get { return s_loaded_simplification_systems; }
    }
    private static void LoadSimplificationSystems()
    {
        if (s_loaded_simplification_systems == null)
        {
            s_loaded_simplification_systems = new Dictionary<string, SimplificationSystem>();
        }

        if (s_loaded_simplification_systems != null)
        {
            s_loaded_simplification_systems.Clear();

            string path = Globals.RULES_FOLDER;
            DirectoryInfo folder = new DirectoryInfo(path);
            if (folder != null)
            {
                FileInfo[] files = folder.GetFiles("*.txt");
                if ((files != null) && (files.Length > 0))
                {
                    foreach (FileInfo file in files)
                    {
                        if (file != null)
                        {
                            string text_mode = file.Name.Remove(file.Name.Length - 4, 4);
                            if (!String.IsNullOrEmpty(text_mode))
                            {
                                // allow Original text without Quranmarks in non-Standard Editions only.
                                if ((Globals.EDITION == Edition.Standard) && (text_mode == "SimplifiedMarks"))
                                {
                                    continue; // skip
                                }
                                LoadSimplificationSystem(text_mode);
                            }
                        }
                    }

                    // start with default simplification system
                    if (s_loaded_simplification_systems.ContainsKey(SimplificationSystem.DEFAULT_NAME))
                    {
                        s_simplification_system = new SimplificationSystem(s_loaded_simplification_systems[SimplificationSystem.DEFAULT_NAME]);
                    }
                    else
                    {
                        //TODO Cannot carry exception over Interface notification
                        //throw new Exception("ERROR: No default simplification system was found");
                    }
                }
            }
        }
    }
    // simplification system [DYNAMIC]
    private static SimplificationSystem s_simplification_system = null;
    public static SimplificationSystem SimplificationSystem
    {
        get { return s_simplification_system; }
    }
    public static void LoadSimplificationSystem(string text_mode)
    {
        if (s_loaded_simplification_systems != null)
        {
            // rebuild on the fly without restarting application
            string path = Globals.RULES_FOLDER + Path.DirectorySeparatorChar + text_mode + ".txt";
            if (File.Exists(path))
            {
                List<string> lines = FileHelper.LoadLines(path);

                SimplificationSystem simplification_system = new SimplificationSystem(text_mode);
                if (simplification_system != null)
                {
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("#"))
                            continue;

                        string[] parts = line.Split('\t');
                        if (parts.Length == 2)
                        {
                            SimplificationRule rule = new SimplificationRule(parts[0], parts[1]);
                            if (rule != null)
                            {
                                simplification_system.Rules.Add(rule);
                            }
                        }
                        else
                        {
                            //TODO Cannot carry exception over Interface notification
                            //throw new Exception(path + " file format must be:\r\n\tText TAB Replacement");
                        }
                    }
                }

                // add the new simplification system or replace the existing one with the same name
                if (!s_loaded_simplification_systems.ContainsKey(text_mode))
                {
                    s_loaded_simplification_systems.Add(text_mode, simplification_system);
                }
                else
                {
                    s_loaded_simplification_systems[text_mode] = simplification_system;
                }

                // set current simplification system
                if (s_loaded_simplification_systems.ContainsKey(text_mode))
                {
                    s_simplification_system = s_loaded_simplification_systems[text_mode];
                }
            }
        }
    }
    public static void BuildSimplifiedBook(string text_mode, bool with_diacritics, bool with_bism_Allah, bool waw_as_word, bool shadda_as_letter, bool hamza_above_horizontal_line_as_letter, bool elf_above_horizontal_line_as_letter, bool yaa_above_horizontal_line_as_letter, bool noon_above_horizontal_line_as_letter, bool emlaaei_text)
    {
        if (!String.IsNullOrEmpty(text_mode))
        {
            LoadSimplificationSystems();

            if (s_loaded_simplification_systems != null)
            {
                if (s_loaded_simplification_systems.ContainsKey(text_mode))
                {
                    s_simplification_system = s_loaded_simplification_systems[text_mode];
                    if (s_simplification_system != null)
                    {
                        // reload original Quran text
                        string path = null;
                        if (emlaaei_text)
                        {
                            path = Globals.TRANSLATIONS_FOLDER + Path.DirectorySeparatorChar + DEFAULT_EMLAAEI_TEXT + ".txt";
                        }
                        else
                        {
                            path = Globals.DATA_FOLDER + Path.DirectorySeparatorChar + DEFAULT_QURAN_TEXT + ".txt";
                        }
                        List<string> lines = DataAccess.LoadVerseTexts(path);

                        // generate Quranmarks/Stopmarks word numbers
                        //int[] numbers = new int[]
                        //{
                        //    7, 286, 200, 176, 120, 165, 206, 75, 129, 109,
                        //    123, 111, 43, 52, 99, 128, 111, 110, 98, 135,
                        //    112, 78, 118, 64, 77, 227, 93, 88, 69, 60,
                        //    34, 30, 73, 54, 45, 83, 182, 88, 75, 85,
                        //    54, 53, 89, 59, 37, 35, 38, 29, 18, 45,
                        //    60, 49, 62, 55, 78, 96, 29, 22, 24, 13,
                        //    14, 11, 11, 18, 12, 12, 30, 52, 52, 44,
                        //    28, 28, 20, 56, 40, 31, 50, 40, 46, 42,
                        //    29, 19, 36, 25, 22, 17, 19, 26, 30, 20,
                        //    15, 21, 11, 8, 8, 19, 5, 8, 8, 11,
                        //    11, 8, 3, 9, 5, 4, 7, 3, 6, 3,
                        //    5, 4, 5, 6
                        //};

                        //str.AppendLine
                        //    (
                        //        "#" + "\t" +
                        //        "Chapter" + "\t" +
                        //        "Verse" + "\t" +
                        //        "Word" + "\t" +
                        //        "Stopmark"
                        //      );

                        //int count = 0;
                        //for (int v = 0; v < lines.Count; v++)
                        //{
                        //    string[] words = lines[v].Split();

                        //    int vv = v + 1;
                        //    int cc = 0 + 1;
                        //    foreach (int n in numbers)
                        //    {
                        //        if (vv <= n) break;

                        //        cc++;
                        //        vv -= n;
                        //    }

                        //    for (int w = 0; w < words.Length; w++)
                        //    {
                        //        if (words[w].Length == 1)
                        //        {
                        //            if (
                        //                Constants.STOPMARKS.Contains(words[w][0])
                        //                ||
                        //                Constants.QURANMARKS.Contains(words[w][0]))
                        //            {
                        //                count++;
                        //                str.AppendLine(
                        //                    count + "\t" +
                        //                    cc + "\t" +
                        //                    vv + "\t" +
                        //                    (w + 1) + "\t" +
                        //                    words[w][0]
                        //                  );
                        //            }
                        //        }
                        //    }
                        //}
                        //if (Directory.Exists(Globals.DATA_FOLDER))
                        //{
                        //    string path = Globals.DATA_FOLDER + "Stopmarks.txt";
                        //    FileHelper.SaveText(path, str.ToString());
                        //    FileHelper.DisplayFile(path);
                        //}

                        List<Stopmark> verse_stopmarks = DataAccess.LoadVerseStopmarks();

                        // remove bismAllah from 112 chapters
                        if (!with_bism_Allah)
                        {
                            string bimsAllah_text1 = emlaaei_text ? "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ " : "بِسْمِ ٱللَّهِ ٱلرَّحْمَـٰنِ ٱلرَّحِيمِ ";
                            string bimsAllah_text2 = emlaaei_text ? "بِّسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِ " : "بِّسْمِ ٱللَّهِ ٱلرَّحْمَـٰنِ ٱلرَّحِيمِ "; // shadda on baa for chapter 95 and 97
                            for (int i = 0; i < lines.Count; i++)
                            {
                                if (lines[i].StartsWith(bimsAllah_text1))
                                {
                                    lines[i] = lines[i].Replace(bimsAllah_text1, "");
                                }
                                else if (lines[i].StartsWith(bimsAllah_text2))
                                {
                                    lines[i] = lines[i].Replace(bimsAllah_text2, "");
                                }
                            }
                        }

                        // Load WawAsWord words
                        if (waw_as_word)
                        {
                            LoadWawWords();

                            if (s_waw_words != null)
                            {
                                // replace shadda with previous letter and to waw exception list
                                if (shadda_as_letter)
                                {
                                    for (int i = 0; i < lines.Count; i++)
                                    {
                                        string[] word_texts = lines[i].Split();
                                        foreach (string word_text in word_texts)
                                        {
                                            if (s_waw_words.Contains(s_simplification_system.Simplify(word_text)))
                                            {
                                                if (word_text.Contains("ّ"))
                                                {
                                                    string shadda_waw_word = null;
                                                    for (int j = 1; j < word_text.Length; j++)
                                                    {
                                                        if (word_text[j] == 'ّ')
                                                        {
                                                            shadda_waw_word = word_text.Insert(j, word_text[j - 1].ToString());
                                                            s_waw_words.Add(s_simplification_system.Simplify(shadda_waw_word));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // replace shadda with previous letter before any simplification
                        if (shadda_as_letter)
                        {
                            for (int i = 0; i < lines.Count; i++)
                            {
                                StringBuilder str = new StringBuilder(lines[i]);
                                for (int j = 1; j < str.Length; j++)
                                {
                                    if (str[j] == 'ّ')
                                    {
                                        str[j] = str[j - 1];
                                    }
                                }
                                lines[i] = str.ToString();
                            }
                        }

                        // convert superscript above horizontal line to letter
                        for (int i = 0; i < lines.Count; i++)
                        {
                            if (hamza_above_horizontal_line_as_letter)
                            {
                                lines[i] = lines[i].Replace("ـٔ", "ـء");
                            }

                            if (elf_above_horizontal_line_as_letter)
                            {
                                lines[i] = lines[i].Replace("ـٰ", "ا");
                                lines[i] = lines[i].Replace("ٰ", "ا");
                            }

                            if (yaa_above_horizontal_line_as_letter)
                            {
                                lines[i] = lines[i].Replace("ـۧ", "ي");
                            }

                            if (noon_above_horizontal_line_as_letter)
                            {
                                lines[i] = lines[i].Replace("ـۨ", "ن");
                            }
                        }

                        // simplify verse texts
                        List<string> verse_texts = new List<string>();
                        foreach (string line in lines)
                        {
                            string verse_text = s_simplification_system.Simplify(line);
                            verse_texts.Add(verse_text);
                        }
                        // AFTER SIMPLIFICATION
                        // remove final diacritis/harakat of each Quran word
                        if (text_mode == "SimplifiedMarks")
                        {
                            for (int i = 0; i < verse_texts.Count; i++)
                            {
                                string[] word_texts = verse_texts[i].Split();
                                verse_texts[i] = ""; // empty to fill with modified word_texts
                                for (int j = 0; j < word_texts.Length; j++)
                                {
                                    string w = word_texts[j];
                                    // remove diacritics from last letter in a word 
                                    // keep shadda above last letter of word
                                    while ((!Constants.ARABIC_LETTERS.Contains(w[w.Length - 1])) && (w[w.Length - 1] != 'ّ'))
                                    {
                                        // remove all diacritics
                                        w = w.Remove(w.Length - 1);
                                    }

                                    // remove shadda above first letter of word
                                    if (w.Length > 1)
                                    {
                                        if (w[1] == 'ّ')
                                        {
                                            w = w.Remove(1, 1);
                                        }
                                    }

                                    // then assign
                                    word_texts[j] = w;

                                    // fill with modified word_texts
                                    verse_texts[i] += word_texts[j] + " ";
                                }

                                // remove last " "
                                verse_texts[i] = verse_texts[i].Remove(verse_texts[i].Length - 1);

                                // إِلَـٰفِهِمْ restore back to إِۦلَـٰفِهِمْ
                                if (verse_texts[i].Contains("إِلَـٰفِهِم"))
                                {
                                    verse_texts[i] = verse_texts[i].Replace("إِلَـٰفِهِم", "إِۦلَـٰفِهِمْ");
                                }
                            }
                        }

                        // build verses
                        List<Verse> verses = new List<Verse>();
                        for (int i = 0; i < verse_texts.Count; i++)
                        {
                            Verse verse = new Verse(i + 1, verse_texts[i], verse_stopmarks[i]);
                            if (verse != null)
                            {
                                verses.Add(verse);
                                verse.ApplyWordStopmarks(lines[i]);
                            }
                        }

                        if (s_numerical_system != null)
                        {
                            s_book = new Book(text_mode, verses, s_numerical_system.AddDistancesWithinChapters, with_diacritics);
                            if (s_book != null)
                            {
                                s_book.WithBismAllah = with_bism_Allah;
                                s_book.WawAsWord = waw_as_word;
                                s_book.ShaddaAsLetter = shadda_as_letter;
                                s_book.HamzaAboveHorizontalLineAsLetter = hamza_above_horizontal_line_as_letter;
                                s_book.ElfAboveHorizontalLineAsLetter = elf_above_horizontal_line_as_letter;
                                s_book.YaaAboveHorizontalLineAsLetter = yaa_above_horizontal_line_as_letter;
                                s_book.NoonAboveHorizontalLineAsLetter = noon_above_horizontal_line_as_letter;

                                // build words before DataAccess.Loads
                                if (waw_as_word)
                                {
                                    SplitWawPrefixsAsWords(s_book, text_mode);

                                    // update verses/words/letters numbers and distances
                                    if (s_numerical_system != null)
                                    {
                                        s_book.SetupNumbers();
                                        s_book.SetupFrequenciesAndOccurrences(with_diacritics);
                                        s_book.SetupDistances(s_numerical_system.AddDistancesWithinChapters, with_diacritics);
                                    }
                                }

                                DataAccess.LoadTranslationInfos(s_book);
                                DataAccess.LoadTranslations(s_book);
                                DataAccess.UpdateWordMeanings(s_book, DEFAULT_WORD_MEANINGS);
                                DataAccess.UpdateVerseTransliteration(s_book, DEFAULT_EMLAAEI_TEXT);
                                DataAccess.UpdateVerseTransliteration(s_book, DEFAULT_TRANSLITERATION);

                                DataAccess.LoadRecitationInfos(s_book);
                                DataAccess.LoadWordParts(s_book);
                                DataAccess.LoadWordRoots(s_book);

                                CalculateValue(s_book, false);
                            }
                        }
                    }
                }
            }
        }
    }
    private static List<string> s_waw_words = null;
    private static void LoadWawWords()
    {
        string path = Globals.DATA_FOLDER + Path.DirectorySeparatorChar + "waw-words.txt";
        if (File.Exists(path))
        {
            s_waw_words = new List<string>();
            if (s_waw_words != null)
            {
                List<string> lines = FileHelper.LoadLines(path);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length > 0)
                    {
                        s_waw_words.Add(parts[0]);
                    }
                }
            }
        }
    }
    private static void SplitWawPrefixsAsWords(Book book, string text_mode)
    {
        if (book != null)
        {
            string path = Globals.DATA_FOLDER + Path.DirectorySeparatorChar + "waw-words.txt";
            if (File.Exists(path))
            {
                // same spelling waw-words but their waw is prefix
                Dictionary<string, List<Verse>> non_exception_words_in_verses = new Dictionary<string, List<Verse>>();

                List<string> lines = FileHelper.LoadLines(path);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length > 0)
                    {
                        string exception_word = parts[0];
                        if (parts.Length > 1)
                        {
                            string[] sub_parts = parts[1].Split(',');
                            foreach (string sub_part in sub_parts)
                            {
                                string[] verse_address_parts = sub_part.Split(':');
                                if (verse_address_parts.Length == 2)
                                {
                                    try
                                    {
                                        Chapter chapter = book.Chapters[int.Parse(verse_address_parts[0]) - 1];
                                        if (chapter != null)
                                        {
                                            Verse verse = chapter.Verses[int.Parse(verse_address_parts[1]) - 1];
                                            if (verse != null)
                                            {
                                                if (non_exception_words_in_verses.ContainsKey(exception_word))
                                                {
                                                    List<Verse> verses = non_exception_words_in_verses[exception_word];
                                                    if (verses != null)
                                                    {
                                                        verses.Add(verse);
                                                    }
                                                }
                                                else
                                                {
                                                    List<Verse> verses = new List<Verse>();
                                                    if (verses != null)
                                                    {
                                                        verses.Add(verse);
                                                        non_exception_words_in_verses.Add(exception_word, verses);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        // skip error
                                    }
                                }
                            }
                        }
                    }
                }

                if (s_waw_words != null)
                {
                    foreach (Verse verse in book.Verses)
                    {
                        if (verse != null)
                        {
                            if (verse.Words != null)
                            {
                                StringBuilder str = new StringBuilder();
                                if (verse.Words.Count > 0)
                                {
                                    for (int i = 0; i < verse.Words.Count; i++)
                                    {
                                        if (verse.Words[i].Text.StartsWith("و"))
                                        {
                                            if (!s_waw_words.Contains(verse.Words[i].Text))
                                            {
                                                str.Append(verse.Words[i].Text.Insert(1, " ") + " ");
                                            }
                                            else // don't split exception words unless they are in non_exception_words_in_verses
                                            {
                                                if (non_exception_words_in_verses.ContainsKey(verse.Words[i].Text))
                                                {
                                                    if (non_exception_words_in_verses[verse.Words[i].Text].Contains(verse))
                                                    {
                                                        str.Append(verse.Words[i].Text.Insert(1, " ") + " ");
                                                    }
                                                    else
                                                    {
                                                        str.Append(verse.Words[i].Text + " ");
                                                    }
                                                }
                                                else
                                                {
                                                    str.Append(verse.Words[i].Text + " ");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            str.Append(verse.Words[i].Text + " ");
                                        }
                                    }
                                    if (str.Length > 1)
                                    {
                                        str.Remove(str.Length - 1, 1); // " "
                                    }

                                    // re-create new Words with word stopmarks
                                    verse.RecreateWordsApplyStopmarks(str.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // loaded numerical systems [STATIC]
    private static Dictionary<string, NumericalSystem> s_loaded_numerical_systems = null;
    public static Dictionary<string, NumericalSystem> LoadedNumericalSystems
    {
        get { return s_loaded_numerical_systems; }
    }
    public static List<string> s_extra_system_patterns = new List<string>();
    public static void LoadExtraSystemPatterns()
    {
        string path = Globals.VALUES_FOLDER + Path.DirectorySeparatorChar + "_ExtraSystems" + ".txt";
        if (File.Exists(path))
        {
            try
            {
                FileHelper.WaitForFileReady(path);
                using (StreamReader reader = File.OpenText(path))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (String.IsNullOrEmpty(line))
                            continue;
                        if (line.StartsWith("#"))
                            continue;

                        s_extra_system_patterns.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ".\r\n\r\n" + ex.StackTrace);
            }
        }
    }
    private static void LoadNumericalSystems()
    {
        LoadExtraSystemPatterns();

        if (s_loaded_numerical_systems == null)
        {
            s_loaded_numerical_systems = new Dictionary<string, NumericalSystem>();
        }

        if (s_loaded_numerical_systems != null)
        {
            s_loaded_numerical_systems.Clear();

            if (Directory.Exists(Globals.VALUES_FOLDER))
            {
                string path = Globals.VALUES_FOLDER;
                DirectoryInfo folder = new DirectoryInfo(path);
                if (folder != null)
                {
                    FileInfo[] files = folder.GetFiles("*.txt");
                    if ((files != null) && (files.Length > 0))
                    {
                        foreach (FileInfo file in files)
                        {
                            string numerical_system_name = file.Name.Remove(file.Name.Length - 4, 4);

                            if (!String.IsNullOrEmpty(numerical_system_name))
                            {
                                if ((Globals.EDITION == Edition.Standard) || (Globals.EDITION == Edition.Research))
                                {
                                    if (s_extra_system_patterns != null)
                                    {
                                        bool skip = false;
                                        foreach (string pattern in s_extra_system_patterns)
                                        {
                                            if (numerical_system_name.Contains(pattern))
                                            {
                                                skip = true;
                                                break;
                                            }
                                        }
                                        if (skip)
                                            continue;
                                    }
                                }

                                string[] parts = numerical_system_name.Split('_');
                                if (parts.Length == 3)
                                {
                                    if (s_loaded_simplification_systems.ContainsKey(parts[0]))
                                    {
                                        LoadNumericalSystem(numerical_system_name);
                                    }
                                }
                                else
                                {
                                    if (!file.Name.StartsWith("_"))
                                    {
                                        //TODO Cannot carry exception over Interface notification
                                        //throw new Exception("ERROR: " + file.FullName + " must contain 3 parts separated by \"_\"");
                                    }
                                }
                            }
                        }

                        // start with default numerical system
                        if (s_loaded_numerical_systems.ContainsKey(NumericalSystem.DEFAULT_NAME))
                        {
                            s_numerical_system = new NumericalSystem(s_loaded_numerical_systems[NumericalSystem.DEFAULT_NAME]);
                        }
                        else
                        {
                            //TODO Cannot carry exception over Interface notification
                            //throw new Exception("ERROR: No default numerical system was found");
                        }
                    }
                }
            }
        }
    }
    // numerical system [DYNAMIC]
    private static NumericalSystem s_numerical_system = null;
    public static NumericalSystem NumericalSystem
    {
        get { return s_numerical_system; }
        set { s_numerical_system = value; }
    }
    public static void LoadNumericalSystem(string numerical_system_name)
    {
        if (String.IsNullOrEmpty(numerical_system_name))
            return;
        if (numerical_system_name.Contains("DNA") || (numerical_system_name.Contains("ConvertTo")))
            return;

        if (s_loaded_numerical_systems != null)
        {
            string path = Globals.VALUES_FOLDER + Path.DirectorySeparatorChar + numerical_system_name + ".txt";
            if (File.Exists(path))
            {
                List<string> lines = FileHelper.LoadLines(path);

                NumericalSystem numerical_system = new NumericalSystem(numerical_system_name);
                if (numerical_system != null)
                {
                    numerical_system.LetterValues.Clear();
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("#"))
                            continue;

                        string[] parts = line.Split('\t');
                        if (parts.Length == 2)
                        {
                            try
                            {
                                numerical_system.LetterValues.Add(parts[0][0], long.Parse(parts[1]));
                            }
                            catch
                            {
                                //TODO Cannot carry exception over Interface notification
                                //throw new Exception(path + " file format must be:\r\n\tLetter TAB Value");
                            }
                        }
                        else
                        {
                            //TODO Cannot carry exception over Interface notification
                            //throw new Exception(path + " file format must be:\r\n\tLetter TAB Value");
                        }
                    }
                }

                if (s_loaded_numerical_systems.ContainsKey(numerical_system_name))
                {
                    s_loaded_numerical_systems[numerical_system.Name] = numerical_system;
                }
                else
                {
                    s_loaded_numerical_systems.Add(numerical_system.Name, numerical_system);
                }

                // set current numerical system
                if (s_loaded_numerical_systems.ContainsKey(numerical_system_name))
                {
                    s_numerical_system = new NumericalSystem(s_loaded_numerical_systems[numerical_system_name]);

                    if (Globals.EDITION == Edition.BigNumbers)
                    {
                        // SLOW !!!
                        // update value for CompareBy.Value for all
                        if (s_numerical_system != null)
                        {
                            if (s_book != null)
                            {
                                foreach (Chapter chapter in s_book.Chapters)
                                {
                                    CalculateValue(chapter, false);
                                    foreach (Verse verse in chapter.Verses)
                                    {
                                        if (verse != null)
                                        {
                                            CalculateValue(verse, false);
                                            foreach (Word word in verse.Words)
                                            {
                                                if (word != null)
                                                {
                                                    CalculateValue(word, false);
                                                    foreach (Letter letter in word.Letters)
                                                    {
                                                        if (letter != null)
                                                        {
                                                            CalculateValue(letter, false);
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
                }
            }
        }
    }
    public static void SaveNumericalSystem(string numerical_system_name)
    {
        if (String.IsNullOrEmpty(numerical_system_name))
            return;
        if (numerical_system_name.Contains("DNA") || (numerical_system_name.Contains("Conversion")))
            return;

        if (s_loaded_numerical_systems != null)
        {
            if (s_loaded_numerical_systems.ContainsKey(numerical_system_name))
            {
                NumericalSystem numerical_system = s_loaded_numerical_systems[numerical_system_name];
                if (numerical_system != null)
                {
                    if (Directory.Exists(Globals.VALUES_FOLDER))
                    {
                        string path = Globals.VALUES_FOLDER + Path.DirectorySeparatorChar + numerical_system.Name + ".txt";
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                            {
                                foreach (char key in numerical_system.Keys)
                                {
                                    writer.WriteLine(key + "\t" + numerical_system[key].ToString());
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
        }
    }
    public static void UpdateNumericalSystem(string dynamic_text, bool? include_diacritics)
    {
        if (s_numerical_system != null)
        {
            if (!String.IsNullOrEmpty(dynamic_text))
            {
                dynamic_text = dynamic_text.Simplify(s_numerical_system.TextMode);

                dynamic_text = dynamic_text.Replace("\r", "");
                dynamic_text = dynamic_text.Replace("\n", " ");
                dynamic_text = dynamic_text.Replace("\t", "");
                dynamic_text = dynamic_text.Replace("_", "");
                dynamic_text = dynamic_text.Replace(" ", "");
                dynamic_text = dynamic_text.Replace(Constants.ORNATE_RIGHT_PARENTHESIS, "");
                dynamic_text = dynamic_text.Replace(Constants.ORNATE_LEFT_PARENTHESIS, "");
                foreach (char character in Constants.INDIAN_DIGITS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.ARABIC_DIGITS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.SYMBOLS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.STOPMARKS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.QURANMARKS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.DIACRITICS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                dynamic_text = dynamic_text.Trim();

                BuildLetterStatistics(dynamic_text, include_diacritics);

                BuildNumericalSystem(dynamic_text);

                if (s_book != null)
                {
                    if (s_book.Verses != null)
                    {
                        foreach (Verse verse in s_book.Verses)
                        {
                            if (verse != null)
                            {
                                CalculateValue(verse, false);
                            }
                        }
                    }
                }
            }
        }
    }
    private static void BuildNumericalSystem(string text)
    {
        if (s_loaded_numerical_systems != null)
        {
            if (s_numerical_system != null)
            {
                if (text != null)
                {
                    // build letter_order using letters in text only
                    string numerical_system_name = s_numerical_system.Name;
                    if (s_loaded_numerical_systems.ContainsKey(numerical_system_name))
                    {
                        NumericalSystem loaded_numerical_system = s_loaded_numerical_systems[numerical_system_name];

                        // re-generate numerical systems
                        List<char> letter_order = new List<char>();
                        List<long> letter_values = new List<long>();

                        switch (s_numerical_system.LetterOrder)
                        {
                            case "Alphabet":
                            case "Alphabet▲":
                                {
                                    LetterStatistic.CompareBy = StatisticCompareBy.Letter;
                                    LetterStatistic.CompareOrder = StatisticCompareOrder.Ascending;
                                    s_letter_statistics.Sort();
                                    foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                    {
                                        letter_order.Add(letter_statistic.Letter);
                                    }
                                }
                                break;
                            case "Alphabet▼":
                                {
                                    LetterStatistic.CompareBy = StatisticCompareBy.Letter;
                                    LetterStatistic.CompareOrder = StatisticCompareOrder.Descending;
                                    s_letter_statistics.Sort();
                                    foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                    {
                                        letter_order.Add(letter_statistic.Letter);
                                    }
                                }
                                break;
                            case "Appearance":
                            case "Appearance▲":
                                {
                                    LetterStatistic.CompareBy = StatisticCompareBy.Order;
                                    LetterStatistic.CompareOrder = StatisticCompareOrder.Ascending;
                                    s_letter_statistics.Sort();
                                    foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                    {
                                        letter_order.Add(letter_statistic.Letter);
                                    }
                                }
                                break;
                            case "Appearance▼":
                                {
                                    LetterStatistic.CompareBy = StatisticCompareBy.Order;
                                    LetterStatistic.CompareOrder = StatisticCompareOrder.Descending;
                                    s_letter_statistics.Sort();
                                    foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                    {
                                        letter_order.Add(letter_statistic.Letter);
                                    }
                                }
                                break;
                            case "Frequency▲":
                                {
                                    LetterStatistic.CompareBy = StatisticCompareBy.Frequency;
                                    LetterStatistic.CompareOrder = StatisticCompareOrder.Ascending;
                                    s_letter_statistics.Sort();
                                    foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                    {
                                        letter_order.Add(letter_statistic.Letter);
                                    }
                                }
                                break;
                            case "Frequency▼":
                            case "Frequency":
                                {
                                    LetterStatistic.CompareBy = StatisticCompareBy.Frequency;
                                    LetterStatistic.CompareOrder = StatisticCompareOrder.Descending;
                                    s_letter_statistics.Sort();
                                    foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                    {
                                        letter_order.Add(letter_statistic.Letter);
                                    }
                                }
                                break;
                            default: // use static numerical system
                                {
                                    foreach (char character in loaded_numerical_system.LetterValues.Keys)
                                    {
                                        if (text.Contains(character.ToString()))
                                        {
                                            letter_order.Add(character);
                                        }
                                    }
                                }
                                break;
                        }

                        if (letter_order.Count > 0)
                        {
                            if (s_numerical_system.Name.EndsWith("Linear"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    letter_values.Add(i + 1L);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("NonAdditivePrimes1"))
                            {
                                if (s_numerical_system.Name.Contains("Alphabet"))
                                {
                                    if (text.Contains("ء"))
                                    {
                                        letter_values.Add(1L);
                                        for (int i = 0; i < letter_order.Count - 1; i++)
                                        {
                                            letter_values.Add(Numbers.NonAdditivePrimes[i]);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < letter_order.Count; i++)
                                        {
                                            letter_values.Add(Numbers.NonAdditivePrimes[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < letter_order.Count; i++)
                                    {
                                        letter_values.Add(Numbers.NonAdditivePrimes[i]);
                                    }
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("NonAdditivePrimes"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    letter_values.Add(Numbers.NonAdditivePrimes[i]);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("AdditivePrimes1"))
                            {
                                if (s_numerical_system.Name.Contains("Alphabet"))
                                {
                                    if (text.Contains("ء"))
                                    {
                                        letter_values.Add(1L);
                                        for (int i = 0; i < letter_order.Count - 1; i++)
                                        {
                                            letter_values.Add(Numbers.AdditivePrimes[i]);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < letter_order.Count; i++)
                                        {
                                            letter_values.Add(Numbers.AdditivePrimes[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < letter_order.Count; i++)
                                    {
                                        letter_values.Add(Numbers.AdditivePrimes[i]);
                                    }
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("AdditivePrimes"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    letter_values.Add(Numbers.AdditivePrimes[i]);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Primes1"))
                            {
                                if (s_numerical_system.Name.Contains("Alphabet"))
                                {
                                    if (text.Contains("ء"))
                                    {
                                        letter_values.Add(1L);
                                        for (int i = 0; i < letter_order.Count - 1; i++)
                                        {
                                            letter_values.Add(Numbers.Primes[i]);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < letter_order.Count; i++)
                                        {
                                            letter_values.Add(Numbers.Primes[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < letter_order.Count; i++)
                                    {
                                        letter_values.Add(Numbers.Primes[i]);
                                    }
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Primes"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    letter_values.Add(Numbers.Primes[i]);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("NonAdditiveComposites"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    letter_values.Add(Numbers.NonAdditiveComposites[i]);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("AdditiveComposites"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    letter_values.Add(Numbers.AdditiveComposites[i]);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Composites"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    letter_values.Add(Numbers.Composites[i]);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("MersennePrimeExponents"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    if (i < Numbers.MersennePrimeExponents.Count)
                                    {
                                        letter_values.Add(Numbers.MersennePrimeExponents[i]);
                                    }
                                    else
                                    {
                                        letter_values.Add(0L);
                                    }
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Fibonacci"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    if (i < Numbers.FibonacciNumbers.Count)
                                    {
                                        letter_values.Add(Numbers.FibonacciNumbers[i]);
                                    }
                                    else
                                    {
                                        letter_values.Add(0L);
                                    }
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Frequency▲"))
                            {
                                // letter-frequency mismacth: different letters for different frequencies
                                LetterStatistic.CompareBy = StatisticCompareBy.Frequency;
                                LetterStatistic.CompareOrder = StatisticCompareOrder.Ascending;
                                s_letter_statistics.Sort();
                                foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                {
                                    letter_values.Add(letter_statistic.Frequency);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Frequency"))
                            {
                                letter_order.Clear();
                                foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                {
                                    letter_order.Add(letter_statistic.Letter);
                                    letter_values.Add(letter_statistic.Frequency);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Frequency▼"))
                            {
                                // letter-frequency mismacth: different letters for different frequencies
                                LetterStatistic.CompareBy = StatisticCompareBy.Frequency;
                                LetterStatistic.CompareOrder = StatisticCompareOrder.Descending;
                                s_letter_statistics.Sort();
                                foreach (LetterStatistic letter_statistic in s_letter_statistics)
                                {
                                    letter_values.Add(letter_statistic.Frequency);
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("Gematria"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    if (i < Numbers.GematriaNumbers.Count)
                                    {
                                        letter_values.Add(Numbers.GematriaNumbers[i]);
                                    }
                                    else
                                    {
                                        letter_values.Add(0L);
                                    }
                                }
                            }
                            else if (s_numerical_system.Name.EndsWith("QuranNumbers"))
                            {
                                for (int i = 0; i < letter_order.Count; i++)
                                {
                                    if (i < Numbers.QuranNumbers.Count)
                                    {
                                        letter_values.Add(Numbers.QuranNumbers[i]);
                                    }
                                    else
                                    {
                                        letter_values.Add(0L);
                                    }
                                }
                            }
                            else // if not defined in Numbers
                            {
                                // use loadeded numerical system instead
                                foreach (long value in loaded_numerical_system.LetterValues.Values)
                                {
                                    letter_values.Add(value);
                                }
                            }
                        }
                        else // if not defined in Numbers
                        {
                            // use loadeded numerical system instead
                            foreach (long value in loaded_numerical_system.LetterValues.Values)
                            {
                                letter_values.Add(value);
                            }
                        }

                        // rebuild the current numerical system
                        s_numerical_system.Clear();
                        for (int i = 0; i < letter_order.Count; i++)
                        {
                            s_numerical_system.Add(letter_order[i], letter_values[i]);
                        }
                    }
                }
            }
        }
    }
    // letter statistics [DYNAMIC]
    private static List<LetterStatistic> s_letter_statistics = new List<LetterStatistic>();
    private static void BuildLetterStatistics(string dynamic_text, bool? include_diacritics)
    {
        if (String.IsNullOrEmpty(dynamic_text)) return;

        if (s_letter_statistics != null)
        {
            if (NumericalSystem != null)
            {
                if (include_diacritics == true) { /* do nothing */ }
                else if (include_diacritics == null) { dynamic_text = dynamic_text.GetDiacritics(); }
                else if (include_diacritics == false) { dynamic_text = dynamic_text.Simplify(NumericalSystem.TextMode); }

                dynamic_text = dynamic_text.Replace("\r", "");
                dynamic_text = dynamic_text.Replace("\n", "");
                dynamic_text = dynamic_text.Replace("\t", "");
                dynamic_text = dynamic_text.Replace("_", "");
                dynamic_text = dynamic_text.Replace(" ", "");
                dynamic_text = dynamic_text.Replace(Constants.ORNATE_RIGHT_PARENTHESIS, "");
                dynamic_text = dynamic_text.Replace(Constants.ORNATE_LEFT_PARENTHESIS, "");
                foreach (char character in Constants.INDIAN_DIGITS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.ARABIC_DIGITS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }
                foreach (char character in Constants.SYMBOLS)
                {
                    dynamic_text = dynamic_text.Replace(character.ToString(), "");
                }

                s_letter_statistics.Clear();
                for (int i = 0; i < dynamic_text.Length; i++)
                {
                    // calculate letter frequency
                    bool is_found = false;
                    for (int j = 0; j < s_letter_statistics.Count; j++)
                    {
                        if (dynamic_text[i] == s_letter_statistics[j].Letter)
                        {
                            s_letter_statistics[j].Frequency++;
                            is_found = true;
                            break;
                        }
                    }

                    // add entry into dictionary
                    if (!is_found)
                    {
                        LetterStatistic letter_statistic = new LetterStatistic();
                        letter_statistic.Order = s_letter_statistics.Count + 1;
                        letter_statistic.Letter = dynamic_text[i];
                        letter_statistic.Frequency++;
                        s_letter_statistics.Add(letter_statistic);
                    }
                }
            }
        }
    }

    // loaded dna sequence systems[STATIC]
    private static Dictionary<string, DNASequenceSystem> s_loaded_dna_sequence_systems = null;
    public static Dictionary<string, DNASequenceSystem> LoadedDNASequenceSystems
    {
        get { return s_loaded_dna_sequence_systems; }
    }
    private static void LoadDNASequenceSystems()
    {
        if (s_loaded_dna_sequence_systems == null)
        {
            s_loaded_dna_sequence_systems = new Dictionary<string, DNASequenceSystem>();
        }

        if (s_loaded_dna_sequence_systems != null)
        {
            s_loaded_dna_sequence_systems.Clear();

            string path = Globals.VALUES_FOLDER;
            DirectoryInfo folder = new DirectoryInfo(path);
            if (folder != null)
            {
                FileInfo[] files = folder.GetFiles("*.txt");
                if ((files != null) && (files.Length > 0))
                {
                    foreach (FileInfo file in files)
                    {
                        string dna_sequence_system_name = file.Name.Remove(file.Name.Length - 4, 4);
                        if (!String.IsNullOrEmpty(dna_sequence_system_name))
                        {
                            //if (dna_sequence_system_name.Contains("DNA"))
                            if (dna_sequence_system_name.Contains("DNA") || (dna_sequence_system_name.Contains("ConvertTo")))
                            {
                                string[] parts = dna_sequence_system_name.Split('_');
                                if (parts.Length == 3)
                                {
                                    LoadDNASequenceSystem(dna_sequence_system_name);
                                }
                                else
                                {
                                    //TODO Cannot carry exception over Interface notification
                                    //throw new Exception("ERROR: " + file.FullName + " must contain 3 parts separated by \"_\"");
                                }
                            }
                        }
                    }

                    // start with default dna_sequence system
                    if (s_loaded_dna_sequence_systems.ContainsKey(DNASequenceSystem.DEFAULT_NAME))
                    {
                        s_dna_sequence_system = new DNASequenceSystem(s_loaded_dna_sequence_systems[DNASequenceSystem.DEFAULT_NAME]);
                    }
                    else
                    {
                        //TODO Cannot carry exception over Interface notification
                        //throw new Exception("ERROR: No default dna sequence system was found");
                    }
                }
            }
        }
    }
    // dna sequence system [DYNAMIC]
    private static DNASequenceSystem s_dna_sequence_system = null;
    public static DNASequenceSystem DNASequenceSystem
    {
        get { return s_dna_sequence_system; }
    }
    public static void LoadDNASequenceSystem(string dna_sequence_system_name)
    {
        if (String.IsNullOrEmpty(dna_sequence_system_name))
            return;

        //if (dna_sequence_system_name.Contains("DNA"))
        if (dna_sequence_system_name.Contains("DNA") || (dna_sequence_system_name.Contains("ConvertTo")))
        {
            if (s_loaded_dna_sequence_systems != null)
            {
                // remove and rebuild on the fly without restarting application
                if (s_loaded_dna_sequence_systems.ContainsKey(dna_sequence_system_name))
                {
                    s_loaded_dna_sequence_systems.Remove(dna_sequence_system_name);
                }

                string path = Globals.VALUES_FOLDER + Path.DirectorySeparatorChar + dna_sequence_system_name + ".txt";
                if (File.Exists(path))
                {
                    List<string> lines = FileHelper.LoadLines(path);

                    DNASequenceSystem dna_sequence_system = new DNASequenceSystem(dna_sequence_system_name);
                    if (dna_sequence_system != null)
                    {
                        dna_sequence_system.LetterValues.Clear();
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("#"))
                                continue;

                            string[] parts = line.Split('\t');
                            if (parts.Length == 2)
                            {
                                dna_sequence_system.LetterValues.Add(parts[0][0], parts[1][0]);
                            }
                            else
                            {
                                //TODO Cannot carry exception over Interface notification
                                //throw new Exception(path + " file format must be:\r\n\tLetter TAB Value");
                            }
                        }
                    }

                    try
                    {
                        // add to dictionary
                        s_loaded_dna_sequence_systems.Add(dna_sequence_system.Name, dna_sequence_system);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + ".\r\n\r\n" + ex.StackTrace);
                    }
                }

                // set current dna_sequence system
                if (s_loaded_dna_sequence_systems.ContainsKey(dna_sequence_system_name))
                {
                    s_dna_sequence_system = s_loaded_dna_sequence_systems[dna_sequence_system_name];
                }
            }
        }
    }
    public static void SaveDNASequenceSystem(string dna_sequence_system_name)
    {
        if (String.IsNullOrEmpty(dna_sequence_system_name))
            return;

        //if (dna_sequence_system_name.Contains("DNA"))
        if (dna_sequence_system_name.Contains("DNA") || (dna_sequence_system_name.Contains("ConvertTo")))
        {
            if (s_loaded_dna_sequence_systems != null)
            {
                if (s_loaded_dna_sequence_systems.ContainsKey(dna_sequence_system_name))
                {
                    DNASequenceSystem dna_sequence_system = s_loaded_dna_sequence_systems[dna_sequence_system_name];
                    if (dna_sequence_system != null)
                    {
                        if (Directory.Exists(Globals.VALUES_FOLDER))
                        {
                            string path = Globals.VALUES_FOLDER + Path.DirectorySeparatorChar + dna_sequence_system.Name + ".txt";
                            try
                            {
                                using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                                {
                                    foreach (char key in dna_sequence_system.Keys)
                                    {
                                        writer.WriteLine(key + "\t" + dna_sequence_system[key].ToString());
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
            }
        }
    }
    //// letter statistics [DYNAMIC]
    //private static List<LetterStatistic> s_letter_statistics = new List<LetterStatistic>();
    //private static void BuildLetterStatistics(string text)
    //{
    //    if (text == null) // null means Book scope
    //    {
    //        if (s_book != null)
    //        {
    //            text = s_book.Text;
    //        }
    //    }
    //    if (text != null)
    //    {
    //        if (s_letter_statistics != null)
    //        {
    //            s_letter_statistics.Clear();
    //            for (int i = 0; i < text.Length; i++)
    //            {
    //                // calculate letter frequency
    //                bool is_found = false;
    //                for (int j = 0; j < s_letter_statistics.Count; j++)
    //                {
    //                    if (text[i] == s_letter_statistics[j].Letter)
    //                    {
    //                        s_letter_statistics[j].Frequency++;
    //                        is_found = true;
    //                        break;
    //                    }
    //                }

    //                // add entry into dictionary
    //                if (!is_found)
    //                {
    //                    LetterStatistic letter_statistic = new LetterStatistic();
    //                    letter_statistic.Order = s_letter_statistics.Count + 1;
    //                    letter_statistic.Letter = text[i];
    //                    letter_statistic.Frequency++;
    //                    s_letter_statistics.Add(letter_statistic);
    //                }
    //            }
    //        }
    //    }
    //}
    private static void Dummy_CommentSeparator()
    {
    }

    // AddTo... 19 parameters
    private static long AdjustValue(Letter letter, bool logging)
    {
        if (logging && (Log == null)) return 0L;

        long result = 0L;

        long value = 0L;
        if (s_numerical_system != null)
        {
            if (letter != null)
            {
                if (logging) Log.Append("\t" + "\t" + "\t");

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToLetterLNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? letter.NumberInWord : letter.NumberInWord;
                    result += value;
                    Log_LSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToLetterWNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? letter.NumberInVerse : letter.Word.NumberInVerse;
                    result += value;
                    Log_WSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToLetterVNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? letter.NumberInChapter : letter.Word.Verse.NumberInChapter;
                    result += value;
                    Log_VSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToLetterCNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? letter.Number : letter.Word.Verse.Chapter.SortedNumber;
                    result += value;
                    Log_CSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToLetterLDistance)
                {
                    value = letter.DistanceToPrevious.dL;
                    result += value;
                    Log_pLSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToLetterWDistance)
                {
                    value = letter.DistanceToPrevious.dW;
                    result += value;
                    Log_pWSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToLetterVDistance)
                {
                    value = letter.DistanceToPrevious.dV;
                    result += value;
                    Log_pVSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToLetterCDistance)
                {
                    value = letter.DistanceToPrevious.dC;
                    result += value;
                    Log_pCSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToLetterLDistance)
                {
                    value = letter.DistanceToNext.dL;
                    result += value;
                    Log_nLSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToLetterWDistance)
                {
                    value = letter.DistanceToNext.dW;
                    result += value;
                    Log_nWSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToLetterVDistance)
                {
                    value = letter.DistanceToNext.dV;
                    result += value;
                    Log_nVSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToLetterCDistance)
                {
                    value = letter.DistanceToNext.dC;
                    result += value;
                    Log_nCSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.AppendLine();
            }
        }

        return result;
    }
    private static long AdjustValue(Word word, bool logging)
    {
        if (logging && (Log == null)) return 0L;

        long result = 0L;

        long value = 0L;
        if (s_numerical_system != null)
        {
            if (word != null)
            {
                if (logging) Log.Append("\t" + "\t");

                if (logging) Log.Append("\t" + "\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToWordWNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? word.NumberInVerse : word.NumberInVerse;
                    result += value;
                    Log_WSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToWordVNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? word.NumberInChapter : word.Verse.NumberInChapter;
                    result += value;
                    Log_VSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToWordCNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? word.Number : word.Verse.Chapter.SortedNumber;
                    result += value;
                    Log_CSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t" + "\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToWordWDistance)
                {
                    value = word.DistanceToPrevious.dW;
                    result += value;
                    Log_pWSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToWordVDistance)
                {
                    value = word.DistanceToPrevious.dV;
                    result += value;
                    Log_pVSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToWordCDistance)
                {
                    value = word.DistanceToPrevious.dC;
                    result += value;
                    Log_pCSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t" + "\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToWordWDistance)
                {
                    value = word.DistanceToNext.dW;
                    result += value;
                    Log_nWSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToWordVDistance)
                {
                    value = word.DistanceToNext.dV;
                    result += value;
                    Log_nVSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToWordCDistance)
                {
                    value = word.DistanceToNext.dC;
                    result += value;
                    Log_nCSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.AppendLine();
            }
        }

        return result;
    }
    private static long AdjustValue(Verse verse, bool logging)
    {
        if (logging && (Log == null)) return 0L;

        long result = 0L;

        long value = 0L;
        if (s_numerical_system != null)
        {
            if (verse != null)
            {
                if (logging) Log.Append("\t");

                if (logging) Log.Append("\t" + "\t" + "\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToVerseVNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? verse.NumberInChapter : verse.NumberInChapter;
                    result += value;
                    Log_VSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToVerseCNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? verse.Number : verse.Chapter.SortedNumber;
                    result += value;
                    Log_CSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t" + "\t" + "\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToVerseVDistance)
                {
                    value = verse.DistanceToPrevious.dV;
                    result += value;
                    Log_pVSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToPrevious && s_numerical_system.AddToVerseCDistance)
                {
                    value = verse.DistanceToPrevious.dC;
                    result += value;
                    Log_pCSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t" + "\t" + "\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToVerseVDistance)
                {
                    value = verse.DistanceToNext.dV;
                    result += value;
                    Log_nVSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.Append("\t");
                if (s_numerical_system.AddDistancesToNext && s_numerical_system.AddToVerseCDistance)
                {
                    value = verse.DistanceToNext.dC;
                    result += value;
                    Log_nCSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.AppendLine();
            }
        }

        return result;
    }
    private static long AdjustValue(Chapter chapter, bool logging)
    {
        if (logging && (Log == null)) return 0L;

        long result = 0L;

        long value = 0L;
        if (s_numerical_system != null)
        {
            if (chapter != null)
            {
                if (logging) Log.Append("");

                if (logging) Log.Append("\t" + "\t" + "\t" + "\t");
                if (s_numerical_system.AddPositions && s_numerical_system.AddToChapterCNumber)
                {
                    value = (s_numerical_system.AbsolutePositions) ? chapter.SortedNumber : chapter.SortedNumber;
                    result += value;
                    Log_CSum += value;
                    if (logging) Log.Append(value);
                }

                if (logging) Log.AppendLine();
            }
        }

        return result;
    }
    // calculation modes (coloured)
    public static CalculationMode CalculationMode = CalculationMode.SumOfLetterValues;
    public static bool AlternateLetterValues = false;
    public static bool AlternateWordValues = false;
    public static bool AlternateVerseValues = false;
    public static bool AlternateChapterValues = false;
    // log data to show how the final Value was calculated
    public static StringBuilder Log = null;
    public static long Log_Value = 0L;
    public static long Log_LSum = 0L;
    public static long Log_WSum = 0L;
    public static long Log_VSum = 0L;
    public static long Log_CSum = 0L;
    public static long Log_pLSum = 0L;
    public static long Log_pWSum = 0L;
    public static long Log_pVSum = 0L;
    public static long Log_pCSum = 0L;
    public static long Log_nLSum = 0L;
    public static long Log_nWSum = 0L;
    public static long Log_nVSum = 0L;
    public static long Log_nCSum = 0L;
    public static long Log_Total = 0L;
    // call from Client.cs only
    public static void ClearLog()
    {
        Log = new StringBuilder();
        Log_Value = 0L;
        Log_LSum = 0L;
        Log_WSum = 0L;
        Log_VSum = 0L;
        Log_CSum = 0L;
        Log_pLSum = 0L;
        Log_pWSum = 0L;
        Log_pVSum = 0L;
        Log_pCSum = 0L;
        Log_nLSum = 0L;
        Log_nWSum = 0L;
        Log_nVSum = 0L;
        Log_nCSum = 0L;
        Log_Total = 0L;
    }
    // calculate value of user text
    public static long CalculateValue(char character, bool logging)
    {
        if (s_numerical_system == null)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            result += s_numerical_system.CalculateValue(character);
            if (logging) Log.Append(character.ToString() + "\t" + result);
        }

        if (logging) Log_Total = result;

        return result;
    }
    public static long CalculateValue(string text, bool logging)
    {
        if (string.IsNullOrEmpty(text))
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            text = text.Replace("\r\n", "\n");
            text = text.Simplify(s_numerical_system.TextMode);

            switch (CalculationMode)
            {
                case CalculationMode.SumOfLetterValues:
                case CalculationMode.SumOfLetterValueDigitSums:
                case CalculationMode.SumOfLetterValueDigitalRoots:
                    {
                        char[] separators1 = { '\n' };
                        string[] verse_texts = text.Split(separators1, StringSplitOptions.RemoveEmptyEntries);

                        long v_sign = +1L;
                        long c_sign = +1L;
                        long chapter_value = 0L;
                        for (int i = 0; i < verse_texts.Length; i++)
                        {
                            if (
                                 (i > 0)
                                 &&
                                 (
                                    verse_texts[i].Simplify28().StartsWith("بسم الله الرحمن الرحيم ")
                                    ||
                                    verse_texts[i].Simplify28().StartsWith("براه من الله ورسوله")
                                 )
                               )
                            {
                                if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + chapter_value);

                                c_sign *= AlternateChapterValues ? -1L : +1L;
                                chapter_value = 0L;
                            }

                            long verse_value = 0L;

                            char[] separators2 = { ' ' };
                            string[] word_texts = verse_texts[i].Split(separators2, StringSplitOptions.RemoveEmptyEntries);

                            long w_sign = +1L;
                            foreach (string word_text in word_texts)
                            {
                                long word_value = 0L;

                                long l_sign = +1L;
                                foreach (char c in word_text)
                                {
                                    long letter_value = s_numerical_system.CalculateValue(c);

                                    if (CalculationMode == CalculationMode.SumOfLetterValues)
                                    {
                                        letter_value = l_sign * letter_value;
                                    }
                                    else if (CalculationMode == CalculationMode.SumOfLetterValueDigitSums)
                                    {
                                        letter_value = l_sign * Numbers.DigitSum(letter_value);
                                    }
                                    else if (CalculationMode == CalculationMode.SumOfLetterValueDigitalRoots)
                                    {
                                        letter_value = l_sign * Numbers.DigitalRoot(letter_value);
                                    }

                                    l_sign *= AlternateLetterValues ? -1L : +1L;
                                    if (logging) Log.AppendLine(c.ToString() + "\t" + letter_value);
                                    if (logging) Log_Value += letter_value;
                                    word_value += letter_value;
                                }

                                word_value = w_sign * word_value;
                                w_sign *= AlternateWordValues ? -1L : +1L;
                                if (logging) Log.AppendLine("\t" + "\t" + word_value);
                                verse_value += word_value;
                            }

                            verse_value = v_sign * verse_value;
                            v_sign *= AlternateVerseValues ? -1L : +1L;
                            if (logging) Log.AppendLine("\t" + "\t" + "\t" + verse_value);

                            chapter_value += c_sign * verse_value;
                            result += c_sign * verse_value;

                            if (i == verse_texts.Length - 1) // last line in text
                            {
                                if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + chapter_value);
                            }
                        }
                    }
                    break;
                case CalculationMode.SumOfWordValueDigitSums:
                case CalculationMode.SumOfWordValueDigitalRoots:
                    {
                        char[] separators1 = { '\n' };
                        string[] verse_texts = text.Split(separators1, StringSplitOptions.RemoveEmptyEntries);

                        long v_sign = +1L;
                        long c_sign = +1L;
                        long chapter_value = 0L;
                        for (int i = 0; i < verse_texts.Length; i++)
                        {
                            if (
                                 (i > 0)
                                 &&
                                 (
                                    verse_texts[i].Simplify28().StartsWith("بسم الله الرحمن الرحيم ")
                                    ||
                                    verse_texts[i].Simplify28().StartsWith("براه من الله ورسوله")
                                 )
                               )
                            {
                                if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + chapter_value);

                                c_sign *= AlternateChapterValues ? -1L : +1L;
                                chapter_value = 0L;
                            }

                            long verse_value = 0L;

                            char[] separators2 = { ' ' };
                            string[] word_texts = verse_texts[i].Split(separators2, StringSplitOptions.RemoveEmptyEntries);

                            long w_sign = +1L;
                            foreach (string word_text in word_texts)
                            {
                                long word_value = 0L;

                                long l_sign = +1L;
                                foreach (char c in word_text)
                                {
                                    long letter_value = s_numerical_system.CalculateValue(c);
                                    letter_value = l_sign * letter_value;
                                    l_sign *= AlternateLetterValues ? -1L : +1L;
                                    if (logging) Log.AppendLine(c.ToString() + "\t" + letter_value);
                                    if (logging) Log_Value += letter_value;
                                    word_value += letter_value;
                                }

                                if (CalculationMode == CalculationMode.SumOfWordValueDigitSums)
                                {
                                    word_value = Numbers.DigitSum(word_value);
                                }
                                else if (CalculationMode == CalculationMode.SumOfWordValueDigitalRoots)
                                {
                                    word_value = Numbers.DigitalRoot(word_value);
                                }

                                word_value = w_sign * word_value;
                                w_sign *= AlternateWordValues ? -1L : +1L;
                                if (logging) Log.AppendLine("\t" + "\t" + word_value);
                                verse_value += word_value;
                            }

                            verse_value = v_sign * verse_value;
                            v_sign *= AlternateVerseValues ? -1L : +1L;
                            if (logging) Log.AppendLine("\t" + "\t" + "\t" + verse_value);

                            chapter_value += c_sign * verse_value;
                            result += c_sign * verse_value;

                            if (i == verse_texts.Length - 1) // last line in text
                            {
                                if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + chapter_value);
                            }
                        }
                    }
                    break;
                case CalculationMode.SumOfVerseValueDigitSums:
                case CalculationMode.SumOfVerseValueDigitalRoots:
                    {
                        char[] separators1 = { '\n' };
                        string[] verse_texts = text.Split(separators1, StringSplitOptions.RemoveEmptyEntries);

                        long v_sign = +1L;
                        long c_sign = +1L;
                        long chapter_value = 0L;
                        for (int i = 0; i < verse_texts.Length; i++)
                        {
                            if (
                                 (i > 0)
                                 &&
                                 (
                                    verse_texts[i].Simplify28().StartsWith("بسم الله الرحمن الرحيم ")
                                    ||
                                    verse_texts[i].Simplify28().StartsWith("براه من الله ورسوله")
                                 )
                               )
                            {
                                if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + chapter_value);

                                c_sign *= AlternateChapterValues ? -1L : +1L;
                                chapter_value = 0L;
                            }

                            long verse_value = 0L;

                            char[] separators2 = { ' ' };
                            string[] word_texts = verse_texts[i].Split(separators2, StringSplitOptions.RemoveEmptyEntries);

                            long w_sign = +1L;
                            foreach (string word_text in word_texts)
                            {
                                long word_value = 0L;

                                long l_sign = +1L;
                                foreach (char c in word_text)
                                {
                                    long letter_value = s_numerical_system.CalculateValue(c);
                                    letter_value = l_sign * letter_value;
                                    l_sign *= AlternateLetterValues ? -1L : +1L;
                                    if (logging) Log.AppendLine(c.ToString() + "\t" + letter_value);
                                    if (logging) Log_Value += letter_value;
                                    word_value += letter_value;
                                }

                                word_value = w_sign * word_value;
                                w_sign *= AlternateWordValues ? -1L : +1L;
                                if (logging) Log.AppendLine("\t" + "\t" + word_value);
                                verse_value += word_value;
                            }

                            if (CalculationMode == CalculationMode.SumOfVerseValueDigitSums)
                            {
                                verse_value = Numbers.DigitSum(verse_value);
                            }
                            else if (CalculationMode == CalculationMode.SumOfVerseValueDigitalRoots)
                            {
                                verse_value = Numbers.DigitalRoot(verse_value);
                            }

                            verse_value = v_sign * verse_value;
                            v_sign *= AlternateVerseValues ? -1L : +1L;
                            if (logging) Log.AppendLine("\t" + "\t" + "\t" + verse_value);

                            chapter_value += c_sign * verse_value;
                            result += c_sign * verse_value;

                            if (i == verse_texts.Length - 1) // last line in text
                            {
                                if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + chapter_value);
                            }
                        }
                    }
                    break;
                case CalculationMode.SumOfUniqueLetterValues:
                    {
                        text = text.Simplify(s_numerical_system.TextMode);
                        text = text.Replace("\r", "");
                        text = text.Replace("\n", "");
                        text = text.Replace(" ", "");
                        text = text.RemovePunctuations();
                        text = text.RemoveDuplicates();

                        long l_sign = +1L;
                        foreach (char c in text)
                        {
                            long letter_value = l_sign * s_numerical_system.CalculateValue(c);
                            if (logging) Log.AppendLine(c.ToString() + "\t" + letter_value);
                            if (logging) Log_Value += letter_value;
                            l_sign *= AlternateLetterValues ? -1L : +1L;
                            result += letter_value;
                        }
                    }
                    break;
                default:
                    {
                        // do nothing
                    }
                    break;
            }
        }

        if (logging) Log_Total = result;
        return result;
    }
    // calculate value of Quran text
    public static long CalculateValue(Letter letter, bool logging)
    {
        if (letter == null)
            return 0L;

        long letter_value = 0L;

        switch (CalculationMode)
        {
            case CalculationMode.SumOfLetterValues:
                {
                    letter_value = s_numerical_system.CalculateValue(letter.Character);
                }
                break;
            case CalculationMode.SumOfLetterValueDigitSums:
                {
                    letter_value = Numbers.DigitSum(s_numerical_system.CalculateValue(letter.Character));
                }
                break;
            case CalculationMode.SumOfLetterValueDigitalRoots:
                {
                    letter_value = Numbers.DigitalRoot(s_numerical_system.CalculateValue(letter.Character));
                }
                break;
            case CalculationMode.SumOfWordValueDigitSums:
                {
                    if (letter.Word.Letters.Count == 1)
                    {
                        letter_value = Numbers.DigitSum(s_numerical_system.CalculateValue(letter.Character));
                    }
                }
                break;
            case CalculationMode.SumOfWordValueDigitalRoots:
                {
                    if (letter.Word.Letters.Count == 1)
                    {
                        letter_value = Numbers.DigitalRoot(s_numerical_system.CalculateValue(letter.Character));
                    }
                }
                break;
            case CalculationMode.SumOfVerseValueDigitSums:
                {
                    if (letter.Word.Letters.Count == 1)
                    {
                        if (letter.Word.Verse.Letters.Count == 1)
                        {
                            letter_value = Numbers.DigitSum(s_numerical_system.CalculateValue(letter.Character));
                        }
                    }
                }
                break;
            case CalculationMode.SumOfVerseValueDigitalRoots:
                {
                    if (letter.Word.Letters.Count == 1)
                    {
                        if (letter.Word.Verse.Letters.Count == 1)
                        {
                            letter_value = Numbers.DigitalRoot(s_numerical_system.CalculateValue(letter.Character));
                        }
                    }
                }
                break;
            case CalculationMode.SumOfUniqueLetterValues:
                {
                    letter_value = s_numerical_system.CalculateValue(letter.Character);
                }
                break;
            default:
                {
                    // do nothing
                }
                break;
        }

        if (logging) Log_Total = letter_value;

        letter.Value = letter_value; // update value for CompareBy.Value

        return letter_value;
    }
    public static long CalculateValue(Word word, bool logging)
    {
        if (word == null)
            return 0L;

        long word_value = 0L;

        if (s_numerical_system != null)
        {
            if (s_numerical_system.LetterValue.StartsWith("Base"))
            {
                string radix_str = "";
                int pos = 4;
                while (Char.IsDigit(s_numerical_system.LetterValue[pos]))
                {
                    radix_str += s_numerical_system.LetterValue[pos];
                    pos++;
                }
                int radix;
                if (int.TryParse(radix_str, out radix))
                {
                    StringBuilder str = new StringBuilder();
                    foreach (Letter letter in word.Letters)
                    {
                        str.Insert(0, s_numerical_system.CalculateValue(letter.Character));
                    }
                    word_value = Radix.Decode(str.ToString(), radix);
                    if (logging) Log.Append("\t" + "\t" + word_value);
                }
            }
            else
            {
                switch (CalculationMode)
                {
                    case CalculationMode.SumOfLetterValues:
                        {
                            long l_sign = +1L;
                            foreach (Letter letter in word.Letters)
                            {
                                long letter_value = l_sign * CalculateValue(letter, logging);
                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                if (logging) Log_Value += letter_value;
                                // adjust letter value
                                letter_value += l_sign * AdjustValue(letter, logging);
                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                word_value += letter_value;
                            }
                        }
                        break;
                    case CalculationMode.SumOfLetterValueDigitSums:
                        {
                            long l_sign = +1L;
                            foreach (Letter letter in word.Letters)
                            {
                                long letter_value = l_sign * Numbers.DigitSum(s_numerical_system.CalculateValue(letter.Character));
                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                if (logging) Log_Value += letter_value;
                                // adjust letter value
                                letter_value += l_sign * AdjustValue(letter, logging);
                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                word_value += letter_value;
                            }
                        }
                        break;
                    case CalculationMode.SumOfLetterValueDigitalRoots:
                        {
                            long l_sign = +1L;
                            foreach (Letter letter in word.Letters)
                            {
                                long letter_value = l_sign * Numbers.DigitalRoot(s_numerical_system.CalculateValue(letter.Character));
                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                if (logging) Log_Value += letter_value;
                                // adjust letter value
                                letter_value += l_sign * AdjustValue(letter, logging);
                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                word_value += letter_value;
                            }
                        }
                        break;
                    case CalculationMode.SumOfWordValueDigitSums:
                        {
                            long l_sign = +1L;
                            foreach (Letter letter in word.Letters)
                            {
                                long letter_value = l_sign * CalculateValue(letter, logging);
                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                if (logging) Log_Value += letter_value;
                                // adjust letter value
                                letter_value += l_sign * AdjustValue(letter, logging);
                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                word_value += letter_value;
                            }
                            word_value = Numbers.DigitSum(word_value);
                        }
                        break;
                    case CalculationMode.SumOfWordValueDigitalRoots:
                        {
                            long l_sign = +1L;
                            foreach (Letter letter in word.Letters)
                            {
                                long letter_value = l_sign * CalculateValue(letter, logging);
                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                if (logging) Log_Value += letter_value;
                                // adjust letter value
                                letter_value += l_sign * AdjustValue(letter, logging);
                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                word_value += letter_value;
                            }
                            word_value = Numbers.DigitalRoot(word_value);
                        }
                        break;
                    case CalculationMode.SumOfVerseValueDigitSums:
                        {
                            if (word.Verse.Words.Count == 1)
                            {
                                long l_sign = +1L;
                                foreach (Letter letter in word.Letters)
                                {
                                    long letter_value = l_sign * CalculateValue(letter, logging);
                                    if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                    if (logging) Log_Value += letter_value;
                                    // adjust letter value
                                    letter_value += l_sign * AdjustValue(letter, logging);
                                    l_sign *= AlternateLetterValues ? -1L : +1L;
                                    word_value += letter_value;
                                }
                                word_value = Numbers.DigitalRoot(word_value);
                            }
                        }
                        break;
                    case CalculationMode.SumOfVerseValueDigitalRoots:
                        {
                            if (word.Verse.Words.Count == 1)
                            {
                                long l_sign = +1L;
                                foreach (Letter letter in word.Letters)
                                {
                                    long letter_value = l_sign * CalculateValue(letter, logging);
                                    if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                    if (logging) Log_Value += letter_value;
                                    // adjust letter value
                                    letter_value += l_sign * AdjustValue(letter, logging);
                                    l_sign *= AlternateLetterValues ? -1L : +1L;
                                    word_value += letter_value;
                                }
                                word_value = Numbers.DigitalRoot(word_value);
                            }
                        }
                        break;
                    case CalculationMode.SumOfUniqueLetterValues:
                        {
                            string text = word.Text;
                            text = text.Simplify(s_numerical_system.TextMode);
                            text = text.RemovePunctuations();
                            text = text.RemoveDuplicates();

                            long l_sign = +1L;
                            foreach (char c in text)
                            {
                                long letter_value = l_sign * s_numerical_system.CalculateValue(c);
                                if (logging) Log.Append(c.ToString() + "\t" + letter_value);
                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                word_value += letter_value;
                            }
                        }
                        break;
                    default:
                        {
                            // do nothing
                        }
                        break;
                }
            }
        }

        if (logging) Log_Total = word_value;

        word.Value = word_value; // update value for CompareBy.Value

        return word_value;
    }
    public static long CalculateValue(List<Word> words, bool logging)
    {
        if (words == null)
            return 0L;
        if (words.Count == 0)
            return 0L;

        long result = 0L;

        long w_sign = +1L;
        long v_sign = +1L;
        long verse_value = 0L;
        foreach (Word word in words)
        {
            long word_value = w_sign * CalculateValue(word, logging);
            if (logging) Log.Append("\t" + "\t" + word_value);
            // adjust word value
            word_value += w_sign * AdjustValue(word, logging);
            w_sign *= AlternateWordValues ? -1L : +1L;

            result += word_value;
            verse_value += word_value;
            if (word.NumberInVerse == word.Verse.Words.Count)
            {
                if (logging) Log.Append("\t" + "\t" + "\t" + "\t" + verse_value);
                // adjust verse value
                result += v_sign * AdjustValue(word.Verse, logging);
                v_sign *= AlternateVerseValues ? -1L : +1L;

                verse_value = 0L;
            }
        }

        if (logging) Log_Total = result;

        return result;
    }
    public static long CalculateValue(Sentence sentence, bool logging)
    {
        if (sentence == null)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            if (s_numerical_system.LetterValue.StartsWith("Base"))
            {
                string radix_str = "";
                int pos = 4;
                while (Char.IsDigit(s_numerical_system.LetterValue[pos]))
                {
                    radix_str += s_numerical_system.LetterValue[pos];
                    pos++;
                }
                int radix;
                if (int.TryParse(radix_str, out radix))
                {
                    StringBuilder str = new StringBuilder();

                    List<Word> complete_words = s_book.GetCompleteWords(sentence);
                    if (complete_words != null)
                    {
                        foreach (Word word in complete_words)
                        {
                            foreach (Letter letter in word.Letters)
                            {
                                str.Insert(0, s_numerical_system.CalculateValue(letter.Character));
                            }
                            long value = Radix.Decode(str.ToString(), radix);
                            if (logging) Log.Append("\t" + "\t" + value);

                            result += value;

                            str.Length = 0;
                        }
                    }
                }
            }
            else
            {
                List<Word> complete_words = s_book.GetCompleteWords(sentence);
                if (complete_words != null)
                {
                    result = CalculateValue(complete_words, logging);
                }
            }
        }

        if (logging) Log_Total = result;

        return result;
    }
    public static long CalculateValue(Verse verse, bool logging)
    {
        if (verse == null)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            if (s_numerical_system.LetterValue.StartsWith("Base"))
            {
                string radix_str = "";
                int pos = 4;
                while (Char.IsDigit(s_numerical_system.LetterValue[pos]))
                {
                    radix_str += s_numerical_system.LetterValue[pos];
                    pos++;
                }
                int radix;
                if (int.TryParse(radix_str, out radix))
                {
                    StringBuilder str = new StringBuilder();
                    foreach (Word word in verse.Words)
                    {
                        foreach (Letter letter in word.Letters)
                        {
                            str.Insert(0, s_numerical_system.CalculateValue(letter.Character));
                        }
                        long value = Radix.Decode(str.ToString(), radix);
                        if (logging) Log.Append("\t" + "\t" + value);

                        result += value;

                        str.Length = 0;
                    }
                }
            }
            else
            {

                if (CalculationMode == CalculationMode.SumOfUniqueLetterValues)
                {
                    result = CalculateValue(verse.Text, logging);
                }
                else
                {
                    result = CalculateValue(verse.Words, logging);
                    if (CalculationMode == CalculationMode.SumOfVerseValueDigitSums)
                    {
                        result = Numbers.DigitSum(result);
                    }
                    else if (CalculationMode == CalculationMode.SumOfVerseValueDigitalRoots)
                    {
                        result = Numbers.DigitalRoot(result);
                    }
                }
            }
        }

        if (logging) Log_Total = result;

        verse.Value = result; // update value for CompareBy.Value

        return result;
    }
    public static long CalculateValue(List<Verse> verses, bool logging)
    {
        if (verses == null)
            return 0L;
        if (verses.Count == 0)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            switch (CalculationMode)
            {
                case CalculationMode.SumOfLetterValues:
                case CalculationMode.SumOfLetterValueDigitSums:
                case CalculationMode.SumOfLetterValueDigitalRoots:
                    {
                        long c_sign = +1L;
                        long chapter_value = 0L;

                        long v_sign = +1L;
                        foreach (Verse verse in verses)
                        {
                            if (verse != null)
                            {
                                long verse_value = 0L;

                                long w_sign = +1L;
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        long word_value = 0L;

                                        long l_sign = +1L;
                                        foreach (Letter letter in word.Letters)
                                        {
                                            if (letter != null)
                                            {
                                                long letter_value = s_numerical_system.CalculateValue(letter.Character);
                                                if (CalculationMode == CalculationMode.SumOfLetterValues)
                                                {
                                                    letter_value = l_sign * letter_value;
                                                }
                                                else if (CalculationMode == CalculationMode.SumOfLetterValueDigitSums)
                                                {
                                                    letter_value = l_sign * Numbers.DigitSum(letter_value);
                                                }
                                                else if (CalculationMode == CalculationMode.SumOfLetterValueDigitalRoots)
                                                {
                                                    letter_value = l_sign * Numbers.DigitalRoot(letter_value);
                                                }
                                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                                if (logging) Log_Value += letter_value;
                                                // adjust letter value
                                                letter_value += l_sign * AdjustValue(letter, logging);
                                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                                word_value += letter_value;
                                            }
                                        }
                                        word_value = w_sign * word_value;
                                        if (logging) Log.Append("\t" + "\t" + word_value);
                                        // adjust word value
                                        word_value += w_sign * AdjustValue(word, logging);
                                        w_sign *= AlternateWordValues ? -1L : +1L;
                                        verse_value += word_value;
                                    }
                                }

                                verse_value = c_sign * v_sign * verse_value;
                                if (logging) Log.Append("\t" + "\t" + "\t" + verse_value);
                                // adjust verse value
                                verse_value += v_sign * AdjustValue(verse, logging);
                                v_sign *= AlternateVerseValues ? -1L : +1L;

                                result += verse_value;
                                chapter_value += verse_value;
                                if (verse.NumberInChapter == verse.Chapter.Verses.Count)
                                {
                                    if (logging) Log.Append("\t" + "\t" + "\t" + "\t" + chapter_value);
                                    // adjust chapter value
                                    result += c_sign * AdjustValue(verse.Chapter, logging);
                                    c_sign *= AlternateChapterValues ? -1L : +1L;

                                    chapter_value = 0L;
                                }
                            }
                        }
                    }
                    break;
                case CalculationMode.SumOfWordValueDigitSums:
                case CalculationMode.SumOfWordValueDigitalRoots:
                    {
                        long c_sign = +1L;
                        long chapter_value = 0L;

                        long v_sign = +1L;
                        foreach (Verse verse in verses)
                        {
                            if (verse != null)
                            {
                                long verse_value = 0L;

                                long w_sign = +1L;
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        long word_value = 0L;

                                        long l_sign = +1L;
                                        foreach (Letter letter in word.Letters)
                                        {
                                            if (letter != null)
                                            {
                                                long letter_value = s_numerical_system.CalculateValue(letter.Character);
                                                letter_value = l_sign * letter_value;
                                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                                if (logging) Log_Value += letter_value;
                                                // adjust letter value
                                                letter_value += l_sign * AdjustValue(letter, logging);
                                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                                word_value += letter_value;
                                            }
                                        }

                                        if (CalculationMode == CalculationMode.SumOfWordValueDigitSums)
                                        {
                                            word_value = Numbers.DigitSum(word_value);
                                        }
                                        else if (CalculationMode == CalculationMode.SumOfWordValueDigitalRoots)
                                        {
                                            word_value = Numbers.DigitalRoot(word_value);
                                        }

                                        word_value = w_sign * word_value;
                                        if (logging) Log.Append("\t" + "\t" + word_value);
                                        // adjust word value
                                        word_value += w_sign * AdjustValue(word, logging);
                                        w_sign *= AlternateWordValues ? -1L : +1L;
                                        verse_value += word_value;
                                    }
                                }

                                verse_value = c_sign * v_sign * verse_value;
                                if (logging) Log.Append("\t" + "\t" + "\t" + verse_value);
                                // adjust verse value
                                verse_value += v_sign * AdjustValue(verse, logging);
                                v_sign *= AlternateVerseValues ? -1L : +1L;

                                result += verse_value;
                                chapter_value += verse_value;
                                if (verse.NumberInChapter == verse.Chapter.Verses.Count)
                                {
                                    if (logging) Log.Append("\t" + "\t" + "\t" + "\t" + chapter_value);
                                    // adjust chapter value
                                    result += c_sign * AdjustValue(verse.Chapter, logging);
                                    c_sign *= AlternateChapterValues ? -1L : +1L;

                                    chapter_value = 0L;
                                }
                            }
                        }
                    }
                    break;
                case CalculationMode.SumOfVerseValueDigitSums:
                case CalculationMode.SumOfVerseValueDigitalRoots:
                    {
                        long c_sign = +1L;
                        long chapter_value = 0L;

                        long v_sign = +1L;
                        foreach (Verse verse in verses)
                        {
                            if (verse != null)
                            {
                                long verse_value = 0L;

                                long w_sign = +1L;
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        long word_value = 0L;

                                        long l_sign = +1L;
                                        foreach (Letter letter in word.Letters)
                                        {
                                            if (letter != null)
                                            {
                                                long letter_value = s_numerical_system.CalculateValue(letter.Character);
                                                letter_value = l_sign * letter_value;
                                                if (logging) Log.Append(letter.Text + "\t" + letter_value);
                                                if (logging) Log_Value += letter_value;
                                                // adjust letter value
                                                letter_value += l_sign * AdjustValue(letter, logging);
                                                l_sign *= AlternateLetterValues ? -1L : +1L;
                                                word_value += letter_value;
                                            }
                                        }

                                        word_value = w_sign * word_value;
                                        if (logging) Log.Append("\t" + "\t" + word_value);
                                        // adjust word value
                                        word_value += w_sign * AdjustValue(word, logging);
                                        w_sign *= AlternateWordValues ? -1L : +1L;
                                        verse_value += word_value;
                                    }
                                }

                                if (CalculationMode == CalculationMode.SumOfVerseValueDigitSums)
                                {
                                    verse_value = Numbers.DigitSum(verse_value);
                                }
                                else if (CalculationMode == CalculationMode.SumOfVerseValueDigitalRoots)
                                {
                                    verse_value = Numbers.DigitalRoot(verse_value);
                                }

                                verse_value = c_sign * v_sign * verse_value;
                                if (logging) Log.Append("\t" + "\t" + "\t" + verse_value);
                                // adjust verse value
                                verse_value += AdjustValue(verse, logging);
                                v_sign *= AlternateVerseValues ? -1L : +1L;

                                result += verse_value;
                                chapter_value += verse_value;
                                if (verse.NumberInChapter == verse.Chapter.Verses.Count)
                                {
                                    if (logging) Log.Append("\t" + "\t" + "\t" + "\t" + chapter_value);
                                    // adjust chapter value
                                    result += c_sign * AdjustValue(verse.Chapter, logging);
                                    c_sign *= AlternateChapterValues ? -1L : +1L;

                                    chapter_value = 0L;
                                }
                            }
                        }
                    }
                    break;
                case CalculationMode.SumOfUniqueLetterValues:
                    {
                        StringBuilder str = new StringBuilder();
                        foreach (Verse verse in verses)
                        {
                            if (verse != null)
                            {
                                str.Append(verse.Text.Simplify(s_numerical_system.TextMode));
                            }
                        }
                        string text = str.ToString();
                        text = text.Replace(" ", "");
                        text = text.RemovePunctuations();
                        text = text.RemoveDuplicates();

                        long l_sign = +1L;
                        foreach (char c in text)
                        {
                            long letter_value = l_sign * s_numerical_system.CalculateValue(c);
                            l_sign *= AlternateLetterValues ? -1L : +1L;
                            if (logging) Log.AppendLine(c.ToString() + "\t" + letter_value);
                            if (logging) Log_Value += letter_value;
                            result += letter_value;
                        }
                    }
                    break;
                default:
                    {
                        // do nothing
                    }
                    break;
            }
        }

        if (logging) Log_Total = result;

        return result;
    }
    public static long CalculateValue(Chapter chapter, bool logging)
    {
        if (chapter == null)
            return 0L;

        long result = CalculateValue(chapter.Verses, logging);

        if (logging) Log_Total = result;

        chapter.Value = result; // update value for CompareBy.Value

        return result;
    }
    public static long CalculateValue(List<Chapter> chapters, bool logging)
    {
        if (chapters == null)
            return 0L;
        if (chapters.Count == 0)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            foreach (Chapter chapter in chapters)
            {
                result += CalculateValue(chapter.Verses, logging);
            }
        }

        if (logging) Log_Total = result;

        return result;
    }
    public static long CalculateValue(Book book, bool logging)
    {
        if (book == null)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            result = CalculateValue(book.Chapters, logging);
        }

        if (logging) Log_Total = result;

        return result;
    }
    public static long CalculateValue(List<Verse> verses, Letter start_letter, Letter end_letter, bool logging)
    {
        if (verses == null)
            return 0L;
        if (verses.Count == 0)
            return 0L;
        if (start_letter == null)
            return 0L;
        if (end_letter == null)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            if (verses.Count == 1)
            {
                result += CalculateValue(verses[0], start_letter, end_letter, logging);
            }
            else if (verses.Count == 2)
            {
                Word first_verse_end_word = verses[0].Words[verses[0].Words.Count - 1];
                if (first_verse_end_word != null)
                {
                    if (first_verse_end_word.Letters.Count > 0)
                    {
                        Letter first_verse_end_letter = first_verse_end_word.Letters[first_verse_end_word.Letters.Count - 1];
                        if (first_verse_end_letter != null)
                        {
                            result += CalculateValue(verses[0], start_letter, first_verse_end_letter, logging);
                        }
                    }
                }

                Word last_verse_start_word = verses[1].Words[0];
                if (last_verse_start_word != null)
                {
                    if (last_verse_start_word.Letters.Count > 0)
                    {
                        Letter last_verse_start_letter = last_verse_start_word.Letters[0];
                        if (last_verse_start_letter != null)
                        {
                            result += CalculateValue(verses[1], last_verse_start_letter, end_letter, logging);
                        }
                    }
                }
            }
            else //if (verses.Count > 2)
            {
                bool first_verse_is_fully_selected = (start_letter.NumberInChapter == 1);
                bool last_verse_is_fully_selected = (end_letter.NumberInChapter == end_letter.Word.Verse.Chapter.Letters.Count);
                Chapter first_chapter = start_letter.Word.Verse.Chapter;
                Chapter last_chapter = end_letter.Word.Verse.Chapter;

                // first verse
                Word first_verse_end_word = verses[0].Words[verses[0].Words.Count - 1];
                if (first_verse_end_word != null)
                {
                    if (first_verse_end_word.Letters.Count > 0)
                    {
                        Letter first_verse_end_letter = first_verse_end_word.Letters[first_verse_end_word.Letters.Count - 1];
                        if (first_verse_end_letter != null)
                        {
                            result += CalculateValue(verses[0], start_letter, first_verse_end_letter, logging);
                        }
                    }
                }

                // middle verses
                long c_sign = +1L;
                for (int i = 1; i < verses.Count - 1; i++)
                {
                    result += CalculateValue(verses[i], logging);

                    Verse verse = verses[i];
                    if (verse != null)
                    {
                        Chapter chapter = verse.Chapter;
                        if (chapter != null)
                        {
                            if (verse.NumberInChapter == verse.Chapter.Verses.Count)    // last verse in chapter
                            {
                                if (
                                     (verse.Chapter != first_chapter)
                                     ||
                                     ((verse.Chapter == first_chapter) && first_verse_is_fully_selected)
                                   )
                                {
                                    List<Chapter> chapters = s_book.GetCompleteChapters(verses);
                                    if (chapters != null)
                                    {
                                        if (chapters.Contains(chapter))
                                        {
                                            long value = CalculateValue(chapter, logging);
                                            if (logging) Log.Append("\t" + "\t" + "\t" + "\t" + value);
                                            result += c_sign * AdjustValue(chapter, logging);
                                            c_sign *= AlternateChapterValues ? -1L : +1L;
                                        }
                                    }
                                }
                                else
                                {
                                    if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + "---");
                                }
                            }
                        }
                    }
                }

                // last verse
                Word last_verse_start_word = verses[verses.Count - 1].Words[0];
                if (last_verse_start_word != null)
                {
                    if (last_verse_start_word.Letters.Count > 0)
                    {
                        Letter last_verse_start_letter = last_verse_start_word.Letters[0];
                        if (last_verse_start_letter != null)
                        {
                            result += CalculateValue(verses[verses.Count - 1], last_verse_start_letter, end_letter, logging);
                        }
                    }
                }

                Verse last_verse = verses[verses.Count - 1];
                if (last_verse != null)
                {
                    if (
                         ((last_chapter == first_chapter) && first_verse_is_fully_selected && last_verse_is_fully_selected)
                         ||
                         ((last_chapter != first_chapter) && (last_verse.Chapter == last_chapter) && last_verse_is_fully_selected)
                       )
                    {
                        Chapter chapter = last_verse.Chapter;
                        if (chapter != null)
                        {
                            if (last_verse.NumberInChapter == last_verse.Chapter.Verses.Count)    // last verse in chapter
                            {
                                List<Chapter> complete_chapters = s_book.GetCompleteChapters(verses);
                                if (complete_chapters != null)
                                {
                                    if (complete_chapters.Contains(chapter))
                                    {
                                        long value = CalculateValue(chapter, logging);
                                        if (logging) Log.Append("\t" + "\t" + "\t" + "\t" + value);
                                        result += c_sign * AdjustValue(chapter, logging);
                                        c_sign *= AlternateChapterValues ? -1L : +1L;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (logging) Log.AppendLine("\t" + "\t" + "\t" + "\t" + "---");
                    }
                }
            }
        }

        if (logging) Log_Total = result;

        return result;
    }
    private static long CalculateValue(Verse verse, Letter from_letter, Letter to_letter, bool logging)
    {
        if (verse == null)
            return 0L;
        if (from_letter == null)
            return 0L;
        if (to_letter == null)
            return 0L;

        long result = 0L;

        if (s_numerical_system != null)
        {
            if (s_numerical_system.LetterValue.StartsWith("Base"))
            {
                string radix_str = "";
                int pos = 4;
                while (Char.IsDigit(s_numerical_system.LetterValue[pos]))
                {
                    radix_str += s_numerical_system.LetterValue[pos];
                    pos++;
                }
                int radix;
                if (int.TryParse(radix_str, out radix))
                {
                    StringBuilder str = new StringBuilder();

                    int word_index = -1;   // in verse
                    int letter_index = -1; // in verse
                    bool done = false;
                    foreach (Word word in verse.Words)
                    {
                        word_index++;

                        if ((word.Letters != null) && (word.Letters.Count > 0))
                        {
                            foreach (Letter letter in word.Letters)
                            {
                                letter_index++;

                                if (letter_index < from_letter.NumberInVerse - 1)
                                    continue;
                                if (letter_index > to_letter.NumberInVerse - 1)
                                {
                                    done = true;
                                    break;
                                }

                                str.Insert(0, s_numerical_system.CalculateValue(letter.Character));
                            }
                        }
                        long value = Radix.Decode(str.ToString(), radix);
                        if (logging) Log.Append("\t" + "\t" + value);

                        result += value;

                        str.Length = 0;

                        if (done)
                            break;
                    }
                }
            }
            else
            {
                long w_sign = +1L;
                int word_index = -1;   // in verse
                int letter_index = -1; // in verse
                bool done = false;
                foreach (Word word in verse.Words)
                {
                    word_index++;

                    if ((word.Letters != null) && (word.Letters.Count > 0))
                    {
                        long l_sign = +1L;
                        long selected_l_sign = +1L;
                        List<Letter> selected_letters = new List<Letter>();
                        foreach (Letter letter in word.Letters)
                        {
                            letter_index++;

                            if (letter_index < from_letter.NumberInVerse - 1) continue;
                            if (letter_index > to_letter.NumberInVerse - 1)
                            {
                                selected_l_sign = +1L;
                                long selected_value = 0L;
                                foreach (Letter selected_letter in selected_letters)
                                {
                                    //////////////////////////////////////////////////////////////////////////////////////
                                    selected_value += selected_l_sign * s_numerical_system.CalculateValue(selected_letter.Character);
                                    selected_l_sign *= AlternateLetterValues ? -1L : +1L;
                                    //////////////////////////////////////////////////////////////////////////////////////
                                }
                                if (logging) Log.Append("\t" + "\t" + selected_value);

                                done = true;
                                break;
                            }

                            // valid letter
                            selected_letters.Add(letter);

                            long letter_value = w_sign * l_sign * CalculateValue(letter, logging);
                            if (logging) Log.Append(letter.Text + "\t" + letter_value);
                            if (logging) Log_Value += letter_value;
                            // adjust letter value
                            letter_value += w_sign * l_sign * AdjustValue(letter, logging);
                            l_sign *= AlternateLetterValues ? -1L : +1L;
                            result += letter_value;
                        }

                        List<Word> complete_words = s_book.GetCompleteWords(selected_letters);
                        if (complete_words != null)
                        {
                            if (complete_words.Contains(word))
                            {
                                l_sign = +1L;
                                long word_value = 0L;
                                foreach (Letter letter in word.Letters)
                                {
                                    word_value += w_sign * l_sign * s_numerical_system.CalculateValue(letter.Character);
                                    l_sign *= AlternateLetterValues ? -1L : +1L;
                                }
                                if (logging) Log.Append("\t" + "\t" + word_value);
                                // adjust word value
                                result += w_sign * AdjustValue(word, logging);
                            }
                        }

                        w_sign *= AlternateWordValues ? -1L : +1L;
                    }

                    if (done)
                        break;
                }
            }

            if ((from_letter.NumberInVerse == 1) && (to_letter.NumberInVerse == verse.Letters.Count))
            {
                /////////////////////////////////////////////////////////////////////////////////
                //long value = CalculateValue(verse, logging);
                //if (logging) Log.Append("\t" + "\t" + "\t" + value);
                /////////////////////////////////////////////////////////////////////////////////

                // adjust verse value
                result += AdjustValue(verse, logging);
            }
            else
            {
                if (logging) Log.AppendLine("\t" + "\t" + "\t" + "---");
            }
        }

        return result;
    }


    // helper methods for finds
    private static List<Verse> GetVerses(List<Word> words)
    {
        List<Verse> result = new List<Verse>();

        if (words != null)
        {
            foreach (Word word in words)
            {
                if (word != null)
                {
                    if (!result.Contains(word.Verse))
                    {
                        result.Add(word.Verse);
                    }
                }
            }
        }

        return result;
    }
    private static List<Verse> GetVerses(List<Phrase> phrases)
    {
        List<Verse> result = new List<Verse>();

        if (phrases != null)
        {
            foreach (Phrase phrase in phrases)
            {
                if (phrase != null)
                {
                    if (!result.Contains(phrase.Verse))
                    {
                        result.Add(phrase.Verse);
                    }
                }
            }
        }

        return result;
    }
    private static List<Chapter> GetSourceChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses)
    {
        List<Chapter> result = new List<Chapter>();

        if (s_book != null)
        {
            if (search_scope == SearchScope.Book)
            {
                result = s_book.Chapters;
            }
            else if (search_scope == SearchScope.Selection)
            {
                result = current_selection.Chapters;
            }
            else if (search_scope == SearchScope.Result)
            {
                if (previous_verses != null)
                {
                    result = s_book.GetChapters(previous_verses);
                }
            }
        }

        return result;
    }
    private static List<Chapter> GetChapters(List<Phrase> phrases)
    {
        List<Chapter> result = new List<Chapter>();

        if (phrases != null)
        {
            foreach (Phrase phrase in phrases)
            {
                if (phrase != null)
                {
                    if (!result.Contains(phrase.Verse.Chapter))
                    {
                        result.Add(phrase.Verse.Chapter);
                    }
                }
            }
        }

        return result;
    }
    private static List<Phrase> BuildPhrases(Verse verse, MatchCollection matches, bool with_diacritics)
    {
        List<Phrase> result = new List<Phrase>();

        foreach (Match match in matches)
        {
            foreach (Capture capture in match.Captures)
            {
                string text = capture.Value;
                int position = capture.Index;
                if (s_numerical_system != null)
                {
                    if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                    {
                        if (with_diacritics)
                        {
                            result.Add(new Phrase(verse, position, text));
                        }
                        else
                        {
                            result.Add(OriginifyPhrase(new Phrase(verse, position, text)));
                        }
                    }
                    else
                    {
                        result.Add(new Phrase(verse, position, text));
                    }
                }
            }
        }

        return result;
    }
    private static Phrase OriginifyPhrase(Phrase phrase)
    {
        if (phrase != null)
        {
            Verse verse = phrase.Verse;
            if (verse != null)
            {
                string text = phrase.Text;
                int position = phrase.Position;

                int start = 0;
                for (int i = 0; i < verse.Text.Length; i++)
                {
                    char character = verse.Text[i];

                    if ((character == ' ') || (Constants.ARABIC_LETTERS.Contains(character)))
                    {
                        start++; // real letter in all text_modes
                    }
                    else if (Constants.STOPMARKS.Contains(character))
                    {
                        // superscript Seen letter in words وَيَبْصُۜطُ and بَصْۜطَةًۭ are not stopmarks
                        // Quran 2:245  مَّن ذَا ٱلَّذِى يُقْرِضُ ٱللَّهَ قَرْضًا حَسَنًۭا فَيُضَٰعِفَهُۥ لَهُۥٓ أَضْعَافًۭا كَثِيرَةًۭ ۚ وَٱللَّهُ يَقْبِضُ وَيَبْصُۜطُ وَإِلَيْهِ تُرْجَعُونَ
                        // Quran 7:69  أَوَعَجِبْتُمْ أَن جَآءَكُمْ ذِكْرٌۭ مِّن رَّبِّكُمْ عَلَىٰ رَجُلٍۢ مِّنكُمْ لِيُنذِرَكُمْ ۚ وَٱذْكُرُوٓا۟ إِذْ جَعَلَكُمْ خُلَفَآءَ مِنۢ بَعْدِ قَوْمِ نُوحٍۢ وَزَادَكُمْ فِى ٱلْخَلْقِ بَصْۜطَةًۭ ۖ فَٱذْكُرُوٓا۟ ءَالَآءَ ٱللَّهِ لَعَلَّكُمْ تُفْلِحُونَ
                        if
                            (
                                (character == 'ۜ') &&  // superscript Seen
                                (
                                    ((verse.Chapter.Number == 2) && (verse.NumberInChapter == 245))
                                    ||
                                    ((verse.Chapter.Number == 7) && (verse.NumberInChapter == 69))
                                )
                            )
                        {
                            // not a stopmark but a Seen above Ssad so treat as part in its word
                        }
                        else
                        {
                            start--; // skip the space after stopmark
                        }
                    }
                    else if (Constants.QURANMARKS.Contains(character))
                    {
                        // already the space after stopmark was skipped
                    }
                    else
                    {
                        // treat character as part of its word
                    }

                    // i has reached phrase start
                    if (start > position)
                    {
                        int phrase_length = text.Length;
                        StringBuilder str = new StringBuilder();

                        int length = 0;
                        for (int j = i; j < verse.Text.Length; j++)
                        {
                            character = verse.Text[j];
                            str.Append(character);

                            if ((character == ' ') || (Constants.ARABIC_LETTERS.Contains(character)))
                            {
                                length++;
                            }
                            else if ((Constants.STOPMARKS.Contains(character)) || (Constants.QURANMARKS.Contains(character)))
                            {
                                length--; // ignore space after stopmark
                                if (length < 0)
                                {
                                    length = 0;
                                }
                            }

                            // j has reached phrase end
                            if (length == phrase_length)
                            {
                                return new Phrase(verse, i, str.ToString());
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
    //TODO Doesn't work with FindByNumbers "L" in Original text_mode
    public static Phrase SimplifyPhrase(Phrase phrase, string to_text_mode)
    {
        if (phrase != null)
        {
            Verse phrase_verse = phrase.Verse;
            int phrase_position = phrase.Position;
            string phrase_text = phrase.Text;
            if (phrase_text != null)
            {
                int phrase_length = phrase_text.Length;

                if (phrase_verse != null)
                {
                    if ((to_text_mode == "Original") || (to_text_mode == "SimplifiedMarks"))
                    {
                        Verse original_verse = phrase_verse;
                        int letter_count = 0;
                        int position = 0;
                        foreach (char c in original_verse.Text)
                        {
                            position++;
                            if ((c == ' ') || (Constants.ARABIC_LETTERS.Contains(c)))
                            {
                                letter_count++;
                            }

                            if (letter_count == phrase_position)
                            {
                                break;
                            }
                        }

                        letter_count = 0;
                        StringBuilder str = new StringBuilder();
                        for (int i = position; i < phrase_verse.Text.Length; i++)
                        {
                            char character = phrase_verse.Text[i];
                            str.Append(character);

                            if ((character == ' ') || (Constants.ARABIC_LETTERS.Contains(character)))
                            {
                                letter_count++;
                            }
                            else if (Constants.STOPMARKS.Contains(character))
                            {
                                letter_count--; // decrement space after stopmark as it will be incremented above
                                if (letter_count < 0)
                                {
                                    letter_count = 0;
                                }
                            }
                            else if (Constants.QURANMARKS.Contains(character))
                            {
                                letter_count--; // decrement space after quranmark as it will be incremented above
                            }

                            // check if finished
                            if (letter_count == phrase_length)
                            {
                                // skip any non-letter at start
                                int index = position;
                                if ((index > 0) && (index < phrase_verse.Text.Length))
                                {
                                    character = phrase_verse.Text[index];
                                    if (!Constants.ARABIC_LETTERS.Contains(character))
                                    {
                                        position++;
                                        str.Append(" "); // increment length
                                    }
                                }

                                // skip any non-letter at end
                                index = position + str.Length - 1;
                                if ((index > 0) && (position + str.Length < phrase_verse.Text.Length))
                                {
                                    character = phrase_verse.Text[index];
                                    if (!Constants.ARABIC_LETTERS.Contains(character))
                                    {
                                        str.Append(" "); // increment length
                                    }
                                }

                                return new Phrase(phrase_verse, position, str.ToString());
                            }
                        }
                    }
                    else // if ((to_text_mode != "Original") && (to_text_mode != "SimplifiedMarks"))
                    {
                        // simplify text
                        phrase_text = phrase_text.Simplify(to_text_mode);
                        phrase_text = phrase_text.Trim();
                        if (!String.IsNullOrEmpty(phrase_text)) // re-test in case text was just harakat which is simplifed to nothing
                        {
                            // simplify phrase
                            string verse_text = phrase_verse.Text.Simplify(to_text_mode);
                            phrase_position = verse_text.IndexOf(phrase_text);  //????? will ONLY build first phrase occurrence in verse

                            // build simplified phrase
                            return new Phrase(phrase_verse, phrase_position, phrase_text);
                        }
                    }
                }
            }
        }
        return null;
    }
    public static List<Verse> GetSourceVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, TextLocationInChapter text_location_in_chapter)
    {
        List<Verse> result = new List<Verse>();

        List<Verse> verses = new List<Verse>();
        if (s_book != null)
        {
            if (search_scope == SearchScope.Book)
            {
                verses = s_book.Verses;
            }
            else if (search_scope == SearchScope.Selection)
            {
                verses = current_selection.Verses;
            }
            else if (search_scope == SearchScope.Result)
            {
                if (previous_verses != null)
                {
                    verses = new List<Verse>(previous_verses);
                }
            }
        }

        switch (text_location_in_chapter)
        {
            case TextLocationInChapter.AtStart:
                {
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            if (verse.NumberInChapter == 1)
                            {
                                result.Add(verse);
                            }
                        }
                    }
                }
                break;
            case TextLocationInChapter.AtMiddle:
                {
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            if ((verse.NumberInChapter != 1) && (verse.NumberInChapter != verse.Chapter.Verses.Count))
                            {
                                result.Add(verse);
                            }
                        }
                    }
                }
                break;
            case TextLocationInChapter.AtEnd:
                {
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            if (verse.NumberInChapter == verse.Chapter.Verses.Count)
                            {
                                result.Add(verse);
                            }
                        }
                    }
                }
                break;
            case TextLocationInChapter.Any:
            default:
                {
                    result = verses;
                }
                break;
        }

        return result;
    }

    // find by text - Exact
    private static string BuildPattern(string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness)
    {
        if ((text_location_in_verse == TextLocationInVerse.Any) && (text_location_in_word == TextLocationInWord.Any))
        {
            return BuildPatternByText(text, text_location_in_verse, text_location_in_word, text_wordness);
        }
        else
        {
            return BuildPatternByWords(text, text_location_in_verse, text_location_in_word, text_wordness);
        }
    }
    private static string BuildPatternByText(string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness)
    {
        string pattern = null;

        if (String.IsNullOrEmpty(text))
            return text;

        // don't Trim(), allow space after/before
        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

        // search for Quran markers, stopmarks, numbers, etc.
        if (text.Length == 1)
        {
            if (!Constants.ARABIC_LETTERS.Contains(text[0]))
            {
                return text;
            }
        }

        /*
        =====================================================================
        Regular Expressions
        =====================================================================
        Best Reference: https://regular-expressions.info/
        RegEx Utility:  https://sourceforge.net/projects/regulator/
        RegEx Tester:   https://regexstorm.net/tester
        =====================================================================
        Matches	Characters
        x	character x
        \\	backslash character
        \0n	character with octal value 0n (0 <= n <= 7)
        \0nn	character with octal value 0nn (0 <= n <= 7)
        \0mnn	character with octal value 0mnn (0 <= m <= 3, 0 <= n <= 7)
        \xhh	character with hexadecimal value 0xhh
        \uhhhh	character with hexadecimal value 0xhhhh
        \t	tab character ('\u0009')
        \n	newline (line feed) character ('\u000A')
        \r	carriage-return character ('\u000D')
        \f	form-feed character ('\u000C')
        \a	alert (bell) character ('\u0007')
        \e	escape character ('\u001B')
        \cx	control character corresponding to x

        Character Classes
        [abc]		    a, b, or c				                    (simple class)
        [^abc]		    any character except a, b, or c		        (negation)
        [a-zA-Z]	    a through z or A through Z, inclusive	    (range)
        [a-d[m-p]]	    a through d, or m through p: [a-dm-p]	    (union)
        [a-z&&[def]]	d, e, or f				                    (intersection)
        [a-z&&[^bc]]	a through z, except for b and c: [ad-z]	    (subtraction)
        [a-z&&[^m-p]]	a through z, and not m through p: [a-lq-z]  (subtraction)

        Predefined
        .	any character (inc line terminators) except newline
        \d	digit				            [0-9]
        \D	non-digit			            [^0-9]
        \s	whitespace character		    [ \t\n\x0B\f\r]
        \S	non-whitespace character	    [^\s]
        \w	word character (alphanumeric)	[a-zA-Z_0-9]
        \W	non-word character		        [^\w]

        Boundary Matchers
        ^	beginning of a line	(in Multiline)
        $	end of a line  		(in Multiline)
        \b	word boundary, including line start and line end
        \B	non-word boundary
        \A	beginning of the input
        \G	end of the previous match
        \Z	end of the input but for the final terminator, if any
        \z	end of the input

        Greedy quantifiers
        X?	     X 0 or 1    times
        X*	     X 0 or more times
        X+	     X 1 or more times
        X{n}	 X n         times
        X{n,}	 X n or more times
        X{n,m}	 X n to m    times

        Reluctant quantifiers
        X??	     X 0 or 1    times
        X*?	     X 0 or more times
        X+?	     X 1 or more times
        X{n}?	 X n         times
        X{n,}?	 X n or more times
        X{n,m}?	 X n to m    times

        Possessive quantifiers
        X?+	     X 0 or 1    times
        X*+	     X 0 or more times
        X++	     X 1 or more times
        X{n}+	 X n         times
        X{n,}+	 X n or more times
        X{n,m}+	 X n to m    times

        (?=text)  positive lookahead
        (?!text)  negative lookahead           // not at end of line  (?!$)
        (?<=text) positive lookbehind
        (?<!text) negative lookbehind          // not at start of line (?<!^)
        =====================================================================
        */

        string pattern_empty_line = @"(^$)";
        string pattern_whole_line = @"(^" + text + @"$)";
        string pattern_whole_word = @"(\b" + text + @"\b)";
        string pattern_prefix = @"(\b" + text + @"\B)";
        string pattern_midfix = @"(\B" + text + @"\B)";
        string pattern_suffix = @"(\B" + text + @"\b)";


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = ANYWHERE
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wanywhere = @"(" + pattern_whole_line + "|"
                                             + @"(" + @"^" + pattern_whole_word + @")" + "|"
                                             + @"(" + @"^" + pattern_prefix + @")" + "|"
                                             + @"(" + @"^" + pattern_midfix + @")" + "|"
                                             + @"(" + @"^" + pattern_suffix + @")"
                                             + @")";
        string pattern_anywordness_vmiddle_wanywhere = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_midfix + @"(?!$)" + @")" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")"
                                                     + @")";
        string pattern_anywordness_vend_wanywhere = @"(" + pattern_whole_line + "|"
                                                  + @"(" + pattern_whole_word + @"$" + @")" + "|"
                                                  + @"(" + pattern_prefix + @"$" + @")" + "|"
                                                  + @"(" + pattern_midfix + @"$" + @")" + "|"
                                                  + @"(" + pattern_suffix + @"$" + @")"
                                                  + @")";
        string pattern_anywordness_vanywhere_wanywhere = @"(" + pattern_anywordness_vstart_wanywhere + "|" + pattern_anywordness_vmiddle_wanywhere + "|" + pattern_anywordness_vend_wanywhere + @")";

        // Part of word
        string pattern_partword_vstart_wanywhere = @"("
                                          + @"(" + @"^" + pattern_prefix + @")" + "|"
                                          + @"(" + @"^" + pattern_midfix + @")" + "|"
                                          + @"(" + @"^" + pattern_suffix + @")"
                                          + @")";
        string pattern_partword_vmiddle_wanywhere = @"("
                                                  + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_midfix + @"(?!$)" + @")" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")"
                                                  + @")";
        string pattern_partword_vend_wanywhere = @"("
                                               + @"(" + pattern_prefix + @"$" + @")" + "|"
                                               + @"(" + pattern_midfix + @"$" + @")" + "|"
                                               + @"(" + pattern_suffix + @"$" + @")"
                                               + @")";
        string pattern_partword_vanywhere_wanywhere = @"(" + pattern_partword_vstart_wanywhere + "|" + pattern_partword_vmiddle_wanywhere + "|" + pattern_partword_vend_wanywhere + @")";

        // Whole word
        string pattern_wholeword_vstart_wanywhere = @"(" + pattern_whole_line + "|"
                                           + @"(" + @"^" + pattern_whole_word + @")"
                                           + @")";
        string pattern_wholeword_vmiddle_wanywhere = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                   + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")"
                                                   + @")";
        string pattern_wholeword_vend_wanywhere = @"(" + pattern_whole_line + "|"
                                                + @"(" + pattern_whole_word + @"$" + @")"
                                                + @")";
        string pattern_wholeword_vanywhere_wanywhere = @"(" + pattern_wholeword_vstart_wanywhere + "|" + pattern_wholeword_vmiddle_wanywhere + "|" + pattern_wholeword_vend_wanywhere + @")";
        ///////////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = START
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wstart = @"(" + pattern_whole_line + "|"
                                          + @"(" + @"^" + pattern_whole_word + @")" + "|"
                                          + @"(" + @"^" + pattern_prefix + @")"
                                          + @")";
        string pattern_anywordness_vmiddle_wstart = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")"
                                                  + @")";
        string pattern_anywordness_vend_wstart = @"(" + pattern_whole_line + "|"
                                               + @"(" + pattern_whole_word + @"$" + @")" + "|"
                                               + @"(" + pattern_prefix + @"$" + @")"
                                               + @")";
        string pattern_anywordness_vanywhere_wstart = @"(" + pattern_anywordness_vstart_wstart + "|" + pattern_anywordness_vmiddle_wstart + "|" + pattern_anywordness_vend_wstart + @")";

        // Part of word
        string pattern_partword_vstart_wstart = @"("
                                              + @"(" + @"^" + pattern_prefix + @")"
                                              + @")";
        string pattern_partword_vmiddle_wstart = @"("
                                               + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")"
                                               + @")";
        string pattern_partword_vend_wstart = @"("
                                            + @"(" + pattern_prefix + @"$" + @")"
                                            + @")";
        string pattern_partword_vanywhere_wstart = @"(" + pattern_partword_vstart_wstart + "|" + pattern_partword_vmiddle_wstart + "|" + pattern_partword_vend_wstart + @")";

        // Whole word
        string pattern_wholeword_vstart_wstart = @"(" + pattern_whole_line + "|"
                                               + @"(" + @"^" + pattern_whole_word + @")"
                                               + @")";
        string pattern_wholeword_vmiddle_wstart = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")"
                                                + @")";
        string pattern_wholeword_vend_wstart = @"(" + pattern_whole_line + "|"
                                             + @"(" + pattern_whole_word + @"$" + @")"
                                             + @")";
        string pattern_wholeword_vanywhere_wstart = @"(" + pattern_wholeword_vstart_wstart + "|" + pattern_wholeword_vmiddle_wstart + "|" + pattern_wholeword_vend_wstart + @")";
        ///////////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = MIDDLE
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wmiddle = @"(" + @"^" + pattern_midfix + @")";
        string pattern_anywordness_vmiddle_wmiddle = @"(" + @"(?<!^)" + pattern_midfix + @"(?!$)" + @")";
        string pattern_anywordness_vend_wmiddle = @"(" + pattern_midfix + @"$" + @")";
        string pattern_anywordness_vanywhere_wmiddle = @"(" + pattern_anywordness_vstart_wmiddle + "|" + pattern_anywordness_vmiddle_wmiddle + "|" + pattern_anywordness_vend_wmiddle + @")";

        // Part of word
        string pattern_partword_vstart_wmiddle = @"(" + pattern_midfix + @")";
        string pattern_partword_vmiddle_wmiddle = @"(" + @"(?<!^)" + pattern_midfix + @"(?!$)" + @")";
        string pattern_partword_vend_wmiddle = @"(" + pattern_midfix + @"$" + @")";
        string pattern_partword_vanywhere_wmiddle = @"(" + pattern_partword_vstart_wmiddle + "|" + pattern_partword_vmiddle_wmiddle + "|" + pattern_partword_vend_wmiddle + @")";

        // Whole word
        string pattern_wholeword_vstart_wmiddle = @"(" + "^Dummy text not to be found.$" + @")";
        string pattern_wholeword_vmiddle_wmiddle = @"(" + "^Dummy text not to be found.$" + @")";
        string pattern_wholeword_vend_wmiddle = @"(" + "^Dummy text not to be found.$" + @")";
        string pattern_wholeword_vanywhere_wmiddle = @"(" + pattern_wholeword_vstart_wmiddle + "|" + pattern_wholeword_vmiddle_wmiddle + "|" + pattern_wholeword_vend_wmiddle + @")";
        ///////////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = END
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wend = @"(" + pattern_whole_line + "|"
                                        + @"(" + @"^" + pattern_whole_word + @")" + "|"
                                        + @"(" + @"^" + pattern_suffix + @")"
                                        + @")";
        string pattern_anywordness_vmiddle_wend = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")" + "|"
                                                + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")"
                                                + @")";
        string pattern_anywordness_vend_wend = @"(" + pattern_whole_line + "|"
                                             + @"(" + pattern_whole_word + @"$" + @")" + "|"
                                             + @"(" + pattern_suffix + @"$" + @")"
                                             + @")";
        string pattern_anywordness_vanywhere_wend = @"(" + pattern_anywordness_vstart_wend + "|" + pattern_anywordness_vmiddle_wend + "|" + pattern_anywordness_vend_wend + @")";

        // Part of word
        string pattern_partword_vstart_wend = @"("
                                     + @"(" + @"^" + pattern_suffix + @")"
                                     + @")";
        string pattern_partword_vmiddle_wend = @"("
                                      + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")"
                                      + @")";
        string pattern_partword_vend_wend = @"("
                                          + @"(" + pattern_suffix + @"$" + @")"
                                          + @")";
        string pattern_partword_vanywhere_wend = @"(" + pattern_partword_vstart_wend + "|" + pattern_partword_vmiddle_wend + "|" + pattern_partword_vend_wend + @")";

        // Whole word
        string pattern_wholeword_vstart_wend = @"(" + pattern_whole_line + "|"
                                      + @"(" + @"^" + pattern_whole_word + @")"
                                      + @")";
        string pattern_wholeword_vmiddle_wend = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                              + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")"
                                              + @")";
        string pattern_wholeword_vend_wend = @"(" + pattern_whole_line + "|"
                                           + @"(" + pattern_whole_word + @"$" + @")"
                                           + @")";
        string pattern_wholeword_vanywhere_wend = @"(" + pattern_wholeword_vstart_wend + "|" + pattern_wholeword_vmiddle_wend + "|" + pattern_wholeword_vend_wend + @")";
        ///////////////////////////////////////////////////////////////////////////////////////



        switch (text_wordness)
        {
            case TextWordness.Any:
                {
                    switch (text_location_in_verse)
                    {
                        case TextLocationInVerse.Any:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtStart:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vstart_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vstart_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vstart_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vstart_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtMiddle:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtEnd:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vend_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vend_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vend_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vend_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
                break;
            case TextWordness.PartOfWord:
                {
                    switch (text_location_in_verse)
                    {
                        case TextLocationInVerse.Any:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vanywhere_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vanywhere_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vanywhere_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vanywhere_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtStart:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vstart_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vstart_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vstart_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vstart_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtMiddle:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vmiddle_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vmiddle_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vmiddle_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vmiddle_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtEnd:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vend_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vend_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vend_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vend_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
                break;
            case TextWordness.WholeWord:
                {
                    switch (text_location_in_verse)
                    {
                        case TextLocationInVerse.Any:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtStart:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vstart_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vstart_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vstart_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vstart_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtMiddle:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtEnd:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vend_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vend_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vend_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vend_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
                break;
            default:
                {
                    pattern = pattern_empty_line;
                }
                break;
        }

        return pattern;
    }
    private static string BuildPatternByWords(string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness)
    {
        string pattern = null;

        if (String.IsNullOrEmpty(text))
            return text;

        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

        // search for Quran markers, stopmarks, numbers, etc.
        if (text.Length == 1)
        {
            if (!Constants.ARABIC_LETTERS.Contains(text[0]))
            {
                return text;
            }
        }

        /*
        =====================================================================
        Regular Expressions
        =====================================================================
        Best Reference: https://regular-expressions.info/
        RegEx Utility:  https://sourceforge.net/projects/regulator/
        RegEx Tester:   https://regexstorm.net/tester
        =====================================================================
        Matches	Characters
        x	character x
        \\	backslash character
        \0n	character with octal value 0n (0 <= n <= 7)
        \0nn	character with octal value 0nn (0 <= n <= 7)
        \0mnn	character with octal value 0mnn (0 <= m <= 3, 0 <= n <= 7)
        \xhh	character with hexadecimal value 0xhh
        \uhhhh	character with hexadecimal value 0xhhhh
        \t	tab character ('\u0009')
        \n	newline (line feed) character ('\u000A')
        \r	carriage-return character ('\u000D')
        \f	form-feed character ('\u000C')
        \a	alert (bell) character ('\u0007')
        \e	escape character ('\u001B')
        \cx	control character corresponding to x

        Character Classes
        [abc]		    a, b, or c				                    (simple class)
        [^abc]		    any character except a, b, or c		        (negation)
        [a-zA-Z]	    a through z or A through Z, inclusive	    (range)
        [a-d[m-p]]	    a through d, or m through p: [a-dm-p]	    (union)
        [a-z&&[def]]	d, e, or f				                    (intersection)
        [a-z&&[^bc]]	a through z, except for b and c: [ad-z]	    (subtraction)
        [a-z&&[^m-p]]	a through z, and not m through p: [a-lq-z]  (subtraction)

        Predefined
        .	any character (inc line terminators) except newline
        \d	digit				            [0-9]
        \D	non-digit			            [^0-9]
        \s	whitespace character		    [ \t\n\x0B\f\r]
        \S	non-whitespace character	    [^\s]
        \w	word character (alphanumeric)	[a-zA-Z_0-9]
        \W	non-word character		        [^\w]

        Boundary Matchers
        ^	beginning of a line	(in Multiline)
        $	end of a line  		(in Multiline)
        \b	word boundary, including line start and line end
        \B	non-word boundary
        \A	beginning of the input
        \G	end of the previous match
        \Z	end of the input but for the final terminator, if any
        \z	end of the input

        Greedy quantifiers
        X?	     X 0 or 1    times
        X*	     X 0 or more times
        X+	     X 1 or more times
        X{n}	 X n         times
        X{n,}	 X n or more times
        X{n,m}	 X n to m    times

        Reluctant quantifiers
        X??	     X 0 or 1    times
        X*?	     X 0 or more times
        X+?	     X 1 or more times
        X{n}?	 X n         times
        X{n,}?	 X n or more times
        X{n,m}?	 X n to m    times

        Possessive quantifiers
        X?+	     X 0 or 1    times
        X*+	     X 0 or more times
        X++	     X 1 or more times
        X{n}+	 X n         times
        X{n,}+	 X n or more times
        X{n,m}+	 X n to m    times

        (?=text)  positive lookahead
        (?!text)  negative lookahead           // not at end of line  (?!$)
        (?<=text) positive lookbehind
        (?<!text) negative lookbehind          // not at start of line (?<!^)
        =====================================================================
        */

        string pattern_empty_line = @"(" + @"^$" + @")";
        string pattern_whole_line = @"(" + @"^" + text + @"$" + @")";

        string pattern_whole_word = @"(" + @"\b" + text + @"\b" + @")";
        string pattern_prefix = @"(" + @"\b" + @"\S+?" + text + @"\b" + @")";
        string pattern_suffix = @"(" + @"\b" + text + @"\S+?" + @"\b" + @")";
        string pattern_prefix_and_suffix = @"(" + @"\b" + @"\S+?" + text + @"\S+?" + @"\b" + @")";


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = ANYWHERE
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wanywhere = @"(" + pattern_whole_line + "|"
                                             + @"(" + @"^" + pattern_whole_word + @")" + "|"
                                             + @"(" + @"^" + pattern_prefix_and_suffix + @")" + "|"
                                             + @"(" + @"^" + pattern_suffix + @")" + "|"
                                             + @"(" + @"^" + pattern_prefix + @")"
                                             + @")";
        string pattern_anywordness_vmiddle_wanywhere = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_prefix_and_suffix + @"(?!$)" + @")" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")" + "|"
                                                     + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")"
                                                     + @")";
        string pattern_anywordness_vend_wanywhere = @"(" + pattern_whole_line + "|"
                                                  + @"(" + pattern_whole_word + @"$" + @")" + "|"
                                                  + @"(" + pattern_prefix_and_suffix + @"$" + @")" + "|"
                                                  + @"(" + pattern_suffix + @"$" + @")" + "|"
                                                  + @"(" + pattern_prefix + @"$" + @")"
                                                  + @")";
        string pattern_anywordness_not_vmiddle_wanywhere = @"(" + pattern_whole_line + "|" + pattern_anywordness_vstart_wanywhere + "|" + pattern_anywordness_vend_wanywhere + @")";
        string pattern_anywordness_vanywhere_wanywhere = @"(" + pattern_anywordness_vstart_wanywhere + "|" + pattern_anywordness_vmiddle_wanywhere + "|" + pattern_anywordness_vend_wanywhere + @")";

        // Part of word
        string pattern_partword_vstart_wanywhere = @"("
                                          + @"(" + @"^" + pattern_prefix_and_suffix + @")" + "|"
                                          + @"(" + @"^" + pattern_suffix + @")" + "|"
                                          + @"(" + @"^" + pattern_prefix + @")"
                                          + @")";
        string pattern_partword_vmiddle_wanywhere = @"("
                                                  + @"(" + @"(?<!^)" + pattern_prefix_and_suffix + @"(?!$)" + @")" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")"
                                                  + @")";
        string pattern_partword_vend_wanywhere = @"("
                                               + @"(" + pattern_prefix_and_suffix + @"$" + @")" + "|"
                                               + @"(" + pattern_suffix + @"$" + @")" + "|"
                                               + @"(" + pattern_prefix + @"$" + @")"
                                               + @")";
        string pattern_partword_not_vmiddle_wanywhere = @"(" + pattern_whole_line + "|" + pattern_partword_vstart_wanywhere + "|" + pattern_partword_vend_wanywhere + @")";
        string pattern_partword_vanywhere_wanywhere = @"(" + pattern_partword_vstart_wanywhere + "|" + pattern_partword_vmiddle_wanywhere + "|" + pattern_partword_vend_wanywhere + @")";

        // Whole word
        string pattern_wholeword_vstart_wanywhere = @"(" + pattern_whole_line + "|"
                                           + @"(" + @"^" + pattern_whole_word + @")"
                                           + @")";
        string pattern_wholeword_vmiddle_wanywhere = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                   + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")"
                                                   + @")";
        string pattern_wholeword_vend_wanywhere = @"(" + pattern_whole_line + "|"
                                                + @"(" + pattern_whole_word + @"$" + @")"
                                                + @")";
        string pattern_wholeword_not_vmiddle_wanywhere = @"(" + pattern_whole_line + "|" + pattern_wholeword_vstart_wanywhere + "|" + pattern_wholeword_vend_wanywhere + @")";
        string pattern_wholeword_vanywhere_wanywhere = @"(" + pattern_wholeword_vstart_wanywhere + "|" + pattern_wholeword_vmiddle_wanywhere + "|" + pattern_wholeword_vend_wanywhere + @")";
        ///////////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = START
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wstart = @"(" + pattern_whole_line + "|"
                                          + @"(" + @"^" + pattern_whole_word + @")" + "|"
                                          + @"(" + @"^" + pattern_suffix + @")"
                                          + @")";
        string pattern_anywordness_vmiddle_wstart = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")" + "|"
                                                  + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")"
                                                  + @")";
        string pattern_anywordness_vend_wstart = @"(" + pattern_whole_line + "|"
                                               + @"(" + pattern_whole_word + @"$" + @")" + "|"
                                               + @"(" + pattern_suffix + @"$" + @")"
                                               + @")";
        string pattern_anywordness_not_vmiddle_wstart = @"(" + pattern_whole_line + "|" + pattern_anywordness_vstart_wstart + "|" + pattern_anywordness_vend_wstart + @")";
        string pattern_anywordness_vanywhere_wstart = @"(" + pattern_anywordness_vstart_wstart + "|" + pattern_anywordness_vmiddle_wstart + "|" + pattern_anywordness_vend_wstart + @")";

        // Part of word
        string pattern_partword_vstart_wstart = @"("
                                              + @"(" + @"^" + pattern_suffix + @")"
                                              + @")";
        string pattern_partword_vmiddle_wstart = @"("
                                               + @"(" + @"(?<!^)" + pattern_suffix + @"(?!$)" + @")"
                                               + @")";
        string pattern_partword_vend_wstart = @"("
                                            + @"(" + pattern_suffix + @"$" + @")"
                                            + @")";
        string pattern_partword_not_vmiddle_wstart = @"(" + pattern_whole_line + "|" + pattern_partword_vstart_wstart + "|" + pattern_partword_vend_wstart + @")";
        string pattern_partword_vanywhere_wstart = @"(" + pattern_partword_vstart_wstart + "|" + pattern_partword_vmiddle_wstart + "|" + pattern_partword_vend_wstart + @")";

        // Whole word
        string pattern_wholeword_vstart_wstart = @"(" + pattern_whole_line + "|"
                                               + @"(" + @"^" + pattern_whole_word + @")"
                                               + @")";
        string pattern_wholeword_vmiddle_wstart = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")"
                                                + @")";
        string pattern_wholeword_vend_wstart = @"(" + pattern_whole_line + "|"
                                             + @"(" + pattern_whole_word + @"$" + @")"
                                             + @")";
        string pattern_wholeword_not_vmiddle_wstart = @"(" + pattern_whole_line + "|" + pattern_wholeword_vstart_wstart + "|" + pattern_wholeword_vend_wstart + @")";
        string pattern_wholeword_vanywhere_wstart = @"(" + pattern_wholeword_vstart_wstart + "|" + pattern_wholeword_vmiddle_wstart + "|" + pattern_wholeword_vend_wstart + @")";
        ///////////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = MIDDLE
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wmiddle = @"(" + @"^" + pattern_prefix_and_suffix + @")";
        string pattern_anywordness_vmiddle_wmiddle = @"(" + @"(?<!^)" + pattern_prefix_and_suffix + @"(?!$)" + @")";
        string pattern_anywordness_vend_wmiddle = @"(" + pattern_prefix_and_suffix + @"$" + @")";
        string pattern_anywordness_not_vmiddle_wmiddle = @"(" + pattern_whole_line + "|" + pattern_anywordness_vstart_wmiddle + "|" + pattern_anywordness_vend_wmiddle + @")";
        string pattern_anywordness_vanywhere_wmiddle = @"(" + pattern_anywordness_vstart_wmiddle + "|" + pattern_anywordness_vmiddle_wmiddle + "|" + pattern_anywordness_vend_wmiddle + @")";

        // Part of word
        string pattern_partword_vstart_wmiddle = @"(" + pattern_prefix_and_suffix + @")";
        string pattern_partword_vmiddle_wmiddle = @"(" + @"(?<!^)" + pattern_prefix_and_suffix + @"(?!$)" + @")";
        string pattern_partword_vend_wmiddle = @"(" + pattern_prefix_and_suffix + @"$" + @")";
        string pattern_partword_not_vmiddle_wmiddle = @"(" + pattern_whole_line + "|" + pattern_partword_vstart_wmiddle + "|" + pattern_partword_vend_wmiddle + @")";
        string pattern_partword_vanywhere_wmiddle = @"(" + pattern_partword_vstart_wmiddle + "|" + pattern_partword_vmiddle_wmiddle + "|" + pattern_partword_vend_wmiddle + @")";

        // Whole word
        string pattern_wholeword_vstart_wmiddle = @"(" + "^Dummy text not to be found.$" + @")";
        string pattern_wholeword_vmiddle_wmiddle = @"(" + "^Dummy text not to be found.$" + @")";
        string pattern_wholeword_vend_wmiddle = @"(" + "^Dummy text not to be found.$" + @")";
        string pattern_wholeword_not_vmiddle_wmiddle = @"(" + pattern_whole_line + "|" + pattern_wholeword_vstart_wmiddle + "|" + pattern_wholeword_vend_wmiddle + @")";
        string pattern_wholeword_vanywhere_wmiddle = @"(" + pattern_wholeword_vstart_wmiddle + "|" + pattern_wholeword_vmiddle_wmiddle + "|" + pattern_wholeword_vend_wmiddle + @")";
        ///////////////////////////////////////////////////////////////////////////////////////


        ///////////////////////////////////////////////////////////////////////////////////////
        // WORD LOCATION = END
        ///////////////////////////////////////////////////////////////////////////////////////
        // Any Wordness
        string pattern_anywordness_vstart_wend = @"(" + pattern_whole_line + "|"
                                        + @"(" + @"^" + pattern_whole_word + @")" + "|"
                                        + @"(" + @"^" + pattern_prefix + @")"
                                        + @")";
        string pattern_anywordness_vmiddle_wend = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                                + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")" + "|"
                                                + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")"
                                                + @")";
        string pattern_anywordness_vend_wend = @"(" + pattern_whole_line + "|"
                                             + @"(" + pattern_whole_word + @"$" + @")" + "|"
                                             + @"(" + pattern_prefix + @"$" + @")"
                                             + @")";
        string pattern_anywordness_not_vmiddle_wend = @"(" + pattern_whole_line + "|" + pattern_anywordness_vstart_wend + "|" + pattern_anywordness_vend_wend + @")";
        string pattern_anywordness_vanywhere_wend = @"(" + pattern_anywordness_vstart_wend + "|" + pattern_anywordness_vmiddle_wend + "|" + pattern_anywordness_vend_wend + @")";

        // Part of word
        string pattern_partword_vstart_wend = @"("
                                     + @"(" + @"^" + pattern_prefix + @")"
                                     + @")";
        string pattern_partword_vmiddle_wend = @"("
                                      + @"(" + @"(?<!^)" + pattern_prefix + @"(?!$)" + @")"
                                      + @")";
        string pattern_partword_vend_wend = @"("
                                          + @"(" + pattern_prefix + @"$" + @")"
                                          + @")";
        string pattern_partword_not_vmiddle_wend = @"(" + pattern_whole_line + "|" + pattern_partword_vstart_wend + "|" + pattern_partword_vend_wend + @")";
        string pattern_partword_vanywhere_wend = @"(" + pattern_partword_vstart_wend + "|" + pattern_partword_vmiddle_wend + "|" + pattern_partword_vend_wend + @")";

        // Whole word
        string pattern_wholeword_vstart_wend = @"(" + pattern_whole_line + "|"
                                      + @"(" + @"^" + pattern_whole_word + @")"
                                      + @")";
        string pattern_wholeword_vmiddle_wend = @"(" + @"(?<!^)" + pattern_whole_line + @"(?!$)" + "|"
                                              + @"(" + @"(?<!^)" + pattern_whole_word + @"(?!$)" + @")"
                                              + @")";
        string pattern_wholeword_vend_wend = @"(" + pattern_whole_line + "|"
                                           + @"(" + pattern_whole_word + @"$" + @")"
                                           + @")";
        string pattern_wholeword_not_vmiddle_wend = @"(" + pattern_whole_line + "|" + pattern_wholeword_vstart_wend + "|" + pattern_wholeword_vend_wend + @")";
        string pattern_wholeword_vanywhere_wend = @"(" + pattern_wholeword_vstart_wend + "|" + pattern_wholeword_vmiddle_wend + "|" + pattern_wholeword_vend_wend + @")";
        ///////////////////////////////////////////////////////////////////////////////////////



        switch (text_wordness)
        {
            case TextWordness.Any:
                {
                    switch (text_location_in_verse)
                    {
                        case TextLocationInVerse.Any:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vanywhere_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtStart:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vstart_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vstart_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vstart_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vstart_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtMiddle:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vmiddle_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtEnd:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_anywordness_vend_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_anywordness_vend_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_anywordness_vend_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_anywordness_vend_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
                break;
            case TextWordness.PartOfWord:
                {
                    switch (text_location_in_verse)
                    {
                        case TextLocationInVerse.Any:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vanywhere_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vanywhere_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vanywhere_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vanywhere_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtStart:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vstart_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vstart_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vstart_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vstart_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtMiddle:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vmiddle_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vmiddle_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vmiddle_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vmiddle_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtEnd:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_partword_vend_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_partword_vend_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_partword_vend_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_partword_vend_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
                break;
            case TextWordness.WholeWord:
                {
                    switch (text_location_in_verse)
                    {
                        case TextLocationInVerse.Any:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vanywhere_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtStart:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vstart_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vstart_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vstart_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vstart_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtMiddle:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vmiddle_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                        case TextLocationInVerse.AtEnd:
                            {
                                switch (text_location_in_word)
                                {
                                    case TextLocationInWord.Any:
                                        {
                                            pattern = pattern_wholeword_vend_wanywhere;
                                        }
                                        break;
                                    case TextLocationInWord.AtStart:
                                        {
                                            pattern = pattern_wholeword_vend_wstart;
                                        }
                                        break;
                                    case TextLocationInWord.AtMiddle:
                                        {
                                            pattern = pattern_wholeword_vend_wmiddle;
                                        }
                                        break;
                                    case TextLocationInWord.AtEnd:
                                        {
                                            pattern = pattern_wholeword_vend_wend;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
                break;
            default:
                {
                    pattern = pattern_empty_line;
                }
                break;
        }

        return pattern;
    }
    public static List<Phrase> FindPhrases(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, TextSearchBlockSize text_search_block_size, string text, LanguageType language_type, string translation, TextLocationInChapter text_location_in_chapter, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Phrase> result = new List<Phrase>();

        if (language_type == LanguageType.RightToLeft)
        {
            result = DoFindPhrases(search_scope, current_selection, previous_verses, text_search_block_size, text, language_type, translation, text_location_in_chapter, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
        }
        else if (language_type == LanguageType.LeftToRight)
        {
            if (!String.IsNullOrEmpty(translation))
            {
                result = DoFindPhrases(search_scope, current_selection, previous_verses, text_search_block_size, text, language_type, translation, text_location_in_chapter, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, false);
            }
            else
            {
                if (s_book != null)
                {
                    if (s_book.Verses != null)
                    {
                        if (s_book.Verses.Count > 0)
                        {
                            foreach (string key in s_book.Verses[0].Translations.Keys)
                            {
                                List<Phrase> new_phrases = DoFindPhrases(search_scope, current_selection, previous_verses, text_search_block_size, text, language_type, key, text_location_in_chapter, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, false);
                                result.AddRange(new_phrases);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Phrase> DoFindPhrases(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, TextSearchBlockSize text_search_block_size, string text, LanguageType language_type, string translation, TextLocationInChapter text_location_in_chapter, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Phrase> result = new List<Phrase>();

        if (!String.IsNullOrEmpty(text))
        {
            List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, text_location_in_chapter);
            if (source != null)
            {
                if (source.Count > 0)
                {
                    List<Verse> verses = new List<Verse>();
                    switch (text_search_block_size)
                    {
                        case TextSearchBlockSize.Verse:
                            {
                                verses = DoFindVerses(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);

                                if (language_type == LanguageType.RightToLeft)
                                {
                                    // special case works for TextSearchBlockSize.Verse with wholeword and count = # 
                                    return DoFindPhrases(verses, current_selection, previous_verses, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, try_emlaaei_if_nothing_found);
                                }
                                else //if (language_type == FindByTextLanguageType.LeftToRight)
                                {
                                    // general case, applies to all. no comparison operator nor count = # are considered
                                    return DoFindPhrases(translation, verses, current_selection, previous_verses, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, NumberType.None, ComparisonOperator.Equal, -1);
                                }
                            }
                        case TextSearchBlockSize.Chapter:
                            {
                                List<Chapter> chapters = DoFindChapters(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (chapters != null)
                                {
                                    foreach (Chapter chapter in chapters)
                                    {
                                        if (chapter != null)
                                        {
                                            verses.AddRange(chapter.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Page:
                            {
                                List<Page> pages = DoFindPages(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (pages != null)
                                {
                                    foreach (Page page in pages)
                                    {
                                        if (page != null)
                                        {
                                            verses.AddRange(page.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Station:
                            {
                                List<Station> stations = DoFindStations(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (stations != null)
                                {
                                    foreach (Station station in stations)
                                    {
                                        if (station != null)
                                        {
                                            verses.AddRange(station.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Part:
                            {
                                List<Part> parts = DoFindParts(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (parts != null)
                                {
                                    foreach (Part part in parts)
                                    {
                                        if (part != null)
                                        {
                                            verses.AddRange(part.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Group:
                            {
                                List<Model.Group> groups = DoFindGroups(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (groups != null)
                                {
                                    foreach (Model.Group group in groups)
                                    {
                                        if (group != null)
                                        {
                                            verses.AddRange(group.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Half:
                            {
                                List<Half> halfs = DoFindHalfs(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (halfs != null)
                                {
                                    foreach (Half half in halfs)
                                    {
                                        if (half != null)
                                        {
                                            verses.AddRange(half.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Quarter:
                            {
                                List<Quarter> quarters = DoFindQuarters(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (quarters != null)
                                {
                                    foreach (Quarter quarter in quarters)
                                    {
                                        if (quarter != null)
                                        {
                                            verses.AddRange(quarter.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Bowing:
                            {
                                List<Bowing> bowings = DoFindBowings(source, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, true);
                                if (bowings != null)
                                {
                                    foreach (Bowing bowing in bowings)
                                    {
                                        if (bowing != null)
                                        {
                                            verses.AddRange(bowing.Verses);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            verses = new List<Verse>();
                            break;
                    }

                    if (language_type == LanguageType.RightToLeft)
                    {
                        // all filtering was already done above
                        result = DoFindPhrases(verses, current_selection, previous_verses, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, (multiplicity == 0) ? 0 : -1, NumberType.None, multiplicity_comparison_operator, -1, try_emlaaei_if_nothing_found);
                    }
                    else //if (language_type == FindByTextLanguageType.LeftToRight)
                    {
                        // general case, applies to all. no comparison operator nor count = # are considered
                        result = DoFindPhrases(translation, verses, current_selection, previous_verses, text, text_location_in_verse, text_location_in_word, text_wordness, case_sensitive, with_diacritics, multiplicity, NumberType.None, ComparisonOperator.Equal, -1);
                    }
                }
            }
        }

        return result;
    }
    private static List<Phrase> DoFindPhrases(List<Verse> source, Selection current_selection, List<Verse> previous_verses, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Phrase> result = new List<Phrase>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                    RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                    if (with_diacritics)
                    {
                        string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                        if (!String.IsNullOrEmpty(pattern))
                        {
                            foreach (Verse verse in source)
                            {
                                if (verse != null)
                                {
                                    string verse_text = s_book.RemoveQuranmarksAndStopmarks(text, verse.Text);

                                    MatchCollection matches = Regex.Matches(verse_text, pattern, regex_options);
                                    if (multiplicity == 0) // contains no matches
                                    {
                                        if (matches.Count == 0)
                                        {
                                            result.Add(new Phrase(verse, 0, ""));
                                        }
                                    }
                                    else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
                                    {
                                        if (matches.Count > 0)
                                        {
                                            if (
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                result.AddRange(BuildPhrases(verse, matches, with_diacritics));
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Verse verse in source)
                                    {
                                        if (verse != null)
                                        {
                                            string verse_text = verse.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                verse_text = verse_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(verse_text, pattern, regex_options);
                                            if (multiplicity == 0) // contains no matches
                                            {
                                                if (matches.Count == 0)
                                                {
                                                    result.Add(new Phrase(verse, 0, ""));
                                                }
                                            }
                                            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
                                            {
                                                if (matches.Count > 0)
                                                {
                                                    if (
                                                         ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                         ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                         (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                                       )
                                                    {
                                                        result.AddRange(BuildPhrases(verse, matches, with_diacritics));
                                                    }
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                    if (!String.IsNullOrEmpty(pattern))
                                    {
                                        if ((source != null) && (source.Count > 0))
                                        {
                                            foreach (Verse verse in source)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        string emlaaei_text = verse.Translations[DEFAULT_EMLAAEI_TEXT];
                                                        emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                                        emlaaei_text = Regex.Replace(emlaaei_text, @"\s+", " "); // remove double space or higher if any

                                                        MatchCollection matches = Regex.Matches(emlaaei_text, pattern, regex_options);
                                                        if (multiplicity == 0) // contains no matches
                                                        {
                                                            if (matches.Count == 0)
                                                            {
                                                                result.Add(new Phrase(verse, 0, ""));
                                                            }
                                                        }
                                                        else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
                                                        {
                                                            if (matches.Count > 0)
                                                            {
                                                                if (
                                                                     ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                                     ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                                     (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                                                   )
                                                                {
                                                                    result.Add(new Phrase(verse, 0, ""));
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            } // end for
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Phrase> DoFindPhrases(string translation, List<Verse> source, Selection current_selection, List<Verse> previous_verses, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        if (String.IsNullOrEmpty(translation))
            return null;

        List<Phrase> result = new List<Phrase>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                    RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                    string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                    if (!String.IsNullOrEmpty(pattern))
                    {
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                MatchCollection matches = Regex.Matches(verse.Translations[translation], pattern, regex_options);
                                if (multiplicity == 0) // contains no matches
                                {
                                    if (matches.Count == 0)
                                    {
                                        result.Add(new Phrase(verse, 0, ""));
                                    }
                                }
                                else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
                                {
                                    if (matches.Count > 0)
                                    {
                                        if (
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            result.Add(new Phrase(verse, 0, ""));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Letter> DoFindLetters(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Letter> result = new List<Letter>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                    RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                    if (s_book != null)
                    {
                        List<Letter> letters = new List<Letter>();
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                letters.AddRange(verse.Letters);
                            }
                        }

                        if (with_diacritics)
                        {
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Letter letter in letters)
                                    {
                                        if (letter != null)
                                        {
                                            string letter_text = s_book.RemoveQuranmarksAndStopmarks(text, letter.Text);

                                            MatchCollection matches = Regex.Matches(letter_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(letter))
                                                {
                                                    result.Add(letter);
                                                }
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                        else // without diacritics
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text))
                                {
                                    string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                    if (!String.IsNullOrEmpty(pattern))
                                    {
                                        foreach (Letter letter in letters)
                                        {
                                            if (letter != null)
                                            {
                                                string letter_text = letter.Text;
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    letter_text = letter_text.Simplify29();
                                                }
                                                MatchCollection matches = Regex.Matches(letter_text, pattern, regex_options);
                                                if (
                                                     ((multiplicity == 0) && (matches.Count == 0)) ||
                                                     ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                     ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                     (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                                   )
                                                {
                                                    if (!result.Contains(letter))
                                                    {
                                                        result.Add(letter);
                                                    }
                                                }
                                            }
                                        } // end for
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Word> DoFindWords(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Word> result = new List<Word>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                    RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                    if (s_book != null)
                    {
                        List<Word> words = new List<Word>();
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                words.AddRange(verse.Words);
                            }
                        }

                        if (with_diacritics)
                        {
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Word word in words)
                                    {
                                        if (word != null)
                                        {
                                            string word_text = s_book.RemoveQuranmarksAndStopmarks(text, word.Text);

                                            MatchCollection matches = Regex.Matches(word_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(word))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                        else // without diacritics
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text))
                                {
                                    string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                    if (!String.IsNullOrEmpty(pattern))
                                    {
                                        foreach (Word word in words)
                                        {
                                            if (word != null)
                                            {
                                                string word_text = word.Text;
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    word_text = word_text.Simplify29();
                                                }
                                                MatchCollection matches = Regex.Matches(word_text, pattern, regex_options);
                                                if (
                                                     ((multiplicity == 0) && (matches.Count == 0)) ||
                                                     ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                     ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                     (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                                   )
                                                {
                                                    if (!result.Contains(word))
                                                    {
                                                        result.Add(word);
                                                    }
                                                }
                                            }
                                        } // end for
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Verse> DoFindVerses(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Verse> result = new List<Verse>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                    RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

                    if (s_book != null)
                    {
                        if (with_diacritics)
                        {
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Verse verse in source)
                                    {
                                        if (verse != null)
                                        {
                                            string verse_text = s_book.RemoveQuranmarksAndStopmarks(text, verse.Text);

                                            MatchCollection matches = Regex.Matches(verse_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(verse))
                                                {
                                                    result.Add(verse);
                                                }
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                        else // without diacritics
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text))
                                {
                                    string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                    if (!String.IsNullOrEmpty(pattern))
                                    {
                                        foreach (Verse verse in source)
                                        {
                                            if (verse != null)
                                            {
                                                string verse_text = verse.Text;
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    verse_text = verse_text.Simplify29();
                                                }
                                                MatchCollection matches = Regex.Matches(verse_text, pattern, regex_options);
                                                if (
                                                     ((multiplicity == 0) && (matches.Count == 0)) ||
                                                     ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                     ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                     (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                                   )
                                                {
                                                    if (!result.Contains(verse))
                                                    {
                                                        result.Add(verse);
                                                    }
                                                }
                                            }
                                        } // end for
                                    }
                                }
                            }
                        }

                        // if nothing found
                        if ((multiplicity != 0) && (result.Count == 0))
                        {
                            if (try_emlaaei_if_nothing_found)
                            {
                                if (s_numerical_system != null)
                                {
                                    text = text.Simplify(s_numerical_system.TextMode);
                                    if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                    {
                                        foreach (Verse verse in source)
                                        {
                                            if (verse != null)
                                            {
                                                if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                {
                                                    string emlaaei_text = verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                                    while (emlaaei_text.Contains("  "))
                                                    {
                                                        emlaaei_text = emlaaei_text.Replace("  ", " ");
                                                    }
                                                    MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                                    if (
                                                         ((multiplicity == 0) && (matches.Count == 0)) ||
                                                         ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                         ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                         (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                                       )
                                                    {
                                                        if (!result.Contains(verse))
                                                        {
                                                            result.Add(verse);
                                                        }
                                                    }
                                                }
                                            }
                                        } // end for
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Chapter> DoFindChapters(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Chapter> result = new List<Chapter>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Chapter> chapters = s_book.GetChapters(source);
                if (chapters != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Chapter chapter in chapters)
                                {
                                    if (chapter != null)
                                    {
                                        string chapter_text = s_book.RemoveQuranmarksAndStopmarks(text, chapter.Text);

                                        MatchCollection matches = Regex.Matches(chapter_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(chapter))
                                            {
                                                result.Add(chapter);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Chapter chapter in chapters)
                                    {
                                        if (chapter != null)
                                        {
                                            string chapter_text = chapter.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                chapter_text = chapter_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(chapter_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(chapter))
                                                {
                                                    result.Add(chapter);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Chapter chapter in chapters)
                                    {
                                        if (chapter != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in chapter.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(chapter))
                                                {
                                                    result.Add(chapter);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Page> DoFindPages(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Page> result = new List<Page>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Page> pages = s_book.GetPages(source);
                if (pages != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Page page in pages)
                                {
                                    if (page != null)
                                    {
                                        string page_text = s_book.RemoveQuranmarksAndStopmarks(text, page.Text);

                                        MatchCollection matches = Regex.Matches(page_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(page))
                                            {
                                                result.Add(page);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Page page in pages)
                                    {
                                        if (page != null)
                                        {
                                            string page_text = page.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                page_text = page_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(page_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(page))
                                                {
                                                    result.Add(page);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Page page in pages)
                                    {
                                        if (page != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in page.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(page))
                                                {
                                                    result.Add(page);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Station> DoFindStations(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Station> result = new List<Station>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Station> stations = s_book.GetStations(source);
                if (stations != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Station station in stations)
                                {
                                    if (station != null)
                                    {
                                        string station_text = s_book.RemoveQuranmarksAndStopmarks(text, station.Text);

                                        MatchCollection matches = Regex.Matches(station_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(station))
                                            {
                                                result.Add(station);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Station station in stations)
                                    {
                                        if (station != null)
                                        {
                                            string station_text = station.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                station_text = station_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(station_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(station))
                                                {
                                                    result.Add(station);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Station station in stations)
                                    {
                                        if (station != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in station.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(station))
                                                {
                                                    result.Add(station);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Part> DoFindParts(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Part> result = new List<Part>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Part> parts = s_book.GetParts(source);
                if (parts != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Part part in parts)
                                {
                                    if (part != null)
                                    {
                                        string part_text = s_book.RemoveQuranmarksAndStopmarks(text, part.Text);

                                        MatchCollection matches = Regex.Matches(part_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(part))
                                            {
                                                result.Add(part);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Part part in parts)
                                    {
                                        if (part != null)
                                        {
                                            string part_text = part.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                part_text = part_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(part_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(part))
                                                {
                                                    result.Add(part);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Part part in parts)
                                    {
                                        if (part != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in part.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(part))
                                                {
                                                    result.Add(part);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Model.Group> DoFindGroups(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Model.Group> result = new List<Model.Group>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Model.Group> groups = s_book.GetGroups(source);
                if (groups != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Model.Group group in groups)
                                {
                                    if (group != null)
                                    {
                                        string group_text = s_book.RemoveQuranmarksAndStopmarks(text, group.Text);

                                        MatchCollection matches = Regex.Matches(group_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(group))
                                            {
                                                result.Add(group);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Model.Group group in groups)
                                    {
                                        if (group != null)
                                        {
                                            string group_text = group.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                group_text = group_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(group_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(group))
                                                {
                                                    result.Add(group);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Model.Group group in groups)
                                    {
                                        if (group != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in group.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(group))
                                                {
                                                    result.Add(group);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Half> DoFindHalfs(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Half> result = new List<Half>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Half> halfs = s_book.GetHalfs(source);
                if (halfs != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Half half in halfs)
                                {
                                    if (half != null)
                                    {
                                        string half_text = s_book.RemoveQuranmarksAndStopmarks(text, half.Text);

                                        MatchCollection matches = Regex.Matches(half_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(half))
                                            {
                                                result.Add(half);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Half half in halfs)
                                    {
                                        if (half != null)
                                        {
                                            string half_text = half.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                half_text = half_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(half_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(half))
                                                {
                                                    result.Add(half);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Half half in halfs)
                                    {
                                        if (half != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in half.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(half))
                                                {
                                                    result.Add(half);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Quarter> DoFindQuarters(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Quarter> result = new List<Quarter>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Quarter> quarters = s_book.GetQuarters(source);
                if (quarters != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Quarter quarter in quarters)
                                {
                                    if (quarter != null)
                                    {
                                        string quarter_text = s_book.RemoveQuranmarksAndStopmarks(text, quarter.Text);

                                        MatchCollection matches = Regex.Matches(quarter_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(quarter))
                                            {
                                                result.Add(quarter);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Quarter quarter in quarters)
                                    {
                                        if (quarter != null)
                                        {
                                            string quarter_text = quarter.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                quarter_text = quarter_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(quarter_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(quarter))
                                                {
                                                    result.Add(quarter);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Quarter quarter in quarters)
                                    {
                                        if (quarter != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in quarter.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(quarter))
                                                {
                                                    result.Add(quarter);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Bowing> DoFindBowings(List<Verse> source, string text, TextLocationInVerse text_location_in_verse, TextLocationInWord text_location_in_word, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Bowing> result = new List<Bowing>();

        if (!String.IsNullOrEmpty(text))
        {
            text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
            RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            if (s_book != null)
            {
                List<Bowing> bowings = s_book.GetBowings(source);
                if (bowings != null)
                {
                    if (with_diacritics)
                    {
                        if (!String.IsNullOrEmpty(text))
                        {
                            string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                            if (!String.IsNullOrEmpty(pattern))
                            {
                                foreach (Bowing bowing in bowings)
                                {
                                    if (bowing != null)
                                    {
                                        string bowing_text = s_book.RemoveQuranmarksAndStopmarks(text, bowing.Text);

                                        MatchCollection matches = Regex.Matches(bowing_text, pattern, regex_options);
                                        if (
                                             ((multiplicity == 0) && (matches.Count == 0)) ||
                                             ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                             ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                             (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(bowing))
                                            {
                                                result.Add(bowing);
                                            }
                                        }
                                    }
                                }
                            } // end for
                        }
                    }
                    else // without diacritics
                    {
                        if (s_numerical_system != null)
                        {
                            text = text.Simplify(s_numerical_system.TextMode);
                            if (!String.IsNullOrEmpty(text))
                            {
                                string pattern = BuildPattern(text, text_location_in_verse, text_location_in_word, text_wordness);
                                if (!String.IsNullOrEmpty(pattern))
                                {
                                    foreach (Bowing bowing in bowings)
                                    {
                                        if (bowing != null)
                                        {
                                            string bowing_text = bowing.Text;
                                            if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                            {
                                                bowing_text = bowing_text.Simplify29();
                                            }
                                            MatchCollection matches = Regex.Matches(bowing_text, pattern, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(bowing))
                                                {
                                                    result.Add(bowing);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }

                    // if nothing found
                    if ((multiplicity != 0) && (result.Count == 0))
                    {
                        if (try_emlaaei_if_nothing_found)
                        {
                            if (s_numerical_system != null)
                            {
                                text = text.Simplify(s_numerical_system.TextMode);
                                if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                                {
                                    foreach (Bowing bowing in bowings)
                                    {
                                        if (bowing != null)
                                        {
                                            string emlaaei_text = "";
                                            foreach (Verse verse in bowing.Verses)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        emlaaei_text += verse.Translations[DEFAULT_EMLAAEI_TEXT] + "\r\n";
                                                    }
                                                }
                                            }
                                            emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode).Trim();
                                            while (emlaaei_text.Contains("  "))
                                            {
                                                emlaaei_text = emlaaei_text.Replace("  ", " ");
                                            }
                                            MatchCollection matches = Regex.Matches(emlaaei_text, text, regex_options);
                                            if (
                                                 ((multiplicity == 0) && (matches.Count == 0)) ||
                                                 ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                                 ((multiplicity_number_type == NumberType.Natural) && (matches.Count == multiplicity)) ||
                                                 (Compare(matches.Count, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                               )
                                            {
                                                if (!result.Contains(bowing))
                                                {
                                                    result.Add(bowing);
                                                }
                                            }
                                        }
                                    } // end for
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by text - Proximity
    public static List<Word> FindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, TextSearchBlockSize text_search_block_size, string text, LanguageType language_type, string translation, TextWordGrouping text_word_grouping, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Word> result = new List<Word>();

        if (language_type == LanguageType.RightToLeft)
        {
            result = DoFindWords(search_scope, current_selection, previous_verses, text_search_block_size, text, text_word_grouping, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, try_emlaaei_if_nothing_found);
        }
        else if (language_type == LanguageType.LeftToRight)
        {
            if (!String.IsNullOrEmpty(translation))
            {
                result = DoFindWords(translation, search_scope, current_selection, previous_verses, text, text_word_grouping, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
            }
            else
            {
                if (s_book != null)
                {
                    if (s_book.Verses != null)
                    {
                        if (s_book.Verses.Count > 0)
                        {
                            foreach (string key in s_book.Verses[0].Translations.Keys)
                            {
                                List<Word> new_words = DoFindWords(search_scope, current_selection, previous_verses, text_search_block_size, key, text_word_grouping, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, try_emlaaei_if_nothing_found);
                                result.AddRange(new_words);
                            }
                        }
                    }
                }
            }
        }

        Word.CompareBy = WordCompareBy.Number;
        Word.CompareOrder = WordCompareOrder.Ascending;
        result.Sort();

        return result;
    }
    private static List<Word> DoFindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, TextSearchBlockSize text_search_block_size, string text, TextWordGrouping text_word_grouping, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Word> result = new List<Word>();

        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        if (source != null)
        {
            if (source.Count > 0)
            {
                List<Verse> verses = new List<Verse>();

                switch (text_search_block_size)
                {
                    case TextSearchBlockSize.Verse:
                        {
                            verses = DoFindVerses(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                        }
                        break;
                    case TextSearchBlockSize.Chapter:
                        {
                            List<Chapter> chapters = DoFindChapters(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (chapters != null)
                            {
                                foreach (Chapter chapter in chapters)
                                {
                                    if (chapter != null)
                                    {
                                        verses.AddRange(chapter.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case TextSearchBlockSize.Page:
                        {
                            List<Page> pages = DoFindPages(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (pages != null)
                            {
                                foreach (Page page in pages)
                                {
                                    if (page != null)
                                    {
                                        verses.AddRange(page.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case TextSearchBlockSize.Station:
                        {
                            List<Station> stations = DoFindStations(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (stations != null)
                            {
                                foreach (Station station in stations)
                                {
                                    if (station != null)
                                    {
                                        verses.AddRange(station.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case TextSearchBlockSize.Part:
                        {
                            List<Part> parts = DoFindParts(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (parts != null)
                            {
                                foreach (Part part in parts)
                                {
                                    if (part != null)
                                    {
                                        verses.AddRange(part.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case TextSearchBlockSize.Group:
                        {
                            List<Model.Group> groups = DoFindGroups(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (groups != null)
                            {
                                foreach (Model.Group group in groups)
                                {
                                    if (group != null)
                                    {
                                        verses.AddRange(group.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case TextSearchBlockSize.Half:
                        {
                            List<Half> halfs = DoFindHalfs(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (halfs != null)
                            {
                                foreach (Half half in halfs)
                                {
                                    if (half != null)
                                    {
                                        verses.AddRange(half.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case TextSearchBlockSize.Quarter:
                        {
                            List<Quarter> quarters = DoFindQuarters(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (quarters != null)
                            {
                                foreach (Quarter quarter in quarters)
                                {
                                    if (quarter != null)
                                    {
                                        verses.AddRange(quarter.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    case TextSearchBlockSize.Bowing:
                        {
                            List<Bowing> bowings = DoFindBowings(source, text, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            if (bowings != null)
                            {
                                foreach (Bowing bowing in bowings)
                                {
                                    if (bowing != null)
                                    {
                                        verses.AddRange(bowing.Verses);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                result = DoFindWords(verses, current_selection, previous_verses, text, text_word_grouping, text_wordness, case_sensitive, with_diacritics, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, try_emlaaei_if_nothing_found);
            }
        }

        return result;
    }
    private static List<Word> DoFindWords(List<Verse> source, Selection current_selection, List<Verse> previous_verses, string text, TextWordGrouping text_word_grouping, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool try_emlaaei_if_nothing_found)
    {
        List<Word> result = new List<Word>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any
                    if (s_numerical_system != null)
                    {
                        text = with_diacritics ? text : text.Simplify(s_numerical_system.TextMode);
                        if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                        {
                            List<string> terms = new List<string>();
                            string[] text_words = text.Split();
                            foreach (string text_word in text_words)
                            {
                                terms.Add(text_word);
                            }

                            foreach (Verse verse in source)
                            {
                                if (verse != null)
                                {
                                    // 1 term found is enough in OR case
                                    if (text_word_grouping == TextWordGrouping.Or)
                                    {
                                        bool found = false;
                                        foreach (string term in terms)
                                        {
                                            foreach (Word word in verse.Words)
                                            {
                                                if (word != null)
                                                {
                                                    string word_text = word.Text;
                                                    word_text = with_diacritics ? word_text : word_text.Simplify(s_numerical_system.TextMode);

                                                    if (text_wordness == TextWordness.Any)
                                                    {
                                                        if (word_text.Contains(term))
                                                        {
                                                            found = true;
                                                            break; // no need to continue even if there are more matches
                                                        }
                                                    }
                                                    else if (text_wordness == TextWordness.PartOfWord)
                                                    {
                                                        if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                        {
                                                            found = true;
                                                            break; // no need to continue even if there are more matches
                                                        }
                                                    }
                                                    else if (text_wordness == TextWordness.WholeWord)
                                                    {
                                                        if (word_text == term)
                                                        {
                                                            found = true;
                                                            break; // no need to continue even if there are more matches
                                                        }
                                                    }
                                                }
                                            }
                                            if (found)
                                            {
                                                break;
                                            }
                                        }
                                        if (found) // 1 term found is enough in OR case
                                        {
                                            foreach (string term in terms)
                                            {
                                                foreach (Word word in verse.Words)
                                                {
                                                    if (word != null)
                                                    {
                                                        string word_text = word.Text;
                                                        word_text = with_diacritics ? word_text : word_text.Simplify(s_numerical_system.TextMode);

                                                        if (text_wordness == TextWordness.Any)
                                                        {
                                                            if (word_text.Contains(term))
                                                            {
                                                                result.Add(word);
                                                                //break; // no break in case there are more matches
                                                            }
                                                        }
                                                        else if (text_wordness == TextWordness.PartOfWord)
                                                        {
                                                            if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                            {
                                                                result.Add(word);
                                                                //break; // no break in case there are more matches
                                                            }
                                                        }
                                                        else if (text_wordness == TextWordness.WholeWord)
                                                        {
                                                            if (word_text == term)
                                                            {
                                                                result.Add(word);
                                                                //break; // no break in case there are more matches
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else // verse failed test, so skip it
                                        {
                                            continue; // next verse
                                        }
                                    }
                                    // all terms must be found in verse in AND case
                                    else if (text_word_grouping == TextWordGrouping.And)
                                    {
                                        int match_count = 0;
                                        foreach (string term in terms)
                                        {
                                            foreach (Word word in verse.Words)
                                            {
                                                if (word != null)
                                                {
                                                    string word_text = word.Text;
                                                    word_text = with_diacritics ? word_text : word_text.Simplify(s_numerical_system.TextMode);

                                                    if (text_wordness == TextWordness.Any)
                                                    {
                                                        if (word_text.Contains(term))
                                                        {
                                                            match_count++;
                                                            break; // no need to continue even if there are more matches
                                                        }
                                                    }
                                                    else if (text_wordness == TextWordness.PartOfWord)
                                                    {
                                                        if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                        {
                                                            match_count++;
                                                            break; // no need to continue even if there are more matches
                                                        }
                                                    }
                                                    else if (text_wordness == TextWordness.WholeWord)
                                                    {
                                                        if (word_text == term)
                                                        {
                                                            match_count++;
                                                            break; // no need to continue even if there are more matches
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        // match all terms in AND case
                                        if (match_count == terms.Count)
                                        {
                                            foreach (string term in terms)
                                            {
                                                foreach (Word word in verse.Words)
                                                {
                                                    if (word != null)
                                                    {
                                                        string word_text = word.Text;
                                                        word_text = with_diacritics ? word_text : word_text.Simplify(s_numerical_system.TextMode);

                                                        if (text_wordness == TextWordness.Any)
                                                        {
                                                            if (word_text.Contains(term))
                                                            {
                                                                result.Add(word);
                                                                //break; // no break in case there are more matches
                                                            }
                                                        }
                                                        else if (text_wordness == TextWordness.PartOfWord)
                                                        {
                                                            if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                            {
                                                                result.Add(word);
                                                                //break; // no break in case there are more matches
                                                            }
                                                        }
                                                        else if (text_wordness == TextWordness.WholeWord)
                                                        {
                                                            if (word_text == term)
                                                            {
                                                                result.Add(word);
                                                                //break; // no break in case there are more matches
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else // verse failed test, so skip it
                                        {
                                            continue; // next verse
                                        }
                                    }
                                }
                            } // end for

                            // if nothing found
                            if (result.Count == 0)
                            {
                                if (try_emlaaei_if_nothing_found)
                                {
                                    if (s_numerical_system != null)
                                    {
                                        if ((source != null) && (source.Count > 0))
                                        {
                                            foreach (Verse verse in source)
                                            {
                                                if (verse != null)
                                                {
                                                    if (verse.Translations.ContainsKey(DEFAULT_EMLAAEI_TEXT))
                                                    {
                                                        string emlaaei_text = verse.Translations[DEFAULT_EMLAAEI_TEXT];
                                                        emlaaei_text = emlaaei_text.Simplify(s_numerical_system.TextMode);
                                                        emlaaei_text = text.Trim();
                                                        string[] emlaaei_words = emlaaei_text.Split();

                                                        // 1 term found is enough in OR case
                                                        if (text_word_grouping == TextWordGrouping.Or)
                                                        {
                                                            bool found = false;
                                                            foreach (string term in terms)
                                                            {
                                                                foreach (string emlaaei_word in emlaaei_words)
                                                                {
                                                                    string word_text = emlaaei_word;
                                                                    word_text = with_diacritics ? emlaaei_word : emlaaei_word.Simplify(s_numerical_system.TextMode);

                                                                    if (text_wordness == TextWordness.Any)
                                                                    {
                                                                        if (word_text.Contains(term))
                                                                        {
                                                                            found = true;
                                                                            break; // no need to continue even if there are more matches
                                                                        }
                                                                    }
                                                                    else if (text_wordness == TextWordness.PartOfWord)
                                                                    {
                                                                        if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                                        {
                                                                            found = true;
                                                                            break; // no need to continue even if there are more matches
                                                                        }
                                                                    }
                                                                    else if (text_wordness == TextWordness.WholeWord)
                                                                    {
                                                                        if (word_text == term)
                                                                        {
                                                                            found = true;
                                                                            break; // no need to continue even if there are more matches
                                                                        }
                                                                    }
                                                                }
                                                                if (found)
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                            if (found) // 1 term found is enough in OR case
                                                            {
                                                                foreach (string term in terms)
                                                                {
                                                                    foreach (Word word in verse.Words)
                                                                    {
                                                                        if (word != null)
                                                                        {
                                                                            string word_text = word.Text;
                                                                            word_text = with_diacritics ? word_text : word_text.Simplify(s_numerical_system.TextMode);

                                                                            if (text_wordness == TextWordness.Any)
                                                                            {
                                                                                if (word_text.Contains(term))
                                                                                {
                                                                                    result.Add(word);
                                                                                    //break; // no break in case there are more matches
                                                                                }
                                                                            }
                                                                            else if (text_wordness == TextWordness.PartOfWord)
                                                                            {
                                                                                if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                                                {
                                                                                    result.Add(word);
                                                                                    //break; // no break in case there are more matches
                                                                                }
                                                                            }
                                                                            else if (text_wordness == TextWordness.WholeWord)
                                                                            {
                                                                                if (word_text == term)
                                                                                {
                                                                                    result.Add(word);
                                                                                    //break; // no break in case there are more matches
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else // verse failed test, so skip it
                                                            {
                                                                continue; // next verse
                                                            }
                                                        }
                                                        // all terms must be found in verse in AND case
                                                        else if (text_word_grouping == TextWordGrouping.And)
                                                        {
                                                            int match_count = 0;
                                                            foreach (string term in terms)
                                                            {
                                                                foreach (Word word in verse.Words)
                                                                {
                                                                    if (word != null)
                                                                    {
                                                                        string word_text = word.Text;
                                                                        word_text = with_diacritics ? word_text : word_text.Simplify(s_numerical_system.TextMode);

                                                                        if (text_wordness == TextWordness.Any)
                                                                        {
                                                                            if (word_text.Contains(term))
                                                                            {
                                                                                match_count++;
                                                                                break; // no need to continue even if there are more matches
                                                                            }
                                                                        }
                                                                        else if (text_wordness == TextWordness.PartOfWord)
                                                                        {
                                                                            if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                                            {
                                                                                match_count++;
                                                                                break; // no need to continue even if there are more matches
                                                                            }
                                                                        }
                                                                        else if (text_wordness == TextWordness.WholeWord)
                                                                        {
                                                                            if (word_text == term)
                                                                            {
                                                                                match_count++;
                                                                                break; // no need to continue even if there are more matches
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            // match all terms in AND case
                                                            if (match_count == terms.Count)
                                                            {
                                                                foreach (string term in terms)
                                                                {
                                                                    foreach (Word word in verse.Words)
                                                                    {
                                                                        if (word != null)
                                                                        {
                                                                            string word_text = word.Text;
                                                                            word_text = with_diacritics ? word_text : word_text.Simplify(s_numerical_system.TextMode);

                                                                            if (text_wordness == TextWordness.Any)
                                                                            {
                                                                                if (word_text.Contains(term))
                                                                                {
                                                                                    result.Add(word);
                                                                                    //break; // no break in case there are more matches
                                                                                }
                                                                            }
                                                                            else if (text_wordness == TextWordness.PartOfWord)
                                                                            {
                                                                                if ((word_text.Contains(term)) && (word_text.Length > term.Length))
                                                                                {
                                                                                    result.Add(word);
                                                                                    //break; // no break in case there are more matches
                                                                                }
                                                                            }
                                                                            else if (text_wordness == TextWordness.WholeWord)
                                                                            {
                                                                                if (word_text == term)
                                                                                {
                                                                                    result.Add(word);
                                                                                    //break; // no break in case there are more matches
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else // verse failed test, so skip it
                                                            {
                                                                continue; // next verse
                                                            }
                                                        }
                                                    }
                                                }
                                            } // end for
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // TODO int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, 

        return result;
    }
    private static List<Word> DoFindWords(string translation, SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string text, TextWordGrouping text_word_grouping, TextWordness text_wordness, bool case_sensitive, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Word> result = new List<Word>();

        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    RegexOptions regex_options = case_sensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    text = Regex.Replace(text, @"\s+", " ", regex_options); // remove double space or higher if any
                    text = text.Trim();
                    if (!String.IsNullOrEmpty(text)) // re-test in case text was just harakat which is simplifed to nothing
                    {
                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                if (verse.Translations.ContainsKey(translation))
                                {
                                    // 1 term found is enough in OR case
                                    if (text_word_grouping == TextWordGrouping.Or)
                                    {
                                        foreach (string term in terms)
                                        {
                                            if (verse.Translations[translation].Contains(term))
                                            {
                                                result.Add(new Word(verse, 0, 0, ""));
                                                break;
                                            }
                                        }
                                    }
                                    // all terms must be found in verse in AND case
                                    else if (text_word_grouping == TextWordGrouping.And)
                                    {
                                        int match_count = 0;
                                        foreach (string term in terms)
                                        {
                                            if (verse.Translations[translation].Contains(term))
                                            {
                                                match_count++;
                                            }
                                        }
                                        // match all terms in AND case
                                        if (match_count == terms.Count)
                                        {
                                            result.Add(new Word(verse, 0, 0, ""));
                                        }
                                    }
                                }
                            }
                        } // end for
                    }
                }
            }
        }

        return result;
    }
    private static List<Verse> DoFindVerses(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Verse> result = new List<Verse>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                string verse_text = verse.Text.Trim();
                                if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                {
                                    verse_text = verse_text.Simplify29();
                                }
                                verse_text = verse_text.Trim();

                                int matches = 0;
                                foreach (string term in terms)
                                {
                                    MatchCollection match_collection = Regex.Matches(verse_text, term);
                                    matches += match_collection.Count;
                                }

                                if (
                                    ((multiplicity == 0) && (matches == 0)) ||
                                    ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                    ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                    (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                   )
                                {
                                    if (!result.Contains(verse))
                                    {
                                        result.Add(verse);
                                    }
                                }
                            }
                        } // end for
                    }
                }
            }
        }

        return result;
    }
    private static List<Chapter> DoFindChapters(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Chapter> result = new List<Chapter>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Chapter> chapters = s_book.GetChapters(source);
                            if (chapters != null)
                            {
                                foreach (Chapter chapter in chapters)
                                {
                                    if (chapter != null)
                                    {
                                        string chapter_text = chapter.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            chapter_text = chapter_text.Simplify29();
                                        }
                                        chapter_text = chapter_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(chapter_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(chapter))
                                            {
                                                result.Add(chapter);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Page> DoFindPages(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Page> result = new List<Page>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Page> pages = s_book.GetPages(source);
                            if (pages != null)
                            {
                                foreach (Page page in pages)
                                {
                                    if (page != null)
                                    {
                                        string page_text = page.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            page_text = page_text.Simplify29();
                                        }
                                        page_text = page_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(page_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(page))
                                            {
                                                result.Add(page);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Station> DoFindStations(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Station> result = new List<Station>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Station> stations = s_book.GetStations(source);
                            if (stations != null)
                            {
                                foreach (Station station in stations)
                                {
                                    if (station != null)
                                    {
                                        string station_text = station.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            station_text = station_text.Simplify29();
                                        }
                                        station_text = station_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(station_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(station))
                                            {
                                                result.Add(station);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Part> DoFindParts(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Part> result = new List<Part>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Part> parts = s_book.GetParts(source);
                            if (parts != null)
                            {
                                foreach (Part part in parts)
                                {
                                    if (part != null)
                                    {
                                        string part_text = part.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            part_text = part_text.Simplify29();
                                        }
                                        part_text = part_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(part_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(part))
                                            {
                                                result.Add(part);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Model.Group> DoFindGroups(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Model.Group> result = new List<Model.Group>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Model.Group> groups = s_book.GetGroups(source);
                            if (groups != null)
                            {
                                foreach (Model.Group group in groups)
                                {
                                    if (group != null)
                                    {
                                        string group_text = group.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            group_text = group_text.Simplify29();
                                        }
                                        group_text = group_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(group_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(group))
                                            {
                                                result.Add(group);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Half> DoFindHalfs(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Half> result = new List<Half>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Half> halfs = s_book.GetHalfs(source);
                            if (halfs != null)
                            {
                                foreach (Half half in halfs)
                                {
                                    if (half != null)
                                    {
                                        string half_text = half.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            half_text = half_text.Simplify29();
                                        }
                                        half_text = half_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(half_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(half))
                                            {
                                                result.Add(half);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Quarter> DoFindQuarters(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Quarter> result = new List<Quarter>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Quarter> quarters = s_book.GetQuarters(source);
                            if (quarters != null)
                            {
                                foreach (Quarter quarter in quarters)
                                {
                                    if (quarter != null)
                                    {
                                        string quarter_text = quarter.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            quarter_text = quarter_text.Simplify29();
                                        }
                                        quarter_text = quarter_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(quarter_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(quarter))
                                            {
                                                result.Add(quarter);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Bowing> DoFindBowings(List<Verse> source, string text, bool with_diacritics, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Bowing> result = new List<Bowing>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(text))
                {
                    if (s_numerical_system != null)
                    {
                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                        {
                            text = text.Simplify29();
                        }
                        text = Regex.Replace(text, @"\s+", " "); // remove double space or higher if any

                        List<string> terms = new List<string>();
                        string[] text_words = text.Split();
                        foreach (string text_word in text_words)
                        {
                            terms.Add(text_word);
                        }

                        if (s_book != null)
                        {
                            List<Bowing> bowings = s_book.GetBowings(source);
                            if (bowings != null)
                            {
                                foreach (Bowing bowing in bowings)
                                {
                                    if (bowing != null)
                                    {
                                        string bowing_text = bowing.Text.Trim();
                                        if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                        {
                                            bowing_text = bowing_text.Simplify29();
                                        }
                                        bowing_text = bowing_text.Trim();

                                        int matches = 0;
                                        foreach (string term in terms)
                                        {
                                            MatchCollection match_collection = Regex.Matches(bowing_text, term);
                                            matches += match_collection.Count;
                                        }

                                        if (
                                            ((multiplicity == 0) && (matches == 0)) ||
                                            ((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) ||
                                            ((multiplicity_number_type == NumberType.Natural) && (matches == multiplicity)) ||
                                            (Compare(matches, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder))
                                           )
                                        {
                                            if (!result.Contains(bowing))
                                            {
                                                result.Add(bowing);
                                            }
                                        }
                                    }
                                } // end for
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by text - Root
    private static void MergeWords(List<Word> current_words, ref List<Verse> previous_verses, ref List<Word> previous_words)
    {
        if (current_words != null)
        {
            if (previous_words != null)
            {
                // extract verses from current words
                previous_verses = new List<Verse>(GetVerses(current_words));
                if (previous_verses != null)
                {
                    if (previous_words.Count == 0)
                    {
                        previous_words = current_words;
                    }
                    else
                    {
                        // add current words
                        List<Word> total = new List<Word>(current_words);

                        // add previous words if their verses exist in current words
                        foreach (Word previous_word in previous_words)
                        {
                            if (previous_word != null)
                            {
                                if (previous_verses.Contains(previous_word.Verse))
                                {
                                    total.Add(previous_word);
                                }
                            }
                        }
                        previous_words = total;
                    }
                }
            }
        }
    }
    public static List<Word> FindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, TextSearchBlockSize text_search_block_size, string roots, TextWordGrouping text_word_grouping, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Word> result = new List<Word>();

        // first call only
        if (previous_verses == null)
        {
            previous_verses = new List<Verse>();
        }

        if (String.IsNullOrEmpty(roots))
            return null;
        roots = roots.Simplify36();   // roots use 36 letters
        while (roots.Contains("  "))
        {
            roots = roots.Replace("  ", " ");
        }
        string[] terms = roots.Split();

        List<Word> current_result = null;
        foreach (string term in terms)
        {
            current_result = DoFindWords(search_scope, current_selection, previous_verses, text_search_block_size, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
            if (text_word_grouping == TextWordGrouping.Or)
            {
                result.AddRange(current_result);
                previous_verses.Union(GetVerses(current_result));
            }
            else if (text_word_grouping == TextWordGrouping.And)
            {
                MergeWords(current_result, ref previous_verses, ref result);
                search_scope = SearchScope.Result;
            }
        }

        Word.CompareBy = WordCompareBy.Number;
        Word.CompareOrder = WordCompareOrder.Ascending;
        result.Sort();

        return result;
    }
    private static List<Word> DoFindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, TextSearchBlockSize text_search_block_size, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Word> result = new List<Word>();

        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        if (source != null)
        {
            if (source.Count > 0)
            {
                roots = roots.Simplify36();   // roots use 36 letters
                while (roots.Contains("  "))
                {
                    roots = roots.Replace("  ", " ");
                }
                string[] terms = roots.Split();
                foreach (string term in terms)
                {
                    switch (text_search_block_size)
                    {
                        case TextSearchBlockSize.Verse:
                            {
                                return DoFindWords(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            }
                        //break;
                        case TextSearchBlockSize.Chapter:
                            {
                                List<Chapter> chapters = DoFindChapters(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (chapters != null)
                                {
                                    foreach (Chapter chapter in chapters)
                                    {
                                        if (chapter != null)
                                        {
                                            foreach (Word word in chapter.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Page:
                            {
                                List<Page> pages = DoFindPages(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (pages != null)
                                {
                                    foreach (Page page in pages)
                                    {
                                        if (page != null)
                                        {
                                            foreach (Word word in page.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Station:
                            {
                                List<Station> stations = DoFindStations(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (stations != null)
                                {
                                    foreach (Station station in stations)
                                    {
                                        if (station != null)
                                        {
                                            foreach (Word word in station.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Part:
                            {
                                List<Part> parts = DoFindParts(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (parts != null)
                                {
                                    foreach (Part part in parts)
                                    {
                                        if (part != null)
                                        {
                                            foreach (Word word in part.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Group:
                            {
                                List<Model.Group> groups = DoFindGroups(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (groups != null)
                                {
                                    foreach (Model.Group group in groups)
                                    {
                                        if (group != null)
                                        {
                                            foreach (Word word in group.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Half:
                            {
                                List<Half> halfs = DoFindHalfs(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (halfs != null)
                                {
                                    foreach (Half half in halfs)
                                    {
                                        if (half != null)
                                        {
                                            foreach (Word word in half.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Quarter:
                            {
                                List<Quarter> quarters = DoFindQuarters(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (quarters != null)
                                {
                                    foreach (Quarter quarter in quarters)
                                    {
                                        if (quarter != null)
                                        {
                                            foreach (Word word in quarter.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case TextSearchBlockSize.Bowing:
                            {
                                List<Bowing> bowings = DoFindBowings(source, roots, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                                if (bowings != null)
                                {
                                    foreach (Bowing bowing in bowings)
                                    {
                                        if (bowing != null)
                                        {
                                            foreach (Word word in bowing.Words)
                                            {
                                                if (word.Roots.Contains(term))
                                                {
                                                    result.Add(word);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            if (source != null)
                            {
                                foreach (Verse verse in source)
                                {
                                    if (verse != null)
                                    {
                                        foreach (Word word in verse.Words)
                                        {
                                            if (word.Roots.Contains(term))
                                            {
                                                result.Add(word);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        return result;
    }
    private static List<Word> DoFindWords(List<Verse> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Word> result = new List<Word>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (!String.IsNullOrEmpty(root))
                {
                    if (s_book != null)
                    {
                        SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                        if (root_words_dictionary != null)
                        {
                            List<Word> root_words = null;
                            if (root_words_dictionary.ContainsKey(root))
                            {
                                // get all pre-identified root_words
                                root_words = root_words_dictionary[root];
                            }

                            if (root_words != null)
                            {
                                result = GetWordsWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            }
                            else // text is a word, not a root
                            {
                                string best_root = s_book.GetBestRoot(root);
                                result = DoFindWords(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Word> GetWordsWithRootWords(List<Verse> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Word> result = new List<Word>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    Dictionary<Verse, int> multiplicity_dictionary = new Dictionary<Verse, int>();
                    foreach (Word word in root_words)
                    {
                        if (word != null)
                        {
                            Verse verse = s_book.Verses[word.Verse.Number - 1];
                            if (source.Contains(verse))
                            {
                                if (multiplicity_dictionary.ContainsKey(verse))
                                {
                                    multiplicity_dictionary[verse]++;
                                }
                                else // first found
                                {
                                    multiplicity_dictionary.Add(verse, 1);
                                }
                            }
                        }
                    }

                    if (multiplicity == 0) // contains no matches
                    {
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                if (!multiplicity_dictionary.ContainsKey(verse))
                                {
                                    result.Add(new Word(verse, 0, 0, "")); //TRICK: add fake word
                                }
                            }
                        }
                    }
                    else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
                    {
                        foreach (Word word in root_words)
                        {
                            if (word != null)
                            {
                                if (word.Verse != null)
                                {
                                    int verse_index = word.Verse.Number - 1;
                                    if ((verse_index >= 0) && (verse_index < s_book.Verses.Count))
                                    {
                                        Verse verse = s_book.Verses[verse_index];
                                        if (multiplicity_dictionary.ContainsKey(verse))
                                        {
                                            if (multiplicity == 0)
                                            {
                                                Word verse_word = verse.Words[0];
                                                result.Add(verse_word);
                                            }
                                            else if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[verse], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                            {
                                                if (source.Contains(verse))
                                                {
                                                    int word_index = word.NumberInVerse - 1;
                                                    if ((word_index >= 0) && (word_index < verse.Words.Count))
                                                    {
                                                        Word verse_word = verse.Words[word_index];
                                                        result.Add(verse_word);
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
            }
        }

        return result;
    }
    private static List<Chapter> DoFindChapters(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Chapter> result = new List<Chapter>();

        if (s_book != null)
        {
            List<Chapter> source = s_book.GetChapters(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Chapter> temp = DoFindChapters(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Chapter chapter in temp)
                {
                    if (chapter != null)
                    {
                        if (!result.Contains(chapter))
                        {
                            result.Add(chapter);
                        }
                    }
                }
                source = new List<Chapter>(result);
            }
        }

        return result;
    }
    private static List<Chapter> DoFindChapters(List<Chapter> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Chapter> result = new List<Chapter>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }

                    if (root_words != null)
                    {
                        result = GetChaptersWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindChapters(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Chapter> GetChaptersWithRootWords(List<Chapter> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Chapter> result = new List<Chapter>();

        if (source != null)
        {
            Dictionary<Chapter, int> multiplicity_dictionary = new Dictionary<Chapter, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Chapter chapter = word.Verse.Chapter;
                        if (chapter != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(chapter))
                            {
                                multiplicity_dictionary[chapter]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(chapter, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // chapter contains no matches
            {
                foreach (Chapter chapter in source)
                {
                    if (chapter != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(chapter))
                        {
                            if (!result.Contains(chapter))
                            {
                                result.Add(chapter);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Word word in root_words)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            Chapter chapter = word.Verse.Chapter;
                            if (chapter != null)
                            {
                                if (source.Contains(chapter))
                                {
                                    if (multiplicity_dictionary.ContainsKey(chapter))
                                    {
                                        if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[chapter], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                        {
                                            if (!result.Contains(chapter))
                                            {
                                                result.Add(chapter);
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

        return result;
    }
    private static List<Page> DoFindPages(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Page> result = new List<Page>();

        if (s_book != null)
        {
            List<Page> source = s_book.GetPages(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Page> temp = DoFindPages(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Page page in temp)
                {
                    if (page != null)
                    {
                        if (!result.Contains(page))
                        {
                            result.Add(page);
                        }
                    }
                }
                source = new List<Page>(result);
            }
        }

        return result;
    }
    private static List<Page> DoFindPages(List<Page> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Page> result = new List<Page>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }
                    if (root_words != null)
                    {
                        result = GetPagesWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindPages(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Page> GetPagesWithRootWords(List<Page> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Page> result = new List<Page>();

        if (source != null)
        {
            Dictionary<Page, int> multiplicity_dictionary = new Dictionary<Page, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Page page = word.Verse.Page;
                        if (page != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(page))
                            {
                                multiplicity_dictionary[page]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(page, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // page contains no matches
            {
                foreach (Page page in source)
                {
                    if (page != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(page))
                        {
                            if (!result.Contains(page))
                            {
                                result.Add(page);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Page key in multiplicity_dictionary.Keys)
                {
                    if (
                        ((multiplicity_number_type == NumberType.None) && (multiplicity == -1))
                        ||
                        Compare(multiplicity_dictionary[key], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)
                        )
                    {
                        if (!result.Contains(key))
                        {
                            result.Add(key);
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Station> DoFindStations(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Station> result = new List<Station>();

        if (s_book != null)
        {
            List<Station> source = s_book.GetStations(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Station> temp = DoFindStations(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Station station in temp)
                {
                    if (station != null)
                    {
                        if (!result.Contains(station))
                        {
                            result.Add(station);
                        }
                    }
                }
                source = new List<Station>(result);
            }
        }

        return result;
    }
    private static List<Station> DoFindStations(List<Station> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Station> result = new List<Station>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }
                    if (root_words != null)
                    {
                        result = GetStationsWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindStations(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Station> GetStationsWithRootWords(List<Station> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Station> result = new List<Station>();

        if (source != null)
        {
            Dictionary<Station, int> multiplicity_dictionary = new Dictionary<Station, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Station station = word.Verse.Station;
                        if (station != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(station))
                            {
                                multiplicity_dictionary[station]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(station, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // station contains no matches
            {
                foreach (Station station in source)
                {
                    if (station != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(station))
                        {
                            if (!result.Contains(station))
                            {
                                result.Add(station);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Word word in root_words)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            Station station = word.Verse.Station;
                            if (station != null)
                            {
                                if (source.Contains(station))
                                {
                                    if (multiplicity_dictionary.ContainsKey(station))
                                    {
                                        if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[station], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                        {
                                            if (!result.Contains(station))
                                            {
                                                result.Add(station);
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

        return result;
    }
    private static List<Part> DoFindParts(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Part> result = new List<Part>();

        if (s_book != null)
        {
            List<Part> source = s_book.GetParts(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Part> temp = DoFindParts(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Part part in temp)
                {
                    if (part != null)
                    {
                        if (!result.Contains(part))
                        {
                            result.Add(part);
                        }
                    }
                }
                source = new List<Part>(result);
            }
        }

        return result;
    }
    private static List<Part> DoFindParts(List<Part> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Part> result = new List<Part>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }
                    if (root_words != null)
                    {
                        result = GetPartsWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindParts(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Part> GetPartsWithRootWords(List<Part> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Part> result = new List<Part>();

        if (source != null)
        {
            Dictionary<Part, int> multiplicity_dictionary = new Dictionary<Part, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Part part = word.Verse.Part;
                        if (part != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(part))
                            {
                                multiplicity_dictionary[part]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(part, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // part contains no matches
            {
                foreach (Part part in source)
                {
                    if (part != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(part))
                        {
                            if (!result.Contains(part))
                            {
                                result.Add(part);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Word word in root_words)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            Part part = word.Verse.Part;
                            if (part != null)
                            {
                                if (source.Contains(part))
                                {
                                    if (multiplicity_dictionary.ContainsKey(part))
                                    {
                                        if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[part], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                        {
                                            if (!result.Contains(part))
                                            {
                                                result.Add(part);
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

        return result;
    }
    private static List<Model.Group> DoFindGroups(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Model.Group> result = new List<Model.Group>();

        if (s_book != null)
        {
            List<Model.Group> source = s_book.GetGroups(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Model.Group> temp = DoFindGroups(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Model.Group group in temp)
                {
                    if (group != null)
                    {
                        if (!result.Contains(group))
                        {
                            result.Add(group);
                        }
                    }
                }
                source = new List<Model.Group>(result);
            }
        }

        return result;
    }
    private static List<Model.Group> DoFindGroups(List<Model.Group> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Model.Group> result = new List<Model.Group>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }
                    if (root_words != null)
                    {
                        result = GetGroupsWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindGroups(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Model.Group> GetGroupsWithRootWords(List<Model.Group> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Model.Group> result = new List<Model.Group>();

        if (source != null)
        {
            Dictionary<Model.Group, int> multiplicity_dictionary = new Dictionary<Model.Group, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Model.Group group = word.Verse.Group;
                        if (group != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(group))
                            {
                                multiplicity_dictionary[group]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(group, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // group contains no matches
            {
                foreach (Model.Group group in source)
                {
                    if (group != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(group))
                        {
                            if (!result.Contains(group))
                            {
                                result.Add(group);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Word word in root_words)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            Model.Group group = word.Verse.Group;
                            if (group != null)
                            {
                                if (source.Contains(group))
                                {
                                    if (multiplicity_dictionary.ContainsKey(group))
                                    {
                                        if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[group], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                        {
                                            if (!result.Contains(group))
                                            {
                                                result.Add(group);
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

        return result;
    }
    private static List<Half> DoFindHalfs(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Half> result = new List<Half>();

        if (s_book != null)
        {
            List<Half> source = s_book.GetHalfs(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Half> temp = DoFindHalfs(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Half half in temp)
                {
                    if (half != null)
                    {
                        if (!result.Contains(half))
                        {
                            result.Add(half);
                        }
                    }
                }
                source = new List<Half>(result);
            }
        }

        return result;
    }
    private static List<Half> DoFindHalfs(List<Half> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Half> result = new List<Half>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }
                    if (root_words != null)
                    {
                        result = GetHalfsWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindHalfs(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Half> GetHalfsWithRootWords(List<Half> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Half> result = new List<Half>();

        if (source != null)
        {
            Dictionary<Half, int> multiplicity_dictionary = new Dictionary<Half, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Half half = word.Verse.Half;
                        if (half != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(half))
                            {
                                multiplicity_dictionary[half]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(half, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // half contains no matches
            {
                foreach (Half half in source)
                {
                    if (half != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(half))
                        {
                            if (!result.Contains(half))
                            {
                                result.Add(half);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Word word in root_words)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            Half half = word.Verse.Half;
                            if (half != null)
                            {
                                if (source.Contains(half))
                                {
                                    if (multiplicity_dictionary.ContainsKey(half))
                                    {
                                        if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[half], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                        {
                                            if (!result.Contains(half))
                                            {
                                                result.Add(half);
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

        return result;
    }
    private static List<Quarter> DoFindQuarters(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Quarter> result = new List<Quarter>();

        if (s_book != null)
        {
            List<Quarter> source = s_book.GetQuarters(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Quarter> temp = DoFindQuarters(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Quarter quarter in temp)
                {
                    if (quarter != null)
                    {
                        if (!result.Contains(quarter))
                        {
                            result.Add(quarter);
                        }
                    }
                }
                source = new List<Quarter>(result);
            }
        }

        return result;
    }
    private static List<Quarter> DoFindQuarters(List<Quarter> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Quarter> result = new List<Quarter>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }
                    if (root_words != null)
                    {
                        result = GetQuartersWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindQuarters(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Quarter> GetQuartersWithRootWords(List<Quarter> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Quarter> result = new List<Quarter>();

        if (source != null)
        {
            Dictionary<Quarter, int> multiplicity_dictionary = new Dictionary<Quarter, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Quarter quarter = word.Verse.Quarter;
                        if (quarter != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(quarter))
                            {
                                multiplicity_dictionary[quarter]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(quarter, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // quarter contains no matches
            {
                foreach (Quarter quarter in source)
                {
                    if (quarter != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(quarter))
                        {
                            if (!result.Contains(quarter))
                            {
                                result.Add(quarter);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Word word in root_words)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            Quarter quarter = word.Verse.Quarter;
                            if (quarter != null)
                            {
                                if (source.Contains(quarter))
                                {
                                    if (multiplicity_dictionary.ContainsKey(quarter))
                                    {
                                        if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[quarter], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                        {
                                            if (!result.Contains(quarter))
                                            {
                                                result.Add(quarter);
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

        return result;
    }
    private static List<Bowing> DoFindBowings(List<Verse> verses, string roots, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Bowing> result = new List<Bowing>();

        if (s_book != null)
        {
            List<Bowing> source = s_book.GetBowings(verses);

            if (String.IsNullOrEmpty(roots)) return null;
            while (roots.Contains("  "))
            {
                roots = roots.Replace("  ", " ");
            }
            string[] terms = roots.Split();

            foreach (string term in terms)
            {
                List<Bowing> temp = DoFindBowings(source, term, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                foreach (Bowing bowing in temp)
                {
                    if (bowing != null)
                    {
                        if (!result.Contains(bowing))
                        {
                            result.Add(bowing);
                        }
                    }
                }
                source = new List<Bowing>(result);
            }
        }

        return result;
    }
    private static List<Bowing> DoFindBowings(List<Bowing> source, string root, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Bowing> result = new List<Bowing>();

        if (!String.IsNullOrEmpty(root))
        {
            if (s_book != null)
            {
                SortedDictionary<string, List<Word>> root_words_dictionary = s_book.RootWords;
                if (root_words_dictionary != null)
                {
                    List<Word> root_words = null;
                    if (root_words_dictionary.ContainsKey(root))
                    {
                        // get all pre-identified root_words
                        root_words = root_words_dictionary[root];
                    }
                    if (root_words != null)
                    {
                        result = GetBowingsWithRootWords(source, root_words, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                    else // text is a word, not a root
                    {
                        string best_root = s_book.GetBestRoot(root);
                        result = DoFindBowings(source, best_root, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
                    }
                }
            }
        }

        return result;
    }
    private static List<Bowing> GetBowingsWithRootWords(List<Bowing> source, List<Word> root_words, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Bowing> result = new List<Bowing>();

        if (source != null)
        {
            Dictionary<Bowing, int> multiplicity_dictionary = new Dictionary<Bowing, int>();
            foreach (Word word in root_words)
            {
                if (word != null)
                {
                    if (word.Verse != null)
                    {
                        Bowing bowing = word.Verse.Bowing;
                        if (bowing != null)
                        {
                            if (multiplicity_dictionary.ContainsKey(bowing))
                            {
                                multiplicity_dictionary[bowing]++;
                            }
                            else // first found
                            {
                                multiplicity_dictionary.Add(bowing, 1);
                            }
                        }
                    }
                }
            }

            if (multiplicity == 0) // bowing contains no matches
            {
                foreach (Bowing bowing in source)
                {
                    if (bowing != null)
                    {
                        if (!multiplicity_dictionary.ContainsKey(bowing))
                        {
                            if (!result.Contains(bowing))
                            {
                                result.Add(bowing);
                            }
                        }
                    }
                }
            }
            else // contains wildcard (-1) or exact multiplicity or number_type multiplicity
            {
                foreach (Word word in root_words)
                {
                    if (word != null)
                    {
                        if (word.Verse != null)
                        {
                            Bowing bowing = word.Verse.Bowing;
                            if (bowing != null)
                            {
                                if (source.Contains(bowing))
                                {
                                    if (multiplicity_dictionary.ContainsKey(bowing))
                                    {
                                        if (((multiplicity_number_type == NumberType.None) && (multiplicity == -1)) || (Compare(multiplicity_dictionary[bowing], multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder)))
                                        {
                                            if (!result.Contains(bowing))
                                            {
                                                result.Add(bowing);
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

        return result;
    }
    // find by text - Related verses
    public static List<Verse> FindRelatedVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_result, Verse verse)
    {
        return DoFindRelatedVerses(search_scope, current_selection, previous_result, verse);
    }
    private static List<Verse> DoFindRelatedVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_result, Verse verse)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_result, TextLocationInChapter.Any);
        return DoFindRelatedVerses(source, current_selection, previous_result, verse);
    }
    private static List<Verse> DoFindRelatedVerses(List<Verse> source, Selection current_selection, List<Verse> previous_result, Verse verse)
    {
        List<Verse> result = new List<Verse>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (verse != null)
                {
                    for (int j = 0; j < source.Count; j++)
                    {
                        if (verse.HasRelatedWordsTo(source[j]))
                        {
                            result.Add(source[j]);
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by text - Repetition
    public static List<Word> FindConsecutivelyRepeatedWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool with_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindConsecutivelyRepeatedWords(source, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder, with_diacritics);
    }
    public static List<Word> FindConsecutivelyRepeatedRoots(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindConsecutivelyRepeatedRoots(source, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
    }
    public static List<Word> FindConsecutivelyRepeatedValues(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindConsecutivelyRepeatedValues(source, multiplicity, multiplicity_number_type, multiplicity_comparison_operator, multiplicity_remainder);
    }
    private static List<Word> DoFindConsecutivelyRepeatedWords(List<Verse> source, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder, bool with_diacritics)
    {
        List<Word> result = new List<Word>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (multiplicity > 0)
                {
                    List<Word> words = new List<Word>();
                    foreach (Verse verse in source)
                    {
                        if (verse != null)
                        {
                            words.AddRange(verse.Words);
                        }
                    }

                    for (int i = 0; i < words.Count - 2 * multiplicity; i++)
                    {
                        bool found = true;
                        for (int j = 0; j < multiplicity; j++)
                        {
                            string word_text_i = words[i + j].Text;
                            string word_text_j = words[i + j + multiplicity].Text;

                            if (!with_diacritics)
                            {
                                if ((!with_diacritics) && ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks")))
                                {
                                    word_text_i = word_text_i.Simplify29();
                                    word_text_j = word_text_j.Simplify29();
                                }
                            }
                            if (word_text_i != word_text_j)
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            for (int j = 0; j < 2 * multiplicity; j++)
                            {
                                result.Add(words[i + j]);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Word> DoFindConsecutivelyRepeatedRoots(List<Verse> source, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Word> result = new List<Word>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (multiplicity > 0)
                {
                    List<Word> words = new List<Word>();
                    foreach (Verse verse in source)
                    {
                        if (verse != null)
                        {
                            words.AddRange(verse.Words);
                        }
                    }

                    for (int i = 0; i < words.Count - 2 * multiplicity; i++)
                    {
                        bool found = true;
                        for (int j = 0; j < multiplicity; j++)
                        {
                            string word_text_i = words[i + j].Root;
                            string word_text_j = words[i + j + multiplicity].Root;

                            if (word_text_i != word_text_j)
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            for (int j = 0; j < 2 * multiplicity; j++)
                            {
                                result.Add(words[i + j]);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    private static List<Word> DoFindConsecutivelyRepeatedValues(List<Verse> source, int multiplicity, NumberType multiplicity_number_type, ComparisonOperator multiplicity_comparison_operator, int multiplicity_remainder)
    {
        List<Word> result = new List<Word>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (multiplicity > 0)
                {
                    List<Word> words = new List<Word>();
                    foreach (Verse verse in source)
                    {
                        if (verse != null)
                        {
                            words.AddRange(verse.Words);
                        }
                    }

                    for (int i = 0; i < words.Count - 2 * multiplicity; i++)
                    {
                        bool found = true;
                        for (int j = 0; j < multiplicity; j++)
                        {
                            long word_value_i = CalculateValue(words[i + j], false);
                            long word_value_j = CalculateValue(words[i + j + multiplicity], false);

                            if (word_value_i != word_value_j)
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            for (int j = 0; j < 2 * multiplicity; j++)
                            {
                                result.Add(words[i + j]);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }


    // find by numbers - helper methods
    private static void CalculateSums(Word word, out int letter_sum)
    {
        letter_sum = 0;
        if (word != null)
        {
            if ((word.Letters != null) && (word.Letters.Count > 0))
            {
                foreach (Letter letter in word.Letters)
                {
                    if (letter != null)
                    {
                        letter_sum += letter.NumberInWord;
                    }
                }
            }
        }
    }
    private static void CalculateSums(List<Word> words, out int word_sum, out int letter_sum)
    {
        word_sum = 0;
        letter_sum = 0;
        if (words != null)
        {
            foreach (Word word in words)
            {
                if (word != null)
                {
                    word_sum += word.NumberInVerse;

                    if ((word.Letters != null) && (word.Letters.Count > 0))
                    {
                        foreach (Letter letter in word.Letters)
                        {
                            if (letter != null)
                            {
                                letter_sum += letter.NumberInWord;
                            }
                        }
                    }
                }
            }
        }
    }
    private static void CalculateSums(Verse verse, out int word_sum, out int letter_sum)
    {
        word_sum = 0;
        letter_sum = 0;
        if (verse != null)
        {
            if (verse.Words != null)
            {
                foreach (Word word in verse.Words)
                {
                    if (word != null)
                    {
                        word_sum += word.NumberInVerse;

                        if ((word.Letters != null) && (word.Letters.Count > 0))
                        {
                            foreach (Letter letter in word.Letters)
                            {
                                if (letter != null)
                                {
                                    letter_sum += letter.NumberInWord;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private static void CalculateSums(List<Verse> verses, out int chapter_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        chapter_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (verses != null)
        {
            if (s_book != null)
            {
                List<Chapter> chapters = s_book.GetChapters(verses);
                if (chapters != null)
                {
                    foreach (Chapter chapter in chapters)
                    {
                        if (chapter != null)
                        {
                            chapter_sum += chapter.SortedNumber;
                        }
                    }

                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(Chapter chapter, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (chapter != null)
        {
            foreach (Verse verse in chapter.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(Page page, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (page != null)
        {
            foreach (Verse verse in page.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(Station station, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (station != null)
        {
            foreach (Verse verse in station.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(Part part, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (part != null)
        {
            foreach (Verse verse in part.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(Model.Group group, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (group != null)
        {
            foreach (Verse verse in group.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(Half half, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (half != null)
        {
            foreach (Verse verse in half.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(Quarter quarter, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (quarter != null)
        {
            foreach (Verse verse in quarter.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(Bowing bowing, out int verse_sum, out int word_sum, out int letter_sum)
    {
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (bowing != null)
        {
            foreach (Verse verse in bowing.Verses)
            {
                if (verse != null)
                {
                    verse_sum += verse.NumberInChapter;
                    if (verse.Words != null)
                    {
                        foreach (Word word in verse.Words)
                        {
                            if (word != null)
                            {
                                word_sum += word.NumberInVerse;

                                if ((word.Letters != null) && (word.Letters.Count > 0))
                                {
                                    foreach (Letter letter in word.Letters)
                                    {
                                        if (letter != null)
                                        {
                                            letter_sum += letter.NumberInWord;
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
    private static void CalculateSums(List<Chapter> chapters, out int chapter_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        chapter_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (chapters != null)
        {
            foreach (Chapter chapter in chapters)
            {
                if (chapter != null)
                {
                    chapter_sum += chapter.SortedNumber;

                    foreach (Verse verse in chapter.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(List<Page> pages, out int page_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        page_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (pages != null)
        {
            foreach (Page page in pages)
            {
                if (page != null)
                {
                    page_sum += page.Number;

                    foreach (Verse verse in page.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(List<Station> stations, out int station_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        station_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (stations != null)
        {
            foreach (Station station in stations)
            {
                if (station != null)
                {
                    station_sum += station.Number;

                    foreach (Verse verse in station.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(List<Part> parts, out int part_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        part_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (parts != null)
        {
            foreach (Part part in parts)
            {
                if (part != null)
                {
                    part_sum += part.Number;

                    foreach (Verse verse in part.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(List<Model.Group> groups, out int group_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        group_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (groups != null)
        {
            foreach (Model.Group group in groups)
            {
                if (group != null)
                {
                    group_sum += group.Number;

                    foreach (Verse verse in group.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(List<Half> halfs, out int half_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        half_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (halfs != null)
        {
            foreach (Half half in halfs)
            {
                if (half != null)
                {
                    half_sum += half.Number;

                    foreach (Verse verse in half.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(List<Quarter> quarters, out int quarter_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        quarter_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (quarters != null)
        {
            foreach (Quarter quarter in quarters)
            {
                if (quarter != null)
                {
                    quarter_sum += quarter.Number;

                    foreach (Verse verse in quarter.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static void CalculateSums(List<Bowing> bowings, out int bowing_sum, out int verse_sum, out int word_sum, out int letter_sum)
    {
        bowing_sum = 0;
        verse_sum = 0;
        word_sum = 0;
        letter_sum = 0;
        if (bowings != null)
        {
            foreach (Bowing bowing in bowings)
            {
                if (bowing != null)
                {
                    bowing_sum += bowing.Number;

                    foreach (Verse verse in bowing.Verses)
                    {
                        if (verse != null)
                        {
                            verse_sum += verse.NumberInChapter;
                            if (verse.Words != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        word_sum += word.NumberInVerse;

                                        if ((word.Letters != null) && (word.Letters.Count > 0))
                                        {
                                            foreach (Letter letter in word.Letters)
                                            {
                                                if (letter != null)
                                                {
                                                    letter_sum += letter.NumberInWord;
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
        }
    }
    private static bool Compare(Letter letter, NumberQuery query)
    {
        if (letter != null)
        {
            int number = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    number = letter.Number;
                    break;
                case NumberScope.NumberInChapter:
                    number = letter.NumberInChapter;
                    break;
                case NumberScope.NumberInVerse:
                    number = letter.NumberInVerse;
                    break;
                case NumberScope.NumberInWord:
                    number = letter.NumberInWord;
                    break;
                default:
                    number = letter.NumberInWord;
                    break;
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = letter.Word.Verse.Chapter.Book.Letters.Count + query.Number + 1;
                                break;
                            case NumberScope.NumberInChapter:
                                query.Number = letter.Word.Verse.Chapter.Letters.Count + query.Number + 1;
                                break;
                            case NumberScope.NumberInVerse:
                                query.Number = letter.Word.Verse.Letters.Count + query.Number + 1;
                                break;
                            case NumberScope.NumberInWord:
                                query.Number = letter.Word.Letters.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = letter.Word.Letters.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(letter, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value != 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }

            int frequency = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    frequency = letter.Frequency; break;
                case NumberScope.NumberInChapter:
                    frequency = letter.FrequencyInChapter; break;
                case NumberScope.NumberInVerse:
                    frequency = letter.FrequencyInVerse; break;
                case NumberScope.NumberInWord:
                    frequency = letter.FrequencyInWord; break;
            }
            if (query.FrequencyNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(frequency, number, query.FrequencyComparisonOperator, query.FrequencyRemainder))
                {
                    return false;
                }
            }
            else if (query.FrequencyNumberType == NumberType.None)
            {
                if (query.Frequency != 0)
                {
                    if (!Numbers.Compare(frequency, query.Frequency, query.FrequencyComparisonOperator, query.FrequencyRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(frequency, query.FrequencyNumberType))
                {
                    return false;
                }
            }

            int occurrence = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    occurrence = letter.Occurrence; break;
                case NumberScope.NumberInChapter:
                    occurrence = letter.OccurrenceInChapter; break;
                case NumberScope.NumberInVerse:
                    occurrence = letter.OccurrenceInVerse; break;
                case NumberScope.NumberInWord:
                    occurrence = letter.OccurrenceInWord; break;
            }
            if (query.OccurrenceNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(occurrence, number, query.OccurrenceComparisonOperator, query.OccurrenceRemainder))
                {
                    return false;
                }
            }
            else if (query.OccurrenceNumberType == NumberType.None)
            {
                if (query.Occurrence != 0)
                {
                    if (!Numbers.Compare(occurrence, query.Occurrence, query.OccurrenceComparisonOperator, query.OccurrenceRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(occurrence, query.OccurrenceNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Word word, NumberQuery query)
    {
        if (word != null)
        {
            int number = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    number = word.Number;
                    break;
                case NumberScope.NumberInChapter:
                    number = word.NumberInChapter;
                    break;
                case NumberScope.NumberInVerse:
                    number = word.NumberInVerse;
                    break;
                default:
                    number = word.NumberInVerse;
                    break;
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = word.Verse.Chapter.Book.Words.Count + query.Number + 1;
                                break;
                            case NumberScope.NumberInChapter:
                                query.Number = word.Verse.Chapter.Words.Count + query.Number + 1;
                                break;
                            case NumberScope.NumberInVerse:
                                query.Number = word.Verse.Words.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = word.Verse.Words.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int letter_sum;
                        CalculateSums(word, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(word.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int letter_sum;
                    CalculateSums(word, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(word.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(word.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(word.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(word, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value != 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }

            int frequency = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    frequency = word.Frequency; break;
                case NumberScope.NumberInChapter:
                    frequency = word.FrequencyInChapter; break;
                case NumberScope.NumberInVerse:
                    frequency = word.FrequencyInVerse; break;
            }
            if (query.FrequencyNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(frequency, number, query.FrequencyComparisonOperator, query.FrequencyRemainder))
                {
                    return false;
                }
            }
            else if (query.FrequencyNumberType == NumberType.None)
            {
                if (query.Frequency != 0)
                {
                    if (!Numbers.Compare(frequency, query.Frequency, query.FrequencyComparisonOperator, query.FrequencyRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(frequency, query.FrequencyNumberType))
                {
                    return false;
                }
            }

            int occurrence = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    occurrence = word.Occurrence; break;
                case NumberScope.NumberInChapter:
                    occurrence = word.OccurrenceInChapter; break;
                case NumberScope.NumberInVerse:
                    occurrence = word.OccurrenceInVerse; break;
            }
            if (query.OccurrenceNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(occurrence, number, query.OccurrenceComparisonOperator, query.OccurrenceRemainder))
                {
                    return false;
                }
            }
            else if (query.OccurrenceNumberType == NumberType.None)
            {
                if (query.Occurrence != 0)
                {
                    if (!Numbers.Compare(occurrence, query.Occurrence, query.OccurrenceComparisonOperator, query.OccurrenceRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(occurrence, query.OccurrenceNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Sentence sentence, NumberQuery query)
    {
        if (sentence != null)
        {
            long value = 0L;

            if (query.WordCountNumberType == NumberType.Natural)
            {
                return false;
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (!Numbers.Compare(sentence.WordCount, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sentence.WordCount, query.WordCountNumberType))
                {
                    return false;
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                return false;
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (!Numbers.Compare(sentence.LetterCount, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sentence.LetterCount, query.LetterCountNumberType))
                {
                    return false;
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                return false;
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(sentence.UniqueLetterCount, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sentence.UniqueLetterCount, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (query.ValueNumberType == NumberType.Natural)
            {
                return false;
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (value == 0L)
                    { value = CalculateValue(sentence, false); }
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (value == 0L)
                { value = CalculateValue(sentence, false); }
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Verse verse, NumberQuery query)
    {
        if (verse != null)
        {
            int number = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    number = verse.Number;
                    break;
                case NumberScope.NumberInChapter:
                    number = verse.NumberInChapter;
                    break;
                default:
                    number = verse.NumberInChapter;
                    break;
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = verse.Book.Verses.Count + query.Number + 1;
                                break;
                            case NumberScope.NumberInChapter:
                                query.Number = verse.Chapter.Verses.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = verse.Chapter.Verses.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int word_sum;
                        int letter_sum;
                        CalculateSums(verse, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(verse.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int word_sum;
                    int letter_sum;
                    CalculateSums(verse, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(verse.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int word_sum;
                        int letter_sum;
                        CalculateSums(verse, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(verse.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int word_sum;
                    int letter_sum;
                    CalculateSums(verse, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(verse.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(verse.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(verse.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(verse, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }

            int frequency = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    frequency = verse.Frequency; break;
                case NumberScope.NumberInChapter:
                    frequency = verse.FrequencyInChapter; break;
            }
            if (query.FrequencyNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(frequency, number, query.FrequencyComparisonOperator, query.FrequencyRemainder))
                {
                    return false;
                }
            }
            else if (query.FrequencyNumberType == NumberType.None)
            {
                if (query.Frequency > 0)
                {
                    if (!Numbers.Compare(frequency, query.Frequency, query.FrequencyComparisonOperator, query.FrequencyRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(frequency, query.FrequencyNumberType))
                {
                    return false;
                }
            }

            int occurrence = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    occurrence = verse.Occurrence; break;
                case NumberScope.NumberInChapter:
                    occurrence = verse.OccurrenceInChapter; break;
            }
            if (query.OccurrenceNumberType == NumberType.Natural)
            {
                if (query.Occurrence > 0)
                {
                    if (!Numbers.Compare(occurrence, number, query.OccurrenceComparisonOperator, query.OccurrenceRemainder))
                    {
                        return false;
                    }
                }
            }
            else if (query.OccurrenceNumberType == NumberType.None)
            {
                if (query.Occurrence > 0)
                {
                    if (!Numbers.Compare(occurrence, query.Occurrence, query.OccurrenceComparisonOperator, query.OccurrenceRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(occurrence, query.OccurrenceNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Chapter chapter, NumberQuery query)
    {
        if (chapter != null)
        {
            int number = chapter.SortedNumber;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Chapters.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Chapters.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(chapter.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(chapter, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(chapter.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(chapter, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(chapter.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(chapter.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(chapter, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(chapter.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(chapter, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(chapter.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(chapter.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(chapter, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(chapter.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(chapter, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(chapter.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(chapter.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(chapter.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(chapter.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(chapter, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Page page, NumberQuery query)
    {
        if (page != null)
        {
            int number = page.Number;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Pages.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Pages.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(page.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(page, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(page.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(page, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(page.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(page.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(page, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(page.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(page, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(page.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(page.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(page, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(page.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(page, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(page.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(page.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(page.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(page.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(page.Verses, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Station station, NumberQuery query)
    {
        if (station != null)
        {
            int number = station.Number;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Stations.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Stations.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(station.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(station, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(station.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(station, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(station.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(station.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(station, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(station.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(station, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(station.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(station.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(station, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(station.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(station, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(station.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(station.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(station.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(station.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(station.Verses, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Part part, NumberQuery query)
    {
        if (part != null)
        {
            int number = part.Number;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Parts.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Parts.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(part.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(part, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(part.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(part, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(part.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(part.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(part, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(part.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(part, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(part.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(part.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(part, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(part.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(part, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(part.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(part.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(part.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(part.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(part.Verses, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Model.Group group, NumberQuery query)
    {
        if (group != null)
        {
            int number = group.Number;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Groups.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Groups.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(group.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(group, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(group.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(group, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(group.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(group.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(group, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(group.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(group, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(group.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(group.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(group, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(group.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(group, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(group.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(group.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(group.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(group.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(group.Verses, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Half half, NumberQuery query)
    {
        if (half != null)
        {
            int number = half.Number;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Halfs.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Halfs.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(half.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(half, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(half.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(half, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(half.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(half.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(half, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(half.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(half, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(half.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(half.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(half, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(half.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(half, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(half.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(half.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(half.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(half.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(half.Verses, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Quarter quarter, NumberQuery query)
    {
        if (quarter != null)
        {
            int number = quarter.Number;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Quarters.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Quarters.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(quarter.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(quarter, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(quarter.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(quarter, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(quarter.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(quarter.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(quarter, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(quarter.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(quarter, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(quarter.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(quarter.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(quarter, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(quarter.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(quarter, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(quarter.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(quarter.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(quarter.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(quarter.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(quarter.Verses, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(Bowing bowing, NumberQuery query)
    {
        if (bowing != null)
        {
            int number = bowing.Number;
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(number, number, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number != 0)
                {
                    if (query.Number < 0)
                    {
                        switch (query.NumberScope)
                        {
                            case NumberScope.Number:
                                query.Number = Book.Bowings.Count + query.Number + 1;
                                break;
                            default:
                                query.Number = Book.Bowings.Count + query.Number + 1;
                                break;
                        }
                    }

                    if (query.Number < 0)
                    {
                        query.Number = number + query.Number + 1;
                    }

                    if (query.Number > 0)
                    {
                        if (!Numbers.Compare(number, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false; // number_out_of_range
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(number, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(bowing.Verses.Count, number, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(bowing, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(bowing.Verses.Count, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(bowing, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(bowing.Verses.Count, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(bowing.Words.Count, number, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(bowing, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(bowing.Words.Count, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(bowing, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(bowing.Words.Count, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(bowing.Letters.Count, number, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(bowing, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(bowing.Letters.Count, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(bowing, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(bowing.Letters.Count, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(bowing.UniqueLetters.Count, number, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(bowing.UniqueLetters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(bowing.UniqueLetters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            long value = CalculateValue(bowing.Verses, false);
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, number, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Letter> letters, NumberQuery query)
    {
        if (letters != null)
        {
            long value = 0L;

            int sum = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    foreach (Letter letter in letters)
                    {
                        if (letter != null)
                        {
                            sum += letter.Number;
                        }
                    }
                    break;
                case NumberScope.NumberInChapter:
                    foreach (Letter letter in letters)
                    {
                        if (letter != null)
                        {
                            sum += letter.NumberInChapter;
                        }
                    }
                    break;
                case NumberScope.NumberInVerse:
                    foreach (Letter letter in letters)
                    {
                        if (letter != null)
                        {
                            sum += letter.NumberInVerse;
                        }
                    }
                    break;
                case NumberScope.NumberInWord:
                    foreach (Letter letter in letters)
                    {
                        if (letter != null)
                        {
                            sum += letter.NumberInWord;
                        }
                    }
                    break;
                default:
                    foreach (Letter letter in letters)
                    {
                        if (letter != null)
                        {
                            sum += letter.NumberInVerse;
                        }
                    }
                    break;
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Letter letter in letters)
                {
                    if (letter != null)
                    {
                        value += CalculateValue(letter, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Word> words, NumberQuery query)
    {
        if (words != null)
        {
            long value = 0L;

            int sum = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    foreach (Word word in words)
                    {
                        if (word != null)
                        {
                            sum += word.Number;
                        }
                    }
                    break;
                case NumberScope.NumberInChapter:
                    foreach (Word word in words)
                    {
                        if (word != null)
                        {
                            sum += word.NumberInChapter;
                        }
                    }
                    break;
                case NumberScope.NumberInVerse:
                    foreach (Word word in words)
                    {
                        if (word != null)
                        {
                            sum += word.NumberInVerse;
                        }
                    }
                    break;
                default:
                    foreach (Word word in words)
                    {
                        if (word != null)
                        {
                            sum += word.NumberInVerse;
                        }
                    }
                    break;
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            sum = 0;
            foreach (Word word in words)
            {
                if (word != null)
                {
                    sum += word.Letters.Count;
                }
            }
            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int word_sum;
                        int letter_sum;
                        CalculateSums(words, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int word_sum;
                    int letter_sum;
                    CalculateSums(words, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Word word in words)
            {
                if (word != null)
                {
                    foreach (char character in word.UniqueLetters)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Word word in words)
                {
                    if (word != null)
                    {
                        value += CalculateValue(word, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Verse> verses, NumberQuery query)
    {
        if (verses != null)
        {
            long value = 0L;

            int sum = 0;
            switch (query.NumberScope)
            {
                case NumberScope.Number:
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            sum += verse.Number;
                        }
                    }
                    break;
                case NumberScope.NumberInChapter:
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            sum += verse.NumberInChapter;
                        }
                    }
                    break;
                default:
                    foreach (Verse verse in verses)
                    {
                        if (verse != null)
                        {
                            sum += verse.NumberInChapter;
                        }
                    }
                    break;
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            sum = 0;
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    sum += verse.Words.Count;
                }
            }
            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int chapter_sum;
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(verses, out chapter_sum, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int chapter_sum;
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(verses, out chapter_sum, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            sum = 0;
            foreach (Verse verse in verses)
            {
                if (verse != null)
                {
                    sum += verse.Letters.Count;
                }
            }
            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        int chapter_sum;
                        int verse_sum;
                        int word_sum;
                        int letter_sum;
                        CalculateSums(verses, out chapter_sum, out verse_sum, out word_sum, out letter_sum);
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    int chapter_sum;
                    int verse_sum;
                    int word_sum;
                    int letter_sum;
                    CalculateSums(verses, out chapter_sum, out verse_sum, out word_sum, out letter_sum);
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Verse verse in verses)
            {
                foreach (char character in verse.UniqueLetters)
                {
                    if (verse != null)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Verse verse in verses)
                {
                    if (verse != null)
                    {
                        value += CalculateValue(verse, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Chapter> chapters, NumberQuery query)
    {
        if (chapters != null)
        {
            int chapter_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(chapters, out chapter_sum, out verse_sum, out word_sum, out letter_sum);

            long value = 0L;
            int sum = 0;
            foreach (Chapter chapter in chapters)
            {
                if (chapter != null)
                {
                    sum += chapter.SortedNumber;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            sum = 0;
            foreach (Chapter chapter in chapters)
            {
                if (chapter != null)
                {
                    sum += chapter.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            sum = 0;
            foreach (Chapter chapter in chapters)
            {
                if (chapter != null)
                {
                    sum += chapter.Words.Count;
                }
            }
            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            sum = 0;
            foreach (Chapter chapter in chapters)
            {
                if (chapter != null)
                {
                    sum += chapter.Letters.Count;
                }
            }
            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Chapter chapter in chapters)
                if (chapter != null)
                {
                    {
                        foreach (char character in chapter.UniqueLetters)
                        {
                            if (!unique_letters.Contains(character))
                            {
                                unique_letters.Add(character);
                            }
                        }
                    }
                }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Chapter chapter in chapters)
                {
                    if (chapter != null)
                    {
                        value += CalculateValue(chapter, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Page> pages, NumberQuery query)
    {
        if (pages != null)
        {
            int page_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(pages, out page_sum, out verse_sum, out word_sum, out letter_sum);

            long value = 0L;
            int sum = 0;
            foreach (Page page in pages)
            {
                if (page != null)
                {
                    sum += page.Number;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            sum = 0;
            foreach (Page page in pages)
            {
                if (page != null)
                {
                    sum += page.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Page page in pages)
            {
                if (page != null)
                {
                    foreach (char character in page.UniqueLetters)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Page page in pages)
                {
                    if (page != null)
                    {
                        value += CalculateValue(page.Verses, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Station> stations, NumberQuery query)
    {
        if (stations != null)
        {
            long value = 0L;
            int sum = 0;
            foreach (Station station in stations)
            {
                if (station != null)
                {
                    sum += station.Number;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            int station_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(stations, out station_sum, out verse_sum, out word_sum, out letter_sum);
            sum = 0;
            foreach (Station station in stations)
            {
                if (station != null)
                {
                    sum += station.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Station station in stations)
            {
                if (station != null)
                {
                    foreach (char character in station.UniqueLetters)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Station station in stations)
                {
                    if (station != null)
                    {
                        value += CalculateValue(station.Verses, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Part> parts, NumberQuery query)
    {
        if (parts != null)
        {
            long value = 0L;
            int sum = 0;
            foreach (Part part in parts)
            {
                if (part != null)
                {
                    sum += part.Number;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            int part_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(parts, out part_sum, out verse_sum, out word_sum, out letter_sum);
            sum = 0;
            foreach (Part part in parts)
            {
                if (part != null)
                {
                    sum += part.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Part part in parts)
            {
                if (part != null)
                {
                    foreach (char character in part.UniqueLetters)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Part part in parts)
                {
                    if (part != null)
                    {
                        value += CalculateValue(part.Verses, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Model.Group> groups, NumberQuery query)
    {
        if (groups != null)
        {
            long value = 0L;
            int sum = 0;
            foreach (Model.Group group in groups)
            {
                if (group != null)
                {
                    sum += group.Number;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            int group_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(groups, out group_sum, out verse_sum, out word_sum, out letter_sum);
            sum = 0;
            foreach (Model.Group group in groups)
            {
                if (group != null)
                {
                    sum += group.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Model.Group group in groups)
            {
                foreach (char character in group.UniqueLetters)
                {
                    if (group != null)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Model.Group group in groups)
                {
                    if (group != null)
                    {
                        value += CalculateValue(group.Verses, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Half> halfs, NumberQuery query)
    {
        if (halfs != null)
        {
            long value = 0L;
            int sum = 0;
            foreach (Half half in halfs)
            {
                if (half != null)
                {
                    sum += half.Number;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            int half_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(halfs, out half_sum, out verse_sum, out word_sum, out letter_sum);
            sum = 0;
            foreach (Half half in halfs)
            {
                if (half != null)
                {
                    sum += half.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Half half in halfs)
            {
                if (half != null)
                {
                    foreach (char character in half.UniqueLetters)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Half half in halfs)
                {
                    if (half != null)
                    {
                        value += CalculateValue(half.Verses, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Quarter> quarters, NumberQuery query)
    {
        if (quarters != null)
        {
            long value = 0L;
            int sum = 0;
            foreach (Quarter quarter in quarters)
            {
                if (quarter != null)
                {
                    sum += quarter.Number;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            int quarter_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(quarters, out quarter_sum, out verse_sum, out word_sum, out letter_sum);
            sum = 0;
            foreach (Quarter quarter in quarters)
            {
                if (quarter != null)
                {
                    sum += quarter.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Quarter quarter in quarters)
            {
                if (quarter != null)
                {
                    foreach (char character in quarter.UniqueLetters)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Quarter quarter in quarters)
                {
                    if (quarter != null)
                    {
                        value += CalculateValue(quarter.Verses, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(List<Bowing> bowings, NumberQuery query)
    {
        if (bowings != null)
        {
            long value = 0L;
            int sum = 0;
            foreach (Bowing bowing in bowings)
            {
                if (bowing != null)
                {
                    sum += bowing.Number;
                }
            }
            if (query.NumberNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(sum, sum, query.NumberComparisonOperator, query.NumberRemainder))
                {
                    return false;
                }
            }
            else if (query.NumberNumberType == NumberType.None)
            {
                if (query.Number > 0)
                {
                    if (!Numbers.Compare(sum, query.Number, query.NumberComparisonOperator, query.NumberRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(sum, query.NumberNumberType))
                {
                    return false;
                }
            }

            int bowing_sum;
            int verse_sum;
            int word_sum;
            int letter_sum;
            CalculateSums(bowings, out bowing_sum, out verse_sum, out word_sum, out letter_sum);
            sum = 0;
            foreach (Bowing bowing in bowings)
            {
                if (bowing != null)
                {
                    sum += bowing.Verses.Count;
                }
            }
            if (query.VerseCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(verse_sum, sum, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                {
                    return false;
                }
            }
            else if (query.VerseCountNumberType == NumberType.None)
            {
                if (query.VerseCount > 0)
                {
                    if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(verse_sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.VerseCount, query.VerseCountComparisonOperator, query.VerseCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.VerseCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(verse_sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.VerseCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.WordCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(word_sum, sum, query.WordCountComparisonOperator, query.WordCountRemainder))
                {
                    return false;
                }
            }
            else if (query.WordCountNumberType == NumberType.None)
            {
                if (query.WordCount > 0)
                {
                    if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(word_sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.WordCount, query.WordCountComparisonOperator, query.WordCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.WordCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(word_sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.WordCountNumberType))
                    {
                        return false;
                    }
                }
            }

            if (query.LetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(letter_sum, sum, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.LetterCountNumberType == NumberType.None)
            {
                if (query.LetterCount > 0)
                {
                    if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                    {
                        if (!Numbers.Compare(letter_sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Numbers.Compare(sum, query.LetterCount, query.LetterCountComparisonOperator, query.LetterCountRemainder))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                if (query.LetterCountComparisonOperator == ComparisonOperator.EqualSum)
                {
                    if (!Numbers.IsNumberType(letter_sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Numbers.IsNumberType(sum, query.LetterCountNumberType))
                    {
                        return false;
                    }
                }
            }

            List<char> unique_letters = new List<char>();
            foreach (Bowing bowing in bowings)
            {
                if (bowing != null)
                {
                    foreach (char character in bowing.UniqueLetters)
                    {
                        if (!unique_letters.Contains(character))
                        {
                            unique_letters.Add(character);
                        }
                    }
                }
            }
            if (query.UniqueLetterCountNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(unique_letters.Count, sum, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                {
                    return false;
                }
            }
            else if (query.UniqueLetterCountNumberType == NumberType.None)
            {
                if (query.UniqueLetterCount > 0)
                {
                    if (!Numbers.Compare(unique_letters.Count, query.UniqueLetterCount, query.UniqueLetterCountComparisonOperator, query.UniqueLetterCountRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(unique_letters.Count, query.UniqueLetterCountNumberType))
                {
                    return false;
                }
            }

            if (value == 0L)
            {
                foreach (Bowing bowing in bowings)
                {
                    if (bowing != null)
                    {
                        value += CalculateValue(bowing.Verses, false);
                    }
                }
            }
            if (query.ValueNumberType == NumberType.Natural)
            {
                if (!Numbers.Compare(value, sum, query.ValueComparisonOperator, query.ValueRemainder))
                {
                    return false;
                }
            }
            else if (query.ValueNumberType == NumberType.None)
            {
                if (query.Value > 0)
                {
                    if (!Numbers.Compare(value, query.Value, query.ValueComparisonOperator, query.ValueRemainder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!Numbers.IsNumberType(value, query.ValueNumberType))
                {
                    return false;
                }
            }
        }

        // passed all tests successfully
        return true;
    }
    private static bool Compare(int number1, int number2, NumberType number_type, ComparisonOperator comparison_operator, int remainder)
    {
        if ((number_type == NumberType.None) || (number_type == NumberType.Natural))
        {
            if (Numbers.Compare(number1, number2, comparison_operator, remainder))
            {
                return true;
            }
        }
        else
        {
            if (Numbers.IsNumberType(number1, number_type))
            {
                return true;
            }
        }

        return false;
    }

    // find by numbers - Letters
    public static List<Letter> FindLetters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindLetters(search_scope, current_selection, previous_verses, query);
    }
    private static List<Letter> DoFindLetters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindLetters(source, query);
    }
    private static List<Letter> DoFindLetters(List<Verse> source, NumberQuery query)
    {
        List<Letter> result = new List<Letter>();

        if (source != null)
        {
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word != null)
                        {
                            foreach (Letter letter in word.Letters)
                            {
                                if (Compare(letter, query))
                                {
                                    result.Add(letter);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - LetterRanges
    public static List<List<Letter>> FindLetterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindLetterRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Letter>> DoFindLetterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindLetterRanges(source, query);
    }
    private static List<List<Letter>> DoFindLetterRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Letter>> result = new List<List<Letter>>();

        if (source != null)
        {
            List<Letter> letters = new List<Letter>();
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word != null)
                        {
                            letters.AddRange(word.Letters);
                        }
                    }
                }
            }

            int range_length = query.LetterCount;
            if (range_length == 1)
            {
                result.Add(DoFindLetters(source, query));
            }
            else if (range_length == 0) // non-specified range length
            {
                for (int r = 1; r <= 29; r++) // try all possible range lengths
                {
                    for (int i = 0; i <= letters.Count - r; i++)
                    {
                        // build required range
                        List<Letter> range = new List<Letter>();
                        for (int j = i; j < i + r; j++)
                        {
                            range.Add(letters[j]);
                        }

                        // check range
                        if (Compare(range, query))
                        {
                            result.Add(range);
                        }
                    }
                }
            }
            else // specified range length
            {
                int r = range_length;
                for (int i = 0; i <= letters.Count - r; i++)
                {
                    // build required range
                    List<Letter> range = new List<Letter>();
                    for (int j = i; j < i + r; j++)
                    {
                        range.Add(letters[j]);
                    }

                    // check range
                    if (Compare(range, query))
                    {
                        result.Add(range);
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - LetterSets
    public static List<List<Letter>> FindLetterSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindLetterSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Letter>> DoFindLetterSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindLetterSets(source, query);
    }
    private static List<List<Letter>> DoFindLetterSets(List<Verse> source, NumberQuery query)
    {
        List<List<Letter>> result = new List<List<Letter>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Letter> letters = new List<Letter>();
                    if (letters != null)
                    {
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        letters.AddRange(word.Letters);
                                    }
                                }
                            }
                        }

                        int set_size = query.LetterCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindLetters(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            for (int i = 0; i < 29; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Letter> sets = new Combinations<Letter>(letters, size, GenerateOption.WithoutRepetition);
                                foreach (List<Letter> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Letter> sets = new Combinations<Letter>(letters, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Letter> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Words
    public static List<Word> FindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindWords(search_scope, current_selection, previous_verses, query);
    }
    private static List<Word> DoFindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindWords(source, query);
    }
    private static List<Word> DoFindWords(List<Verse> source, NumberQuery query)
    {
        List<Word> result = new List<Word>();

        if (source != null)
        {
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    foreach (Word word in verse.Words)
                    {
                        if (word != null)
                        {
                            if (Compare(word, query))
                            {
                                result.Add(word);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - WordRanges
    public static List<List<Word>> FindWordRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindWordRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Word>> DoFindWordRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindWordRanges(source, query);
    }
    private static List<List<Word>> DoFindWordRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Word>> result = new List<List<Word>>();

        if (source != null)
        {
            List<Word> words = new List<Word>();
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    words.AddRange(verse.Words);
                }
            }

            int range_length = query.WordCount;
            if (range_length == 1)
            {
                result.Add(DoFindWords(source, query));
            }
            else if (range_length == 0) // non-specified range length
            {
                for (int r = 1; r <= 29; r++) // try all possible range lengths
                {
                    for (int i = 0; i <= words.Count - r; i++)
                    {
                        // build required range
                        List<Word> range = new List<Word>();
                        for (int j = i; j < i + r; j++)
                        {
                            range.Add(words[j]);
                        }

                        // check range
                        if (Compare(range, query))
                        {
                            result.Add(range);
                        }
                    }
                }
            }
            else // specified range length
            {
                int r = range_length;
                for (int i = 0; i <= words.Count - r; i++)
                {
                    // build required range
                    List<Word> range = new List<Word>();
                    for (int j = i; j < i + r; j++)
                    {
                        range.Add(words[j]);
                    }

                    // check range
                    if (Compare(range, query))
                    {
                        result.Add(range);
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - WordSets
    public static List<List<Word>> FindWordSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindWordSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Word>> DoFindWordSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindWordSets(source, query);
    }
    private static List<List<Word>> DoFindWordSets(List<Verse> source, NumberQuery query)
    {
        List<List<Word>> result = new List<List<Word>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Word> words = new List<Word>();
                    if (words != null)
                    {
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                words.AddRange(verse.Words);
                            }
                        }

                        int set_size = query.WordCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindWords(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            for (int i = 0; i < 29; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Word> sets = new Combinations<Word>(words, size, GenerateOption.WithoutRepetition);
                                foreach (List<Word> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Word> sets = new Combinations<Word>(words, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Word> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Sentences
    public static List<Sentence> FindSentences(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Sentence> result = new List<Sentence>();

        List<Sentence> sentences = DoFindSentences(search_scope, current_selection, previous_verses, query);
        List<Sentence> verse_sentences = new List<Sentence>();

        List<Verse> verses = FindVerses(search_scope, current_selection, previous_verses, query);
        if (verses != null)
        {
            foreach (Verse v in verses)
            {
                if (v != null)
                {
                    bool found = false;
                    foreach (Sentence s in sentences)
                    {
                        if (s != null)
                        {
                            if (s.Text == v.Text)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        verse_sentences.Add(new Sentence(v, 0, v, v.Text.Length - 1, v.Text));
                    }
                }
            }
        }

        result.AddRange(verse_sentences);
        result.AddRange(sentences);

        return result;
    }
    private static List<Sentence> DoFindSentences(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindSentences(source, query);
    }
    private static List<Sentence> DoFindSentences(List<Verse> source, NumberQuery query)
    {
        List<Sentence> result = new List<Sentence>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                List<Word> words = new List<Word>();
                foreach (Verse verse in source)
                {
                    if (verse != null)
                    {
                        words.AddRange(verse.Words);
                    }
                }

                if (s_numerical_system != null)
                {
                    // scan linearly for sequence of words with total Text matching query
                    bool done_MustContinue = false;
                    for (int i = 0; i < words.Count - 1; i++)
                    {
                        StringBuilder str = new StringBuilder();

                        // start building word sequence
                        str.Append(words[i].Text);

                        string stopmark_text = StopmarkHelper.GetStopmarkText(words[i].Stopmark);

                        // 1-word sentence
                        if (
                             (words[i].Stopmark != Stopmark.None) &&
                             (words[i].Stopmark != Stopmark.CanStopAtEither) &&
                             (words[i].Stopmark != Stopmark.MustPause) //&&
                                                                       //(words[i].Stopmark != Stopmark.MustContinue)
                           )
                        {
                            Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[i].Verse, words[i].Position + words[i].Text.Length, str.ToString());
                            if (sentence != null)
                            {
                                if (Compare(sentence, query))
                                {
                                    result.Add(sentence);
                                }
                            }
                        }
                        else // multi-word sentence
                        {
                            // mark the start of 1-to-m MustContinue stopmarks
                            int backup_i = i;

                            // continue building with next words until a stopmark
                            bool done_CanStopAtEither = false;
                            for (int j = i + 1; j < words.Count; j++)
                            {
                                str.Append(" " + words[j].Text);

                                if (words[j].Stopmark == Stopmark.None)
                                {
                                    continue; // continue building longer senetence
                                }
                                else // there is a real stopmark
                                {
                                    if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                    {
                                        str.Append(" " + stopmark_text);
                                    }

                                    if (words[j].Stopmark == Stopmark.MustContinue)
                                    {
                                        // TEST Stopmark.MustContinue
                                        //----1 2 3 4 sentences
                                        //1268
                                        //4153
                                        //1799
                                        //2973
                                        //----1 12 123 1234 sentences
                                        //1268
                                        //5421
                                        //7220
                                        //10193
                                        //-------------
                                        //ERRORS
                                        //# duplicate 1
                                        //# short str
                                        //  in 123 1234
                                        //-------------
                                        //// not needed yet
                                        //// multi-mid sentences
                                        //5952
                                        //4772

                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            if (Compare(sentence, query))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        if (done_MustContinue)
                                        {
                                            done_MustContinue = false;
                                            continue; // get all overlapping long sentence
                                        }

                                        StringBuilder k_str = new StringBuilder();
                                        for (int k = j + 1; k < words.Count; k++)
                                        {
                                            k_str.Append(words[k].Text + " ");

                                            if (words[k].Stopmark == Stopmark.None)
                                            {
                                                continue; // next k
                                            }
                                            else // there is a stopmark
                                            {
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    stopmark_text = StopmarkHelper.GetStopmarkText(words[k].Stopmark);
                                                    k_str.Append(stopmark_text + " ");
                                                }
                                                if (k_str.Length > 0)
                                                {
                                                    k_str.Remove(k_str.Length - 1, 1);
                                                }

                                                sentence = new Sentence(words[j + 1].Verse, words[j + 1].Position, words[k].Verse, words[k].Position + words[k].Text.Length, k_str.ToString());
                                                if (sentence != null)
                                                {
                                                    if (Compare(sentence, query))
                                                    {
                                                        result.Add(sentence);
                                                    }
                                                }

                                                if (
                                                     (words[k].Stopmark == Stopmark.ShouldContinue) ||
                                                     (words[k].Stopmark == Stopmark.CanStop) ||
                                                     (words[k].Stopmark == Stopmark.ShouldStop)
                                                   )
                                                {
                                                    done_MustContinue = true;   // restart from beginning skipping any MustContinue
                                                }
                                                else
                                                {
                                                    done_MustContinue = false;   // keep building ever-longer multi-MustContinue sentence
                                                }

                                                j = k;
                                                break; // next j
                                            }
                                        }

                                        if (done_MustContinue)
                                        {
                                            i = backup_i - 1;  // start new sentence from beginning
                                            break; // next i
                                        }
                                        else
                                        {
                                            continue; // next j
                                        }
                                    }
                                    else if (
                                         (words[j].Stopmark == Stopmark.ShouldContinue) ||
                                         (words[j].Stopmark == Stopmark.CanStop) ||
                                         (words[j].Stopmark == Stopmark.ShouldStop)
                                       )
                                    {
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            if (Compare(sentence, query))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        i = j; // start new sentence after j
                                        break; // next i
                                    }
                                    else if (words[j].Stopmark == Stopmark.MustPause)
                                    {
                                        if (
                                             (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَنْ".Simplify(s_numerical_system.TextMode)) ||
                                             (words[j].Text.Simplify(s_numerical_system.TextMode) == "بَلْ".Simplify(s_numerical_system.TextMode))
                                           )
                                        {
                                            continue; // continue building longer senetence
                                        }
                                        else if (
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "عِوَجَا".Simplify(s_numerical_system.TextMode)) ||
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَّرْقَدِنَا".Simplify(s_numerical_system.TextMode)) ||
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَالِيَهْ".Simplify(s_numerical_system.TextMode))
                                                )
                                        {
                                            Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                            if (sentence != null)
                                            {
                                                if (Compare(sentence, query))
                                                {
                                                    result.Add(sentence);
                                                }
                                            }

                                            i = j; // start new sentence after j
                                            break; // next i
                                        }
                                        else // unknown case
                                        {
                                            throw new Exception("Unknown paused Quran word");
                                        }
                                    }
                                    // first CanStopAtEither found at j
                                    else if ((!done_CanStopAtEither) && (words[j].Stopmark == Stopmark.CanStopAtEither))
                                    {
                                        // ^ ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            if (Compare(sentence, query))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        int kk = -1; // start after ^ (e.g. هُدًۭى)
                                        StringBuilder kk_str = new StringBuilder();
                                        StringBuilder kkk_str = new StringBuilder();
                                        for (int k = j + 1; k < words.Count; k++)
                                        {
                                            str.Append(" " + words[k].Text);
                                            if (kkk_str.Length > 0) // skip first k loop
                                            {
                                                kk_str.Append(" " + words[k].Text);
                                            }
                                            kkk_str.Append(" " + words[k].Text);

                                            if (words[k].Stopmark == Stopmark.None)
                                            {
                                                continue; // next k
                                            }
                                            else // there is a stopmark
                                            {
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    str.Append(" " + stopmark_text);
                                                    if (kk_str.Length > 0)
                                                    {
                                                        kk_str.Append(" " + stopmark_text);
                                                    }
                                                    kkk_str.Append(" " + stopmark_text);
                                                }

                                                // second CanStopAtEither found at k
                                                if (words[k].Stopmark == Stopmark.CanStopAtEither)
                                                {
                                                    // ^ ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ ۛ^ فِيهِ
                                                    sentence = new Sentence(words[i].Verse, words[i].Position, words[k].Verse, words[k].Position + words[k].Text.Length, str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        if (Compare(sentence, query))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    kk = k + 1; // backup k after second ^
                                                    continue; // next k
                                                }
                                                else // non-CanStopAtEither stopmark
                                                {
                                                    // kkk_str   فِيهِ ۛ^ هُدًۭى لِّلْمُتَّقِينَ
                                                    sentence = new Sentence(words[j + 1].Verse, words[j + 1].Position, words[k].Verse, words[k].Position + words[k].Text.Length, kkk_str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        if (Compare(sentence, query))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    // kk_str   هُدًۭى لِّلْمُتَّقِينَ
                                                    sentence = new Sentence(words[kk].Verse, words[kk].Position, words[k].Verse, words[k].Position + words[k].Text.Length, kk_str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        if (Compare(sentence, query))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    // skip the whole surrounding non-CanStopAtEither sentence
                                                    j = k;
                                                    break; // next j
                                                }
                                            }
                                        }

                                        // restart from last
                                        str.Length = 0;
                                        j = i - 1; // will be j++ by reloop
                                        done_CanStopAtEither = true;
                                    }
                                    else if (words[j].Stopmark == Stopmark.MustStop)
                                    {
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            if (Compare(sentence, query))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        i = j; // start new sentence after j
                                        break; // next i
                                    }
                                    else // unknown case
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Verses
    public static List<Verse> FindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindVerses(search_scope, current_selection, previous_verses, query);
    }
    private static List<Verse> DoFindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerses(source, query);
    }
    private static List<Verse> DoFindVerses(List<Verse> source, NumberQuery query)
    {
        List<Verse> result = new List<Verse>();

        if (source != null)
        {
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    if (Compare(verse, query))
                    {
                        result.Add(verse);
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - VerseRanges
    public static List<List<Verse>> FindVerseRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindVerseRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Verse>> DoFindVerseRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerseRanges(source, query);
    }
    private static List<List<Verse>> DoFindVerseRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Verse>> result = new List<List<Verse>>();

        if (source != null)
        {
            int range_length = query.VerseCount;
            if (range_length == 1)
            {
                result.Add(DoFindVerses(source, query));
            }
            else if (range_length == 0) // non-specified range length
            {
                for (int r = 1; r <= 29; r++) // try all possible range lengths
                {
                    for (int i = 0; i < source.Count - r + 1; i++)
                    {
                        // build required range
                        List<Verse> range = new List<Verse>();
                        for (int j = i; j < i + r; j++)
                        {
                            range.Add(source[j]);
                        }

                        // check range
                        if (Compare(range, query))
                        {
                            result.Add(range);
                        }
                    }
                }
            }
            else // specified range length
            {
                int r = range_length;
                for (int i = 0; i < source.Count - r + 1; i++)
                {
                    // build required range
                    List<Verse> range = new List<Verse>();
                    for (int j = i; j < i + r; j++)
                    {
                        range.Add(source[j]);
                    }

                    // check range
                    if (Compare(range, query))
                    {
                        result.Add(range);
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - VerseSets
    public static List<List<Verse>> FindVerseSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindVerseSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Verse>> DoFindVerseSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerseSets(source, query);
    }
    private static List<List<Verse>> DoFindVerseSets(List<Verse> source, NumberQuery query)
    {
        List<List<Verse>> result = new List<List<Verse>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Verse> verses = source;
                    if (verses != null)
                    {
                        int set_size = query.VerseCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindVerses(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            for (int i = 0; i < 29; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Verse> sets = new Combinations<Verse>(verses, size, GenerateOption.WithoutRepetition);
                                foreach (List<Verse> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Verse> sets = new Combinations<Verse>(verses, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Verse> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Chapters
    public static List<Chapter> FindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindChapters(search_scope, current_selection, previous_verses, query);
    }
    private static List<Chapter> DoFindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindChapters(source, query);
    }
    private static List<Chapter> DoFindChapters(List<Verse> source, NumberQuery query)
    {
        List<Chapter> result = new List<Chapter>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Chapter> chapters = s_book.GetChapters(source);
                    if (chapters != null)
                    {
                        foreach (Chapter chapter in chapters)
                        {
                            if (chapter != null)
                            {
                                if (Compare(chapter, query))
                                {
                                    result.Add(chapter);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - ChapterRanges
    public static List<List<Chapter>> FindChapterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindChapterRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Chapter>> DoFindChapterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindChapterRanges(source, query);
    }
    private static List<List<Chapter>> DoFindChapterRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Chapter>> result = new List<List<Chapter>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Chapter> chapters = s_book.GetChapters(source);
                    if (chapters != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindChapters(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = chapters.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < chapters.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Chapter> range = new List<Chapter>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(chapters[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < chapters.Count - r + 1; i++)
                            {
                                // build required range
                                List<Chapter> range = new List<Chapter>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(chapters[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - ChapterSets
    public static List<List<Chapter>> FindChapterSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindChapterSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Chapter>> DoFindChapterSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindChapterSets(source, query);
    }
    private static List<List<Chapter>> DoFindChapterSets(List<Verse> source, NumberQuery query)
    {
        List<List<Chapter>> result = new List<List<Chapter>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Chapter> chapters = s_book.GetChapters(source);
                    if (chapters != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindChapters(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = chapters.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Chapter> sets = new Combinations<Chapter>(chapters, size, GenerateOption.WithoutRepetition);
                                foreach (List<Chapter> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Chapter> sets = new Combinations<Chapter>(chapters, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Chapter> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Pages
    public static List<Page> FindPages(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindPages(search_scope, current_selection, previous_verses, query);
    }
    private static List<Page> DoFindPages(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindPages(source, query);
    }
    private static List<Page> DoFindPages(List<Verse> source, NumberQuery query)
    {
        List<Page> result = new List<Page>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Page> pages = s_book.GetPages(source);
                    if (pages != null)
                    {
                        foreach (Page page in pages)
                        {
                            if (page != null)
                            {
                                if (Compare(page, query))
                                {
                                    result.Add(page);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - PageRanges
    public static List<List<Page>> FindPageRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindPageRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Page>> DoFindPageRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindPageRanges(source, query);
    }
    private static List<List<Page>> DoFindPageRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Page>> result = new List<List<Page>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Page> pages = s_book.GetPages(source);
                    if (pages != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindPages(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = pages.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < pages.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Page> range = new List<Page>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(pages[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < pages.Count - r + 1; i++)
                            {
                                // build required range
                                List<Page> range = new List<Page>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(pages[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - PageSets
    public static List<List<Page>> FindPageSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindPageSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Page>> DoFindPageSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindPageSets(source, query);
    }
    private static List<List<Page>> DoFindPageSets(List<Verse> source, NumberQuery query)
    {
        List<List<Page>> result = new List<List<Page>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Page> pages = s_book.GetPages(source);
                    if (pages != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindPages(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = pages.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Page> sets = new Combinations<Page>(pages, size, GenerateOption.WithoutRepetition);
                                foreach (List<Page> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Page> sets = new Combinations<Page>(pages, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Page> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Stations
    public static List<Station> FindStations(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindStations(search_scope, current_selection, previous_verses, query);
    }
    private static List<Station> DoFindStations(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindStations(source, query);
    }
    private static List<Station> DoFindStations(List<Verse> source, NumberQuery query)
    {
        List<Station> result = new List<Station>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Station> stations = s_book.GetStations(source);
                    if (stations != null)
                    {
                        foreach (Station station in stations)
                        {
                            if (station != null)
                            {
                                if (Compare(station, query))
                                {
                                    result.Add(station);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - StationRanges
    public static List<List<Station>> FindStationRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindStationRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Station>> DoFindStationRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindStationRanges(source, query);
    }
    private static List<List<Station>> DoFindStationRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Station>> result = new List<List<Station>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Station> stations = s_book.GetStations(source);
                    if (stations != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindStations(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = stations.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < stations.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Station> range = new List<Station>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(stations[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < stations.Count - r + 1; i++)
                            {
                                // build required range
                                List<Station> range = new List<Station>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(stations[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - StationSets
    public static List<List<Station>> FindStationSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindStationSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Station>> DoFindStationSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindStationSets(source, query);
    }
    private static List<List<Station>> DoFindStationSets(List<Verse> source, NumberQuery query)
    {
        List<List<Station>> result = new List<List<Station>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Station> stations = s_book.GetStations(source);
                    if (stations != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindStations(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = stations.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Station> sets = new Combinations<Station>(stations, size, GenerateOption.WithoutRepetition);
                                foreach (List<Station> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Station> sets = new Combinations<Station>(stations, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Station> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Parts
    public static List<Part> FindParts(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindParts(search_scope, current_selection, previous_verses, query);
    }
    private static List<Part> DoFindParts(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindParts(source, query);
    }
    private static List<Part> DoFindParts(List<Verse> source, NumberQuery query)
    {
        List<Part> result = new List<Part>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Part> parts = s_book.GetParts(source);
                    if (parts != null)
                    {
                        foreach (Part part in parts)
                        {
                            if (part != null)
                            {
                                if (Compare(part, query))
                                {
                                    result.Add(part);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - PartRanges
    public static List<List<Part>> FindPartRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindPartRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Part>> DoFindPartRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindPartRanges(source, query);
    }
    private static List<List<Part>> DoFindPartRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Part>> result = new List<List<Part>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Part> parts = s_book.GetParts(source);
                    if (parts != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindParts(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = parts.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < parts.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Part> range = new List<Part>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(parts[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < parts.Count - r + 1; i++)
                            {
                                // build required range
                                List<Part> range = new List<Part>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(parts[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - PartSets
    public static List<List<Part>> FindPartSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindPartSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Part>> DoFindPartSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindPartSets(source, query);
    }
    private static List<List<Part>> DoFindPartSets(List<Verse> source, NumberQuery query)
    {
        List<List<Part>> result = new List<List<Part>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Part> parts = s_book.GetParts(source);
                    if (parts != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindParts(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = parts.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Part> sets = new Combinations<Part>(parts, size, GenerateOption.WithoutRepetition);
                                foreach (List<Part> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Part> sets = new Combinations<Part>(parts, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Part> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Groups
    public static List<Model.Group> FindGroups(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindGroups(search_scope, current_selection, previous_verses, query);
    }
    private static List<Model.Group> DoFindGroups(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindGroups(source, query);
    }
    private static List<Model.Group> DoFindGroups(List<Verse> source, NumberQuery query)
    {
        List<Model.Group> result = new List<Model.Group>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Model.Group> groups = s_book.GetGroups(source);
                    if (groups != null)
                    {
                        foreach (Model.Group group in groups)
                        {
                            if (group != null)
                            {
                                if (Compare(group, query))
                                {
                                    result.Add(group);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - GroupRanges
    public static List<List<Model.Group>> FindGroupRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindGroupRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Model.Group>> DoFindGroupRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindGroupRanges(source, query);
    }
    private static List<List<Model.Group>> DoFindGroupRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Model.Group>> result = new List<List<Model.Group>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Model.Group> groups = s_book.GetGroups(source);
                    if (groups != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindGroups(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = groups.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < groups.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Model.Group> range = new List<Model.Group>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(groups[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < groups.Count - r + 1; i++)
                            {
                                // build required range
                                List<Model.Group> range = new List<Model.Group>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(groups[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - GroupSets
    public static List<List<Model.Group>> FindGroupSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindGroupSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Model.Group>> DoFindGroupSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindGroupSets(source, query);
    }
    private static List<List<Model.Group>> DoFindGroupSets(List<Verse> source, NumberQuery query)
    {
        List<List<Model.Group>> result = new List<List<Model.Group>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Model.Group> groups = s_book.GetGroups(source);
                    if (groups != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindGroups(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = groups.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Model.Group> sets = new Combinations<Model.Group>(groups, size, GenerateOption.WithoutRepetition);
                                foreach (List<Model.Group> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Model.Group> sets = new Combinations<Model.Group>(groups, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Model.Group> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Halfs
    public static List<Half> FindHalfs(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindHalfs(search_scope, current_selection, previous_verses, query);
    }
    private static List<Half> DoFindHalfs(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindHalfs(source, query);
    }
    private static List<Half> DoFindHalfs(List<Verse> source, NumberQuery query)
    {
        List<Half> result = new List<Half>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Half> halfs = s_book.GetHalfs(source);
                    if (halfs != null)
                    {
                        foreach (Half half in halfs)
                        {
                            if (half != null)
                            {
                                if (Compare(half, query))
                                {
                                    result.Add(half);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - HalfRanges
    public static List<List<Half>> FindHalfRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindHalfRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Half>> DoFindHalfRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindHalfRanges(source, query);
    }
    private static List<List<Half>> DoFindHalfRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Half>> result = new List<List<Half>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Half> halfs = s_book.GetHalfs(source);
                    if (halfs != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindHalfs(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = halfs.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < halfs.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Half> range = new List<Half>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(halfs[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < halfs.Count - r + 1; i++)
                            {
                                // build required range
                                List<Half> range = new List<Half>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(halfs[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - HalfSets
    public static List<List<Half>> FindHalfSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindHalfSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Half>> DoFindHalfSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindHalfSets(source, query);
    }
    private static List<List<Half>> DoFindHalfSets(List<Verse> source, NumberQuery query)
    {
        List<List<Half>> result = new List<List<Half>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Half> halfs = s_book.GetHalfs(source);
                    if (halfs != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindHalfs(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = halfs.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Half> sets = new Combinations<Half>(halfs, size, GenerateOption.WithoutRepetition);
                                foreach (List<Half> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Half> sets = new Combinations<Half>(halfs, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Half> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Quarters
    public static List<Quarter> FindQuarters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindQuarters(search_scope, current_selection, previous_verses, query);
    }
    private static List<Quarter> DoFindQuarters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindQuarters(source, query);
    }
    private static List<Quarter> DoFindQuarters(List<Verse> source, NumberQuery query)
    {
        List<Quarter> result = new List<Quarter>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Quarter> quarters = s_book.GetQuarters(source);
                    if (quarters != null)
                    {
                        foreach (Quarter quarter in quarters)
                        {
                            if (quarter != null)
                            {
                                if (Compare(quarter, query))
                                {
                                    result.Add(quarter);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - QuarterRanges
    public static List<List<Quarter>> FindQuarterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindQuarterRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Quarter>> DoFindQuarterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindQuarterRanges(source, query);
    }
    private static List<List<Quarter>> DoFindQuarterRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Quarter>> result = new List<List<Quarter>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Quarter> quarters = s_book.GetQuarters(source);
                    if (quarters != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindQuarters(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = quarters.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < quarters.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Quarter> range = new List<Quarter>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(quarters[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < quarters.Count - r + 1; i++)
                            {
                                // build required range
                                List<Quarter> range = new List<Quarter>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(quarters[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - QuarterSets
    public static List<List<Quarter>> FindQuarterSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindQuarterSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Quarter>> DoFindQuarterSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindQuarterSets(source, query);
    }
    private static List<List<Quarter>> DoFindQuarterSets(List<Verse> source, NumberQuery query)
    {
        List<List<Quarter>> result = new List<List<Quarter>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Quarter> quarters = s_book.GetQuarters(source);
                    if (quarters != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindQuarters(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = quarters.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Quarter> sets = new Combinations<Quarter>(quarters, size, GenerateOption.WithoutRepetition);
                                foreach (List<Quarter> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Quarter> sets = new Combinations<Quarter>(quarters, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Quarter> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    // find by numbers - Bowings
    public static List<Bowing> FindBowings(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindBowings(search_scope, current_selection, previous_verses, query);
    }
    private static List<Bowing> DoFindBowings(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindBowings(source, query);
    }
    private static List<Bowing> DoFindBowings(List<Verse> source, NumberQuery query)
    {
        List<Bowing> result = new List<Bowing>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Bowing> bowings = s_book.GetBowings(source);
                    if (bowings != null)
                    {
                        foreach (Bowing bowing in bowings)
                        {
                            if (bowing != null)
                            {
                                if (Compare(bowing, query))
                                {
                                    result.Add(bowing);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - BowingRanges
    public static List<List<Bowing>> FindBowingRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindBowingRanges(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Bowing>> DoFindBowingRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindBowingRanges(source, query);
    }
    private static List<List<Bowing>> DoFindBowingRanges(List<Verse> source, NumberQuery query)
    {
        List<List<Bowing>> result = new List<List<Bowing>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Bowing> bowings = s_book.GetBowings(source);
                    if (bowings != null)
                    {
                        int range_length = query.PartitionCount;
                        if (range_length == 1)
                        {
                            result.Add(DoFindBowings(source, query));
                        }
                        else if (range_length == 0) // non-specified range length
                        {
                            // limit range length to minimum
                            int limit = bowings.Count - 1;

                            for (int r = 1; r <= limit; r++) // try all possible range lengths
                            {
                                for (int i = 0; i < bowings.Count - r + 1; i++)
                                {
                                    // build required range
                                    List<Bowing> range = new List<Bowing>();
                                    for (int j = i; j < i + r; j++)
                                    {
                                        range.Add(bowings[j]);
                                    }

                                    // check range
                                    if (Compare(range, query))
                                    {
                                        result.Add(range);
                                    }
                                }
                            }
                        }
                        else // specified range length
                        {
                            int r = range_length;
                            for (int i = 0; i < bowings.Count - r + 1; i++)
                            {
                                // build required range
                                List<Bowing> range = new List<Bowing>();
                                for (int j = i; j < i + r; j++)
                                {
                                    range.Add(bowings[j]);
                                }

                                // check range
                                if (Compare(range, query))
                                {
                                    result.Add(range);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by numbers - BowingSets
    public static List<List<Bowing>> FindBowingSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        return DoFindBowingSets(search_scope, current_selection, previous_verses, query);
    }
    private static List<List<Bowing>> DoFindBowingSets(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, NumberQuery query)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindBowingSets(source, query);
    }
    private static List<List<Bowing>> DoFindBowingSets(List<Verse> source, NumberQuery query)
    {
        List<List<Bowing>> result = new List<List<Bowing>>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_book != null)
                {
                    List<Bowing> bowings = s_book.GetBowings(source);
                    if (bowings != null)
                    {
                        int set_size = query.PartitionCount;
                        if (set_size == 1)
                        {
                            result.Add(DoFindBowings(source, query));
                        }
                        else if (set_size == 0) // non-specified set m_size
                        {
                            // limit set length to minimum
                            int limit = bowings.Count - 1;

                            for (int i = 0; i <= limit; i++) // try all possible set sizes
                            {
                                int size = i + 1;
                                Combinations<Bowing> sets = new Combinations<Bowing>(bowings, size, GenerateOption.WithoutRepetition);
                                foreach (List<Bowing> set in sets)
                                {
                                    // check set against query
                                    if (Compare(set, query))
                                    {
                                        result.Add(set);
                                    }
                                }
                            }
                        }
                        else // specified set m_size
                        {
                            Combinations<Bowing> sets = new Combinations<Bowing>(bowings, set_size, GenerateOption.WithoutRepetition);
                            foreach (List<Bowing> set in sets)
                            {
                                // check set against query
                                if (Compare(set, query))
                                {
                                    result.Add(set);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }


    // find by similarity - verses similar to given verse
    public static List<Verse> FindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_result, Verse verse, SimilarityMethod similarity_method, double similarity_percentage)
    {
        return DoFindVerses(search_scope, current_selection, previous_result, verse, similarity_method, similarity_percentage);
    }
    private static List<Verse> DoFindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_result, Verse verse, SimilarityMethod similarity_method, double similarity_percentage)
    {
        List<Verse> target = GetSourceVerses(search_scope, current_selection, previous_result, TextLocationInChapter.Any);
        return DoFindVerses(target, verse, similarity_method, similarity_percentage);
    }
    private static List<Verse> DoFindVerses(List<Verse> target, Verse verse, SimilarityMethod similarity_method, double similarity_percentage)
    {
        List<Verse> result = new List<Verse>();

        if (verse != null)
        {
            switch (similarity_method)
            {
                case SimilarityMethod.SimilarText:
                    {
                        // VERSE
                        string simplified_verse_text = verse.Text;
                        if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                        {
                            simplified_verse_text = verse.Text.Simplify29();
                        }

                        // TARGET
                        List<string> simplified_target_texts = new List<string>();
                        if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                        {
                            foreach (Verse v in target)
                            {
                                if (v != null)
                                {
                                    string v_text = v.Text.Simplify29();
                                    simplified_target_texts.Add(v_text);
                                }
                            }
                        }
                        else
                        {
                            foreach (Verse v in target)
                            {
                                if (v != null)
                                {
                                    simplified_target_texts.Add(v.Text);
                                }
                            }
                        }

                        // COMPARE
                        for (int i = 0; i < target.Count; i++)
                        {
                            //if (simplified_verse_text.IsSimilarTo(simplified_target_texts[i], similarity_percentage, true))
                            if (simplified_target_texts[i].IsSimilarTo(simplified_verse_text, similarity_percentage, true))
                            {
                                result.Add(target[i]);
                            }
                        }
                    }
                    break;
                case SimilarityMethod.SimilarWords:
                    {
                        // VERSE
                        List<string> simplified_verse_words = new List<string>();
                        if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                        {
                            foreach (Word w in verse.Words)
                            {
                                if (w != null)
                                {
                                    simplified_verse_words.Add(w.Text.Simplify29().Trim());
                                }
                            }
                        }
                        else
                        {
                            foreach (Word w in verse.Words)
                            {
                                if (w != null)
                                {
                                    simplified_verse_words.Add(w.Text);
                                }
                            }
                        }

                        // TARGET
                        List<List<string>> simplified_target_wordss = new List<List<string>>();
                        foreach (Verse v in target)
                        {
                            if (v != null)
                            {
                                List<string> simplified_target_words = new List<string>();
                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                {
                                    foreach (Word w in v.Words)
                                    {
                                        if (w != null)
                                        {
                                            simplified_target_words.Add(w.Text.Simplify29().Trim());
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (Word w in v.Words)
                                    {
                                        if (w != null)
                                        {
                                            simplified_target_words.Add(w.Text);
                                        }
                                    }
                                }
                                simplified_target_wordss.Add(simplified_target_words);
                            }
                        }

                        // COMPARE
                        for (int i = 0; i < target.Count; i++)
                        {
                            //if (simplified_verse_words.HasSimilarWordsTo(simplified_target_wordss[i], similarity_percentage, true))
                            if (simplified_target_wordss[i].HasSimilarWordsTo(simplified_verse_words, similarity_percentage, true))
                            {
                                result.Add(target[i]);
                            }
                        }
                    }
                    break;
                case SimilarityMethod.SameWordRoots:
                    {
                        // VERSE
                        List<string> verse_word_roots = new List<string>();
                        foreach (Word w in verse.Words)
                        {
                            if (w != null)
                            {
                                verse_word_roots.Add(w.Root);
                            }
                        }

                        // TARGET
                        List<List<string>> target_word_rootss = new List<List<string>>();
                        foreach (Verse v in target)
                        {
                            if (v != null)
                            {
                                List<string> target_word_roots = new List<string>();
                                foreach (Word w in v.Words)
                                {
                                    if (w != null)
                                    {
                                        target_word_roots.Add(w.Root);
                                    }
                                }
                                target_word_rootss.Add(target_word_roots);
                            }
                        }

                        // COMPARE
                        for (int i = 0; i < target.Count; i++)
                        {
                            //if (verse_word_roots.HasSameWordsTo(target_word_rootss[i], similarity_percentage, true))
                            if (target_word_rootss[i].HasSameWordsTo(verse_word_roots, similarity_percentage, true))
                            {
                                result.Add(target[i]);
                            }
                        }
                    }
                    break;
                case SimilarityMethod.SameWordValues:
                    {
                        // VERSE
                        List<long> verse_word_values = new List<long>();
                        foreach (Word w in verse.Words)
                        {
                            if (w != null)
                            {
                                verse_word_values.Add(w.Value);
                            }
                        }

                        // TARGET
                        List<List<long>> target_word_valuess = new List<List<long>>();
                        foreach (Verse v in target)
                        {
                            if (v != null)
                            {
                                List<long> target_word_values = new List<long>();
                                foreach (Word w in v.Words)
                                {
                                    if (w != null)
                                    {
                                        target_word_values.Add(w.Value);
                                    }
                                }
                                target_word_valuess.Add(target_word_values);
                            }
                        }

                        // COMPARE
                        for (int i = 0; i < target.Count; i++)
                        {
                            //if (verse_word_values.HasSameValuesTo(target_word_valuess[i], similarity_percentage, true))
                            if (target_word_valuess[i].HasSameValuesTo(verse_word_values, similarity_percentage, true))
                            {
                                result.Add(target[i]);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        return result;
    }
    // find by similarity - all similar verses to each other throughout the book
    public static List<List<Verse>> FindVersess(SearchScope search_scope, Selection current_selection, List<Verse> previous_result, SimilarityMethod similarity_method, double similarity_percentage)
    {
        return DoFindVersess(search_scope, current_selection, previous_result, similarity_method, similarity_percentage);
    }
    private static List<List<Verse>> DoFindVersess(SearchScope search_scope, Selection current_selection, List<Verse> previous_result, SimilarityMethod similarity_method, double similarity_percentage)
    {
        List<Verse> target = GetSourceVerses(search_scope, current_selection, previous_result, TextLocationInChapter.Any);
        return DoFindVersess(target, similarity_method, similarity_percentage);
    }
    private static List<List<Verse>> DoFindVersess(List<Verse> target, SimilarityMethod similarity_method, double similarity_percentage)
    {
        List<List<Verse>> result = new List<List<Verse>>();

        Dictionary<Verse, List<Verse>> verse_ranges = new Dictionary<Verse, List<Verse>>(); // need dictionary to check if key exist
        if (target != null)
        {
            if (target.Count > 0)
            {
                switch (similarity_method)
                {
                    case SimilarityMethod.SimilarText:
                    case SimilarityMethod.SimilarWords:
                    case SimilarityMethod.SameWordRoots:
                        {
                            return DoDoFindVersess(target, similarity_method, similarity_percentage);
                        }
                    case SimilarityMethod.SameWordValues:
                        {
                            // call above verse by verse
                            for (int i = 0; i < target.Count; i++)
                            {
                                verse_ranges.Add(target[i], DoFindVerses(target, target[i], similarity_method, similarity_percentage));
                                //result.Add(DoFindVerses(target, target[i], similarity_method, similarity_percentage));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        // copy dictionary to list of list
        if (verse_ranges.Count > 0)
        {
            foreach (List<Verse> verse_range in verse_ranges.Values)
            {
                result.Add(verse_range);
            }
        }

        return result;
    }
    private static List<List<Verse>> DoDoFindVersess(List<Verse> source, SimilarityMethod find_similarity_method, double similarity_percentage)
    {
        List<List<Verse>> result = new List<List<Verse>>();

        List<Verse> simplified_source = new List<Verse>();
        if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
        {
            // simplify verse texts
            List<string> verse_texts = new List<string>();
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    string verse_text = verse.Text.Simplify29();
                    verse_texts.Add(verse_text);
                }
            }

            // build verses
            for (int i = 0; i < verse_texts.Count; i++)
            {
                Verse verse = new Verse(i + 1, verse_texts[i], Stopmark.None);
                if (verse != null)
                {
                    simplified_source.Add(verse);
                }
            }
        }
        else
        {
            simplified_source = source;
        }

        Dictionary<Verse, List<Verse>> verse_ranges = new Dictionary<Verse, List<Verse>>(); // need dictionary to check if key exist
        bool[] already_compared = new bool[simplified_source.Count];
        if (simplified_source != null)
        {
            if (simplified_source.Count > 0)
            {
                switch (find_similarity_method)
                {
                    case SimilarityMethod.SimilarText:
                        {
                            for (int i = 0; i < simplified_source.Count - 1; i++)
                            {
                                for (int j = i + 1; j < simplified_source.Count; j++)
                                {
                                    if (!already_compared[j])
                                    {
                                        if (simplified_source[i].Text.IsSimilarTo(simplified_source[j].Text, similarity_percentage, true))
                                        {
                                            if (!verse_ranges.ContainsKey(source[i])) // first time matching verses found
                                            {
                                                List<Verse> similar_verses = new List<Verse>();
                                                verse_ranges.Add(source[i], similar_verses);
                                                similar_verses.Add(source[i]);
                                                similar_verses.Add(source[j]);
                                                already_compared[i] = true;
                                                already_compared[j] = true;
                                            }
                                            else // matching verses already exists
                                            {
                                                List<Verse> similar_verses = verse_ranges[source[i]];
                                                similar_verses.Add(source[j]);
                                                already_compared[j] = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case SimilarityMethod.SimilarWords:
                        {
                            for (int i = 0; i < simplified_source.Count - 1; i++)
                            {
                                for (int j = i + 1; j < simplified_source.Count; j++)
                                {
                                    if (!already_compared[j])
                                    {
                                        if (simplified_source[i].Text.HasSimilarWordsTo(simplified_source[j].Text, similarity_percentage, true))
                                        {
                                            if (!verse_ranges.ContainsKey(source[i])) // first time matching verses found
                                            {
                                                List<Verse> similar_verses = new List<Verse>();
                                                verse_ranges.Add(source[i], similar_verses);
                                                similar_verses.Add(source[i]);
                                                similar_verses.Add(source[j]);
                                                already_compared[i] = true;
                                                already_compared[j] = true;
                                            }
                                            else // matching verses already exists
                                            {
                                                List<Verse> similar_verses = verse_ranges[source[i]];
                                                similar_verses.Add(source[j]);
                                                already_compared[j] = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case SimilarityMethod.SameWordRoots:
                        {
                            for (int i = 0; i < simplified_source.Count - 1; i++)
                            {
                                for (int j = i + 1; j < simplified_source.Count; j++)
                                {
                                    if (!already_compared[j])
                                    {
                                        if (simplified_source[i].Text.HasSameWordsTo(simplified_source[j].Text, similarity_percentage, true))
                                        {
                                            if (!verse_ranges.ContainsKey(source[i])) // first time matching verses found
                                            {
                                                List<Verse> similar_verses = new List<Verse>();
                                                verse_ranges.Add(source[i], similar_verses);
                                                similar_verses.Add(source[i]);
                                                similar_verses.Add(source[j]);
                                                already_compared[i] = true;
                                                already_compared[j] = true;
                                            }
                                            else // matching verses already exists
                                            {
                                                List<Verse> similar_verses = verse_ranges[source[i]];
                                                similar_verses.Add(source[j]);
                                                already_compared[j] = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case SimilarityMethod.SameWordValues:
                        {
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        // copy dictionary to list of list
        if (verse_ranges.Count > 0)
        {
            foreach (List<Verse> verse_range in verse_ranges.Values)
            {
                result.Add(verse_range);
            }
        }

        return result;
    }


    // find by sum - Verses
    public static List<Verse> FindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, CVWLSumType cvwl_sum_type, long cvwl_sum)
    {
        return DoFindVerses(search_scope, current_selection, previous_verses, cvwl_sum_type, cvwl_sum);
    }
    private static List<Verse> DoFindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_result, CVWLSumType cvwl_sum_type, long cvwl_sum)
    {
        List<Verse> verses = GetSourceVerses(search_scope, current_selection, previous_result, TextLocationInChapter.Any);
        return DoFindVerses(verses, cvwl_sum_type, cvwl_sum);
    }
    private static List<Verse> DoFindVerses(List<Verse> source, CVWLSumType cvwl_sum_type, long cvwl_sum)
    {
        List<Verse> result = new List<Verse>();

        if (source != null)
        {
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    switch (cvwl_sum_type)
                    {
                        case CVWLSumType.CVSum:
                            {
                                long sum = verse.Chapter.SortedNumber + verse.NumberInChapter;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.CVWsSum:
                            {
                                long sum = verse.Chapter.SortedNumber + verse.NumberInChapter + verse.Words.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.CVWsLsSum:
                            {
                                long sum = verse.Chapter.SortedNumber + verse.NumberInChapter + verse.Words.Count + verse.Letters.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.CWsLsSum:
                            {
                                long sum = verse.Chapter.SortedNumber + verse.Words.Count + verse.Letters.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.CWsSum:
                            {
                                long sum = verse.Chapter.SortedNumber + verse.Words.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.CLsSum:
                            {
                                long sum = verse.Chapter.SortedNumber + verse.Letters.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.VWsLsSum:
                            {
                                long sum = verse.NumberInChapter + verse.Words.Count + verse.Letters.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.VWsSum:
                            {
                                long sum = verse.NumberInChapter + verse.Words.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                        case CVWLSumType.VLsSum:
                            {
                                long sum = verse.NumberInChapter + verse.Letters.Count;
                                if (cvwl_sum == sum)
                                {
                                    result.Add(verse);
                                }
                            }
                            break;
                    }
                }
            }
        }

        return result;
    }


    // find by prostration type
    public static List<Verse> FindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, ProstrationType prostration_type)
    {
        return DoFindVerses(search_scope, current_selection, previous_verses, prostration_type);
    }
    private static List<Verse> DoFindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, ProstrationType prostration_type)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerses(source, prostration_type);
    }
    private static List<Verse> DoFindVerses(List<Verse> source, ProstrationType prostration_type)
    {
        List<Verse> result = new List<Verse>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                foreach (Verse verse in source)
                {
                    if (verse != null)
                    {
                        if ((verse.ProstrationType & prostration_type) > 0)
                        {
                            result.Add(verse);
                        }
                    }
                }
            }
        }

        return result;
    }


    // find by revelation place
    public static List<Chapter> FindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, RevelationPlace revelation_place)
    {
        return DoFindChapters(search_scope, current_selection, previous_verses, revelation_place);
    }
    private static List<Chapter> DoFindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, RevelationPlace revelation_place)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindChapters(source, revelation_place);
    }
    private static List<Chapter> DoFindChapters(List<Verse> source, RevelationPlace revelation_place)
    {
        List<Chapter> result = new List<Chapter>();

        List<Verse> result_verses = new List<Verse>();
        if (source != null)
        {
            if (source.Count > 0)
            {
                foreach (Verse verse in source)
                {
                    if (verse != null)
                    {
                        if (verse.Chapter != null)
                        {
                            if (verse.Chapter.RevelationPlace == revelation_place)
                            {
                                result_verses.Add(verse);
                            }
                        }
                    }
                }
            }
        }

        int current_chapter_number = -1;
        foreach (Verse verse in result_verses)
        {
            if (verse != null)
            {
                if (verse.Chapter != null)
                {
                    if (current_chapter_number != verse.Chapter.SortedNumber)
                    {
                        current_chapter_number = verse.Chapter.SortedNumber;
                        result.Add(verse.Chapter);
                    }
                }
            }
        }

        return result;
    }


    // find by initialization type
    public static List<Verse> FindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, InitializationType initialization_type)
    {
        return DoFindVerses(search_scope, current_selection, previous_verses, initialization_type);
    }
    private static List<Verse> DoFindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, InitializationType initialization_type)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerses(source, initialization_type);
    }
    private static List<Verse> DoFindVerses(List<Verse> source, InitializationType initialization_type)
    {
        List<Verse> result = new List<Verse>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                foreach (Verse verse in source)
                {
                    if (verse != null)
                    {
                        if ((verse.InitializationType & initialization_type) > 0)
                        {
                            result.Add(verse);
                        }
                    }
                }
            }
        }

        return result;
    }


    // find by frequency - helper methods   
    public static int CalculateLetterFrequencySum(string text, string phrase, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        int result = 0;

        if (s_numerical_system != null)
        {
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
            text = text.Simplify(s_numerical_system.TextMode).Trim();

            if (!String.IsNullOrEmpty(phrase))
            {
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
                phrase = phrase.Simplify(s_numerical_system.TextMode).Trim();

                if (frequency_search_type == FrequencySearchType.UniqueLetters)
                {
                    phrase = phrase.RemoveDuplicates();
                    phrase = phrase.Replace(" ", "");
                }

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
                            result += frequency;
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by frequency - Words
    public static List<Word> FindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindWords(search_scope, current_selection, previous_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Word> DoFindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindWords(source, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Word> DoFindWords(List<Verse> source, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Word> result = new List<Word>();

        if (!string.IsNullOrEmpty(phrase))
        {
            if (source != null)
            {
                if (source.Count > 0)
                {
                    if (!String.IsNullOrEmpty(phrase))
                    {
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                foreach (Word word in verse.Words)
                                {
                                    if (word != null)
                                    {
                                        string text = word.Text;
                                        int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                        if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                        {
                                            result.Add(word);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by frequency - WordRanges
    public static List<List<Word>> FindWordRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindWordRanges(search_scope, current_selection, previous_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<List<Word>> DoFindWordRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindWordRanges(source, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<List<Word>> DoFindWordRanges(List<Verse> source, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<List<Word>> result = new List<List<Word>>();

        if (source != null)
        {
            List<Word> words = new List<Word>();
            foreach (Verse verse in source)
            {
                if (verse != null)
                {
                    words.AddRange(verse.Words);
                }
            }

            for (int r = 1; r <= 29; r++) // try all possible range lengths
            {
                for (int i = 0; i <= words.Count - r; i++)
                {
                    // build required range
                    List<Word> range = new List<Word>();
                    for (int j = i; j < i + r; j++)
                    {
                        range.Add(words[j]);
                    }

                    // check range
                    StringBuilder str = new StringBuilder();
                    foreach (Word word in range)
                    {
                        if (word != null)
                        {
                            str.Append(word.Text + " ");
                        }
                    }
                    int letter_frequency_sum = CalculateLetterFrequencySum(str.ToString(), phrase, frequency_search_type, include_diacritics);
                    if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                    {
                        result.Add(range);
                    }
                }
            }
        }

        return result;
    }
    // find by frequency - Sentences
    public static List<Sentence> FindSentences(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindSentences(search_scope, current_selection, previous_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Sentence> DoFindSentences(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindSentences(source, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Sentence> DoFindSentences(List<Verse> source, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Sentence> result = new List<Sentence>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                List<Word> words = new List<Word>();
                foreach (Verse verse in source)
                {
                    if (verse != null)
                    {
                        words.AddRange(verse.Words);
                    }
                }

                if (s_numerical_system != null)
                {
                    // scan linearly for sequence of words with total Text matching query
                    bool done_MustContinue = false;
                    for (int i = 0; i < words.Count - 1; i++)
                    {
                        StringBuilder str = new StringBuilder();

                        // start building word sequence
                        str.Append(words[i].Text);

                        string stopmark_text = StopmarkHelper.GetStopmarkText(words[i].Stopmark);

                        // 1-word sentence
                        if (
                             (words[i].Stopmark != Stopmark.None) &&
                             (words[i].Stopmark != Stopmark.CanStopAtEither) &&
                             (words[i].Stopmark != Stopmark.MustPause) //&&
                                                                       //(words[i].Stopmark != Stopmark.MustContinue)
                           )
                        {
                            Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[i].Verse, words[i].Position + words[i].Text.Length, str.ToString());
                            if (sentence != null)
                            {
                                string text = sentence.ToString();
                                int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                {
                                    result.Add(sentence);
                                }
                            }
                        }
                        else // multi-word sentence
                        {
                            // mark the start of 1-to-m MustContinue stopmarks
                            int backup_i = i;

                            // continue building with next words until a stopmark
                            bool done_CanStopAtEither = false;
                            for (int j = i + 1; j < words.Count; j++)
                            {
                                str.Append(" " + words[j].Text);

                                if (words[j].Stopmark == Stopmark.None)
                                {
                                    continue; // continue building longer senetence
                                }
                                else // there is a real stopmark
                                {
                                    if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                    {
                                        str.Append(" " + stopmark_text);
                                    }

                                    if (words[j].Stopmark == Stopmark.MustContinue)
                                    {
                                        // TEST Stopmark.MustContinue
                                        //----1 2 3 4 sentences
                                        //1268
                                        //4153
                                        //1799
                                        //2973
                                        //----1 12 123 1234 sentences
                                        //1268
                                        //5421
                                        //7220
                                        //10193
                                        //-------------
                                        //ERRORS
                                        //# duplicate 1
                                        //# short str
                                        //  in 123 1234
                                        //-------------
                                        //// not needed yet
                                        //// multi-mid sentences
                                        //5952
                                        //4772

                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                            if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        if (done_MustContinue)
                                        {
                                            done_MustContinue = false;
                                            continue; // get all overlapping long sentence
                                        }

                                        StringBuilder k_str = new StringBuilder();
                                        for (int k = j + 1; k < words.Count; k++)
                                        {
                                            k_str.Append(words[k].Text + " ");

                                            if (words[k].Stopmark == Stopmark.None)
                                            {
                                                continue; // next k
                                            }
                                            else // there is a stopmark
                                            {
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    stopmark_text = StopmarkHelper.GetStopmarkText(words[k].Stopmark);
                                                    k_str.Append(stopmark_text + " ");
                                                }
                                                if (k_str.Length > 0)
                                                {
                                                    k_str.Remove(k_str.Length - 1, 1);
                                                }

                                                sentence = new Sentence(words[j + 1].Verse, words[j + 1].Position, words[k].Verse, words[k].Position + words[k].Text.Length, k_str.ToString());
                                                if (sentence != null)
                                                {
                                                    string text = sentence.ToString();
                                                    int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                                    if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                                    {
                                                        result.Add(sentence);
                                                    }
                                                }

                                                if (
                                                     (words[k].Stopmark == Stopmark.ShouldContinue) ||
                                                     (words[k].Stopmark == Stopmark.CanStop) ||
                                                     (words[k].Stopmark == Stopmark.ShouldStop)
                                                   )
                                                {
                                                    done_MustContinue = true;   // restart from beginning skipping any MustContinue
                                                }
                                                else
                                                {
                                                    done_MustContinue = false;   // keep building ever-longer multi-MustContinue sentence
                                                }

                                                j = k;
                                                break; // next j
                                            }
                                        }

                                        if (done_MustContinue)
                                        {
                                            i = backup_i - 1;  // start new sentence from beginning
                                            break; // next i
                                        }
                                        else
                                        {
                                            continue; // next j
                                        }
                                    }
                                    else if (
                                         (words[j].Stopmark == Stopmark.ShouldContinue) ||
                                         (words[j].Stopmark == Stopmark.CanStop) ||
                                         (words[j].Stopmark == Stopmark.ShouldStop)
                                       )
                                    {
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                            if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        i = j; // start new sentence after j
                                        break; // next i
                                    }
                                    else if (words[j].Stopmark == Stopmark.MustPause)
                                    {
                                        if (
                                             (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَنْ".Simplify(s_numerical_system.TextMode)) ||
                                             (words[j].Text.Simplify(s_numerical_system.TextMode) == "بَلْ".Simplify(s_numerical_system.TextMode))
                                           )
                                        {
                                            continue; // continue building longer senetence
                                        }
                                        else if (
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "عِوَجَا".Simplify(s_numerical_system.TextMode)) ||
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَّرْقَدِنَا".Simplify(s_numerical_system.TextMode)) ||
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَالِيَهْ".Simplify(s_numerical_system.TextMode))
                                                )
                                        {
                                            Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                            if (sentence != null)
                                            {
                                                string text = sentence.ToString();
                                                int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                                if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                                {
                                                    result.Add(sentence);
                                                }
                                            }

                                            i = j; // start new sentence after j
                                            break; // next i
                                        }
                                        else // unknown case
                                        {
                                            throw new Exception("Unknown paused Quran word");
                                        }
                                    }
                                    // first CanStopAtEither found at j
                                    else if ((!done_CanStopAtEither) && (words[j].Stopmark == Stopmark.CanStopAtEither))
                                    {
                                        // ^ ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                            if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        int kk = -1; // start after ^ (e.g. هُدًۭى)
                                        StringBuilder kk_str = new StringBuilder();
                                        StringBuilder kkk_str = new StringBuilder();
                                        for (int k = j + 1; k < words.Count; k++)
                                        {
                                            str.Append(" " + words[k].Text);
                                            if (kkk_str.Length > 0) // skip first k loop
                                            {
                                                kk_str.Append(" " + words[k].Text);
                                            }
                                            kkk_str.Append(" " + words[k].Text);

                                            if (words[k].Stopmark == Stopmark.None)
                                            {
                                                continue; // next k
                                            }
                                            else // there is a stopmark
                                            {
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    str.Append(" " + stopmark_text);
                                                    if (kk_str.Length > 0)
                                                    {
                                                        kk_str.Append(" " + stopmark_text);
                                                    }
                                                    kkk_str.Append(" " + stopmark_text);
                                                }

                                                // second CanStopAtEither found at k
                                                if (words[k].Stopmark == Stopmark.CanStopAtEither)
                                                {
                                                    // ^ ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ ۛ^ فِيهِ
                                                    sentence = new Sentence(words[i].Verse, words[i].Position, words[k].Verse, words[k].Position + words[k].Text.Length, str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        string text = sentence.ToString();
                                                        int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                                        if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    kk = k + 1; // backup k after second ^
                                                    continue; // next k
                                                }
                                                else // non-CanStopAtEither stopmark
                                                {
                                                    // kkk_str   فِيهِ ۛ^ هُدًۭى لِّلْمُتَّقِينَ
                                                    sentence = new Sentence(words[j + 1].Verse, words[j + 1].Position, words[k].Verse, words[k].Position + words[k].Text.Length, kkk_str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        string text = sentence.ToString();
                                                        int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                                        if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    // kk_str   هُدًۭى لِّلْمُتَّقِينَ
                                                    sentence = new Sentence(words[kk].Verse, words[kk].Position, words[k].Verse, words[k].Position + words[k].Text.Length, kk_str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        string text = sentence.ToString();
                                                        int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                                        if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    // skip the whole surrounding non-CanStopAtEither sentence
                                                    j = k;
                                                    break; // next j
                                                }
                                            }
                                        }

                                        // restart from last
                                        str.Length = 0;
                                        j = i - 1; // will be j++ by reloop
                                        done_CanStopAtEither = true;
                                    }
                                    else if (words[j].Stopmark == Stopmark.MustStop)
                                    {
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                            if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        i = j; // start new sentence after j
                                        break; // next i
                                    }
                                    else // unknown case
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by frequency - Verses
    public static List<Verse> FindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindVerses(search_scope, current_selection, previous_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Verse> DoFindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerses(source, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Verse> DoFindVerses(List<Verse> source, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> result = new List<Verse>();

        if (!string.IsNullOrEmpty(phrase))
        {
            if (source != null)
            {
                if (source.Count > 0)
                {
                    if (!String.IsNullOrEmpty(phrase))
                    {
                        foreach (Verse verse in source)
                        {
                            if (verse != null)
                            {
                                string text = verse.Text;
                                int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                {
                                    result.Add(verse);
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by frequency - VerseRanges
    public static List<List<Verse>> FindVerseRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindVerseRanges(search_scope, current_selection, previous_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<List<Verse>> DoFindVerseRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerseRanges(source, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<List<Verse>> DoFindVerseRanges(List<Verse> source, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<List<Verse>> result = new List<List<Verse>>();

        if (source != null)
        {
            List<Verse> verses = source;

            for (int r = 1; r <= 29; r++) // try all possible range lengths
            {
                for (int i = 0; i <= verses.Count - r; i++)
                {
                    // build required range
                    List<Verse> range = new List<Verse>();
                    for (int j = i; j < i + r; j++)
                    {
                        range.Add(verses[j]);
                    }

                    // check range
                    StringBuilder str = new StringBuilder();
                    foreach (Verse verse in range)
                    {
                        if (verse != null)
                        {
                            str.Append(verse.Text + "\r\n");
                        }
                    }
                    int letter_frequency_sum = CalculateLetterFrequencySum(str.ToString(), phrase, frequency_search_type, include_diacritics);
                    if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                    {
                        result.Add(range);
                    }
                }
            }
        }

        return result;
    }
    // find by frequency - Chapters
    public static List<Chapter> FindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindChapters(search_scope, current_selection, previous_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Chapter> DoFindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindChapters(source, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<Chapter> DoFindChapters(List<Verse> source, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Chapter> result = new List<Chapter>();

        if (!string.IsNullOrEmpty(phrase))
        {
            if (source != null)
            {
                if (source.Count > 0)
                {
                    if (s_book != null)
                    {
                        List<Chapter> source_chapters = s_book.GetChapters(source);
                        if (!String.IsNullOrEmpty(phrase))
                        {
                            foreach (Chapter chapter in source_chapters)
                            {
                                if (chapter != null)
                                {
                                    string text = chapter.Text;
                                    int letter_frequency_sum = CalculateLetterFrequencySum(text, phrase, frequency_search_type, include_diacritics);
                                    if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                                    {
                                        result.Add(chapter);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by frequency - ChapterRanges
    public static List<List<Chapter>> FindChapterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindChapterRanges(search_scope, current_selection, previous_verses, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<List<Chapter>> DoFindChapterRanges(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindChapterRanges(source, phrase, sum, number_type, comparison_operator, sum_remainder, frequency_search_type, include_diacritics);
    }
    private static List<List<Chapter>> DoFindChapterRanges(List<Verse> source, string phrase, int sum, NumberType number_type, ComparisonOperator comparison_operator, int sum_remainder, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<List<Chapter>> result = new List<List<Chapter>>();

        if (source != null)
        {
            List<Chapter> chapters = s_book.GetCompleteChapters(source);

            for (int r = 1; r <= 29; r++) // try all possible range lengths
            {
                for (int i = 0; i <= chapters.Count - r; i++)
                {
                    // build required range
                    List<Chapter> range = new List<Chapter>();
                    for (int j = i; j < i + r; j++)
                    {
                        range.Add(chapters[j]);
                    }

                    // check range
                    StringBuilder str = new StringBuilder();
                    foreach (Chapter chapter in range)
                    {
                        if (chapter != null)
                        {
                            str.Append(chapter.Text + "\r\n");
                        }
                    }
                    int letter_frequency_sum = CalculateLetterFrequencySum(str.ToString(), phrase, frequency_search_type, include_diacritics);
                    if (Compare(letter_frequency_sum, sum, number_type, comparison_operator, sum_remainder))
                    {
                        result.Add(range);
                    }
                }
            }
        }

        return result;
    }

    // find by frequency matching letters - Words
    public static List<Word> FindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindWords(search_scope, current_selection, previous_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Word> DoFindWords(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindWords(source, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Word> DoFindWords(List<Verse> source, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Word> result = new List<Word>();

        if (!string.IsNullOrEmpty(phrase))
        {
            if (source != null)
            {
                if (source.Count > 0)
                {
                    if (!String.IsNullOrEmpty(phrase))
                    {
                        if (s_numerical_system != null)
                        {
                            foreach (Verse verse in source)
                            {
                                if (verse != null)
                                {
                                    foreach (Word word in verse.Words)
                                    {
                                        if (word != null)
                                        {
                                            string text = word.Text;
                                            if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                            {
                                                result.Add(word);
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

        return result;
    }
    // find by frequency matching letters - Sentences
    public static List<Sentence> FindSentences(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindSentences(search_scope, current_selection, previous_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Sentence> DoFindSentences(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindSentences(source, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Sentence> DoFindSentences(List<Verse> source, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Sentence> result = new List<Sentence>();

        if (source != null)
        {
            if (source.Count > 0)
            {
                if (s_numerical_system != null)
                {
                    List<Word> words = new List<Word>();
                    foreach (Verse verse in source)
                    {
                        if (verse != null)
                        {
                            words.AddRange(verse.Words);
                        }
                    }

                    // scan linearly for sequence of words with total Text matching query
                    bool done_MustContinue = false;
                    for (int i = 0; i < words.Count - 1; i++)
                    {
                        StringBuilder str = new StringBuilder();

                        // start building word sequence
                        str.Append(words[i].Text);

                        string stopmark_text = StopmarkHelper.GetStopmarkText(words[i].Stopmark);

                        // 1-word sentence
                        if (
                             (words[i].Stopmark != Stopmark.None) &&
                             (words[i].Stopmark != Stopmark.CanStopAtEither) &&
                             (words[i].Stopmark != Stopmark.MustPause) //&&
                                                                       //(words[i].Stopmark != Stopmark.MustContinue)
                           )
                        {
                            Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[i].Verse, words[i].Position + words[i].Text.Length, str.ToString());
                            if (sentence != null)
                            {
                                string text = sentence.ToString();
                                if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                {
                                    result.Add(sentence);
                                }
                            }
                        }
                        else // multi-word sentence
                        {
                            // mark the start of 1-to-m MustContinue stopmarks
                            int backup_i = i;

                            // continue building with next words until a stopmark
                            bool done_CanStopAtEither = false;
                            for (int j = i + 1; j < words.Count; j++)
                            {
                                str.Append(" " + words[j].Text);

                                if (words[j].Stopmark == Stopmark.None)
                                {
                                    continue; // continue building longer senetence
                                }
                                else // there is a real stopmark
                                {
                                    if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                    {
                                        str.Append(" " + stopmark_text);
                                    }

                                    if (words[j].Stopmark == Stopmark.MustContinue)
                                    {
                                        // TEST Stopmark.MustContinue
                                        //----1 2 3 4 sentences
                                        //1268
                                        //4153
                                        //1799
                                        //2973
                                        //----1 12 123 1234 sentences
                                        //1268
                                        //5421
                                        //7220
                                        //10193
                                        //-------------
                                        //ERRORS
                                        //# duplicate 1
                                        //# short str
                                        //  in 123 1234
                                        //-------------
                                        //// not needed yet
                                        //// multi-mid sentences
                                        //5952
                                        //4772

                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        if (done_MustContinue)
                                        {
                                            done_MustContinue = false;
                                            continue; // get all overlapping long sentence
                                        }

                                        StringBuilder k_str = new StringBuilder();
                                        for (int k = j + 1; k < words.Count; k++)
                                        {
                                            k_str.Append(words[k].Text + " ");

                                            if (words[k].Stopmark == Stopmark.None)
                                            {
                                                continue; // next k
                                            }
                                            else // there is a stopmark
                                            {
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    stopmark_text = StopmarkHelper.GetStopmarkText(words[k].Stopmark);
                                                    k_str.Append(stopmark_text + " ");
                                                }
                                                if (k_str.Length > 0)
                                                {
                                                    k_str.Remove(k_str.Length - 1, 1);
                                                }

                                                sentence = new Sentence(words[j + 1].Verse, words[j + 1].Position, words[k].Verse, words[k].Position + words[k].Text.Length, k_str.ToString());
                                                if (sentence != null)
                                                {
                                                    string text = sentence.ToString();
                                                    if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                                    {
                                                        result.Add(sentence);
                                                    }
                                                }

                                                if (
                                                     (words[k].Stopmark == Stopmark.ShouldContinue) ||
                                                     (words[k].Stopmark == Stopmark.CanStop) ||
                                                     (words[k].Stopmark == Stopmark.ShouldStop)
                                                   )
                                                {
                                                    done_MustContinue = true;   // restart from beginning skipping any MustContinue
                                                }
                                                else
                                                {
                                                    done_MustContinue = false;   // keep building ever-longer multi-MustContinue sentence
                                                }

                                                j = k;
                                                break; // next j
                                            }
                                        }

                                        if (done_MustContinue)
                                        {
                                            i = backup_i - 1;  // start new sentence from beginning
                                            break; // next i
                                        }
                                        else
                                        {
                                            continue; // next j
                                        }
                                    }
                                    else if (
                                         (words[j].Stopmark == Stopmark.ShouldContinue) ||
                                         (words[j].Stopmark == Stopmark.CanStop) ||
                                         (words[j].Stopmark == Stopmark.ShouldStop)
                                       )
                                    {
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        i = j; // start new sentence after j
                                        break; // next i
                                    }
                                    else if (words[j].Stopmark == Stopmark.MustPause)
                                    {
                                        if (
                                             (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَنْ".Simplify(s_numerical_system.TextMode)) ||
                                             (words[j].Text.Simplify(s_numerical_system.TextMode) == "بَلْ".Simplify(s_numerical_system.TextMode))
                                           )
                                        {
                                            continue; // continue building longer senetence
                                        }
                                        else if (
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "عِوَجَا".Simplify(s_numerical_system.TextMode)) ||
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَّرْقَدِنَا".Simplify(s_numerical_system.TextMode)) ||
                                                  (words[j].Text.Simplify(s_numerical_system.TextMode) == "مَالِيَهْ".Simplify(s_numerical_system.TextMode))
                                                )
                                        {
                                            Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                            if (sentence != null)
                                            {
                                                string text = sentence.ToString();
                                                if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                                {
                                                    result.Add(sentence);
                                                }
                                            }

                                            i = j; // start new sentence after j
                                            break; // next i
                                        }
                                        else // unknown case
                                        {
                                            throw new Exception("Unknown paused Quran word");
                                        }
                                    }
                                    // first CanStopAtEither found at j
                                    else if ((!done_CanStopAtEither) && (words[j].Stopmark == Stopmark.CanStopAtEither))
                                    {
                                        // ^ ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        int kk = -1; // start after ^ (e.g. هُدًۭى)
                                        StringBuilder kk_str = new StringBuilder();
                                        StringBuilder kkk_str = new StringBuilder();
                                        for (int k = j + 1; k < words.Count; k++)
                                        {
                                            str.Append(" " + words[k].Text);
                                            if (kkk_str.Length > 0) // skip first k loop
                                            {
                                                kk_str.Append(" " + words[k].Text);
                                            }
                                            kkk_str.Append(" " + words[k].Text);

                                            if (words[k].Stopmark == Stopmark.None)
                                            {
                                                continue; // next k
                                            }
                                            else // there is a stopmark
                                            {
                                                if ((s_numerical_system.TextMode == "Original") || (s_numerical_system.TextMode == "SimplifiedMarks"))
                                                {
                                                    str.Append(" " + stopmark_text);
                                                    if (kk_str.Length > 0)
                                                    {
                                                        kk_str.Append(" " + stopmark_text);
                                                    }
                                                    kkk_str.Append(" " + stopmark_text);
                                                }

                                                // second CanStopAtEither found at k
                                                if (words[k].Stopmark == Stopmark.CanStopAtEither)
                                                {
                                                    // ^ ذَٰلِكَ ٱلْكِتَٰبُ لَا رَيْبَ ۛ^ فِيهِ
                                                    sentence = new Sentence(words[i].Verse, words[i].Position, words[k].Verse, words[k].Position + words[k].Text.Length, str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        string text = sentence.ToString();
                                                        if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    kk = k + 1; // backup k after second ^
                                                    continue; // next k
                                                }
                                                else // non-CanStopAtEither stopmark
                                                {
                                                    // kkk_str   فِيهِ ۛ^ هُدًۭى لِّلْمُتَّقِينَ
                                                    sentence = new Sentence(words[j + 1].Verse, words[j + 1].Position, words[k].Verse, words[k].Position + words[k].Text.Length, kkk_str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        string text = sentence.ToString();
                                                        if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    // kk_str   هُدًۭى لِّلْمُتَّقِينَ
                                                    sentence = new Sentence(words[kk].Verse, words[kk].Position, words[k].Verse, words[k].Position + words[k].Text.Length, kk_str.ToString());
                                                    if (sentence != null)
                                                    {
                                                        string text = sentence.ToString();
                                                        if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                                        {
                                                            result.Add(sentence);
                                                        }
                                                    }

                                                    // skip the whole surrounding non-CanStopAtEither sentence
                                                    j = k;
                                                    break; // next j
                                                }
                                            }
                                        }

                                        // restart from last
                                        str.Length = 0;
                                        j = i - 1; // will be j++ by reloop
                                        done_CanStopAtEither = true;
                                    }
                                    else if (words[j].Stopmark == Stopmark.MustStop)
                                    {
                                        Sentence sentence = new Sentence(words[i].Verse, words[i].Position, words[j].Verse, words[j].Position + words[j].Text.Length, str.ToString());
                                        if (sentence != null)
                                        {
                                            string text = sentence.ToString();
                                            if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                            {
                                                result.Add(sentence);
                                            }
                                        }

                                        i = j; // start new sentence after j
                                        break; // next i
                                    }
                                    else // unknown case
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by frequency matching letters - Verses
    public static List<Verse> FindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindVerses(search_scope, current_selection, previous_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Verse> DoFindVerses(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindVerses(source, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Verse> DoFindVerses(List<Verse> source, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> result = new List<Verse>();

        if (!string.IsNullOrEmpty(phrase))
        {
            if (source != null)
            {
                if (source.Count > 0)
                {
                    if (!String.IsNullOrEmpty(phrase))
                    {
                        if (s_numerical_system != null)
                        {
                            foreach (Verse verse in source)
                            {
                                if (verse != null)
                                {
                                    string text = verse.Text;
                                    if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                    {
                                        result.Add(verse);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    // find by frequency matching letters - Chapters
    public static List<Chapter> FindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        return DoFindChapters(search_scope, current_selection, previous_verses, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Chapter> DoFindChapters(SearchScope search_scope, Selection current_selection, List<Verse> previous_verses, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Verse> source = GetSourceVerses(search_scope, current_selection, previous_verses, TextLocationInChapter.Any);
        return DoFindChapters(source, phrase, frequency_matching_type, frequency_search_type, include_diacritics);
    }
    private static List<Chapter> DoFindChapters(List<Verse> source, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        List<Chapter> result = new List<Chapter>();

        if (!string.IsNullOrEmpty(phrase))
        {
            if (source != null)
            {
                if (source.Count > 0)
                {
                    if (s_book != null)
                    {
                        List<Chapter> source_chapters = s_book.GetChapters(source);
                        if (!String.IsNullOrEmpty(phrase))
                        {
                            if (s_numerical_system != null)
                            {
                                foreach (Chapter chapter in source_chapters)
                                {
                                    if (chapter != null)
                                    {
                                        string text = chapter.Text;
                                        if (IsMatchingLetters(s_numerical_system.TextMode, text, phrase, frequency_matching_type, frequency_search_type, include_diacritics))
                                        {
                                            result.Add(chapter);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return result;
    }
    public static bool IsMatchingLetters(string text_mode, string text, string phrase, FrequencyMatchingType frequency_matching_type, FrequencySearchType frequency_search_type, bool? include_diacritics)
    {
        if (String.IsNullOrEmpty(text))
            return false;
        if (String.IsNullOrEmpty(phrase))
            return false;

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
        foreach (char character in Constants.STOPMARKS)
        {
            text = text.Replace(character.ToString(), "");
        }
        foreach (char character in Constants.QURANMARKS)
        {
            text = text.Replace(character.ToString(), "");
        }
        text = text.Simplify(s_numerical_system.TextMode).Trim();

        phrase = phrase.Simplify(NumericalSystem.TextMode);
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
        foreach (char character in Constants.STOPMARKS)
        {
            phrase = phrase.Replace(character.ToString(), "");
        }
        foreach (char character in Constants.QURANMARKS)
        {
            phrase = phrase.Replace(character.ToString(), "");
        }
        phrase = phrase.Simplify(s_numerical_system.TextMode).Trim();

        if (frequency_search_type == FrequencySearchType.UniqueLetters)
        {
            phrase = phrase.RemoveDuplicates();
        }

        Dictionary<char, int> text_letter_statistics = new Dictionary<char, int>();
        if (text_letter_statistics != null)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text_letter_statistics.ContainsKey(text[i]))
                {
                    text_letter_statistics[text[i]]++;
                }
                else
                {
                    text_letter_statistics.Add(text[i], 1);
                }
            }
        }

        Dictionary<char, int> phrase_letter_statistics = new Dictionary<char, int>();
        if (phrase_letter_statistics != null)
        {
            for (int i = 0; i < phrase.Length; i++)
            {
                if (phrase_letter_statistics.ContainsKey(phrase[i]))
                {
                    phrase_letter_statistics[phrase[i]]++;
                }
                else
                {
                    phrase_letter_statistics.Add(phrase[i], 1);
                }
            }
        }

        if ((text_letter_statistics != null) && (phrase_letter_statistics != null))
        {
            switch (frequency_matching_type)
            {
                case FrequencyMatchingType.AllLettersOf:
                    {
                        for (int i = 0; i < phrase.Length; i++)
                        {
                            if (!text_letter_statistics.ContainsKey(phrase[i]))
                            {
                                return false;
                            }

                            if (frequency_search_type == FrequencySearchType.DuplicateLetters)
                            {
                                if (text_letter_statistics[phrase[i]] != phrase_letter_statistics[phrase[i]])
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    break;
                case FrequencyMatchingType.AnyLetterOf:
                    {
                        int count = 0;
                        for (int i = 0; i < phrase.Length; i++)
                        {
                            if (text_letter_statistics.ContainsKey(phrase[i]))
                            {
                                if (frequency_search_type == FrequencySearchType.DuplicateLetters)
                                {
                                    if (text_letter_statistics[phrase[i]] == phrase_letter_statistics[phrase[i]])
                                    {
                                        count++;
                                    }
                                }
                                else
                                {
                                    count++;
                                }
                            }
                        }
                        if (count == 0)
                            return false;
                    }
                    break;
                case FrequencyMatchingType.OnlyLettersOf:
                    {
                        int count = 0;
                        for (int i = 0; i < text.Length; i++)
                        {
                            if (phrase_letter_statistics.ContainsKey(text[i]))
                            {
                                if (frequency_search_type == FrequencySearchType.DuplicateLetters)
                                {
                                    if (text_letter_statistics[text[i]] == phrase_letter_statistics[text[i]])
                                    {
                                        count++;
                                    }
                                }
                                else
                                {
                                    count++;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        if (count < text.Length)
                            return false;
                    }
                    break;
                case FrequencyMatchingType.NoLetterOf:
                    {
                        int count = 0;
                        for (int i = 0; i < text.Length; i++)
                        {
                            if (phrase_letter_statistics.ContainsKey(text[i]))
                            {
                                if (frequency_search_type == FrequencySearchType.DuplicateLetters)
                                {
                                    //if (text_letter_statistics[text[i]] == phrase_letter_statistics[text[i]])
                                    {
                                        count++;
                                    }
                                }
                                else
                                {
                                    count++;
                                }
                            }
                        }
                        if (count > 0)
                            return false;
                    }
                    break;
                default:
                    {
                        return false;
                    }
            }
        }
        return true;
    }


    public static string GetTranslationKey(string translation)
    {
        string result = null;

        if (s_book != null)
        {
            if (s_book.TranslationInfos != null)
            {
                foreach (string key in s_book.TranslationInfos.Keys)
                {
                    if (s_book.TranslationInfos[key].Name == translation)
                    {
                        result = key;
                    }
                }
            }
        }

        return result;
    }
    public static void LoadTranslation(string translation)
    {
        DataAccess.LoadTranslation(s_book, translation);
    }
    public static void UnloadTranslation(string translation)
    {
        DataAccess.UnloadTranslation(s_book, translation);
    }
    public static void SaveTranslation(string translation)
    {
        DataAccess.SaveTranslation(s_book, translation);
    }


    // help messages
    private static List<string> s_help_messages = new List<string>();
    public static List<string> HelpMessages
    {
        get { return s_help_messages; }
    }
    private static void LoadHelpMessages()
    {
        string path = Globals.HELP_FOLDER + Path.DirectorySeparatorChar + "Messages.txt";
        if (File.Exists(path))
        {
            s_help_messages = FileHelper.LoadLines(path);
        }
    }
}
