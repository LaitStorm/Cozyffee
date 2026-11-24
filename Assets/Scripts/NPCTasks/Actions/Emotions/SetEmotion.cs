using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    /// <summary>
    /// Sets the NPC's emotion and succeeds right away. With a <see cref="Duration"/> above 0, the NPC goes back to the
    /// emotion it had before once that time is up, while the tree carries on (unlike FeelEmotion, which waits).
    /// </summary>
    public class SetEmotion : NPCAction
    {
        public SharedEmotion Emotion = Cozyffee.Emotion.Neutral;

        /// <summary>Seconds before going back to the previous emotion. 0 = keep it.</summary>
        public SharedFloat Duration = 0f;

        public SetEmotion() { }
        public SetEmotion(SharedEmotion emotion) { Emotion = emotion; }

        protected override TaskStatus OnUpdate()
        {
            if (npc == null || Emotion == null) return TaskStatus.Failure;

            float duration = Duration != null ? Duration.Value : 0f;

            if (duration > 0f && npc.SetEmotionFor != null)
            {
                npc.SetEmotionFor(Emotion.Value, duration);
                return TaskStatus.Success;
            }

            if (npc.SetEmotion == null) return TaskStatus.Failure;

            npc.SetEmotion(Emotion.Value);
            return TaskStatus.Success;
        }
    }
}
