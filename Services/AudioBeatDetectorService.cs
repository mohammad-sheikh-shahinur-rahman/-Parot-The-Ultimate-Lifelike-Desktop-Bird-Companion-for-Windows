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

    public event Action<float>? BeatDetected;
    public event Action<bool>? MusicStateChanged;

    public bool IsMusicPlaying => _isMusicPlaying;
    public bool Enabled { get; set; } = true;

    public AudioBeatDetectorService()
    {
        InitializeMeter();

        _sampleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(80) // 12.5 checks per second, ultra lightweight
        };
        _sampleTimer.Tick += OnSampleTick;
        _sampleTimer.Start();
    }

    private void InitializeMeter()
    {
        try
        {
            var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
            if (device != null)
            {
                var iid = typeof(IAudioMeterInformation).GUID;
                device.Activate(ref iid, 0, IntPtr.Zero, out var meterObj);
                _audioMeter = meterObj as IAudioMeterInformation;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio meter init failed: {ex.Message}");
        }
    }

    private void OnSampleTick(object? sender, EventArgs e)
    {
        if (!Enabled || _audioMeter == null) return;

        try
        {
            _audioMeter.GetPeakValue(out float currentPeak);

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
            // If device changed, try to re-initialize
            _audioMeter = null;
            InitializeMeter();
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

    private enum EDataFlow { eRender, eCapture, eAll }
    private enum ERole { eConsole, eMultimedia, eCommunications }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        int NotImpl1();
        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }

    [Guid("C022167D-0308-4E45-83AC-87B5E2A0DFBA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioMeterInformation
    {
        [PreserveSig]
        int GetPeakValue(out float pfPeak);
    }

    #endregion
}
