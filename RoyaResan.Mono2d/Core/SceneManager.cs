namespace RoyaResan.Mono2d.Core;

public class SceneManager
{
    public Scene Current;

    public void Change(Scene scene) => Current = scene;
}