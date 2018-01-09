using System;
using System.Collections.Generic;
using System.Linq;
using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates
{
    internal class Reducer<TState> : IReducer<TState>
    {
        private readonly IDictionary<Type, IList<ReducerDelegate<TState>>> _reducers;

        internal Reducer(IDictionary<Type, IList<ReducerDelegate<TState>>> reducers)
        {
            _reducers = reducers;
        }

        public TState Reduce(TState state, IAction action)
        {
            if (!_reducers.ContainsKey(action.GetType()))
                return state;

            var reducers = _reducers[action.GetType()];
            return reducers
                .Aggregate(state, (updatedState, reducer) => reducer(updatedState, action));
        }
    }
}
