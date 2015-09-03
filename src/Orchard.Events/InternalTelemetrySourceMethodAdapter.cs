using System;
using System.Reflection;

namespace Orchard.Events {
    public interface IMethodAdaptor {
        Func<object, object, bool> Adapt(MethodInfo method, object parameters);
    }

    public class DefaultMethodAdaptor : IMethodAdaptor {
        public Func<object, object, bool> Adapt(MethodInfo method, object parameters) {
            throw new NotImplementedException();
        }
    }
}
