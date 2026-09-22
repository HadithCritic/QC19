using System;
using System.Threading;
using System.Runtime.InteropServices;

public static class MouseSimulator
{
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int X, int Y);
    public static void MoveTo(int x, int y)
    {
        SetCursorPos(x, y);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const int MOUSEEVENTF_RIGHTUP = 0x10;
    public static void Click()
    {
        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }

    static void Demo()
    {
        bool toggle = false;
        while (true)
        {
            toggle = !toggle;
            if (toggle)
            {
                MouseSimulator.MoveTo(200, 200);
            }
            else
            {
                MouseSimulator.MoveTo(400, 200);
            }
            MouseSimulator.Click();
            Thread.Sleep(1000);
        }
    }
}
