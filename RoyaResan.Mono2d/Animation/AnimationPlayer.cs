namespace RoyaResan.Mono2d.Animation
{
    /// <summary>
    /// Plays a single AnimationClip at a given speed. Not state-aware -
    /// see Animator for state machine + blending on top of this.
    /// </summary>
    public class AnimationPlayer
    {
        public AnimationClip Clip;

        /// <summary>Playback speed multiplier (1 = normal). Used for
        /// environment/situation-based speed changes.</summary>
        public float Speed = 1f;

        public bool Playing = true;

        private float _timer;
        private int _frameIndex;

        public void Play(AnimationClip clip, float speed = 1f)
        {
            Clip = clip;
            Speed = speed;
            _timer = 0f;
            _frameIndex = 0;
            Playing = true;

            if (Clip?.FrameSounds != null && Clip.FrameSounds.TryGetValue(0, out var startSound))
                AudioManager.PlaySfx(startSound);
        }

        public void Update(GameTime gameTime)
        {
            if (!Playing || Clip == null || Clip.Frames.Count == 0)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds * Speed;
            _timer += dt;

            // while, not if - so a high speed multiplier at low framerate
            // can still advance multiple frames in one update instead of
            // getting stuck a frame behind.
            while (Clip.FrameTime > 0f && _timer >= Clip.FrameTime)
            {
                _timer -= Clip.FrameTime;
                _frameIndex++;

                if (_frameIndex >= Clip.Frames.Count)
                {
                    if (Clip.Loop)
                    {
                        _frameIndex = 0;
                    }
                    else
                    {
                        _frameIndex = Clip.Frames.Count - 1;
                        Playing = false;
                        break;
                    }
                }

                if (Clip.FrameSounds != null && Clip.FrameSounds.TryGetValue(_frameIndex, out var frameSound))
                    AudioManager.PlaySfx(frameSound);
            }
        }

        public Texture2D Texture => Clip?.SpriteSheet;

        public Rectangle? CurrentRect =>
            (Clip != null && Clip.Frames.Count > 0) ? Clip.Frames[_frameIndex] : (Rectangle?)null;
    }
}