using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContainerRoute.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContainerRoute.Drivers
{
    public class RouteHandlerPartDisplay : ContentPartDisplayDriver<RouteHandlerPart>
    {
        //TODO move to validator
        public static char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,.;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;

        private readonly IContentRoutingValidationCoordinator _contentRoutingValidationCoordinator;
        private readonly IStringLocalizer<ContainerRoutePartDisplay> S;

        public RouteHandlerPartDisplay(
            IContentRoutingValidationCoordinator contentRoutingValidationCoordinator,
            IStringLocalizer<ContainerRoutePartDisplay> localizer
            )
        {
            _contentRoutingValidationCoordinator = contentRoutingValidationCoordinator;
            S = localizer;
        }

        public override IDisplayResult Edit(RouteHandlerPart routeHandlerPart, BuildPartEditorContext context)
        {
            return Initialize<RouteHandlerPartViewModel>("RouteHandlerPart_Edit", model =>
            {
                model.Path = routeHandlerPart.Path;
                model.RouteHandlerPart = routeHandlerPart;
                model.IsRelative = routeHandlerPart.IsRelative;
                model.IsRoutable = routeHandlerPart.IsRoutable;

                model.Settings = context.TypePartDefinition.GetSettings<RouteHandlerPartSettings>();
            });
        }


        public override async Task<IDisplayResult> UpdateAsync(RouteHandlerPart model, UpdatePartEditorContext context)
        {
            var viewModel = new RouteHandlerPartViewModel();

            await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Path, t => t.UpdatePath, t => t.IsRelative, t => t.IsRoutable);

            var settings = context.TypePartDefinition.GetSettings<RouteHandlerPartSettings>();

            if (settings.AllowCustomPath)
            {
                model.Path = viewModel.Path;
            }

            if (settings.AllowUpdatePath && viewModel.UpdatePath)
            {
                // Make it empty to force a regeneration
                model.Path = "";
            }

            model.IsRelative = viewModel.IsRelative;

            // TODO make this disable / enable like allow update path in the settings.
            model.IsRoutable = viewModel.IsRoutable;

            await ValidateAsync(model, context.Updater);

            return Edit(model, context);
        }

        // TODO move to validator.
        private async Task ValidateAsync(RouteHandlerPart containerRoutePart, IUpdateModel updater)
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
