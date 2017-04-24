using Microsoft.Extensions.Primitives;

namespace Orchard.Environment.Cache
{
    public interface ISignal
    {
        IChangeToken GetToken(string key);

        void SignalToken(string key);
    }
}
