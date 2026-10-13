using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Parot.Services;

public class TypingDetectorService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private readonly DispatcherTimer _pollTimer;
    private int _keyPressCount = 0;
    private DateTime _lastReset = DateTime.Now;
    private DateTime _lastBobTime = DateTime.MinValue;
    private bool _disposed = false;

    public bool Enabled { get; set; } = true;
    public event Action<int>? FastTypingDetected;

    public TypingDetectorService()
    {
        _pollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _pollTimer.Tick += OnPollTick;
        _pollTimer.Start();
    }

    private void OnPollTick(object? sender, EventArgs e)
    {
        if (!Enabled) return;

        // Sample common typing keys (Letters A-Z, numbers 0-9, Space, Enter, Backspace)
        bool keyHit = false;
        for (int vk = 0x41; vk <= 0x5A; vk++) // A-Z
        {
            if ((GetAsyncKeyState(vk) & 0x8000) != 0)
            {
                keyHit = true;
                break;
            }
        }

        if (!keyHit)
        {
            if ((GetAsyncKeyState(0x20) & 0x8000) != 0 || // Space
                (GetAsyncKeyState(0x0D) & 0x8000) != 0 || // Enter
                (GetAsyncKeyState(0x08) & 0x8000) != 0)   // Backspace
            {
                keyHit = true;
            }
        }

        if (keyHit)
        {
            _keyPressCount++;
        }

        var elapsed = (DateTime.Now - _lastReset).TotalSeconds;
        if (elapsed >= 1.0)
        {
            if (_keyPressCount >= 3)
            {
                if ((DateTime.Now - _lastBobTime).TotalMilliseconds > 400)
                {
                    _lastBobTime = DateTime.Now;
                    FastTypingDetected?.Invoke(_keyPressCount);
                }
            }

            _keyPressCount = 0;
            _lastReset = DateTime.Now;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _pollTimer.Stop();
    }
}
