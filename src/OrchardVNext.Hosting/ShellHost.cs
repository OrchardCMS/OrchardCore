using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Hosting {
    public class ShellHost : IShellHost {
        void IShellHost.BeginRequest(ShellSettings settings) {
            Logger.Debug("Begin Request for tenant {0}", settings.Name);
        }

        void IShellHost.EndRequest(ShellSettings settings) {
            Logger.Debug("End Request for tenant {0}", settings.Name);
        }
    }
}