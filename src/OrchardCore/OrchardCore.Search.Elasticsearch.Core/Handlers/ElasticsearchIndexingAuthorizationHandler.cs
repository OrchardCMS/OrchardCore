using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Elasticsearch;
using OrchardCore.Security;

namespace OrchardCore.Indexing.Core.Handlers;

public sealed class ElasticsearchIndexingAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    private IAuthorizationService _authorizationService;

    public ElasticsearchIndexingAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.HasSucceeded ||
            context.Resource is not IndexProfile { ProviderName: ElasticsearchConstants.ProviderName } indexProfile ||
            requirement.Permission != IndexingPermissions.QuerySearchIndex)
        {
            return;
        }

        _authorizationService ??= _serviceProvider?.GetService<IAuthorizationService>();
        var permission = ElasticsearchIndexPermissionHelper.GetElasticIndexPermission(indexProfile.IndexName);

        if (await _authorizationService.AuthorizeAsync(context.User, permission))
        {
            context.Succeed(requirement);
        }
    }
}
