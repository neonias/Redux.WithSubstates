using System;
using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates
{
    public interface IStore<TState> : IObservable<TState>
    {
        TState State { get; }
        void Dispatch(IAction action);
        IStore<TSubState> Select<TSubState>(Func<TState, TSubState> selector);
        IDisposable Subscribe(Action<TState> action);
        void ExecuteForEveryDispatch(Action<IAction> action);
    }
}
