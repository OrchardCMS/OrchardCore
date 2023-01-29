using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    /// <summary>
    /// An implementation of this interface is used to initialize the services and the HTTP request pipeline.
    /// pipeline of a module.
    /// </summary>
    public interface IStartup
    {
        /// <summary>
        /// Get the value to use to order startups to configure services. The default is 0.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Get the value to use to order startups to initialize services. The default is the 'Order' property.
        /// </summary>
        int InitializeOrder { get; }

        /// <summary>
        /// Get the value to use to order startups to build the pipeline. The default is the 'Order' property.
        /// </summary>
        int ConfigureOrder { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the tenant container.
        /// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// This async method gets called by the runtime. Use this method to initialize tenant container services.
        /// </summary>
        /// <param name="serviceProvider">The tenant container service provider.</param>
        Task InitializeServicesAsync(IServiceProvider serviceProvider);

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the tenant HTTP request pipeline.
        /// </summary>
        /// <param name="builder">The tenant application builder</param>
        /// <param name="routes">The tenant endpoint route builder</param>
        /// <param name="serviceProvider">The service provider of the current shell scope.</param>
        void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider);
    }
}
