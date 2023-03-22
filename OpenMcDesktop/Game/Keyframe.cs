public record struct Keyframe
(
    int timeMilliseconds,
    Dictionary<string, float> Components,
    KeyframeEase ease
);