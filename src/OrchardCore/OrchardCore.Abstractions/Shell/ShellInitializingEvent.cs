using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// The type of the delegate that will get called on <see cref="IShellHost.InitializeAsync"/>.
    /// </summary>
    public delegate Task ShellInitializingEvent();
}
