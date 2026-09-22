using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

public partial class MainForm : Form
{
    private void FixMicrosoft(object sender, KeyPressEventArgs e)
    {
        // stop annoying beep due to parent not having an AcceptButton
        if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Escape))
        {
            e.Handled = true;
        }
        // enable Ctrl+A to SelectAll in TextBox and RichTextBox
        if ((ModifierKeys == Keys.Control) && (e.KeyChar == (char)1))
        {
            TextBoxBase control = (sender as TextBoxBase);
            if (control != null)
            {
                control.SelectAll();
                e.Handled = true;
            }
        }
    }

    private static int ROWS = 30;
    private static int COLS = 19;
    private TextBox[,] controls = new TextBox[ROWS, COLS];

    private string m_ini_filename = null;
    private void Initialize()
    {
        this.Top = Screen.PrimaryScreen.WorkingArea.Top;
        this.Left = Screen.PrimaryScreen.WorkingArea.Left;
        this.Width = (m_dpi == 96.0F) ? 1120 : 1345;
        this.Height = (m_dpi == 96.0F) ? 668 : 738;
    }
    private void LoadSettings()
    {
        if (File.Exists(m_ini_filename))
        {
            using (StreamReader reader = File.OpenText(m_ini_filename))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!String.IsNullOrEmpty(line))
                        {
                            string[] parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                switch (parts[0])
                                {
                                    case "Top":
                                        {
                                            this.Top = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Left":
                                        {
                                            this.Left = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Width":
                                        {
                                            this.Width = int.Parse(parts[1]);
                                        }
                                        break;
                                    case "Height":
                                        {
                                            this.Height = int.Parse(parts[1]);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Initialize();
                }
            }
        }
        else // first start
        {
            Initialize();
        }
    }
    private void SaveSettings()
    {
        try
        {
            using (StreamWriter writer = File.CreateText(m_ini_filename))
            {
                writer.WriteLine("[Window]");
                writer.WriteLine("Top=" + this.Top);
                writer.WriteLine("Left=" + this.Left);
                writer.WriteLine("Width=" + this.Width);
                writer.WriteLine("Height=" + this.Height);
            }
        }
        catch
        {
            // silence IO error in case running from read-only media (CD/DVD)
        }
    }

    private float m_dpi = 0F;
    public MainForm()
    {
        using (Graphics graphics = this.CreateGraphics())
        {
            m_dpi = graphics.DpiX;    // 100% = 96.0F,   125% = 120.0F,   150% = 144.0F
        }
        InitializeComponent();
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

        // Copy label
        {
            Label control = new Label();
            if (control != null)
            {
                control.Width = 16;
                control.Height = 12;
                control.Top = -3;
                control.Left = 0;
                control.Cursor = Cursors.Hand;
                control.TextAlign = ContentAlignment.TopLeft;
                control.Font = new Font("Arial", 10);
                control.Text = "˄";
                control.MouseEnter += Label_MouseEnter;
                control.MouseLeave += Label_MouseLeave;
                control.Click += CopyLabel_Click;
                ToolTip.SetToolTip(control, "Copy");
                MainPanel.Controls.Add(control);
            }
        }

        // Paste label
        {
            Label control = new Label();
            if (control != null)
            {
                control.Width = 16;
                control.Height = 12;
                control.Top = 8;
                control.Left = 0;
                control.Cursor = Cursors.Hand;
                control.TextAlign = ContentAlignment.TopLeft;
                control.Font = new Font("Arial", 10);
                control.Text = "˅";
                control.MouseEnter += Label_MouseEnter;
                control.MouseLeave += Label_MouseLeave;
                control.Click += PasteLabel_Click;
                ToolTip.SetToolTip(control, "Paste");
                MainPanel.Controls.Add(control);
            }
        }

        // Row numbers (DeleteRow)
        for (int i = 0; i < ROWS; i++)
        {
            Label control = new Label();
            if (control != null)
            {
                control.Width = (m_dpi == 96.0F) ? 20 : 24;
                control.Height = 19;
                control.Top = ((m_dpi == 96.0F) ? 23 : 27) + (i * (control.Height + ((m_dpi == 96.0F) ? 1 : 3)));
                control.Left = 0;
                control.TextAlign = ContentAlignment.MiddleRight;
                control.Font = new Font("Arial", 8);
                control.Text = (i + 1).ToString();
                ToolTip.SetToolTip(control, "Delete, Ctrl to Clear");
                control.Cursor = Cursors.PanEast;
                control.Click += DeleteRowLabel_Click;
                MainPanel.Controls.Add(control);
            }
        }

        // Column headings
        for (int j = 0; j < COLS; j++)
        {
            Label control = new Label();
            if (control != null)
            {
                control.Width = (m_dpi == 96.0F) ? 57 : 69;
                control.Height = 19;
                control.Top = 0;
                control.Left = 19 + (j * control.Width + 2);
                control.TextAlign = ContentAlignment.MiddleCenter;
                control.Font = new Font("Arial", 8);
                MainPanel.Controls.Add(control);

                switch (j)
                {
                    case 0: { control.Text = "i"; ToolTip.SetToolTip(control, "\tIndex\r\nAuto-fill, Shift to go back"); control.Click += IndexLabel_Click; control.Cursor = Cursors.PanSouth; break; }
                    default: { control.Text = "D" + j.ToString(); break; }
                }
            }
        }

        // TextBox cells
        for (int i = 0; i < ROWS; i++)
        {
            for (int j = 0; j < COLS; j++)
            {
                TextBox control = new TextBox();
                if (control != null)
                {
                    control.Width = (m_dpi == 96.0F) ? 57 : 69;
                    control.Height = 21;
                    control.Top = 19 + (i * control.Height + 1);
                    control.Left = 19 + (j * control.Width + 2);
                    control.TextAlign = HorizontalAlignment.Center;
                    control.Font = new Font("Arial", 11);
                    control.MaxLength = 7;
                    MainPanel.Controls.Add(control);

                    control.KeyPress += FixMicrosoft;
                    if (j == 0) control.TextChanged += TextBox_TextChanged;
                    control.KeyDown += TextBox_KeyDown;

                    if (j == 0) control.AllowDrop = true;
                    if (j == 0) control.MouseDown += Control_MouseDown;
                    if (j == 0) control.DragEnter += Control_DragEnter;
                    if (j == 0) control.DragDrop += Control_DragDrop;
                    control.MouseHover += Control_MouseHover;

                    switch (j)
                    {
                        case 0:
                            control.BackColor = Color.White;
                            break;
                        default:
                            control.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[7];
                            break;
                    }

                    controls[i, j] = control;
                }
            }
        }

        AboutToolStripMenuItem.Font = new Font(AboutToolStripMenuItem.Font, AboutToolStripMenuItem.Font.Style | FontStyle.Bold);

        m_ini_filename = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".ini");
        LoadSettings();
    }
    private void ClearCells()
    {
        if (controls != null)
        {
            for (int i = 0; i < ROWS; i++)
            {
                ClearCells(i);
            }
        }
    }
    private void ClearCells(int row)
    {
        if (controls != null)
        {
            int i = row;
            {
                for (int j = 0; j < COLS; j++)
                {
                    TextBox control = controls[i, j];
                    if (control != null)
                    {
                        control.Text = "";
                        switch (j)
                        {
                            case 0:
                                control.BackColor = Color.White;
                                break;
                            default:
                                control.BackColor = Numbers.NUMBER_TYPE_BACKCOLORS[7];
                                break;
                        }
                    }
                }
            }
        }
    }

    private void DeleteRowLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Control)
        {
            ClearLabel_Click(sender, e);
        }
        else
        {
            Control control = (sender as Label);
            if (control != null)
            {
                int i = int.Parse(control.Text) - 1;
                if (controls != null)
                {
                    for (int j = 0; j < COLS; j++)
                    {
                        controls[i, j].Text = "";
                    }
                    controls[i, 0].Focus();
                }
            }
        }
    }
    private void ClearLabel_Click(object sender, EventArgs e)
    {
        if (controls != null)
        {
            for (int i = 0; i < ROWS; i++)
            {
                controls[i, 0].Text = "";
            }
            controls[0, 0].Focus();
        }
    }
    private void Label_MouseEnter(object sender, EventArgs e)
    {
        Control control = sender as Label;
        if (control != null)
        {
            control.BackColor = SystemColors.Info;
        }
    }
    private void Label_MouseLeave(object sender, EventArgs e)
    {
        Control control = sender as Label;
        if (control != null)
        {
            control.BackColor = SystemColors.Control;
        }
    }
    private void CopyLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + "_" + "Numbers.txt";

                StringBuilder str = new StringBuilder();
                str.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------------------------------");
                str.AppendLine("i" + "\t");
                for (int i = 0; i < ROWS; i++)
                {
                    str.AppendLine("D" + (i + 1).ToString() + "\t");
                }
                str.Remove(str.Length - 1, 1);
                str.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------------------------------");
                for (int i = 0; i < ROWS; i++)
                {
                    for (int j = 0; j < COLS; j++)
                    {
                        str.Append(controls[i, j].Text + "\t");
                    }
                    if (str.Length > 0)
                    {
                        str.Remove(str.Length - 1, 1);
                    }
                    str.AppendLine();
                }
                str.AppendLine("----------------------------------------------------------------------------------------------------------------------------------------------------------------");

                FileHelper.SaveText(path, str.ToString());
                FileHelper.DisplayFile(path);
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private void PasteLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            string text = Clipboard.GetText();
            string[] lines = text.Split('\n');

            char[] separators = { ' ', '.', ',', ';', '\t', 't', 's', 'n' };
            List<int> numbers = new List<int>();
            foreach (string line in lines)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    int number = 0;
                    //if (Char.IsDigit(line[0])) // 0..9
                    //{
                    string[] parts = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (int.TryParse(parts[0], out number))
                    {
                        numbers.Add(number);
                    }
                    else
                    {
                        numbers.Add(0);
                    }
                    //}
                    //else
                    //{
                    //    numbers.Add(0);
                    //}
                }
                else
                {
                    numbers.Add(0);
                }
            }

            if (numbers.Count <= ROWS)
            {
                for (int i = 0; i < numbers.Count; i++)
                {
                    if (numbers[i] > 0)
                    {
                        controls[i, 0].Text = numbers[i].ToString();
                    }
                    else
                    {
                        controls[i, 0].Text = "";
                    }
                }
                controls[0, 0].Focus();
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }
    private int batch_number = -1;
    private void IndexLabel_Click(object sender, EventArgs e)
    {
        if (ModifierKeys == Keys.Shift)
        {
            if (batch_number > 0)
            {
                batch_number--;
            }
            else
            {
                batch_number = -1;
                ClearLabel_Click(sender, e);
                return;
            }
        }
        else
        {
            batch_number++;
        }

        if (controls != null)
        {
            for (int i = 0; i < ROWS; i++)
            {
                controls[i, 0].Text = ((i + 1) + (batch_number * ROWS)).ToString();
            }
            controls[0, 0].Focus();
        }
    }

    private string DataFormatName = Application.ProductName;
    private void Control_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Control source = (sender as Control);
            if (source != null)
            {
                DataObject data = new DataObject(DataFormatName, source);
                source.DoDragDrop(data, DragDropEffects.Move);
            }
        }
    }
    private void Control_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormatName))
            e.Effect = e.AllowedEffect;
        else
            e.Effect = DragDropEffects.None;
    }
    private void Control_DragDrop(object sender, DragEventArgs e)
    {
        Control target = (sender as Control);
        if (target != null)
        {
            Control source = (Control)e.Data.GetData(DataFormatName);
            if (source != null)
            {
                string temp = target.Text;
                target.Text = source.Text;
                source.Text = temp;
            }
        }
    }
    private void Control_MouseHover(object sender, EventArgs e)
    {
        Control control = sender as Control;
        if (control != null)
        {
            try
            {
                string text = control.Text;
                if (!String.IsNullOrEmpty(text))
                {
                    long number = (long)double.Parse(text);
                    string factors_str = Numbers.FactorizeToString(number);
                    ToolTip.SetToolTip(control, factors_str);
                }
            }
            catch
            {
                ToolTip.SetToolTip(control, null);
            }
        }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        VersionLabel.Text = Globals.SHORT_VERSION;

        if (this.Top < 0)
        {
            Initialize();
        }
    }
    private void MainForm_Shown(object sender, EventArgs e)
    {
        NotifyIcon.Visible = true;
    }
    private bool m_was_maximized = false;
    private void MainForm_Resize(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Visible = false; // send to system try instead of minimize
            this.WindowState = FormWindowState.Normal;
        }
        else
        {
            m_was_maximized = this.WindowState == FormWindowState.Maximized;
        }
    }
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        //// prevent user from closing from the X close button
        //if (e.CloseReason == CloseReason.UserClosing)
        //{
        //    e.Cancel = true;
        //    this.Visible = false;
        //}
    }
    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        CloseApplication();
    }
    private void CloseApplication()
    {
        // remove icon from tray
        if (NotifyIcon != null)
        {
            NotifyIcon.Visible = false;
            NotifyIcon.Dispose();
        }

        SaveSettings();
    }

    private void IncrementIndex(object sender)
    {
        Point point = GetControlLocation(sender);
        if (point != null)
        {
            TextBox index_control = controls[point.X, 0];
            if (index_control != null)
            {
                index_control.Text = index_control.Text.Replace(" ", "");
                if (index_control.Text.Length == 0)
                {
                    index_control.Text = "0";
                    index_control.Refresh();
                }

                int index = 0;
                if (int.TryParse(index_control.Text, out index))
                {
                    if (index < int.MaxValue) index++;
                    index_control.Text = index.ToString();
                    index_control.Refresh();
                }
            }
        }
    }
    private void DecrementIndex(object sender)
    {
        Point point = GetControlLocation(sender);
        if (point != null)
        {
            TextBox index_control = controls[point.X, 0];
            if (index_control != null)
            {
                index_control.Text = index_control.Text.Replace(" ", "");
                if (index_control.Text.Length == 0)
                {
                    index_control.Text = "0";
                    index_control.Refresh();
                }

                int index = 0;
                if (int.TryParse(index_control.Text, out index))
                {
                    if (index > 1) index--;
                    index_control.Text = index.ToString();
                    index_control.Refresh();
                }
            }
        }
    }
    private Point GetControlLocation(object sender)
    {
        for (int i = 0; i < ROWS; i++)
        {
            for (int j = 0; j < COLS; j++)
            {
                if (sender == controls[i, j])
                {
                    return new Point(i, j);
                }
            }
        }
        return new Point(-1, -1);
    }
    private void TextBox_TextChanged(object sender, EventArgs e)
    {
        Point point = GetControlLocation(sender);
        if (point != null)
        {
            TextBox index_control = controls[point.X, 0];
            if (index_control != null)
            {
                index_control.Text = index_control.Text.Replace(" ", "");

                int number = 0;
                if (int.TryParse(index_control.Text, out number))
                {
                    index_control.ForeColor = Numbers.GetNumberForeColor(number);

                    if (number > 0)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        try
                        {
                            int i = number - 1;
                            for (int c = 1; c < COLS; c++)
                            {
                                try
                                {
                                    long d = Numbers.NumberDimensions[c - 1][i];
                                    controls[point.X, c].Text = d.ToString();
                                    controls[point.X, c].ForeColor = Numbers.GetNumberForeColor(d);
                                }
                                catch
                                {
                                    controls[point.X, c].Text = "";
                                }
                            }
                        }
                        catch
                        {
                            ClearCells(point.X);
                        }
                        finally
                        {
                            this.Cursor = Cursors.Default;
                        }
                    }
                    else
                    {
                        ClearCells(point.X);
                    }
                }
                else
                {
                    ClearCells(point.X);
                }
            }
        }
    }
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up)
        {
            IncrementIndex(sender);
        }
        else if (e.KeyCode == Keys.Down)
        {
            DecrementIndex(sender);
        }
        else if (e.KeyCode == Keys.Enter)
        {
            Point point = GetControlLocation(sender);
            if (point != null)
            {
                if (point.Y > 0)
                {
                    Control index_control = controls[point.X, 0] as TextBox;
                    if (index_control != null)
                    {
                        long number = 0L;
                        if (long.TryParse((sender as TextBox).Text, out number))
                        {
                            int index = Numbers.NumberDimensionIndexOf(point.Y, number) + 1;
                            index_control.Text = index.ToString();

                            if (index == 0)
                            {
                                (sender as TextBox).Text = "";
                                index_control.Text = "";
                            }
                        }
                    }
                }
            }
        }
    }
    private void GenerateSampleDataLabel_Click(object sender, EventArgs e)
    {
        this.Cursor = Cursors.WaitCursor;
        try
        {
            if (Directory.Exists(Globals.NUMBERS_FOLDER))
            {
                string path = Globals.NUMBERS_FOLDER + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") + "_" + "Indices.txt";

                StringBuilder str = new StringBuilder();
                str.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------");
                str.AppendLine("i" + "\t");
                for (int i = 0; i < ROWS; i++)
                {
                    str.AppendLine("D" + (i + 1).ToString() + "\t");
                }
                str.Remove(str.Length - 1, 1);
                str.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------");

                if (controls != null)
                {
                    for (int n = 0; n < 729; n++)
                    {
                        for (int i = 0; i < ROWS; i++)
                        {
                            controls[i, 0].Text = ((i + 1) + (n * ROWS)).ToString();
                        }

                        for (int i = 0; i < ROWS; i++)
                        {
                            for (int j = 0; j < COLS; j++)
                            {
                                str.Append(controls[i, j].Text + "\t");
                            }
                            if (str.Length > 0)
                            {
                                str.Remove(str.Length - 1, 1);
                            }
                            str.AppendLine();
                        }
                    }
                }

                str.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------");
                FileHelper.SaveText(path, str.ToString());
                FileHelper.DisplayFile(path);
            }
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if ((this.Visible == true) && (139 == 319))
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
                if (this.WindowState == FormWindowState.Minimized)
                {
                    if (m_was_maximized)
                    {
                        this.WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Normal;
                    }
                }
                this.Activate();    // bring to foreground
                this.BringToFront();
            }
        }
    }
    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show
        (
            Application.ProductName + "  v" + Application.ProductVersion + "\r\n" +
            "©2009-2026 Ali Adams - علي عبد الرزاق عبد الكريم القره غولي" + "\r\n" + "\r\n" +
            "http://qurancode.com" + "\r\n" +
            "God > ∞",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1
        );
    }
    private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        CloseApplication();

        Environment.Exit(0); // close Console and WinForms applications immediately without errors
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
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + ".\r\n\r\n" + ex.StackTrace, Application.ProductName);
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
