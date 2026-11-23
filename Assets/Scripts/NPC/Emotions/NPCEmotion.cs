// Assets/Scripts/NPC/Emotions/NPCEmotion.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    /// <summary>
    /// Shows the NPC's emotion by swapping a material on <see cref="bodyRenderer"/>.
    /// If an emotion has a <see cref="EmotionMaterial.BlinkMaterial"/>, the NPC also blinks:
    /// every so often it swaps to that material for <see cref="blinkDuration"/> seconds.
    /// Emotions without one (e.g. the squirrel bodies) never blink.
    /// While the NPC sleeps it shows the Neutral emotion's BlinkMaterial (eyes closed), if there is one.
    /// </summary>
    public class NPCEmotion : NPCComponent
    {
        [Serializable]
        public struct EmotionMaterial
        {
            public Emotion Emotion;
            public Material Material;

            [Tooltip("Optional. Shown briefly while blinking. Leave empty for no blinking.")]
            public Material BlinkMaterial;
        }

        [SerializeField, Tooltip("Renderer whose material shows the emotion (squirrel body, axolotl face, ...).")]
        Renderer bodyRenderer;

        [SerializeField, Tooltip("Which material slot on the renderer to swap.")]
        int materialIndex = 0;

        [SerializeField] Emotion startingEmotion = Emotion.Neutral;
        [SerializeField] List<EmotionMaterial> materials = new();

        [Header("Blinking")]
        [SerializeField, Min(0.1f), Tooltip("Shortest time between blinks, in seconds.")]
        float minBlinkInterval = 2.5f;

        [SerializeField, Min(0.1f), Tooltip("Longest time between blinks, in seconds.")]
        float maxBlinkInterval = 5f;

        [SerializeField, Min(0.01f), Tooltip("How long the eyes stay closed, in seconds.")]
        float blinkDuration = 0.15f;

        public Emotion Current { get; private set; }

        EmotionMaterial current;
        bool isBlinking;
        float nextBlinkChange;
        bool isAsleep;

        public override void SetNPC(NPC npc)
        {
            if (this.npc != null)
            {
                this.npc.FallAsleep -= OnFallAsleep;
                this.npc.WakeUp -= OnWakeUp;
            }

            base.SetNPC(npc);
            npc.SetEmotion = SetEmotion;
            npc.GetEmotion = () => Current;
            npc.FallAsleep += OnFallAsleep;
            npc.WakeUp += OnWakeUp;
            SetEmotion(startingEmotion);
        }

        void OnDestroy()
        {
            if (npc != null)
            {
                npc.FallAsleep -= OnFallAsleep;
                npc.WakeUp -= OnWakeUp;
            }
        }

        public void SetEmotion(Emotion emotion)
        {
            Current = emotion;
            current = materials.Find(m => m.Emotion == emotion);

            // Asleep: remember the emotion, show it once the NPC wakes up
            if (isAsleep)
            {
                return;
            }

            // A new emotion always starts with open eyes
            isBlinking = false;
            ScheduleNextBlink();
            ApplyMaterial(current.Material);
        }

        void OnFallAsleep()
        {
            isAsleep = true;
            isBlinking = false;

            // Eyes closed: the neutral face's blink frame. Characters without one keep their current face.
            ApplyMaterial(materials.Find(m => m.Emotion == Emotion.Neutral).BlinkMaterial);
        }

        void OnWakeUp()
        {
            isAsleep = false;
            SetEmotion(Current);
        }

        void Update()
        {
            if (isAsleep || current.BlinkMaterial == null || Time.time < nextBlinkChange)
            {
                return;
            }

            isBlinking = !isBlinking;

            if (isBlinking)
            {
                ApplyMaterial(current.BlinkMaterial);
                nextBlinkChange = Time.time + blinkDuration;
            }
            else
            {
                ApplyMaterial(current.Material);
                ScheduleNextBlink();
            }
        }

        void ScheduleNextBlink()
        {
            // Random interval so a group of NPCs doesn't blink in sync
            nextBlinkChange = Time.time + UnityEngine.Random.Range(minBlinkInterval, Mathf.Max(minBlinkInterval, maxBlinkInterval));
        }

        void ApplyMaterial(Material mat)
        {
            if (mat == null || bodyRenderer == null) return;

            // sharedMaterials: assigns the existing asset, no per-NPC material copy is created
            Material[] mats = bodyRenderer.sharedMaterials;
            mats[materialIndex] = mat;
            bodyRenderer.sharedMaterials = mats;
        }

        [ContextMenu("Test Neutral")] void TestNeutral() => SetEmotion(Emotion.Neutral);
        [ContextMenu("Test Happy")]   void TestHappy()   => SetEmotion(Emotion.Happy);
        [ContextMenu("Test Crying")]  void TestCrying()  => SetEmotion(Emotion.Crying);
        [ContextMenu("Test Angry")]   void TestAngry()   => SetEmotion(Emotion.Angry);
    }
}
