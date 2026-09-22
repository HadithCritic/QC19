using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

// Splash screen adapted from
// https://stackoverflow.com/questions/48916/multi-threaded-splash-screen-in-c
public partial class SplashForm : Form
{
    private const int SLEEP_TIME = 50;

    public SplashForm()
    {
        InitializeComponent();

        update_version_delegate = this.UpdateVersion;
        update_information_delegate = this.UpdateInformation;
        update_progress_delegate = this.UpdateProgress;

        InitialLettersLabel.BackColor = Globals.EDITION_BACKCOLOR;
        InitialLettersLabel.ForeColor = Globals.EDITION_FORECOLOR;
        ProgressBar.BackColor = Globals.EDITION_BACKCOLOR;
        ProgressBar.ForeColor = Globals.EDITION_FORECOLOR;
    }

    private delegate void VersionDelegate(string value);
    private VersionDelegate update_version_delegate;
    private void UpdateVersion(string value)
    {
        if (this.Handle == null)
        {
            return;
        }
        this.VersionLabel.Text = value;
        this.VersionLabel.Refresh();
    }
    public string Version
    {
        get { return VersionLabel.Text; }
        set
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(update_version_delegate, value);
            }
            else
            {
                try
                {
                    // recursive retry
                    Thread.Sleep(SLEEP_TIME);
                    Version = value;
                }
                catch (Exception /*ex*/)
                {
                    this.Close();
                }
            }
        }
    }

    private delegate void InformationDelegate(string value);
    private InformationDelegate update_information_delegate;
    private void UpdateInformation(string value)
    {
        if (this.Handle == null)
        {
            return;
        }
        this.InformationLabel.Text = value;
        this.InformationLabel.Refresh();
    }
    public string Information
    {
        get { return InformationLabel.Text; }
        set
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(update_information_delegate, value);
            }
            else
            {
                try
                {
                    // recursive retry
                    Thread.Sleep(SLEEP_TIME);
                    Information = value;
                }
                catch (Exception /*ex*/)
                {
                    this.Close();
                }
            }
        }
    }

    private delegate void ProgressDelegate(int value);
    private ProgressDelegate update_progress_delegate;
    private void UpdateProgress(int value)
    {
        if (this.Handle == null)
        {
            return;
        }
        this.ProgressBar.Value = value;
        this.ProgressBar.Refresh();
    }
    public int Progress
    {
        get { return ProgressBar.Value; }
        set
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(update_progress_delegate, value);
            }
            else
            {
                try
                {
                    // recursive retry
                    Thread.Sleep(SLEEP_TIME);
                    Progress = value;
                }
                catch (Exception /*ex*/)
                {
                    this.Close();
                }
            }
        }
    }

    private void SplashForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }
    }
    private void SplashForm_Shown(object sender, EventArgs e)
    {
    }
    private void CloseLabel_Click(object sender, EventArgs e)
    {
        this.Close();
    }

    private void LinkLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            Control control = (sender as Control);
            if (control != null)
            {
                if (control.Tag != null)
                {
                    if (!String.IsNullOrEmpty(control.Tag.ToString()))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(control.Tag.ToString());
                        }
                        catch //(Exception ex)
                        {
                            //MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
                        }
                    }
                }
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
}
