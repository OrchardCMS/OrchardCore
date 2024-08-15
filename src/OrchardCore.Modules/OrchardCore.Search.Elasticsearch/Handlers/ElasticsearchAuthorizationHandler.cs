using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Services;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Handler;

public class ElasticsearchAuthorizationHandler(IServiceProvider serviceProvider) : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private IAuthorizationService _authorizationService;
    private ISiteService _siteService;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded)
        {
            // This handler is not revoking any pre-existing grants.
            return;
        }

        if (context.Resource is not SearchPermissionParameters parameters)
        {
            return;
        }

        if (ElasticsearchService.Key != parameters.ServiceName)
        {
            // Only validate if Elasticsearch is requested.
            return;
        }

        var indexName = await GetIndexNameAsync(parameters);

        if (!string.IsNullOrEmpty(indexName))
        {
            _authorizationService ??= _serviceProvider?.GetService<IAuthorizationService>();

            var permission = ElasticsearchIndexPermissionHelper.GetElasticIndexPermission(indexName);

            if (await _authorizationService.AuthorizeAsync(context.User, permission))
            {
                context.Succeed(requirement);

                return;
            }
        }
    }

    private async Task<string> GetIndexNameAsync(SearchPermissionParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.IndexName))
        {
            return parameters.IndexName.Trim();
        }

        _siteService ??= _serviceProvider.GetRequiredService<ISiteService>();

        return (await _siteService.GetSettingsAsync<ElasticSettings>()).SearchIndex;
    }
}
