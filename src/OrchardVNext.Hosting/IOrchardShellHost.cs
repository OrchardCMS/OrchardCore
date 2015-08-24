using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Hosting {
    public interface IOrchardShellHost {
        void BeginRequest(ShellSettings shellSettings);
        void EndRequest(ShellSettings shellSettings);
    }
}