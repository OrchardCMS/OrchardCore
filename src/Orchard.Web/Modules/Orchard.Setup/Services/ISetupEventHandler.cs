using System.Threading.Tasks;
using Orchard.Events;

namespace Orchard.Setup.Services
{
    public interface ISetupEventHandler : IEventHandler
    {
        Task CreateSuperUserAsync(string userName, string email, string password);
    }
}
