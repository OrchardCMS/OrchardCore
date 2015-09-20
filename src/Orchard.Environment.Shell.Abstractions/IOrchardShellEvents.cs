using Orchard.Events;

namespace Orchard.Environment.Shell {
    public interface IOrchardShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }
}
