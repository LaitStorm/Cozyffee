using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
	public class SayImage : NPCAction
	{
		public SharedSprite Image = new SharedSprite();

		public float Duration = 0f;

		bool hasWarned;

		public SayImage()
		{
		}

		public SayImage(SharedSprite image, float duration = 0f)
		{
			Image = image;
			Duration = duration;
		}

		protected override TaskStatus OnUpdate()
		{
			Sprite sprite = Image?.Value;

			if (sprite == null)
			{
				if (!hasWarned)
				{
					Debug.LogWarning($"{nameof(SayImage)}: no Image assigned.");
					hasWarned = true;
				}

				return TaskStatus.Failure;
			}

			npc.SayImage?.Invoke(sprite, Duration);

			return TaskStatus.Success;
		}
	}
}