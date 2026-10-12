using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class TaskSystemTest1 : MonoBehaviour
	{
		Task root;

		void Start ()
		{
			Sequence sequence = new();
			Log log = new("Hello World!");
			Wait wait = new(1f);
			sequence.CreateTasks(log, wait);
			root = new Repeater(sequence);

			Task.Awaken(root);
			Task.Start(root);
		}

		private void Update()
		{
			if (root.Status == TaskStatus.Running)
			{
				Task.Update(root);
			}
			else if (root.Status == TaskStatus.Success || root.Status == TaskStatus.Failure)
			{
				Task.Stop(root);
			}
			else
			{
			}
		}


	}
}