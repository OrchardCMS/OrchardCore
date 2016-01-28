using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement.Drivers.Coordinators
{
    /// <summary>
    /// This component coordinates how parts are affecting content items.
    /// </summary>
    public class ContentPartCoordinator : ContentHandlerBase {
        private readonly IEnumerable<IContentPartDriver> _partDrivers;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentFieldDriver> _fieldDrivers;

        public ContentPartCoordinator(
            IEnumerable<IContentPartDriver> partDrivers,
            IEnumerable<IContentFieldDriver> fieldDrivers,
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentPartCoordinator> logger) {
            _fieldDrivers = fieldDrivers;
            _partDrivers = partDrivers;
            _contentDefinitionManager = contentDefinitionManager;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public override void Activating(ActivatingContentContext context)
        {
            // This method is called on New()
            // Adds all the parts to a content item based on the content type definition.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var partInfos = _partDrivers.Select(cpp => cpp.GetPartInfo()).ToArray();

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var partInfo = partInfos.FirstOrDefault(pi => pi.PartName == partName);
                var part = partInfo != null
                    ? partInfo.Factory(typePartDefinition)
                    : new ContentPart();

                context.Builder.Weld(partName, part);
            }
        }

        public override void Initializing(InitializingContentContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var fieldInfos = _fieldDrivers.Select(cpp => cpp.GetFieldInfo()).ToArray();

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldTypeName = partFieldDefinition.FieldDefinition.Name;
                    var fieldInfo = fieldInfos.FirstOrDefault(x => x.FieldTypeName == fieldTypeName);
                    if (fieldInfo != null)
                    {
                        var field = fieldInfo.Factory(partFieldDefinition);
                        var fieldName = partFieldDefinition.Name;
                        context.ContentItem.Get(partName).Weld(fieldName, field);
                    }
                }
            }
        }

        public override void Loading(LoadContentContext context)
        {
            // This method is called on Get()
            // Adds all the missing parts to a content item based on the content type definition.

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null)
                return;

            var partInfos = _partDrivers.Select(cpp => cpp.GetPartInfo()).ToArray();
            var fieldInfos = _fieldDrivers.Select(cpp => cpp.GetFieldInfo()).ToArray();

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                if (!context.ContentItem.Has(partName))
                {
                    var partInfo = partInfos.FirstOrDefault(pi => pi.PartName == partName);
                    var part = partInfo != null
                        ? partInfo.Factory(typePartDefinition)
                        : new ContentPart();

                    context.ContentItem.Weld(partName, part);
                }

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var part = context.ContentItem.Get(partName);
                    var fieldName = partFieldDefinition.Name;

                    if (!part.Has(fieldName))
                    {

                        var fieldTypeName = partFieldDefinition.FieldDefinition.Name;
                        var fieldInfo = fieldInfos.FirstOrDefault(fi => fi.FieldTypeName == fieldTypeName);
                        var field = fieldInfo != null
                            ? fieldInfo.Factory(partFieldDefinition)
                            : new ContentField();

                        context.ContentItem.Get(partName).Weld(fieldName, field);
                    }
                }
            }
        }
    }
}