using System;
using System.Windows;
using System.Windows.Controls;
using Parot.Models;
using Parot.Services;

namespace Parot.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly SoundService _soundService;
    private readonly AppSettings _current;

    public event Action? FeedTreatRequested;
    public event Action? TestReminderRequested;

    public SettingsWindow(SettingsService settingsService, SoundService soundService)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _soundService = soundService;
        _current = _settingsService.CurrentSettings;

        LoadValues();
    }

    private void LoadValues()
    {
        SliderScale.Value = _current.Scale;
        TxtScaleVal.Text = $"{(int)(_current.Scale * 100)}%";

        ChkAlwaysOnTop.IsChecked = _current.AlwaysOnTop;
        ChkAllowWalking.IsChecked = _current.AllowWalking;
        ChkGhostMode.IsChecked = _current.GhostMode;

        // Auto sleep
        CmbAutoSleep.SelectedIndex = _current.AutoSleepMinutes switch
        {
            2 => 0,
            3 => 1,
            5 => 2,
            10 => 3,
            0 => 4,
            _ => 1
        };

        // Sound
        ChkSoundEnabled.IsChecked = _current.SoundEnabled;
        SliderVolume.Value = _current.SoundVolume * 100;
        TxtVolumeVal.Text = $"{(int)(_current.SoundVolume * 100)}%";

        // Break
        ChkScreenBreak.IsChecked = _current.ScreenBreakEnabled;
        CmbScreenBreakInterval.SelectedIndex = _current.ScreenBreakIntervalMinutes switch
        {
            15 => 0,
            20 => 1,
            25 => 2,
            30 => 3,
            45 => 4,
            60 => 5,
            _ => 2
        };

        // Hydration
        ChkHydration.IsChecked = _current.HydrationReminderEnabled;
        CmbHydrationInterval.SelectedIndex = _current.HydrationIntervalMinutes switch
        {
            30 => 0,
            45 => 1,
            60 => 2,
            90 => 3,
            _ => 1
        };

        // Posture
        ChkPosture.IsChecked = _current.PostureReminderEnabled;
        CmbPostureInterval.SelectedIndex = _current.PostureIntervalMinutes switch
        {
            45 => 0,
            60 => 1,
            90 => 2,
            _ => 1
        };

        // Language
        CmbLanguage.SelectedIndex = _current.Language switch
        {
            "Bilingual" => 0,
            "Bengali" => 1,
            "English" => 2,
            _ => 0
        };

        ChkStartWithWindows.IsChecked = AutostartService.IsAutostartEnabled();
    }

    private void SliderScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtScaleVal != null)
        {
            TxtScaleVal.Text = $"{(int)(e.NewValue * 100)}%";
        }
    }

    private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtVolumeVal != null)
        {
            TxtVolumeVal.Text = $"{(int)e.NewValue}%";
            _soundService.Volume = e.NewValue / 100.0;
        }
    }

    private void BtnTestSound_Click(object sender, RoutedEventArgs e)
    {
        _soundService.PlayDoubleChirp();
    }

    private void BtnTestReminder_Click(object sender, RoutedEventArgs e)
    {
        TestReminderRequested?.Invoke();
    }

    private void BtnFeedTreat_Click(object sender, RoutedEventArgs e)
    {
        FeedTreatRequested?.Invoke();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _current.Scale = SliderScale.Value;
        _current.AlwaysOnTop = ChkAlwaysOnTop.IsChecked ?? true;
        _current.AllowWalking = ChkAllowWalking.IsChecked ?? true;
        _current.GhostMode = ChkGhostMode.IsChecked ?? false;

        _current.AutoSleepMinutes = CmbAutoSleep.SelectedIndex switch
        {
            0 => 2,
            1 => 3,
            2 => 5,
            3 => 10,
            4 => 0,
            _ => 3
        };

        _current.SoundEnabled = ChkSoundEnabled.IsChecked ?? true;
        _current.SoundVolume = SliderVolume.Value / 100.0;
        _soundService.Enabled = _current.SoundEnabled;
        _soundService.Volume = _current.SoundVolume;

        _current.ScreenBreakEnabled = ChkScreenBreak.IsChecked ?? true;
        _current.ScreenBreakIntervalMinutes = CmbScreenBreakInterval.SelectedIndex switch
        {
            0 => 15,
            1 => 20,
            2 => 25,
            3 => 30,
            4 => 45,
            5 => 60,
            _ => 25
        };

        _current.HydrationReminderEnabled = ChkHydration.IsChecked ?? true;
        _current.HydrationIntervalMinutes = CmbHydrationInterval.SelectedIndex switch
        {
            0 => 30,
            1 => 45,
            2 => 60,
            3 => 90,
            _ => 45
        };

        _current.PostureReminderEnabled = ChkPosture.IsChecked ?? true;
        _current.PostureIntervalMinutes = CmbPostureInterval.SelectedIndex switch
        {
            0 => 45,
            1 => 60,
            2 => 90,
            _ => 60
        };

        _current.Language = CmbLanguage.SelectedIndex switch
        {
            0 => "Bilingual",
            1 => "Bengali",
            2 => "English",
            _ => "Bilingual"
        };

        bool runOnStartup = ChkStartWithWindows.IsChecked ?? false;
        _current.StartWithWindows = runOnStartup;
        AutostartService.SetAutostart(runOnStartup);

        _settingsService.SaveSettings(_current);

        Close();
    }
}
