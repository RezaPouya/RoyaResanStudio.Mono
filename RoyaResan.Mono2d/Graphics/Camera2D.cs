namespace RoyaResan.Mono2d.Graphics
{
    public class Camera2D
    {
        public Vector2 Position;

        /// <summary>
        /// Half the viewport size - set this once from World.cs
        /// (GraphicsDevice.Viewport.Width/Height / 2) after the graphics
        /// device is ready. Without it, WorldToScreen maps the camera's
        /// world Position to screen (0,0) instead of screen-center, which
        /// makes a followed target visibly drift toward the top-left
        /// corner as FollowSmoothing catches up, instead of staying
        /// centered on screen like a normal platformer camera. Defaults
        /// to Vector2.Zero (the old, wrong-looking behavior) only so a
        /// forgotten assignment fails obviously instead of silently.
        /// </summary>
        public Vector2 ScreenCenter = Vector2.Zero;

        public float Zoom = 1f; // reserved, NOT used yet

        public TransformNode FollowTarget;

        /// <summary>
        /// How quickly the camera catches up to the target, in "lerp per
        /// second" terms. 0 = snap instantly (old behavior). Higher =
        /// snappier; lower = looser/floatier follow.
        /// </summary>
        public float FollowSmoothing = 8f;

        /// <summary>
        /// How far the camera biases ahead of the target's movement
        /// direction (e.g. shows more of the level in front of a running
        /// player). Only applies when FollowTarget is a PhysicsBody, since
        /// look-ahead needs a velocity to read. 0 disables it.
        /// </summary>
        public float LookAheadDistance = 0f;

        public float LookAheadSmoothing = 6f;

        private Vector2 _lookAheadOffset;

        private Vector2 _shakeOffset;
        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeMagnitude;
        private readonly Random _rng = new();

        /// <summary>
        /// Triggers a screen shake. If a shake is already in progress, the
        /// stronger of the two wins rather than the new one weakly
        /// overwriting an ongoing big shake.
        /// </summary>
        public void Shake(float duration, float magnitude)
        {
            if (_shakeTimer <= 0f || magnitude >= _shakeMagnitude)
            {
                _shakeMagnitude = magnitude;
                _shakeDuration = duration;
                _shakeTimer = duration;
            }
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateFollow(dt);
            UpdateShake(dt);
        }

        private void UpdateFollow(float dt)
        {
            if (FollowTarget == null)
                return;

            Vector2 targetPos = FollowTarget.GlobalPosition;

            Vector2 desiredLookAhead = Vector2.Zero;
            if (LookAheadDistance > 0f && FollowTarget is PhysicsBody body && body.Velocity.LengthSquared() > 0.01f)
            {
                Vector2 dir = body.Velocity;
                dir.Normalize();
                desiredLookAhead = dir * LookAheadDistance;
            }

            float lookAheadT = MathHelper.Clamp(LookAheadSmoothing * dt, 0f, 1f);
            _lookAheadOffset = Vector2.Lerp(_lookAheadOffset, desiredLookAhead, lookAheadT);

            targetPos += _lookAheadOffset;

            if (FollowSmoothing <= 0f)
            {
                Position = targetPos;
            }
            else
            {
                float followT = MathHelper.Clamp(FollowSmoothing * dt, 0f, 1f);
                Position = Vector2.Lerp(Position, targetPos, followT);
            }
        }

        private void UpdateShake(float dt)
        {
            if (_shakeTimer <= 0f)
            {
                _shakeOffset = Vector2.Zero;
                return;
            }

            _shakeTimer -= dt;
            float strength = _shakeDuration > 0f ? MathHelper.Clamp(_shakeTimer / _shakeDuration, 0f, 1f) : 0f;

            _shakeOffset = new Vector2(
                (float)(_rng.NextDouble() * 2.0 - 1.0),
                (float)(_rng.NextDouble() * 2.0 - 1.0)) * _shakeMagnitude * strength;
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return worldPosition - Position + ScreenCenter - _shakeOffset;
        }

        /// <summary>Inverse of WorldToScreen - needed to aim at the mouse (e.g. RopeController.TryFireAtMouse).</summary>
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return screenPosition - ScreenCenter + Position + _shakeOffset;
        }
    }
}