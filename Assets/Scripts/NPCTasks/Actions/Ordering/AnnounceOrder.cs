namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Fires <see cref="NPC.Order"/> so listeners (sound, particles, ...) can react to the NPC placing its order.
    /// Run it once the NPC has reached its order spot, right before it shows the order bubble.
    /// </summary>
    public class AnnounceOrder : NPCAction
    {
        protected override TaskStatus OnUpdate()
        {
            if (npc == null)
            {
                return TaskStatus.Failure;
            }

            npc.Order?.Invoke();
            return TaskStatus.Success;
        }
    }
}
