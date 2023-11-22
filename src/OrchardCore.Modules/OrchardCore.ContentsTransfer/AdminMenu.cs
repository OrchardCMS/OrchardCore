using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentsTransfer.Controllers;
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

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals("admin", name, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var adminControllerName = typeof(AdminController).ControllerName();

        builder
            .Add(S["Content"], content => content
                .Add(S["Bulk Transfers"], S["Bulk Transfers"].PrefixPosition(), transfer => transfer
                    .Action(nameof(AdminController.List), adminControllerName, new
                    {
                        area = ContentTransferConstants.Feature.ModuleId
                    })
                    .Permission(ContentTransferPermissions.ListContentTransferEntries)
                )
            );

        return Task.CompletedTask;
    }
}
