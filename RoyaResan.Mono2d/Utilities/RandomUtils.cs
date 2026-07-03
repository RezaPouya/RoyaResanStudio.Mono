namespace RoyaResan.Mono2d.Utilities;

public static class RandomUtils
{
    private static readonly Random _random = new();

    public static float Range(float min, float max)
        => (float)(_random.NextDouble() * (max - min) + min);

    public static int Range(int min, int max)
        => _random.Next(min, max);

    public static bool Chance(float percent)
        => _random.NextDouble() < percent;
}