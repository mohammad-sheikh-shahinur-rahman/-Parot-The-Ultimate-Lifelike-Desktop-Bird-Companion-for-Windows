using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Parot.Services;

public class DevWatcherService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    private readonly DispatcherTimer _timer;
    private string _lastDetectedTitle = "";
    private string _lastClipboardText = "";
    private DateTime _lastTriggerTime = DateTime.MinValue;
    private bool _disposed = false;

    public bool Enabled { get; set; } = true;

    public event Action<string>? BuildSucceeded;
    public event Action<string>? BuildFailed;

    public DevWatcherService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.5)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!Enabled) return;

        try
        {
            // 1. Inspect foreground window title
            IntPtr fgHwnd = GetForegroundWindow();
            if (fgHwnd != IntPtr.Zero)
            {
                var sb = new StringBuilder(256);
                if (GetWindowText(fgHwnd, sb, sb.Capacity) > 0)
                {
                    string title = sb.ToString();
                    if (!string.Equals(title, _lastDetectedTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        _lastDetectedTitle = title;
                        EvaluateText(title);
                    }
                }
            }

            // 2. Check Clipboard text for dev messages (e.g. copied terminal output)
            if (System.Windows.Clipboard.ContainsText())
            {
                string clip = System.Windows.Clipboard.GetText();
                if (clip.Length < 1000 && !string.Equals(clip, _lastClipboardText, StringComparison.Ordinal))
                {
                    _lastClipboardText = clip;
                    EvaluateText(clip);
                }
            }
        }
        catch
        {
            // Ignore clipboard lock or permission exceptions
        }
    }

    private void EvaluateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if ((DateTime.Now - _lastTriggerTime).TotalSeconds < 8) return; // Debounce 8s

        // Success patterns
        if (text.Contains("Build succeeded", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("compiled successfully", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Build successful", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("git push", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("commit succeeded", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("All tests passed", StringComparison.OrdinalIgnoreCase) ||
            (text.Contains("0 Error(s)", StringComparison.OrdinalIgnoreCase) && text.Contains("Build", StringComparison.OrdinalIgnoreCase)))
        {
            _lastTriggerTime = DateTime.Now;
            BuildSucceeded?.Invoke(text);
            return;
        }

        // Failure patterns
        if (text.Contains("Build FAILED", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Compilation error", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("error CS", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("fatal: ", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Test Run Failed", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("0 succeeded, 1 failed", StringComparison.OrdinalIgnoreCase))
        {
            _lastTriggerTime = DateTime.Now;
            BuildFailed?.Invoke(text);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
    }
}
