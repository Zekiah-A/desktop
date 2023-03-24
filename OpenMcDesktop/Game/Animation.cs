namespace OpenMcDesktop.Game;

public class Animation
{
    public List<Keyframe> Keyframes;
    public int LengthMilliseconds;
    private DateTimeOffset animationStart;

    public Animation()
    {
        Keyframes = new List<Keyframe>();
    }

    public Animation AddKeyFrame(int timeMilliseconds, Dictionary<string, float> values, KeyframeEase ease)
    {
        Keyframes.Add(new Keyframe(timeMilliseconds, values, ease));
        LengthMilliseconds = timeMilliseconds > LengthMilliseconds ? timeMilliseconds : LengthMilliseconds;
        return this;
    }

    public void Start()
    {
        animationStart = DateTime.Now;
    }

    public float GetAnimationState(DateTimeOffset currentTime, string componentName)
    {
        var time = (currentTime - animationStart).Milliseconds % LengthMilliseconds; // Time relative to animation
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
            else if (Keyframes[i].timeMilliseconds > time)
            {
                nextFrame = Keyframes[i];
                previousFrame = Keyframes[i - 1];
                break;
            }
        }

        // Now find out what the value should be at the current time given the ease function
        var frameTime = time - previousFrame.timeMilliseconds; // Time relative to previous keyframe
        var timeDiff = nextFrame.timeMilliseconds - previousFrame.timeMilliseconds;
        var timeProgress = frameTime / (float) timeDiff;
        var previousValue = previousFrame.Components[componentName];
        var valueDiff = nextFrame.Components[componentName] - previousValue;

        switch (previousFrame.ease)
        {
            case KeyframeEase.Linear:
                return timeProgress * valueDiff + previousValue;
            case KeyframeEase.SineIn:
                return (float) (1 - Math.Cos(timeProgress * Math.PI / 2)) * valueDiff + previousValue;
            case KeyframeEase.SineInOut:
                return (float) (-(Math.Cos(Math.PI * timeProgress) - 1) / 2) * valueDiff + previousValue;
            case KeyframeEase.SineOut:
                return (float) Math.Sin(timeProgress * Math.PI / 2) * valueDiff + previousValue;
            case KeyframeEase.BounceZeroIn:
                var a = Math.Pow(2, Math.Floor(Math.Log2(1 - timeProgress)));
                var b = timeProgress / a % 1;
                return (float) ((float)((float) 2 * (-4 * Math.Pow(b - 0.5, 2) + 1) * valueDiff + previousValue) * a);
            default:
                throw new NotImplementedException();
        }
    }
}