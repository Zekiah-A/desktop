namespace OpenMcDesktop.Game;

public record struct Keyframe
(
    int TimeMilliseconds,
    Dictionary<string, float> Components,
    KeyframeEase Ease
);