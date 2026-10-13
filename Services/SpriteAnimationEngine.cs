using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Parot.Services;

public class SpriteAnimationClip
{
    public string Name { get; set; } = string.Empty;
    public List<BitmapSource> Frames { get; set; } = new();
    public double Fps { get; set; } = 10.0;
    public bool IsLooping { get; set; } = true;
    public string? NextClipName { get; set; } = null;
}

public class SpriteAnimationEngine
{
    private readonly DispatcherTimer _frameTimer;
    private readonly Dictionary<string, SpriteAnimationClip> _clips = new(StringComparer.OrdinalIgnoreCase);

    private SpriteAnimationClip? _currentClip;
    private int _currentFrameIndex = 0;

    public event Action<BitmapSource>? FrameUpdated;
    public event Action<string>? AnimationCompleted;

    public string CurrentClipName => _currentClip?.Name ?? string.Empty;
    public double SpeedMultiplier { get; set; } = 1.0;

    public SpriteAnimationEngine()
    {
        _frameTimer = new DispatcherTimer(DispatcherPriority.Render);
        _frameTimer.Tick += OnFrameTick;
    }

    public void RegisterClip(SpriteAnimationClip clip)
    {
        _clips[clip.Name] = clip;
    }

    public void LoadClipsFromDirectory(string birdRootPath)
    {
        if (!Directory.Exists(birdRootPath)) return;

        // Load standard folders: Idle, Blink, Hop, TakeOff, Flying, Landing, Sleep
        RegisterFolder(birdRootPath, "Idle", fps: 5.0, isLooping: true);
        RegisterFolder(birdRootPath, "Blink", fps: 8.0, isLooping: false, nextClip: "Idle");
        RegisterFolder(birdRootPath, "Hop", fps: 9.0, isLooping: false, nextClip: "Idle");
        RegisterFolder(birdRootPath, "TakeOff", fps: 10.0, isLooping: false, nextClip: "Flying");
        RegisterFolder(birdRootPath, "Flying", fps: 12.0, isLooping: true);
        RegisterFolder(birdRootPath, "Landing", fps: 10.0, isLooping: false, nextClip: "Idle");
        RegisterFolder(birdRootPath, "Sleep", fps: 3.0, isLooping: true);
    }

    private void RegisterFolder(string root, string folderName, double fps, bool isLooping, string? nextClip = null)
    {
        string dir = Path.Combine(root, folderName);
        if (!Directory.Exists(dir)) return;

        var files = Directory.GetFiles(dir, "*.png");
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        var frames = new List<BitmapSource>();
        foreach (var file in files)
        {
            var bmp = LoadBitmap(file);
            if (bmp != null) frames.Add(bmp);
        }

        if (frames.Count > 0)
        {
            RegisterClip(new SpriteAnimationClip
            {
                Name = folderName,
                Frames = frames,
                Fps = fps,
                IsLooping = isLooping,
                NextClipName = nextClip
            });
        }
    }

    private static BitmapImage? LoadBitmap(string fullPath)
    {
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null;
        }
    }

    public bool Play(string clipName, bool restartIfAlreadyPlaying = false)
    {
        if (!_clips.TryGetValue(clipName, out var clip) || clip.Frames.Count == 0)
        {
            return false;
        }

        if (_currentClip?.Name == clipName && !restartIfAlreadyPlaying && _frameTimer.IsEnabled)
        {
            return true;
        }

        _currentClip = clip;
        _currentFrameIndex = 0;

        // Apply first frame immediately
        FrameUpdated?.Invoke(_currentClip.Frames[0]);

        UpdateTimerInterval();
        _frameTimer.Stop();
        _frameTimer.Start();

        return true;
    }

    public void Stop()
    {
        _frameTimer.Stop();
    }

    private void UpdateTimerInterval()
    {
        if (_currentClip == null || _currentClip.Fps <= 0) return;
        double effectiveFps = Math.Max(1.0, _currentClip.Fps * SpeedMultiplier);
        _frameTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / effectiveFps);
    }

    private void OnFrameTick(object? sender, EventArgs e)
    {
        if (_currentClip == null || _currentClip.Frames.Count == 0)
        {
            _frameTimer.Stop();
            return;
        }

        _currentFrameIndex++;

        if (_currentFrameIndex >= _currentClip.Frames.Count)
        {
            if (_currentClip.IsLooping)
            {
                _currentFrameIndex = 0;
            }
            else
            {
                string completedClipName = _currentClip.Name;
                string? next = _currentClip.NextClipName;

                _frameTimer.Stop();
                AnimationCompleted?.Invoke(completedClipName);

                if (!string.IsNullOrEmpty(next))
                {
                    Play(next);
                }
                return;
            }
        }

        FrameUpdated?.Invoke(_currentClip.Frames[_currentFrameIndex]);
    }
}
