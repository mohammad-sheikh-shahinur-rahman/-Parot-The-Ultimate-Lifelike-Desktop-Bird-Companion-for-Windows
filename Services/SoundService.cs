using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Parot.Services;

public class SoundService
{
    private bool _enabled = true;
    private double _volume = 0.8;
    private readonly Random _random = new();

    private readonly MemoryStream? _chirpStream;
    private readonly MemoryStream? _doubleChirpStream;
    private readonly MemoryStream? _trillStream;
    private readonly MemoryStream? _munchStream;
    private readonly MemoryStream? _chimeStream;
    private readonly MemoryStream? _snoreStream;

    private readonly string _voiceFolder;
    private readonly string? _fileChatter;
    private readonly string? _fileMimic;
    private readonly string? _fileScreech;
    private readonly string? _fileJungle;
    private readonly string? _fileKea;
    private readonly string? _fileMacaw;

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public double Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0, 1.0);
    }

    public SoundService(bool enabled = true, double volume = 0.8)
    {
        _enabled = enabled;
        _volume = volume;

        // 1. Locate Voice Folder (first check local Assets\Sounds, then K:\Parot\Parot Voice)
        string localFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds");
        string explicitFolder = @"K:\Parot\Parot Voice";

        _voiceFolder = Directory.Exists(localFolder) && Directory.GetFiles(localFolder, "*.mp3").Length > 0
            ? localFolder
            : (Directory.Exists(explicitFolder) ? explicitFolder : localFolder);

        if (Directory.Exists(_voiceFolder))
        {
            _fileChatter = FindAudioFile("chatter");
            _fileMimic = FindAudioFile("imitating");
            _fileScreech = FindAudioFile("screech");
            _fileJungle = FindAudioFile("88486");
            _fileKea = FindAudioFile("105461");
            _fileMacaw = FindAudioFile("31178");
        }

        // 2. Fallback procedural synthesis
        try
        {
            _chirpStream = CreateChirpWav(2400, 3800, 0.12, 0.8);
            _doubleChirpStream = CreateDoubleChirpWav();
            _trillStream = CreateTrillWav(2800, 0.22, 0.7);
            _munchStream = CreateMunchWav();
            _chimeStream = CreateChimeWav();
            _snoreStream = CreateSnoreWav();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error pre-generating sound effects: {ex.Message}");
        }
    }

    private string? FindAudioFile(string pattern)
    {
        try
        {
            if (!Directory.Exists(_voiceFolder)) return null;
            var files = Directory.GetFiles(_voiceFolder, $"*{pattern}*.mp3");
            return files.Length > 0 ? files[0] : null;
        }
        catch
        {
            return null;
        }
    }

    // High level sound triggers
    public void PlayChirp()
    {
        // Try real Kea or screech first
        if (_fileKea != null && _random.Next(2) == 0)
        {
            PlayMp3(_fileKea);
        }
        else if (_fileScreech != null && _random.Next(3) == 0)
        {
            PlayMp3(_fileScreech);
        }
        else
        {
            PlayStream(_chirpStream);
        }
    }

    public void PlayDoubleChirp()
    {
        if (_fileMimic != null && _random.Next(2) == 0)
        {
            PlayMp3(_fileMimic);
        }
        else if (_fileChatter != null)
        {
            PlayMp3(_fileChatter);
        }
        else
        {
            PlayStream(_doubleChirpStream);
        }
    }

    public void PlayTrill()
    {
        if (_fileChatter != null)
        {
            PlayMp3(_fileChatter);
        }
        else if (_fileJungle != null)
        {
            PlayMp3(_fileJungle);
        }
        else
        {
            PlayStream(_trillStream);
        }
    }

    public void PlayRealParrotChatter()
    {
        if (_fileChatter != null) PlayMp3(_fileChatter);
        else PlayStream(_doubleChirpStream);
    }

    public void PlayRealParrotScreech()
    {
        if (_fileScreech != null) PlayMp3(_fileScreech);
        else PlayStream(_chirpStream);
    }

    public void PlayRealParrotMimic()
    {
        if (_fileMimic != null) PlayMp3(_fileMimic);
        else PlayStream(_trillStream);
    }

    public void PlayMunch() => PlayStream(_munchStream);
    public void PlayChime() => PlayStream(_chimeStream);
    public void PlaySnore() => PlayStream(_snoreStream);

    private void PlayMp3(string? filePath)
    {
        if (!_enabled || string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || _volume <= 0.01)
        {
            return;
        }

        try
        {
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        var player = new MediaPlayer();
                        player.Volume = _volume;
                        player.Open(new Uri(filePath, UriKind.Absolute));
                        player.Play();
                        player.MediaEnded += (s, e) => player.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"MediaPlayer error: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to play MP3: {ex.Message}");
        }
    }

    private void PlayStream(MemoryStream? stream)
    {
        if (!_enabled || stream == null || _volume <= 0.01) return;

        Task.Run(() =>
        {
            try
            {
                stream.Position = 0;
                using var player = new SoundPlayer(stream);
                player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sound play failed: {ex.Message}");
            }
        });
    }

    #region Procedural Fallbacks

    private static MemoryStream CreateChirpWav(double startFreq, double endFreq, double durationSeconds, double maxAmp)
    {
        const int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / sampleRate;
            double progress = t / durationSeconds;

            double freq = startFreq + (endFreq - startFreq) * Math.Pow(progress, 1.2);
            double phase = 2.0 * Math.PI * (startFreq * t + 0.5 * (endFreq - startFreq) * t * progress);

            double envelope = Math.Sin(Math.PI * Math.Pow(progress, 0.6));
            double sampleValue = Math.Sin(phase) * envelope * maxAmp * short.MaxValue;
            samples[i] = (short)Math.Clamp(sampleValue, short.MinValue, short.MaxValue);
        }

        return EncodeWav(samples, sampleRate);
    }

    private static MemoryStream CreateDoubleChirpWav()
    {
        const int sampleRate = 44100;
        const double duration = 0.28;
        int totalSamples = (int)(sampleRate * duration);
        var samples = new short[totalSamples];

        int chirp1End = (int)(sampleRate * 0.10);
        int gapEnd = (int)(sampleRate * 0.14);
        int chirp2End = (int)(sampleRate * 0.26);

        for (int i = 0; i < chirp1End; i++)
        {
            double t = (double)i / sampleRate;
            double p = (double)i / chirp1End;
            double freq = 2200 + 1200 * p;
            double env = Math.Sin(Math.PI * p) * 0.75;
            samples[i] = (short)(Math.Sin(2.0 * Math.PI * freq * t) * env * short.MaxValue);
        }

        for (int i = gapEnd; i < chirp2End; i++)
        {
            double t = (double)(i - gapEnd) / sampleRate;
            double p = (double)(i - gapEnd) / (chirp2End - gapEnd);
            double freq = 2600 + 1500 * p;
            double env = Math.Sin(Math.PI * p) * 0.85;
            samples[i] = (short)(Math.Sin(2.0 * Math.PI * freq * t) * env * short.MaxValue);
        }

        return EncodeWav(samples, sampleRate);
    }

    private static MemoryStream CreateTrillWav(double centerFreq, double durationSeconds, double maxAmp)
    {
        const int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];

        double phase = 0;
        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / sampleRate;
            double progress = t / durationSeconds;

            double vibrato = Math.Sin(2.0 * Math.PI * 22.0 * t) * 350.0;
            double currentFreq = centerFreq + vibrato;
            phase += 2.0 * Math.PI * currentFreq / sampleRate;

            double envelope = Math.Sin(Math.PI * progress);
            double sampleValue = Math.Sin(phase) * envelope * maxAmp * short.MaxValue;
            samples[i] = (short)Math.Clamp(sampleValue, short.MinValue, short.MaxValue);
        }

        return EncodeWav(samples, sampleRate);
    }

    private static MemoryStream CreateMunchWav()
    {
        const int sampleRate = 44100;
        const double duration = 0.25;
        int totalSamples = (int)(sampleRate * duration);
        var samples = new short[totalSamples];
        var rand = new Random(42);

        int[] crunchStarts = [0, (int)(sampleRate * 0.08), (int)(sampleRate * 0.16)];
        int crunchLen = (int)(sampleRate * 0.04);

        foreach (var start in crunchStarts)
        {
            for (int i = 0; i < crunchLen && (start + i) < totalSamples; i++)
            {
                double p = (double)i / crunchLen;
                double env = (1.0 - p) * 0.6;
                double tone = Math.Sin(2.0 * Math.PI * (1600 + rand.Next(-300, 300)) * (double)i / sampleRate);
                samples[start + i] = (short)(tone * env * short.MaxValue);
            }
        }

        return EncodeWav(samples, sampleRate);
    }

    private static MemoryStream CreateChimeWav()
    {
        const int sampleRate = 44100;
        const double duration = 0.6;
        int totalSamples = (int)(sampleRate * duration);
        var samples = new short[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / sampleRate;
            double progress = t / duration;
            double env = Math.Exp(-progress * 5.0) * 0.6;

            double val = Math.Sin(2.0 * Math.PI * 1046.5 * t) * 0.4
                       + Math.Sin(2.0 * Math.PI * 1318.5 * t) * 0.35
                       + Math.Sin(2.0 * Math.PI * 1567.9 * t) * 0.25;

            samples[i] = (short)(val * env * short.MaxValue);
        }

        return EncodeWav(samples, sampleRate);
    }

    private static MemoryStream CreateSnoreWav()
    {
        const int sampleRate = 44100;
        const double duration = 0.5;
        int totalSamples = (int)(sampleRate * duration);
        var samples = new short[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / sampleRate;
            double progress = t / duration;
            double env = Math.Sin(Math.PI * progress) * 0.35;
            double tone = Math.Sin(2.0 * Math.PI * (380.0 + Math.Sin(progress * Math.PI) * 40.0) * t);
            samples[i] = (short)(tone * env * short.MaxValue);
        }

        return EncodeWav(samples, sampleRate);
    }

    private static MemoryStream EncodeWav(short[] samples, int sampleRate)
    {
        var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            int byteRate = sampleRate * 2;
            int dataChunkSize = samples.Length * 2;

            writer.Write("RIFF"u8.ToArray());
            writer.Write(36 + dataChunkSize);
            writer.Write("WAVE"u8.ToArray());

            writer.Write("fmt "u8.ToArray());
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)2);
            writer.Write((short)16);

            writer.Write("data"u8.ToArray());
            writer.Write(dataChunkSize);
            foreach (var sample in samples)
            {
                writer.Write(sample);
            }
        }

        ms.Position = 0;
        return ms;
    }

    #endregion
}
