using UnityEngine;

namespace Game.Cozyffee
{
    public abstract class NPCBubble : NPCComponent
    {
        [Header("Bubble")]
        [SerializeField, Tooltip("The object shown while the bubble is visible.")]
        protected GameObject Root;

        [SerializeField, Tooltip("How long the bubble stays visible, in seconds.")]
        protected float visibleDuration = 3f;

        [SerializeField, Tooltip("Drag the Main Camera from your XR Origin here.")]
        Transform playerCamera;

        float visibleTimer = 0f;

        protected abstract void Subscribe();

        protected virtual void Start()
        {
            if (Root == null)
            {
                Debug.LogError($"{GetType().Name} in '{gameObject.name}' has no Root assigned.");
                enabled = false;
                return;
            }

            Root.SetActive(false);

            if (playerCamera == null)
            {
                playerCamera = PlayerLocator.Head;
            }

            Subscribe();
        }

        protected virtual void Update()
        {
            if (visibleTimer <= 0f)
            {
                if (Root.activeSelf)
                {
                    Root.SetActive(false);
                }

                return;
            }

            visibleTimer -= Time.deltaTime;
            FaceCamera();
        }

        protected void Show(float duration)
        {
            foreach (NPCBubble other in npc.GetComponentsInChildren<NPCBubble>(true))
            {
                if (other != this)
                {
                    other.Hide();
                }
            }

            visibleTimer = duration;
            Root.SetActive(true);

            FaceCamera();
        }

        public void Hide()
        {
            visibleTimer = 0f;

            if (Root != null)
            {
                Root.SetActive(false);
            }
        }

        void FaceCamera()
        {
            if (playerCamera == null)
            {
                playerCamera = PlayerLocator.Head;
            }

            if (playerCamera != null)
            {
                Root.transform.forward = playerCamera.forward;
            }
        }
    }
}