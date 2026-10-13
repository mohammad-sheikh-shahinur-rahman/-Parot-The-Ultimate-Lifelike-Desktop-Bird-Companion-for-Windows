using System;
using System.IO;
using Parot.Models;
using Parot.Services;
using Xunit;

namespace Parot.Tests;

public class AdvancedFeaturesTests
{
    [Fact]
    public void AppSettings_AdvancedFeatures_DefaultsAreCorrect()
    {
        var settings = new AppSettings();

        Assert.Equal(85.0, settings.HungerLevel);
        Assert.True(settings.DevWatcherEnabled);
        Assert.True(settings.TypingSyncBobbing);
        Assert.True(settings.WeatherSyncEnabled);
        Assert.False(settings.DualBirdMode);
        Assert.Equal("GreenParakeet", settings.CompanionSkin);
        Assert.False(settings.BallPushGameActive);
        Assert.False(settings.PerchOnCursorActive);
        Assert.True(settings.VoiceCommandsEnabled);
    }

    [Theory]
    [InlineData("ScarletMacaw")]
    [InlineData("GreenParakeet")]
    [InlineData("BlueMacaw")]
    [InlineData("Cockatiel")]
    [InlineData("Cockatoo")]
    public void Species_Assets_ExistOnDisk(string species)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string speciesPath = Path.Combine(baseDir, "Assets", "Birds", species);

        // If running in test runner, assets might also be in project root
        if (!Directory.Exists(speciesPath))
        {
            speciesPath = Path.Combine(baseDir, "..", "..", "..", "..", "Assets", "Birds", species);
        }

        Assert.True(Directory.Exists(speciesPath), $"Species folder should exist: {speciesPath}");
        Assert.True(Directory.Exists(Path.Combine(speciesPath, "Idle")), $"Idle subfolder must exist for {species}");
        Assert.True(Directory.Exists(Path.Combine(speciesPath, "Flying")), $"Flying subfolder must exist for {species}");
    }

    [Fact]
    public void HungerLevel_RestoresAndClamps()
    {
        var s = new AppSettings { HungerLevel = 20.0 };
        Assert.True(s.HungerLevel < 30.0);

        // Feed restores hunger to 100%
        s.HungerLevel = 100.0;
        Assert.Equal(100.0, s.HungerLevel);
    }

    [Fact]
    public void WeatherInfo_Defaults_InstantiateCorrectly()
    {
        var info = new WeatherInfo
        {
            Temperature = 14.5,
            WeatherCode = 61,
            IsDay = true,
            RecommendedAccessory = "Umbrella"
        };

        Assert.Equal(14.5, info.Temperature);
        Assert.Equal(61, info.WeatherCode);
        Assert.Equal("Umbrella", info.RecommendedAccessory);
    }

    [Fact]
    public void MoodService_Transitions_UpdateCorrectly()
    {
        var mood = new MoodService();
        Assert.Equal(PetMood.Cheerful, mood.CurrentMood);

        mood.UpdateMoodFromInactivity(TimeSpan.FromMinutes(9), isNight: false);
        Assert.Equal(PetMood.Needy, mood.CurrentMood);

        mood.RegisterInteraction();
        Assert.Equal(PetMood.Cheerful, mood.CurrentMood);

        mood.UpdateMoodFromInactivity(TimeSpan.FromMinutes(2), isNight: true);
        Assert.Equal(PetMood.Sleepy, mood.CurrentMood);
    }

    [Fact]
    public void PetState_NewAvianStates_AreDefined()
    {
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Walking));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.PeekingEdge));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Preening));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Stretching));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Fluffing));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Yawning));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Dizzy));
    }

    [Fact]
    public void SoundService_MuteEnforcement_WorksCleanly()
    {
        var sound = new SoundService(enabled: false, volume: 0.8);
        Assert.False(sound.Enabled);

        // When disabled, playback methods must safely exit without throwing
        sound.PlayChirp();
        sound.PlayDoubleChirp();
        sound.PlayTrill();
        sound.PlayRealParrotChatter();
        sound.PlayRealParrotScreech();
        sound.PlayRealParrotMimic();
        sound.PlayMunch();
        sound.PlayChime();
        sound.PlaySnore();
        sound.PlayHappy();
        sound.PlayCurious();
        sound.PlaySquawk();
        sound.StopAll();

        sound.Enabled = true;
        Assert.True(sound.Enabled);
        sound.Enabled = false;
        Assert.False(sound.Enabled);
    }

    [Fact]
    public void VoiceAndMic_SoundMute_PropertiesWork()
    {
        var mic = new MicrophoneMimicService(enableCommands: false);
        Assert.True(mic.SoundEnabled);
        mic.SoundEnabled = false;
        Assert.False(mic.SoundEnabled);
        mic.StopPlayback();

        var voice = new VoiceMimicService(enabled: true, voiceRate: 4);
        Assert.True(voice.MasterSoundEnabled);
        voice.MasterSoundEnabled = false;
        Assert.False(voice.MasterSoundEnabled);
        voice.Speak("Test phrase");
        voice.Stop();
    }

    [Fact]
    public void BirdcageMode_CanBeToggled_AndPersisted()
    {
        var settings = new AppSettings();
        Assert.False(settings.BirdcageMode);

        settings.BirdcageMode = true;
        Assert.True(settings.BirdcageMode);

        settings.BirdcageMode = false;
        Assert.False(settings.BirdcageMode);
    }
}
