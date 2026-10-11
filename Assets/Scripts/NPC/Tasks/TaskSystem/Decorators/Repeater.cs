using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Repeater : Task
    {
        public Task ChildTask;

        public Repeater(Task childTask)
        {
            ChildTask = childTask;
        }

        public override void OnEntry()
        {
            ChildTask.OnEntry();
        }

        public override TaskStatus OnUpdate()
        {
            if (ChildTask == null)
            {
                return TaskStatus.Failure;
            }

            TaskStatus status = ChildTask.OnUpdate();

            if (status == TaskStatus.Success || status == TaskStatus.Failure)
            {
                ChildTask.OnExit();
                ChildTask.OnEntry();
            }

            return TaskStatus.Running;
        }
    }
}