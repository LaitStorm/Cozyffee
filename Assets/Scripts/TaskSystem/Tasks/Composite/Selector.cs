using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class Selector : Sequence
	{
		protected override TaskStatus OnUpdate()
		{
			if (tasks.Count <= 0)
			{
				return TaskStatus.Failure;
			}

			TaskStatus status = Task.Update(currentTask);

			if (status == TaskStatus.Success)
			{
				Task.Stop(currentTask);
				return TaskStatus.Success;
			}
			else if (status == TaskStatus.Failure)
			{
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