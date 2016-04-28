using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
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
        private readonly IEnumerable<IContentPartDriver> _partDrivers;

        private readonly IDictionary<string, ContentPartInfo> _partInfos;

        public ContentFieldDisplayCoordinator(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentFieldDisplayDriver> displayDrivers,
            IEnumerable<IContentPartDriver> partDrivers,
            ILogger<ContentDisplayCoordinator> logger)
        {
            _partDrivers = partDrivers;
            _logger = logger;
            _contentDefinitionManager = contentDefinitionManager;
            _displayDrivers = displayDrivers;
            Logger = logger;

            _partInfos = _partDrivers.Select(cpp => cpp.GetPartInfo()).ToDictionary(x => x.PartName, x => x);
        }

        private ILogger Logger { get; set; }

        public Task BuildDisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            return Process(model, (part, fieldName) =>
                _displayDrivers.InvokeAsync(async contentDisplay => {
                    var result = await contentDisplay.BuildDisplayAsync(fieldName, part, context);
                    if (result != null)
                        result.Apply(context);
                }, Logger)
            );
        }

        public Task BuildEditorAsync(ContentItem model, BuildEditorContext context)
        {
            return Process(model, (part, fieldName) =>
                _displayDrivers.InvokeAsync(async contentDisplay => {
                    var result = await contentDisplay.BuildEditorAsync(fieldName, part, context);
                    if (result != null)
                        result.Apply(context);
                }, Logger)
            );
        }

        public Task UpdateEditorAsync(ContentItem model, UpdateEditorContext context)
        {
            return Process(model, (part, fieldName) =>
                _displayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(fieldName, part, context);
                if (result != null)
                    result.Apply(context);
                }, Logger)
            );
        }

        public Task Process(ContentItem contentItem, Func<ContentPart, string, Task> action)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return Task.CompletedTask;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                // Abort if there are not fields in this part
                if (!typePartDefinition.PartDefinition.Fields.Any())
                {
                    continue;
                }

                var partName = typePartDefinition.PartDefinition.Name;
                ContentPartInfo partInfo;
                _partInfos.TryGetValue(partName, out partInfo);

                ContentPart part = partInfo != null
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
                    return action(part, fieldName);
                }
            }

            return Task.CompletedTask;
        }
    }
}
