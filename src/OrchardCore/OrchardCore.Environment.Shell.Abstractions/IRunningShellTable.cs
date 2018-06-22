namespace OrchardCore.Environment.Shell
{
    public interface IRunningShellTable
    {
        void Add(ShellSettings settings);
        void Remove(ShellSettings settings);
        ShellSettings Match(string host, string appRelativeCurrentExecutionFilePath, bool fallbackToDefault = true);
    }
}