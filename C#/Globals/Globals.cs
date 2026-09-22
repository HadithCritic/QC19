using System.Drawing;

public enum Edition { Standard, Research, Ultimate, BigNumbers }

public static class Globals
{
    public static Edition EDITION = Edition.Standard;
    public static Color EDITION_BACKCOLOR
    {
        get
        {
            switch (Globals.EDITION)
            {
                case Edition.Standard:
                    {
                        return Color.FromArgb(208, 240, 255);
                    }
                case Edition.Research:
                    {
                        return Color.FromArgb(208, 255, 208);
                    }
                case Edition.Ultimate:
                    {
                        return Color.FromArgb(255, 208, 240);
                    }
                case Edition.BigNumbers:
                    {
                        return Color.FromArgb(240, 240, 240);
                    }
                default:
                    {
                        return Color.FromArgb(208, 208, 240);
                    }
            }
        }
    }
    public static Color EDITION_FORECOLOR
    {
        get
        {
            switch (Globals.EDITION)
            {
                case Edition.Standard:
                    {
                        return Color.FromArgb(0, 0, 128);
                    }
                case Edition.Research:
                    {
                        return Color.FromArgb(0, 128, 0);
                    }
                case Edition.Ultimate:
                    {
                        return Color.FromArgb(160, 32, 64);
                    }
                case Edition.BigNumbers:
                    {
                        return Color.FromArgb(0, 0, 0);
                    }
                default:
                    {
                        return Color.FromArgb(0, 0, 128);
                    }
            }
        }
    }

    private static string RELEASE = "B89";
    private static string VERSION = "7.29.139.8317"; // updated by Version.bat (with AssemblyInfo.cs of all projects)
    public static string RELEASE_EDITION
    {
        get
        {
            string version = VERSION;
            string[] parts = version.Split('.');
            if (parts.Length == 4)
            {
                int pos = version.LastIndexOf('.');
                if (pos > -1)
                {
                    version = version.Substring(0, pos);
                }
            }

            if (EDITION == Edition.Standard)
            {
                return (RELEASE + " " + "");
            }
            else if (EDITION == Edition.Research)
            {
                return (RELEASE + " " + "r");
            }
            else if (EDITION == Edition.Ultimate)
            {
                return (RELEASE + " " + "u");
            }
            else if (EDITION == Edition.BigNumbers)
            {
                return (RELEASE + " " + "b");
            }
            else
            {
                return (RELEASE + " " + "!"); // Unknown Edition
            }
        }
    }
    public static string SHORT_VERSION
    {
        get
        {
            string version = VERSION;
            string[] parts = version.Split('.');
            if (parts.Length == 4)
            {
                int pos = version.LastIndexOf('.');
                if (pos > -1)
                {
                    version = version.Substring(0, pos);
                }
            }

            if (EDITION == Edition.Standard)
            {
                return (version + " " + RELEASE + " " + "");
            }
            else if (EDITION == Edition.Research)
            {
                return (version + " " + RELEASE + " " + "r");
            }
            else if (EDITION == Edition.Ultimate)
            {
                return (version + " " + RELEASE + " " + "u");
            }
            else if (EDITION == Edition.BigNumbers)
            {
                return (version + " " + RELEASE + " " + "b");
            }
            else
            {
                return (version + " " + RELEASE + " " + "!"); // Unknown Edition
            }
        }
    }
    public static string LONG_VERSION
    {
        get
        {
            return (VERSION + " " + RELEASE + "   " + EDITION + " " + "Edition");
        }
    }

    // Global Variables
    public static string DELIMITER = "\t";
    public static string SUB_DELIMITER = "|";
    public static string DATE_FORMAT = "yyyy-MM-dd";
    public static string TIME_FORMAT = "HH:mm:ss";
    public static string DATETIME_FORMAT = DATE_FORMAT + " " + TIME_FORMAT;
    public static string NUMBER_FORMAT = "000";
    public static int MAX_NUMBERS = int.MaxValue;

    // Global Folders
    public static string LANGUAGES_FOLDER = "Languages";
    public static string USERTEXT_FOLDER = "UserText";
    public static string TRANSLATIONS_FOLDER = "Translations";
    public static string IMAGES_FOLDER = "Images";
    public static string FONTS_FOLDER = "Fonts";
    public static string DATA_FOLDER = "Data";
    public static string AUDIO_FOLDER = "Audio";
    public static string EXTERNAL_AUDIO_FOLDER = "D:/Quran/Audio";
    public static string SCRIPTS_FOLDER = "Scripts";
    public static string RULES_FOLDER = "Rules";
    public static string VALUES_FOLDER = "Values";
    public static string NUMBERS_FOLDER = "Numbers";
    public static string INTERESTING_NUMBERS_FILENAME = "interesting_numbers.txt";
    public static string STATISTICS_FOLDER = "Statistics";
    public static string RESEARCH_FOLDER = "Research";
    public static string DRAWINGS_FOLDER = "Drawings";
    public static string BOOKMARKS_FOLDER = "Bookmarks";
    public static string HISTORY_FOLDER = "History";
    public static string HELP_FOLDER = "Help";
}
