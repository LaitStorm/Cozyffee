using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Eats the item the NPC is holding and restores <see cref="FillAmount"/> Hunger Points (clamped to MaxPoints).
    /// Fails if the NPC isn't holding anything, so a timed-out order doesn't feed the NPC for free.
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
            if (!TryGetReceiver(out NPCItemReceiver receiver) || receiver.HeldItem == null)
            {
                return TaskStatus.Failure;
            }

            receiver.Consume();

            if (npc.Hunger != null && FillAmount != null)
            {
                npc.Hunger.Points = Mathf.Min(npc.Hunger.Points + FillAmount.Value, npc.Hunger.MaxPoints);
            }

            npc.Eat?.Invoke();

            return TaskStatus.Success;
        }
    }
}
