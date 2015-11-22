using Orchard.Environment.Shell;
using Orchard.Hosting.ShellBuilders;

namespace Orchard.Hosting
{
    public interface IOrchardHost
    {
        void Initialize();
        ShellContext GetShellContext(ShellSettings settings);
        void UpdateShellSettings(ShellSettings settings);
        ShellContext CreateShellContext(ShellSettings settings);
    }
}