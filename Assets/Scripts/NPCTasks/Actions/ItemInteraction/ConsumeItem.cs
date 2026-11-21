namespace Game.Cozyffee.TaskSystem
{
    public class ConsumeItem : ItemReceiverAction
    {
        protected override TaskStatus OnUpdate()
        {
            if (!TryGetReceiver(out NPCItemReceiver receiver))
            {
                return TaskStatus.Failure;
            }

            receiver.Consume();
            return TaskStatus.Success;
        }
    }
}