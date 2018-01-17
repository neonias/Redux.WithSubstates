using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Redux.WithSubstates.Actions;
using Redux.WithSubstates.DecoratingInterfaces;
using Redux.WithSubstates.Extensions;

namespace Redux.WithSubstates
{
    public class Store<TState> : IStore<TState>
    {
        private readonly IReducer<TState> _reducer;
        private readonly IEnumerable<IEffect<TState>> _effects;
        private readonly ReplaySubject<TState> _stateReplaySubject;
        private TState _state;
        private readonly IList<Action<IAction>> _allDispatchListeners;
        private readonly object _lock = new object();

        public Store(TState initialState, IReducer<TState> reducer, IEnumerable<IEffect<TState>> effects)
        {
            _stateReplaySubject = new ReplaySubject<TState>(1);
            _reducer = reducer;
            _effects = effects;
            _allDispatchListeners = new List<Action<IAction>>();
            State = initialState;
        }

        public Store(TState initialState, IReducer<TState> reducer)
            : this(initialState, reducer, new List<IEffect<TState>>())
        {
        }

        public TState State
        {
            get => _state;
            set
            {
                // intentionally allow to assign the same value
                // in order to notify subscribers observing substates of _state
                _state = value;
                _stateReplaySubject.OnNext(_state);
            }
        }

        public void Dispatch(IAction action)
        {
            lock (_lock)
            {
                State = _reducer.Reduce(State, action);
                DispatchEffects(action, State);
                NotifyAllDispatchListeners(action);
            }
        }

        public IDisposable Subscribe(IObserver<TState> observer)
        {
            return _stateReplaySubject.Subscribe(observer);
        }

        public IDisposable Subscribe(Action<TState> action)
        {
            return _stateReplaySubject.Subscribe(action);
        }

        public IStore<TSubState> Select<TSubState>(Func<TState, TSubState> selector)
        {
            return new SubStore<TState, TSubState>(this, selector);
        }

        public void ExecuteForEveryDispatch(Action<IAction> action)
        {
            _allDispatchListeners.Add(action);
        }

        private void NotifyAllDispatchListeners(IAction action)
        {
            foreach (var dispatchListener in _allDispatchListeners)
                dispatchListener(action);
        }

        private void DispatchEffects(IAction action, TState state)
        {
            SequentialEffects
                .Select(_ => _.Execute(state, action))
                .ForEach(requestedAction =>
                {
                    if (requestedAction.GetType() != typeof(NullAction))
                        Dispatch(requestedAction);
                });
            NonSequentialEffects
                .Select(_ => Observable.Start(() => _.Execute(state, action)))
                .ForEach(requestedAction =>
                {
                    requestedAction.Subscribe(_ =>
                    {
                        if (_.GetType() == typeof(NullAction))
                            return;
                        Dispatch(_);
                    });
                });
        }

        private IEnumerable<IEffect<TState>> SequentialEffects =>
            _effects.Where(_ => typeof(ISequentialEffectClass<TState>).IsAssignableFrom(_.GetType()));

        private IEnumerable<IEffect<TState>> NonSequentialEffects =>
            _effects.Where(_ => !typeof(ISequentialEffectClass<TState>).IsAssignableFrom(_.GetType()));
    }
}
