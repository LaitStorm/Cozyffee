using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Cozyffee.TaskSystem
{
    public class Wait : Action
    {
        public float Duration = 1f;
        public float DurationRandom = 0f;
        private float timer = 0f;

        public Wait(float duration = 1f, float durationRandom = 0f)
        {
            this.Duration = duration;
            this.DurationRandom = durationRandom;
        }

        public override string FullName
        {
            get
            {
                string timerString = $"[{Mathf.Round(timer * 10f) / 10f} s]";

                if (timer <= 0.0001f)
                {
                    timerString = "";
                }

                if (string.IsNullOrEmpty(Name))
                {
                    return GetType().Name + $"{timerString}";
                }
                else
                {
                    return $"{Name}{timerString} ({GetType().Name})";
                }
            }
        }

        protected override void OnEntry()
        {
            timer = Duration + Random.Range(0f, DurationRandom);
        }

        protected override TaskStatus OnUpdate()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}