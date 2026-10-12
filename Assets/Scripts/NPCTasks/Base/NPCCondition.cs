using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public abstract class NPCCondition : Condition
	{
		protected NPC npc;
		protected NPCSensor sensor;

		protected override void OnAwake()
		{
			npc = Owner.GetComponentInParent<NPC>();
			sensor = Owner.GetComponentInParent<NPCSensor>();
		}
	}

}