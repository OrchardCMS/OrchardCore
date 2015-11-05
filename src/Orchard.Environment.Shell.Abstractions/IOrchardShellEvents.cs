using Orchard.Events;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell {
    public interface IOrchardShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }
}
