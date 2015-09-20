using Orchard.Environment.Shell;
using Orchard.Hosting.ShellBuilders;

namespace Orchard.Hosting {
    public interface IOrchardHost {
        void Initialize();

        ShellContext CreateShellContext(ShellSettings settings);
    }
}
