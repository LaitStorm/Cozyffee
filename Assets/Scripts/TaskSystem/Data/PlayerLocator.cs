using Unity.XR.CoreUtils;
using UnityEngine;

namespace Game.Cozyffee
{
	public static class PlayerLocator
	{
		const float RetryInterval = 10.0f;
		static Transform head;
		static float nextSearchTime;

		public static Transform Head
		{
			get
			{
				if (head == null && Time.unscaledTime >= nextSearchTime)
				{
					head = FindHead();
					nextSearchTime = Time.unscaledTime + RetryInterval;
				}

				return head;
			}
		}

		static Transform FindHead()
		{
			XROrigin origin = Object.FindAnyObjectByType<XROrigin>();

			if (origin != null && origin.Camera != null)
			{
				return origin.Camera.transform;
			}

			return Camera.main != null ? Camera.main.transform : null;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics()
		{
			head = null;
			nextSearchTime = 0f;
		}
	}
}