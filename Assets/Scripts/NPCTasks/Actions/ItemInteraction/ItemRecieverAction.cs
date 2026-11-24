using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public abstract class ItemReceiverAction : NPCAction
    {
        private NPCItemReceiver receiver;
        private bool hasWarned;

        protected bool TryGetReceiver(out NPCItemReceiver result)
        {
            if (receiver == null)
            {
                receiver = npc.GetComponentInChildren<NPCItemReceiver>(true);
            }

            result = receiver;

            if (receiver == null && !hasWarned)
            {
                Debug.LogWarning($"{GetType().Name}: '{npc.gameObject.name}' has no {nameof(NPCItemReceiver)}.");
                hasWarned = true;
            }

            return receiver != null;
        }
    }
}