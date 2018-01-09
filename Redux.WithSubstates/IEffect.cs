using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates
{
    public interface IEffect<TState>
    {
        IAction Execute(TState state, IAction action);
    }
}
