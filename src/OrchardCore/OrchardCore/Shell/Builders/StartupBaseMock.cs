using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Builders
{
    internal class StartupBaseMock : StartupBase
    {
        private readonly object _startup;
        private readonly MethodInfo _configureServices;
        private readonly MethodInfo _initializeServicesAsync;
        private readonly MethodInfo _configure;

        public StartupBaseMock(
            object startup,
            MethodInfo configureServices,
            MethodInfo initializeServicesAsync,
            MethodInfo configure,
            PropertyInfo order,
            PropertyInfo initializeOrder,
            PropertyInfo configureOrder)
        {
            _startup = startup;
            _configureServices = configureServices;
            _initializeServicesAsync = initializeServicesAsync;
            _configure = configure;

            var orderValue = order?.GetValue(_startup);
            var initializeOrderValue = initializeOrder?.GetValue(_startup);
            var configureOrderValue = configureOrder?.GetValue(_startup);

            Order = orderValue != null ? (int)orderValue : default;
            InitializeOrder = initializeOrderValue != null ? (int)initializeOrderValue : Order;
            ConfigureOrder = configureOrderValue != null ? (int)configureOrderValue : Order;
        }

        /// <inheritdoc />
        public override int Order { get; }

        /// <inheritdoc />
        public override int InitializeOrder { get; }

        /// <inheritdoc />
        public override int ConfigureOrder { get; }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (_configureServices == null)
            {
                return;
            }

            _configureServices.Invoke(_startup, new[] { services });
        }

        public override Task InitializeServicesAsync(IServiceProvider serviceProvider)
        {
            if (_initializeServicesAsync == null)
            {
                return Task.CompletedTask;
            }

            return (Task)_initializeServicesAsync.Invoke(_startup, new[] { serviceProvider });
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
