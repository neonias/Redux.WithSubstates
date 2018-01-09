using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates
{
    public interface IReducer<TState>
    {
        TState Reduce(TState state, IAction action);
    }
}
