using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Cozyffee.TaskSystem
{
    public sealed class NPCCustomAction : Action
    {
        public NPC NPC;
		public NPCSensor Sensor;

        public UnityAction<NPCCustomAction> Awake;
        public UnityAction<NPCCustomAction> Entry;
        public UnityAction<NPCCustomAction> Exit;
        public new System.Func<NPCCustomAction, TaskStatus> Update;

        protected override void OnAwake()
        {
            NPC = Owner.GetComponentInParent<NPC>();
			Sensor = Owner.GetComponentInParent<NPCSensor>();

            Awake?.Invoke(this);
        }

        protected override void OnEntry()
        {
            Entry?.Invoke(this);
        }

        protected override void OnExit()
        {
            Exit?.Invoke(this);
        }

        protected override TaskStatus OnUpdate()
        {
            if (Update != null)
            {
                return Update.Invoke(this);
            }

            return TaskStatus.Success;
        }
    }
}