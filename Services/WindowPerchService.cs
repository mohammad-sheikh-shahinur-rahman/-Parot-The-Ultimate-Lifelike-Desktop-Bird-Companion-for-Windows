using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Parot.Helpers;
using Point = System.Windows.Point;

namespace Parot.Services;

public class WindowPerchService
{
    private static readonly Random _random = new();

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

    private const int GWL_EXSTYLE = -20;
    private const int GWL_STYLE = -16;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const long WS_CHILD = 0x40000000L;
    private const long WS_CAPTION = 0x00C00000L;
    private const int DWMWA_CLOAKED = 14;

    public static bool IsWindowCloaked(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero) return false;
        try
        {
            if (DwmGetWindowAttribute(hWnd, DWMWA_CLOAKED, out int isCloaked, sizeof(int)) == 0)
            {
                return isCloaked != 0;
            }
        }
        catch { }
        return false;
    }

    public static bool TryGetWindowRect(IntPtr hWnd, out RECT rect)
    {
        rect = default;
        if (hWnd == IntPtr.Zero || !IsWindow(hWnd) || IsIconic(hWnd) || !IsWindowVisible(hWnd) || IsWindowCloaked(hWnd))
        {
            return false;
        }
        return GetWindowRect(hWnd, out rect);
    }

    public static bool IsValidPerchCandidate(IntPtr hWnd, IntPtr ignoreHwnd, IntPtr shellHwnd, out string title, out RECT rect)
    {
        title = string.Empty;
        rect = default;

        if (hWnd == IntPtr.Zero || hWnd == ignoreHwnd || hWnd == shellHwnd) return false;
        if (!IsWindow(hWnd) || IsIconic(hWnd) || !IsWindowVisible(hWnd) || IsWindowCloaked(hWnd)) return false;

        int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        long style = GetWindowLong(hWnd, GWL_STYLE);
        if ((exStyle & WS_EX_TOOLWINDOW) != 0 || (style & WS_CHILD) != 0) return false;

        // Ensure the window has standard application caption/titlebar
        if ((style & WS_CAPTION) != WS_CAPTION) return false;

        var classSb = new StringBuilder(256);
        GetClassName(hWnd, classSb, 256);
        string className = classSb.ToString();
        if (className is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd" or "Windows.UI.Core.CoreWindow")
        {
            return false;
        }

        var sb = new StringBuilder(256);
        GetWindowText(hWnd, sb, 256);
        title = sb.ToString();
        if (string.IsNullOrWhiteSpace(title) || title == "Parot") return false;

        // Filter system-level shell titles that aren't user application tabs
        if (title is "Program Manager" or "Microsoft Text Input Application" or "Windows Shell Experience Host" or "Settings" or "Cortana" or "Search" or "Start")
        {
            return false;
        }

        if (!GetWindowRect(hWnd, out rect)) return false;

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        if (width < 350 || height < 220) return false;

        var workArea = WindowInteropHelperNative.GetMonitorWorkArea(hWnd);

        // Check if window actually intersects with visible monitor work area
        if (rect.Right <= workArea.Left || rect.Left >= workArea.Right || rect.Bottom <= workArea.Top || rect.Top >= workArea.Bottom)
        {
            return false;
        }

        return true;
    }

    public static string GetActiveWindowTitle()
    {
        var foregroundHwnd = GetForegroundWindow();
        if (foregroundHwnd == IntPtr.Zero || foregroundHwnd == GetShellWindow()) return string.Empty;
        var sb = new StringBuilder(256);
        GetWindowText(foregroundHwnd, sb, 256);
        return sb.ToString();
    }

    public static bool GetActiveWindowPerchPosition(IntPtr ignoreHwnd, double petWidth, double petHeight, out Point perchPoint, out IntPtr perchedHwnd, int spot = -1)
    {
        perchPoint = new Point(0, 0);
        perchedHwnd = IntPtr.Zero;

        var foregroundHwnd = GetForegroundWindow();
        if (!IsValidPerchCandidate(foregroundHwnd, ignoreHwnd, GetShellWindow(), out _, out var rect))
        {
            return false;
        }

        var workArea = WindowInteropHelperNative.GetMonitorWorkArea(foregroundHwnd);

        if (spot < 0)
        {
            spot = _random.Next(3);
        }

        double targetX = spot switch
        {
            1 => rect.Left + 50,
            2 => (rect.Left + rect.Right) / 2.0 - petWidth / 2.0,
            _ => rect.Right - petWidth - 50
        };

        // If window is maximized or top is near top edge: perch at top edge
        double targetY;
        if (rect.Top <= workArea.Top + 35)
        {
            targetY = workArea.Top;
        }
        else if (rect.Top <= workArea.Top + 240)
        {
            targetY = rect.Top - petHeight + 6;
        }
        else
        {
            // Window titlebar is too low/obscure, avoid floating mid-air
            return false;
        }

        targetX = Math.Clamp(targetX, workArea.Left, workArea.Right - petWidth);
        targetY = Math.Clamp(targetY, workArea.Top, workArea.Bottom - petHeight);

        perchPoint = new Point(targetX, targetY);
        perchedHwnd = foregroundHwnd;
        return true;
    }

    public static bool GetActiveWindowPerchPosition(IntPtr ignoreHwnd, double petWidth, double petHeight, out Point perchPoint, int spot = -1)
    {
        return GetActiveWindowPerchPosition(ignoreHwnd, petWidth, petHeight, out perchPoint, out _, spot);
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    public record PerchTarget(IntPtr Hwnd, string Title, RECT Rect, Point PerchPoint);

    public static System.Collections.Generic.List<PerchTarget> GetAllVisiblePerchWindows(IntPtr ignoreHwnd, double petWidth, double petHeight)
    {
        var list = new System.Collections.Generic.List<PerchTarget>();
        var shellHwnd = GetShellWindow();

        EnumWindows((hWnd, lParam) =>
        {
            if (IsValidPerchCandidate(hWnd, ignoreHwnd, shellHwnd, out string title, out var rect))
            {
                var workArea = WindowInteropHelperNative.GetMonitorWorkArea(hWnd);

                double targetY;
                if (rect.Top <= workArea.Top + 35)
                {
                    targetY = workArea.Top;
                }
                else if (rect.Top <= workArea.Top + 240 && (rect.Bottom - rect.Top) < workArea.Height - 30)
                {
                    targetY = rect.Top - petHeight + 6;
                }
                else
                {
                    // Too low to be a natural titlebar perch, skip to avoid mid-screen float
                    return true;
                }

                int spot = _random.Next(3);
                double targetX = spot switch
                {
                    1 => rect.Left + 50,
                    2 => (rect.Left + rect.Right) / 2.0 - petWidth / 2.0,
                    _ => rect.Right - petWidth - 50
                };

                targetX = Math.Clamp(targetX, workArea.Left, workArea.Right - petWidth);
                targetY = Math.Clamp(targetY, workArea.Top, workArea.Bottom - petHeight);

                list.Add(new PerchTarget(hWnd, title, rect, new Point(targetX, targetY)));
            }
            return true;
        }, IntPtr.Zero);

        return list;
    }

    public static bool GetRandomWindowPerchPosition(IntPtr ignoreHwnd, double petWidth, double petHeight, out Point perchPoint, out IntPtr perchedHwnd, out string windowTitle)
    {
        perchPoint = new Point(0, 0);
        perchedHwnd = IntPtr.Zero;
        windowTitle = string.Empty;

        var allWindows = GetAllVisiblePerchWindows(ignoreHwnd, petWidth, petHeight);
        if (allWindows.Count > 0)
        {
            var chosen = allWindows[_random.Next(allWindows.Count)];
            perchPoint = chosen.PerchPoint;
            perchedHwnd = chosen.Hwnd;
            windowTitle = chosen.Title;
            return true;
        }

        // Fallback to active window
        if (GetActiveWindowPerchPosition(ignoreHwnd, petWidth, petHeight, out perchPoint, out perchedHwnd))
        {
            var sb = new StringBuilder(256);
            GetWindowText(perchedHwnd, sb, 256);
            windowTitle = sb.ToString();
            return true;
        }

        return false;
    }
}
