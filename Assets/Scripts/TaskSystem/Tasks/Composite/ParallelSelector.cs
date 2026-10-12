using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class ParallelSelector : Parallel
	{
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

			return TaskStatus.Running;
		}
	}
}