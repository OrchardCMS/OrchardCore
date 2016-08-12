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

namespace Orchard.ContentManagement.Display
{
    /// <summary>
    /// Provides a concrete implementation of a display handler coordinating <see cref="IContentFieldDisplayDriver"/>
    /// and <see cref="IContentPartDisplayDriver"/> instances.
    /// </summary>
    public class ContentPartDisplayCoordinator : IContentDisplayHandler
    {
        private readonly IEnumerable<IContentFieldDisplayDriver> _fieldDisplayDrivers;
        private readonly IEnumerable<IContentPartDisplayDriver> _partDisplayDrivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;

        public ContentPartDisplayCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentFieldDisplayDriver> fieldDisplayDrivers,
            IEnumerable<IContentPartDisplayDriver> partDisplayDrivers,
            IEnumerable<IContentPartDriver> partDrivers,
            ILogger<ContentPartDisplayCoordinator> logger)
        {
            _contentPartDrivers = partDrivers;
            _contentDefinitionManager = contentDefinitionManager;
            _fieldDisplayDrivers = fieldDisplayDrivers;
            _partDisplayDrivers = partDisplayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public async Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context)
        {
            // Optimized implementation for display

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if(contentTypeDefinition == null)
            {
                return;
            }

            var partInfos = _contentPartDrivers.Select(x => x.GetPartInfo()).ToDictionary(x => x.PartName);

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partType = partInfos.ContainsKey(partTypeName) ? partInfos[partTypeName].Factory(contentTypePartDefinition).GetType() : null;
                var part = contentItem.Get(partType ?? typeof(ContentPart), partName) as ContentPart;

                foreach (var displayDriver in _partDisplayDrivers)
                {
                    try
                    {
                        var result = await displayDriver.BuildDisplayAsync(part, contentTypePartDefinition, context);
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

                foreach (var contentPartFieldDefinition in contentTypePartDefinition.PartDefinition.Fields)
                {
                    foreach (var displayDriver in _fieldDisplayDrivers)
                    {
                        try
                        {
                            var result = await displayDriver.BuildDisplayAsync(part, contentPartFieldDefinition, contentTypePartDefinition, context);
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
            return Process(model,
                async (partFieldDefinition, contentTypePartDefinition, part) =>
                    await _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                        var result = await contentDisplay.BuildEditorAsync(part, partFieldDefinition, contentTypePartDefinition, context);
                        if (result != null)
                            result.Apply(context);
                    }, Logger),
                async (contentTypePartDefinition, part) =>
                    await _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                        var result = await contentDisplay.BuildEditorAsync(part, contentTypePartDefinition, context);
                        if (result != null)
                            result.Apply(context);
                    }, Logger)
                );
        }

        public Task UpdateEditorAsync(ContentItem model, UpdateEditorContext context)
        {
            return Process(model,
                async (partFieldDefinition, contentTypePartDefinition, part) =>
                    await _fieldDisplayDrivers.InvokeAsync(async contentDisplay => {
                    var result = await contentDisplay.UpdateEditorAsync(part, partFieldDefinition, contentTypePartDefinition, context);
                    if (result != null)
                        result.Apply(context);
                    }, Logger),
                async (contentTypePartDefinition, part) =>
                    await _partDisplayDrivers.InvokeAsync(async contentDisplay => {
                        var result = await contentDisplay.UpdateEditorAsync(part, contentTypePartDefinition, context);
                        if (result != null)
                            result.Apply(context);
                    }, Logger)
                );
        }

        public Task Process(ContentItem contentItem,
            Func<ContentPartFieldDefinition, ContentTypePartDefinition, ContentPart, Task> processField,
            Func<ContentTypePartDefinition, ContentPart, Task> processPart
            )
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

                ContentPartInfo partInfo;
                ContentPart part;

                if (partInfos.TryGetValue(typePartDefinition.PartDefinition.Name, out partInfo))
                {
                    // It's a well-known part type, bind the data to the model
                    part = partInfo.Factory(typePartDefinition);
                }
                else
                {
                    // Generic content part model (custom part)
                    part = new ContentPart();
                }

                part = contentItem.GetOrCreate(part.GetType(), () => new ContentPart(), typePartDefinition.Name) as ContentPart;

                processPart(typePartDefinition, part);

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;
                    processField(partFieldDefinition, typePartDefinition, part);
                }
            }

            return Task.CompletedTask;
        }
    }
}
