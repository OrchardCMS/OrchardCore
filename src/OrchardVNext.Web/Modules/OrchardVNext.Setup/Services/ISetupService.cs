using OrchardVNext.Configuration.Environment;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Setup.Services {
    public interface ISetupService : IDependency {
        ShellSettings Prime();
        string Setup(SetupContext context);
    }
}