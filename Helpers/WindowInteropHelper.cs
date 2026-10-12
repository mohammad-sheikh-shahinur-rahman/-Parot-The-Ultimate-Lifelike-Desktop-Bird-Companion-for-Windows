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
