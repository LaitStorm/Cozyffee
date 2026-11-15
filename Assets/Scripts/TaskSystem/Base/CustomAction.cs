using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Cozyffee.TaskSystem
{
    public sealed class CustomAction : Action
    {
        public UnityAction Awake;
        public UnityAction Entry;
        public UnityAction Exit;
        public new System.Func<TaskStatus> Update;
        public System.Func<float> Utility;

        protected override void OnAwake()
        {
            Awake?.Invoke();
        }

        protected override void OnEntry()
        {
            Entry?.Invoke();
        }

        protected override void OnExit()
        {
            Exit?.Invoke();
        }

        protected override TaskStatus OnUpdate()
        {
            if (Update != null)
            {
                return Update.Invoke();
            }

            return TaskStatus.Success;
        }

        public override float GetUtility()
        {
            if (Utility != null)
            {
                return Utility.Invoke();
            }

            return DefaultUtility;
        }
    }
}