using System;
using System.Collections.Generic;
using Parot.Models;
using Parot.Services;
using Xunit;

namespace Parot.Tests;

public class StateTransitionTests
{
    [Fact]
    public void PetState_EnumValues_ContainEssentialStates()
    {
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Idle));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Blinking));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Hopping));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.TakeOff));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Flying));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Landing));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Perching));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.FollowCursor));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.Sleeping));
        Assert.True(Enum.IsDefined(typeof(PetState), PetState.WakeUp));
    }

    [Fact]
    public void SpriteAnimationClip_Chaining_ConfiguresExpectedSequence()
    {
        var takeOffClip = new SpriteAnimationClip
        {
            Name = "TakeOff",
            Fps = 10.0,
            IsLooping = false,
            NextClipName = "Flying"
        };

        var flyingClip = new SpriteAnimationClip
        {
            Name = "Flying",
            Fps = 12.0,
            IsLooping = true,
            NextClipName = null
        };

        var landingClip = new SpriteAnimationClip
        {
            Name = "Landing",
            Fps = 10.0,
            IsLooping = false,
            NextClipName = "Idle"
        };

        Assert.Equal("Flying", takeOffClip.NextClipName);
        Assert.False(takeOffClip.IsLooping);
        Assert.True(flyingClip.IsLooping);
        Assert.Null(flyingClip.NextClipName);
        Assert.Equal("Idle", landingClip.NextClipName);
        Assert.False(landingClip.IsLooping);
    }

    [Fact]
    public void PomodoroPhase_Transitions_FollowCycle()
    {
        var phase = PomodoroPhase.Stopped;
        Assert.Equal(PomodoroPhase.Stopped, phase);

        phase = PomodoroPhase.Focus;
        Assert.Equal(PomodoroPhase.Focus, phase);

        phase = PomodoroPhase.ShortBreak;
        Assert.Equal(PomodoroPhase.ShortBreak, phase);

        phase = PomodoroPhase.LongBreak;
        Assert.Equal(PomodoroPhase.LongBreak, phase);
    }
}
