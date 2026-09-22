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

        if ((args != null) && (args.Length > 0))
        {
            if (args[0] == "")
            {
                Globals.EDITION = Edition.Standard;
            }
            else if (args[0].ToUpper() == "R")
            {
                Globals.EDITION = Edition.Research;
            }
            else if (args[0].ToUpper() == "U")
            {
                Globals.EDITION = Edition.Ultimate;
            }
            else if (args[0].ToUpper() == "B")
            {
                Globals.EDITION = Edition.BigNumbers;
            }
            else // default
            {
                Globals.EDITION = Edition.Standard;
            }
        }
        else
        {
            if (Control.ModifierKeys == (Keys.Shift | Keys.Control))
            {
                Globals.EDITION = Edition.BigNumbers;
                Globals.MAX_NUMBERS = int.MaxValue / 8;
            }
            else if (Control.ModifierKeys == Keys.Shift)
            {
                Globals.EDITION = Edition.Research;
            }
            else if (Control.ModifierKeys == Keys.Control)
            {
                Globals.EDITION = Edition.Ultimate;
            }
            else // default
            {
                Globals.EDITION = Edition.Standard;
            }
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        MainForm form = new MainForm();
        if (form != null)
        {
            try
            {
                if ((args != null) && (args.Length > 0))
                {
                    long number = 0L;
                    if (long.TryParse(args[0], out number))
                    {
                        form.Tag = number.ToString();
                    }
                }
                Application.Run(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
            }
        }
    }
}
