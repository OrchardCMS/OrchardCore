using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Cache
{
    public interface ISignal : IModularTenantEvents
    {
        IChangeToken GetToken(string key);
        Task SignalTokenAsync(string key);
    }

    public static class SignalExtensions
    {
        /// <summary>
        /// Adds a Signal (if not already present) to be sent at the end of the shell scope.
        /// </summary>
        public static void DeferredSignalToken(this ISignal _, string key)
        {
            ShellScope.AddDeferredSignal(key);
        }
    }
}
