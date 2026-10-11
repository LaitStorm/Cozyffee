using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Cozyffee
{
    [SelectionBase]
    [RequireComponent(typeof(Animator))]

    public class NPC : MonoBehaviour
    {
        public bool CanMove = true;

        public float WalkSpeed = 3.5f;
        public float RunSpeed = 7f;

        public bool Alive = true;
        public Group Group = null;

        public Vector3 Velocity = Vector3.zero;
        public Vector3 Direction {get; private set; } = Vector3.forward;
        public Vector3 Position => transform.position;

        public Vector3 SensorPosition
        {
            get
            {
                return transform.position + Vector3.up * 1.5f;
            }
        }

        [Header("NPC Components")]
        public Animator Animator;

        #region Events
        public UnityAction<Vector3> SetDestination;
        public UnityAction<float> SetMaxSpeed;
        #endregion

        #region UnityCallbacks

        private void Awake()
        {
            if (Animator == null)
            {
                Animator = GetComponent<Animator>();
            }
            if (Animator == null)
            {
                Debug.LogError($"Animator not found in '{gameObject.name}'");
            }
        }

        private void LateUpdate()
        {
            Vector3 horizontalVelocity = new Vector3(Velocity.x, 0f, Velocity.z);

            if (horizontalVelocity.sqrMagnitude > 3f)
            {
                Direction = horizontalVelocity.normalized;
            }
        }

        #endregion
    }
}