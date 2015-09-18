using Orchard.Events;

namespace Orchard.Hosting {
    public interface IOrchardShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }
}
