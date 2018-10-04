using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Cache
{
    public interface ISignal
    {
        IChangeToken GetToken(string key);
        Task SignalTokenAsync(string key);
    }
}
