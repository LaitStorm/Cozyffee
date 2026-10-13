using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCImageBubble : NPCBubble
    {
        [Header("Image bubble")]
        [SerializeField, Tooltip("Renders the PNG.")]
        SpriteRenderer imageRenderer;

        protected override void Subscribe()
        {
            npc.SayImage += OnSayImage;
        }

        void OnSayImage(Sprite sprite, float duration)
        {
            if (sprite == null)
            {
                return;
            }

            if (imageRenderer == null)
            {
                Debug.LogError($"Image bubble in '{gameObject.name}' has no Image Renderer assigned.");
                return;
            }

            imageRenderer.sprite = sprite;
            Show(duration > 0f ? duration : visibleDuration);
        }
    }
}