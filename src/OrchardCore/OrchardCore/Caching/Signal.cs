using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Cache
{
    /// <summary>
    /// This component is a singleton that holds all the existing signal tokens for a tenant.
    /// </summary>
    public class Signal : ModularTenantEvents, ISignal
    {
        private readonly ConcurrentDictionary<string, ChangeTokenInfo> _changeTokens;

        public Signal()
        {
            _changeTokens = new ConcurrentDictionary<string, ChangeTokenInfo>();
        }

        public IChangeToken GetToken(string key)
        {
            return _changeTokens.GetOrAdd(
                key,
                _ =>
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var changeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                    return new ChangeTokenInfo(changeToken, cancellationTokenSource);
                })
                .ChangeToken;
        }

        public void SignalToken(string key)
        {
            if (_changeTokens.TryRemove(key, out var changeTokenInfo))
            {
                changeTokenInfo.TokenSource.Cancel();
            }
        }

        public Task SignalTokenAsync(string key)
        {
            SignalToken(key);
            return Task.CompletedTask;
        }

        private readonly struct ChangeTokenInfo
        {
            public ChangeTokenInfo(IChangeToken changeToken, CancellationTokenSource tokenSource)
            {
                ChangeToken = changeToken;
                TokenSource = tokenSource;
            }

            public IChangeToken ChangeToken { get; }

            public CancellationTokenSource TokenSource { get; }
        }
    }
}
