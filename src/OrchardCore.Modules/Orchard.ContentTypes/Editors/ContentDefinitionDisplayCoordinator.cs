using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentTypes.Editors
{
    public class ContentDefinitionDisplayCoordinator : IContentDefinitionDisplayHandler
    {
        private readonly IEnumerable<IContentTypeDefinitionDisplayDriver> _typeDisplayDrivers;
        private readonly IEnumerable<IContentTypePartDefinitionDisplayDriver> _typePartDisplayDrivers;
        private readonly IEnumerable<IContentPartDefinitionDisplayDriver> _partDisplayDrivers;
        private readonly IEnumerable<IContentPartFieldDefinitionDisplayDriver> _partFieldDisplayDrivers;

        public ContentDefinitionDisplayCoordinator(
            IEnumerable<IContentTypeDefinitionDisplayDriver> typeDisplayDrivers,
            IEnumerable<IContentTypePartDefinitionDisplayDriver> typePartDisplayDrivers,
            IEnumerable<IContentPartDefinitionDisplayDriver> partDisplayDrivers,
            IEnumerable<IContentPartFieldDefinitionDisplayDriver> partFieldDisplayDrivers,
            ILogger<IContentDefinitionDisplayHandler> logger)
        {
            _partFieldDisplayDrivers = partFieldDisplayDrivers;
            _partDisplayDrivers = partDisplayDrivers;
            _typePartDisplayDrivers = typePartDisplayDrivers;
            _typeDisplayDrivers = typeDisplayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public Task BuildTypeEditorAsync(ContentTypeDefinition model, BuildEditorContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateTypeEditorAsync(ContentTypeDefinition model, UpdateTypeEditorContext context)
        {
            return _typeDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildTypePartEditorAsync(ContentTypePartDefinition model, BuildEditorContext context)
        {
            return _typePartDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateTypePartEditorAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
        {
            return _typePartDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartEditorAsync(ContentPartDefinition model, BuildEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdatePartEditorAsync(ContentPartDefinition model, UpdatePartEditorContext context)
        {
            return _partDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildPartFieldEditorAsync(ContentPartFieldDefinition model, BuildEditorContext context)
        {
            return _partFieldDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdatePartFieldEditorAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
        {
            return _partFieldDisplayDrivers.InvokeAsync(async contentDisplay =>
            {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}