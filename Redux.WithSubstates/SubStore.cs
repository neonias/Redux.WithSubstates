using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates
{
    internal class SubStore<TState, TSubState> : IStore<TSubState>
    {
        private readonly IStore<TState> _rootStore;
        private readonly Func<TState, TSubState> _selector;

        public SubStore(IStore<TState> rootStore, Func<TState, TSubState> selector)
        {
            _rootStore = rootStore;
            _selector = selector;
        }

        public IDisposable Subscribe(IObserver<TSubState> observer)
        {
            return Observable
                .Select(_rootStore, _selector)
                .DistinctUntilChanged(new Comparer())
                .Subscribe(observer);
        }

        public IDisposable Subscribe(Action<TSubState> observer)
        {
            return Observable
                .Select(_rootStore, _selector)
                .DistinctUntilChanged(new Comparer())
                .Subscribe(observer);
        }

        public TSubState State => _selector(_rootStore.State);

        public void Dispatch(IAction action)
        {
            _rootStore.Dispatch(action);
        }

        public IStore<TSubSubState> Select<TSubSubState>(Func<TSubState, TSubSubState> selector)
        {
            return new SubStore<TState, TSubSubState>(_rootStore, state => selector(State));
        }

        public void ExecuteForEveryDispatch(Action<IAction> action)
        {
            _rootStore.ExecuteForEveryDispatch(action);
        }

        private class Comparer : IEqualityComparer<TSubState>
        {
            public bool Equals(TSubState x, TSubState y)
            {

                return x != null && x.Equals(y) || x == null && y == null;
            }

            public int GetHashCode(TSubState obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
