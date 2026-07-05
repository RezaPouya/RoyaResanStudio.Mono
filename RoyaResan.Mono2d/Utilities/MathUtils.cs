namespace RoyaResan.Mono2d.Utilities;

public static class MathUtils
{
    public static float Clamp(float value, float min, float max)
        => MathHelper.Clamp(value, min, max);

    public static float Lerp(float a, float b, float t)
        => a + (b - a) * t;

    public static float Distance(Vector2 a, Vector2 b)
        => Vector2.Distance(a, b);

    public static Vector2 Direction(Vector2 from, Vector2 to)
    {
        var dir = to - from;
        if (dir == Vector2.Zero)
            return Vector2.Zero;

        dir.Normalize();
        return dir;
    }
}