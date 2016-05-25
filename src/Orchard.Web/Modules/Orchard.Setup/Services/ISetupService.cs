using System.Threading.Tasks;
using Orchard.DependencyInjection;

namespace Orchard.Setup.Services
{
    public interface ISetupService : IDependency
    {
        Task<string> SetupAsync(SetupContext context);
    }
}