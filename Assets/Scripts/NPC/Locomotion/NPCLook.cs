using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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

        [Header("Rigs")]
        [SerializeField]
        Rig SpineRig;

        [SerializeField]
        Transform AimTarget;

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

        #region Rig Manipulation Methods

        void SetSpineRigWeight(float weight)
		{
			if (SpineRig == null) return;

			float smoothing = 5f;

			SpineRig.weight = Mathf.Lerp(SpineRig.weight, weight, smoothing * Time.deltaTime);
		}

        #endregion

        #region Delegates
        void OnLookAt(Vector3 position)
        {
            lookTimer = ExpireTime;
            targetPosition = position;

            SetSpineRigWeight(1.0f);

            if (AimTarget == null) return;

            AimTarget.position = position;
            ClampAimTargetAngle();
        }

        void ClampAimTargetAngle()
		{
			float HorizontalAngleLimit = 15f;

			Vector3 forward = npc.transform.forward;
			Vector3 direction = (AimTarget.position - npc.SensorPosition).normalized;


			Vector3 horizontalDirection = direction;
			horizontalDirection.y = 0f;
			horizontalDirection.Normalize();


			float angle = Vector3.Angle(forward, horizontalDirection);

			if (angle > HorizontalAngleLimit)
			{
				Vector3 clampedDirection = Vector3.RotateTowards(forward, horizontalDirection, Mathf.Deg2Rad * HorizontalAngleLimit, 0f);

				clampedDirection.y = direction.y;
				clampedDirection.Normalize();

				AimTarget.position = npc.SensorPosition + clampedDirection * Vector3.Distance(npc.SensorPosition, AimTarget.position);
			}
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

                SetSpineRigWeight(0f);
            }
            else
            {
                npc.CanTurn = false;
                NPCLookAtPosition(targetPosition);
            }

            Animator animator = npc.GetAnimator();

			if (animator != null)
			{
				animator.SetFloat("Turn", animatorTurnParameter);
			}
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
            forward.y = 0f;
			forward.Normalize();

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