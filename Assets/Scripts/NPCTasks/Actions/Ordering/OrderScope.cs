namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Runs its child and, when the child stops for any reason (served, timed out, or interrupted by a
    /// higher-priority branch), clears <see cref="NPC.CurrentOrder"/> and frees the order spot the NPC reserved.
    /// </summary>
    public class OrderScope : Decorator
    {
        NPC npc;

        public OrderScope(Task childTask) : base(childTask) {}

        protected override void OnAwake()
        {
            base.OnAwake();
            npc = Owner.GetComponentInParent<NPC>();
        }

        protected override TaskStatus OnUpdate()
        {
            if (child == null)
            {
                return TaskStatus.Failure;
            }

            return Task.Update(child);
        }

        protected override void OnExit()
        {
            base.OnExit();

            if (npc != null)
            {
                npc.CurrentOrder = null;
                Area.ReleaseAll(npc);
            }
        }
    }
}
