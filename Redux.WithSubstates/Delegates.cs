using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates
{
    internal delegate TState ReducerDelegate<TState>(TState state, IAction action);

    internal delegate IAction EffectDelegate<TState>(TState state, IAction action);
}
