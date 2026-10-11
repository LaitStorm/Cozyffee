using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class UtilitySelector : CompositeTask
    {
        protected Task currentTask;
        public UtilitySelector()
        { 
        }

        public override float GetUtility()
        {
            if (currentTask != null)
            {
                return currentTask.GetUtility();
            }
            return 0f;
        }

        public override TaskStatus OnUpdate()
        {
            PickCurrentTask();

            if (currentTask != null)
            {
                return currentTask.OnUpdate();
            }

            return TaskStatus.Success;
        }

        void SetCurrentTask(Task task)
        {
            if (currentTask != task)
            {
                if (currentTask != null)
                {
                    currentTask.OnExit();
                }

                currentTask = task;

                if (currentTask != null)
                {
                    currentTask.OnEntry();
                }
            }
        }

        void PickCurrentTask()
        {
            Task bestTask = Task.SelectTaskByUtility(tasks);

            SetCurrentTask(bestTask);
        }
    }
}