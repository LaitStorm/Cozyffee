namespace Game.Cozyffee.TaskSystem
{
    public class HideBubble : NPCAction
    {
        protected override TaskStatus OnUpdate()
        {
            foreach (NPCBubble bubble in npc.GetComponentsInChildren<NPCBubble>(true))
            {
                bubble.Hide();
            }

            return TaskStatus.Success;
        }
    }
}