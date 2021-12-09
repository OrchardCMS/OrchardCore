using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Events
{
    /// <summary>
    /// The type of the delegate that will get called before loading all shells.
    /// </summary>
    public delegate Task ShellsEvent();
}
