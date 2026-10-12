using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Cozyffee
{
	public class Area : MonoBehaviour
	{
		public float Radius = 20f;

		public Vector3 GetRandomPoint()
		{
			Vector3 randomDirection = Random.insideUnitSphere * Radius;
			randomDirection.y = 0f;
			Vector3 randomPoint = transform.position + randomDirection;
			NavMeshHit hit;
			Vector3 finalPosition = transform.position;

			if (NavMesh.SamplePosition(randomPoint, out hit, 2f, 1))
			{
				finalPosition = hit.position;
			}

			return finalPosition;
		}
	}
}
