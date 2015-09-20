using Orchard.Environment.Shell;

namespace Orchard.Hosting {
    public interface IOrchardShellHost {
        void BeginRequest(ShellSettings shellSettings);
        void EndRequest(ShellSettings shellSettings);
    }
}