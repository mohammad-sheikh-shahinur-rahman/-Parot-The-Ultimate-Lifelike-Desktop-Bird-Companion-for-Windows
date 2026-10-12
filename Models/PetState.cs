namespace Parot.Models;

public enum PetState
{
    Idle,
    LookingLeft,
    LookingRight,
    CuriousHeadTilt,
    Blinking,
    FlappingWings,
    Hopping,
    Dragged,
    Sleeping,
    Happy,
    Eating,
    Speaking,
    Dancing,          // Feature 2: Beat sync headbang & dance
    PomodoroFocus,    // Feature 1: Study mode with glasses
    PomodoroBreak,    // Feature 1: Break celebration
    PlayingCatch,     // Feature 6: Flying to catch treat
    NightSleeping     // Feature 7: Night mode sleep with nightcap
}

public enum ReminderType
{
    ScreenBreak,
    Hydration,
    Posture,
    IdleThought,
    PomodoroAlert
}

public enum PomodoroPhase
{
    Stopped,
    Focus,
    ShortBreak,
    LongBreak
}
