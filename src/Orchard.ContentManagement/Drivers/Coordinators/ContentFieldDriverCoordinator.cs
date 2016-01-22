using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.MetaData;
using System;

namespace Orchard.ContentManagement.Drivers.Coordinators {
    /// <summary>
    /// This component coordinates how fields are taking part in the rendering when some content needs to be rendered.
    /// It will dispatch BuildDisplay/BuildEditor to all <see cref="IContentFieldDriver"/> implementations.
    /// </summary>
    public class ContentFieldDriverCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentFieldDriver> _drivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentFieldDriverCoordinator(
            IEnumerable<IContentFieldDriver> drivers,
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentFieldDriverCoordinator> logger) {
            _contentDefinitionManager = contentDefinitionManager;
            _drivers = drivers;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public override void Initializing(InitializingContentContext context) {
            // Adds all the parts to a content item based on the content type definition.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var fieldInfos = _drivers.Select(cpp => cpp.GetFieldInfo()).ToArray();

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldTypeName = partFieldDefinition.FieldDefinition.Name;
                    var fieldInfo = fieldInfos.FirstOrDefault(fi => fi.FieldName == fieldTypeName);
                    var field = fieldInfo != null
                        ? fieldInfo.Factory(partFieldDefinition)
                        : new ContentField();

                    var fieldName = partFieldDefinition.Name;
                    context.ContentItem.Get(partName).Weld(fieldName, field);
                }
            }
        }

        protected void Apply(ContentItem contentItem, Action<ContentElement, ContentElement> action)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null)
                return;

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.Name;

                    var part = contentItem.Get(partName);
                    var field = part.Get(fieldName);

                    action(part, field);
                }
            }
        }

        public override void GetContentItemMetadata(ContentItemMetadataContext context)
        {
            Apply(context.ContentItem, (part, field) => _drivers.Invoke(driver => driver.GetContentItemMetadata(part, field, context), Logger));
        }
    }
}