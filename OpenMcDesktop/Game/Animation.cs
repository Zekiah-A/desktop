namespace OpenMcDesktop.Game;

public class Animation
{
    public List<Keyframe> Keyframes;
    public int LengthMilliseconds;

    public Animation()
    {
        Keyframes = new List<Keyframe>();
    }

    public Animation AddKeyFrame(DateTimeOffset time, Dictionary<string, float> values, KeyframeEase ease)
    {
        Keyframes.Add(new Keyframe(time.Millisecond, values, ease));
        LengthMilliseconds = time.Millisecond > LengthMilliseconds ? time.Millisecond : LengthMilliseconds;
        return this;
    }

    public float GetAnimationState(DateTimeOffset currentTime, string componentName)
    {
        var time = (currentTime.Millisecond % LengthMilliseconds); // Time relative to animation
        // First figure out what two keyframes we are supposedly between
        var previousFrame = Keyframes[0];
        var nextFrame = Keyframes[1];
        for (var i = 0; i < Keyframes.Count; i++)
        {
            if (!Keyframes[i].Components.ContainsKey(componentName))
            {
                continue;
            }

            if (Keyframes[i].timeMilliseconds < time)
            {
                previousFrame = Keyframes[i];
            }
            else if (Keyframes[i].timeMilliseconds > time )
            {
                nextFrame = Keyframes[i];
                previousFrame = Keyframes[i - 1];
                break;
            }
        }

        // Now find out what the value should be at the current time given the ease function
        var frameTime = time - previousFrame.timeMilliseconds; // Time relative to previous keyframe
        var timeDiff = nextFrame.timeMilliseconds - previousFrame.timeMilliseconds;
        var valueDiff = nextFrame.Components[componentName] - previousFrame.Components[componentName];
        var previousValue = previousFrame.Components[componentName];

        switch (previousFrame.ease)
        {
            case KeyframeEase.Linear:
                return (frameTime / timeDiff) * valueDiff + previousValue;
            default:
                throw new NotImplementedException();
        }
    }
}