using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// How an NPC's emotion changes around an order (see <see cref="BehaviorFactory"/>.Order):
    /// - While it waits for its item: Crying after <see cref="CryingAfter"/> seconds, Angry after <see cref="AngryAfter"/>.
    /// - Served: back to Neutral right away, then Happy for <see cref="HappyDuration"/> seconds after eating.
    /// - Not served (timed out or interrupted): Crying for <see cref="UnservedCryingDuration"/> seconds, then Neutral.
    /// The wait is counted from the moment the NPC shows its order at the order spot.
    /// </summary>
    [System.Serializable]
    public class OrderMood
    {
        [Tooltip("Seconds waiting for the item before the NPC starts crying.")]
        [Min(0f)]
        public float CryingAfter = 150f;

        [Tooltip("Seconds waiting for the item before the NPC gets angry. Counted from when it showed its order, not from when it started crying.")]
        [Min(0f)]
        public float AngryAfter = 300f;

        [Tooltip("Seconds the NPC stays Happy after eating what it ordered.")]
        [Min(0f)]
        public float HappyDuration = 30f;

        [Tooltip("Seconds the NPC cries after giving up on an order nobody served. 0 = just go back to Neutral.")]
        [Min(0f)]
        public float UnservedCryingDuration = 60f;
    }
}
