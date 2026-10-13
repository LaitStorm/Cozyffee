using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class RandomChoice : Composite
    {
        public const float DefaultWeight = 1f;

        private readonly List<float> weights = new List<float>();

        private Task chosen;

        public override void ClearTasks()
        {
            base.ClearTasks();
            weights.Clear();
        }

        public override void AddTask(Task task)
        {
            AddTask(task, DefaultWeight);
        }

        public void AddTask(Task task, float weight)
        {
            if (task == null)
            {
                throw new System.ArgumentNullException(nameof(task));
            }

            if (float.IsNaN(weight) || float.IsInfinity(weight) || weight < 0f)
            {
                weight = DefaultWeight;
            }

            base.AddTask(task);
            weights.Add(weight);
        }

        private Task Pick()
        {
            int count = tasks.Count;

            if (count == 0)
            {
                return null;
            }

            float total = 0f;
            for (int i = 0; i < count; i++)
            {
                total += weights[i];
            }

            if (total <= 0f)
            {
                return tasks[Random.Range(0, count)];
            }

            float roll = Random.value * total;
            float cumulative = 0f;
            int lastCandidate = -1;

            for (int i = 0; i < count; i++)
            {
                if (weights[i] <= 0f)
                {
                    continue;
                }

                cumulative += weights[i];
                lastCandidate = i;

                if (roll < cumulative)
                {
                    return tasks[i];
                }
            }

            return tasks[lastCandidate];
        }

        protected override void OnEntry()
        {
            chosen = Pick();

            if (chosen != null)
            {
                Task.Start(chosen);
            }
        }

        protected override TaskStatus OnUpdate()
        {
            if (chosen == null)
            {
                return TaskStatus.Failure;
            }

            TaskStatus status = Task.Update(chosen);

            if (status == TaskStatus.Success || status == TaskStatus.Failure)
            {
                Task.Stop(chosen);
                return status;
            }

            return TaskStatus.Running;
        }
    }
}