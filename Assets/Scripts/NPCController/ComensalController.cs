using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class ComensalController : AIController
    {
        [SerializeField]
        WanderParameters wanderParameters;

        Wander wander;

        protected override void CreateTasks()
        {
            wander = new Wander(npc, wanderParameters);

            UtilitySelector.CreateTasks(wander);
        }
    }
}