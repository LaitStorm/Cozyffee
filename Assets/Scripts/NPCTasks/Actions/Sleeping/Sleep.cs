using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    /// <summary>
    /// Puts the NPC to sleep for <see cref="Duration"/> (+ up to <see cref="DurationRandom"/>) seconds.
    /// The NPC stops moving while asleep, and wakes up when the task ends or gets interrupted.
    /// </summary>
    public class Sleep : NPCAction
    {
        public SharedFloat Duration = 10f;
        public SharedFloat DurationRandom = 0f;

        private float endTime;

        public Sleep() { }
        public Sleep(SharedFloat duration, SharedFloat durationRandom = null)
        {
            Duration = duration;
            DurationRandom = durationRandom ?? 0f;
        }

        protected override void OnEntry()
        {
            endTime = Time.time + Duration.Value + Random.Range(0f, DurationRandom.Value);

            npc.CanMove = false;
            npc.IsSleeping = true;
            npc.FallAsleep?.Invoke();
        }

        protected override TaskStatus OnUpdate()
            => Time.time >= endTime ? TaskStatus.Success : TaskStatus.Running;

        protected override void OnExit()
        {
            npc.IsSleeping = false;
            npc.CanMove = true;
            npc.WakeUp?.Invoke();
        }
    }
}
