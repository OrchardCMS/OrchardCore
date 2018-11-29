using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    public interface IModularTenantEvents
    {
        Task ActivatingAsync();
        Task ActivatedAsync();
        Task TerminatingAsync();
        Task TerminatedAsync();
    }
}