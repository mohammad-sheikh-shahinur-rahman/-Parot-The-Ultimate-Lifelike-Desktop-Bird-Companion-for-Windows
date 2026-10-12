using System;
using System.Windows;
using Parot.Models;
using Parot.Services;

namespace Parot.Views;

public partial class DailyReportWindow : Window
{
    public DailyReportWindow(StatisticsService statsService, AppSettings settings)
    {
        InitializeComponent();

        var stats = statsService.TodayStats;
        TxtDate.Text = DateTime.Today.ToString("dd MMMM, yyyy (dddd)");

        // Friendship
        TxtFriendshipLevel.Text = $"Friendship Level {settings.FriendshipLevel} (Best Buddy)";
        TxtTotalXP.Text = $"Total XP: {settings.FriendshipXP} XP (+{stats.XPEarnedToday} earned today)";

        // Active Time
        int hours = stats.ActiveMinutes / 60;
        int mins = stats.ActiveMinutes % 60;
        TxtActiveTime.Text = hours > 0 ? $"{hours}h {mins}m" : $"{mins}m";

        TxtEyeBreaks.Text = $"{stats.EyeBreaksCompleted} বার";
        TxtWaterGlasses.Text = $"{stats.WaterGlassesLogged} গ্লাস";
        TxtPomodoroCount.Text = $"{stats.PomodoroCyclesCompleted} সেশন";

        if (stats.EyeBreaksCompleted >= 5 && stats.WaterGlassesLogged >= 4)
        {
            TxtEncouragement.Text = "🌟 আপনি একজন চ্যাম্পিয়ন! কাজ এবং সুস্বাস্থ্যের চমৎকার ভারসাম্য বজায় রাখছেন!";
        }
        else
        {
            TxtEncouragement.Text = "🦜 মনে রাখবেন: কাজের ফাঁকে সামান্য বিশ্রাম ও পর্যাপ্ত পানি আপনার উৎপাদনশীলতা বহুগুণ বাড়িয়ে দেয়!";
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
