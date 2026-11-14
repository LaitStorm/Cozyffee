using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Lowers the NPC's Hunger Points by <see cref="Amount"/> once.
    /// Fails if the NPC has no hunger component that implements <see cref="ICanHunger"/>.
    /// </summary>
    public class NPCHungry : NPCAction
    {
        public SharedFloat Amount = 10f;

        public NPCHungry()
        {
        }

        public NPCHungry(SharedFloat amount)
        {
            Amount = amount;
        }

        protected override TaskStatus OnUpdate()
        {
            if (npc == null || Amount == null || !(npc.Hunger is ICanHunger canHunger))
            {
                return TaskStatus.Failure;
            }

            canHunger.Hungry(Amount.Value);
            return TaskStatus.Success;
        }
    }
}
