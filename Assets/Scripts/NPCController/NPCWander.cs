using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Cozyffee
{
    public class NPCWander : NPCComponent
    {
        public Area Area;

        private void Start()
        {
            SetRandomDestination();
        }

        private void Update()
        {
            if (HasArrived()) {
                SetRandomDestination();
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
    }
}