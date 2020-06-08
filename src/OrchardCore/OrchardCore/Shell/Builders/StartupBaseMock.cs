using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Builders
{
    internal class StartupBaseMock : StartupBase
    {
        private readonly object _startup;
        private readonly MethodInfo _configureService;
        private readonly MethodInfo _configure;

        public StartupBaseMock(
            object startup,
            MethodInfo configureService,
            MethodInfo configure,
            PropertyInfo order,
            PropertyInfo configureOrder)
        {
            _startup = startup;
            _configureService = configureService;
            _configure = configure;

            var orderValue = order?.GetValue(_startup);
            var configureOrderValue = configureOrder?.GetValue(_startup);

            Order = orderValue != null ? (int)orderValue : default;
            ConfigureOrder = configureOrderValue != null ? (int)configureOrderValue : Order;
        }

        /// <inheritdoc />
        public override int Order { get; }

        /// <inheritdoc />
        public override int ConfigureOrder { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (_configureService == null)
            {
                return;
            }

            _configureService.Invoke(_startup, new[] { services });
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            if (_configure == null)
            {
                return;
            }

            // Resolve all services

            var parameters = _configure.GetParameters().Select(x =>
            {
                if (x.ParameterType == typeof(IServiceProvider))
                {
                    return serviceProvider;
                }
                else if (x.ParameterType == typeof(IApplicationBuilder))
                {
                    return app;
                }
                else if (x.ParameterType == typeof(IEndpointRouteBuilder))
                {
                    return routes;
                }

                return serviceProvider.GetService(x.ParameterType);
            });

            _configure.Invoke(_startup, parameters.ToArray());
        }
    }
}
