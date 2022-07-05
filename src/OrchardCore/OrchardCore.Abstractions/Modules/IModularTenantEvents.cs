using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    public interface IModularTenantEvents
    {
        Task ActivatingAsync();
        Task ActivatedAsync();
        Task TerminatingAsync();
        Task TerminatedAsync();
        Task RemovingAsync();
    }

    public class ModularTenantEvents : IModularTenantEvents
    {
        public virtual Task ActivatedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task ActivatingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task TerminatedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task RemovingAsync() => Task.CompletedTask;
    }
}
