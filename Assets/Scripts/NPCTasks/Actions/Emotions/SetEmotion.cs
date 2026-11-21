using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class SetEmotion : NPCAction
    {
        public SharedEmotion Emotion = Cozyffee.Emotion.Neutral;

        public SetEmotion() { }
        public SetEmotion(SharedEmotion emotion) { Emotion = emotion; }

        protected override TaskStatus OnUpdate()
        {
            if (npc == null || npc.SetEmotion == null || Emotion == null) return TaskStatus.Failure;
            npc.SetEmotion(Emotion.Value);
            return TaskStatus.Success;
        }
    }
}