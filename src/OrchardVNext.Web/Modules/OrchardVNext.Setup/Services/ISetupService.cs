using OrchardVNext.DependencyInjection;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Setup.Services {
    public interface ISetupService : IDependency {
        ShellSettings Prime();
        string Setup(SetupContext context);
    }
}