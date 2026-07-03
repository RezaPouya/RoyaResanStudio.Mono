namespace RoyaResan.Mono2d.Graphics
{
    public class Camera2D
    {
        public Vector2 Position;

        public float Zoom = 1f; // reserved, NOT used yet

        public TransformNode FollowTarget;

        public void Update()
        {
            if (FollowTarget != null)
            {
                Position = FollowTarget.GlobalPosition;
            }
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return worldPosition - Position;
        }
    }
}