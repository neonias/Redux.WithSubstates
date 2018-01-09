using System;
using Redux.WithSubstates.Actions;

namespace Redux.WithSubstates
{
    internal class Effect<TState> : IEffect<TState>
    {
        private readonly EffectDelegate<TState> _effect;
        private readonly Type _actionParameterType;

        internal Effect(EffectDelegate<TState> effect, Type actionParameterType)
        {
            _effect = effect;
            _actionParameterType = actionParameterType;
        }

        public IAction Execute(TState state, IAction action)
        {
            return _actionParameterType == action.GetType() 
                ? _effect(state, action) 
                : NullAction.Instance;
        }
    }
}
