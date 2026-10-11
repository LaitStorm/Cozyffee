using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Game.Cozyffee
{
    public class ComensalController : AIController
    {
        [SerializeField]
        WanderParameters wanderParameters;

        Wander wander;

        List<NPCTask> tasks = new List<NPCTask>();
        NPCTask currentTask;  
        
        public override void SetNPC(NPC npc)
        {
            base.SetNPC(npc);

            if (npc != null)
            {
                tasks.Clear();

                wander = new Wander(npc, wanderParameters);
                tasks.Add(wander);
            }
        }

        protected override void Update()
        {
            base.Update();

            NPCTask currentTask = NPCTask.SelectTaskByUtility(tasks);
            currentTask.OnUpdate();
        }
    }
}