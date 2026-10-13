using System;
using Parot.Models;
using Xunit;

namespace Parot.Tests;

public class DialogueTests
{
    [Theory]
    [InlineData("Bengali")]
    [InlineData("English")]
    [InlineData("Bilingual")]
    public void GetEyeBreakDialogue_ReturnsNonEmptyMessage(string language)
    {
        var msg = Dialogues.GetEyeBreakDialogue(language);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Theory]
    [InlineData("Bengali")]
    [InlineData("English")]
    [InlineData("Bilingual")]
    public void GetHydrationDialogue_ReturnsNonEmptyMessage(string language)
    {
        var msg = Dialogues.GetHydrationDialogue(language);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Theory]
    [InlineData("Bengali")]
    [InlineData("English")]
    [InlineData("Bilingual")]
    public void GetPostureDialogue_ReturnsNonEmptyMessage(string language)
    {
        var msg = Dialogues.GetPostureDialogue(language);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Theory]
    [InlineData("Bengali")]
    [InlineData("English")]
    [InlineData("Bilingual")]
    public void GetIdleDialogue_ReturnsNonEmptyMessage(string language)
    {
        var msg = Dialogues.GetIdleDialogue(language);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Theory]
    [InlineData("Bengali")]
    [InlineData("English")]
    [InlineData("Bilingual")]
    public void GetTreatDialogue_ReturnsNonEmptyMessage(string language)
    {
        var msg = Dialogues.GetTreatDialogue(language);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Fact]
    public void BengaliDialogue_ContainsBengaliCharacters()
    {
        var msg = Dialogues.GetEyeBreakDialogue("Bengali");
        // Verify Bengali unicode range (\u0980 - \u09FF)
        bool hasBengaliChar = false;
        foreach (char c in msg)
        {
            if (c >= '\u0980' && c <= '\u09FF')
            {
                hasBengaliChar = true;
                break;
            }
        }
        Assert.True(hasBengaliChar, "Bengali dialogue should contain Bengali Unicode characters");
    }

    [Fact]
    public void EnglishDialogue_ContainsAsciiCharacters()
    {
        var msg = Dialogues.GetEyeBreakDialogue("English");
        bool hasEnglishChar = false;
        bool hasBengaliChar = false;
        foreach (char c in msg)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                hasEnglishChar = true;
            if (c >= '\u0980' && c <= '\u09FF')
                hasBengaliChar = true;
        }
        Assert.True(hasEnglishChar, "English dialogue should contain English alphabet characters");
        Assert.False(hasBengaliChar, "English dialogue should not contain Bengali characters");
    }
}
