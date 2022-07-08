using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Events
{
    /// <summary>
    /// The type of the delegate that will get called before releasing or reloading a given shell.
    /// </summary>
    public delegate Task ShellEvent(string name);
}
