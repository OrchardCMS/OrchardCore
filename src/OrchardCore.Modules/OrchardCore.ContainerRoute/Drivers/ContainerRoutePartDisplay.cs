using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContainerRoute.Routing;
using OrchardCore.ContainerRoute.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;

namespace OrchardCore.ContainerRoute.Drivers
{
    public class ContainerRoutePartDisplay : ContentPartDisplayDriver<ContainerRoutePart>
    {
        public static char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,.;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;

        private readonly ContainerRouteOptions _options;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IContentRoutingValidationCoordinator _contentRoutingValidationCoordinator;
        private readonly IStringLocalizer<ContainerRoutePartDisplay> S;

        public ContainerRoutePartDisplay(
            IOptions<ContainerRouteOptions> options,
            ISiteService siteService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IContentRoutingValidationCoordinator contentRoutingValidationCoordinator,
            IStringLocalizer<ContainerRoutePartDisplay> localizer
            )
        {
            _options = options.Value;
            _siteService = siteService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _contentRoutingValidationCoordinator = contentRoutingValidationCoordinator;
            S = localizer;
        }

        public override IDisplayResult Edit(ContainerRoutePart containerRoutePart, BuildPartEditorContext context)
        {
            return Initialize<ContainerRoutePartViewModel>("ContainerRoutePart_Edit", async model =>
            {
                model.Path = containerRoutePart.Path;
                model.ContainerRoutePart = containerRoutePart;
                model.SetHomepage = false;

                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var homeRoute = siteSettings.HomeRoute;

                if (containerRoutePart.ContentItem.ContentItemId == homeRoute?[_options.ContainerContentItemIdKey]?.ToString())
                {
                    model.IsHomepage = true;
                }

                model.Settings = context.TypePartDefinition.GetSettings<ContainerRoutePartSettings>();
            });
        }


        public override async Task<IDisplayResult> UpdateAsync(ContainerRoutePart model, UpdatePartEditorContext context)
        {
            var viewModel = new ContainerRoutePartViewModel();

            await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Path, t => t.UpdatePath);

            var settings = context.TypePartDefinition.GetSettings<ContainerRoutePartSettings>();

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

            if (httpContext != null)// TODO && await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.SetHomepage))
            {
                await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.SetHomepage);
            }

            await ValidateAsync(model, context.Updater);

            return Edit(model, context);
        }

        private async Task ValidateAsync(ContainerRoutePart containerRoutePart, IUpdateModel updater)
        {
            if (containerRoutePart.Path == "/")
            {
                updater.ModelState.AddModelError(Prefix, nameof(containerRoutePart.Path), S["Your permalink can't be set to the homepage, please use the homepage option instead."]);
            }

            if (containerRoutePart.Path?.IndexOfAny(InvalidCharactersForPath) > -1 || containerRoutePart.Path?.IndexOf(' ') > -1)
            {
                var invalidCharactersForMessage = string.Join(", ", InvalidCharactersForPath.Select(c => $"\"{c}\""));
                updater.ModelState.AddModelError(Prefix, nameof(containerRoutePart.Path), S["Please do not use any of the following characters in your permalink: {0}. No spaces are allowed (please use dashes or underscores instead).", invalidCharactersForMessage]);
            }

            if (containerRoutePart.Path?.Length > MaxPathLength)
            {
                updater.ModelState.AddModelError(Prefix, nameof(containerRoutePart.Path), S["Your permalink is too long. The permalink can only be up to {0} characters.", MaxPathLength]);
            }

            if (containerRoutePart.Path != null && !await _contentRoutingValidationCoordinator.IsPathUniqueAsync(containerRoutePart.Path, containerRoutePart.ContentItem.ContentItemId))
            {
                updater.ModelState.AddModelError(Prefix, nameof(containerRoutePart.Path), S["Your permalink is already in use."]);
            }
        }
    }
}
