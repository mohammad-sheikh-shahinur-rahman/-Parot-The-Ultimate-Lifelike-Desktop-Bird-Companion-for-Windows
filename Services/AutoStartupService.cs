using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Parot.Services;

/// <summary>
/// Manages Windows boot auto-startup via HKCU\Software\Microsoft\Windows\CurrentVersion\Run registry key.
/// </summary>
public static class AutoStartupService
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Parot";

    public static bool IsAutoStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            if (key == null) return false;
            var val = key.GetValue(AppName) as string;
            return !string.IsNullOrEmpty(val);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to read auto-startup registry: {ex.Message}");
            return false;
        }
    }

    public static string GetExecutablePath()
    {
        return Process.GetCurrentProcess().MainModule?.FileName 
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Parot.exe");
    }

    public static bool SetAutoStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null) return false;

            if (enable)
            {
                string exePath = GetExecutablePath();
                key.SetValue(AppName, $"\"{exePath}\" --autostart");
            }
            else
            {
                if (key.GetValue(AppName) != null)
                {
                    key.DeleteValue(AppName, false);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to update auto-startup registry: {ex.Message}");
            return false;
        }
    }
}
