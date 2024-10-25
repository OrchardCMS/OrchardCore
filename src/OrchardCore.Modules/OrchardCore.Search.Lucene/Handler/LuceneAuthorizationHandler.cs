using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Handler;

public class LuceneAuthorizationHandler(IServiceProvider serviceProvider) : AuthorizationHandler<PermissionRequirement>
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

        if (LuceneSearchService.Key != parameters.ServiceName)
        {
            // Only validate if Lucene is requested.
            return;
        }

        var indexName = await GetIndexNameAsync(parameters);

        if (!string.IsNullOrEmpty(indexName))
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
        if (!string.IsNullOrWhiteSpace(parameters.IndexName))
        {
            return parameters.IndexName.Trim();
        }

        _siteService ??= _serviceProvider.GetRequiredService<ISiteService>();

        return (await _siteService.GetSettingsAsync<LuceneSettings>()).SearchIndex;
    }
}
