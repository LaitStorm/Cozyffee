using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class Decorator : Task
    {
        protected Task child;

        public Task Child => child;

        public Decorator(Task childTask)
        {
            child = childTask;
        }

        public override void SetOwner(GameObject owner)
        {
            base.SetOwner(owner);
            Child.SetOwner(owner);
        }

        protected override void OnAwake()
        {
            Task.Awaken(child);
        }

        protected override void OnEntry()
        {
            Task.Start(child);
        }

        protected override void OnExit()
        {
           if (child.Active)
            {
                Task.Stop(child);
            }
        }
    }
}