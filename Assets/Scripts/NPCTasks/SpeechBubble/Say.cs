using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class Say : NPCAction
    {
        public SharedString Text = "";

        public Say()
        {
        }

        public Say(SharedString text)
        {
            Text = text;
        }

        protected override TaskStatus OnUpdate()
        {
            if (Text != null)
            {
                npc.Say?.Invoke(Text.Value);
            }

            return TaskStatus.Success;
        }
    }
}