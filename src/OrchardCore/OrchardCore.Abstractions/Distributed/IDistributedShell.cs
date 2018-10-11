using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Distributed
{
    public interface IDistributedShell : IModularTenantEvents
    {
        /// <summary>
        /// These events are only invoked on the 'Default' tenant.
        /// </summary>
        Task ActivatedAsync(string tenant);
        Task TerminatedAsync(string tenant);
    }
}