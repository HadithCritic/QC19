using System;
using System.Collections.Generic;

namespace Model
{
    public class RecitationInfo
    {
        public const string DEFAULT_URL_PREFIX = "https://everyayah.com/data/";
        public const string DEFAULT_FILE_TYPE = "mp3";
        public static string UrlPrefix = DEFAULT_URL_PREFIX;
        public static string FileType = DEFAULT_FILE_TYPE;

        public string Url = null;
        public string Folder = null;
        public string Language = null;
        public string Reciter = null;
        public int Quality = 0;
        public string Name = null; // Language - Reciter
    }
}
