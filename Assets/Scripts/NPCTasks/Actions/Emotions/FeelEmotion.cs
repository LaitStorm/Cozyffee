using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class FeelEmotion : NPCAction
    {
        public SharedEmotion Emotion = Cozyffee.Emotion.Happy;
        public SharedFloat Duration = 2f;

        private Emotion previousEmotion;
        private float endTime;

        public FeelEmotion() { }
        public FeelEmotion(SharedEmotion emotion, SharedFloat duration)
        {
            Emotion = emotion;
            Duration = duration;
        }

        protected override void OnEntry()
        {
            previousEmotion = npc.GetEmotion?.Invoke() ?? Cozyffee.Emotion.Neutral;
            npc.SetEmotion?.Invoke(Emotion.Value);
            endTime = Time.time + Duration.Value;
        }

        protected override TaskStatus OnUpdate()
            => Time.time >= endTime ? TaskStatus.Success : TaskStatus.Running;

        protected override void OnExit() => npc.SetEmotion?.Invoke(previousEmotion);
    }
}