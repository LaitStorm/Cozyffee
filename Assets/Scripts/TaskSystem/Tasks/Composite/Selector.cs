using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class Selector : Sequence
	{
		public Selector() {}

        public Selector(params Task[] tasks) : base(tasks) {}

		bool ShouldInterruptLowerPriority(Task task)
		{
			if (task is Composite)
			{
				Composite c = (Composite)task;

				if (c.InterruptLowerPriority && c.EvaluateConditions() == true)
				{
					return true;
				}
			}

			return false;
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
				return TaskStatus.Success;
			}
			else if (status == TaskStatus.Failure)
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

			return TaskStatus.Running;

		}
	}
}