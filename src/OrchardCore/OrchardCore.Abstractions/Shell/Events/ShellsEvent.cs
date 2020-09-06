using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Events
{
    /// <summary>
    /// The type of the delegate that will get called on <see cref="IShellHost.InitializeAsync"/>.
    /// </summary>
    public delegate Task ShellsEvent();
}
