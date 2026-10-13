using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using PowerLineStatus = System.Windows.Forms.PowerLineStatus;

namespace Parot.Services;

public class OsAwarenessService
{
    private readonly DispatcherTimer _pollTimer;
    private PowerLineStatus _lastPowerStatus = PowerLineStatus.Unknown;
    private bool _hasAlertedLowBattery = false;
    private string _lastClipboardText = string.Empty;
    private bool _lastNetworkAvailable = true;

    public event Action? ChargerPluggedIn;
    public event Action? ChargerUnplugged;
    public event Action<int>? LowBatteryWarning;
    public event Action<string>? ClipboardTextCopied;
    public event Action<bool>? NetworkAvailabilityChanged;

    public bool Enabled { get; set; } = true;

    public OsAwarenessService()
    {
        _lastNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        NetworkChange.NetworkAvailabilityChanged += OnNetworkChanged;

        try
        {
            _lastPowerStatus = SystemInformation.PowerStatus.PowerLineStatus;
        }
        catch
        {
            _lastPowerStatus = PowerLineStatus.Unknown;
        }

        // Poll every 1.5 seconds for battery status and clipboard changes
        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
        _pollTimer.Tick += OnPollTick;
        _pollTimer.Start();
    }

    private void OnPollTick(object? sender, EventArgs e)
    {
        if (!Enabled) return;

        CheckPowerStatus();
        CheckClipboard();
    }

    private void CheckPowerStatus()
    {
        try
        {
            var power = SystemInformation.PowerStatus;
            var currentLine = power.PowerLineStatus;

            // Detect charger plugged in
            if (_lastPowerStatus == PowerLineStatus.Offline && currentLine == PowerLineStatus.Online)
            {
                ChargerPluggedIn?.Invoke();
                _hasAlertedLowBattery = false;
            }
            // Detect charger unplugged
            else if (_lastPowerStatus == PowerLineStatus.Online && currentLine == PowerLineStatus.Offline)
            {
                ChargerUnplugged?.Invoke();
            }

            _lastPowerStatus = currentLine;

            // Detect low battery below 20%
            if (currentLine == PowerLineStatus.Offline)
            {
                int batteryPercent = (int)(power.BatteryLifePercent * 100);
                if (batteryPercent > 0 && batteryPercent <= 20 && !_hasAlertedLowBattery)
                {
                    _hasAlertedLowBattery = true;
                    LowBatteryWarning?.Invoke(batteryPercent);
                }
                else if (batteryPercent > 25)
                {
                    _hasAlertedLowBattery = false;
                }
            }
        }
        catch
        {
            // Ignore power query failures on non-battery desktop systems
        }
    }

    private void CheckClipboard()
    {
        try
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                string text = System.Windows.Clipboard.GetText();
                if (!string.IsNullOrEmpty(text) && text != _lastClipboardText)
                {
                    bool isInitial = string.IsNullOrEmpty(_lastClipboardText);
                    _lastClipboardText = text;

                    if (!isInitial)
                    {
                        string preview = text.Trim();
                        if (preview.Length > 24)
                        {
                            preview = preview.Substring(0, 24) + "...";
                        }
                        ClipboardTextCopied?.Invoke(preview);
                    }
                }
            }
        }
        catch
        {
            // Clipboard may be temporarily locked by other applications
        }
    }

    private void OnNetworkChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        if (!Enabled) return;

        if (_lastNetworkAvailable != e.IsAvailable)
        {
            _lastNetworkAvailable = e.IsAvailable;
            NetworkAvailabilityChanged?.Invoke(e.IsAvailable);
        }
    }

    public void Stop()
    {
        _pollTimer.Stop();
        NetworkChange.NetworkAvailabilityChanged -= OnNetworkChanged;
    }
}
