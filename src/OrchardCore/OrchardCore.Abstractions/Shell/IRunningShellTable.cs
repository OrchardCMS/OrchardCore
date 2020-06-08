using Microsoft.AspNetCore.Http;

namespace OrchardCore.Environment.Shell
{
    public interface IRunningShellTable
    {
        void Add(ShellSettings settings);
        void Remove(ShellSettings settings);
        ShellSettings Match(HostString host, PathString path, bool fallbackToDefault = true);
    }
}
