using System;
using System.Collections.Generic;
using System.Reflection;

namespace Orchard.Events {
    public interface IMethodAdaptor {
        Func<object, object, bool> Adapt(MethodInfo method, Type inputType, object parameters);
    }

    public class DefaultMethodAdaptor : IMethodAdaptor {
        public Func<object, object, bool> Adapt(MethodInfo method, Type inputType, object parameters) {
            return InternalProxyMethodEmitter.CreateProxyMethod(method, inputType, parameters as IDictionary<string, object>);
        }
    }
}
