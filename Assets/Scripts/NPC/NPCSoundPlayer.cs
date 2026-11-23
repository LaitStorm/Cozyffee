using System.Collections;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCSoundPlayer : NPCComponent
    {
        [Header("One-shots")]
        [SerializeField, Tooltip("Plays short, one-off sounds like the order call.")]
        AudioSource audioSource;

        [SerializeField]
        AudioClip heyListenClip;

        [Header("Sleep")]
        [SerializeField, Tooltip("Looping source for the purr, separate so stopping it never cuts off a one-shot.")]
        AudioSource sleepAudioSource;

        [SerializeField]
        AudioClip purrClip;

        [SerializeField, Min(0f), Tooltip("Seconds the purr takes to fade out when the NPC wakes up.")]
        float purrFadeOutDuration = 0.75f;

        // The purr source's Inspector volume, restored every time the purr starts
        float purrVolume = 1f;
        Coroutine purrFade;

        protected override void Awake()
        {
            if (sleepAudioSource != null)
            {
                purrVolume = sleepAudioSource.volume;
            }

            base.Awake();
        }

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                Unsubscribe(this.npc);
            }

            base.SetNPC(npc);

            if (npc != null)
            {
                npc.Order += PlayOrderSound;
                npc.FallAsleep += PlayPurr;
                npc.WakeUp += StopPurr;
            }
        }

        void OnDestroy()
        {
            if (npc != null)
            {
                Unsubscribe(npc);
            }
        }

        void Unsubscribe(NPC npc)
        {
            npc.Order -= PlayOrderSound;
            npc.FallAsleep -= PlayPurr;
            npc.WakeUp -= StopPurr;
        }

        public void PlayOrderSound()
        {
            if (audioSource != null && heyListenClip != null)
            {
                audioSource.PlayOneShot(heyListenClip);
            }
        }

        public void PlayPurr()
        {
            if (sleepAudioSource == null || purrClip == null)
            {
                return;
            }

            // Falling asleep again mid fade-out: cancel the fade and start fresh
            CancelPurrFade();

            sleepAudioSource.clip = purrClip;
            sleepAudioSource.loop = true;
            sleepAudioSource.volume = purrVolume;

            // Start at a random point so several NPCs napping together don't purr in sync
            sleepAudioSource.time = Random.Range(0f, purrClip.length);
            sleepAudioSource.Play();
        }

        public void StopPurr()
        {
            if (sleepAudioSource == null || !sleepAudioSource.isPlaying)
            {
                return;
            }

            CancelPurrFade();

            // Coroutines can't run on an inactive object, so just cut the sound
            if (purrFadeOutDuration <= 0f || !isActiveAndEnabled)
            {
                sleepAudioSource.Stop();
                return;
            }

            purrFade = StartCoroutine(FadeOutPurr());
        }

        IEnumerator FadeOutPurr()
        {
            float startVolume = sleepAudioSource.volume;
            float elapsed = 0f;

            while (elapsed < purrFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                sleepAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / purrFadeOutDuration);
                yield return null;
            }

            sleepAudioSource.Stop();
            sleepAudioSource.volume = purrVolume;
            purrFade = null;
        }

        void CancelPurrFade()
        {
            if (purrFade != null)
            {
                StopCoroutine(purrFade);
                purrFade = null;
            }
        }
    }
}
