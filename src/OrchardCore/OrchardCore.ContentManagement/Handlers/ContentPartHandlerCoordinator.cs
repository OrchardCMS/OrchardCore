using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Handlers
{
    /// <summary>
    /// This component coordinates how parts are affecting content items.
    /// </summary>
    public class ContentPartHandlerCoordinator : ContentHandlerBase
    {
        private readonly IContentPartHandlerResolver _contentPartHandlerResolver;
        private readonly IContentFieldHandlerResolver _contentFieldHandlerResolver;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentField> _contentFieldFactory;
        private readonly ILogger _logger;

        public ContentPartHandlerCoordinator(
            IContentPartHandlerResolver contentPartHandlerResolver,
            IContentFieldHandlerResolver contentFieldHandlerResolver,
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            ITypeActivatorFactory<ContentField> contentFieldFactory,
            IContentDefinitionManager contentDefinitionManager,
            ILogger<ContentPartHandlerCoordinator> logger)
        {
            _contentPartHandlerResolver = contentPartHandlerResolver;
            _contentFieldHandlerResolver = contentFieldHandlerResolver;
            _contentPartFactory = contentPartFactory;
            _contentFieldFactory = contentFieldFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _logger = logger;
        }

        public override Task ActivatingAsync(ActivatingContentContext context)
        {
            // This method is called on New()
            // Adds all the parts to a content item based on the content type definition.
            // When a part/field does not exists, we create the part from it's known type or from a generic one
            return InvokeHandlers(context,
                (handler, context, part) => handler.ActivatingAsync(context, part),
                (handler, context, field) => handler.ActivatingAsync(context, field),
                createPartIfNotExists: true,
                createFieldIfNotExists: true);
        }

        public override Task ActivatedAsync(ActivatedContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.ActivatedAsync(context, part),
                (handler, context, field) => handler.ActivatedAsync(context, field));
        }

        public override Task CreatingAsync(CreateContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.CreatingAsync(context, part),
                (handler, context, field) => handler.CreatingAsync(context, field));
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.CreatedAsync(context, part),
                (handler, context, field) => handler.CreatedAsync(context, field));
        }

        public override Task ImportingAsync(ImportContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.ImportingAsync(context, part),
                (handler, context, field) => handler.ImportingAsync(context, field));
        }

        public override Task ImportedAsync(ImportContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.ImportedAsync(context, part),
                (handler, context, field) => handler.ImportedAsync(context, field));
        }

        public override Task InitializingAsync(InitializingContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.InitializingAsync(context, part),
                (handler, context, field) => handler.InitializingAsync(context, field));
        }

        public override Task InitializedAsync(InitializingContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.InitializedAsync(context, part),
                (handler, context, field) => handler.InitializedAsync(context, field));
        }

        public override Task LoadingAsync(LoadContentContext context)
        {
            // This method is called on Get()
            // Adds all the missing parts to a content item based on the content type definition.
            // A part is missing if the content type is changed and an old content item is loaded,
            // like edited.

            return InvokeHandlers(context,
                (handler, context, part) => handler.LoadingAsync(context, part),
                (handler, context, field) => handler.LoadingAsync(context, field),
                createPartIfNotExists: true,
                createFieldIfNotExists: true);
        }

        public override Task LoadedAsync(LoadContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.LoadedAsync(context, part),
                (handler, context, field) => handler.LoadedAsync(context, field));
        }

        public override Task ValidatingAsync(ValidateContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.ValidatingAsync(context, part),
                (handler, context, field) => handler.ValidatingAsync(context, field));
        }

        public override Task ValidatedAsync(ValidateContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.ValidatedAsync(context, part),
                (handler, context, field) => handler.ValidatedAsync(context, field));
        }

        public override Task DraftSavingAsync(SaveDraftContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.DraftSavingAsync(context, part),
                (handler, context, field) => handler.DraftSavingAsync(context, field));
        }

        public override Task DraftSavedAsync(SaveDraftContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.DraftSavedAsync(context, part),
                (handler, context, field) => handler.DraftSavedAsync(context, field));
        }

        public override Task PublishingAsync(PublishContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.PublishingAsync(context, part),
                (handler, context, field) => handler.PublishingAsync(context, field));
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.PublishedAsync(context, part),
                (handler, context, field) => handler.PublishedAsync(context, field));
        }

        public override Task RemovingAsync(RemoveContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.RemovingAsync(context, part),
                (handler, context, field) => handler.RemovingAsync(context, field));
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.RemovedAsync(context, part),
                (handler, context, field) => handler.RemovedAsync(context, field));
        }

        public override Task UnpublishingAsync(PublishContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.UnpublishingAsync(context, part),
                (handler, context, field) => handler.UnpublishingAsync(context, field));
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.UnpublishedAsync(context, part),
                (handler, context, field) => handler.UnpublishedAsync(context, field));
        }

        public override Task UpdatingAsync(UpdateContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.UpdatingAsync(context, part),
                (handler, context, field) => handler.UpdatingAsync(context, field));
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.UpdatedAsync(context, part),
                (handler, context, field) => handler.UpdatedAsync(context, field));
        }

        public override Task VersioningAsync(VersionContentContext context)
        {
            return InvokeHandlers(context,
                 (handler, context, existingPart, buildingPart) => handler.VersioningAsync(context, existingPart, buildingPart),
                 (handler, context, existingField, buildingField) => handler.VersioningAsync(context, existingField, buildingField));
        }

        public override Task VersionedAsync(VersionContentContext context)
        {
            return InvokeHandlers(context,
                 (handler, context, existingPart, buildingPart) => handler.VersionedAsync(context, existingPart, buildingPart),
                 (handler, context, existingField, buildingField) => handler.VersionedAsync(context, existingField, buildingField));
        }

        public override async Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
            {
                return;
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var activator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(activator.Type, typePartDefinition.Name) as ContentPart;

                if (part == null)
                {
                    continue;
                }

                var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                await partHandlers.InvokeAsync((handler, context, part) => handler.GetContentItemAspectAsync(context, part), context, part, _logger);

                if (typePartDefinition.PartDefinition?.Fields == null)
                {
                    continue;
                }

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.FieldDefinition.Name;

                    if (String.IsNullOrEmpty(partName))
                    {
                        _logger.LogError("The content part '{contentPert}' contains field '{contentField}' that does not exists.", partName, fieldName);
                        continue;
                    }

                    var fieldActivator = _contentFieldFactory.GetTypeActivator(fieldName);
                    var field = context.ContentItem.Get(fieldActivator.Type, partFieldDefinition.Name) as ContentField;

                    if (field == null)
                    {
                        continue;
                    }

                    var fieldHandlers = _contentFieldHandlerResolver.GetHandlers(fieldName);
                    await fieldHandlers.InvokeAsync((handler, context, field) => handler.GetContentItemAspectAsync(context, field), context, field, _logger);
                }
            }
        }

        public override Task ClonedAsync(CloneContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.ClonedAsync(context, part),
                (handler, context, field) => handler.ClonedAsync(context, field));
        }
        public override Task CloningAsync(CloneContentContext context)
        {
            return InvokeHandlers(context,
                (handler, context, part) => handler.CloningAsync(context, part),
                (handler, context, field) => handler.CloningAsync(context, field));
        }

        private async Task InvokeHandlers<TContext>(
            TContext context,
            Func<IContentPartHandler, TContext, ContentPart, Task> partHandlerAction,
            Func<IContentFieldHandler, TContext, ContentField, Task> fieldHandlerAction,
            bool createPartIfNotExists = false,
            bool createFieldIfNotExists = false)
            where TContext : ContentContextBase
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
            {
                return;
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(partActivator.Type, typePartDefinition.Name) as ContentPart;

                if (part == null && createPartIfNotExists)
                {
                    part = partActivator.CreateInstance();
                    context.ContentItem.Weld(typePartDefinition.Name, part);
                }

                if (part == null)
                {
                    continue;
                }

                var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                await partHandlers.InvokeAsync(partHandlerAction, context, part, _logger);

                if (typePartDefinition.PartDefinition?.Fields == null)
                {
                    continue;
                }

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.FieldDefinition.Name;
                    var fieldActivator = _contentFieldFactory.GetTypeActivator(fieldName);

                    // Attempt to get the field from the part.
                    var field = part.Get(fieldActivator.Type, partFieldDefinition.Name) as ContentField;

                    if (field == null && createFieldIfNotExists)
                    {
                        field = fieldActivator.CreateInstance();

                        part.Weld(partFieldDefinition.Name, field);
                    }

                    if (field == null)
                    {
                        continue;
                    }

                    var fieldHandlers = _contentFieldHandlerResolver.GetHandlers(fieldName);
                    await fieldHandlers.InvokeAsync(fieldHandlerAction, context, field, _logger);
                }
            }
        }

        private async Task InvokeHandlers<TContext>(
            TContext context,
            Func<IContentPartHandler, TContext, ContentPart, ContentPart, Task> partHandlerAction,
            Func<IContentFieldHandler, TContext, ContentField, ContentField, Task> fieldHandlerAction)
            where TContext : VersionContentContext
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
            {
                return;
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partName);

                var buildingPart = (ContentPart)context.BuildingContentItem.Get(partActivator.Type, partName);
                var existingPart = (ContentPart)context.ContentItem.Get(partActivator.Type, typePartDefinition.Name);

                if (buildingPart == null || existingPart == null)
                {
                    continue;
                }

                var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);
                await partHandlers.InvokeAsync(partHandlerAction, context, existingPart, buildingPart, _logger);

                if (typePartDefinition.PartDefinition?.Fields == null)
                {
                    continue;
                }

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.FieldDefinition.Name;
                    var fieldActivator = _contentFieldFactory.GetTypeActivator(fieldName);

                    var buildingField = (ContentField)buildingPart.Get(fieldActivator.Type, fieldName);
                    var existingField = (ContentField)existingPart.Get(fieldActivator.Type, partFieldDefinition.Name);

                    if (buildingField == null || existingField == null)
                    {
                        continue;
                    }

                    var fieldHandlers = _contentFieldHandlerResolver.GetHandlers(fieldName);
                    await fieldHandlers.InvokeAsync(fieldHandlerAction, context, existingField, buildingField, _logger);
                }
            }
        }

        private async Task InvokeHandlers<TContext>(
            TContext context,
            Func<IContentPartHandler, ValidateContentContext, ContentPart, Task> partHandlerAction,
            Func<IContentFieldHandler, ValidateFieldContentContext, ContentField, Task> fieldHandlerAction)
            where TContext : ValidateContentContext
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
            if (contentTypeDefinition == null)
            {
                return;
            }

            foreach (var typePartDefinition in contentTypeDefinition.Parts)
            {
                var partName = typePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partName);
                var part = context.ContentItem.Get(partActivator.Type, typePartDefinition.Name) as ContentPart;

                if (part == null)
                {
                    continue;
                }

                var partHandlers = _contentPartHandlerResolver.GetHandlers(partName);

                var partValidationContext = new ValidateContentContext(context.ContentItem)
                {
                    ContentTypePartDefinition = typePartDefinition,
                };

                await partHandlers.InvokeAsync(partHandlerAction, partValidationContext, part, _logger);

                if (typePartDefinition.PartDefinition?.Fields == null)
                {
                    continue;
                }

                foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                {
                    var fieldName = partFieldDefinition.FieldDefinition.Name;

                    var fieldActivator = _contentFieldFactory.GetTypeActivator(fieldName);

                    var field = part.Get(fieldActivator.Type, partFieldDefinition.Name) as ContentField;

                    if (field == null)
                    {
                        continue;
                    }

                    var validateFieldContentContext = new ValidateFieldContentContext(
                        context.ContentItem,
                        partFieldDefinition,
                        typePartDefinition.Name ?? partName);

                    var fieldHandlers = _contentFieldHandlerResolver.GetHandlers(fieldName);
                    await fieldHandlers.InvokeAsync(fieldHandlerAction, validateFieldContentContext, field, _logger);

                    // Add any field errors to the context errors.
                    foreach (var error in validateFieldContentContext.ContentValidateResult.Errors)
                    {
                        context.ContentValidateResult.Fail(error);
                    }
                }

                // Add any part errors to the context errors.
                foreach (var error in partValidationContext.ContentValidateResult.Errors)
                {
                    context.ContentValidateResult.Fail(error);
                }
            }
        }
    }
}
