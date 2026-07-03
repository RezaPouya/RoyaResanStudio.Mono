namespace RoyaResan.Mono2d.Animation
{
    public class AnimationPlayer
    {
        public AnimationClip Clip;

        private float _timer;
        private int _frameIndex;

        public bool Playing = true;

        public void Play(AnimationClip clip)
        {
            Clip = clip;
            _timer = 0;
            _frameIndex = 0;
            Playing = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!Playing || Clip == null || Clip.Frames.Count == 0)
                return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _timer += dt;

            if (_timer >= Clip.FrameTime)
            {
                _timer -= Clip.FrameTime;
                _frameIndex++;

                if (_frameIndex >= Clip.Frames.Count)
                {
                    if (Clip.Loop)
                        _frameIndex = 0;
                    else
                    {
                        _frameIndex = Clip.Frames.Count - 1;
                        Playing = false;
                    }
                }
            }
        }

        public Texture2D CurrentFrame()
        {
            if (Clip == null || Clip.Frames.Count == 0)
                return null;

            return Clip.Frames[_frameIndex];
        }
    }
}