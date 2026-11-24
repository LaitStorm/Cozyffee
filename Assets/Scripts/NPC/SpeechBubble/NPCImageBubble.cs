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

        [SerializeField, Tooltip("How far in front of the image each overlay (e.g. an ice cream topping) is drawn, " +
                                 "so it renders on top. Overlay renderers are created at runtime as copies of the Image Renderer.")]
        float overlaySpacing = 0.001f;

        readonly List<SpriteRenderer> overlayRenderers = new List<SpriteRenderer>();

        protected override void Subscribe()
        {
            npc.SayImage += OnSayImage;
            npc.SayLayeredImage += OnSayLayeredImage;
        }

        void OnSayImage(Sprite sprite, float duration)
        {
            OnSayLayeredImage(sprite, null, duration);
        }

        void OnSayLayeredImage(Sprite sprite, IReadOnlyList<Sprite> overlays, float duration)
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
            ShowOverlays(overlays);
            Show(duration > 0f ? duration : visibleDuration);
        }

        void ShowOverlays(IReadOnlyList<Sprite> overlays)
        {
            int count = overlays?.Count ?? 0;

            while (overlayRenderers.Count < count)
            {
                overlayRenderers.Add(CreateOverlayRenderer(overlayRenderers.Count));
            }

            for (int i = 0; i < overlayRenderers.Count; i++)
            {
                SpriteRenderer overlay = overlayRenderers[i];
                bool used = i < count && overlays[i] != null;

                overlay.sprite = used ? overlays[i] : null;
                overlay.gameObject.SetActive(used);
            }
        }

        // A copy of the image renderer, nudged toward the viewer. The bubble shader ignores depth, so draw order comes
        // from distance to the camera: the closer overlay draws last. Using sortingOrder instead would also draw this
        // NPC's overlays over a nearer NPC's bubble.
        SpriteRenderer CreateOverlayRenderer(int index)
        {
            var overlayObject = new GameObject($"Overlay {index + 1}");
            overlayObject.layer = imageRenderer.gameObject.layer;

            Transform overlayTransform = overlayObject.transform;
            overlayTransform.SetParent(imageRenderer.transform, false);
            overlayTransform.localPosition = new Vector3(0f, 0f, -overlaySpacing * (index + 1));

            var overlay = overlayObject.AddComponent<SpriteRenderer>();
            overlay.sharedMaterial = imageRenderer.sharedMaterial;
            overlay.sortingLayerID = imageRenderer.sortingLayerID;
            overlay.sortingOrder = imageRenderer.sortingOrder;
            overlay.color = imageRenderer.color;
            overlay.flipX = imageRenderer.flipX;
            overlay.flipY = imageRenderer.flipY;
            overlay.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            overlay.receiveShadows = false;

            return overlay;
        }
    }
}
