using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public enum TaskStatus
    {
        Success,
        Failure,
        Running,
        Inactive
    }

    internal class TaskViewParameters
	{
		internal bool ShowFoldout = true;
		internal float FinishedTime = -1000.0f;
		internal TaskStatus FinishedStatus = TaskStatus.Inactive;
	}

    public abstract class Task
    {
        private bool hasAwaken = false;
        protected GameObject Owner { get; set; }

        public virtual void SetOwner(GameObject owner)
        {
            Owner = owner;
        }

        public float DefaultUtility = 0f;

        #region Name and BehaviorTree Viewer Parameters

        public string Name { get; set; } = "";

        public virtual string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return GetType().Name;
                }
                else
                {
                    return $"{Name} ({GetType().Name})";
                }
            }
        }

        [HideInInspector]
        internal TaskViewParameters ViewParameters = new TaskViewParameters();

        #endregion

        #region Status

        public TaskStatus Status { get; set; } = TaskStatus.Inactive;

        public bool Active => Status != TaskStatus.Inactive;

        public bool CompareStatus(TaskStatus other)
        {
            return Status == other;
        }

        #endregion

        #region Virtual Methods
        public static Task SelectTaskByUtility(List<Task> tasks)
        {
            if (tasks == null || tasks.Count == 0)
            {
                return null;
            }

            Task bestTask = null;
            float highestUtility = float.MinValue;

            foreach (Task task in tasks)
            {
                float utility = task.GetUtility();

                if (utility > highestUtility)
                {
                    highestUtility = utility;
                    bestTask = task;
                }
            }

            return bestTask;
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnEntry()
        {
        }

        protected virtual void OnExit()
        {
        }

        protected virtual TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }

        public virtual float GetUtility()
        {
            return DefaultUtility;
        }

        #endregion

        #region Static Task Manipulation Methods

        public static void Awaken(Task task)
        {
            if (task == null)
            {
                Debug.LogError($"Trying to awaken a null Task!");
                return;
            }

            if (task.hasAwaken)
            {
                Debug.LogError($"Trying to awaken a Task that is already awake! Task name = '{task.FullName}'");
                return;
            }

            task.OnAwake();
            task.hasAwaken = true;
        }

        public static void Start(Task task)
        {
            if (task == null)
            {
                Debug.LogError($"Trying to start a null Task!");
                return;
            }

            if (!task.hasAwaken)
            {
                Debug.LogError($"Task '{task.FullName}' is not awake, thus cannot be started!");
                return;
            }

            if (task.CompareStatus(TaskStatus.Running))
            {
                Debug.LogError($"Task '{task.FullName}' is already running, thus cannot be started!");
                return;
            }

            task.Status = TaskStatus.Running;
            task.OnEntry();
        }

        public static void Stop(Task task)
        {
            if (task == null)
            {
                Debug.LogError($"Trying to stop a null Task!");
                return;
            }

            if (!task.hasAwaken)
            {
                Debug.LogError($"Task '{task.FullName}' is not awake, thus cannot be stopped!");
                return;
            }

            if (task.CompareStatus(TaskStatus.Inactive))
            {
                Debug.LogError($"Task '{task.FullName}' is already inactive, thus cannot be stopped!");
                return;
            }

            task.Status = TaskStatus.Inactive;
            task.OnExit();
        }

        public static void Restart(Task task)
        {
            if (task.CompareStatus(TaskStatus.Inactive))
            {
                Start(task);
            }
            else
            {
                Stop(task);
                Start(task);
            }
        }

        public static TaskStatus Update(Task task)
        {
            if (task == null)
            {
                Debug.LogError($"Trying to update a null task!");
                return TaskStatus.Failure;
            }

            if (task.Status == TaskStatus.Failure || task.Status == TaskStatus.Failure)
            {
                return task.Status;
            }

            if (task.Status == TaskStatus.Inactive)
            {
                Debug.LogError($"Trying to update an inactive task! Task name = '{task.FullName}'");
                return TaskStatus.Inactive;
            }

            TaskStatus status = task.OnUpdate();
            task.Status = status;

            if (task.Status == TaskStatus.Success || task.Status == TaskStatus.Failure)
            {
                task.ViewParameters.FinishedTime = Time.time;
                task.ViewParameters.FinishedStatus = task.Status;
            }

            return status;
        }

        #endregion
    }
}