namespace Game.Cozyffee.TaskSystem
{
    /// <summary>
    /// Succeeds while <see cref="Variable"/> is true. Use <see cref="Condition.Invert"/> to check for false.
    /// </summary>
    public class IsTrue : Condition
    {
        public SharedBool Variable;

        public IsTrue()
        {
        }

        public IsTrue(SharedBool variable)
        {
            Variable = variable;
        }

        protected override TaskStatus OnUpdate()
        {
            return Variable != null && Variable.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
