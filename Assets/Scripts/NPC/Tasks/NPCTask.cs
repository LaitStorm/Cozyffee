using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public enum NPCTaskStatus
    {
        Success,
        Failure,
        Running
    }
    public abstract class NPCTask
    {
        protected NPC npc;
        public NPCTask(NPC npc)
        {
            this.npc = npc;
        }

        public static NPCTask SelectTaskByUtility(List<NPCTask> tasks)
        {
            if (tasks == null || tasks.Count == 0)
            {
                return null;
            }

            NPCTask bestTask = null;
            float highestUtility = float.MinValue;

            foreach (NPCTask task in tasks)
            {
                float utility = task.GetUtility();

                if (utility > highestUtility)
                {
                    highestUtility = utility;
                    bestTask = task;
                }
            }

            return bestTask;
        }

        public virtual void OnEntry()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual NPCTaskStatus OnUpdate()
        {
            return NPCTaskStatus.Success;
        }

        public virtual float GetUtility()
        {
            return 0f;
        }
    }
}