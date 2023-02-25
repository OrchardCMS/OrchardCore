using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Services;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Handler;

public class ElasticsearchAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    private IAuthorizationService _authorizationService;
    private ISiteService _siteService;

    public ElasticsearchAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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

        var service = _serviceProvider.GetService<ElasticsearchService>();

        if (service == null || service.Name != parameters.ServiceName)
        {
            // Only validate if Elasticsearch is requested.
            return;
        }

        var indexName = await GetIndexNameAsync(parameters);

        if (!String.IsNullOrEmpty(indexName))
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
        if (!String.IsNullOrWhiteSpace(parameters.IndexName))
        {
            return parameters.IndexName.Trim();
        }

        _siteService ??= _serviceProvider.GetRequiredService<ISiteService>();
        var siteSettings = await _siteService.GetSiteSettingsAsync();

        return siteSettings.As<ElasticSettings>().SearchIndex;
    }
}
