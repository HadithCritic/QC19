using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

public static class FileHelper
{
    // https://stackoverflow.com/questions/1406808/wait-for-file-to-be-freed-by-process
    public static bool IsFileReady(string filename)
    {
        try
        {
            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return false;
        }

        //file is not locked
        return true;
    }
    public static bool IsFolderReady(string filename, string filters)
    {
        if (Directory.Exists(filename))
        {
            DirectoryInfo directory = new DirectoryInfo(filename);
            if (directory != null)
            {
                FileInfo[] files = directory.GetFiles(filters);
                foreach (FileInfo file in files)
                {
                    if (!IsFileReady(file.FullName))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        return false;
    }
    public static void WaitForFileReady(string filename)
    {
        while (!IsFileReady(filename))
        {
            Thread.Sleep(114);
        }
    }
    public static void WaitForFolderReady(string filename, string filters)
    {
        while (!IsFolderReady(filename, filters))
        {
            Thread.Sleep(114);
        }
    }
    public static void DeleteFile(string filename)
    {
        if (File.Exists(filename))
        {
            WaitForFileReady(filename);
            File.Delete(filename);
        }
    }
    public static void BackupFile(string filename)
    {
        if (File.Exists(filename))
        {
            WaitForFileReady(filename);

            string backup_folder = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + "Backup";
            if (!Directory.Exists(backup_folder))
            {
                Directory.CreateDirectory(backup_folder);
            }
            if (Directory.Exists(backup_folder))
            {
                string path = Path.GetFileName(filename);
                int pos = path.LastIndexOf(".");
                string backup = backup_folder + Path.DirectorySeparatorChar + path.Insert(pos, "_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss"));
                if (!File.Exists(backup))
                {
                    File.Copy(filename, backup);
                }
            }
        }
    }

    public static void SaveLetters(string filename, char[] characters)
    {
        DoSaveLetters(filename, characters, Encoding.Unicode);
    }
    public static void SaveWords(string filename, List<string> words)
    {
        DoSaveWords(filename, words, Encoding.Unicode);
    }
    public static void SaveValues(string filename, List<long> values)
    {
        DoSaveValues(filename, values, Encoding.Unicode);
    }
    public static void SaveValues(string filename, List<int> values)
    {
        DoSaveValues(filename, values, Encoding.Unicode);
    }
    private static void DoSaveLetters(string filename, char[] characters, Encoding encoding)
    {
        if (String.IsNullOrEmpty(filename)) return;

        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            using (StreamWriter writer = new StreamWriter(filename, false, encoding))
            {
                foreach (char character in characters)
                {
                    if (character == '\0')
                    {
                        continue;
                    }
                    writer.Write(character);
                }
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    private static void DoSaveWords(string filename, List<string> words, Encoding encoding)
    {
        if (String.IsNullOrEmpty(filename)) return;

        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            using (StreamWriter writer = new StreamWriter(filename, false, encoding))
            {
                foreach (string word in words)
                {
                    if (String.IsNullOrEmpty(word))
                    {
                        continue;
                    }
                    writer.WriteLine(word);
                }
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    private static void DoSaveValues(string filename, List<long> values, Encoding encoding)
    {
        if (String.IsNullOrEmpty(filename)) return;

        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            using (StreamWriter writer = new StreamWriter(filename, false, encoding))
            {
                foreach (long value in values)
                {
                    writer.WriteLine(value.ToString());
                }
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    private static void DoSaveValues(string filename, List<int> values, Encoding encoding)
    {
        if (String.IsNullOrEmpty(filename)) return;

        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            using (StreamWriter writer = new StreamWriter(filename, false, encoding))
            {
                foreach (int value in values)
                {
                    writer.WriteLine(value.ToString());
                }
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }

    public static void AppendLine(string filename, string line)
    {
        if (String.IsNullOrEmpty(filename)) return;

        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            using (StreamWriter writer = new StreamWriter(filename, true, Encoding.Unicode))
            {
                writer.WriteLine(line);
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    public static void SaveLines(string filename, List<string> lines)
    {
        SaveWords(filename, lines);
    }
    public static void SaveText(string filename, string text)
    {
        DoSaveText(filename, text, Encoding.Unicode);
    }
    private static void DoSaveText(string filename, string text, Encoding encoding)
    {
        if (String.IsNullOrEmpty(filename)) return;

        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            using (StreamWriter writer = new StreamWriter(filename, false, encoding))
            {
                writer.Write(text);
                writer.Close();
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }
    public static List<string> LoadLines(string filename)
    {
        if (String.IsNullOrEmpty(filename)) return new List<string>();

        List<string> result = new List<string>();
        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            if (File.Exists(filename))
            {
                WaitForFileReady(filename);

                using (StreamReader reader = File.OpenText(filename))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        result.Add(line);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ".\r\n\r\n" + ex.StackTrace);
        }
        return result;
    }
    public static string LoadText(string filename)
    {
        if (String.IsNullOrEmpty(filename)) return "";

        StringBuilder str = new StringBuilder();
        try
        {
            filename = filename.Replace("|", "+"); // remove illegal char | in filename
            if (File.Exists(filename))
            {
                WaitForFileReady(filename);

                using (StreamReader reader = File.OpenText(filename))
                {
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        str.AppendLine(line);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ".\r\n\r\n" + ex.StackTrace);
        }
        return str.ToString();
    }

    public static void DisplayFile(string filename)
    {
        if (String.IsNullOrEmpty(filename)) return;

        filename = filename.Replace("|", "+"); // remove illegal char | in filename
        if (File.Exists(filename))
        {
            WaitForFileReady(filename);
            System.Diagnostics.Process.Start(filename);
        }
    }
}
