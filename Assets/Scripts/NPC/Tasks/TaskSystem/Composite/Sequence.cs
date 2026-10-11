using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Sequence : CompositeTask
    {
        int currentTaskIndex = 0;
        Task currentTask
        {
            get
            {
                try
                {
                    return tasks[currentTaskIndex];
                }
                catch
                {
                    Debug.LogError($"Current task index {currentTaskIndex} out of bounds, task count = {tasks.Count}.");

                    return null;
                }
            }
        }

        bool IsCurrentTaskIndexValid()
        {
            return currentTaskIndex >= 0 && currentTaskIndex < tasks.Count;
        }

        public override void OnEntry()
        {
            currentTaskIndex = 0;

            if (currentTask != null)
            {
                currentTask.OnEntry();
            }
        }

        public override float GetUtility()
        {
            float maxUtility = 0f;
            foreach (Task task in tasks)
            {
                float utility = task.GetUtility();
                if (utility > maxUtility)
                {
                    maxUtility = utility;
                }
            }
            return maxUtility;
        }

        public override TaskStatus OnUpdate()
        {
            if (tasks.Count <= 0)
            {
                return TaskStatus.Failure;
            }

            TaskStatus status = currentTask.OnUpdate();

            if (status == TaskStatus.Success)
            {
                Task previousTask = currentTask;
                currentTaskIndex++;

                if (currentTaskIndex >= tasks.Count)
                {
                    previousTask.OnExit();
                    return TaskStatus.Success;
                }
                else
                {
                    previousTask.OnExit();
                    currentTask.OnEntry();
                }
            }
            else if (status == TaskStatus.Failure)
            {
                currentTask.OnExit();

                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }
    }
}