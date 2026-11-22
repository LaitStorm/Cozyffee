using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class OrderSoundPlayer : NPCComponent
    {
        [SerializeField]
        AudioSource audioSource;

        [SerializeField]
        AudioClip heyListenClip;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                this.npc.Order -= PlayOrderSound;
            }

            base.SetNPC(npc);

            if (npc != null)
            {
                npc.Order += PlayOrderSound;
            }
        }

        void Destroy()
        {
            if (this.npc != null)
            {
                this.npc.Order -= PlayOrderSound;
            }
        }

        public void PlayOrderSound()
        {
            audioSource.PlayOneShot(heyListenClip);
        }
    }
}