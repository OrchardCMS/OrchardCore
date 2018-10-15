using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// These events are only invoked on the default tenant.
    /// </summary>
    public interface IDefaultShellEvents
    {
        /// <summary>
        /// Invoked when the 'Default' tenant is 1st created or has changed.
        /// </summary>
        Task CreatedAsync();

        /// <summary>
        /// Invoked when any tenant has changed.
        /// </summary>
        Task ChangedAsync(string tenant);
    }
}