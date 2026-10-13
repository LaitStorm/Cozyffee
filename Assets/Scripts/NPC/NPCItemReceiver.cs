using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Game.Cozyffee
{
    public class NPCItemReceiver : MonoBehaviour
    {
        [SerializeField, Tooltip("The NPC's 'hand'.")]
        XRSocketInteractor socket;

        public event System.Action<Item> ItemReceived;

        public Item HeldItem { get; private set; }

        public bool Accepting { get; private set; }

        public string AcceptedItemId { get; private set; } = "";

        ItemFilter filter;

        public void Open(string itemId = "")
        {
            AcceptedItemId = itemId ?? "";
            Accepting = true;
        }

        public void Close()
        {
            Accepting = false;
        }

        public void Consume()
        {
            if (HeldItem == null)
            {
                return;
            }

            Item item = HeldItem;
            HeldItem = null;
            Destroy(item.gameObject);
        }

        private void OnEnable()
        {
            if (socket == null)
            {
                socket = GetComponentInChildren<XRSocketInteractor>();
            }

            if (socket == null)
            {
                Debug.LogError($"{nameof(NPCItemReceiver)} in '{gameObject.name}' needs an XR Socket Interactor (on this object or a child).");
                return;
            }

            filter ??= new ItemFilter(this);
            socket.selectFilters.Add(filter);
            socket.selectEntered.AddListener(OnSelectEntered);
            socket.selectExited.AddListener(OnSelectExited);
        }

        private void OnDisable()
        {
            if (socket == null)
            {
                return;
            }

            socket.selectFilters.Remove(filter);
            socket.selectEntered.RemoveListener(OnSelectEntered);
            socket.selectExited.RemoveListener(OnSelectExited);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            Item item = args.interactableObject.transform.GetComponentInParent<Item>();

            if (item == null)
            {
                return;
            }

            HeldItem = item;
            ItemReceived?.Invoke(item);
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            // The socket holds one item at a time.
            HeldItem = null;
        }

        private bool CanReceive(IXRSelectInteractable interactable)
        {
            Item item = interactable.transform.GetComponentInParent<Item>();

            // Only things marked as Items can be given to an NPC.
            if (item == null)
            {
                return false;
            }

            // Never reject the item we already hold.
            if (item == HeldItem)
            {
                return true;
            }

            if (!Accepting)
            {
                return false;
            }

            return AcceptedItemId.Length == 0 || item.Id == AcceptedItemId;
        }

        // Stops the socket from taking items the NPC didn't ask for (or when it isn't asking at all).
        private sealed class ItemFilter : IXRSelectFilter
        {
            private readonly NPCItemReceiver receiver;

            public ItemFilter(NPCItemReceiver receiver)
            {
                this.receiver = receiver;
            }

            public bool canProcess => receiver != null && receiver.isActiveAndEnabled;

            public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
            {
                return receiver.CanReceive(interactable);
            }
        }
    }
}