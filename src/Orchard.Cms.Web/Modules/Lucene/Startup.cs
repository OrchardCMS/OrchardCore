using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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

            indexManager.Providers.Add("Lucene", new LuceneIndexProvider());
        }
    }
}
