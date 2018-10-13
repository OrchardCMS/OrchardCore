using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// 'IDefaultShellEvents' events are only invoked on the default tenant.
    /// </summary>
    public interface IDefaultShellEvents
    {
        Task CreatedAsync();
        Task ChangedAsync(string tenant);
    }
}