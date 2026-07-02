using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Rendering;

public class Camera2D
{
    public Vector2 Position;
    public float Zoom = 1f;
    public float Rotation = 0f;

    public Matrix GetViewMatrix()
    {
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1f);
    }
}