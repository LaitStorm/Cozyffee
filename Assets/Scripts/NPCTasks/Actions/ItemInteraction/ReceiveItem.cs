namespace Game.Cozyffee.TaskSystem
{
    public class ReceiveItem : ItemReceiverAction
    {
        public SharedString ItemId = new SharedString();

        private bool isOpen;

        protected override TaskStatus OnUpdate()
        {
            if (!TryGetReceiver(out NPCItemReceiver receiver))
            {
                return TaskStatus.Failure;
            }

            string id = ItemId?.Value ?? "";

            if (!isOpen)
            {
                receiver.Open(id);
                isOpen = true;
            }

            Item held = receiver.HeldItem;

            if (held != null && (id.Length == 0 || held.Id == id))
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        protected override void OnExit()
        {
            if (isOpen && TryGetReceiver(out NPCItemReceiver receiver))
            {
                receiver.Close();
            }

            isOpen = false;
        }
    }
}