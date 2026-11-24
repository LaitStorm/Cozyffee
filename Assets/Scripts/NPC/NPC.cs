using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Cozyffee
{
    [SelectionBase]
    // [RequireComponent(typeof(Animator))]

    public class NPC : MonoBehaviour
    {
        public bool CanMove = true;
        public bool CanTurn = true;

        public float WalkSpeed = 3.5f;
        public float RunSpeed = 7f;

        public bool Alive = true;
        public Group Group = null;

        public Vector3 Velocity = Vector3.zero;
        public Vector3 Direction {get; private set; } = Vector3.forward;
        public Vector3 Position => transform.position;

        #region Sensors

        public NPCSensor Sensor;

        public Vector3 SensorPosition
        {
            get
            {
                return transform.position + Vector3.up * 1.5f;
            }
        }

        public void MakeNoise(NoiseInfo noiseInfo)
        {
            noiseInfo.Owner = this;

            NoiseSystem.Instance?.MakeNoise(noiseInfo);
        }

        #endregion

        [Header("NPC Components")]
        // public Animator Animator;
        public System.Func<Animator> GetAnimator;

        #region Hunger

        public IHunger Hunger;

        /// <summary>Fired by the EatItem task when the NPC eats an item it was given.</summary>
        public UnityAction Eat;

        /// <summary>Fired by the AnnounceOrder task when the NPC reaches its order spot and asks for its item.</summary>
        public UnityAction Order;

        /// <summary>Fired by OrderScope whenever an order ends: served, timed out or interrupted.</summary>
        public UnityAction OrderEnded;

        /// <summary>
        /// Fired by <see cref="NPCItemReceiver"/> when the NPC takes the item it ordered (the item has just been destroyed).
        /// Listeners play the reaction (particles, sound). The order then plays the Eating animation and cheers the NPC up.
        /// </summary>
        public UnityAction AcceptFood;

        /// <summary>
        /// Fired by <see cref="NPCItemReceiver"/> when, while ordering, the player lets go of the wrong item in the NPC's hand.
        /// The item has already been destroyed; listeners play the reaction (particles, sound, crying face).
        /// </summary>
        public UnityAction RejectFood;

        /// <summary>What this NPC is currently waiting for, or null when it isn't ordering. Set by the PickOrder task.</summary>
        [System.NonSerialized]
        public CafeOrder CurrentOrder;

        #endregion

        #region Sleep

        /// <summary>Fired by the Sleep task when the NPC falls asleep.</summary>
        public UnityAction FallAsleep;

        /// <summary>Fired by the Sleep task when the NPC wakes up: nap finished or interrupted.</summary>
        public UnityAction WakeUp;

        #endregion

        #region Animation

        /// <summary>Plays an Animator state once (e.g. "Ordering"), then goes back to the usual state.</summary>
        public UnityAction<string> PlayAnimation;

        /// <summary>True while an animation started with <see cref="PlayAnimation"/> is still playing its single cycle.</summary>
        public System.Func<bool> IsPlayingAnimation;

        #endregion

        #region Animator Booleans

        public bool IsOrdering = false;
        public bool IsEating = false;
        public bool IsSleeping = false;

        #endregion

        #region Locomotion Delegates
        public UnityAction<Vector3> SetDestination;
        public UnityAction<float> SetMaxSpeed;
        public System.Func<bool> HasPathToDestination;
        public UnityAction<Vector3> LookAt;
        public System.Func<Vector3, bool> IsLookingAt;

        /// <summary>True while the NPC is holding a spot (HoldPosition task): other NPCs should steer around it.</summary>
        public UnityAction<bool> SetHoldingPosition;
        #endregion

        #region Emotion Delegates

        public UnityAction<Emotion> SetEmotion;

        /// <summary>Sets the emotion for some seconds, then goes back to the one the NPC had before.</summary>
        public UnityAction<Emotion, float> SetEmotionFor;
        public System.Func<Emotion> GetEmotion;

        #endregion

        #region Other Unity Actions
        public UnityAction<string> Say;
        public UnityAction<Sprite, float> SayImage;

        /// <summary>Like <see cref="SayImage"/>, with more sprites drawn on top of the first (e.g. an ice cream's toppings).</summary>
        public UnityAction<Sprite, IReadOnlyList<Sprite>, float> SayLayeredImage;
        #endregion

        #region UnityCallbacks

        private void Awake()
        {
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