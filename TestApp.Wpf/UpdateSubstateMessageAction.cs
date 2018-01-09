using Redux.WithSubstates.Actions;

namespace TestApp.Wpf
{
    public class UpdateSubstateMessageAction : IAction
    {
        public string Message { get; }

        public UpdateSubstateMessageAction(string message)
        {
            Message = message;
        }
    }
}