using System;
using System.IO;
using System.Text.Json;
using Parot.Models;
using Parot.Services;
using Xunit;

namespace Parot.Tests;

public class SettingsPersistenceTests
{
    [Fact]
    public void AppSettings_Defaults_AreCorrect()
    {
        var settings = new AppSettings();

        Assert.Equal(1.0, settings.Scale);
        Assert.True(settings.AlwaysOnTop);
        Assert.False(settings.GhostMode);
        Assert.Equal("ScarletMacaw", settings.Skin);
        Assert.Equal("None", settings.Accessory);
        Assert.True(settings.AutonomousFlight);
        Assert.False(settings.FollowCursor);
        Assert.True(settings.SoundEnabled);
        Assert.Equal(0.8, settings.SoundVolume);
        Assert.Equal(25, settings.ScreenBreakIntervalMinutes);
        Assert.Equal(45, settings.HydrationIntervalMinutes);
        Assert.Equal(60, settings.PostureIntervalMinutes);
        Assert.False(settings.BirdcageMode);
        Assert.Equal(150, settings.FriendshipXP);
        Assert.Equal(2, settings.FriendshipLevel);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(50, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(250, 3)]
    [InlineData(599, 6)]
    public void FriendshipLevel_CalculatesCorrectly(int xp, int expectedLevel)
    {
        var settings = new AppSettings { FriendshipXP = xp };
        Assert.Equal(expectedLevel, settings.FriendshipLevel);
    }

    [Fact]
    public void AppSettings_JsonSerialization_RoundtripSucceeds()
    {
        var original = new AppSettings
        {
            Scale = 1.25,
            AlwaysOnTop = false,
            GhostMode = true,
            Skin = "GreenParakeet",
            Accessory = "Sunglasses",
            AutonomousFlight = false,
            BirdcageMode = true,
            FollowCursor = true,
            SoundVolume = 0.5,
            ScreenBreakIntervalMinutes = 30,
            GeminiApiKey = "test-api-key-12345"
        };

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions { WriteIndented = true });
        var deserialized = JsonSerializer.Deserialize<AppSettings>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Scale, deserialized.Scale);
        Assert.Equal(original.AlwaysOnTop, deserialized.AlwaysOnTop);
        Assert.Equal(original.GhostMode, deserialized.GhostMode);
        Assert.Equal(original.Skin, deserialized.Skin);
        Assert.Equal(original.Accessory, deserialized.Accessory);
        Assert.Equal(original.AutonomousFlight, deserialized.AutonomousFlight);
        Assert.Equal(original.BirdcageMode, deserialized.BirdcageMode);
        Assert.Equal(original.FollowCursor, deserialized.FollowCursor);
        Assert.Equal(original.SoundVolume, deserialized.SoundVolume);
        Assert.Equal(original.ScreenBreakIntervalMinutes, deserialized.ScreenBreakIntervalMinutes);
        Assert.Equal(original.GeminiApiKey, deserialized.GeminiApiKey);
    }
}
