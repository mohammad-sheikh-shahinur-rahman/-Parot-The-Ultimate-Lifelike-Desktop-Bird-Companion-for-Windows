using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Parot.Models;

namespace Parot.Views;

public partial class CompanionWindow : Window
{
    private readonly DispatcherTimer _followTimer;
    private double _targetX = 0;
    private double _targetY = 0;
    private readonly Random _random = new();

    public CompanionWindow(string skin = "GreenParakeet")
    {
        InitializeComponent();
        CompanionPet.ApplySkin(skin);

        // Position on taskbar initially, to the left of the leader
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - 550;
        Top = workArea.Bottom - Height + 6;

        _followTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(40)
        };
        _followTimer.Tick += OnFollowTick;
        _followTimer.Start();
    }

    public void SetLeaderTarget(double leaderLeft, double leaderTop, PetState leaderState)
    {
        // Companion rests 160px to the left or right of the leader
        double offset = (leaderLeft > SystemParameters.WorkArea.Width / 2) ? -180 : 180;
        _targetX = leaderLeft + offset;
        _targetY = leaderTop;

        if (leaderState == PetState.Sleeping || leaderState == PetState.NightSleeping)
        {
            CompanionPet.SetState(PetState.Sleeping);
        }
        else if (leaderState == PetState.Dancing)
        {
            CompanionPet.SetState(PetState.Dancing);
            ShowLoveHearts();
        }
        else if (leaderState == PetState.Flying)
        {
            CompanionPet.SetState(PetState.Flying);
        }
        else if (CompanionPet.CurrentState == PetState.Flying && leaderState == PetState.Idle)
        {
            CompanionPet.SetState(PetState.Landing);
        }
    }

    private void OnFollowTick(object? sender, EventArgs e)
    {
        double dx = _targetX - Left;
        double dy = _targetY - Top;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        if (dist > 15)
        {
            Left += dx * 0.12;
            Top += dy * 0.12;
            CompanionPet.SetFacing(dx < 0);
        }
    }

    public void ShowLoveHearts()
    {
        LoveHearts.Opacity = 1;
        var anim = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(1.8)
        };
        LoveHearts.BeginAnimation(OpacityProperty, anim);
    }

    public void CloseCompanion()
    {
        _followTimer.Stop();
        Close();
    }
}
