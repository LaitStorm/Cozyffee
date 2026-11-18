using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Lowers the NPC's Hunger Points by <see cref="Amount"/> (+ up to <see cref="AmountRandom"/>) once.
    /// Fails if the NPC has no hunger component that implements <see cref="ICanHunger"/>.
    /// </summary>
    public class NPCHungry : NPCAction
    {
        public SharedFloat Amount = 10f;
        public SharedFloat AmountRandom = 0f;

        public NPCHungry()
        {
        }

        public NPCHungry(SharedFloat amount, SharedFloat amountRandom = null)
        {
            Amount = amount;
            AmountRandom = amountRandom ?? 0f;
        }

        protected override TaskStatus OnUpdate()
        {
            if (npc == null || Amount == null || !(npc.Hunger is ICanHunger canHunger))
            {
                return TaskStatus.Failure;
            }

            float random = AmountRandom != null ? Random.Range(0f, AmountRandom.Value) : 0f;
            canHunger.Hungry(Amount.Value + random);
            return TaskStatus.Success;
        }
    }
}
