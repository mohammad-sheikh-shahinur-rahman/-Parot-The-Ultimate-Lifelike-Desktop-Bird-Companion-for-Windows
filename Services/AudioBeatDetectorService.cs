using System;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Parot.Services;

public class AudioBeatDetectorService : IDisposable
{
    private readonly DispatcherTimer _sampleTimer;
    private IAudioMeterInformation? _audioMeter;
    private float _lastPeak = 0f;
    private bool _isMusicPlaying = false;
    private bool _initAttempted = false;

    public event Action<float>? BeatDetected;
    public event Action<bool>? MusicStateChanged;

    public bool IsMusicPlaying => _isMusicPlaying;
    public bool Enabled { get; set; } = true;

    public AudioBeatDetectorService()
    {
        _sampleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _sampleTimer.Tick += OnSampleTick;

        // Enable CoreAudio beat detector
        TryInitializeMeter();
        if (_audioMeter != null) _sampleTimer.Start();
    }

    private void TryInitializeMeter()
    {
        if (_initAttempted && _audioMeter != null) return;
        _initAttempted = true;

        try
        {
            var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            int hr = enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
            if (hr == 0 && device != null)
            {
                var iid = typeof(IAudioMeterInformation).GUID;
                const int CLSCTX_ALL = 23;
                hr = device.Activate(ref iid, CLSCTX_ALL, IntPtr.Zero, out var meterObj);
                if (hr == 0 && meterObj is IAudioMeterInformation meter)
                {
                    _audioMeter = meter;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio meter init failed: {ex.Message}");
            _audioMeter = null;
        }
    }

    private void OnSampleTick(object? sender, EventArgs e)
    {
        if (!Enabled || _audioMeter == null) return;

        try
        {
            int hr = _audioMeter.GetPeakValue(out float currentPeak);
            if (hr != 0) return;

            bool nowPlaying = currentPeak > 0.05f;
            if (nowPlaying != _isMusicPlaying)
            {
                _isMusicPlaying = nowPlaying;
                MusicStateChanged?.Invoke(_isMusicPlaying);
            }

            // Beat detection: Significant sudden pulse in audio
            if (nowPlaying && (currentPeak - _lastPeak > 0.06f || currentPeak > 0.25f))
            {
                BeatDetected?.Invoke(currentPeak);
            }

            _lastPeak = currentPeak;
        }
        catch
        {
            // Do not crash under any circumstances
            _audioMeter = null;
        }
    }

    public void Dispose()
    {
        _sampleTimer.Stop();
        _audioMeter = null;
    }

    #region COM Interop Definitions

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumerator { }

    private enum EDataFlow { eRender = 0, eCapture = 1, eAll = 2 }
    private enum ERole { eConsole = 0, eMultimedia = 1, eCommunications = 2 }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IntPtr ppDevices);

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

        [PreserveSig]
        int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);

        [PreserveSig]
        int RegisterEndpointNotificationCallback(IntPtr pClient);

        [PreserveSig]
        int UnregisterEndpointNotificationCallback(IntPtr pClient);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        [PreserveSig]
        int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);

        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

        [PreserveSig]
        int GetState(out int pdwState);
    }

    [Guid("C022167D-0308-4E45-83AC-87B5E2A0DFBA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioMeterInformation
    {
        [PreserveSig]
        int GetPeakValue(out float pfPeak);

        [PreserveSig]
        int GetMeteringChannelCount(out int pnChannelCount);

        [PreserveSig]
        int GetChannelsPeakValues(int u32ChannelCount, [In] IntPtr afPeakValues);

        [PreserveSig]
        int QueryHardwareSupport(out int pdwHardwareSupportMask);
    }

    #endregion
}
