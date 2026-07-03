namespace RoyaResan.Mono2d.Animation
{
    /// <summary>
    /// Drives a sprite through named animation states with transitions and
    /// crossfade blending (e.g. idle -> attack, run -> parry). Frames are
    /// sub-rectangles into a shared spritesheet - no per-frame textures.
    ///
    /// Blending is done as a 2-layer alpha crossfade: the outgoing state's
    /// current frame fades out under the incoming state's frame fading in.
    /// This is a simple, standard approach for sprite-sheet animation and
    /// does not require any extra art beyond the existing frames.
    /// </summary>
    public class Animator
    {
        private readonly Dictionary<string, AnimationState> _states = new();
        private readonly List<AnimationTransition> _transitions = new();

        private AnimationPlayer _current = new();
        private AnimationPlayer _previous = new();
        private float _blendTimer;
        private float _blendDuration;
        private bool _blending;

        public string CurrentStateName { get; private set; }

        /// <summary>
        /// Global speed scale applied on top of each state's own base
        /// Speed. Set this from gameplay/environment code (e.g. 0.5 while
        /// wading through water, 1.3 on an icy slope) without touching
        /// individual clips or states.
        /// </summary>
        public float SpeedMultiplier = 1f;

        public void AddState(AnimationState state) => _states[state.Name] = state;

        public void AddTransition(AnimationTransition transition) => _transitions.Add(transition);

        /// <summary>
        /// Switch to a state immediately. If blendDuration > 0 and a state
        /// is already playing, crossfades from it instead of cutting.
        /// </summary>
        public void Play(string stateName, float blendDuration = 0f)
        {
            if (!_states.TryGetValue(stateName, out var state))
                return;

            if (stateName == CurrentStateName && !_blending)
                return;

            if (blendDuration > 0f && CurrentStateName != null)
            {
                _previous = _current;
                _blendDuration = blendDuration;
                _blendTimer = 0f;
                _blending = true;
            }
            else
            {
                _blending = false;
            }

            _current = new AnimationPlayer();
            _current.Play(state.Clip, state.Speed * SpeedMultiplier);
            CurrentStateName = stateName;
        }

        public void Update(GameTime gameTime)
        {
            // Check transitions - first matching rule wins.
            for (int i = 0; i < _transitions.Count; i++)
            {
                var t = _transitions[i];
                bool fromMatches = t.From == null || t.From == "*" || t.From == CurrentStateName;

                if (fromMatches && t.To != CurrentStateName && (t.Condition?.Invoke() ?? false))
                {
                    Play(t.To, t.BlendDuration);
                    break;
                }
            }

            // Re-apply speed multiplier every frame so environment changes
            // (e.g. entering water mid-animation) take effect immediately,
            // not just at the moment a state starts playing.
            if (CurrentStateName != null && _states.TryGetValue(CurrentStateName, out var currentState))
                _current.Speed = currentState.Speed * SpeedMultiplier;

            _current.Update(gameTime);

            if (_blending)
            {
                _previous.Update(gameTime);
                _blendTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_blendTimer >= _blendDuration)
                    _blending = false;
            }
        }

        public bool IsBlending => _blending;

        /// <summary>0 at blend start (previous fully visible) to 1 at blend end (current fully visible).</summary>
        public float BlendWeight => _blending && _blendDuration > 0f
            ? MathHelper.Clamp(_blendTimer / _blendDuration, 0f, 1f)
            : 1f;

        public (Texture2D texture, Rectangle? rect) CurrentFrame => (_current.Texture, _current.CurrentRect);

        /// <summary>Only meaningful while IsBlending is true.</summary>
        public (Texture2D texture, Rectangle? rect) PreviousFrame => (_previous.Texture, _previous.CurrentRect);
    }
}
