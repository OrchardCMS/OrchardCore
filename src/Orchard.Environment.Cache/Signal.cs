using Microsoft.Extensions.Primitives;
using Orchard.Environment.Cache.Abstractions;
using System.Collections.Concurrent;
using System.Threading;

namespace Orchard.Environment.Cache
{
    public class Signal : ISignal
    {
        private readonly ConcurrentDictionary<string, ChangeTokenInfo> _changeTokens
            = new ConcurrentDictionary<string, ChangeTokenInfo>();

        public IChangeToken GetToken(string key)
        {
            return _changeTokens.GetOrAdd(
                key,
                _ =>
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var changeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                    return new ChangeTokenInfo(changeToken, cancellationTokenSource);
                }).ChangeToken;
        }

        public void SignalToken(string key)
        {
            ChangeTokenInfo changeTokenInfo;
            if (_changeTokens.TryRemove(key, out changeTokenInfo))
            {
                changeTokenInfo.TokenSource.Cancel();
            }
        }

        private class ChangeTokenInfo
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
