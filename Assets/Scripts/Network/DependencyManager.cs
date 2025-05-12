using System;
using System.Collections.Generic;

namespace Network
{
    public class DependencyManager
    {
        private Dictionary<Type, object> _dependencies = new Dictionary<Type, object>();

        public void Register<TInterface, TImplementation>(TImplementation instance) where TImplementation : TInterface
        {
            _dependencies[typeof(TInterface)] = instance;
        }

        public TInterface Resolve<TInterface>()
        {
            if (_dependencies.TryGetValue(typeof(TInterface), out object dependency))
            {
                return (TInterface)dependency;
            }
            throw new Exception($"Dependency not registered: {typeof(TInterface).Name}");
        }

        public void Clear()
        {
            _dependencies.Clear();
        }
    }
}