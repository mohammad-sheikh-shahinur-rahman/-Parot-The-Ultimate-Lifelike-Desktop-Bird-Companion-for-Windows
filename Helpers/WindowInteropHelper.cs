using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Parot.Helpers;

public static class WindowInteropHelperNative
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public static System.Windows.Point GetCurrentMousePosition()
    {
        if (GetCursorPos(out var p))
        {
            return new System.Windows.Point(p.X, p.Y);
        }
        return new System.Windows.Point(0, 0);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    public static Rect GetMonitorWorkAreaFromPoint(System.Windows.Point pt)
    {
        try
        {
            var p = new POINT { X = (int)Math.Round(pt.X), Y = (int)Math.Round(pt.Y) };
            IntPtr hMonitor = MonitorFromPoint(p, MONITOR_DEFAULTTONEAREST);
            if (hMonitor != IntPtr.Zero)
            {
                var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    return new Rect(
                        mi.rcWork.Left,
                        mi.rcWork.Top,
                        mi.rcWork.Right - mi.rcWork.Left,
                        mi.rcWork.Bottom - mi.rcWork.Top);
                }
            }
        }
        catch { }

        return SystemParameters.WorkArea;
    }

    public static Rect GetMonitorWorkArea(IntPtr hwnd)
    {
        try
        {
            if (hwnd != IntPtr.Zero)
            {
                IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                if (hMonitor != IntPtr.Zero)
                {
                    var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
                    if (GetMonitorInfo(hMonitor, ref mi))
                    {
                        return new Rect(
                            mi.rcWork.Left,
                            mi.rcWork.Top,
                            mi.rcWork.Right - mi.rcWork.Left,
                            mi.rcWork.Bottom - mi.rcWork.Top);
                    }
                }
            }
        }
        catch { }

        return SystemParameters.WorkArea;
    }

    public static void SetClickThrough(Window window, bool enable)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        int currentExStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        if (enable)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, currentExStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }
        else
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, currentExStyle & ~WS_EX_TRANSPARENT);
        }
    }
}
