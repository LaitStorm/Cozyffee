using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
	/// <summary>
	/// Shows <see cref="NPC.CurrentOrder"/> in the image bubble: its icon plus any overlays (e.g. ice cream toppings).
	/// Fails if the NPC has no current order or the order has no icon, so a fallback (SayImage, Say) can run instead.
	/// </summary>
	public class SayOrder : NPCAction
	{
		public float Duration = 0f;

		public SayOrder()
		{
		}

		public SayOrder(float duration)
		{
			Duration = duration;
		}

		protected override TaskStatus OnUpdate()
		{
			CafeOrder order = npc != null ? npc.CurrentOrder : null;

			if (order == null || order.Icon == null)
			{
				return TaskStatus.Failure;
			}

			npc.SayLayeredImage?.Invoke(order.Icon, order.Overlays, Duration);

			return TaskStatus.Success;
		}
	}
}
