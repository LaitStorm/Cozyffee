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
        public GameObject Owner { get; protected set; }
        public BehaviorTree BehaviorTree { get; protected set; }
        public BehaviorTreeData Data => BehaviorTree.Data;

        public virtual void SetOwner(GameObject owner)
        {
            Owner = owner;
        }

        public virtual void SetBehaviorTree(BehaviorTree behaviorTree)
        {
            this.BehaviorTree = behaviorTree;
        }

        public float DefaultUtility = 0f;

        public System.Func<Task, float> UtilityOverride = null;

        #region Status

        private TaskStatus status = TaskStatus.Inactive;

        public TaskStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                if (status != value)
                {
                    if (value == TaskStatus.Success || value == TaskStatus.Failure)
                    {
                        ViewParameters.FinishedTime = Time.time;
                        ViewParameters.FinishedStatus = value;
                    }
                }

                status = value;
            }
        }

        public bool Active => Status != TaskStatus.Inactive;

        public bool CompareStatus(TaskStatus other)
        {
            return Status == other;
        }

        #endregion

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

        public bool FoldoutVisible
        {
            get { return ViewParameters.ShowFoldout; }
            set
            {
                ViewParameters.ShowFoldout = value;
            }
        }

        public void SetFoldoutVisible(bool visible)
        {
            ViewParameters.ShowFoldout = visible;
        }

        #endregion

        #region Virtual Methods

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
            if (UtilityOverride != null)
            {
                return UtilityOverride(this);
            }

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

            if (task is Condition)
            {
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

            if (task is Condition)
            {
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

            if (task is Condition)
            {
                return UpdateCondition((Condition)task);
            }

            if (task.Status == TaskStatus.Success || task.Status == TaskStatus.Failure)
            {
                return task.Status;
            }

            if (task.Status == TaskStatus.Inactive)
            {
                Debug.LogError($"Trying to update an inactive task! Task name = '{task.FullName}'");
                return TaskStatus.Inactive;
            }

            if (task is Composite)
            {
                return UpdateComposite((Composite)task);
            }

            TaskStatus status = task.OnUpdate();
            task.Status = status;

            return status;
        }

        private static TaskStatus UpdateCondition(Condition condition)
        {
            condition.Status = condition.OnUpdate();

            if (condition.Status != TaskStatus.Success && condition.Status != TaskStatus.Failure)
            {
                Debug.LogError($"Condition '{condition.FullName}' can only return success or failure, but it's returning '{condition.Status}', which is something else!");
            }

            if (condition.Invert)
            {
                condition.Status = (condition.Status == TaskStatus.Success) ? TaskStatus.Failure : TaskStatus.Success;
            }

            return condition.Status;
        }

        private static TaskStatus UpdateComposite(Composite task)
        {
            if (task.EvaluateConditions() == false)
            {
                Stop(task);
                task.Status = TaskStatus.Failure;
                return TaskStatus.Failure;
            }

            if (task.InterruptSelf && task.EvaluateConditions() == true)
            {
                Restart(task);
                return TaskStatus.Running;
            }

            TaskStatus status = task.OnUpdate();

            return status;
        }

        #endregion
    }
}