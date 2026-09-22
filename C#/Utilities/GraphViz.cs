//http://implicitoperator.com/blog/2010/4/11/graphviz-c-sample.html
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

public static class GraphViz
{
    public static int Layout = 0;
    public static int Output = 9;
    public static int Preview = 1;
    public static string InitialDir1 = "";
    public static string InitialDir2 = "";
    public static string InitialDir3 = "";
    public static string binPath = "";

    public static string GenerateGraph(string dot_filename)
    {
        string tool_name = "dot.exe";
        switch (Layout)
        {
            case 0:
                tool_name = "dot";
                break;
            case 1:
                tool_name = "neato";
                break;
            case 2:
                tool_name = "twopi";
                break;
            case 3:
                tool_name = "circo";
                break;
            case 4:
                tool_name = "fdp";
                break;
            case 5:
                tool_name = "sfdp";
                break;
            case 6:
                tool_name = "patchwork";
                break;
            case 7:
                tool_name = "osage";
                break;
            default:
                tool_name = "dot";
                break;
        }

        //-Tps (PostScript),
        //-Tsvg -Tsvgz (Structured Vector Graphics),
        //-Tfig (XFIG graphics),
        //-Tpng -Tgif (bitmap graphics),
        //-Timap (imagemap files for httpd servers for each node or edge that has a non-null href attribute.),
        //-Tcmapx (client-side imagemap for use in html and xhtml).

        string graph_filename = dot_filename.Replace(".dot", ".ps");
        graph_filename = graph_filename.Insert(graph_filename.Length - 4, "_" + tool_name);
        FileHelper.DeleteFile(graph_filename);
        ExecuteCommand(binPath + "/" + tool_name + ".exe", string.Format(@"""{0}"" -o ""{1}"" -Tps", dot_filename, graph_filename));

        graph_filename = dot_filename.Replace(".dot", ".svg");
        graph_filename = graph_filename.Insert(graph_filename.Length - 4, "_" + tool_name);
        FileHelper.DeleteFile(graph_filename);
        ExecuteCommand(binPath + "/" + tool_name + ".exe", string.Format(@"""{0}"" -o ""{1}"" -Tsvg", dot_filename, graph_filename));

        //graph_filename = dot_filename.Replace(".dot", ".fig");
        //graph_filename = graph_filename.Insert(graph_filename.Length - 4, "_" + tool_name);
        //FileHelper.DeleteFile(graph_filename);
        //ExecuteCommand(binPath + "/" + tool_name + ".exe", string.Format(@"""{0}"" -o ""{1}"" -Tfig", dot_filename, graph_filename));

        //graph_filename = dot_filename.Replace(".dot", ".imap");
        //graph_filename = graph_filename.Insert(graph_filename.Length - 4, "_" + tool_name);
        //FileHelper.DeleteFile(graph_filename);
        //ExecuteCommand(binPath + "/" + tool_name + ".exe", string.Format(@"""{0}"" -o ""{1}"" -Timap", dot_filename, graph_filename));

        graph_filename = dot_filename.Replace(".dot", ".png");
        graph_filename = graph_filename.Insert(graph_filename.Length - 4, "_" + tool_name);
        FileHelper.DeleteFile(graph_filename);
        ExecuteCommand(binPath + "/" + tool_name + ".exe", string.Format(@"""{0}"" -o ""{1}"" -Tpng", dot_filename, graph_filename));

        FileHelper.WaitForFileReady(graph_filename);

        Thread.Sleep(3000); // must add this or picture won't display
        return graph_filename;
    }

    private static void ExecuteCommand(string command, string @params)
    {
        try
        {
            Process.Start(new ProcessStartInfo(command, @params) { CreateNoWindow = true, UseShellExecute = false });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
