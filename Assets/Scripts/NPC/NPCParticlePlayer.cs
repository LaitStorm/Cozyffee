using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCParticlePlayer : NPCComponent
    {
        [SerializeField]
        ParticleSystem orderParticles;

        [SerializeField]
        ParticleSystem sleepParticles;

        [SerializeField, Tooltip("Played once when the NPC is handed the wrong item (NPC.RejectFood).")]
        ParticleSystem rejectParticles;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                Unsubscribe(this.npc);
            }

            base.SetNPC(npc);

            if (npc != null)
            {
                npc.Order += PlayOrderParticles;
                npc.OrderEnded += StopOrderParticles;
                npc.FallAsleep += PlaySleepParticles;
                npc.WakeUp += StopSleepParticles;
                npc.RejectFood += PlayRejectParticles;
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
            npc.Order -= PlayOrderParticles;
            npc.OrderEnded -= StopOrderParticles;
            npc.FallAsleep -= PlaySleepParticles;
            npc.WakeUp -= StopSleepParticles;
            npc.RejectFood -= PlayRejectParticles;
        }

        public void PlayOrderParticles() => Play(orderParticles);
        public void StopOrderParticles() => Stop(orderParticles);

        public void PlaySleepParticles() => Play(sleepParticles);
        public void StopSleepParticles() => Stop(sleepParticles);

        public void PlayRejectParticles() => Play(rejectParticles);

        static void Play(ParticleSystem particles)
        {
            if (particles != null)
            {
                particles.Play();
            }
        }

        static void Stop(ParticleSystem particles)
        {
            // StopEmitting lets particles already on screen fade out instead of vanishing
            if (particles != null)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
