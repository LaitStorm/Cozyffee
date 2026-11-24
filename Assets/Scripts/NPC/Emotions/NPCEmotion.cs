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
    ///
    /// Reactions (e.g. crying when handed the wrong item) are shown for a few seconds on top of the NPC's emotion
    /// without changing it: <see cref="Current"/> and NPC.GetEmotion keep returning the underlying emotion, and the face
    /// goes back to it afterwards, even if something set a new emotion in the meantime.
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

        [Header("Reactions")]
        [SerializeField, Tooltip("Face shown for a moment when the NPC is handed the wrong item.")]
        Emotion rejectFoodEmotion = Emotion.Crying;

        [SerializeField, Min(0f), Tooltip("How long the reject face stays, in seconds.")]
        float rejectFoodEmotionDuration = 2f;

        /// <summary>The NPC's emotion. A reaction being shown on top of it (see <see cref="ShowReaction"/>) doesn't change it.</summary>
        public Emotion Current { get; private set; }

        /// <summary>The emotion on the NPC's face right now: a reaction if one is showing, otherwise <see cref="Current"/>.</summary>
        public Emotion Shown => reaction ?? Current;

        Emotion? reaction;
        float reactionEndTime;

        // Set by SetEmotionFor: go back to emotionAfterTimer once the timer runs out
        bool hasEmotionTimer;
        Emotion emotionAfterTimer;
        float emotionTimerEnd;

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
                this.npc.RejectFood -= OnRejectFood;
            }

            base.SetNPC(npc);
            npc.SetEmotion = SetEmotion;
            npc.SetEmotionFor = SetEmotionFor;
            npc.GetEmotion = () => Current;
            npc.FallAsleep += OnFallAsleep;
            npc.WakeUp += OnWakeUp;
            npc.RejectFood += OnRejectFood;
            SetEmotion(startingEmotion);
        }

        void OnDestroy()
        {
            if (npc != null)
            {
                npc.FallAsleep -= OnFallAsleep;
                npc.WakeUp -= OnWakeUp;
                npc.RejectFood -= OnRejectFood;
            }
        }

        /// <summary>Sets the emotion until something changes it. Cancels a pending <see cref="SetEmotionFor"/>.</summary>
        public void SetEmotion(Emotion emotion)
        {
            hasEmotionTimer = false;
            ApplyEmotion(emotion);
        }

        /// <summary>
        /// Sets the emotion for <paramref name="duration"/> seconds, then goes back to the one before. Calling it again
        /// while one is pending restarts the timer but still goes back to the original emotion; SetEmotion cancels it.
        /// </summary>
        public void SetEmotionFor(Emotion emotion, float duration)
        {
            if (duration <= 0f)
            {
                SetEmotion(emotion);
                return;
            }

            Emotion previous = hasEmotionTimer ? emotionAfterTimer : Current;

            ApplyEmotion(emotion);

            hasEmotionTimer = true;
            emotionAfterTimer = previous;
            emotionTimerEnd = Time.time + duration;
        }

        void ApplyEmotion(Emotion emotion)
        {
            Current = emotion;

            // A reaction is showing: keep it, and come back to this emotion when it ends
            if (reaction.HasValue)
            {
                return;
            }

            Show(emotion);
        }

        /// <summary>Shows <paramref name="emotion"/> for <paramref name="duration"/> seconds, then goes back to <see cref="Current"/>.</summary>
        public void ShowReaction(Emotion emotion, float duration)
        {
            reaction = emotion;
            reactionEndTime = Time.time + duration;
            Show(emotion);
        }

        void OnRejectFood() => ShowReaction(rejectFoodEmotion, rejectFoodEmotionDuration);

        void Show(Emotion emotion)
        {
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
            Show(Shown);
        }

        void Update()
        {
            if (hasEmotionTimer && Time.time >= emotionTimerEnd)
            {
                hasEmotionTimer = false;
                ApplyEmotion(emotionAfterTimer);
            }

            if (reaction.HasValue && Time.time >= reactionEndTime)
            {
                reaction = null;
                Show(Current);
            }

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
        [ContextMenu("Test Reject Food Reaction")] void TestRejectFood() => OnRejectFood();
    }
}
