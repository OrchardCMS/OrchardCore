using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Environment {
    public interface IOrchardShellHost {
        void BeginRequest(ShellSettings shellSettings);
        void EndRequest(ShellSettings shellSettings);
    }

    public class DefaultOrchardShellHost : IOrchardShellHost {
        void IOrchardShellHost.BeginRequest(ShellSettings settings) {
            Logger.Debug("Begin Request for tenant {0}", settings.Name);
        }

        void IOrchardShellHost.EndRequest(ShellSettings settings) {
            Logger.Debug("End Request for tenant {0}", settings.Name);
        }
    }
}