using System.Runtime.CompilerServices;

namespace OpenMcDesktop;

public static class MathHelpers
{
    public const float PIf = (float) Math.PI;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RadToDeg(float angle)
    {
        return 180f / PIf * angle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float from, float to, float weight)
    {
        return from * (1 - weight) + to * weight;
    }
}