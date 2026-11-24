using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Game.Cozyffee
{
    public class NPCItemReceiver : NPCComponent
    {
        [SerializeField, Tooltip("The NPC's 'hand'.")]
        XRSocketInteractor socket;

        public event System.Action<Item> ItemReceived;

        /// <summary>Fired right before a wrong item is destroyed (see <see cref="NPC.RejectFood"/>).</summary>
        public event System.Action<Item> ItemRejected;

        public Item HeldItem { get; private set; }

        public bool Accepting { get; private set; }

        public string AcceptedItemId { get; private set; } = "";

        ItemFilter filter;

        // Everything hovering the hand right now. If the player lets go of a wrong item here, it counts as handing it over.
        readonly HashSet<IXRSelectInteractable> hovered = new HashSet<IXRSelectInteractable>();

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
            socket.hoverEntered.AddListener(OnHoverEntered);
            socket.hoverExited.AddListener(OnHoverExited);
        }

        private void OnDisable()
        {
            foreach (IXRSelectInteractable interactable in hovered)
            {
                interactable.selectExited.RemoveListener(OnHoveringItemReleased);
            }

            hovered.Clear();

            if (socket == null)
            {
                return;
            }

            socket.selectFilters.Remove(filter);
            socket.selectEntered.RemoveListener(OnSelectEntered);
            socket.selectExited.RemoveListener(OnSelectExited);
            socket.hoverEntered.RemoveListener(OnHoverEntered);
            socket.hoverExited.RemoveListener(OnHoverExited);
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

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (args.interactableObject is IXRSelectInteractable interactable && hovered.Add(interactable))
            {
                interactable.selectExited.AddListener(OnHoveringItemReleased);
            }
        }

        private void OnHoverExited(HoverExitEventArgs args)
        {
            if (args.interactableObject is IXRSelectInteractable interactable && hovered.Remove(interactable))
            {
                interactable.selectExited.RemoveListener(OnHoveringItemReleased);
            }
        }

        // Something hovering the hand was let go of. The filter already stops a wrong item from snapping in;
        // here the NPC reacts to being handed it.
        private void OnHoveringItemReleased(SelectExitEventArgs args)
        {
            // Not the player letting go: the socket itself releasing, or the selection being cancelled (object destroyed...).
            if (args.isCanceled || ReferenceEquals(args.interactorObject, socket))
            {
                return;
            }

            IXRSelectInteractable interactable = args.interactableObject;

            // Still held by the other hand.
            if (interactable.isSelected)
            {
                return;
            }

            Item item = interactable.transform.GetComponentInParent<Item>();

            if (item != null && item != HeldItem && IsWrongItem(item))
            {
                Reject(item);
            }
        }

        private bool IsWrongItem(Item item)
        {
            return Accepting && AcceptedItemId.Length > 0 && item.Id != AcceptedItemId;
        }

        private void Reject(Item item)
        {
            ItemRejected?.Invoke(item);
            Destroy(item.gameObject);

            if (npc != null)
            {
                npc.RejectFood?.Invoke();
            }
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
