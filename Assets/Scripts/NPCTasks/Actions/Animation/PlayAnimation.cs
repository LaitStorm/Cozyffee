namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Plays the Animator state <see cref="StateName"/> once (via <see cref="NPCAnimator"/>), which then returns to
    /// the usual state on its own. Succeeds right away; it doesn't wait for the animation to finish.
    /// </summary>
    public class PlayAnimation : NPCAction
    {
        public SharedString StateName = new SharedString();

        public PlayAnimation()
        {
        }

        public PlayAnimation(SharedString stateName)
        {
            StateName = stateName;
        }

        protected override TaskStatus OnUpdate()
        {
            if (npc == null || StateName == null || string.IsNullOrEmpty(StateName.Value))
            {
                return TaskStatus.Failure;
            }

            npc.PlayAnimation?.Invoke(StateName.Value);
            return TaskStatus.Success;
        }
    }
}
