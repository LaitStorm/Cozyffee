using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCLook : NPCComponent
    {
        [SerializeField]
        public float turnSpeed = 15f;
        [SerializeField]
        float ExpireTime = 0.2f;
        float lookTimer = 0f;

        Vector3 targetPosition = Vector3.zero;
        float animatorTurnParameter = 0f;

        [Header("Animator Settings"), SerializeField]
        float animatorTurnSmoothing = 5f;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                this.npc.LookAt -= OnLookAt;
                this.npc.IsLookingAt -= OnLookingAt;
            }
            base.SetNPC(npc);
            npc.LookAt += OnLookAt;
            npc.IsLookingAt += OnLookingAt;
        }

        #region Delegates
        void OnLookAt(Vector3 position)
        {
            lookTimer = ExpireTime;
            targetPosition = position;
        }

        bool OnLookingAt(Vector3 position)
        {
            Vector3 direction = (targetPosition - npc.Position);
            direction.y = 0f;
            direction.Normalize();

            Vector3 fwd = npc.transform.forward;
            fwd.y = 0f;
            fwd.Normalize();

            return Vector3.Dot(fwd, direction) > 0.95f;
        }
        #endregion

        #region Unity Methods

        private void LateUpdate()
        {
            lookTimer -= Time.deltaTime;

            if (lookTimer < 0f)
            {
                lookTimer = -1f;
                npc.CanTurn = true;
                //NPCLookForward();

                animatorTurnParameter = Mathf.Lerp(animatorTurnParameter, 0f, animatorTurnSmoothing * Time.deltaTime);
            }
            else
            {
                npc.CanTurn = false;
                NPCLookAtPosition(targetPosition);
            }

            npc.Animator.SetFloat("Turn", animatorTurnParameter);
        }

        #endregion

        void NPCLookAtPosition(Vector3 position)
        {
            Vector3 directionToTarget = position - npc.Position;
            directionToTarget.y = 0f;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            npc.transform.rotation = Quaternion.RotateTowards(
                npc.transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );

            UpdateAnimatorTurnParameter(directionToTarget);
        }

        void UpdateAnimatorTurnParameter(Vector3 targetPosition)
        {
            Vector3 normalizedDirection = targetPosition - npc.transform.position;
            normalizedDirection.y = 0f;
            normalizedDirection.Normalize();

            Vector3 forward = npc.transform.forward;

            float angle = Vector3.SignedAngle(forward, normalizedDirection, Vector3.up);

            float maxAngle = 30f;
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle) / maxAngle;

            animatorTurnParameter = Mathf.Lerp(animatorTurnParameter, angle, animatorTurnSmoothing * Time.deltaTime);
        }

        void NPCLookForward()
        {
            Vector3 pos = npc.transform.position;
            pos += npc.transform.forward * 1000.0f;
            pos += Vector3.up * 1.5f;

            NPCLookAtPosition(pos);
        }
    }
}