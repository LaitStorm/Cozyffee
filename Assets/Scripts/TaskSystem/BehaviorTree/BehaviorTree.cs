using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class BehaviorTree : MonoBehaviour
    {
        [SerializeField]
        bool RestartWhenDone = true;

        [SerializeField]
        ExternalBehaviorTree externalBehaviorTree;

        public BehaviorTreeData Data = new();
        bool hasAwaken = false;
        public Task Root;
        internal int Restarts = 0;

        private void Awake()
        {
            hasAwaken = false;
        }

        private void Update()
        {
            if (!hasAwaken)
            {
                AwakenRootTask();
                hasAwaken = true;
            }

            if (Root == null)
            {
                return;
            }
            if (Root != null && Root.Status == TaskStatus.Inactive)
            {
                return;
            }

            TaskStatus status = Task.Update(Root);

            if (status == TaskStatus.Success || status == TaskStatus.Failure)
            {
                if (RestartWhenDone)
                {
                    Task.Restart(Root);
                    Restarts++;
                }
                else
                {
                    Task.Stop(Root);
                }
            }
        }

        internal void CreateBehaviorTreeInternal()
        {
            if (externalBehaviorTree != null)
            {
                Root = externalBehaviorTree.CreateBehaviorTree();
                externalBehaviorTree.CreateData(Data);
            }
        }

        private void AwakenRootTask()
        {
            if (externalBehaviorTree != null)
            {
                Root = externalBehaviorTree.CreateBehaviorTree();
                externalBehaviorTree.CreateData(Data);
            }

            if (Root == null)
            {
                Debug.LogError($"Behavior Tree in object '{gameObject.name}' has a null root task.");
                return;
            }

            Root.SetOwner(gameObject);
            Task.Awaken(Root);
            Task.Start(Root);
        }
    }
}