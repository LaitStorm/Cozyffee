using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee
{
    public class SayRandom : NPCAction
    {
        string[] sentences;

        public SayRandom(params string[] sentences)
        {
            this.sentences = (sentences == null) ? new string[0] : sentences;
        }

        protected override TaskStatus OnUpdate()
        {
            string sentence = sentences[Random.Range(0, sentences.Length)];

            npc.Say?.Invoke(sentence);

            return TaskStatus.Success;
        }
    }
}