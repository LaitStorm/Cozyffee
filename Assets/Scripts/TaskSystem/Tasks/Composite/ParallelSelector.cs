using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class ParallelSelector : Parallel
	{
		public ParallelSelector() {}

		public ParallelSelector(params Task[] tasks) : base(tasks) {}

		protected override TaskStatus OnUpdate()
		{
			if (tasks.Count <= 0)
			{
				return TaskStatus.Failure;
			}

			foreach (Task task in tasks)
			{
				if (task.Active == false)
				{
					continue;
				}

				TaskStatus status = Task.Update(task);

				if (status == TaskStatus.Failure)
				{
					Task.Stop(task);
				}
				else if (status == TaskStatus.Success)
				{
					Task.Stop(task);
					return TaskStatus.Success;
				}
			}

			if (AllChildrenFinished())
            {
                return TaskStatus.Failure;
            }

			return TaskStatus.Running;
		}
	}
}