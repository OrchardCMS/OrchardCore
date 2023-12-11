using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using YesSql.Services;

namespace OrchardCore.ContentsTransfer.Services;

public class ContentImportManager : IContentImportManager
{
    private readonly IContentImportHandlerResolver _contentImportHandlerResolver;
    private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
    private readonly ITypeActivatorFactory<ContentField> _contentFieldFactory;
    private readonly IEnumerable<IContentImportHandler> _contentImportHandlers;
    private readonly IContentManager _contentManager;
    private readonly ILogger _logger;

    public ContentImportManager(
        IContentImportHandlerResolver contentImportHandlerResolver,
        ITypeActivatorFactory<ContentPart> contentPartFactory,
        ITypeActivatorFactory<ContentField> contentFieldFactory,
        IEnumerable<IContentImportHandler> contentImportHandlers,
        IContentManager contentManager,
        ILogger<ContentImportManager> logger
        )
    {
        _contentImportHandlerResolver = contentImportHandlerResolver;
        _contentPartFactory = contentPartFactory;
        _contentFieldFactory = contentFieldFactory;
        _contentImportHandlers = contentImportHandlers;
        _contentManager = contentManager;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ImportColumn>> GetColumnsAsync(ImportContentContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var contentItem = await _contentManager.NewAsync(context.ContentTypeDefinition.Name);

        var columns = new List<ImportColumn>();

        columns.AddRange(_contentImportHandlers.Invoke(handler => handler.GetColumns(context), _logger).SelectMany(x => x));

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
                // ContentPart = part,
                ContentTypePartDefinition = typePartDefinition,
            };

            var partColumns = partHandlers.Invoke((handler) => handler.GetColumns(partContext), _logger);

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
                    // ContentField = field,
                };

                var fieldHandlers = _contentImportHandlerResolver.GetFieldHandlers(fieldName);

                var fieldColumns = fieldHandlers.Invoke((handler) => handler.GetColumns(fieldContext), _logger);

                columns.AddRange(fieldColumns.SelectMany(x => x));
            }
        }

        return columns;
    }

    public async Task ImportAsync(ContentImportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        await _contentImportHandlers.InvokeAsync(handler => handler.ImportAsync(context), _logger);

        foreach (var typePartDefinition in context.ContentTypeDefinition.Parts)
        {
            var partName = typePartDefinition.PartDefinition.Name;
            var partActivator = _contentPartFactory.GetTypeActivator(partName);

            var partContext = new ContentPartImportMapContext()
            {
                ContentItem = context.ContentItem,
                ContentTypePartDefinition = typePartDefinition,
                Columns = context.Columns,
                Row = context.Row,
            };

            var partHandlers = _contentImportHandlerResolver.GetPartHandlers(partName);
            await partHandlers.InvokeAsync((handler) => handler.ImportAsync(partContext), _logger);

            if (typePartDefinition.PartDefinition?.Fields == null)
            {
                continue;
            }

            var part = context.ContentItem.Get(partActivator.Type, typePartDefinition.Name) as ContentPart;
            if (part == null)
            {
                part = partActivator.CreateInstance();
                part.Weld(typePartDefinition.Name, part);
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
                    ContentPart = part,
                    Row = context.Row,
                    Columns = context.Columns,
                    ContentItem = context.ContentItem,
                };

                var fieldHandlers = _contentImportHandlerResolver.GetFieldHandlers(fieldName);

                await fieldHandlers.InvokeAsync((handler) => handler.ImportAsync(fieldContext), _logger);
            }
        }
    }

    public async Task ExportAsync(ContentExportContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _contentImportHandlers.InvokeAsync(handler => handler.ExportAsync(context), _logger);

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

            await partHandlers.InvokeAsync((handler) => handler.ExportAsync(partContext), _logger);

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

                await fieldHandlers.InvokeAsync((handler) => handler.ExportAsync(fieldContext), _logger);
            }
        }
    }
}
