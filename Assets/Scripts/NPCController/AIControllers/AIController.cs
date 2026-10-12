using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class AIController : NPCController
    {
        [SerializeField, HideInInspector]
        private NavMeshAgent Agent;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                this.npc.SetDestination -= OnSetDestination;
                this.npc.SetMaxSpeed -= OnSetMaxSpeed;
                this.npc.HasPathToDestination -= OnHasPathToDestination;
            }
            base.SetNPC(npc);

            if (npc != null)
            {
                Agent = GetComponentInParent<NavMeshAgent>();

                Agent.speed = npc.WalkSpeed;

                npc.SetDestination += OnSetDestination;
                npc.SetMaxSpeed += OnSetMaxSpeed;
                npc.HasPathToDestination += OnHasPathToDestination;

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

        bool OnHasPathToDestination()
        {
            return Agent.pathStatus != NavMeshPathStatus.PathInvalid;
        }

        #region UnityMethods
        protected virtual void Update()
        {
            if (Agent == null)
            {
                return;
            }
            npc.Velocity = Agent.velocity;
            Agent.isStopped = !npc.CanMove;
            Agent.updateRotation = npc.CanTurn;
        }
        #endregion
    }
}