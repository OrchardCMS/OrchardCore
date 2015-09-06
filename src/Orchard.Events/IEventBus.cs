using Orchard.DependencyInjection;
using System.Collections;
using System.Collections.Generic;

namespace Orchard.Events {
    public interface IEventBus : IDependency {
        IEnumerable Notify(string messageName, IDictionary<string, object> eventData);
    }
}
