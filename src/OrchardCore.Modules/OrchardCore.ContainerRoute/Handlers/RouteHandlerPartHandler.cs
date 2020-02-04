using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.ContainerRoute.Drivers;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContainerRoute.Routing;
using OrchardCore.ContainerRoute.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Liquid;

namespace OrchardCore.ContainerRoute.Handlers
{
    public class RouteHandlerPartHandler : ContentPartHandler<RouteHandlerPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentRoutingValidationCoordinator _contentRoutingValidationCoordinator;

        public RouteHandlerPartHandler(
            ILiquidTemplateManager liquidTemplateManager,
            IContentDefinitionManager contentDefinitionManager,
            IContentRoutingValidationCoordinator contentRoutingValidationCoordinator
            )
        {
            _liquidTemplateManager = liquidTemplateManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentRoutingValidationCoordinator = contentRoutingValidationCoordinator;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, RouteHandlerPart part)
        {
            return context.ForAsync<RouteHandlerAspect>(aspect =>
            {
                aspect.Path = part.Path;
                aspect.IsRelative = part.IsRelative;
                aspect.IsRouteable = part.IsRoutable;

                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// This cannot validate the content item pattern because it does not know it's parent.
        /// The pattern will be validated when updating the parent.
        /// </summary>
        public override async Task UpdatedAsync(UpdateContentContext context, RouteHandlerPart part)
        {
            //TODO we can validate an absolute path here.

            // Compute the Path only if it's empty
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                return;
            }

            var pattern = GetPattern(part);

            if (!String.IsNullOrEmpty(pattern))
            {
                var model = new RouteHandlerPartViewModel()
                {
                    Path = part.Path,
                    RouteHandlerPart = part,
                    ContentItem = part.ContentItem
                };

                part.Path = await _liquidTemplateManager.RenderAsync(pattern, NullEncoder.Default, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));

                part.Path = part.Path.Replace("\r", String.Empty).Replace("\n", String.Empty);

                if (part.Path?.Length > ContainerRoutePartDisplay.MaxPathLength)
                {
                    part.Path = part.Path.Substring(0, ContainerRoutePartDisplay.MaxPathLength);
                }

                part.Apply();
            }
        }

        /// <summary>
        /// Get the pattern from the RouteHandlerPartSettings property for its type
        /// </summary>
        private string GetPattern(RouteHandlerPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "RouteHandlerPart"));
            var pattern = contentTypePartDefinition.GetSettings<ContainerRoutePartSettings>().Pattern;

            return pattern;
        }
    }
}
