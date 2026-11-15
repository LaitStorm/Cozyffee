using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Interrupt : Decorator
    {
        public bool ReturnSuccess = true;

        public List<Condition> Conditions { get; private set; }

        public Interrupt(Task childTask, params Condition[] conditions) : base(childTask)
        {
            Conditions = new List<Condition>(conditions);
        }

        public void AddCondition(Condition condition)
        {
            Conditions.Add(condition);
        }

        public override void SetOwner(GameObject owner)
        {
            base.SetOwner(owner);

            foreach (Condition condition in Conditions)
            {
                condition.SetOwner(owner);
            }
        }

        public override void SetBehaviorTree(BehaviorTree behaviorTree)
        {
            base.SetBehaviorTree(behaviorTree);

            foreach (Condition condition in Conditions)
            {
                condition.SetBehaviorTree(behaviorTree);
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            foreach (Condition condition in Conditions)
            {
                Task.Awaken(condition);
            }
        }

        bool EvaluateConditions()
        {
            foreach (Task task in Conditions)
            {
                TaskStatus status = Task.Update(task);

                if (status == TaskStatus.Failure)
                {
                    return false;
                }
            }

            return true;
        }

        protected override TaskStatus OnUpdate()
        {
            if (child == null)
            {
                return TaskStatus.Failure;
            }

            if (EvaluateConditions() == false)
            {
                return ReturnSuccess ? TaskStatus.Success : TaskStatus.Failure;
            }

            TaskStatus status = Task.Update(child);

            if (status == TaskStatus.Success || status == TaskStatus.Failure)
            {
                Task.Stop(child);
                return status;
            }

            return TaskStatus.Running;
        }
    }
}