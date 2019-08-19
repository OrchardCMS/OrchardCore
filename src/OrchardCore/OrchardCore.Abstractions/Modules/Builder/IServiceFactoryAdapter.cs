using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IServiceFactoryAdapter
    {
        object CreateBuilder(IServiceCollection services);

        IServiceProvider CreateServiceProvider(object containerBuilder);
    }
}