using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCSpeechBubble : NPCComponent
    {
        [SerializeField]
        TypingEffect typingEffect;

        [SerializeField]
        GameObject Root;

        [SerializeField]
        float typingDuration = 1f;

        [SerializeField]
        float visibleDuration = 3f;

        float visibleTimer = 0f;

        void OnSay(string text)
        {
            visibleTimer = visibleDuration;

            Root.SetActive(true);
            typingEffect.StartTyping(text, typingDuration);
        }

        private void Start()
        {
            Root.SetActive(false);

            npc.Say += OnSay;
        } 

        private void Update()
        {
            if (visibleTimer <= 0.01f)
            {
                visibleTimer = 0f;
                Root.SetActive(false);
            }
            else
            {
                visibleTimer -= Time.deltaTime;

                Root.SetActive(true);

                if (Camera.main != null)
                {
                    Root.transform.forward = Camera.main.transform.forward;
                }
            }
        }
    }
}