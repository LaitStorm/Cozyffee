using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCAnimator : NPCComponent
    {
        private string targetStateName = "";

        [SerializeField]
        Animator animator;

        public override void SetNPC(NPC npc)
        {
            base.SetNPC(npc);

            npc.GetAnimator = () => animator;

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError($"Animator required in object {name}");
            }
        }

        void ChangeState(string stateName, float transitionDuration = 0.1f)
        {
            if (targetStateName == stateName)
            {
                return;
            }

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

            if (currentState.IsName(stateName))
            {
                return;
            }

            animator.CrossFadeInFixedTime(stateName, transitionDuration, 0);

            targetStateName = stateName;
        }

        private void Update()
        {
            float forwardSpeed = Vector3.Dot(npc.Velocity, npc.transform.forward);
            float rightSpeed = Vector3.Dot(npc.Velocity, npc.transform.right);

            animator.SetFloat("Speed", forwardSpeed);
            animator.SetFloat("StrafeSpeed", rightSpeed);

            if (npc.IsEating)
            {
                ChangeState("Eating");
            }
            else if (npc.IsOrdering)
            {
                ChangeState("Ordering");
            }
            else if (npc.IsSleeping)
            {
                ChangeState("Sleep");
            }
            else
            {
                ChangeState("Stand");
            }
        }
    }
}
