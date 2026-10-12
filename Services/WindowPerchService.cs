using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Parot.Services;

public class WindowPerchService
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    public static bool GetActiveWindowPerchPosition(IntPtr ignoreHwnd, double petWidth, double petHeight, out System.Windows.Point perchPoint)
    {
        perchPoint = new System.Windows.Point(0, 0);

        var foregroundHwnd = GetForegroundWindow();
        if (foregroundHwnd == IntPtr.Zero || foregroundHwnd == ignoreHwnd || foregroundHwnd == GetShellWindow())
        {
            return false;
        }

        var sb = new StringBuilder(256);
        GetWindowText(foregroundHwnd, sb, 256);
        string title = sb.ToString();
        if (string.IsNullOrWhiteSpace(title) || title == "Parot")
        {
            return false;
        }

        if (GetWindowRect(foregroundHwnd, out var rect))
        {
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // Only perch on real application windows (width > 300, height > 200)
            if (width > 300 && height > 200)
            {
                // Place near top-right of the titlebar
                double targetX = rect.Right - petWidth - 30;
                double targetY = rect.Top - petHeight + 35;

                // Ensure it doesn't go completely off-screen
                var workArea = SystemParameters.WorkArea;
                targetX = Math.Clamp(targetX, workArea.Left, workArea.Right - petWidth);
                targetY = Math.Clamp(targetY, workArea.Top, workArea.Bottom - petHeight);

                perchPoint = new System.Windows.Point(targetX, targetY);
                return true;
            }
        }

        return false;
    }
}
