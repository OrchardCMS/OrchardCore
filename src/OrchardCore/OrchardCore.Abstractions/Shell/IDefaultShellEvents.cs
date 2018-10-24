using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// These events are only invoked on the default tenant.
    /// </summary>
    public interface IDefaultShellEvents
    {
        /// <summary>
        /// Invoked when the 'Default' tenant is 1st created or has been updated.
        /// </summary>
        Task CreatedAsync();

        /// <summary>
        /// Invoked when any tenant has changed.
        /// </summary>
        Task ChangedAsync(string tenant);

        /// <summary>
        /// Invoked when any tenant has been reloaded.
        /// </summary>
        Task ReloadAsync(string tenant);

        /// <summary>
        /// Invoked when any tenant settings has been updated.
        /// </summary>
        Task UpdateSettingsAsync(string tenant);
    }
}