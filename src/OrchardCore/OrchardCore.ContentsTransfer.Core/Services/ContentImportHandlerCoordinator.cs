using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Modules;

namespace OrchardCore.ContentsTransfer.Services;

public class ContentImportHandlerCoordinator : IContentImportHandlerCoordinator
{
    private readonly IContentImportHandlerResolver _contentImportHandlerResolver;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
    private readonly ITypeActivatorFactory<ContentField> _contentFieldFactory;
    private readonly IEnumerable<IContentImportHandler> _contentImportHandlers;
    private readonly IContentManager _contentManager;
    private readonly ILogger _logger;

    public ContentImportHandlerCoordinator(
        IContentImportHandlerResolver contentImportHandlerResolver,
        IContentDefinitionManager contentDefinitionManager,
        ITypeActivatorFactory<ContentPart> contentPartFactory,
        ITypeActivatorFactory<ContentField> contentFieldFactory,
        IEnumerable<IContentImportHandler> contentImportHandlers,
        IContentManager contentManager,
        ILogger<ContentImportHandlerCoordinator> logger
        )
    {
        _contentImportHandlerResolver = contentImportHandlerResolver;
        _contentDefinitionManager = contentDefinitionManager;
        _contentPartFactory = contentPartFactory;
        _contentFieldFactory = contentFieldFactory;
        _contentImportHandlers = contentImportHandlers;
        _contentManager = contentManager;
        _logger = logger;
    }

    public IReadOnlyCollection<ImportColumn> Columns(ImportContentContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var contentItem = _contentManager.NewAsync(context.ContentTypeDefinition.Name).GetAwaiter().GetResult();

        var columns = new List<ImportColumn>();

        columns.AddRange(_contentImportHandlers.Invoke(handler => handler.Columns(context), _logger).SelectMany(x => x));

        foreach (var typePartDefinition in context.ContentTypeDefinition.Parts)
        {
            var partName = typePartDefinition.PartDefinition.Name;
            var partActivator = _contentPartFactory.GetTypeActivator(partName);
            var part = contentItem.Get(partActivator.Type, typePartDefinition.Name) as ContentPart;

            if (part == null)
            {
                part = partActivator.CreateInstance();
                part.Weld(typePartDefinition.Name, part);
            }

            var partHandlers = _contentImportHandlerResolver.GetPartHandlers(partName);

            var partContext = new ContentPartImportMapContext()
            {
                ContentItem = contentItem,
                ContentPart = part,
                ContentTypePartDefinition = typePartDefinition,
            };

            var partColumns = partHandlers.Invoke((handler) => handler.Columns(partContext), _logger);

            columns.AddRange(partColumns.SelectMany(x => x));

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
                    field = fieldActivator.CreateInstance();
                    part.Weld(partFieldDefinition.Name, field);
                }

                var fieldContext = new ImportContentFieldContext()
                {
                    ContentPartFieldDefinition = partFieldDefinition,
                    ContentPart = part,
                    PartName = typePartDefinition.Name ?? partName,
                    ContentField = field,
                };

                var fieldHandlers = _contentImportHandlerResolver.GetFieldHandlers(fieldName);

                var fieldColumns = fieldHandlers.Invoke((handler) => handler.Columns(fieldContext), _logger);

                columns.AddRange(fieldColumns.SelectMany(x => x));
            }
        }

        return columns;
    }

    public async Task MapAsync(ContentImportMapContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _contentImportHandlers.InvokeAsync(handler => handler.MapAsync(context), _logger);

        foreach (var typePartDefinition in context.ContentTypeDefinition.Parts)
        {
            var partName = typePartDefinition.PartDefinition.Name;
            var partActivator = _contentPartFactory.GetTypeActivator(partName);
            var part = context.ContentItem.Get(partActivator.Type, typePartDefinition.Name) as ContentPart;
            if (part == null)
            {
                part = partActivator.CreateInstance();
                part.Weld(typePartDefinition.Name, part);
            }

            var partHandlers = _contentImportHandlerResolver.GetPartHandlers(partName);

            var partContext = new ContentPartImportMapContext()
            {
                ContentItem = context.ContentItem,
                ContentPart = part,
                ContentTypePartDefinition = typePartDefinition,
                Columns = context.Columns,
                Row = context.Row,
            };

            await partHandlers.InvokeAsync((handler) => handler.MapAsync(partContext), _logger);

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
                    field = fieldActivator.CreateInstance();
                    part.Weld(partFieldDefinition.Name, field);
                }

                var fieldContext = new ContentFieldImportMapContext()
                {
                    ContentPartFieldDefinition = partFieldDefinition,
                    PartName = typePartDefinition.Name ?? partName,
                    ContentField = field,
                    ContentPart = part,
                    Row = context.Row,
                    Columns = context.Columns,
                    ContentItem = context.ContentItem,
                };

                var fieldHandlers = _contentImportHandlerResolver.GetFieldHandlers(fieldName);

                await fieldHandlers.InvokeAsync((handler) => handler.MapAsync(fieldContext), _logger);
            }
        }
    }

    public async Task MapOutAsync(ContentExportMapContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _contentImportHandlers.InvokeAsync(handler => handler.MapOutAsync(context), _logger);

        foreach (var typePartDefinition in context.ContentTypeDefinition.Parts)
        {
            var partName = typePartDefinition.PartDefinition.Name;
            var partActivator = _contentPartFactory.GetTypeActivator(partName);
            var part = context.ContentItem.Get(partActivator.Type, typePartDefinition.Name) as ContentPart;
            if (part == null)
            {
                part = partActivator.CreateInstance();
                part.Weld(typePartDefinition.Name, part);
            }

            var partHandlers = _contentImportHandlerResolver.GetPartHandlers(partName);

            var partContext = new ContentPartExportMapContext()
            {
                ContentItem = context.ContentItem,
                ContentPart = part,
                ContentTypePartDefinition = typePartDefinition,
                Row = context.Row,
            };

            await partHandlers.InvokeAsync((handler) => handler.MapOutAsync(partContext), _logger);

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
                    field = fieldActivator.CreateInstance();
                    part.Weld(partFieldDefinition.Name, field);
                }

                var fieldContext = new ContentFieldExportMapContext()
                {
                    ContentPartFieldDefinition = partFieldDefinition,
                    PartName = typePartDefinition.Name ?? partName,
                    ContentField = field,
                    ContentPart = part,
                    Row = context.Row,
                    ContentItem = context.ContentItem,
                };

                var fieldHandlers = _contentImportHandlerResolver.GetFieldHandlers(fieldName);

                await fieldHandlers.InvokeAsync((handler) => handler.MapOutAsync(fieldContext), _logger);
            }
        }
    }

    public async Task ValidateColumnsAsync(ValidateImportContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        foreach (var typePartDefinition in context.ContentTypeDefinition.Parts)
        {
            var partName = typePartDefinition.PartDefinition.Name;
            var partActivator = _contentPartFactory.GetTypeActivator(partName);
            var part = context.ContentItem.Get(partActivator.Type, typePartDefinition.Name) as ContentPart;

            if (part == null)
            {
                part = partActivator.CreateInstance();
                part.Weld(typePartDefinition.Name, part);
            }

            var partHandlers = _contentImportHandlerResolver.GetPartHandlers(partName);

            var partContext = new ValidatePartImportContext()
            {
                ContentItem = context.ContentItem,
                ContentPart = part,
                ContentTypePartDefinition = typePartDefinition,
                Columns = context.Columns,
            };

            await partHandlers.InvokeAsync((handler) => handler.ValidateColumnsAsync(partContext), _logger);

            foreach (var error in partContext.ContentValidateResult.Errors)
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
                    field = fieldActivator.CreateInstance();
                    part.Weld(partFieldDefinition.Name, field);
                }

                var fieldContext = new ValidateFieldImportContext()
                {
                    ContentPartFieldDefinition = partFieldDefinition,
                    PartName = typePartDefinition.Name ?? partName,
                    ContentField = field,
                    ContentPart = part,
                    Columns = context.Columns,
                    ContentItem = context.ContentItem,
                };

                var fieldHandlers = _contentImportHandlerResolver.GetFieldHandlers(fieldName);

                await fieldHandlers.InvokeAsync((handler) => handler.ValidateColumnsAsync(fieldContext), _logger);

                foreach (var error in fieldContext.ContentValidateResult.Errors)
                {
                    context.ContentValidateResult.Fail(error);
                }
            }
        }
    }
}
