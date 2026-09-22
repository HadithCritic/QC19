using System;
using System.Collections.Generic;

namespace Model
{
    public class TranslationInfo
    {
        public const string DEFAULT_URL_PREFIX = "https://tanzil.net/trans/";
        public const string DEFAULT_FILE_TYPE = "txt";
        public const string DEFAULT_ICON_URL_PREFIX = "https://tanzil.net/pub/img/flag/";
        public static string UrlPrefix = DEFAULT_URL_PREFIX;
        public static string FileType = DEFAULT_FILE_TYPE;
        public static string IconUrlPrefix = DEFAULT_ICON_URL_PREFIX;

        public string Url = null;
        public string Flag = null;
        public string Language = null;
        public string Translator = null;
        public string Name = null; // Language - Translator
    }
}
