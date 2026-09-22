using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        if (args != null)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Version path <version> [<file version>] [-ignore_foler1 ignore_foler2 ...]");
                Console.WriteLine("Ex1:    Version . 1.2.3.4");
                Console.WriteLine("Ex2:    Version . 1.2.3.4 -Common");
                Console.WriteLine("Ex2:    Version . 1.2.3.4 1.2.3.4 -Common -External -3rdParty");
                Console.WriteLine();
                Console.WriteLine("This program will update the VERSION variable in Globals\\Globals.cs.");
                Console.WriteLine("This program will update all AssemblyVersions except in ignore_folers.");
                return;
            }
            string path = args[0];


            if (args.Length > 1)
            {
                string version;
                string[] version_parts = args[1].Split('.');
                if (version_parts.Length == 1)
                {
                    version = args[1] + ".0.0";
                }
                else if (version_parts.Length == 2)
                {
                    version = args[1] + ".0";
                }
                else if (version_parts.Length == 3)
                {
                    version = args[1] + ".*";
                }
                else if (version_parts.Length == 4)
                {
                    version = args[1];
                }
                else
                {
                    Console.WriteLine("Usage: Version path <version> [<file version>] [-ignore_folder1 ignore_folder2 ...]");
                    Console.WriteLine("ERROR: " + args[1] + " should have 1, 2, 3 or 4 parts (major.minor.build.revision)");
                    return;
                }
                UpdateGlobals(path, version);

                string arg_2 = args[1];
                if (args.Length > 2)
                {
                    arg_2 = args[2];
                }
                string file_version = null;
                List<string> ignore_folders = null;
                if (arg_2.StartsWith("-")) // args[2] is ignore_folder
                {
                    // if no file_version specified, 
                    // then use version with 4th part as the primalogy value of surat Al-Fatiha (The Key)
                    // http://heliwave.com/Primalogy.pdf
                    file_version = arg_2 + ".8317";

                    // ignore_folders start at args[2]
                    ignore_folders = BuildIgnoreFolders(args, 2);
                }
                else // arg_2 is a file_version
                {
                    string[] file_version_parts = arg_2.Split('.');
                    if ((file_version_parts.Length < 3) || (file_version_parts.Length > 4))
                    {
                        Console.WriteLine("Usage: Version path <version> [<file version>] [-ignore_folder1 ignore_folder2 ...]");
                        Console.WriteLine("ERROR: " + file_version_parts + " must have 3 or 4 parts (major.minor.build.revision)");
                        return;
                    }

                    if (file_version_parts.Length == 3)
                    {
                        // no file_version specified, so use version with 4th part as PV of surat Al-Fatiha (The Key)
                        file_version = arg_2 + ".8317";
                    }
                    else
                    {
                        file_version = arg_2;
                    }

                    // ignore_folders start at args[3]
                    if (args.Length > 3)
                    {
                        ignore_folders = BuildIgnoreFolders(args, 3);
                    }
                }

                UpdateAssemblyInfos(path, version, file_version, ignore_folders);
            }
        }
    }
    private static List<string> BuildIgnoreFolders(string[] args, int ignore_folders_start)
    {
        List<string> result = new List<string>();
        if (args.Length > ignore_folders_start)
        {
            for (int i = ignore_folders_start; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    args[i] = args[i].Remove(0, 1);
                }
                else
                {
                    Console.WriteLine("ERROR: " + "Folder" + args[i] + " must start with a - sign to be ignored");
                    continue;
                }
                result.Add(args[i]);
            }
        }
        return result;
    }

    private static void UpdateGlobals(string path, string version)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            DirectoryInfo[] folders = folder.GetDirectories();
            foreach (DirectoryInfo subfolder in folders)
            {
                string filename = subfolder.FullName + "/" + "Globals.cs";
                if (File.Exists(filename))
                {
                    try
                    {
                        int pos = version.IndexOf('*');
                        if (pos > 1)
                        {
                            version = version.Substring(0, pos - 1);
                        }

                        string content = LoadFile(filename);
                        content = Regex.Replace(content, "VERSION = \"\\d+\\.\\d+(\\.\\d+(\\.\\d+)?)?", "VERSION = \"" + version);
                        SaveFile(filename, content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: " + filename + " has " + ex.Message);
                    }
                }
                else
                {
                    // nested call (depth first)
                    UpdateGlobals(subfolder.FullName, version);
                }
            }
        }
    }
    public static void UpdateAssemblyInfos(string path, string version, string file_version, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            DoUpdateAssemblyInfos(path, version, file_version, ignore_folders);

            DirectoryInfo[] folders = folder.GetDirectories();
            foreach (DirectoryInfo subfolder in folders)
            {
                if (ignore_folders != null)
                {
                    if (ignore_folders.Contains(subfolder.Name))
                    {
                        continue;
                    }
                }

                UpdateAssemblyInfos(subfolder.FullName, version, file_version, null);
            }
        }
    }
    private static void DoUpdateAssemblyInfos(string path, string version, string file_version, List<string> ignore_folders)
    {
        if ((path == ".") || (path == ".\\"))
        {
            path = Directory.GetCurrentDirectory();
        }

        DirectoryInfo folder = new DirectoryInfo(path);
        if (folder != null)
        {
            FileInfo[] files = folder.GetFiles("AssemblyInfo.cs");
            foreach (FileInfo file in files)
            {
                if (ignore_folders != null)
                {
                    if (ignore_folders.Contains(file.Directory.Name))
                    {
                        continue;
                    }
                }

                try
                {
                    string content = LoadFile(file.FullName);

                    // disallow * in version
                    content = Regex.Replace(content, "\\.\\*", ".0");

                    // update the AssemblyVersion
                    content = Regex.Replace(content, "AssemblyVersion\\(\"\\d+\\.\\d+(\\.\\d+(\\.\\d+)?)?", "AssemblyVersion(\"" + version);
                    // update the AssemblyFileVersion
                    content = Regex.Replace(content, "AssemblyFileVersion\\(\"\\d+\\.\\d+(\\.\\d+(\\.\\d+)?)?", "AssemblyFileVersion(\"" + file_version);

                    // restore the sample back to [1.0.0.*]
                    content = Regex.Replace(content, "// .assembly: AssemblyVersion\\(\"\\d+\\.\\d+\\.\\d+\\.\\d+\"\\)", "// [assembly: AssemblyVersion(\"1.0.0.*\")");

                    SaveFile(file.FullName, content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + file.FullName + " has " + ex.Message);
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
