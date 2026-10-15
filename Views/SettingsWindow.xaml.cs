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
            "GreenParakeet" => 1,
            "Cockatiel" => 2,
            "BlueMacaw" => 3,
            _ => 0 // ScarletMacaw
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
        ChkAutonomousFlight.IsChecked = _current.AutonomousFlight;
        ChkBirdcageMode.IsChecked = _current.BirdcageMode;
        ChkFollowCursor.IsChecked = _current.FollowCursor;
        ChkGhostMode.IsChecked = _current.GhostMode;
        ChkSoundEnabled.IsChecked = _current.SoundEnabled;
        ChkScreenBreak.IsChecked = _current.ScreenBreakEnabled;
        ChkHydration.IsChecked = _current.HydrationReminderEnabled;

        // Auto-Startup & Language
        ChkStartWithWindows.IsChecked = AutoStartupService.IsAutoStartupEnabled() || _current.StartWithWindows;
        CmbLanguage.SelectedIndex = _current.Language switch
        {
            "Bengali" => 1,
            "Hindi" => 2,
            "Spanish" => 3,
            "Bilingual" => 4,
            _ => 0
        };
        ApplyLanguage(_current.Language);
    }

    private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbLanguage == null || CmbLanguage.SelectedIndex < 0) return;
        string selectedLang = CmbLanguage.SelectedIndex switch
        {
            1 => "Bengali",
            2 => "Hindi",
            3 => "Spanish",
            4 => "Bilingual",
            _ => "English"
        };
        ApplyLanguage(selectedLang);
    }

    public void ApplyLanguage(string lang)
    {
        Title = LocalizationService.Get("Settings_Title", lang);
        if (TxtSettingsSubtitle != null) TxtSettingsSubtitle.Text = LocalizationService.Get("Settings_Subtitle", lang);

        if (TxtCard1Title != null) TxtCard1Title.Text = LocalizationService.Get("Settings_CardSkins", lang);
        if (TxtLabelSkin != null) TxtLabelSkin.Text = LocalizationService.Get("Settings_LabelSpecies", lang);
        if (TxtLabelAccessory != null) TxtLabelAccessory.Text = LocalizationService.Get("Settings_LabelAccessories", lang);
        if (TxtLabelPerchMode != null) TxtLabelPerchMode.Text = LocalizationService.Get("Settings_LabelPerchMode", lang);

        if (CmbSkin != null && CmbSkin.Items.Count >= 4)
        {
            ((ComboBoxItem)CmbSkin.Items[0]).Content = LocalizationService.Get("MenuSkin_Scarlet", lang);
            ((ComboBoxItem)CmbSkin.Items[1]).Content = LocalizationService.Get("MenuSkin_Parakeet", lang);
            ((ComboBoxItem)CmbSkin.Items[2]).Content = LocalizationService.Get("MenuSkin_Cockatiel", lang);
            ((ComboBoxItem)CmbSkin.Items[3]).Content = LocalizationService.Get("MenuSkin_BlueMacaw", lang);
        }

        if (CmbAccessory != null && CmbAccessory.Items.Count >= 7)
        {
            ((ComboBoxItem)CmbAccessory.Items[0]).Content = LocalizationService.Get("MenuAcc_None", lang);
            ((ComboBoxItem)CmbAccessory.Items[1]).Content = LocalizationService.Get("MenuAcc_Glasses", lang);
            ((ComboBoxItem)CmbAccessory.Items[2]).Content = LocalizationService.Get("MenuAcc_Sunglasses", lang);
            ((ComboBoxItem)CmbAccessory.Items[3]).Content = LocalizationService.Get("MenuAcc_Scarf", lang);
            ((ComboBoxItem)CmbAccessory.Items[4]).Content = LocalizationService.Get("MenuAcc_TopHat", lang);
            ((ComboBoxItem)CmbAccessory.Items[5]).Content = LocalizationService.Get("MenuAcc_PartyHat", lang);
            ((ComboBoxItem)CmbAccessory.Items[6]).Content = LocalizationService.Get("MenuAcc_Umbrella", lang);
        }

        if (CmbPerchMode != null && CmbPerchMode.Items.Count >= 2)
        {
            ((ComboBoxItem)CmbPerchMode.Items[0]).Content = LocalizationService.Get("PerchDesktop", lang);
            ((ComboBoxItem)CmbPerchMode.Items[1]).Content = LocalizationService.Get("PerchActiveWindow", lang);
        }

        if (TxtCard2Title != null) TxtCard2Title.Text = LocalizationService.Get("Settings_CardPomodoro", lang);
        if (TxtLabelPomodoroFocus != null) TxtLabelPomodoroFocus.Text = LocalizationService.Get("Settings_LabelFocus", lang);
        if (TxtLabelPomodoroBreak != null) TxtLabelPomodoroBreak.Text = LocalizationService.Get("Settings_LabelBreak", lang);
        if (ChkDanceToMusic != null) ChkDanceToMusic.Content = LocalizationService.Get("Settings_ChkDanceMusic", lang);
        if (ChkNightModeAuto != null) ChkNightModeAuto.Content = LocalizationService.Get("Settings_ChkNightMode", lang);

        if (TxtCard3Title != null) TxtCard3Title.Text = LocalizationService.Get("Settings_CardAI", lang);
        if (ChkVoiceMimic != null) ChkVoiceMimic.Content = LocalizationService.Get("Settings_ChkVoiceMimic", lang);
        if (TxtLabelGeminiKey != null) TxtLabelGeminiKey.Text = LocalizationService.Get("Settings_LabelGeminiKey", lang);

        if (TxtCard4Title != null) TxtCard4Title.Text = LocalizationService.Get("Settings_CardBehavior", lang);
        if (TxtLabelScale != null) TxtLabelScale.Text = LocalizationService.Get("Settings_LabelScale", lang);
        if (ChkAlwaysOnTop != null) ChkAlwaysOnTop.Content = LocalizationService.Get("Settings_ChkAlwaysOnTop", lang);
        if (ChkAllowWalking != null) ChkAllowWalking.Content = LocalizationService.Get("Settings_ChkAllowWalking", lang);
        if (ChkAutonomousFlight != null) ChkAutonomousFlight.Content = LocalizationService.Get("Settings_ChkAutonomousFlight", lang);
        if (ChkBirdcageMode != null) ChkBirdcageMode.Content = LocalizationService.Get("Settings_ChkBirdcage", lang);
        if (ChkFollowCursor != null) ChkFollowCursor.Content = LocalizationService.Get("Settings_ChkFollowCursor", lang);
        if (ChkGhostMode != null) ChkGhostMode.Content = LocalizationService.Get("Settings_ChkGhostMode", lang);

        if (TxtCard5Title != null) TxtCard5Title.Text = LocalizationService.Get("Settings_CardWellness", lang);
        if (ChkSoundEnabled != null) ChkSoundEnabled.Content = LocalizationService.Get("Settings_ChkSound", lang);
        if (ChkScreenBreak != null) ChkScreenBreak.Content = LocalizationService.Get("Settings_ChkScreenBreak", lang);
        if (ChkHydration != null) ChkHydration.Content = LocalizationService.Get("Settings_ChkHydration", lang);
        if (BtnDailyReport != null) BtnDailyReport.Content = LocalizationService.Get("Settings_BtnDailyReport", lang);
        if (BtnTestSound != null) BtnTestSound.Content = LocalizationService.Get("Settings_BtnTestSound", lang);

        if (TxtCard6Title != null) TxtCard6Title.Text = LocalizationService.Get("Settings_CardSystem", lang);
        if (TxtLabelLanguage != null) TxtLabelLanguage.Text = LocalizationService.Get("Settings_LabelLanguage", lang);
        if (ChkStartWithWindows != null) ChkStartWithWindows.Content = LocalizationService.Get("Settings_ChkStartup", lang);

        if (BtnFeedTreat != null) BtnFeedTreat.Content = LocalizationService.Get("Settings_BtnFeedTreat", lang);
        if (BtnCancel != null) BtnCancel.Content = LocalizationService.Get("Settings_BtnCancel", lang);
        if (BtnSave != null) BtnSave.Content = LocalizationService.Get("Settings_BtnSave", lang);
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
            1 => "GreenParakeet",
            2 => "Cockatiel",
            3 => "BlueMacaw",
            _ => "ScarletMacaw"
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
        _current.AutonomousFlight = ChkAutonomousFlight.IsChecked ?? true;
        _current.BirdcageMode = ChkBirdcageMode.IsChecked ?? false;
        _current.FollowCursor = ChkFollowCursor.IsChecked ?? false;
        _current.GhostMode = ChkGhostMode.IsChecked ?? false;
        _current.SoundEnabled = ChkSoundEnabled.IsChecked ?? true;
        SoundService.MasterMute = !_current.SoundEnabled;
        _current.ScreenBreakEnabled = ChkScreenBreak.IsChecked ?? true;
        _current.HydrationReminderEnabled = ChkHydration.IsChecked ?? true;

        // Auto-Startup & Language
        bool startWithWindows = ChkStartWithWindows.IsChecked ?? false;
        _current.StartWithWindows = startWithWindows;
        AutoStartupService.SetAutoStartup(startWithWindows);

        _current.Language = CmbLanguage.SelectedIndex switch
        {
            1 => "Bengali",
            2 => "Hindi",
            3 => "Spanish",
            4 => "Bilingual",
            _ => "English"
        };

        _settingsService.SaveSettings(_current);

        Close();
    }
}
