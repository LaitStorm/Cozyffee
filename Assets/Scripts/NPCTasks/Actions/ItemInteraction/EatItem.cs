using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Restores <see cref="FillAmount"/> Hunger Points (clamped to MaxPoints) and fires <see cref="NPC.Eat"/>.
    /// If the NPC is still holding the item, it's consumed first. In an order this runs last, after
    /// <see cref="ConsumeItem"/> took the item, so a timed-out order (nothing to take) never feeds the NPC.
    /// </summary>
    public class EatItem : ItemReceiverAction
    {
        public SharedFloat FillAmount = 50f;

        public EatItem()
        {
        }

        public EatItem(SharedFloat fillAmount)
        {
            FillAmount = fillAmount;
        }

        protected override TaskStatus OnUpdate()
        {
            if (npc == null)
            {
                return TaskStatus.Failure;
            }

            if (TryGetReceiver(out NPCItemReceiver receiver) && receiver.HeldItem != null)
            {
                receiver.Consume();
            }

            if (npc.Hunger != null && FillAmount != null)
            {
                npc.Hunger.Points = Mathf.Min(npc.Hunger.Points + FillAmount.Value, npc.Hunger.MaxPoints);
            }

            npc.Eat?.Invoke();

            return TaskStatus.Success;
        }
    }
}
