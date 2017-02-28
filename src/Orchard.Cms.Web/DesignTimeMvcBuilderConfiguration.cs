using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.PlatformAbstractions;
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

            var startUp = new Startup(env);
            startUp.ConfigureServices(builder.Services);
            serviceProvider = builder.Services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var app = new ApplicationBuilder(serviceProvider);
            startUp.Configure(app, loggerFactory);

            var assemblyFileName = Path.Combine(ApplicationEnvironment.ApplicationBasePath,
                env.ApplicationName + ".PrecompiledViews.dll");

            if (File.Exists(assemblyFileName))
            {
                File.Delete(assemblyFileName);
            }

            var orchardHost = serviceProvider.GetRequiredService<IOrchardHost>();
            var runningShellTable = serviceProvider.GetRequiredService<IRunningShellTable>();

            orchardHost.Initialize();
            var ShellSettings = runningShellTable.Match(new DefaultHttpContext());
            var shellContext = orchardHost.GetOrCreateShellContext(ShellSettings);

            builder.Services.AddSingleton<IHttpContextAccessor, DesignTimeHttpContextAccessor>(s =>
                    new DesignTimeHttpContextAccessor(shellContext.ServiceProvider));

            builder.AddRazorOptions(options =>
            {
                var extensionLibraryService = shellContext.ServiceProvider.GetService<IExtensionLibraryService>();
                ((List<MetadataReference>)options.AdditionalCompilationReferences).AddRange(extensionLibraryService.MetadataReferences());
            });
        }

        private class DesignTimeHttpContextAccessor : IHttpContextAccessor
        {
            public DesignTimeHttpContextAccessor(IServiceProvider serviceProvider)
            {
                HttpContext = new DefaultHttpContext();
                HttpContext.RequestServices = serviceProvider;
            }

            public HttpContext HttpContext { get; set; }
        }
    }
}