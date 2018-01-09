namespace Redux.WithSubstates.Actions
{
    /// <summary>
    /// This action should be used in Effect.
    /// If the effect does not require any subsequent action,
    /// this type should be returned.
    /// </summary>
    public class NullAction : IAction
    {
        static NullAction()
        {
            Instance = new NullAction();
        }

        private NullAction() { }

        public static NullAction Instance { get; }
    }
}
