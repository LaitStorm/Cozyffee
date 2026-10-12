using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
	public class NPCComponent : MonoBehaviour
	{
		protected NPC npc;

		protected virtual void Awake()
		{
			SetNPC(GetComponentInParent<NPC>());
		}

		public virtual void SetNPC(NPC npc)
		{
			this.npc = npc;
		}
	}
}