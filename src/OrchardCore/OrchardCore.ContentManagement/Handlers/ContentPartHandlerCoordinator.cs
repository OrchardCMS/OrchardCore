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
            var fieldContext = new ActivatingContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.ActivatingAsync(context, part),
                (handler, context, field) => handler.ActivatingAsync(fieldContext, field),
                createPartIfNotExists: true,
                createFieldIfNotExists: true);
        }

        public override Task ActivatedAsync(ActivatedContentContext context)
        {
            var fieldContext = new ActivatedContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.ActivatedAsync(context, part),
                (handler, context, field) => handler.ActivatedAsync(fieldContext, field));
        }

        public override Task CreatingAsync(CreateContentContext context)
        {
            var fieldContext = new CreateContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.CreatingAsync(context, part),
                (handler, context, field) => handler.CreatingAsync(fieldContext, field));
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            var fieldContext = new CreateContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.CreatedAsync(context, part),
                (handler, context, field) => handler.CreatedAsync(fieldContext, field));
        }

        public override Task ImportingAsync(ImportContentContext context)
        {
            var fieldContext = new ImportContentFieldContext(context.ContentItem, context.OriginalContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.ImportingAsync(context, part),
                (handler, context, field) => handler.ImportingAsync(fieldContext, field));
        }

        public override Task ImportedAsync(ImportContentContext context)
        {
            var fieldContext = new ImportContentFieldContext(context.ContentItem, context.OriginalContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.ImportedAsync(context, part),
                (handler, context, field) => handler.ImportedAsync(fieldContext, field));
        }

        public override Task InitializingAsync(InitializingContentContext context)
        {
            var fieldContext = new InitializingContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.InitializingAsync(context, part),
                (handler, context, field) => handler.InitializingAsync(fieldContext, field));
        }

        public override Task InitializedAsync(InitializingContentContext context)
        {
            var fieldContext = new InitializingContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.InitializedAsync(context, part),
                (handler, context, field) => handler.InitializedAsync(fieldContext, field));
        }

        public override Task LoadingAsync(LoadContentContext context)
        {
            // This method is called on Get()
            // Adds all the missing parts to a content item based on the content type definition.
            // A part is missing if the content type is changed and an old content item is loaded,
            // like edited.
            var fieldContext = new LoadContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.LoadingAsync(context, part),
                (handler, context, field) => handler.LoadingAsync(fieldContext, field),
                createPartIfNotExists: true,
                createFieldIfNotExists: true);
        }

        public override Task LoadedAsync(LoadContentContext context)
        {
            var fieldContext = new LoadContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.LoadedAsync(context, part),
                (handler, context, field) => handler.LoadedAsync(fieldContext, field));
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
                (handler, context, field) => handler.ValidatedAsync(new ValidateContentFieldContext(context.ContentItem), field));
        }

        public override Task DraftSavingAsync(SaveDraftContentContext context)
        {
            var fieldContext = new SaveDraftContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.DraftSavingAsync(context, part),
                (handler, context, field) => handler.DraftSavingAsync(fieldContext, field));
        }

        public override Task DraftSavedAsync(SaveDraftContentContext context)
        {
            var fieldContext = new SaveDraftContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.DraftSavedAsync(context, part),
                (handler, context, field) => handler.DraftSavedAsync(fieldContext, field));
        }

        public override Task PublishingAsync(PublishContentContext context)
        {
            var fieldContext = new PublishContentFieldContext(context.ContentItem, context.PreviousItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.PublishingAsync(context, part),
                (handler, context, field) => handler.PublishingAsync(fieldContext, field));
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            var fieldContext = new PublishContentFieldContext(context.ContentItem, context.PreviousItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.PublishedAsync(context, part),
                (handler, context, field) => handler.PublishedAsync(fieldContext, field));
        }

        public override Task RemovingAsync(RemoveContentContext context)
        {
            var fieldContext = new RemoveContentFieldContext(context.ContentItem, context.NoActiveVersionLeft);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.RemovingAsync(context, part),
                (handler, context, field) => handler.RemovingAsync(fieldContext, field));
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            var fieldContext = new RemoveContentFieldContext(context.ContentItem, context.NoActiveVersionLeft);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.RemovedAsync(context, part),
                (handler, context, field) => handler.RemovedAsync(fieldContext, field));
        }

        public override Task UnpublishingAsync(PublishContentContext context)
        {
            var fieldContext = new PublishContentFieldContext(context.ContentItem, context.PreviousItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.UnpublishingAsync(context, part),
                (handler, context, field) => handler.UnpublishingAsync(fieldContext, field));
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            var fieldContext = new PublishContentFieldContext(context.ContentItem, context.PreviousItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.UnpublishedAsync(context, part),
                (handler, context, field) => handler.UnpublishedAsync(fieldContext, field));
        }

        public override Task UpdatingAsync(UpdateContentContext context)
        {
            var fieldContext = new UpdateContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.UpdatingAsync(context, part),
                (handler, context, field) => handler.UpdatingAsync(fieldContext, field));
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            var fieldContext = new UpdateContentFieldContext(context.ContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.UpdatedAsync(context, part),
                (handler, context, field) => handler.UpdatedAsync(fieldContext, field));
        }

        public override Task VersioningAsync(VersionContentContext context)
        {
            var fieldContext = new VersionContentFieldContext(context.ContentItem, context.BuildingContentItem);

            return InvokeHandlers(context, fieldContext,
                 (handler, context, existingPart, buildingPart) => handler.VersioningAsync(context, existingPart, buildingPart),
                 (handler, context, existingField, buildingField) => handler.VersioningAsync(fieldContext, existingField, buildingField));
        }

        public override Task VersionedAsync(VersionContentContext context)
        {
            var fieldContext = new VersionContentFieldContext(context.ContentItem, context.BuildingContentItem);

            return InvokeHandlers(context, fieldContext,
                 (handler, context, existingPart, buildingPart) => handler.VersionedAsync(context, existingPart, buildingPart),
                 (handler, context, existingField, buildingField) => handler.VersionedAsync(fieldContext, existingField, buildingField));
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
            var fieldContext = new CloneContentFieldContext(context.ContentItem, context.CloneContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.ClonedAsync(context, part),
                (handler, context, field) => handler.ClonedAsync(fieldContext, field));
        }
        public override Task CloningAsync(CloneContentContext context)
        {
            var fieldContext = new CloneContentFieldContext(context.ContentItem, context.CloneContentItem);

            return InvokeHandlers(context, fieldContext,
                (handler, context, part) => handler.CloningAsync(context, part),
                (handler, context, field) => handler.CloningAsync(fieldContext, field));
        }

        private async Task InvokeHandlers<TContext, TFieldContext>(
            TContext context,
            TFieldContext fieldContext,
            Func<IContentPartHandler, TContext, ContentPart, Task> partHandlerAction,
            Func<IContentFieldHandler, TFieldContext, ContentField, Task> fieldHandlerAction,
            bool createPartIfNotExists = false,
            bool createFieldIfNotExists = false)
            where TContext : ContentContextBase
            where TFieldContext : ContentFieldContextBase
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

                    fieldContext.ContentPartFieldDefinition = partFieldDefinition;
                    fieldContext.PartName = typePartDefinition.Name ?? partName;

                    var fieldHandlers = _contentFieldHandlerResolver.GetHandlers(fieldName);
                    await fieldHandlers.InvokeAsync(fieldHandlerAction, fieldContext, field, _logger);
                }
            }
        }

        private async Task InvokeHandlers<TContext, TFieldContext>(
            TContext context,
            TFieldContext fieldContext,
            Func<IContentPartHandler, TContext, ContentPart, ContentPart, Task> partHandlerAction,
            Func<IContentFieldHandler, TFieldContext, ContentField, ContentField, Task> fieldHandlerAction)
            where TContext : VersionContentContext
            where TFieldContext : VersionContentFieldContext

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

                    fieldContext.ContentPartFieldDefinition = partFieldDefinition;
                    fieldContext.PartName = typePartDefinition.Name ?? partName;

                    var fieldHandlers = _contentFieldHandlerResolver.GetHandlers(fieldName);
                    await fieldHandlers.InvokeAsync(fieldHandlerAction, fieldContext, existingField, buildingField, _logger);
                }
            }
        }

        private async Task InvokeHandlers<TContext>(
            TContext context,
            Func<IContentPartHandler, ValidateContentContext, ContentPart, Task> partHandlerAction,
            Func<IContentFieldHandler, ValidateContentFieldContext, ContentField, Task> fieldHandlerAction)
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

                var partValidationContext = new ValidateContentPartContext(context.ContentItem)
                {
                    ContentTypePartDefinition = typePartDefinition,
                };

                await partHandlers.InvokeAsync(partHandlerAction, partValidationContext, part, _logger);

                // Add any part errors to the context errors.
                foreach (var error in partValidationContext.ContentValidateResult.Errors)
                {
                    context.ContentValidateResult.Fail(error);
                }

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

                    var validateFieldContentContext = new ValidateContentFieldContext(context.ContentItem)
                    {
                        ContentPartFieldDefinition = partFieldDefinition,
                        PartName = typePartDefinition.Name ?? partName
                    };

                    var fieldHandlers = _contentFieldHandlerResolver.GetHandlers(fieldName);
                    await fieldHandlers.InvokeAsync(fieldHandlerAction, validateFieldContentContext, field, _logger);

                    // Add any field errors to the context errors.
                    foreach (var error in validateFieldContentContext.ContentValidateResult.Errors)
                    {
                        context.ContentValidateResult.Fail(error);
                    }
                }
            }
        }
    }
}
