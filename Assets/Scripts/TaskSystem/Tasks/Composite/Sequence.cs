using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Sequence : Composite
    {
        public Sequence() {}

        public Sequence(params Task[] tasks) : base(tasks) {}

        protected List<int> childIndexes = new List<int>();

        protected int currentTaskIndex = 0;

        public bool Randomize = false;

        protected Task currentTask
        {
            get
            {
                try
                {
                    return GetTask(currentTaskIndex);
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

        protected Task GetTask(int index)
        {
            int indexInTasks = childIndexes[index];

            return tasks[indexInTasks];
        }

        protected void CreateChildIndexes()
        {
            childIndexes.Clear();

            for (int i = 0; i < tasks.Count; i++)
            {
                childIndexes.Add(i);
            }

            if (Randomize)
            {
                for (int i = 0; i < childIndexes.Count; i++)
                {
                    int otherIndex = Random.Range(0, childIndexes.Count);

                    int value = childIndexes[otherIndex];
                    childIndexes[otherIndex] = childIndexes[i];
                    childIndexes[i] = value;
                }
            }
        }

        protected override void OnEntry()
        {
            currentTaskIndex = 0;

            CreateChildIndexes();

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
				if (ShouldInterruptLowerPriority(GetTask(i)))
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