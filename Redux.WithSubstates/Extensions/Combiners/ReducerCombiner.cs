using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Redux.WithSubstates.Actions;
using Redux.WithSubstates.DecoratingInterfaces;

namespace Redux.WithSubstates.Extensions.Combiners
{
    public static class ReducerCombiner
    {
        public static IReducer<TState> Combine<TState>(this IEnumerable<IReducerClass<TState>> reducers)
        {
            var reducerDelegates = new Dictionary<Type, IList<ReducerDelegate<TState>>>();

            foreach (var reducer in reducers)
            {
                var reducerType = reducer.GetType();
                var methods =
                    reducerType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

                foreach (var method in methods)
                {
                    var methodParameters = method.GetParameters();
                    var returnType = method.ReturnType;
                    var actionType = methodParameters[1].ParameterType;

                    if (methodParameters.Length == 2 && returnType == typeof(TState) &&
                        methodParameters[0].ParameterType == typeof(TState) &&
                        actionType.GetInterfaces().Contains(typeof(IAction)))
                    {
                        if(!reducerDelegates.ContainsKey(actionType))
                            reducerDelegates.Add(actionType, new List<ReducerDelegate<TState>>());
                        reducerDelegates[actionType].Add((state, action) => (TState)method.Invoke(reducer, new object[] { state, action }));
                    }
                }
            }

            return new Reducer<TState>(reducerDelegates);
        }

        public static IReducer<TState> Combine<TState>(this IReducerClass<TState> reducer)
        {
            return new[] {reducer}.Combine();
        }
    }
}
