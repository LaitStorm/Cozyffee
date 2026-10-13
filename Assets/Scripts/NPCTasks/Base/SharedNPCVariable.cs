using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class SharedNPCVariable : SharedVariable<NPC>
	{
		public static implicit operator SharedNPCVariable(NPC value)
		{
			return new SharedNPCVariable { Value = value };
		}
	}
}