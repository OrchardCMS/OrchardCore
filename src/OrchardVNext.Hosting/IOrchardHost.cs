using OrchardVNext.Configuration.Environment;
using OrchardVNext.Hosting.ShellBuilders;

namespace OrchardVNext.Hosting {
    public interface IOrchardHost {
        void Initialize();

        ShellContext CreateShellContext(ShellSettings settings);
    }
}
