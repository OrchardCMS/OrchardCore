using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Security;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Contents;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Contents" },
        { "contentTypeId", string.Empty },
    };

    private static readonly RouteValueDictionary _adminListRouteValues = new()
    {
        { "area", "OrchardCore.Contents" },
        { "controller", "Admin" },
        { "Action", "List" },
    };

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;

    internal readonly IStringLocalizer S;

    public AdminMenu(
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService,
        ISiteService siteService,
        IStringLocalizer<AdminMenu> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
        _siteService = siteService;
        S = stringLocalizer;
    }

    protected override async ValueTask BuildAsync(NavigationBuilder builder)
    {
        var context = _httpContextAccessor.HttpContext;

        var contentTypeDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();
        var listableContentTypes = contentTypeDefinitions.Where(ctd => ctd.IsListable());
        await builder.AddAsync(S["Content"], NavigationConstants.AdminMenuContentPosition, async content =>
        {
            content.AddClass("content").Id("content");
            await content.AddAsync(S["Content Items"], S["Content Items"].PrefixPosition(), async contentItems =>
            {
                if (!await _authorizationService.AuthorizeContentTypeDefinitionsAsync(context.User, CommonPermissions.ListContent, listableContentTypes, _contentManager))
                {
                    contentItems.Permission(CommonPermissions.ListContent);
                }

                contentItems.Action(nameof(AdminController.List), typeof(AdminController).ControllerName(), _routeValues);
                contentItems.LocalNav();
            });
        });

        var adminSettings = await _siteService.GetSettingsAsync<AdminSettings>();

        if (adminSettings.DisplayNewMenu)
        {
            var creatableContentTypes = contentTypeDefinitions.Where(ctd => ctd.IsCreatable()).OrderBy(ctd => ctd.DisplayName);

            if (creatableContentTypes.Any())
            {
                await builder.AddAsync(S["New"], "-1", async newMenu =>
                {
                    newMenu.LinkToFirstChild(false).AddClass("new").Id("new");
                    foreach (var contentTypeDefinition in creatableContentTypes)
                    {
                        var ci = await _contentManager.NewAsync(contentTypeDefinition.Name);
                        var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);
                        var createRouteValues = cim.CreateRouteValues;
                        createRouteValues.Add("returnUrl", _linkGenerator.GetPathByRouteValues(context, string.Empty, _adminListRouteValues));

                        if (createRouteValues.Count > 0)
                        {
                            newMenu.Add(new LocalizedString(contentTypeDefinition.DisplayName, contentTypeDefinition.DisplayName), "5", item => item
                                .Action(cim.CreateRouteValues["Action"] as string, cim.CreateRouteValues["Controller"] as string, cim.CreateRouteValues)
                                .Permission(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditOwnContent.Name], contentTypeDefinition))
                                );
                        }
                    }
                });
            }
        }
    }
}
