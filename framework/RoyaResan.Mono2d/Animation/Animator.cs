namespace RoyaResan.Mono2d.Animation
{
    public class Animator
    {
        public AnimationPlayer Player = new AnimationPlayer();

        public Texture2D GetFrame()
        {
            return Player.CurrentFrame();
        }

        public void Update(GameTime gameTime)
        {
            Player.Update(gameTime);
        }

        public void Play(AnimationClip clip)
        {
            Player.Play(clip);
        }
    }
}