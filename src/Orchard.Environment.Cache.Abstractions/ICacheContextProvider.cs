using Orchard.DependencyInjection;

namespace Orchard.Environment.Cache.Abstractions
{
    public interface ICacheContextProvider : IDependency
    {
        void Update(string content, ref int hash);
    }
}
