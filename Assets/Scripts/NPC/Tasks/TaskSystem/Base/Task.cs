using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public enum TaskStatus
    {
        Success,
        Failure,
        Running
    }

    public class Task
    {
        public static Task SelectTaskByUtility(List<Task> tasks)
        {
            if (tasks == null || tasks.Count == 0)
            {
                return null;
            }

            Task bestTask = null;
            float highestUtility = float.MinValue;

            foreach (Task task in tasks)
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

        public virtual TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }

        public virtual float GetUtility()
        {
            return 0f;
        }
    }
}