using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Display
{
    /// <summary>
    /// Provides a concrete implementation of a display handler coordinating part, field and content item drivers.
    /// </summary>
    public class ContentPartDisplayCoordinator : IContentPartDisplayHandler
    {
        private readonly IContentPartDisplayDriverResolver _contentPartDisplayDriverResolver;
        private readonly IContentFieldDisplayDriverResolver _contentFieldDisplayDriverResolver;
        private readonly IEnumerable<IContentDisplayDriver> _displayDrivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly ILogger _logger;

        public ContentPartDisplayCoordinator(
            IContentPartDisplayDriverResolver contentPartDisplayDriverResolver,
            IContentFieldDisplayDriverResolver contentFieldDisplayDriverResolver,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentDisplayDriver> displayDrivers,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            ILogger<ContentItemDisplayCoordinator> logger)
        {
            _contentPartDisplayDriverResolver = contentPartDisplayDriverResolver;
            _contentFieldDisplayDriverResolver = contentFieldDisplayDriverResolver;
            _contentPartFactory = contentPartFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _displayDrivers = displayDrivers;
            _logger = logger;
        }

        public async Task BuildDisplayAsync(ContentPart contentPart, BuildDisplayContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentPart.ContentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return;
            }

            // We kind of do need these. It screws up metadata though, because there will be multiples.

            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.BuildDisplayAsync(contentPart.ContentItem, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, _logger, displayDriver.GetType().Name, nameof(BuildDisplayAsync));
                }
            }

            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => x.Name == contentPart.GetType().Name);
            var partName = contentTypePartDefinition.Name;
            var partTypeName = contentTypePartDefinition.PartDefinition.Name;
            var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
            var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
            var part = contentPart.ContentItem.Get(partActivator.Type, partName) as ContentPart;

            if (part != null)
            {
                var partDisplayDrivers = _contentPartDisplayDriverResolver.GetDisplayModeDrivers(partTypeName, contentTypePartDefinition.DisplayMode());
                foreach (var partDisplayDriver in partDisplayDrivers)
                {
                    try
                    {
                        var result = await partDisplayDriver.BuildDisplayAsync(part, contentTypePartDefinition, context);
                        if (result != null)
                        {
                            await result.ApplyAsync(context);
                        }
                    }
                    catch (Exception ex)
                    {
                        InvokeExtensions.HandleException(ex, _logger, partDisplayDrivers.GetType().Name, nameof(BuildDisplayAsync));
                    }
                }
                    // This no longer makes sense.
                    // as you'd have to make your own shape to build those fields.

                    // Create a custom ContentPart shape that will hold the fields for dynamic content part (not implicit parts)
                    // This allows its fields to be grouped and templated


            }
        }
    }
}
