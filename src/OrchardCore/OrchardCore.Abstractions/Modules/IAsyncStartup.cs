using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace OrchardCore.Modules
{
    /// <summary>
    /// An implementation of this interface allows to configure asynchronously the tenant pipeline.
    /// </summary>
    public interface IAsyncStartup
    {
        /// <summary>
        /// This async method gets called before any <see cref="IStartup.Configure"/> and can collaborate
        /// to build the tenant pipeline, but is not intended to configure the route/endpoint middleware.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceProvider"></param>
        ValueTask ConfigureAsync(IApplicationBuilder builder, IServiceProvider serviceProvider);
    }
}
