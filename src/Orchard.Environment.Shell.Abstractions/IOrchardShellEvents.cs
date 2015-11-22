using Orchard.Events;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell
{
    public interface IOrchardShellEvents : IEventHandler
    {
        Task ActivatingAsync();
        Task ActivatedAsync();
        Task TerminatingAsync();
        Task TerminatedAsync();
    }
}