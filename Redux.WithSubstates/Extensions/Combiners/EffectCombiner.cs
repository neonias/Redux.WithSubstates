using System.Collections.Generic;
using System.Reflection;
using Redux.WithSubstates.Actions;
using Redux.WithSubstates.DecoratingInterfaces;

namespace Redux.WithSubstates.Extensions.Combiners
{
    public static class EffectCombiner
    {
        public static IEnumerable<IEffect<TState>> Combine<TState>(this IEnumerable<IEffectClass<TState>> effectClasses)
        {
            var effects = new List<IEffect<TState>>();

            foreach (var effectClass in effectClasses)
            {
                var effectType = effectClass.GetType();
                var methods =
                    effectType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

                foreach (var method in methods)
                {
                    var methodParameters = method.GetParameters();
                    var returnType = method.ReturnType;
                    var actionType = methodParameters[1].ParameterType;

                    if (methodParameters.Length == 2 
                        && typeof(IAction).IsAssignableFrom(returnType)
                        && methodParameters[0].ParameterType == typeof(TState) 
                        && typeof(IAction).IsAssignableFrom(actionType))
                    {
                        var newEffect = new Effect<TState>((state, action) => (IAction)method.Invoke(effectClass, new object[] { state, action }), actionType);
                        effects.Add(newEffect);
                    }
                }
            }

            return effects;
        }

        public static IEnumerable<IEffect<TState>> Combine<TState>(this IEffectClass<TState> effectClass)
        {
            return new[] {effectClass}.Combine();
        }
    }
}
