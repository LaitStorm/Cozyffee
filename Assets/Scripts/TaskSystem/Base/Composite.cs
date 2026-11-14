using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public abstract class Composite : Task
    {
        protected List<Task> tasks = new List<Task>();
        public IReadOnlyCollection<Task> Tasks => tasks.AsReadOnly();

        protected List<Condition> conditions = new List<Condition>();

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

        public void SetConditions(params Condition[] conditions)
        {
            this.conditions = new List<Condition>(conditions);
        }

        public void AddCondition(Condition condition)
        {
            conditions.Add(condition);
        }

        public bool EvaluateConditions()
        {
            foreach (Condition c in conditions)
            {
                TaskStatus status = Task.Update(c);

                if (status == TaskStatus.Failure)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        public override void SetOwner(GameObject owner)
        {
            base.SetOwner(owner);

            foreach (Task task in tasks)
            {
                task.SetOwner(owner);
            }

            foreach (Condition condition in conditions)
            {
                condition.SetOwner(owner);
            }
        }

        protected override void OnAwake()
        {
            foreach (Task task in tasks)
            {
                Task.Awaken(task);
            }

            foreach (Task condition in conditions)
            {
                Task.Awaken(condition);
            }
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