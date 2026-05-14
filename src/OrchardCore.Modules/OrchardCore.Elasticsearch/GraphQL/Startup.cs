using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Modules;
using OrchardCore.Elasticsearch.GraphQL.Queries;

namespace OrchardCore.Elasticsearch.GraphQL;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
[RequireFeatures("OrchardCore.Apis.GraphQL", "OrchardCore.Queries")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISchemaBuilder, ElasticsearchQueryFieldTypeProvider>();
    }
}
