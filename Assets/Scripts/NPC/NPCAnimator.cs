using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCAnimator : NPCComponent
    {
        private void Update()
        {
            float forwardSpeed = Vector3.Dot(npc.Velocity, npc.transform.forward);
            float rightSpeed = Vector3.Dot(npc.Velocity, npc.transform.right);

            npc.Animator.SetFloat("Speed", forwardSpeed);
            npc.Animator.SetFloat("StrafeSpeed", rightSpeed);
        }
    }
}
