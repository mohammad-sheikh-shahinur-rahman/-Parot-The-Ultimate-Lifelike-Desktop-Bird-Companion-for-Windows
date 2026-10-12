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
    private readonly StatisticsService? _statsService;
    private readonly AppSettings _current;

    public event Action? FeedTreatRequested;
    public event Action? OpenDailyReportRequested;

    public SettingsWindow(SettingsService settingsService, SoundService soundService, StatisticsService? statsService = null)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _soundService = soundService;
        _statsService = statsService;
        _current = _settingsService.CurrentSettings;

        LoadValues();
    }

    private void LoadValues()
    {
        // Skins
        CmbSkin.SelectedIndex = _current.Skin switch
        {
            "Cockatiel" => 1,
            "Macaw" => 2,
            _ => 0
        };

        // Accessories
        CmbAccessory.SelectedIndex = _current.Accessory switch
        {
            "Glasses" => 1,
            "Sunglasses" => 2,
            "Scarf" => 3,
            "TopHat" => 4,
            "PartyHat" => 5,
            "Umbrella" => 6,
            _ => 0
        };

        // Perch Mode
        CmbPerchMode.SelectedIndex = _current.PerchMode == "ActiveWindow" ? 1 : 0;

        // Pomodoro Focus
        CmbPomodoroFocus.SelectedIndex = _current.PomodoroFocusMinutes switch
        {
            20 => 0,
            25 => 1,
            30 => 2,
            45 => 3,
            50 => 4,
            _ => 1
        };

        // Pomodoro Break
        CmbPomodoroBreak.SelectedIndex = _current.PomodoroBreakMinutes switch
        {
            3 => 0,
            5 => 1,
            10 => 2,
            _ => 1
        };

        ChkDanceToMusic.IsChecked = _current.DanceToMusic;
        ChkNightModeAuto.IsChecked = _current.NightModeAuto;
        ChkVoiceMimic.IsChecked = _current.VoiceMimicEnabled;
        TxtGeminiKey.Text = _current.GeminiApiKey;

        // General
        SliderScale.Value = _current.Scale;
        TxtScaleVal.Text = $"{(int)(_current.Scale * 100)}%";
        ChkAlwaysOnTop.IsChecked = _current.AlwaysOnTop;
        ChkAllowWalking.IsChecked = _current.AllowWalking;
        ChkGhostMode.IsChecked = _current.GhostMode;
        ChkSoundEnabled.IsChecked = _current.SoundEnabled;
        ChkScreenBreak.IsChecked = _current.ScreenBreakEnabled;
        ChkHydration.IsChecked = _current.HydrationReminderEnabled;
    }

    private void SliderScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (TxtScaleVal != null)
        {
            TxtScaleVal.Text = $"{(int)(e.NewValue * 100)}%";
        }
    }

    private void BtnTestSound_Click(object sender, RoutedEventArgs e)
    {
        _soundService.PlayDoubleChirp();
    }

    private void BtnDailyReport_Click(object sender, RoutedEventArgs e)
    {
        OpenDailyReportRequested?.Invoke();
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
        // Skins
        _current.Skin = CmbSkin.SelectedIndex switch
        {
            1 => "Cockatiel",
            2 => "Macaw",
            _ => "GreenParakeet"
        };

        // Accessories
        _current.Accessory = CmbAccessory.SelectedIndex switch
        {
            1 => "Glasses",
            2 => "Sunglasses",
            3 => "Scarf",
            4 => "TopHat",
            5 => "PartyHat",
            6 => "Umbrella",
            _ => "None"
        };

        // Perch Mode
        _current.PerchMode = CmbPerchMode.SelectedIndex == 1 ? "ActiveWindow" : "Desktop";

        // Pomodoro
        _current.PomodoroFocusMinutes = CmbPomodoroFocus.SelectedIndex switch
        {
            0 => 20,
            1 => 25,
            2 => 30,
            3 => 45,
            4 => 50,
            _ => 25
        };

        _current.PomodoroBreakMinutes = CmbPomodoroBreak.SelectedIndex switch
        {
            0 => 3,
            1 => 5,
            2 => 10,
            _ => 5
        };

        _current.DanceToMusic = ChkDanceToMusic.IsChecked ?? true;
        _current.NightModeAuto = ChkNightModeAuto.IsChecked ?? true;
        _current.VoiceMimicEnabled = ChkVoiceMimic.IsChecked ?? true;
        _current.GeminiApiKey = TxtGeminiKey.Text?.Trim() ?? "";

        // General
        _current.Scale = SliderScale.Value;
        _current.AlwaysOnTop = ChkAlwaysOnTop.IsChecked ?? true;
        _current.AllowWalking = ChkAllowWalking.IsChecked ?? true;
        _current.GhostMode = ChkGhostMode.IsChecked ?? false;
        _current.SoundEnabled = ChkSoundEnabled.IsChecked ?? true;
        _current.ScreenBreakEnabled = ChkScreenBreak.IsChecked ?? true;
        _current.HydrationReminderEnabled = ChkHydration.IsChecked ?? true;

        _settingsService.SaveSettings(_current);

        Close();
    }
}
