using Microsoft.Extensions.Primitives;
using Orchard.DependencyInjection;

namespace Orchard.Environment.Cache.Abstractions
{
    public interface ISignal : ISingletonDependency
    {
        IChangeToken GetToken(string key);

        void SignalToken(string key);
    }
}
