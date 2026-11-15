using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public abstract class Composite : Task
    {
        protected List<Task> tasks = new List<Task>();
        public IReadOnlyCollection<Task> Tasks => tasks.AsReadOnly();

        public bool InterruptLowerPriority = false;

        protected CompositeCondition condition = new CompositeCondition();
        protected CompositeCondition interruptCondition = new CompositeCondition();

        public Composite(){}

        public Composite(params Task[] tasks)
        {
            this.tasks = new List<Task>(tasks);
        }

        #region Tasks Methods

        public virtual void ClearTasks()
        {
            this.tasks.Clear();
        }

        public virtual void AddTask(Task task)
        {
            tasks.Add(task);
        }

        public void CreateTasks(params Task[] tasks)
        {
            ClearTasks();

            foreach (Task task in tasks)
            {
                AddTask(task);
            }
        }

        #endregion

        #region Condition Methods

        public void SetConditionsEvaluationMode(ConditionEvaluationMode evaluationMode)
        {
            condition.EvaluationMode = evaluationMode;
        }

        public void SetInterruptConditionsEvaluationMode(ConditionEvaluationMode evaluationMode)
        {
            interruptCondition.EvaluationMode = evaluationMode;
        }

        public void SetConditions(params Condition[] conditions)
        {
            condition.SetConditions(conditions);
        }

        public void AddCondition(Condition condition)
        {
            this.condition.AddCondition(condition);
        }

        public bool EvaluateConditions()
        {
            return condition.Evaluate();
        }

        public void SetInterruptConditions(params Condition[] conditions)
        {
            interruptCondition.SetConditions(conditions);
        }

        public void AddInterruptCondition(Condition condition)
        {
            interruptCondition.AddCondition(condition);
        }

        public bool EvaluateInterruptConditions()
        {
            return interruptCondition.Evaluate();
        }

        #endregion

        public override void SetOwner(GameObject owner)
        {
            base.SetOwner(owner);

            foreach (Task task in tasks)
            {
                task.SetOwner(owner);
            }

            condition.SetOwner(owner);
            interruptCondition.SetOwner(owner);
        }

        protected override void OnAwake()
        {
            foreach (Task task in tasks)
            {
                Task.Awaken(task);
            }

            Task.Awaken(condition);
            Task.Awaken(interruptCondition);
        }

        protected override void OnExit()
        {
            foreach (Task task in tasks)
            {
                if (task.Active)
                {
                    Task.Stop(task);
                }
            }
        }

        public override float GetUtility()
        {
            if (UtilityOverride != null)
            {
                return UtilityOverride(this);
            }
            
            float maxUtility = 0.0f;

            foreach (Task task in tasks)
            {
                float utility = task.GetUtility();

                if (utility > maxUtility)
                {
                    maxUtility = utility;
                }
            }

            return DefaultUtility + maxUtility;
        }
    }
}