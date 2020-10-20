using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Autoroute.Models;
using OrchardCore.Autoroute.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;

namespace OrchardCore.Autoroute.Drivers
{
    public class AutoroutePartDisplay : ContentPartDisplayDriver<AutoroutePart>
    {
        public static char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,.;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;

        private readonly AutorouteOptions _options;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly YesSql.ISession _session;
        private readonly IStringLocalizer S;

        public AutoroutePartDisplay(
            IOptions<AutorouteOptions> options,
            ISiteService siteService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            YesSql.ISession session,
            IStringLocalizer<AutoroutePartDisplay> localizer
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

        public override async Task<IDisplayResult> UpdateAsync(AutoroutePart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new AutoroutePartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Path, t => t.UpdatePath, t => t.RouteContainedItems, t => t.Absolute, t => t.Disabled);

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
                    model.Path = "";
                }

                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext != null && await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.SetHomepage))
                {
                    await updater.TryUpdateModelAsync(model, Prefix, t => t.SetHomepage);
                }

                var errors = await context.ValidateAsync(model);
                updater.ModelState.BindValidationResults(Prefix, errors);
            }

            return Edit(model, context);
        }

    }
}
