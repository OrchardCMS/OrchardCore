using Orchard.Configuration.Environment;
using Orchard.DependencyInjection;

namespace Orchard.Setup.Services {
    public interface ISetupService : IDependency {
        ShellSettings Prime();
        string Setup(SetupContext context);
    }
}