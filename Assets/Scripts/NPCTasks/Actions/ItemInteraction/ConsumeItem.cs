namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Takes the item the NPC is holding: destroys it and fires <see cref="NPC.AcceptFood"/> (particles, sound).
    /// Fails if the NPC isn't holding anything (e.g. the order timed out), so the steps after it don't run.
    /// </summary>
    public class ConsumeItem : ItemReceiverAction
    {
        protected override TaskStatus OnUpdate()
        {
            if (!TryGetReceiver(out NPCItemReceiver receiver))
            {
                return TaskStatus.Failure;
            }

            return receiver.Consume() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
