using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Liquid.Fields;
using OrchardCore.Liquid.Models;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Liquid.Security;

internal sealed class LiquidContentAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly HashSet<string> s_mutationPermissions = new(StringComparer.Ordinal)
    {
        CommonPermissions.EditContent.Name,
        CommonPermissions.EditOwnContent.Name,
        CommonPermissions.PublishContent.Name,
        CommonPermissions.PublishOwnContent.Name,
    };

    private readonly IServiceProvider _serviceProvider;

    public LiquidContentAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!s_mutationPermissions.Contains(requirement.Permission.Name))
        {
            return;
        }

        if (context.Resource is ContentItem contentItem)
        {
            if (!await RequiresLiquidPermissionAsync(contentItem))
            {
                return;
            }
        }
        else
        {
            var contentType = context.Resource switch
            {
                ContentTypeDefinition definition => definition.Name,
                string contentTypeName => contentTypeName,
                _ => null,
            };

            if (string.IsNullOrEmpty(contentType))
            {
                return;
            }

            var contentDefinitionManager = _serviceProvider.GetService<IContentDefinitionManager>();

            if (contentDefinitionManager is null ||
                !RequiresLiquidPermission(await contentDefinitionManager.GetTypeDefinitionAsync(contentType)))
            {
                return;
            }
        }

        var authorizationService = _serviceProvider.GetRequiredService<IAuthorizationService>();

        if (!await authorizationService.AuthorizeAsync(context.User, Permissions.ManageLiquidTemplates))
        {
            context.Fail();
        }
    }

    private async Task<bool> RequiresLiquidPermissionAsync(ContentItem contentItem)
    {
        var contentDefinitionManager = _serviceProvider.GetService<IContentDefinitionManager>();

        if (contentDefinitionManager is null)
        {
            return false;
        }

        if (RequiresLiquidPermission(
            await contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType)))
        {
            return true;
        }

        var checkedContentTypes = new Dictionary<string, bool>(StringComparer.Ordinal);

        return await ContainsProtectedContentItemAsync(
            (JsonObject)contentItem.Content,
            contentDefinitionManager,
            checkedContentTypes);
    }

    private static async Task<bool> ContainsProtectedContentItemAsync(
        JsonNode node,
        IContentDefinitionManager contentDefinitionManager,
        Dictionary<string, bool> checkedContentTypes)
    {
        if (node is null)
        {
            return false;
        }

        if (node is JsonObject jsonObject)
        {
            if (jsonObject[nameof(ContentItem.ContentType)] is JsonValue contentTypeValue &&
                contentTypeValue.TryGetValue<string>(out var contentType) &&
                !string.IsNullOrEmpty(contentType))
            {
                if (!checkedContentTypes.TryGetValue(contentType, out var requiresLiquidPermission))
                {
                    requiresLiquidPermission = RequiresLiquidPermission(
                        await contentDefinitionManager.GetTypeDefinitionAsync(contentType));
                    checkedContentTypes[contentType] = requiresLiquidPermission;
                }

                if (requiresLiquidPermission)
                {
                    return true;
                }
            }

            foreach (var property in jsonObject)
            {
                if (await ContainsProtectedContentItemAsync(
                    property.Value,
                    contentDefinitionManager,
                    checkedContentTypes))
                {
                    return true;
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (await ContainsProtectedContentItemAsync(
                    item,
                    contentDefinitionManager,
                    checkedContentTypes))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool RequiresLiquidPermission(ContentTypeDefinition contentTypeDefinition)
    {
        if (contentTypeDefinition is null)
        {
            return false;
        }

        foreach (var typePartDefinition in contentTypeDefinition.Parts)
        {
            if (string.Equals(
                typePartDefinition.PartDefinition.Name,
                nameof(LiquidPart),
                StringComparison.Ordinal))
            {
                return true;
            }

            foreach (var fieldDefinition in typePartDefinition.PartDefinition.Fields)
            {
                if (string.Equals(
                    fieldDefinition.FieldDefinition.Name,
                    nameof(LiquidField),
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
