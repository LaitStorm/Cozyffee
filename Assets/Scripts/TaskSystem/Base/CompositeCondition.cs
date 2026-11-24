using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public enum ConditionEvaluationMode
    {
        AND,
        OR
    }

    public sealed class CompositeCondition : Condition
    {

        List<Condition> conditions = new List<Condition>();

        public ConditionEvaluationMode EvaluationMode = ConditionEvaluationMode.AND;

        public bool Empty => conditions.Count <= 0;

        public void SetConditions(params Condition[] conditions)
        {
            this.conditions = new List<Condition>(conditions);
        }

        public void AddCondition(Condition condition)
        {
            conditions.Add(condition);
        }

        public bool Evaluate()
        {
            foreach (Condition c in conditions)
            {
                TaskStatus status = Task.Update(c);

                if (status == TaskStatus.Failure && EvaluationMode == ConditionEvaluationMode.AND)
                {
                    return false;
                }
                else if (status == TaskStatus.Success && EvaluationMode == ConditionEvaluationMode.OR)
                {
                    return true;
                }
            }

            if (EvaluationMode == ConditionEvaluationMode.AND)
            {
                return true;
            }

            return false;
        }

        protected override void OnAwake()
        {
            foreach (Condition c in conditions)
            {
                Task.Awaken(c);
            }
        }

        public override void SetOwner(GameObject owner)
        {
            base.SetOwner(owner);

            foreach (Condition c in conditions)
            {
                c.SetOwner(owner);
            }
        }

        public override void SetBehaviorTree(BehaviorTree behaviorTree)
        {
            base.SetBehaviorTree(behaviorTree);
            
            foreach (Condition c in conditions)
            {
                c.SetBehaviorTree(behaviorTree);
            }
        }

        protected override TaskStatus OnUpdate()
        {
            if (Evaluate())
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}