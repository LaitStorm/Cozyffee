namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Runs its child and, when the child stops for any reason (served, timed out, or interrupted by a
    /// higher-priority branch), clears <see cref="NPC.CurrentOrder"/>, frees the order spot the NPC reserved
    /// and fires <see cref="NPC.OrderEnded"/>.
    ///
    /// It also handles the order's emotions that don't belong to a single step (see <see cref="OrderMood"/>):
    /// when the NPC takes its item it goes back to Neutral (the tree makes it Happy after eating), and if the order
    /// ends after the NPC asked for its item but before it got it, the NPC cries for a while and <see cref="Unserved"/> fires.
    /// </summary>
    public class OrderScope : Decorator
    {
        /// <summary>Fired when the order ends after the NPC asked for its item, but nobody served it.</summary>
        public event System.Action Unserved;

        public OrderMood Mood;

        NPC npc;
        bool ordered;
        bool served;

        public OrderScope(Task childTask, OrderMood mood = null) : base(childTask)
        {
            Mood = mood ?? new OrderMood();
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            npc = Owner.GetComponentInParent<NPC>();
        }

        protected override void OnEntry()
        {
            ordered = false;
            served = false;

            if (npc != null)
            {
                npc.Order += OnOrder;
                npc.AcceptFood += OnAcceptFood;
            }

            base.OnEntry();
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

            if (npc == null)
            {
                return;
            }

            npc.Order -= OnOrder;
            npc.AcceptFood -= OnAcceptFood;

            npc.CurrentOrder = null;
            Area.ReleaseAll(npc);
            npc.OrderEnded?.Invoke();

            if (ordered && !served)
            {
                // Clear the waiting emotion first, so the crying goes back to Neutral instead of to Angry.
                npc.SetEmotion?.Invoke(Emotion.Neutral);

                if (Mood.UnservedCryingDuration > 0f)
                {
                    npc.SetEmotionFor?.Invoke(Emotion.Crying, Mood.UnservedCryingDuration);
                }

                Unserved?.Invoke();
            }
        }

        void OnOrder() => ordered = true;

        void OnAcceptFood()
        {
            served = true;

            // However long it waited, being served calms it down. Happy comes after eating (see BehaviorFactory).
            npc.SetEmotion?.Invoke(Emotion.Neutral);
        }
    }
}
