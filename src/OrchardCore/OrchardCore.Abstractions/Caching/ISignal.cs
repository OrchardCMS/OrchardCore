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
#pragma warning disable IDE0060 // Remove unused parameter
        public static void DeferredSignalToken(this ISignal signal, string key)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            ShellScope.AddDeferredSignal(key);
        }
    }
}
