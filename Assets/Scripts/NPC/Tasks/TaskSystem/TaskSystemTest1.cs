using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    public class TaskSystemTest1 : MonoBehaviour
    {
        Task root;

        void Start()
        {
            Sequence sequence = new Sequence();

            Log log = new Log("Hewwo Fwendy Wends!");
            Wait wait = new Wait(1f);

            sequence.CreateTasks(log, wait);
            Repeater repeater = new Repeater(sequence);

            root = repeater;

            root.OnEntry();
        }

        private void Update()
        {
            root.OnUpdate();
        }
    }
}