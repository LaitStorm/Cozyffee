using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Picks a random item from <see cref="Menu"/>, creates this NPC's order from it (see <see cref="CafeMenuItem.CreateOrder"/>)
    /// and writes its details into the shared variables, so the tasks that follow (bubble, receive, eat) use whatever
    /// this NPC decided to order this time.
    /// Fails if the menu is missing or has nothing available.
    /// </summary>
    public class PickOrder : NPCAction
    {
        public CafeMenu Menu;

        public SharedString ItemId = new SharedString();
        public SharedString DisplayName = new SharedString();
        public SharedSprite Icon = new SharedSprite();
        public SharedFloat HungerFill = new SharedFloat();

        bool hasWarned;

        public PickOrder()
        {
        }

        public PickOrder(CafeMenu menu, SharedString itemId, SharedString displayName, SharedSprite icon, SharedFloat hungerFill)
        {
            Menu = menu;
            ItemId = itemId;
            DisplayName = displayName;
            Icon = icon;
            HungerFill = hungerFill;
        }

        protected override TaskStatus OnUpdate()
        {
            if (Menu == null || !Menu.TryPickRandom(out CafeMenuItem item))
            {
                if (!hasWarned)
                {
                    string reason = Menu == null ? "no Cafe Menu assigned" : $"nothing on '{Menu.name}' is available";
                    Debug.LogWarning($"{nameof(PickOrder)} on '{npc?.name}': {reason}.");
                    hasWarned = true;
                }

                return TaskStatus.Failure;
            }

            CafeOrder order = item.CreateOrder();

            if (ItemId != null) ItemId.Value = order.Id;
            if (DisplayName != null) DisplayName.Value = order.DisplayName;
            if (Icon != null) Icon.Value = order.Icon;
            if (HungerFill != null) HungerFill.Value = order.HungerFill;

            if (npc != null)
            {
                npc.CurrentOrder = order;
            }

            return TaskStatus.Success;
        }
    }
}
