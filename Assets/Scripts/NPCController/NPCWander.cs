using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Game.Cozyffee
{
    public class NPCWander : NPCComponent
    {
        public Area Area;

        enum EState
        {
            Wandering,
            Waiting
        }

        [SerializeField]
        float maxWaitTime = 3f;

        [SerializeField]
        float maxWaitTimeRandom = 5f;

        [Space(15f)]
        float maxWanderTime = 5f;


        [SerializeField]
        private float waitTime = 0f;

        [SerializeField]
        private float wanderTime = 0f;

        [Header("Debugging")]
        [SerializeField]
        EState state = EState.Wandering;

        private void Start()
        {
            if (Random.Range(0f,100.0f) > 50f)
            {
                ChangeState(EState.Wandering);
            }
            else
            {
                ChangeState(EState.Waiting);
            }
        }

        private void Update()
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
        }

        bool HasArrived()
        {
            return npc.Agent.remainingDistance <= npc.Agent.stoppingDistance;
        }

        void SetRandomDestination()
        {
            npc.Agent.SetDestination(Area.GetRandomPoint());
        }

        void ChangeState(EState newState)
        {
            state = newState;
            if (state == EState.Wandering)
            {
                npc.Agent.isStopped = false;
                SetRandomDestination();
                wanderTime = maxWanderTime;
            }
            else if (state == EState.Waiting)
            {
                waitTime = maxWaitTime + Random.Range(0f,maxWaitTimeRandom);

                npc.Agent.isStopped = true;
            }
        }
    }
}