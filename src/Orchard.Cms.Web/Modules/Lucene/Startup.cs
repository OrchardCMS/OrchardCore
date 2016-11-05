using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orchard.Environment.Shell;
using Orchard.Indexing;

namespace Lucene
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var indexManager = serviceProvider.GetRequiredService<IndexManager>();

            var shellOptions = serviceProvider.GetService<IOptions<ShellOptions>>();
            var shellSettings = serviceProvider.GetService<ShellSettings>();

            var rootIndexPath = Path.Combine( shellOptions.Value.ShellsRootContainerName, shellOptions.Value.ShellsContainerName, shellSettings.Name, "Lucene");

            indexManager.Providers.Add("Lucene", new LuceneIndexProvider(serviceProvider.GetService<IHostingEnvironment>(), rootIndexPath));
        }
    }
}
