using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCSpeechBubble : NPCBubble
    {
        [Header("Text bubble")]
        [SerializeField]
        TypingEffect typingEffect;

        [SerializeField]
        float typingDuration = 1f;

        protected override void Subscribe()
        {
            npc.Say += OnSay;
        }

        void OnSay(string text)
        {
            Show(visibleDuration);
            typingEffect.StartTyping(text, typingDuration);
        }
    }
}