using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using Orchard.Hosting;

namespace Orchard.Cms.Web
{
    public class DesignTimeMvcBuilderConfiguration : IDesignTimeMvcBuilderConfiguration
    {
        public void ConfigureMvc(IMvcBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

            var services = new ServiceCollection();
            services.AddSingleton(env);

            var startUp = new Startup(env);
            startUp.ConfigureServices(builder.Services);

            serviceProvider = builder.Services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var app = new ApplicationBuilder(serviceProvider);
            startUp.Configure(app, loggerFactory);

            var orchardHost = serviceProvider.GetRequiredService<IOrchardHost>();
            var runningShellTable = serviceProvider.GetRequiredService<IRunningShellTable>();

            orchardHost.Initialize();
            var shellSettings = runningShellTable.Match(new DefaultHttpContext());

            var shellContext = orchardHost.GetOrCreateShellContext(shellSettings);
            using (var scope = shellContext.CreateServiceScope())
            {
                builder.Services.AddSingleton<IHttpContextAccessor, DesignTimeHttpContextAccessor>(s =>
                    new DesignTimeHttpContextAccessor(scope.ServiceProvider, shellSettings));
            }

            builder.AddRazorOptions(options =>
            {
                var extensionLibraryService = serviceProvider.GetService<IExtensionLibraryService>();
                ((List<MetadataReference>)options.AdditionalCompilationReferences).AddRange(extensionLibraryService.MetadataReferences());
            });
        }

        private class DesignTimeHttpContextAccessor : IHttpContextAccessor
        {
            public DesignTimeHttpContextAccessor(IServiceProvider serviceProvider, ShellSettings shellSettings)
            {
                HttpContext = new DefaultHttpContext();
                HttpContext.RequestServices = serviceProvider;
                HttpContext.Features[typeof(ShellSettings)] = shellSettings;
            }

            public HttpContext HttpContext { get; set; }
        }
    }
}