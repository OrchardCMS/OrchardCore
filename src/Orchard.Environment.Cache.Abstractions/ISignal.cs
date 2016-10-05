using Microsoft.Extensions.Primitives;
using Orchard.DependencyInjection;

namespace Orchard.Environment.Cache
{
    public interface ISignal
    {
        IChangeToken GetToken(string key);

        void SignalToken(string key);
    }
}
