using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class UtilitySelector : Composite
    {
        protected Task currentTask;
        public Task CurrentTask => currentTask;

        protected override void OnEntry()
        {
            currentTask = null;
        }

        protected Task SelectTaskByUtility(List<Task> tasks)
        {
            if (tasks == null || tasks.Count == 0)
            {
                return null;
            }

            if (tasks.Count == 1)
            {
                return tasks[0];
            }

            Task bestTask = null;
            float highestUtility = float.MinValue;

            foreach (Task task in tasks)
            {
                float utility = task.GetUtility();
                if (utility > highestUtility)
                {
                    bestTask = task;
                    highestUtility = utility;
                }
            }

            return bestTask;
        }

        public override float GetUtility()
        {
            if (currentTask != null)
            {
                return currentTask.GetUtility();
            }
            return DefaultUtility;
        }

        protected override TaskStatus OnUpdate()
        {
            SelectTask();

            if (currentTask != null)
            {
                TaskStatus status = Task.Update(currentTask);

                if (status == TaskStatus.Success)
                {
                    Task.Stop(currentTask);
                    return TaskStatus.Success;
                }
                else if (status == TaskStatus.Failure)
                {
                    Task.Stop(currentTask);
                    currentTask = null;
                }
            }

            return TaskStatus.Running;
        }

        void SelectTask()
        {
            Task bestTask = SelectTaskByUtility(tasks);

            if (bestTask == null)
            {
                Debug.LogError($"Utility selector gets null when trying to select the best task!");
                return;
            }
            if (bestTask == currentTask)
            {
                return;
            }

            if (currentTask != null && currentTask.Active)
            {
                Task.Stop(currentTask);
            }

            currentTask = bestTask;
            Task.Start(currentTask);
        }
    }
}