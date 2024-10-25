using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Models;
using OrchardCore.Autoroute.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Autoroute.Drivers;

public sealed class AutoroutePartDisplayDriver : ContentPartDisplayDriver<AutoroutePart>
{
    private readonly AutorouteOptions _options;
    private readonly ISiteService _siteService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly YesSql.ISession _session;

    internal readonly IStringLocalizer S;

    public AutoroutePartDisplayDriver(
        IOptions<AutorouteOptions> options,
        ISiteService siteService,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        YesSql.ISession session,
        IStringLocalizer<AutoroutePartDisplayDriver> localizer
    )
    {
        _options = options.Value;
        _siteService = siteService;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _session = session;
        S = localizer;
    }

    public override IDisplayResult Edit(AutoroutePart autoroutePart, BuildPartEditorContext context)
    {
        return Initialize<AutoroutePartViewModel>("AutoroutePart_Edit", async model =>
        {
            model.Path = autoroutePart.Path;
            model.AutoroutePart = autoroutePart;
            model.ContentItem = autoroutePart.ContentItem;
            model.SetHomepage = false;

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var homeRoute = siteSettings.HomeRoute;

            if (homeRoute != null && homeRoute.TryGetValue(_options.ContainedContentItemIdKey, out var containedContentItemId))
            {
                if (string.Equals(autoroutePart.ContentItem.ContentItemId, containedContentItemId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    model.IsHomepage = true;
                }
            }
            else if (string.Equals(autoroutePart.ContentItem.ContentItemId, homeRoute?[_options.ContentItemIdKey]?.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                model.IsHomepage = true;
            }

            model.Disabled = autoroutePart.Disabled;
            model.Absolute = autoroutePart.Absolute;
            model.RouteContainedItems = autoroutePart.RouteContainedItems;

            model.Settings = context.TypePartDefinition.GetSettings<AutoroutePartSettings>();
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(AutoroutePart model, UpdatePartEditorContext context)
    {
        var viewModel = new AutoroutePartViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix,
            t => t.Path,
            t => t.UpdatePath,
            t => t.RouteContainedItems,
            t => t.Absolute,
            t => t.Disabled);

        var settings = context.TypePartDefinition.GetSettings<AutoroutePartSettings>();

        model.Disabled = viewModel.Disabled;
        model.Absolute = viewModel.Absolute;
        model.RouteContainedItems = viewModel.RouteContainedItems;

        // When disabled these values are not updated.
        if (!model.Disabled)
        {
            if (settings.AllowCustomPath)
            {
                model.Path = viewModel.Path;
            }

            if (settings.AllowUpdatePath && viewModel.UpdatePath)
            {
                // Make it empty to force a regeneration
                model.Path = string.Empty;
            }

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null && await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.SetHomepage))
            {
                await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.SetHomepage);
            }

            context.Updater.ModelState.BindValidationResults(Prefix, model.ValidatePathFieldValue(S));

            // This can only validate the path if the Autoroute is not managing content item routes or the path is absolute.
            if (!string.IsNullOrEmpty(model.Path) && (!settings.ManageContainedItemRoutes || (settings.ManageContainedItemRoutes && model.Absolute)))
            {
                var path = model.Path.Trim('/');
                var paths = new string[] { path, "/" + path, path + "/", "/" + path + "/" };

                var possibleConflicts = await _session.QueryIndex<AutoroutePartIndex>(o => (o.Published || o.Latest) && o.Path.IsIn(paths)).ListAsync();
                if (possibleConflicts.Any(x => x.ContentItemId != model.ContentItem.ContentItemId && x.ContainedContentItemId != model.ContentItem.ContentItemId))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Path), S["Your permalink is already in use."]);
                }
            }
        }

        return Edit(model, context);
    }
}
