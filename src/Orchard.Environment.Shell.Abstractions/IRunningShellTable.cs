namespace Orchard.Environment.Shell
{
    public interface IRunningShellTable
    {
        void Add(ShellSettings settings);
        void Remove(ShellSettings settings);
        void Update(ShellSettings settings);
        ShellSettings Match(string host, string appRelativeCurrentExecutionFilePath);
    }
}