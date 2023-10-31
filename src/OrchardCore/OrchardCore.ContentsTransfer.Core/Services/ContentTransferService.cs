using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentsTransfer.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.ContentsTransfer.Services;

public class ContentTransferService
{
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ContentTransferService(
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<ContentTransferService> stringLocalizer,
        ILogger<ContentTransferService> logger)
    {
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        S = stringLocalizer;
        _logger = logger;
    }
    public async Task<DataTableImportResult> ImportAsync(string contentTypeId, DataTable dataTable)
    {
        if (dataTable == null)
        {
            throw new ArgumentNullException(nameof(dataTable));
        }

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

        if(contentTypeDefinition == null)
        {
            return DataTableImportResult.Fail(S["Unable to find a content type definition for '{0}'.", contentTypeId]);
        }

        var settings = contentTypeDefinition.GetSettings<ImportableContentTypeSettings>();

        if (settings.AllowBulkImport)
        {
            return DataTableImportResult.Fail(S["The content type '{0}' does not allow bulk import.", contentTypeId]);
        }

        var result = new DataTableImportResult();

        foreach (DataRow row in dataTable.Rows)
        {
            if (row == null || row.ItemArray.All(x => x == null || x is DBNull || string.IsNullOrWhiteSpace(x?.ToString())))
            {
                // Ignore empty rows.
                continue;
            }

            var mapContext = new ContentImportMapContext()
            {
                ContentItem = await _contentManager.NewAsync(contentTypeId),
                ContentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId),
                Columns = dataTable.Columns,
                Row = row,
            };

            // Important to map the data first since the map could identify existing content item.
            // MapAsync could change the content item id.
            await _contentImportHandlerCoordinator.InvokeAsync(async mapper => await mapper.MapAsync(mapContext), _logger);

            contentItems.Add(mapContext.ContentItem);
        }

        return result;
    }
}

public class DataTableImportResult
{
    public bool Success { get; set; }

    public string Error { get; set; }

    public static DataTableImportResult Fail(string message)
    {
        return new DataTableImportResult()
        {
            Success = false,
            Error = message,
        };
    }
}
