using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace Parot.Services;

public class MicrophoneMimicService : IDisposable
{
    [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
    private static extern int mciSendString(string lpstrCommand, StringBuilder? lpstrReturnString, int uReturnLength, IntPtr hwndCallback);

    private SpeechRecognitionEngine? _speechEngine;
    private bool _isRecording = false;
    private bool _disposed = false;
    private readonly string _tempDir;

    public bool VoiceCommandsEnabled { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;

    public void StopPlayback()
    {
        try
        {
            mciSendString("close parot_play", null, 0, IntPtr.Zero);
            mciSendString("close parot_rec", null, 0, IntPtr.Zero);
        }
        catch { }
    }

    public event Action? RecordingStarted;
    public event Action? MimicPlaybackStarted;
    public event Action? MimicPlaybackCompleted;
    public event Action<string>? CommandRecognized;

    public MicrophoneMimicService(bool enableCommands = true)
    {
        VoiceCommandsEnabled = enableCommands;
        _tempDir = Path.Combine(Path.GetTempPath(), "ParotVoice");
        try
        {
            Directory.CreateDirectory(_tempDir);
        }
        catch { }

        if (VoiceCommandsEnabled)
        {
            InitializeVoiceRecognition();
        }
    }

    private void InitializeVoiceRecognition()
    {
        try
        {
            // Initialize system speech recognizer
            _speechEngine = new SpeechRecognitionEngine();
            var choices = new Choices();
            choices.Add(new[] {
                "fly", "take off", "flying",
                "sleep", "go to sleep", "good night",
                "wake up", "wake",
                "eat", "food", "hungry", "feed",
                "dance", "party",
                "come here", "perch"
            });

            var gb = new GrammarBuilder(choices);
            var grammar = new Grammar(gb);
            _speechEngine.LoadGrammar(grammar);

            _speechEngine.SpeechRecognized += (s, e) =>
            {
                if (e.Result.Confidence > 0.55f)
                {
                    CommandRecognized?.Invoke(e.Result.Text.ToLowerInvariant());
                }
            };

            _speechEngine.SetInputToDefaultAudioDevice();
            _speechEngine.RecognizeAsync(RecognizeMode.Multiple);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SpeechRecognition initialization failed (fallback to manual mic): {ex.Message}");
            _speechEngine = null;
        }
    }

    /// <summary>
    /// Records from microphone for specified seconds, shifts pitch to parrot voice, and plays back!
    /// </summary>
    public async Task RecordAndEchoParrotVoiceAsync(int durationSeconds = 3)
    {
        if (_isRecording) return;
        _isRecording = true;

        string rawWav = Path.Combine(_tempDir, "parot_raw.wav");
        string parrotWav = Path.Combine(_tempDir, "parot_echo.wav");

        try
        {
            RecordingStarted?.Invoke();

            // 1. Record via Windows Multimedia MCI
            mciSendString("close parot_rec", null, 0, IntPtr.Zero);
            mciSendString("open new type waveaudio alias parot_rec", null, 0, IntPtr.Zero);
            mciSendString("set parot_rec bitspersample 16 channels 1 samplespersec 22050", null, 0, IntPtr.Zero);
            mciSendString("record parot_rec", null, 0, IntPtr.Zero);

            await Task.Delay(durationSeconds * 1000);

            mciSendString($"save parot_rec \"{rawWav}\"", null, 0, IntPtr.Zero);
            mciSendString("close parot_rec", null, 0, IntPtr.Zero);

            if (File.Exists(rawWav))
            {
                // 2. Pitch-shift WAV by altering the sample rate in the RIFF header
                PitchShiftWavFile(rawWav, parrotWav, pitchMultiplier: 1.35f);

                // 3. Play the squeaky parrot mimic back if sound is enabled
                if (SoundEnabled && !SoundService.MasterMute)
                {
                    MimicPlaybackStarted?.Invoke();
                    mciSendString("close parot_play", null, 0, IntPtr.Zero);
                    mciSendString($"open \"{parrotWav}\" type waveaudio alias parot_play", null, 0, IntPtr.Zero);
                    mciSendString("play parot_play wait", null, 0, IntPtr.Zero);
                    mciSendString("close parot_play", null, 0, IntPtr.Zero);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Voice mimic failed: {ex.Message}");
        }
        finally
        {
            _isRecording = false;
            MimicPlaybackCompleted?.Invoke();
        }
    }

    private static void PitchShiftWavFile(string inputPath, string outputPath, float pitchMultiplier)
    {
        byte[] bytes = File.ReadAllBytes(inputPath);
        if (bytes.Length > 44)
        {
            // Standard WAV RIFF header:
            // Offset 24-27: Sample Rate (uint32)
            // Offset 28-31: Byte Rate (uint32)
            uint sampleRate = BitConverter.ToUInt32(bytes, 24);
            uint byteRate = BitConverter.ToUInt32(bytes, 28);

            uint newSampleRate = (uint)(sampleRate * pitchMultiplier);
            uint newByteRate = (uint)(byteRate * pitchMultiplier);

            byte[] newSampleRateBytes = BitConverter.GetBytes(newSampleRate);
            byte[] newByteRateBytes = BitConverter.GetBytes(newByteRate);

            Array.Copy(newSampleRateBytes, 0, bytes, 24, 4);
            Array.Copy(newByteRateBytes, 0, bytes, 28, 4);

            File.WriteAllBytes(outputPath, bytes);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _speechEngine?.RecognizeAsyncCancel();
            _speechEngine?.Dispose();
        }
        catch { }

        mciSendString("close parot_rec", null, 0, IntPtr.Zero);
        mciSendString("close parot_play", null, 0, IntPtr.Zero);
    }
}
