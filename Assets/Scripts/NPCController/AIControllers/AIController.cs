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

        [SerializeField, Range(0, 99), Tooltip("NavMeshAgent avoidance priority while holding a spot. Lower = more important: " +
                                                "NPCs with a higher number steer around this one instead of pushing it.")]
        int holdingAvoidancePriority = 10;

        [SerializeField, Range(0, 99), Tooltip("NavMeshAgent avoidance priority while asleep. Lower = more important: " +
                                                "NPCs with a higher number steer around the sleeper instead of pushing it.")]
        int sleepingAvoidancePriority = 10;

        int defaultAvoidancePriority;
        bool isHolding;
        bool isSleeping;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                this.npc.SetDestination -= OnSetDestination;
                this.npc.SetMaxSpeed -= OnSetMaxSpeed;
                this.npc.HasPathToDestination -= OnHasPathToDestination;
                this.npc.Eat -= OnEat;
                this.npc.SetHoldingPosition -= OnSetHoldingPosition;
                this.npc.FallAsleep -= OnFallAsleep;
                this.npc.WakeUp -= OnWakeUp;
            }
            base.SetNPC(npc);

            if (npc != null)
            {
                Agent = GetComponentInParent<NavMeshAgent>();

                Agent.speed = npc.WalkSpeed;
                defaultAvoidancePriority = Agent.avoidancePriority;

                npc.SetDestination += OnSetDestination;
                npc.SetMaxSpeed += OnSetMaxSpeed;
                npc.HasPathToDestination += OnHasPathToDestination;
                npc.Eat += OnEat;
                npc.SetHoldingPosition += OnSetHoldingPosition;
                npc.FallAsleep += OnFallAsleep;
                npc.WakeUp += OnWakeUp;

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

        void OnEat()
        {
            return;
        }

        void OnSetHoldingPosition(bool holding)
        {
            isHolding = holding;
            UpdateAvoidancePriority();
        }

        void OnFallAsleep()
        {
            isSleeping = true;
            UpdateAvoidancePriority();
        }

        void OnWakeUp()
        {
            isSleeping = false;
            UpdateAvoidancePriority();
        }

        // Uses the most important (lowest) priority that currently applies
        void UpdateAvoidancePriority()
        {
            if (Agent == null)
            {
                return;
            }

            int priority = defaultAvoidancePriority;

            if (isHolding)
            {
                priority = Mathf.Min(priority, holdingAvoidancePriority);
            }

            if (isSleeping)
            {
                priority = Mathf.Min(priority, sleepingAvoidancePriority);
            }

            Agent.avoidancePriority = priority;
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