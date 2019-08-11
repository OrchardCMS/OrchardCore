using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Cache
{
    public interface ISignal
    {
        IChangeToken GetToken(string key);

        void SignalToken(string key);
    }
}
