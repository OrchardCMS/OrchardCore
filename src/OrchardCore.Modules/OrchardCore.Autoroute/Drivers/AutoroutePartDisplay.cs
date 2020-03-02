using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Autoroute.Models;
using OrchardCore.Autoroute.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using YesSql;

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
        private readonly IStringLocalizer<AutoroutePartDisplay> S;

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
                model.SetHomepage = false;

                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var homeRoute = siteSettings.HomeRoute;

                if (autoroutePart.ContentItem.ContentItemId == homeRoute?[_options.ContentItemIdKey]?.ToString())
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

            if (settings.AllowCustomPath)
            {
                model.Path = viewModel.Path;
            }

            if (settings.AllowUpdatePath && viewModel.UpdatePath)
            {
                // Make it empty to force a regeneration
                model.Path = "";
            }

            model.Disabled = viewModel.Disabled;
            model.Absolute = viewModel.Absolute;
            model.RouteContainedItems = viewModel.RouteContainedItems;

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null && await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.SetHomepage))
            {
                await updater.TryUpdateModelAsync(model, Prefix, t => t.SetHomepage);
            }

            await ValidateAsync(model, updater, settings);

            return Edit(model, context);
        }

        private async Task ValidateAsync(AutoroutePart autoroute, IUpdateModel updater, AutoroutePartSettings settings)
        {
            if (autoroute.Path == "/")
            {
                updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), S["Your permalink can't be set to the homepage, please use the homepage option instead."]);
            }

            if (autoroute.Path?.IndexOfAny(InvalidCharactersForPath) > -1 || autoroute.Path?.IndexOf(' ') > -1)
            {
                var invalidCharactersForMessage = string.Join(", ", InvalidCharactersForPath.Select(c => $"\"{c}\""));
                updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), S["Please do not use any of the following characters in your permalink: {0}. No spaces are allowed (please use dashes or underscores instead).", invalidCharactersForMessage]);
            }

            if (autoroute.Path?.Length > MaxPathLength)
            {
                updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), S["Your permalink is too long. The permalink can only be up to {0} characters.", MaxPathLength]);
            }

            // TODO resolve for absolute routes. Hard to resolve for bag items as they are changing ids.
            // So for later...
            // Or choose that absolute routing is too hard to achieve in this scenario.

            //if (autoroute.Path != null &&
            //    (await _session.QueryIndex<AutoroutePartIndex>(o => o.Path == autoroute.Path &&
            //    o.ContentItemId != autoroute.ContentItem.ContentItemId &&
            //    o.ContainedContentItemId != autoroute.ContentItem.ContentItemId).CountAsync()) > 0)
            //{
            //    updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), S["Your permalink is already in use."]);

            //}

            // TODO a. clean this up.
            // b. this has no knowledge of whether it is contained or not
            // so path test will fail if it is supposed to be relative.
            // maybe with an Relative flag that is false by default
            // and must be turned on, we can skip path checking here.

            // We either need a RouteHandlerPart
            // Or a flag on the Settings called ManageContainedItemRoutes

            // Or the hack of binding the main contentItemId to see if it matches this one.
            // Too hacky!
            if (autoroute.Path != null && (!settings.ManageContainedItemRoutes || (settings.ManageContainedItemRoutes && autoroute.Absolute)))
            {
                var possibleConflicts = await _session.QueryIndex<AutoroutePartIndex>(o => o.Path == autoroute.Path).ListAsync();
                if (possibleConflicts.Any())
                {
                    var hasConflict = false;
                    if (possibleConflicts.Any(x => x.ContentItemId != autoroute.ContentItem.ContentItemId) &&
                        possibleConflicts.Any(x => x.ContainedContentItemId != autoroute.ContentItem.ContentItemId))
                    {
                        hasConflict = true;
                    }
                    if (hasConflict)
                    {
                        updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), S["Your permalink is already in use."]);
                    }
                }
            }
        }
    }
}
