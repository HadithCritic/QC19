using System;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;

public class Downloader : WebClient
{
    private int timeout;

    private Downloader(int timeout)
    {
        this.timeout = timeout;
    }

    protected override WebRequest GetWebRequest(Uri uri)
    {
        WebRequest result = base.GetWebRequest(uri);
        if (result != null)
        {
            result.Timeout = this.timeout;
        }
        return result;
    }

    /// <summary>
    /// Download file from uri and save to filepath (default timeout = 30000 ms)
    /// </summary>
    /// <param name="uri">source url</param>
    /// <param name="filepath">target full path</param>
    public static void Download(string uri, string filepath)
    {
        Download(uri, filepath, 30000);
    }
    /// <summary>
    /// Download file from uri and save to filepath
    /// </summary>
    /// <param name="uri">source url</param>
    /// <param name="filepath">target target full path</param>
    /// <param name="timeout">milliseconds</param>
    public static void Download(string uri, string filepath, int timeout)
    {
        string folder = Path.GetDirectoryName(filepath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            Thread.Sleep(1000);
        }

        using (Downloader web_client = new Downloader(timeout))
        {
            byte[] data = web_client.DownloadData(uri);
            if ((data != null) && (data.Length > 0))
            {
                File.WriteAllBytes(filepath, data);
            }
            else
            {
                throw new Exception("Invalid server address.\r\nPlease correct address in .INI file");
            }
        }
    }

    //// Async example from MainForm.cs
    //private void DownloadAsync(string uri)
    //{
    //    using (WebClient web_client = new WebClient())
    //    {
    //        web_client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDataCompleted);
    //        web_client.DownloadDataAsync(new Uri(uri));
    //    }
    //}
    //private void DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
    //{
    //    // WARNING: runs on different thread to UI thread
    //    byte[] data = e.Result;

    //    string filepath = "Downloads";
    //    string download_folder = Path.GetDirectoryName(filepath);
    //    if (!Directory.Exists(download_folder))
    //    {
    //        Directory.CreateDirectory(download_folder);
    //    }

    //    if ((data != null) && (data.Length > 0))
    //    {
    //        File.WriteAllBytes(filepath, data);
    //    }
    //    else
    //    {
    //        throw new Exception("Invalid server address.\r\nPlease correct address in .INI file");
    //    }
    //}
}
