using System;
using System.Windows.Threading;
using Parot.Models;

namespace Parot.Services;

public class PomodoroService
{
    private readonly DispatcherTimer _timer;
    private PomodoroPhase _currentPhase = PomodoroPhase.Stopped;
    private TimeSpan _timeRemaining;
    private int _completedCycles = 0;

    public int FocusMinutes { get; set; } = 25;
    public int BreakMinutes { get; set; } = 5;

    public PomodoroPhase CurrentPhase => _currentPhase;
    public TimeSpan TimeRemaining => _timeRemaining;
    public int CompletedCycles => _completedCycles;

    public event Action<TimeSpan, PomodoroPhase>? Tick;
    public event Action<PomodoroPhase>? PhaseChanged;

    public PomodoroService(int focusMinutes = 25, int breakMinutes = 5)
    {
        FocusMinutes = focusMinutes;
        BreakMinutes = breakMinutes;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
    }

    public void StartFocus()
    {
        _currentPhase = PomodoroPhase.Focus;
        _timeRemaining = TimeSpan.FromMinutes(FocusMinutes);
        _timer.Start();
        PhaseChanged?.Invoke(_currentPhase);
    }

    public void StartBreak()
    {
        _currentPhase = PomodoroPhase.ShortBreak;
        _timeRemaining = TimeSpan.FromMinutes(BreakMinutes);
        _timer.Start();
        PhaseChanged?.Invoke(_currentPhase);
    }

    public void Stop()
    {
        _timer.Stop();
        _currentPhase = PomodoroPhase.Stopped;
        PhaseChanged?.Invoke(_currentPhase);
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_timeRemaining.TotalSeconds > 1)
        {
            _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
            Tick?.Invoke(_timeRemaining, _currentPhase);
        }
        else
        {
            // Transition phase
            if (_currentPhase == PomodoroPhase.Focus)
            {
                _completedCycles++;
                StartBreak();
            }
            else
            {
                Stop();
            }
        }
    }
}
