using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Parot.Helpers;
using Parot.Services;
using Point = System.Windows.Point;

namespace Parot.Views;

public partial class LaserPointerWindow : Window
{
    private readonly DispatcherTimer _trackTimer;
    public event Action<Point>? LaserPositionUpdated;

    public LaserPointerWindow()
    {
        InitializeComponent();

        _trackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _trackTimer.Tick += OnTrackTick;
        _trackTimer.Start();
    }

    private void OnTrackTick(object? sender, EventArgs e)
    {
        var mouse = WindowInteropHelperNative.GetCurrentMousePosition();
        Left = mouse.X - (Width / 2);
        Top = mouse.Y - (Height / 2);

        LaserPositionUpdated?.Invoke(new Point(mouse.X, mouse.Y));
    }

    public void AnimateCatch()
    {
        CatchSparkle.Opacity = 1;
        var fade = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(400)
        };
        CatchSparkle.BeginAnimation(OpacityProperty, fade);
    }

    protected override void OnClosed(EventArgs e)
    {
        _trackTimer.Stop();
        base.OnClosed(e);
    }
}
