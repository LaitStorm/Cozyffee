using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class LookAtRandomAngle : NPCAction
    {
        Vector3 target;

        float minAngle = 45f;
        float maxAngle = 180f;

        void ChooseRandomTarget()
        {
            float angle = npc.transform.rotation.eulerAngles.y;
            float sign = Random.Range(0f, 100f) > 50f ? 1f : -1f;
            float deltaAngle = sign * Random.Range(minAngle, maxAngle);

            target = npc.Position + Quaternion.Euler(0f, angle + deltaAngle, 0f) * Vector3.forward * 1000.0f + Vector3.up * 1.5f;
        }

        protected override void OnEntry()
        {
            ChooseRandomTarget();
        }

        protected override TaskStatus OnUpdate()
        {
            npc.LookAt?.Invoke(target);

            if (npc.IsLookingAt(target))
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}