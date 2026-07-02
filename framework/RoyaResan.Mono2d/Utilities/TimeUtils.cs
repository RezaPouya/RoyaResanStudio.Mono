namespace RoyaResan.Mono2d.Utilities;

public static class TimeUtils
{
    public static float ToSeconds(double milliseconds)
        => (float)(milliseconds / 1000.0);
}