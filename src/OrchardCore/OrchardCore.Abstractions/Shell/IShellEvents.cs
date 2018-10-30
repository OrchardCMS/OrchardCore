using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// These events are only invoked on the default tenant.
    /// </summary>
    public interface IShellEvents
    {
        /// <summary>
        /// Invoked when the 'Default' tenant has been created.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Invoked before any tenant run a setup or recipe.
        /// </summary>
        Task InitializeAsync(string tenant);

        /// <summary>
        /// Invoked when any tenant is going to be reloaded.
        /// </summary>
        Task ReloadAsync(string tenant);
    }
}