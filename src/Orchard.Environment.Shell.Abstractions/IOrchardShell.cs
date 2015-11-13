using System.Threading.Tasks;

namespace Orchard.Environment.Shell {
    public interface IOrchardShell {
        bool IsActivated { get; }
        void Activate();
        void Terminate();
    }
}