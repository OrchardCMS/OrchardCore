using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Handler;

public class LuceneAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    private IAuthorizationService _authorizationService;
    private ISiteService _siteService;

    public LuceneAuthorizationHandler(IServiceProvider serviceProvider)
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

        var service = _serviceProvider.GetService<LuceneSearchService>();

        if (service == null || service.Name != parameters.ServiceName)
        {
            // Only validate if Lucene is requested.
            return;
        }

        var indexName = await GetIndexNameAsync(parameters);

        if (!String.IsNullOrEmpty(indexName))
        {
            _authorizationService ??= _serviceProvider?.GetService<IAuthorizationService>();

            var permission = LuceneIndexPermissionHelper.GetLuceneIndexPermission(indexName);

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

        return siteSettings.As<LuceneSettings>().SearchIndex;
    }
}
