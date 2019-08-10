using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Cache
{
    public interface ISignal
    {
        IChangeToken GetToken(string key);

        void SignalToken(string key);
    }

    public static class SignalExtensions
    {
        /// <summary>
        /// A 'SignalToken' deferred at the end of the shell scope.
        /// </summary>
        public static void DeferredSignalToken(this ISignal signal, string key)
        {
            ShellScope.RegisterBeforeDispose(scope =>
            {
                signal.SignalToken(key);
                return Task.CompletedTask;
            });
        }
    }
}
