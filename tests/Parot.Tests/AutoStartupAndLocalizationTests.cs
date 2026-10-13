using System;
using Parot.Services;
using Xunit;

namespace Parot.Tests;

public class AutoStartupAndLocalizationTests
{
    [Theory]
    [InlineData("Bengali", "Bengali")]
    [InlineData("বাংলা", "Bengali")]
    [InlineData("English", "English")]
    [InlineData("ইংরেজি", "English")]
    [InlineData("Hindi", "Hindi")]
    [InlineData("Spanish", "Spanish")]
    [InlineData("Bilingual", "Bilingual")]
    [InlineData("Unknown", "Bilingual")]
    [InlineData(null, "Bilingual")]
    public void Localization_NormalizeLanguage_ReturnsExpected(string? input, string expected)
    {
        var result = LocalizationService.NormalizeLanguage(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Bengali", "🇧🇩")]
    [InlineData("English", "🇬🇧")]
    [InlineData("Hindi", "🇮🇳")]
    [InlineData("Spanish", "🇪🇸")]
    [InlineData("Bilingual", "🌐")]
    public void Localization_GetLanguageDisplayName_ContainsFlag(string lang, string flag)
    {
        var displayName = LocalizationService.GetLanguageDisplayName(lang);
        Assert.Contains(flag, displayName);
    }

    [Fact]
    public void Localization_Get_SoundToggles_ReturnValidStrings()
    {
        foreach (var lang in LocalizationService.SupportedLanguages)
        {
            var soundOn = LocalizationService.Get("SoundOn", lang);
            var soundOff = LocalizationService.Get("SoundOff", lang);

            Assert.False(string.IsNullOrWhiteSpace(soundOn));
            Assert.False(string.IsNullOrWhiteSpace(soundOff));
            Assert.NotEqual("SoundOn", soundOn);
            Assert.NotEqual("SoundOff", soundOff);
        }
    }

    [Fact]
    public void Localization_Get_AutoStartup_ReturnValidStrings()
    {
        foreach (var lang in LocalizationService.SupportedLanguages)
        {
            var startup = LocalizationService.Get("AutoStartup", lang);
            Assert.False(string.IsNullOrWhiteSpace(startup));
            Assert.NotEqual("AutoStartup", startup);
        }
    }

    [Fact]
    public void Localization_Get_Behaviors_ReturnValidStrings()
    {
        string[] keys = ["Preen", "Stretch", "Fluff", "Yawn", "Observing", "HappyDance"];
        foreach (var key in keys)
        {
            var msg = LocalizationService.Get(key, "English");
            Assert.False(string.IsNullOrWhiteSpace(msg));
            Assert.NotEqual(key, msg);
        }
    }

    [Fact]
    public void AutoStartupService_GetExecutablePath_ReturnsValidPath()
    {
        var path = AutoStartupService.GetExecutablePath();
        Assert.False(string.IsNullOrWhiteSpace(path));
    }

    [Fact]
    public void SoundService_MasterMute_MutesSoundGlobally()
    {
        SoundService.MasterMute = true;
        Assert.True(SoundService.MasterMute);

        var soundService = new SoundService(true, 0.8);
        Assert.False(soundService.Enabled); // Globally muted!

        // Reset
        SoundService.MasterMute = false;
        Assert.False(SoundService.MasterMute);
        Assert.True(soundService.Enabled);
    }
}
