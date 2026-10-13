namespace Parot.Models;

public enum PetState
{
    Idle,
    Blinking,
    LookingLeft,
    LookingRight,
    CuriousHeadTilt,
    Hopping,
    TakeOff,
    Flying,
    Landing,
    Perching,
    FollowCursor,
    FlappingWings,
    Dragged,
    Sleeping,
    NightSleeping,
    WakeUp,
    Happy,
    Eating,
    Speaking,
    Dancing,
    PomodoroFocus,
    PomodoroBreak,
    PlayingCatch,
    Walking,
    PeekingEdge,
    Preening,
    Stretching,
    Fluffing,
    Yawning,
    BeakWiping,
    HeadScratching,
    TailWagging,
    Dizzy
}

public enum PetMood
{
    Cheerful,
    Curious,
    Needy,
    Sleepy
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
