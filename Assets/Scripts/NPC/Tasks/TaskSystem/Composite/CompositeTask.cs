using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public abstract class CompositeTask : Task
    {
        protected List<Task> tasks = new List<Task>();

        public void CreateTasks(params Task[] tasks)
        {
            this.tasks.Clear();
            foreach (Task task in tasks)
            {
                this.tasks.Add(task);
            }
        }
    }
}