using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args != null)
        {
            string path = null;
            string file_pattern = null;
            string source = null;
            string target = null;
            List<string> ignore_folders = new List<string>();

            if (args.Length < 3)
            {
                Console.WriteLine("Usage: Replace path file_pattern source target [-ignore_foler1 -ignore_foler2 ...]");
            }
            else // if (args.Length >= 3)
            {
                path = args[0];
                file_pattern = args[1];
                source = args[2];

                if (args.Length >= 4)
                {
                    if (!args[3].StartsWith("-"))
                    {
                        target = args[3];
                    }
                    else
                    {
                        if (args[3].StartsWith("-"))
                        {
                            ignore_folders.Add(args[3].Remove(0, 1));
                        }
                    }

                    if (args.Length >= 5)
                    {
                        for (int i = 4; i < args.Length; i++)
                        {
                            if (args[i].StartsWith("-"))
                            {
                                ignore_folders.Add(args[i].Remove(0, 1));
                            }
                        }
                    }
                }

                Replace(path, file_pattern, source, target, ignore_folders);
            }
        }
    }
    private static void Replace(string path, string file_pattern, string source, string target, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            if (ignore_folders != null)
            {
                if (ignore_folders.Contains(folder.Name))
                {
                    return;
                }
            }

            DoReplace(path, file_pattern, source, target, ignore_folders);

            DirectoryInfo[] folders = folder.GetDirectories();
            if ((folders != null) && (folders.Length > 0))
            {
                foreach (DirectoryInfo subfolder in folders)
                {
                    Replace(subfolder.FullName, file_pattern, source, target, ignore_folders);
                }
            }
        }
    }
    private static void DoReplace(string path, string file_pattern, string source, string target, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            if (ignore_folders != null)
            {
                if (ignore_folders.Contains(folder.Name))
                {
                    return;
                }
            }

            FileInfo[] files = folder.GetFiles(file_pattern);
            if ((files != null) && (files.Length > 0))
            {
                foreach (FileInfo file in files)
                {
                    try
                    {
                        string content = LoadFile(file.FullName);
                        if (source.ToUpper() == "RELEASE")
                        {
                            // Replace Globals.cs: RELEASE = "YYYYMMDD";
                            content = Regex.Replace(content, source.ToUpper() + " = \"(.*)\"", source.ToUpper() + " = \"" + target + "\"");
                        }
                        else
                        {
                            if (target == "\"\"") // remove source
                            {
                                content = content.Replace(source, "");
                            }
                            else
                            {
                                content = content.Replace(source, target);
                            }
                        }
                        SaveFile(file.FullName, content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: " + file.Name + " has " + ex.Message);
                    }
                }
            }
        }
    }
    private static string LoadFile(string filename)
    {
        using (StreamReader reader = new StreamReader(filename))
        {
            if (reader != null)
            {
                return reader.ReadToEnd();
            }
        }
        return "";
    }
    private static void SaveFile(string filename, string content)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            if (writer != null)
            {
                writer.Write(content);
            }
        }
    }
}
