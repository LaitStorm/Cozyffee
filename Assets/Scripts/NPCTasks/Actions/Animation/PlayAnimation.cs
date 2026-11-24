namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Plays the Animator state <see cref="StateName"/> once (via <see cref="NPCAnimator"/>), which then returns to
    /// the usual state on its own. By default it succeeds right away; with <see cref="WaitUntilFinished"/> it keeps
    /// running until that single cycle is over (or something else takes over the Animator).
    /// </summary>
    public class PlayAnimation : NPCAction
    {
        public SharedString StateName = new SharedString();

        public bool WaitUntilFinished = false;

        bool started;

        public PlayAnimation()
        {
        }

        public PlayAnimation(SharedString stateName)
        {
            StateName = stateName;
        }

        protected override void OnEntry()
        {
            started = false;
        }

        protected override TaskStatus OnUpdate()
        {
            if (!started)
            {
                if (npc == null || StateName == null || string.IsNullOrEmpty(StateName.Value))
                {
                    return TaskStatus.Failure;
                }

                npc.PlayAnimation?.Invoke(StateName.Value);
                started = true;

                return WaitUntilFinished ? TaskStatus.Running : TaskStatus.Success;
            }

            bool playing = npc.IsPlayingAnimation != null && npc.IsPlayingAnimation();
            return playing ? TaskStatus.Running : TaskStatus.Success;
        }
    }
}
