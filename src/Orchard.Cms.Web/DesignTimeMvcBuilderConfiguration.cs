using System.Collections.Generic;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;

namespace Orchard.Cms.Web
{
    public class DesignTimeMvcBuilderConfiguration : IDesignTimeMvcBuilderConfiguration
    {
        public void ConfigureMvc(IMvcBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var env = serviceProvider.GetRequiredService<IHostingEnvironment>();

            var services = new ServiceCollection();
            services.AddSingleton<IHostingEnvironment>(env);

            var startUp = new Startup(env);
            startUp.ConfigureServices(services);

            serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            var app = new ApplicationBuilder(serviceProvider);
            startUp.Configure(app, env, loggerFactory);

            builder.AddRazorOptions(options =>
            {
                var extensionLibraryService = services.BuildServiceProvider().GetService<IExtensionLibraryService>();
                ((List<MetadataReference>)options.AdditionalCompilationReferences).AddRange(extensionLibraryService.MetadataReferences());
            });
        }
    }
}