using System;
using System.Windows.Forms;
//using System.Threading;
//using System.Runtime.InteropServices;
//using Microsoft.Win32;

static class Program
{
    //// disable the X close icon
    //const int MF_BYPOSITION = 0x400;
    //[DllImport("User32")]
    //private static extern int RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);
    //[DllImport("User32")]
    //private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    //[DllImport("User32")]
    //private static extern int GetMenuItemCount(IntPtr hWnd);
    //static void DisableCloseIcon(IntPtr system_menu_handle)
    //{
    //    int system_menu_item_count = GetMenuItemCount(system_menu_handle);
    //    RemoveMenu(system_menu_handle, system_menu_item_count - 1, MF_BYPOSITION);
    //}

    /// <summary>
    /// The main entry point for the Application.
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

        try
        {
            if ((args != null) && (args.Length > 0))
            {
                if (args[0] == "")
                {
                    Globals.EDITION = Edition.Standard;
                }
                else if (args[0].ToLower() == "r")
                {
                    Globals.EDITION = Edition.Research;
                }
                else if (args[0].ToLower() == "u")
                {
                    Globals.EDITION = Edition.Ultimate;
                }
                else if (args[0].ToLower() == "b")
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, Application.ProductName);
        }

        //// enforce single instance
        //bool m_first_instance = true;
        //using (Mutex mutex = new Mutex(true, Application.ProductName, out m_first_instance))
        //{
        //    if (m_first_instance)
        //    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        MainForm form = new MainForm();
        if (form != null)
        {
            //// disable the X close button of the form
            //IntPtr system_menu_handle = GetSystemMenu(form.Handle, false);
            //int system_menu_item_count = GetMenuItemCount(system_menu_handle);

            //RemoveMenu(system_menu_handle, system_menu_item_count - 1, MF_BYPOSITION);
            // or
            //DisableCloseIcon(system_menu_handle);

            Application.Run(form);
        }
        //    }
        //    else
        //    {
        //        Windows windows = new Windows(true, true);
        //        if (windows != null)
        //        {
        //            foreach (Window window in windows)
        //            {
        //                if (window.Title.StartsWith(Application.ProductName))
        //                {
        //                    window.Visible = true;
        //                    window.BringToFront();
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
