using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// 'IDefaultShellEvents' events are only invoked on the default tenant.
    /// </summary>
    public interface IDefaultShellEvents
    {
        /// <summary>
        /// Invoked when the 'Default' tenant has been created or recreated.
        /// </summary>
        Task CreatedAsync();

        /// <summary>
        /// Invoked when any tenant has changed.
        /// </summary>
        Task ChangedAsync(string tenant);
    }
}