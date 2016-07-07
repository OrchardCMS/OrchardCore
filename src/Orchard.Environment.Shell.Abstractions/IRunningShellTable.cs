namespace Orchard.Environment.Shell
{
    public interface IRunningShellTable
    {
        void Store(ShellSettings settings);
        void Remove(ShellSettings settings);
        ShellSettings Match(string host, string appRelativeCurrentExecutionFilePath);
    }
}