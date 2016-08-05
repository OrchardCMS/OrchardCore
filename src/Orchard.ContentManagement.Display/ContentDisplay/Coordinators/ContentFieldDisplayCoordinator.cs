using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// Provides a concrete implementation of a display coordinator managing <see cref="IContentDisplayDriver"/>
    /// implementations.
    /// </summary>
    public class ContentFieldDisplayCoordinator : IContentDisplayHandler
    {
        private readonly IEnumerable<IContentFieldDisplayDriver> _displayDrivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILogger<ContentDisplayCoordinator> _logger;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;

        public ContentFieldDisplayCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentFieldDisplayDriver> displayDrivers,
            IEnumerable<IContentPartDriver> partDrivers,
            ILogger<ContentDisplayCoordinator> logger)
        {
            _contentPartDrivers = partDrivers;
            _logger = logger;
            _contentDefinitionManager = contentDefinitionManager;
            _displayDrivers = displayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public async Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context)
        {
            // Optimized implementation for display

            // For each field on the content item, invoke all IContentFieldDisplayDriver instances

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if(contentTypeDefinition == null)
            {
                return;
            }

            var partInfos = _contentPartDrivers.Select(x => x.GetPartInfo()).ToDictionary(x => x.PartName);

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.PartDefinition.Name;
                var partType = partInfos.ContainsKey(partName) ? partInfos[partName].Factory(contentTypePartDefinition).GetType() : null;
                var part = contentItem.Get(partType ?? typeof(ContentPart), partName) as ContentPart;

                foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = contentPartFieldDefinition.Name;

                    foreach (var displayDriver in _displayDrivers)
                    {
                        try
                        {
                            var result = await displayDriver.BuildDisplayAsync(fieldName, part, contentPartFieldDefinition, contentTypePartDefinition, context);
                            if (result != null)
                            {
                                result.Apply(context);
                            }
                        }
                        catch (Exception ex)
                        {
                            InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, "BuildDisplayAsync");
                        }
                    }
                }
            }
        }

        public Task BuildEditorAsync(ContentItem model, BuildEditorContext context)
        {
            return Process(model, async (partFieldDefinition, contentTypePartDefinition, part, fieldName) =>
                await _displayDrivers.InvokeAsync(async contentDisplay => {
                    var result = await contentDisplay.BuildEditorAsync(fieldName, part, partFieldDefinition, contentTypePartDefinition, context);
                    if (result != null)
                        result.Apply(context);
                }, Logger)
            );
        }

        public Task UpdateEditorAsync(ContentItem model, UpdateEditorContext context)
        {
            return Process(model, async (partFieldDefinition, contentTypePartDefinition, part, fieldName) =>
                await _displayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(fieldName, part, partFieldDefinition, contentTypePartDefinition, context);
                if (result != null)
                    result.Apply(context);
                }, Logger)
            );
        }

        public Task Process(ContentItem contentItem, Func<ContentPartFieldDefinition, ContentTypePartDefinition, ContentPart, string, Task> action)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return Task.CompletedTask;

            var partInfos = _contentPartDrivers.Select(x => x.GetPartInfo()).ToDictionary(x => x.PartName);

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                // Abort if there are not fields in this part
                if (!typePartDefinition.PartDefinition.Fields.Any())
                {
                    continue;
                }

                var partName = typePartDefinition.PartDefinition.Name;
                ContentPartInfo partInfo;

                ContentPart part = partInfos.TryGetValue(partName, out partInfo)
                    ? partInfo.Factory(typePartDefinition)
                    : new ContentPart();

                part = contentItem.Get(part.GetType(), partName) as ContentPart;

                if(part == null)
                {
                    return Task.CompletedTask;
                }

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;
                    action(partFieldDefinition, typePartDefinition, part, fieldName);
                }
            }

            return Task.CompletedTask;
        }
    }
}
