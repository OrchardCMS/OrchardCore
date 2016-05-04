using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Hosting.Extensions;
using Orchard.Hosting.Web.Routing;

namespace Orchard.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder ConfigureWebHost(
            this IApplicationBuilder builder,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddOrchardLogging(builder.ApplicationServices);

            // Add diagnostices pages
            // TODO: make this modules or configurations
            builder.UseRuntimeInfoPage();
            builder.UseDeveloperExceptionPage();

            // Add static files to the request pipeline.
            builder.UseStaticFiles();

            // Ensure the shell tenants are loaded when a request comes in
            // and replaces the current service provider for the tenant's one.
            builder.UseMiddleware<OrchardContainerMiddleware>();

            // Route the request to the correct Orchard pipeline
            builder.UseMiddleware<OrchardRouterMiddleware>();

            
            // Load controllers and specific dependencies of extensions
            var applicationPartManager = builder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            var extensionManager = builder.ApplicationServices.GetRequiredService<IExtensionManager>();

            var assemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var appContext = ProjectContext.CreateContextForEachFramework("").FirstOrDefault();
            var appExporter = appContext.CreateExporter("Debug");

            foreach (var export in appExporter.GetAllExports())
            {
                foreach (var asset in export.CompilationAssemblies)
                {
                    assemblies.Add(asset.Name);
                }
            }

            foreach (var extension in extensionManager.AvailableExtensions())
            {
                var extensionEntry = extensionManager.LoadExtension(extension);
                applicationPartManager.ApplicationParts.Add(new AssemblyPart(extensionEntry.Assembly));

                if (!extensionEntry.Assembly.FullName.StartsWith("Orchard.Core")) {

                    var path =  Path.Combine(extensionEntry.Descriptor.Location, extensionEntry.Descriptor.Id);
                    var projectContext = ProjectContext.CreateContextForEachFramework(path).FirstOrDefault();
                    var projectExporter = projectContext.CreateExporter("Debug");

                    foreach (var export in projectExporter.GetAllExports())
                    {
                        foreach (var asset in export.CompilationAssemblies)
                        {
                            if (assemblies.Add(asset.Name)) {
                                try {
                                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(asset.ResolvedPath);
                                    applicationPartManager.ApplicationParts.Add(new AssemblyPart(assembly));
                                }
                                catch { }
                            }
                        }
                   }
                }
           }

            return builder;
        }
    }
}