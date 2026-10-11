using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Game.Cozyffee
{
    public class AIController : NPCController
    {
        [SerializeField]
        private NavMeshAgent Agent;

        public override void SetNPC(NPC npc)
        {
            if (npc != null)
            {
                npc.SetDestination -= OnSetDestination;
                npc.SetMaxSpeed -= OnSetMaxSpeed;
            }
            base.SetNPC(npc);

            if (npc != null)
            {
                Agent = GetComponentInParent<NavMeshAgent>();

                Agent.speed = npc.WalkSpeed;

                npc.SetDestination += OnSetDestination;
                npc.SetMaxSpeed += OnSetMaxSpeed;

                if (Agent == null)
                {
                    Debug.LogError($"NavMeshAgent not found on NPC '{npc.gameObject.name}'.");
                }
            }
        }

        void OnSetDestination(Vector3 destination)
        {
            Agent.SetDestination(destination);
        }

        void OnSetMaxSpeed(float maxSpeed)
        {
            Agent.speed = maxSpeed;
        }

        #region UnityMethods
        protected virtual void Update()
        {
            if (Agent == null)
            {
                return;
            }

            if (Agent != null)
            {
                npc.Velocity = Agent.velocity;
                Agent.isStopped = !npc.CanMove;
            }
        }
        #endregion
    }
}