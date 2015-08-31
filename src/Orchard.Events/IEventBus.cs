using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Events {
    public interface IEventBus : IDependency {
        void Notify(string messageName, IDictionary<string, object> eventData);
    }
}
