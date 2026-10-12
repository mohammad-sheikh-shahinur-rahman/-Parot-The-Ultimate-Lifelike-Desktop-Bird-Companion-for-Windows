using System;
using System.Windows.Threading;
using Parot.Models;

namespace Parot.Services;

public class ReminderEventArgs : EventArgs
{
    public ReminderType Type { get; }
    public string Message { get; }

    public ReminderEventArgs(ReminderType type, string message)
    {
        Type = type;
        Message = message;
    }
}

public class ReminderService
{
    private readonly DispatcherTimer _timer;
    private AppSettings _settings;

    private DateTime _lastEyeBreak = DateTime.Now;
    private DateTime _lastHydration = DateTime.Now;
    private DateTime _lastPosture = DateTime.Now;
    private DateTime _lastIdleChatter = DateTime.Now;

    public event EventHandler<ReminderEventArgs>? ReminderTriggered;

    public ReminderService(AppSettings settings)
    {
        _settings = settings;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) // Check every 5 seconds
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    public void UpdateSettings(AppSettings settings)
    {
        _settings = settings;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;

        // 1. Eye Break (20-20-20 rule)
        if (_settings.ScreenBreakEnabled &&
            (now - _lastEyeBreak).TotalMinutes >= _settings.ScreenBreakIntervalMinutes)
        {
            _lastEyeBreak = now;
            var text = Dialogues.GetEyeBreakDialogue(_settings.Language);
            ReminderTriggered?.Invoke(this, new ReminderEventArgs(ReminderType.ScreenBreak, text));
            return;
        }

        // 2. Hydration Reminder
        if (_settings.HydrationReminderEnabled &&
            (now - _lastHydration).TotalMinutes >= _settings.HydrationIntervalMinutes)
        {
            _lastHydration = now;
            var text = Dialogues.GetHydrationDialogue(_settings.Language);
            ReminderTriggered?.Invoke(this, new ReminderEventArgs(ReminderType.Hydration, text));
            return;
        }

        // 3. Posture Reminder
        if (_settings.PostureReminderEnabled &&
            (now - _lastPosture).TotalMinutes >= _settings.PostureIntervalMinutes)
        {
            _lastPosture = now;
            var text = Dialogues.GetPostureDialogue(_settings.Language);
            ReminderTriggered?.Invoke(this, new ReminderEventArgs(ReminderType.Posture, text));
            return;
        }

        // 4. Occasional Idle Thought (every 7-12 minutes)
        if (_settings.IdleChirping && (now - _lastIdleChatter).TotalMinutes >= 8)
        {
            _lastIdleChatter = now;
            var text = Dialogues.GetIdleDialogue(_settings.Language);
            ReminderTriggered?.Invoke(this, new ReminderEventArgs(ReminderType.IdleThought, text));
        }
    }

    public void Snooze(ReminderType type, int minutes = 5)
    {
        var targetTime = DateTime.Now.AddMinutes(minutes);
        switch (type)
        {
            case ReminderType.ScreenBreak:
                _lastEyeBreak = targetTime.AddMinutes(-_settings.ScreenBreakIntervalMinutes);
                break;
            case ReminderType.Hydration:
                _lastHydration = targetTime.AddMinutes(-_settings.HydrationIntervalMinutes);
                break;
            case ReminderType.Posture:
                _lastPosture = targetTime.AddMinutes(-_settings.PostureIntervalMinutes);
                break;
        }
    }

    public void ResetTimer(ReminderType type)
    {
        switch (type)
        {
            case ReminderType.ScreenBreak:
                _lastEyeBreak = DateTime.Now;
                break;
            case ReminderType.Hydration:
                _lastHydration = DateTime.Now;
                break;
            case ReminderType.Posture:
                _lastPosture = DateTime.Now;
                break;
            case ReminderType.IdleThought:
                _lastIdleChatter = DateTime.Now;
                break;
        }
    }

    public void TriggerBreakNow()
    {
        _lastEyeBreak = DateTime.Now;
        var text = Dialogues.GetEyeBreakDialogue(_settings.Language);
        ReminderTriggered?.Invoke(this, new ReminderEventArgs(ReminderType.ScreenBreak, text));
    }

    public void TriggerHydrationNow()
    {
        _lastHydration = DateTime.Now;
        var text = Dialogues.GetHydrationDialogue(_settings.Language);
        ReminderTriggered?.Invoke(this, new ReminderEventArgs(ReminderType.Hydration, text));
    }
}
