using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public abstract class StartupBase : IStartup
    {
        private int? _configureOrder;

        /// <inheritdoc />
        public virtual int Order { get; } = 0;

        /// <inheritdoc />
        public virtual int ConfigureOrder
        {
            get => _configureOrder ?? Order;
            set => _configureOrder = value;
        }

        /// <inheritdoc />
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        /// <inheritdoc />
        public virtual void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
