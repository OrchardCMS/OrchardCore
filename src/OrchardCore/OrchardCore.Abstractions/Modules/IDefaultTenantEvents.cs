using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    public interface IDefaultTenantEvents
    {
        /// <summary>
        /// This event is only invoked on the 'Default' tenant.
        /// </summary>
        Task ReloadAsync(string tenant);
    }
}