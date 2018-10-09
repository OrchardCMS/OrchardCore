using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    /// <summary>
    /// These events are only invoked on the default tenant.
    /// </summary>
    public interface IDefaultTenantEvents
    {
        Task DefaultTenantCreatedAsync();
        Task ReloadedAsync(string tenant);
    }
}