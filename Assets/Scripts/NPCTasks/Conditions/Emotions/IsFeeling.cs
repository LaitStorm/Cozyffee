using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class IsFeeling : NPCCondition
    {
        public SharedEmotion Emotion = Cozyffee.Emotion.Neutral;

        public IsFeeling() { }
        public IsFeeling(SharedEmotion emotion)
        {
            Emotion = emotion;
        }

        protected override TaskStatus OnUpdate()
            => npc?.GetEmotion != null && npc.GetEmotion() == Emotion.Value ? TaskStatus.Success : TaskStatus.Failure;
    }

}