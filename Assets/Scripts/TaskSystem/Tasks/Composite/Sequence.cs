using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Sequence : Composite
    {
        public Sequence() {}

        public Sequence(params Task[] tasks) : base(tasks) {}

        protected int currentTaskIndex = 0;
        protected Task currentTask
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

        bool ShouldInterruptLowerPriority(Task task)
		{
			if (task is Composite)
			{
				Composite c = (Composite)task;

				if (c.InterruptLowerPriority && c.EvaluateInterruptConditions() == false)
				{
					return true;
				}
			}

			return false;
		}

        protected override void OnEntry()
        {
            currentTaskIndex = 0;

            if (IsCurrentTaskIndexValid())
            {
                Task.Start(currentTask);
            }
        }

        protected override TaskStatus OnUpdate()
        {
            if (tasks.Count <= 0)
            {
                return TaskStatus.Failure;
            }

            for (int i = 0; i < currentTaskIndex; i++)
			{
				if (ShouldInterruptLowerPriority(tasks[i]))
				{
					Task.Stop(currentTask);
					currentTaskIndex = i;
					Task.Start(currentTask);

					break;
				}
			}

            TaskStatus status = Task.Update(currentTask);

            if (status == TaskStatus.Success)
            {
                Task.Stop(currentTask);
                currentTaskIndex++;

                if (currentTaskIndex >= tasks.Count)
                {
                    return TaskStatus.Success;
                }
                else
                {
                    Task.Start(currentTask);
                }
            }
            else if (status == TaskStatus.Failure)
            {
                Task.Stop(currentTask);
                return TaskStatus.Failure;
            }

            return TaskStatus.Running;
        }
    }
}