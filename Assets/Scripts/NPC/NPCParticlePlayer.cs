using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCParticlePlayer : NPCComponent
    {
        [SerializeField]
        ParticleSystem orderParticles;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                this.npc.Order -= PlayOrderParticles;
                this.npc.OrderEnded -= StopOrderParticles;
            }

            base.SetNPC(npc);

            if (npc != null)
            {
                npc.Order += PlayOrderParticles;
                npc.OrderEnded += StopOrderParticles;
            }
        }

        void OnDestroy()
        {
            if (npc != null)
            {
                npc.Order -= PlayOrderParticles;
                npc.OrderEnded -= StopOrderParticles;
            }
        }

        public void PlayOrderParticles()
        {
            orderParticles.Play();
        }

        public void StopOrderParticles()
        {
            // StopEmitting lets particles already on screen fade out instead of vanishing
            orderParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}