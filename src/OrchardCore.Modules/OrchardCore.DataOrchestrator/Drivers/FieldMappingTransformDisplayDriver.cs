using System.Text.Json.Nodes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.DataOrchestrator.ViewModels;
using OrchardCore.FileStorage;
using OrchardCore.Media;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class FieldMappingTransformDisplayDriver : EtlActivityDisplayDriver<FieldMappingTransform, FieldMappingTransformViewModel>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEtlPipelineService _pipelineService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IMediaFileStore _mediaFileStore;

    public FieldMappingTransformDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IEtlPipelineService pipelineService,
        IContentDefinitionManager contentDefinitionManager,
        IMediaFileStore mediaFileStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _pipelineService = pipelineService;
        _contentDefinitionManager = contentDefinitionManager;
        _mediaFileStore = mediaFileStore;
    }

    protected override async ValueTask EditActivityAsync(FieldMappingTransform activity, FieldMappingTransformViewModel model)
    {
        model.AvailableSourceFields = (await GetAvailableSourceFieldsAsync())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
        model.MappingsJson = activity.MappingsJson;
    }

    protected override void UpdateActivity(FieldMappingTransformViewModel model, FieldMappingTransform activity)
    {
        activity.MappingsJson = model.MappingsJson;
    }

    private async Task<IList<string>> GetAvailableSourceFieldsAsync()
    {
        if (!long.TryParse(_httpContextAccessor.HttpContext?.Request.Query["pipelineId"], out var pipelineId))
        {
            return [];
        }

        var pipeline = await _pipelineService.GetByDocumentIdAsync(pipelineId);
        if (pipeline == null)
        {
            return [];
        }

        var fields = new List<string>();

        foreach (var activityRecord in pipeline.Activities)
        {
            switch (activityRecord.Name)
            {
                case nameof(ContentItemSource):
                    fields.AddRange(await GetContentItemFieldsAsync(activityRecord.Properties));
                    break;
                case nameof(JsonSource):
                    fields.AddRange(GetJsonFields(activityRecord.Properties));
                    break;
                case nameof(ExcelSource):
                    fields.AddRange(await GetExcelFieldsAsync(activityRecord.Properties));
                    break;
            }
        }

        return fields;
    }

    private async Task<IEnumerable<string>> GetContentItemFieldsAsync(JsonObject properties)
    {
        var fields = new List<string>
        {
            "ContentItemId",
            "ContentItemVersionId",
            "ContentType",
            "DisplayText",
            "Owner",
            "CreatedUtc",
            "ModifiedUtc",
            "PublishedUtc",
            "Published",
            "Latest",
        };

        var contentType = properties?["ContentType"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return fields;
        }

        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentType);
        if (typeDefinition == null)
        {
            return fields;
        }

        foreach (var part in typeDefinition.Parts)
        {
            fields.Add(part.Name);

            foreach (var field in part.PartDefinition.Fields)
            {
                fields.Add($"{part.Name}.{field.Name}");
            }
        }

        return fields;
    }

    private static IEnumerable<string> GetJsonFields(JsonObject properties)
    {
        var data = properties?["Data"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(data))
        {
            return [];
        }

        try
        {
            if (JsonNode.Parse(data) is not JsonArray array || array.Count == 0 || array[0] is not JsonObject firstObject)
            {
                return [];
            }

            var fields = new List<string>();
            CollectJsonPaths(firstObject, null, fields);
            return fields;
        }
        catch
        {
            return [];
        }
    }

    private async Task<IEnumerable<string>> GetExcelFieldsAsync(JsonObject properties)
    {
        var filePath = properties?["FilePath"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return [];
        }

        var normalizedPath = _mediaFileStore.NormalizePath(filePath);
        var fileInfo = await _mediaFileStore.GetFileInfoAsync(normalizedPath);
        if (fileInfo == null)
        {
            return [];
        }

        await using var stream = await _mediaFileStore.GetFileStreamAsync(fileInfo);
        using var spreadsheetDocument = SpreadsheetDocument.Open(stream, false);
        var workbookPart = spreadsheetDocument.WorkbookPart;

        if (workbookPart?.Workbook == null)
        {
            return [];
        }

        var worksheetName = properties?["WorksheetName"]?.GetValue<string>();
        var sheet = string.IsNullOrWhiteSpace(worksheetName)
            ? workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault()
            : workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(x => string.Equals(x.Name?.Value, worksheetName, StringComparison.OrdinalIgnoreCase));

        if (sheet == null)
        {
            return [];
        }

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        var headerRow = sheetData?.Elements<Row>().FirstOrDefault();
        if (headerRow == null)
        {
            return [];
        }

        var sharedStringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault()?.SharedStringTable;
        return GetRowValues(headerRow, sharedStringTable)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim());
    }

    private static void CollectJsonPaths(JsonObject obj, string prefix, IList<string> fields)
    {
        foreach (var property in obj)
        {
            var path = string.IsNullOrWhiteSpace(prefix) ? property.Key : $"{prefix}.{property.Key}";

            if (property.Value is JsonObject childObject)
            {
                CollectJsonPaths(childObject, path, fields);
            }
            else
            {
                fields.Add(path);
            }
        }
    }

    private static List<string> GetRowValues(Row row, SharedStringTable sharedStringTable)
    {
        var values = new List<string>();

        foreach (var cell in row.Elements<Cell>())
        {
            var columnIndex = GetColumnIndexFromCellReference(cell.CellReference);

            while (values.Count <= columnIndex)
            {
                values.Add(string.Empty);
            }

            values[columnIndex] = GetCellValue(cell, sharedStringTable);
        }

        return values;
    }

    private static int GetColumnIndexFromCellReference(StringValue cellReference)
    {
        var reference = cellReference?.Value ?? string.Empty;
        var columnName = new string(reference.TakeWhile(char.IsLetter).ToArray());
        var columnIndex = 0;

        foreach (var ch in columnName)
        {
            columnIndex *= 26;
            columnIndex += ch - 'A' + 1;
        }

        return Math.Max(columnIndex - 1, 0);
    }

    private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
    {
        if (cell.CellValue == null)
        {
            return string.Empty;
        }

        var value = cell.CellValue.InnerText;

        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(value, out var sharedStringIndex) &&
            sharedStringTable?.ElementAtOrDefault(sharedStringIndex) is SharedStringItem sharedStringItem)
        {
            return sharedStringItem.InnerText;
        }

        return value;
    }
}
