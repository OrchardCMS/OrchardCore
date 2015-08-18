using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Hosting {
    public interface IShellHost {
        void BeginRequest(ShellSettings shellSettings);
        void EndRequest(ShellSettings shellSettings);
    }
}