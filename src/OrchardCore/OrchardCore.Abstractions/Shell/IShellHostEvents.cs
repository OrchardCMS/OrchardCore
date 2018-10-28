using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// These events are only invoked on the default tenant.
    /// </summary>
    public interface IShellHostEvents
    {
        /// <summary>
        /// Invoked when the 'Default' tenant is 1st created or has been updated.
        /// </summary>
        Task CreatedAsync();

        /// <summary>
        /// Invoked when any tenant feature has changed.
        /// </summary>
        Task ChangedAsync(string tenant);

        /// <summary>
        /// Invoked when any tenant has been reloaded.
        /// </summary>
        Task ReloadedAsync(string tenant);

        /// <summary>
        /// Invoked when any tenant settings has been updated.
        /// </summary>
        Task UpdatedAsync(string tenant);
    }
}