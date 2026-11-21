// Assets/Scripts/NPC/Emotions/NPCEmotion.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCEmotion : NPCComponent
    {
        [Serializable]
        public struct EmotionMaterial
        {
            public Emotion Emotion;
            public Material Material;
        }

        [SerializeField] Renderer bodyRenderer;          // the SkinnedMeshRenderer using Squirrel_Body_*
        [SerializeField] int materialIndex = 0;          // body renderer has a single slot
        [SerializeField] Emotion startingEmotion = Emotion.Neutral;
        [SerializeField] List<EmotionMaterial> materials = new();

        public Emotion Current { get; private set; }

        public override void SetNPC(NPC npc)
        {
            base.SetNPC(npc);
            npc.SetEmotion = SetEmotion;
            npc.GetEmotion = () => Current;
            SetEmotion(startingEmotion);
        }

        public void SetEmotion(Emotion emotion)
        {
            Current = emotion;

            Material mat = materials.Find(m => m.Emotion == emotion).Material;
            if (mat == null || bodyRenderer == null) return;

            // sharedMaterials: assigns the existing asset, no per-NPC material copy is created
            Material[] mats = bodyRenderer.sharedMaterials;
            mats[materialIndex] = mat;
            bodyRenderer.sharedMaterials = mats;
        }

        [ContextMenu("Test Happy")]  void TestHappy()  => SetEmotion(Emotion.Happy);
        [ContextMenu("Test Crying")] void TestCrying() => SetEmotion(Emotion.Crying);
        [ContextMenu("Test Angry")]  void TestAngry()  => SetEmotion(Emotion.Angry);
    }
}
