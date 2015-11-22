using Orchard.DependencyInjection;

namespace Orchard.Setup.Services
{
    public interface ISetupService : IDependency
    {
        string Setup(SetupContext context);
    }
}