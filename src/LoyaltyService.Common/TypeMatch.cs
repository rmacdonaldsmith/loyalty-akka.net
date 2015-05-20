using System;
using System.Collections.Generic;

namespace LoyaltyService.Common
{
    //from here: http://stackoverflow.com/questions/7252186/switch-case-on-type-c-sharp/7301514#7301514
    public class TypeMatch
    {
        private readonly Dictionary<Type, Action<object>> _matches = new Dictionary<Type, Action<object>>();
        private Action _defaultCase = Throw;

        public TypeMatch Case<T>(Action<T> action)
        {
            _matches.Add(typeof(T), (x) => action((T)x)); 
            return this;
        }

        public TypeMatch Default(Action defaultAction)
        {
            _defaultCase = defaultAction;
            return this;
        }

        public void Match(object x)
        {
            if (!_matches.ContainsKey(x.GetType()))
                _defaultCase();
            else
                _matches[x.GetType()](x);
        }

        public static Action DoNothing
        {
            get { return () => { }; }
        }

        public static Action Throw
        {
            get { return () => { throw new InvalidOperationException(); }; }
        }
    }
}
