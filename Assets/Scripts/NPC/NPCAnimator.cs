using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCAnimator : NPCComponent
    {
        private void Update()
        {
            npc.Animator.SetFloat("Speed", npc.CurrentSpeed);
        }
    }
}
