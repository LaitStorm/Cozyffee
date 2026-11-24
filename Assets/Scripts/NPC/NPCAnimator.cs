using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCAnimator : NPCComponent
    {
        const string StandState = "Stand";
        const string SleepState = "Sleep";

        private string targetStateName = "";

        [SerializeField]
        Animator animator;

        [Header("Sleep")]
        [SerializeField, Min(0f), Tooltip("Playback speed of the Sleep animation (1 = normal, lower = slower).")]
        float sleepAnimationSpeed = 1f;

        [Header("Play Once")]
        [SerializeField, Min(0f), Tooltip("Cross-fade time, in seconds, into and out of animations played once (e.g. Ordering).")]
        float playOnceTransition = 0.2f;

        // The state being played once via npc.PlayAnimation, or null
        string playOnceState;
        bool playOnceEntered;
        float playOnceGiveUpTime;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                this.npc.PlayAnimation -= PlayOnce;
            }

            base.SetNPC(npc);

            npc.GetAnimator = () => animator;
            npc.PlayAnimation += PlayOnce;
            npc.IsPlayingAnimation = () => playOnceState != null;

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError($"Animator required in object {name}");
            }
        }

        void OnDestroy()
        {
            if (npc != null)
            {
                npc.PlayAnimation -= PlayOnce;
            }
        }

        /// <summary>Plays <paramref name="stateName"/> from the start once, then returns to the usual state.</summary>
        public void PlayOnce(string stateName)
        {
            if (animator == null || string.IsNullOrEmpty(stateName))
            {
                return;
            }

            if (!animator.HasState(0, Animator.StringToHash(stateName)))
            {
                Debug.LogWarning($"{name}: Animator has no state '{stateName}' to play.", this);
                return;
            }

            playOnceState = stateName;
            playOnceEntered = false;
            // Safety net in case the cross-fade never lands on the state
            playOnceGiveUpTime = Time.time + playOnceTransition + 0.5f;

            animator.CrossFadeInFixedTime(stateName, playOnceTransition, 0, 0f);
            targetStateName = stateName;
        }

        /// <summary>True while a play-once animation still owns the Animator.</summary>
        bool UpdatePlayOnce()
        {
            if (playOnceState == null)
            {
                return false;
            }

            // Eating and sleeping take over immediately
            if (npc.IsEating || npc.IsSleeping)
            {
                playOnceState = null;
                return false;
            }

            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);

            if (!animator.IsInTransition(0) && current.IsName(playOnceState))
            {
                playOnceEntered = true;

                if (current.normalizedTime >= 1f)
                {
                    playOnceState = null;
                    return false;
                }

                return true;
            }

            // Still fading in, unless something else took over after it started
            if (playOnceEntered || (!animator.IsInTransition(0) && Time.time > playOnceGiveUpTime))
            {
                playOnceState = null;
                return false;
            }

            return true;
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
                targetStateName = stateName;
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

            if (UpdatePlayOnce())
            {
                animator.speed = 1f;
                return;
            }

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
                ChangeState(StandState, playOnceTransition);
            }

            animator.speed = targetStateName == SleepState ? sleepAnimationSpeed : 1f;
        }
    }
}
