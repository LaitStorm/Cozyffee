using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public abstract class NPCTask : Task
    {
        protected NPC npc;
        public NPCTask(NPC npc)
        {
            this.npc = npc;
        }
    }
}