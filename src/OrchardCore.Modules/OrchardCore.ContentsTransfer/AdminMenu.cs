using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.ContentsTransfer;

public class AdminMenu : INavigationProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IStringLocalizer S;

    public AdminMenu(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<AdminMenu> stringLocalizer,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        S = stringLocalizer;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals("admin", name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var controllerName = typeof(AdminController).ControllerName();

        builder.Add(S["Content"], content => content

            .Add(S["Imports"], S["Imports"].PrefixPosition(), imports =>
            {
                foreach (var contentTypeDefinition in _contentDefinitionManager.LoadTypeDefinitions())
                {
                    var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();
                    if (!settings.AllowBulkImport)
                    {
                        continue;
                    }

                    imports.Add(S["Import {0}", contentTypeDefinition.DisplayName], S["Import {0}", contentTypeDefinition.DisplayName].PrefixPosition(),
                        contentTypeBuilder => contentTypeBuilder
                            .Action(nameof(AdminController.Import), controllerName, new
                            {
                                area = ContentTransferConstants.Feature.ModuleId,
                                contentTypeId = contentTypeDefinition.Name,
                            })
                            .Permission(ImportPermissions.ImportContentFromFile)
                            .Resource(contentTypeDefinition.Name)
                        );
                }
            })
        );

        var showExport = false;

        foreach (var contentTypeDefinition in _contentDefinitionManager.LoadTypeDefinitions())
        {
            // check settings
            var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();
            if (settings.AllowBulkExport
                && await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, ImportPermissions.ExportContentFromFile, (object)contentTypeDefinition.Name))
            {
                showExport = true;
                break;
            }
        }

        if (showExport)
        {
            builder
                .Add(S["Content"], content => content
                    .Add(S["Export"], S["Export"].PrefixPosition(), exports => exports
                        .Action("Export", "Admin", new
                        {
                            area = ContentTransferConstants.Feature.ModuleId
                        })
                    )
                );
        }
    }
}
