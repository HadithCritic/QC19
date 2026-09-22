using System;
using System.Windows.Forms;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (!NETVersion.HasNET45OrLater())
        {
            try
            {
                MessageBox.Show("This application requires a higher .NET version than " + NETVersion.GetVersion() + "\r\n\r\n" +
                                "Automatic download of .NET 4.5.2 will start now." + "\r\n" +
                                "Once the download is complete, install it and restart the application.",
                                Application.ProductName);

                string uri = "https://microsoft.com/en-us/download/confirmation.aspx?id=42642";
                System.Diagnostics.Process.Start(uri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
            finally
            {
                Environment.Exit(-1);
            }
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        MainForm form = new MainForm();
        if (form != null)
        {
            try
            {
                Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
        }
    }
}
