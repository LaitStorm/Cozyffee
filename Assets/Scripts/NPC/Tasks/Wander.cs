using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Cozyffee
{
    [System.Serializable]
    public class WanderParameters
    {
        public float searchRadius = 20f;

        [Space(15)]
        public float maxWaitTime = 3f;
        public float maxWaitTimeRandom = 5f;

        [Space(15)]
        public float maxWanderTime = 5f;
        public float maxWanderTimeRandom = 7f;
    }
    public class Wander : NPCTask
    {
        private WanderParameters parameters;

        enum EState
        {
            Wandering,
            Waiting
        }

        EState state = EState.Wandering;
        private float waitTime = 0f;
        private float wanderTime = 0f;
        Vector3 wanderDestination;

        public Wander(NPC npc, WanderParameters parameters) : base(npc)
        {
            this.parameters = parameters;
        }

        public override float GetUtility()
        {
            return 1.0f;
        }

        public override void OnEntry()
        {
            if (state == EState.Waiting)
            {
                ChangeState(EState.Wandering);
            }
            else
            {
                npc.SetDestination?.Invoke(wanderDestination);
                npc.CanMove = true;
            }
        }

        public override NPCTaskStatus OnUpdate()
        {
            if (state == EState.Waiting) {
                waitTime -= Time.deltaTime;
                if (waitTime <= 0f)
                {
                    ChangeState(EState.Wandering);
                }
            }
            else if (state == EState.Wandering)
            {
                wanderTime -= Time.deltaTime;
                if (HasArrived() || wanderTime <= 0f)
                {
                    ChangeState(EState.Waiting);
                }
            }

            return NPCTaskStatus.Running;
        }

        void ChangeState(EState newState)
        {
            state = newState;
            if (state == EState.Wandering)
            {
                npc.CanMove = true;
                SetRandomDestination();
                wanderTime = parameters.maxWanderTime + Random.Range(0f,parameters.maxWanderTimeRandom);
            }
            else if (state == EState.Waiting)
            {
                waitTime = parameters.maxWaitTime + Random.Range(0f,parameters.maxWaitTimeRandom);

                npc.CanMove = false;
            }
        }

        void SetRandomDestination()
        {
            for (int i = 0; i < 1000; i++)
            {
                Vector3 randomDirection = Random.insideUnitSphere * parameters.searchRadius;
                randomDirection += npc.Position;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    wanderDestination = hit.position;
                    npc.SetDestination?.Invoke(wanderDestination);
                    return;
                }
            }
        }

        bool HasArrived()
        {
            return Vector3.Distance(npc.Position, wanderDestination) <= 1f;
        }
    }
}