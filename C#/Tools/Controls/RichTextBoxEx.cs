//http://geekswithblogs.net/pvidler/archive/2003/10/14/182.aspx
// Modified by Ali Adams - heliwave@yahoo.com

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;

public class RichTextBoxEx : RichTextBox
{
    #region Win32 Messages

    private const int WM_NULL = 0x00;
    private const int WM_CREATE = 0x01;
    private const int WM_DESTROY = 0x02;
    private const int WM_MOVE = 0x03;
    private const int WM_SIZE = 0x05;
    private const int WM_ACTIVATE = 0x06;
    private const int WM_SETFOCUS = 0x07;
    private const int WM_KILLFOCUS = 0x08;
    private const int WM_ENABLE = 0x0A;
    private const int WM_SETREDRAW = 0x0B;
    private const int WM_SETTEXT = 0x0C;
    private const int WM_GETTEXT = 0x0D;
    private const int WM_GETTEXTLENGTH = 0x0E;
    private const int WM_PAINT = 0x0F;
    private const int WM_CLOSE = 0x10;
    private const int WM_QUERYENDSESSION = 0x11;
    private const int WM_QUIT = 0x12;
    private const int WM_QUERYOPEN = 0x13;
    private const int WM_ERASEBKGND = 0x14;
    private const int WM_SYSCOLORCHANGE = 0x15;
    private const int WM_ENDSESSION = 0x16;
    private const int WM_SYSTEMERROR = 0x17;
    private const int WM_SHOWWINDOW = 0x18;
    private const int WM_CTLCOLOR = 0x19;
    private const int WM_WININICHANGE = 0x1A;
    private const int WM_SETTINGCHANGE = 0x1A;
    private const int WM_DEVMODECHANGE = 0x1B;
    private const int WM_ACTIVATEAPP = 0x1C;
    private const int WM_FONTCHANGE = 0x1D;
    private const int WM_TIMECHANGE = 0x1E;
    private const int WM_CANCELMODE = 0x1F;
    private const int WM_SETCURSOR = 0x20;
    private const int WM_MOUSEACTIVATE = 0x21;
    private const int WM_CHILDACTIVATE = 0x22;
    private const int WM_QUEUESYNC = 0x23;
    private const int WM_GETMINMAXINFO = 0x24;
    private const int WM_PAINTICON = 0x26;
    private const int WM_ICONERASEBKGND = 0x27;
    private const int WM_NEXTDLGCTL = 0x28;
    private const int WM_SPOOLERSTATUS = 0x2A;
    private const int WM_DRAWITEM = 0x2B;
    private const int WM_MEASUREITEM = 0x2C;
    private const int WM_DELETEITEM = 0x2D;
    private const int WM_VKEYTOITEM = 0x2E;
    private const int WM_CHARTOITEM = 0x2F;

    private const int WM_SETFONT = 0x30;
    private const int WM_GETFONT = 0x31;
    private const int WM_SETHOTKEY = 0x32;
    private const int WM_GETHOTKEY = 0x33;
    private const int WM_QUERYDRAGICON = 0x37;
    private const int WM_COMPAREITEM = 0x39;
    private const int WM_COMPACTING = 0x41;
    private const int WM_WINDOWPOSCHANGING = 0x46;
    private const int WM_WINDOWPOSCHANGED = 0x47;
    private const int WM_POWER = 0x48;
    private const int WM_COPYDATA = 0x4A;
    private const int WM_CANCELJOURNAL = 0x4B;
    private const int WM_NOTIFY = 0x4E;
    private const int WM_INPUTLANGCHANGEREQUEST = 0x50;
    private const int WM_INPUTLANGCHANGE = 0x51;
    private const int WM_TCARD = 0x52;
    private const int WM_HELP = 0x53;
    private const int WM_USERCHANGED = 0x54;
    private const int WM_NOTIFYFORMAT = 0x55;
    private const int WM_CONTEXTMENU = 0x7B;
    private const int WM_STYLECHANGING = 0x7C;
    private const int WM_STYLECHANGED = 0x7D;
    private const int WM_DISPLAYCHANGE = 0x7E;
    private const int WM_GETICON = 0x7F;
    private const int WM_SETICON = 0x80;

    private const int WM_NCCREATE = 0x81;
    private const int WM_NCDESTROY = 0x82;
    private const int WM_NCCALCSIZE = 0x83;
    private const int WM_NCHITTEST = 0x84;
    private const int WM_NCPAINT = 0x85;
    private const int WM_NCACTIVATE = 0x86;
    private const int WM_GETDLGCODE = 0x87;
    private const int WM_NCMOUSEMOVE = 0xA0;
    private const int WM_NCLBUTTONDOWN = 0xA1;
    private const int WM_NCLBUTTONUP = 0xA2;
    private const int WM_NCLBUTTONDBLCLK = 0xA3;
    private const int WM_NCRBUTTONDOWN = 0xA4;
    private const int WM_NCRBUTTONUP = 0xA5;
    private const int WM_NCRBUTTONDBLCLK = 0xA6;
    private const int WM_NCMBUTTONDOWN = 0xA7;
    private const int WM_NCMBUTTONUP = 0xA8;
    private const int WM_NCMBUTTONDBLCLK = 0xA9;

    private const int WM_KEYFIRST = 0x100;
    private const int WM_KEYDOWN = 0x100;
    private const int WM_KEYUP = 0x101;
    private const int WM_CHAR = 0x102;
    private const int WM_DEADCHAR = 0x103;
    private const int WM_SYSKEYDOWN = 0x104;
    private const int WM_SYSKEYUP = 0x105;
    private const int WM_SYSCHAR = 0x106;
    private const int WM_SYSDEADCHAR = 0x107;
    private const int WM_KEYLAST = 0x108;

    private const int WM_IME_STARTCOMPOSITION = 0x10D;
    private const int WM_IME_ENDCOMPOSITION = 0x10E;
    private const int WM_IME_COMPOSITION = 0x10F;
    private const int WM_IME_KEYLAST = 0x10F;

    private const int WM_INITDIALOG = 0x110;
    private const int WM_COMMAND = 0x111;
    private const int WM_SYSCOMMAND = 0x112;
    private const int WM_TIMER = 0x113;
    private const int WM_SCROLL = 0x114;
    private const int WM_HSCROLL = 0x114;
    private const int WM_VSCROLL = 0x115;
    private const int WM_INITMENU = 0x116;
    private const int WM_INITMENUPOPUP = 0x117;
    private const int WM_MENUSELECT = 0x11F;
    private const int WM_MENUCHAR = 0x120;
    private const int WM_ENTERIDLE = 0x121;

    private const int WM_CTLCOLORMSGBOX = 0x132;
    private const int WM_CTLCOLOREDIT = 0x133;
    private const int WM_CTLCOLORLISTBOX = 0x134;
    private const int WM_CTLCOLORBTN = 0x135;
    private const int WM_CTLCOLORDLG = 0x136;
    private const int WM_CTLCOLORSCROLLBAR = 0x137;
    private const int WM_CTLCOLORSTATIC = 0x138;

    private const int WM_MOUSEFIRST = 0x200;
    private const int WM_MOUSEMOVE = 0x200;
    private const int WM_LBUTTONDOWN = 0x201;
    private const int WM_LBUTTONUP = 0x202;
    private const int WM_LBUTTONDBLCLK = 0x203;
    private const int WM_RBUTTONDOWN = 0x204;
    private const int WM_RBUTTONUP = 0x205;
    private const int WM_RBUTTONDBLCLK = 0x206;
    private const int WM_MBUTTONDOWN = 0x207;
    private const int WM_MBUTTONUP = 0x208;
    private const int WM_MBUTTONDBLCLK = 0x209;
    private const int WM_MOUSEWHEEL = 0x20A;
    private const int WM_MOUSEHWHEEL = 0x20E;

    private const int WM_PARENTNOTIFY = 0x210;
    private const int WM_ENTERMENULOOP = 0x211;
    private const int WM_EXITMENULOOP = 0x212;
    private const int WM_NEXTMENU = 0x213;
    private const int WM_SIZING = 0x214;
    private const int WM_CAPTURECHANGED = 0x215;
    private const int WM_MOVING = 0x216;
    private const int WM_POWERBROADCAST = 0x218;
    private const int WM_DEVICECHANGE = 0x219;

    private const int WM_MDICREATE = 0x220;
    private const int WM_MDIDESTROY = 0x221;
    private const int WM_MDIACTIVATE = 0x222;
    private const int WM_MDIRESTORE = 0x223;
    private const int WM_MDINEXT = 0x224;
    private const int WM_MDIMAXIMIZE = 0x225;
    private const int WM_MDITILE = 0x226;
    private const int WM_MDICASCADE = 0x227;
    private const int WM_MDIICONARRANGE = 0x228;
    private const int WM_MDIGETACTIVE = 0x229;
    private const int WM_MDISETMENU = 0x230;
    private const int WM_ENTERSIZEMOVE = 0x231;
    private const int WM_EXITSIZEMOVE = 0x232;
    private const int WM_DROPFILES = 0x233;
    private const int WM_MDIREFRESHMENU = 0x234;

    private const int WM_IME_SETCONTEXT = 0x281;
    private const int WM_IME_NOTIFY = 0x282;
    private const int WM_IME_CONTROL = 0x283;
    private const int WM_IME_COMPOSITIONFULL = 0x284;
    private const int WM_IME_SELECT = 0x285;
    private const int WM_IME_CHAR = 0x286;
    private const int WM_IME_KEYDOWN = 0x290;
    private const int WM_IME_KEYUP = 0x291;

    private const int WM_MOUSEHOVER = 0x2A1;
    private const int WM_NCMOUSELEAVE = 0x2A2;
    private const int WM_MOUSELEAVE = 0x2A3;

    private const int WM_CUT = 0x300;
    private const int WM_COPY = 0x301;
    private const int WM_PASTE = 0x302;
    private const int WM_CLEAR = 0x303;
    private const int WM_UNDO = 0x304;

    private const int WM_RENDERFORMAT = 0x305;
    private const int WM_RENDERALLFORMATS = 0x306;
    private const int WM_DESTROYCLIPBOARD = 0x307;
    private const int WM_DRAWCLIPBOARD = 0x308;
    private const int WM_PAINTCLIPBOARD = 0x309;
    private const int WM_VSCROLLCLIPBOARD = 0x30A;
    private const int WM_SIZECLIPBOARD = 0x30B;
    private const int WM_ASKCBFORMATNAME = 0x30C;
    private const int WM_CHANGECBCHAIN = 0x30D;
    private const int WM_HSCROLLCLIPBOARD = 0x30E;
    private const int WM_QUERYNEWPALETTE = 0x30F;
    private const int WM_PALETTEISCHANGING = 0x310;
    private const int WM_PALETTECHANGED = 0x311;

    private const int WM_HOTKEY = 0x312;
    private const int WM_PRINT = 0x317;
    private const int WM_PRINTCLIENT = 0x318;

    private const int WM_HANDHELDFIRST = 0x358;
    private const int WM_HANDHELDLAST = 0x35F;
    private const int WM_PENWINFIRST = 0x380;
    private const int WM_PENWINLAST = 0x38F;
    private const int WM_COALESCE_FIRST = 0x390;
    private const int WM_COALESCE_LAST = 0x39F;
    private const int WM_DDE_FIRST = 0x3E0;
    private const int WM_DDE_INITIATE = 0x3E0;
    private const int WM_DDE_TERMINATE = 0x3E1;
    private const int WM_DDE_ADVISE = 0x3E2;
    private const int WM_DDE_UNADVISE = 0x3E3;
    private const int WM_DDE_ACK = 0x3E4;
    private const int WM_DDE_DATA = 0x3E5;
    private const int WM_DDE_REQUEST = 0x3E6;
    private const int WM_DDE_POKE = 0x3E7;
    private const int WM_DDE_EXECUTE = 0x3E8;
    private const int WM_DDE_LAST = 0x3E8;

    private const int WM_USER = 0x400;
    private const int WM_APP = 0x8000;

    // Event Messages
    private const int EM_GETEVENTMASK = (WM_USER + 59);
    private const int EM_SETEVENTMASK = (WM_USER + 69);
    private const int EM_GETPARAFORMAT = (WM_USER + 61);
    private const int EM_SETPARAFORMAT = (WM_USER + 71);
    private const int EM_SETTYPOGRAPHYOPTIONS = (WM_USER + 202);

    // WM_HSCROLL
    private const int SB_LINELEFT = 0;      // Scrolls left by one unit.
    private const int SB_LINERIGHT = 1;     // Scrolls right by one unit.
    private const int SB_PAGELEFT = 2;      // Scrolls left by the width of the window.
    private const int SB_PAGERIGHT = 3;     // Scrolls right by the width of the window.
    private const int SB_LEFT = 6;          // Scrolls to upper left.
    private const int SB_RIGHT = 7;         // Scrolls to lower right.

    // WM_VSCROLL
    private const int SB_LINEUP = 0;        // Scrolls one line up.
    private const int SB_LINEDOWN = 1;      // Scrolls one line down.
    private const int SB_PAGEUP = 2;        // Scrolls one page up.
    private const int SB_PAGEDOWN = 3;      // Scrolls one page down.
    private const int SB_TOP = 6;           // Scrolls to the upper left.
    private const int SB_BOTTOM = 7;        // Scrolls to the lower right.

    // WM_HSCROLL and WM_VSCROLL
    private const int SB_THUMBPOSITION = 4; // The user has dragged the scroll box (thumb) and released the mouse button. The HIWORD indicates the position of the scroll box at the end of the drag operation.
    private const int SB_THUMBTRACK = 5;    // The user is dragging the scroll box. This message is sent repeatedly until the user releases the mouse button. The HIWORD indicates the position that the scroll box has been dragged to.
    private const int SB_ENDSCROLL = 8;     // Ends scroll

    #endregion

    /// <summary>
    /// Specifies where to send the caret in a <see cref="RichTextBox"/>.
    /// </summary>
    public enum ScrollMode
    {
        /// <summary>
        /// Scrolls left by one unit.
        /// </summary>
        LineLeft = SB_LINELEFT,

        /// <summary>
        /// Scrolls right by one unit.
        /// </summary>
        LineRight = SB_LINERIGHT,

        /// <summary>
        /// Scrolls left by the width of the window.
        /// </summary>
        PageLeft = SB_PAGELEFT,

        /// <summary>
        /// Scrolls right by the width of the window.
        /// </summary>
        PageRight = SB_PAGERIGHT,

        /// <summary>
        /// The user has dragged the scroll box (thumb) and released the mouse button. The HIWORD indicates the position of the scroll box at the end of the drag operation.
        /// </summary>
        ThumbPosition = SB_THUMBPOSITION,

        /// <summary>
        /// The user is dragging the scroll box. This message is sent repeatedly until the user releases the mouse button. The HIWORD indicates the position that the scroll box has been dragged to.
        /// </summary>
        ThumbTrack = SB_THUMBTRACK,

        /// <summary>
        /// Scrolls to upper left.
        /// </summary>
        UpperLeft = SB_LEFT,

        /// <summary>
        /// Scrolls to lower right.
        /// </summary>
        LowerRight = SB_RIGHT,

        /// <summary>
        /// Ends scroll
        /// </summary>
        EndScroll = SB_ENDSCROLL,

        /// <summary>
        /// Scrolls one line up.
        /// </summary>
        LineUp = WM_USER + SB_LINEUP,

        /// <summary>
        /// Scrolls one line down.
        /// </summary>
        LineDown = WM_USER + SB_LINEDOWN,

        /// <summary>
        /// Scrolls one page up.
        /// </summary>
        PageUp = WM_USER + SB_PAGEUP,

        /// <summary>
        /// Scrolls one page down.
        /// </summary>
        PageDown = WM_USER + SB_PAGEDOWN,

        /// <summary>
        /// Scrolls to the upper left.
        /// </summary>
        Top = WM_USER + SB_TOP,

        /// <summary>
        /// Scrolls to the lower right.
        /// </summary>
        Bottom = WM_USER + SB_BOTTOM
    }

    /// <summary>
    /// Specifies how text in a <see cref="RichTextBox"/> is horizontally aligned.
    /// </summary>
    public enum TextAlign
    {
        /// <summary>
        /// The text is aligned to the left.
        /// </summary>
        Left = 1,

        /// <summary>
        /// The text is aligned to the right.
        /// </summary>
        Right = 2,

        /// <summary>
        /// The text is aligned in the center.
        /// </summary>
        Center = 3,

        /// <summary>
        /// The text is justified.
        /// </summary>
        Justify = 4
    }

    [DllImport("user32", CharSet = CharSet.Unicode)]
    private extern static IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

    private IntPtr m_old_event_mask = IntPtr.Zero;
    private int m_updating = 0;
    /// <summary>
    /// Maintains performance while updating.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is recommended to call this method before doing
    /// any major updates that you do not wish the user to
    /// see. Remember to call EndUpdate when you are finished
    /// with the update. Nested calls are supported.
    /// </para>
    /// <para>
    /// Calling this method will prevent redrawing. It will
    /// also setup the event mask of the underlying richedit
    /// control so that no events are sent.
    /// </para>
    /// </remarks>
    public void BeginUpdate()
    {
        // Deal with nested calls.
        ++m_updating;

        if (m_updating > 1)
            return;

        // Prevent the control from raising any events.
        m_old_event_mask = SendMessage(this.Handle, EM_SETEVENTMASK, (IntPtr)0, IntPtr.Zero);

        // Prevent the control from redrawing itself.
        SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
    }
    /// <summary>
    /// Resumes drawing and event handling.
    /// </summary>
    /// <remarks>
    /// This method should be called every time a call is made
    /// made to BeginUpdate. It resets the event mask to it's
    /// original value and enables redrawing of the control.
    /// </remarks>
    public void EndUpdate()
    {
        // Deal with nested calls.
        --m_updating;

        if (m_updating > 0)
            return;

        // Allow the control to redraw itself.
        SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);

        // Allow the control to raise event messages.
        SendMessage(this.Handle, EM_SETEVENTMASK, (IntPtr)0, m_old_event_mask);

        this.Invalidate(true);
    }

    public void ScrollTo(ScrollMode mode)
    {
        if (mode <= ScrollMode.EndScroll)
        {
            SendMessage(this.Handle, WM_HSCROLL, (IntPtr)mode, IntPtr.Zero);
        }
        else
        {
            mode -= WM_USER;
            SendMessage(this.Handle, WM_VSCROLL, (IntPtr)mode, IntPtr.Zero);
        }
    }
    public void ScrollTo(int start)
    {
        if ((start >= 0) && (start < this.Text.Length))
        {
            this.Select(start, 1);
            this.ScrollToCaret();
            this.DeselectAll();
        }
    }
    public void AlignToStart()
    {
        if (this.Text.Length > 0)
        {
            this.ScrollToCaret(); // must be first
            this.Select(0, 0);    // must be second
        }
    }
    public void AlignSelection()
    {
        if (this.Text.Length > 0)
        {
            // backup selection length
            int length = this.SelectionLength;
            int start = this.SelectionStart;

            // move caret to start of selection
            this.SelectionStart = start;
            this.SelectionLength = 0;
            this.ScrollToCaret();

            // re-select
            this.Select(start, length);
        }
    }
    public void AlignToLineStart()
    {
        // WARNING
        // NOT WORKING :(
        SetHorizontalScrollBarPosition(0);

        // use alight to top-left instead (until we fix it)
        AlignToStart();
    }
    #region RichScrollBars


    [DllImport("user32.dll")]
    private static extern bool GetScrollInfo(IntPtr hWnd, int nBar, ref SCROLLINFO si);
    [DllImport("user32.dll")]
    private static extern int GetScrollPos(IntPtr hWnd, int nBar);
    [DllImport("user32.dll")]
    private static extern int SetScrollInfo(IntPtr hWnd, int nBar, SCROLLINFO si, bool bRedraw);
    [DllImport("user32.dll")]
    private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

    [StructLayout(LayoutKind.Sequential)]
    public class SCROLLINFO
    {
        public int cbSize;
        public int fMask;
        public int nMin;
        public int nMax;
        public int nPage;
        public int nPos;
        public int nTrackPos;
        public SCROLLINFO()
        {
            this.cbSize = Marshal.SizeOf(typeof(SCROLLINFO));
        }

        public SCROLLINFO(int mask, int min, int max, int page, int pos)
        {
            this.cbSize = Marshal.SizeOf(typeof(SCROLLINFO));
            this.fMask = mask;
            this.nMin = min;
            this.nMax = max;
            this.nPage = page;
            this.nPos = pos;
        }
    }

    private const int SIF_ALL = 0X17;
    private const int SB_HORZ = 0;
    private const int SB_VERT = 1;

    public int GetHorizontalScrollBarWidth()
    {
        SCROLLINFO si = new SCROLLINFO() { fMask = SIF_ALL };
        GetScrollInfo(this.Handle, SB_HORZ, ref si);
        return Math.Max(si.nMax, this.Width);
    }
    public void SetHorizontalScrollBarWidth(int value)
    {
        SCROLLINFO si = new SCROLLINFO() { fMask = SIF_ALL };
        si.nMax = value;
        SetScrollInfo(this.Handle, SB_HORZ, si, true);
    }
    public int GetHorizontalScrollBarPosition(int value)
    {
        return GetScrollPos(this.Handle, SB_HORZ);
    }
    public void SetHorizontalScrollBarPosition(int value)
    {
        SetScrollPos(this.Handle, SB_HORZ, value, true);
    }

    public int GetVerticalScrollBarHeight()
    {
        SCROLLINFO si = new SCROLLINFO() { fMask = SIF_ALL };
        GetScrollInfo(this.Handle, SB_VERT, ref si);
        return Math.Max(si.nMax, this.Height);
    }
    public void SetVerticalScrollBarHeight(int value)
    {
        SCROLLINFO si = new SCROLLINFO() { fMask = SIF_ALL };
        si.nMax = value;
        SetScrollInfo(this.Handle, SB_VERT, si, true);
    }
    public int GetVerticalScrollBarPosition(int value)
    {
        return GetScrollPos(this.Handle, SB_VERT);
    }
    public void SetVerticalScrollBarPosition(int value)
    {
        SetScrollPos(this.Handle, SB_VERT, value, true);
    }

    #endregion

    private LastHighlight mLastHighlight = new LastHighlight(0, Rectangle.Empty);
    private class LastHighlight
    {
        public int mCharIndex;
        public Rectangle mRectangle;

        public LastHighlight(int index, Rectangle rect)
        {
            mCharIndex = index;
            mRectangle = rect;
        }
    }
    private void PaintLineHighlight()
    {
        using (Graphics g = this.CreateGraphics())
        {
            Color color = Color.Lavender;
            Pen highlight_pen = new Pen(Color.Lavender);
            Pen clear_pen = new Pen(this.BackColor);
            SolidBrush highlight_brush = new SolidBrush(Color.FromArgb(64, color.R, color.G, color.B));
            SolidBrush clear_brush = new SolidBrush(this.BackColor);

            // index of the current line
            int index = this.GetFirstCharIndexOfCurrentLine();

            // rectangle to specify which region to paint too
            Rectangle rect = new Rectangle();

            // specify dimensions
            rect.X = 0;
            rect.Y = this.GetPositionFromCharIndex(index).Y;
            rect.Width = this.GetHorizontalScrollBarWidth();
            rect.Height = Convert.ToInt32(this.Font.Height * this.ZoomFactor);

            // this will always be true unless the current line remains the same
            if (!(mLastHighlight.mCharIndex == index) && !(mLastHighlight.mRectangle == rect))
            {
                // remove the last highlight. regardless of the brush specified, white is always applied, and the text is painted over
                g.DrawRectangle(clear_pen, mLastHighlight.mRectangle);
                g.FillRectangle(clear_brush, mLastHighlight.mRectangle);
                // apply highlight to the current line
                g.DrawRectangle(highlight_pen, rect);
                g.FillRectangle(highlight_brush, rect);
            }

            mLastHighlight = new LastHighlight(index, rect);
        }
    }

    public int GetLinePosition(int line_index)
    {
        int result = 0;
        if ((line_index >= 0) && (line_index < this.Lines.Length))
        {
            for (int i = 0; i < line_index; i++)
            {
                result += this.Lines[i].Length;
            }
        }
        return result;
    }

    /// <summary>
    /// Efficient text coloring using RTF language directly
    /// </summary>
    /// <param name="text">the text to find and colorize</param>
    public void Colorize(string text, Color color)
    {
        //Get RTF from RichTextBox
        string rtf = this.Rtf;

        // Search for colour table info. If it exists (it shouldn't,
        // but we'll check anyway) remove it and replace with our one
        int colortbl_index = rtf.IndexOf("colortbl");

        if (colortbl_index != -1) //then colortbl exists
        {
            //find end of colortbl tab by searching
            //forward from the colortbl tab itself
            int insert_index = rtf.IndexOf('}', colortbl_index);

            //remove the existing colour table
            rtf = rtf.Remove(colortbl_index, insert_index - colortbl_index + 1);

            //now insert new colour table at index of old colortbl tag
            rtf = rtf.Insert(colortbl_index, "colortbl;\\red255\\green0\\blue0;\\red0\\green128\\blue0;red0\\green0\\blue128;}");
        }
        else  //colour table doesn't exist yet, so let's make one
        {
            // find index of start of header
            int rtf_index = rtf.IndexOf("\\rtf");

            // get index of where we'll insert the colour table
            // try finding opening bracket of first property of header first                
            int insert_index = rtf.IndexOf('{', rtf_index);

            // if there is no property, we'll insert colour table
            // just before the end bracket of the header
            if (insert_index == -1) insert_index = rtf.IndexOf('}', rtf_index) - 1;

            // insert the colour table at our chosen location                
            rtf = rtf.Insert(insert_index, "{\\colortbl;\\red255\\green0\\blue0;\\red0\\green128\\blue0;red0\\green0\\blue128;}");
        }

        /*
         * In our colour table we defined:
         * cf0 = black, default, immutable
         * cf1 = red  
         * cf2 = green
         * cf3 = navy             
         * */
        rtf = rtf.Replace(@"cf1", "cf3"); // restore main color

        string rtf_match_text = text.ToRtfChars();
        rtf = rtf.Replace(rtf_match_text, @"\cf1" + rtf_match_text + @" \cf3");
        this.Rtf = rtf;

        //StringBuilder str = new StringBuilder(rtf);
        //for (int i = 0; i < str.Length; i++)
        //{
        //    //check for correct character
        //    if (str[i] == 'ع')
        //    {
        //        //add RTF tags after symbol 
        //        str.Insert(i + 1, "\\cf1 ");
        //        //add RTF before symbol
        //        str.Insert(i, "\\cf3 ");
        //        //skip forward past the characters we've just added
        //        //to avoid getting trapped in the loop
        //        i += 6;
        //    }

        //    // chemical format demo, atoms have subscripted counts like 2 in H2O
        //    if (char.IsDigit(str[i]))
        //    {
        //        str.Insert(i, @"\dn7 ");
        //        str.Insert(i + 1 + 5, @"\up0 ");
        //        i += 10;
        //    }

        //    this.Rtf = str.ToString();
        //}
    }

    public Color LineColor
    {
        set
        {
            int start = this.GetFirstCharIndexOfCurrentLine();
            int line_index = this.GetLineFromCharIndex(start);
            int length = this.Lines[line_index].Length;
            this.Select(start, length);
            this.SelectionColor = value;
        }
    }
    public Color LineHighlight
    {
        set
        {
            int start = this.GetFirstCharIndexOfCurrentLine();
            int line_index = this.GetLineFromCharIndex(start);
            int length = this.Lines[line_index].Length;
            this.Select(start, length);
            this.SelectionBackColor = value;
        }
    }
    public void ColorizeLine(int line_index, Color color)
    {
        if (this.Lines.Length > line_index)
        {
            int start = this.GetFirstCharIndexFromLine(line_index);
            int length = this.Lines[line_index].Length;
            this.Select(start, length);
            this.SelectionColor = color;
        }
    }
    public void HighlightLine(int line_index, Color color)
    {
        if (this.Lines.Length > line_index)
        {
            int start = this.GetFirstCharIndexFromLine(line_index);
            int length = this.Lines[line_index].Length;
            this.Select(start, length);
            this.SelectionBackColor = color;
        }
    }
    public void ClearColor(int line_index)
    {
        if (this.Lines.Length > line_index)
        {
            int start = this.GetFirstCharIndexFromLine(line_index);
            int length = this.Lines[line_index].Length;
            this.ClearColor(start, length);
        }
    }
    public void ClearHighlight(int line_index)
    {
        if (this.Lines.Length > line_index)
        {
            int start = this.GetFirstCharIndexFromLine(line_index);
            int length = this.Lines[line_index].Length;
            this.ClearHighlight(start, length);
        }
    }
    // Duplicate Properties 
    //public Color SelectionColor
    //{
    //    get
    //    {
    //        return this.SelectionColor;
    //    }
    //    set
    //    {
    //        if (this.SelectedText.Length > 0)
    //        {
    //            this.SelectionColor = value;
    //        }
    //    }
    //}
    //public Color SelectionHighlight
    //{
    //    get
    //    {
    //        return this.SelectionBackColor;
    //    }
    //    set
    //    {
    //        if (this.SelectedText.Length > 0)
    //        {
    //            this.SelectionBackColor = value;
    //        }
    //    }
    //}
    public void Colorize(Color color)
    {
        this.Select(0, this.Text.Length);
        this.SelectionColor = color;
    }
    public void Colorize(int start, int length, Color color)
    {
        if ((start >= 0) && (start < this.Text.Length))
        {
            this.Select(start, length);
            this.SelectionColor = color;
        }
    }
    public void Highlight(int start, int length, Color color)
    {
        if ((start >= 0) && (start < this.Text.Length))
        {
            this.Select(start, length);
            this.SelectionBackColor = color;
        }
    }
    public void ClearColor(int start, int length)
    {
        if ((start >= 0) && (start < this.Text.Length))
        {
            this.Select(start, length);
            this.SelectionColor = this.ForeColor;
        }
    }
    public void ClearHighlight(int start, int length)
    {
        if ((start >= 0) && (start < this.Text.Length))
        {
            this.Select(start, length);
            this.SelectionBackColor = this.BackColor;
        }
    }
    public void ClearColor()
    {
        this.SelectAll();
        this.SelectionColor = this.ForeColor;
    }
    public void ClearHighlight()
    {
        this.SelectAll();
        this.SelectionBackColor = this.BackColor;
    }

    private const int EM_SETCHARFORMAT = 1092;
    private const int CFM_BOLD = 1;
    private const int CFM_ITALIC = 2;
    private const int CFM_UNDERLINE = 4;
    private const int SCF_SELECTION = 1;
    [StructLayout(LayoutKind.Sequential)]
    private struct CHARFORMAT
    {
        public int cbSize;
        public uint dwMask;
        public uint dwEffects;
        public int yHeight;
        public int yOffset;
        public int crTextColor;
        public byte bCharSet;
        public byte bPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szFaceName;

        // CHARFORMAT2 from here onwards.
        public short wWeight;
        public short sSpacing;
        public int crBackColor;
        public int LCID;
        public uint dwReserved;
        public short sStyle;
        public short wKerning;
        public byte bUnderlineType;
        public byte bAnimation;
        public byte bRevAuthor;
    }
    [DllImport("user32", CharSet = CharSet.Unicode)]
    private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, ref CHARFORMAT lp);
    private void SetCharFormatMessage(ref CHARFORMAT char_format)
    {
        SendMessage(new HandleRef(this, Handle), EM_SETCHARFORMAT, SCF_SELECTION, ref char_format);
    }
    private void ApplyStyle(uint style, bool value)
    {

        CHARFORMAT fmt = new CHARFORMAT();
        fmt.cbSize = Marshal.SizeOf(fmt);
        fmt.dwMask = style;

        if (value)
            fmt.dwEffects = style;
        SetCharFormatMessage(ref fmt);
    }
    public bool SelectionBold
    {
        set { ApplyStyle(CFM_BOLD, value); }
    }
    public bool SelectionItalic
    {
        set { ApplyStyle(CFM_ITALIC, value); }
    }
    public bool SelectionUnderline
    {
        set { ApplyStyle(CFM_UNDERLINE, value); }
    }

    internal class CursorPosition
    {
        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern int GetCaretPos(ref Point lpPoint);
        public static int Line(RichTextBox e, int index)
        {
            return e.GetLineFromCharIndex(index) + 1;
        }
        public static int Column(RichTextBox e, int index)
        {
            int correction = e.GetFirstCharIndexOfCurrentLine();
            Point point = e.GetPositionFromCharIndex(index);
            return e.GetCharIndexFromPosition(point) - correction;
        }
    }
    public int ColumnNumber
    {
        get { return CursorPosition.Column(this, SelectionStart); }
    }
    public int LineNumber
    {
        get { return CursorPosition.Line(this, SelectionStart); }
    }
    public int Position
    {
        get { return this.SelectionStart; }
        //set
        //{
        //    this.SelectionStart = value;
        //    this.SelectionLength = 0;
        //    this.ScrollToCaret();
        //}
    }
    public int SelectionEnd
    {
        get { return SelectionStart + SelectionLength; }
    }

    //// override SelectionChanged event
    //public event EventHandler CursorPositionChanged;
    //protected virtual void OnCursorPositionChanged(EventArgs e)
    //{
    //    if (CursorPositionChanged != null)
    //        CursorPositionChanged(this, e);
    //}
    //protected override void OnSelectionChanged(EventArgs e)
    //{
    //    if (SelectionLength == 0)
    //        OnCursorPositionChanged(e);
    //    else
    //        base.OnSelectionChanged(e);
    //}

    public int VisibleLines
    {
        get
        {
            //Get the height of the text area.
            int height = TextRenderer.MeasureText(this.Text, this.Font).Height;

            //rate = visible height / Total height.
            float rate = (1.0f * this.Height) / height;

            //Get visible lines.
            return (int)(this.Lines.Length * rate);
        }
    }

    /// <summary>
    /// Gets or sets the alignment to apply to the current
    /// selection or insertion point.
    /// </summary>
    /// <remarks>
    /// Replaces the SelectionAlignment from
    /// <see cref="RichTextBox"/>.
    /// </remarks>
    public new TextAlign SelectionAlignment
    {
        get
        {
            PARAFORMAT fmt = new PARAFORMAT();
            fmt.cbSize = Marshal.SizeOf(fmt);

            // Get the alignment.
            SendMessage(new HandleRef(this, Handle),
                         EM_GETPARAFORMAT,
                         SCF_SELECTION, ref fmt);

            // Default to Left align.
            if ((fmt.dwMask & PFM_ALIGNMENT) == 0)
                return TextAlign.Left;

            return (TextAlign)fmt.wAlignment;
        }

        set
        {
            PARAFORMAT fmt = new PARAFORMAT();
            fmt.cbSize = Marshal.SizeOf(fmt);
            fmt.dwMask = PFM_ALIGNMENT;
            fmt.wAlignment = (short)value;

            // Set the alignment.
            SendMessage(new HandleRef(this, Handle),
                         EM_SETPARAFORMAT,
                         SCF_SELECTION, ref fmt);
        }
    }

    /// <summary>
    /// This member overrides
    /// <see cref="Control"/>.OnHandleCreated.
    /// </summary>
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // Enable support for justification.
        SendMessage(new HandleRef(this, Handle),
                     EM_SETTYPOGRAPHYOPTIONS,
                     TO_ADVANCEDTYPOGRAPHY,
                     TO_ADVANCEDTYPOGRAPHY);
    }

    // Constants from the Platform SDK.
    private const int TO_ADVANCEDTYPOGRAPHY = 1;
    private const int PFM_ALIGNMENT = 8;

    // It makes no difference if we use PARAFORMAT or PARAFORMAT2.
    [StructLayout(LayoutKind.Sequential)]
    private struct PARAFORMAT
    {
        public int cbSize;
        public uint dwMask;
        public short wNumbering;
        public short wReserved;
        public int dxStartIndent;
        public int dxRightIndent;
        public int dxOffset;
        public short wAlignment;
        public short cTabCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public int[] rgxTabs;

        // PARAFORMAT2 from here onwards.
        public int dySpaceBefore;
        public int dySpaceAfter;
        public int dyLineSpacing;
        public short sStyle;
        public byte bLineSpacingRule;
        public byte bOutlineLevel;
        public short wShadingWeight;
        public short wShadingStyle;
        public short wNumberingStart;
        public short wNumberingStyle;
        public short wNumberingTab;
        public short wBorderSpace;
        public short wBorderWidth;
        public short wBorders;
    }

    [DllImport("user32", CharSet = CharSet.Unicode)]
    private static extern int SendMessage(HandleRef hWnd,
                                           int msg,
                                           int wParam,
                                           int lParam);

    [DllImport("user32", CharSet = CharSet.Unicode)]
    private static extern int SendMessage(HandleRef hWnd,
                                           int msg,
                                           int wParam,
                                           ref PARAFORMAT lp);

    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryW(string s_File);

    public static IntPtr LoadLibrary(string s_File)
    {
        IntPtr h_Module = LoadLibraryW(s_File);
        if (h_Module != IntPtr.Zero)
            return h_Module;

        int s32_Error = Marshal.GetLastWin32Error();
        throw new Win32Exception(s32_Error);
    }

    //http://stackoverflow.com/questions/18668920/c-sharp-richeditbox-has-extremely-slow-performance-4-minutes-loading-solve
    /// <summary>
    /// The framework uses by default "Richedit20W" in RICHED20.DLL.
    /// This needs 4 minutes to load a 2,5MB RTF file with 45000 lines.
    /// Richedit50W needs only 2 seconds for the same RTF document !!!
    /// </summary>
    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams i_Params = base.CreateParams;
            try
            {
                // Available since XP SP1
                LoadLibrary("MsftEdit.dll"); // throws

                // Replace "RichEdit20W" with "RichEdit50W"
                i_Params.ClassName = "RichEdit50W";
            }
            catch
            {
                // Windows XP without any Service Pack.
            }
            return i_Params;
        }
    }
}

public static class StringExtensions
{
    //http://stackoverflow.com/questions/1368020/how-to-output-unicode-string-to-rtf-using-c
    public static string ToUnicodeEscapedChars(this string source)
    {
        if (String.IsNullOrEmpty(source)) return null;

        StringBuilder str = new StringBuilder();
        foreach (char c in source)
        {
            if (c == '\\' || c == '{' || c == '}')
                str.Append(@"\" + c);
            else if (c <= 0x7f)
                str.Append(c);
            else
                str.Append("\\u" + Convert.ToUInt32(c) + "?");
        }
        return str.ToString();
    }
    public static string ToRtfChars(this string source)
    {
        if (String.IsNullOrEmpty(source)) return null;

        #region Windows-1256 Code Page
        //    Dictionary<char, string> dictionary = new Dictionary<char, string>()
        //    {
        //        {' ', @"\'20" },
        //        {'!', @"\'21" },
        //        {'"', @"\'22" },
        //        {'#', @"\'23" },
        //        {'$', @"\'24" },
        //        {'%', @"\'25" },
        //        {'&', @"\'26" },
        //        {'\'', @"\'27" },
        //        {'(', @"\'28" },
        //        {')', @"\'29" },
        //        {'*', @"\'2A" },
        //        {'+', @"\'2B" },
        //        {',', @"\'2C" },
        //        {'-', @"\'2D" },
        //        {'.', @"\'2E" },
        //        {'/', @"\'2F" },
        //        {'0', @"\'30" },
        //        {'1', @"\'31" },
        //        {'2', @"\'32" },
        //        {'3', @"\'33" },
        //        {'4', @"\'34" },
        //        {'5', @"\'35" },
        //        {'6', @"\'36" },
        //        {'7', @"\'37" },
        //        {'8', @"\'38" },
        //        {'9', @"\'39" },
        //        {':', @"\'3A" },
        //        {';', @"\'3B" },
        //        {'<', @"\'3C" },
        //        {'=', @"\'3D" },
        //        {'>', @"\'3E" },
        //        {'?', @"\'3F" },
        //        {'@', @"\'40" },
        //        {'A', @"\'41" },
        //        {'B', @"\'42" },
        //        {'C', @"\'43" },
        //        {'D', @"\'44" },
        //        {'E', @"\'45" },
        //        {'F', @"\'46" },
        //        {'G', @"\'47" },
        //        {'H', @"\'48" },
        //        {'I', @"\'49" },
        //        {'J', @"\'4A" },
        //        {'K', @"\'4B" },
        //        {'L', @"\'4C" },
        //        {'M', @"\'4D" },
        //        {'N', @"\'4E" },
        //        {'O', @"\'4F" },
        //        {'P', @"\'50" },
        //        {'Q', @"\'51" },
        //        {'R', @"\'52" },
        //        {'S', @"\'53" },
        //        {'T', @"\'54" },
        //        {'U', @"\'55" },
        //        {'V', @"\'56" },
        //        {'W', @"\'57" },
        //        {'X', @"\'58" },
        //        {'Y', @"\'59" },
        //        {'Z', @"\'5A" },
        //        {'[', @"\'5B" },
        //        {'\\', @"\'5C" },
        //        {']', @"\'5D" },
        //        {'^', @"\'5E" },
        //        {'_', @"\'5F" },
        //        {'`', @"\'60" },
        //        {'a', @"\'61" },
        //        {'b', @"\'62" },
        //        {'c', @"\'63" },
        //        {'d', @"\'64" },
        //        {'e', @"\'65" },
        //        {'f', @"\'66" },
        //        {'g', @"\'67" },
        //        {'h', @"\'68" },
        //        {'i', @"\'69" },
        //        {'j', @"\'6A" },
        //        {'k', @"\'6B" },
        //        {'l', @"\'6C" },
        //        {'m', @"\'6D" },
        //        {'n', @"\'6E" },
        //        {'o', @"\'6F" },
        //        {'p', @"\'70" },
        //        {'q', @"\'71" },
        //        {'r', @"\'72" },
        //        {'s', @"\'73" },
        //        {'t', @"\'74" },
        //        {'u', @"\'75" },
        //        {'v', @"\'76" },
        //        {'w', @"\'77" },
        //        {'x', @"\'78" },
        //        {'y', @"\'79" },
        //        {'z', @"\'7A" },
        //        {'{', @"\'7B" },
        //        {'|', @"\'7C" },
        //        {'}', @"\'7D" },
        //        {'~', @"\'7E" },
        //        {'€', @"\'80" },
        //        {'پ', @"\'81" },
        //        {'‚', @"\'82" },
        //        {'ƒ', @"\'83" },
        //        {'„', @"\'84" },
        //        {'…', @"\'85" },
        //        {'†', @"\'86" },
        //        {'‡', @"\'87" },
        //        {'ˆ', @"\'88" },
        //        {'‰', @"\'89" },
        //        {'ٹ', @"\'8A" },
        //        {'‹', @"\'8B" },
        //        {'Œ', @"\'8C" },
        //        {'چ', @"\'8D" },
        //        {'ژ', @"\'8E" },
        //        {'ڈ', @"\'8F" },
        //        {'گ', @"\'90" },
        //        {'‘', @"\'91" },
        //        {'’', @"\'92" },
        //        {'“', @"\'93" },
        //        {'”', @"\'94" },
        //        {'•', @"\'95" },
        //        {'–', @"\'96" },
        //        {'—', @"\'97" },
        //        {'ک', @"\'98" },
        //        {'™', @"\'99" },
        //        {'ڑ', @"\'9A" },
        //        {'›', @"\'9B" },
        //        {'œ', @"\'9C" },
        //        {'ں', @"\'9F" },
        //        {'،', @"\'A1" },
        //        {'¢', @"\'A2" },
        //        {'£', @"\'A3" },
        //        {'¤', @"\'A4" },
        //        {'¥', @"\'A5" },
        //        {'¦', @"\'A6" },
        //        {'§', @"\'A7" },
        //        {'¨', @"\'A8" },
        //        {'©', @"\'A9" },
        //        {'ھ', @"\'AA" },
        //        {'«', @"\'AB" },
        //        {'¬', @"\'AC" },
        //        {'®', @"\'AE" },
        //        {'¯', @"\'AF" },
        //        {'°', @"\'B0" },
        //        {'±', @"\'B1" },
        //        {'²', @"\'B2" },
        //        {'³', @"\'B3" },
        //        {'´', @"\'B4" },
        //        {'µ', @"\'B5" },
        //        {'¶', @"\'B6" },
        //        {'•', @"\'B7" },
        //        {'¸', @"\'B8" },
        //        {'¹', @"\'B9" },
        //        {'؛', @"\'BA" },
        //        {'»', @"\'BB" },
        //        {'¼', @"\'BC" },
        //        {'½', @"\'BD" },
        //        {'¾', @"\'BE" },
        //        {'؟', @"\'BF" },
        //        {'ہ', @"\'C0" },
        //        {'ء', @"\'C1" },
        //        {'آ', @"\'C2" },
        //        {'أ', @"\'C3" },
        //        {'ؤ', @"\'C4" },
        //        {'إ', @"\'C5" },
        //        {'ئ', @"\'C6" },
        //        {'ا', @"\'C7" },
        //        {'ب', @"\'C8" },
        //        {'ة', @"\'C9" },
        //        {'ت', @"\'CA" },
        //        {'ث', @"\'CB" },
        //        {'ج', @"\'CC" },
        //        {'ح', @"\'CD" },
        //        {'خ', @"\'CE" },
        //        {'د', @"\'CF" },
        //        {'ذ', @"\'D0" },
        //        {'ر', @"\'D1" },
        //        {'ز', @"\'D2" },
        //        {'س', @"\'D3" },
        //        {'ش', @"\'D4" },
        //        {'ص', @"\'D5" },
        //        {'ض', @"\'D6" },
        //        {'×', @"\'D7" },
        //        {'ط', @"\'D8" },
        //        {'ظ', @"\'D9" },
        //        {'ع', @"\'DA" },
        //        {'غ', @"\'DB" },
        //        {'ـ', @"\'DC" },
        //        {'ف', @"\'DD" },
        //        {'ق', @"\'DE" },
        //        {'ك', @"\'DF" },
        //        {'à', @"\'E0" },
        //        {'ل', @"\'E1" },
        //        {'â', @"\'E2" },
        //        {'م', @"\'E3" },
        //        {'ن', @"\'E4" },
        //        {'ه', @"\'E5" },
        //        {'و', @"\'E6" },
        //        {'ç', @"\'E7" },
        //        {'è', @"\'E8" },
        //        {'é', @"\'E9" },
        //        {'ê', @"\'EA" },
        //        {'ë', @"\'EB" },
        //        {'ى', @"\'EC" },
        //        {'ي', @"\'ED" },
        //        {'î', @"\'EE" },
        //        {'ï', @"\'EF" },
        //        {'ً', @"\'F0" },
        //        {'ٌ', @"\'F1" },
        //        {'ٍ', @"\'F2" },
        //        {'َ', @"\'F3" },
        //        {'ô', @"\'F4" },
        //        {'ُ', @"\'F5" },
        //        {'ِ', @"\'F6" },
        //        {'÷', @"\'F7" },
        //        {'ّ', @"\'F8" },
        //        {'ù', @"\'F9" },
        //        {'ْ', @"\'FA" },
        //        {'û', @"\'FB" },
        //        {'ü', @"\'FC" },
        //        {'ے', @"\'FF" }
        //    };
        #endregion

        Dictionary<char, string> dictionary = new Dictionary<char, string>()
        {
            {'0', @"\'30" },
            {'1', @"\'31" },
            {'2', @"\'32" },
            {'3', @"\'33" },
            {'4', @"\'34" },
            {'5', @"\'35" },
            {'6', @"\'36" },
            {'7', @"\'37" },
            {'8', @"\'38" },
            {'9', @"\'39" },
            {'ھ', @"\'AA" },
            {'°', @"\'B0" },
            {'•', @"\'B7" },
            {'ہ', @"\'C0" },
            {'ء', @"\'C1" },
            {'آ', @"\'C2" },
            {'أ', @"\'C3" },
            {'ؤ', @"\'C4" },
            {'إ', @"\'C5" },
            {'ئ', @"\'C6" },
            {'ا', @"\'C7" },
            {'ب', @"\'C8" },
            {'ة', @"\'C9" },
            {'ت', @"\'CA" },
            {'ث', @"\'CB" },
            {'ج', @"\'CC" },
            {'ح', @"\'CD" },
            {'خ', @"\'CE" },
            {'د', @"\'CF" },
            {'ذ', @"\'D0" },
            {'ر', @"\'D1" },
            {'ز', @"\'D2" },
            {'س', @"\'D3" },
            {'ش', @"\'D4" },
            {'ص', @"\'D5" },
            {'ض', @"\'D6" },
            {'ط', @"\'D8" },
            {'ظ', @"\'D9" },
            {'ع', @"\'DA" },
            {'غ', @"\'DB" },
            {'ف', @"\'DD" },
            {'ق', @"\'DE" },
            {'ك', @"\'DF" },
            {'ل', @"\'E1" },
            {'م', @"\'E3" },
            {'ن', @"\'E4" },
            {'ه', @"\'E5" },
            {'و', @"\'E6" },
            {'ى', @"\'EC" },
            {'ي', @"\'ED" },
            {'ً', @"\'F0" },
            {'ٌ', @"\'F1" },
            {'ٍ', @"\'F2" },
            {'َ', @"\'F3" },
            {'ُ', @"\'F5" },
            {'ِ', @"\'F6" },
            {'ّ', @"\'F8" },
            {'ْ', @"\'FA" },
            {'ے', @"\'FF" }
        };

        StringBuilder str = new StringBuilder();
        if (dictionary != null)
        {
            foreach (char c in source)
            {
                if (dictionary.ContainsKey(c))
                {
                    str.Append(dictionary[c]);
                }
                else
                {
                    str.Append(c);
                }
            }
        }
        return str.ToString().ToLower();
    }
    public static string ToRtfTest(this string source)
    {
        if (String.IsNullOrEmpty(source)) return null;

        // change e to ë
        char[] chars = Encoding.Unicode.GetChars(new byte[] { 0xEB, 0x00 });

        StringBuilder str = new StringBuilder();
        foreach (char c in source)
        {
            if (c == 'e')
            {
                str.Append(ToRtfChars(new String(chars)));
            }
            else
            {
                str.Append(c);
            }
        }
        return (@"{\rtf {\fonttbl {\f0 Times New Roman;}} \f0\fs12 " + str.ToString() + "}");
    }
}
