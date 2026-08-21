# Content Transfer (`OrchardCore.ContentTransfer`)

This feature provides a way to bulk import and export content using Excel files (`.xlsx` format). Once the feature is enabled, you can navigate to **Content** > **Bulk Transfers** to import data, and **Content** > **Bulk Export** to export data.

## Getting Started

### 1. Enable the Module

Enable the **Content Transfer** feature from **Admin** > **Configuration** > **Features**.

### 2. Enable Bulk Import/Export on Content Types

Before you can import or export content items, you must enable bulk transfer on the desired content types:

1. Navigate to **Admin** > **Content** > **Content Types**.
2. Edit the content type you want to enable (e.g., `Article`).
3. Check the **Allow Bulk Import** and/or **Allow Bulk Export** options under the content type settings.
4. Save the content type.

Only content types with these options enabled will appear in the import and export interfaces.

### 3. Optional: Enable Notifications

If you enable the **OrchardCore.Notifications** module, users will receive in-app notifications when queued export files are ready for download.

## Supported File Format

The Content Transfer feature supports only **Excel Workbook (`.xlsx`)** files. This format is based on the Office Open XML standard and provides:

- Better compatibility across different systems
- Structured data handling with proper data types
- Support for large datasets

> **Note:** Older Excel formats (`.xls`) and CSV files are not supported.

## Bulk Import

### Importing Content

1. Navigate to **Content** > **Bulk Transfers**.
2. Click the **Import** button and select the content type you want to import.
3. Upload an Excel file (`.xlsx`). You can download a template for the selected content type to see the expected column format.
4. The file is uploaded and queued for background processing. A background task processes the file in configurable batch sizes, with checkpoint-based resume to ensure records are not re-imported if the process is restarted.
5. Progress is displayed on the **Bulk Transfers** list showing the number of records processed out of the total, along with error counts and status badges.

### Import Validation

Imported records are validated using `IContentManager.ValidateAsync()`. Rows that fail validation are tracked. When an import has validation errors:

- The error count is displayed as a badge on the import entry.
- A **Download errors** option is available in the **Actions** dropdown menu, which generates an Excel file containing only the rejected rows for review and re-import.

### Template Download

For each importable content type, you can download a template Excel file that contains the expected column headers. This template is generated based on the registered import handlers and includes column descriptions and required/optional indicators.

## Bulk Export

### Exporting Content

1. Navigate to **Content** > **Bulk Export**.
2. Select the content type you want to export.
3. Choose the export scope:
   - **Export all published** (default): Exports all published content items.
   - **Partial Export**: Enables additional filters:
     - **Created from/to**: Filter by creation date range.
     - **Modified from/to**: Filter by modification date range.
     - **Owners**: Comma-separated list of usernames to filter by content owner.
     - **Published only**: Export only published versions.
     - **Latest only**: Export only the latest version of each content item.
     - **All versions**: Export all versions of content items.
4. Click **Export**.

### Immediate vs. Queued Export

- If the number of matching records is **≤500** (configurable), the export file is generated immediately and downloaded directly.
- If the number of matching records is **>500**, the export is queued for background processing:
  - A background task generates the export file using memory-efficient pagination.
  - When the `OrchardCore.Notifications` module is enabled, the user is notified when the file is ready.
  - The user can visit the **Export Dashboard** (**Content** > **Export Dashboard**) to check the status and download the completed file.

### Export Dashboard

The Export Dashboard shows all queued export requests for the current user, including:

- **Status**: Pending, In Progress, Completed, or Failed.
- **Download**: A download button appears for completed exports.
- **Search**: A client-side search bar to filter entries by content type.

## Configuration

The module can be configured via `appsettings.json` using the `OrchardCore_ContentTransfer` section:

```json
{
  "OrchardCore_ContentTransfer": {
    "ImportBatchSize": 100,
    "ExportBatchSize": 200,
    "ExportQueueThreshold": 500
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `ImportBatchSize` | 100 | Number of records processed per batch during import. |
| `ExportBatchSize` | 200 | Number of records fetched per page during export. |
| `ExportQueueThreshold` | 500 | Maximum number of records for immediate export. Records above this threshold are queued for background processing. |

## Memory Efficiency

The Content Transfer module is designed to handle large datasets without excessive memory consumption:

- **Import**: The uploaded file is read in streaming batches. Only the current batch of rows is held in memory at a time. Processed rows are cleared before the next batch is loaded.
- **Export**: Content items are fetched page-by-page and written directly to a temporary file stream. The file is never fully loaded into memory.

## Adding a Custom Part Importer

You can create a custom import/export handler for a content part by implementing the `IContentPartImportHandler` interface. The following illustrates an example of defining the title part importer.

```csharp
public sealed class TitlePartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    private ImportColumn _column;

    internal readonly IStringLocalizer S;

    public TitlePartContentImportHandler(IStringLocalizer<TitlePartContentImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        if (_column == null)
        {
            var settings = context.ContentTypePartDefinition.GetSettings<TitlePartSettings>();

            if (settings.Options == TitlePartOptions.Editable || settings.Options == TitlePartOptions.EditableRequired)
            {
                _column = new ImportColumn()
                {
                    Name = $"{context.ContentTypePartDefinition.Name}_{nameof(TitlePart.Title)}",
                    Description = S["The title for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
                    IsRequired = settings.Options == TitlePartOptions.EditableRequired,
                };
            }
        }

        if (_column == null)
        {
            return Array.Empty<ImportColumn>();
        }

        return new[] { _column };
    }

    public Task ImportAsync(ContentPartImportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Columns, nameof(context.Columns));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

        if (_column?.Name != null)
        {
            foreach (DataColumn column in context.Columns)
            {
                if (!Is(column.ColumnName, _column))
                {
                    continue;
                }
                var title = context.Row[column]?.ToString();

                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                context.ContentItem.DisplayText = title;
                context.ContentItem.Alter<TitlePart>(part =>
                {
                    part.Title = title.Trim();
                });
            }
        }

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentPartExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

        if (_column?.Name != null)
        {
            context.Row[_column.Name] = context.ContentItem.DisplayText;
        }

        return Task.CompletedTask;
    }
}
```

Register the custom implementation in your module's `Startup`:

```csharp
services.AddContentPartImportHandler<TitlePart, TitlePartContentImportHandler>();
```

## Adding a Custom Field Importer

You can create a custom import/export handler for a content field by either implementing the `IContentFieldImportHandler` interface or inheriting from the `StandardFieldImportHandler` class. The following illustrates an example of defining a text field importer.

```csharp
public sealed class TextFieldImportHandler : StandardFieldImportHandler
{
    private readonly IStringLocalizer S;

    public TextFieldImportHandler(IStringLocalizer<TextFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override string BindingPropertyName
        => nameof(TextField.Text);

    protected override Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        context.ContentPart.Alter<TextField>(context.ContentPartFieldDefinition.Name, (field) =>
        {
            field.Text = text?.Trim();
        });

        return Task.CompletedTask;
    }

    protected override Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<TextField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Text);
    }

    protected override string Description(ImportContentFieldContext context)
        => S["A text value for {0}", context.ContentPartFieldDefinition.DisplayName()];

    protected override bool IsRequired(ImportContentFieldContext context)
        => context.ContentPartFieldDefinition.GetSettings<TextFieldSettings>()?.Required ?? false;

    protected override string[] GetValidValues(ImportContentFieldContext context)
    {
        var predefined = context.ContentPartFieldDefinition.GetSettings<TextFieldPredefinedListEditorSettings>();

        if (predefined == null)
        {
            var multiText = context.ContentPartFieldDefinition.GetSettings<MultiTextFieldSettings>();

            return multiText?.Options?.Select(x => x.Value)?.ToArray() ?? [];
        }

        return predefined?.Options?.Select(x => x.Value)?.ToArray() ?? [];
    }
}
```

Register the custom implementation in your module's `Startup`:

```csharp
services.AddContentFieldImportHandler<TextField, TextFieldImportHandler>();
```

## Adding a Custom Content Item Importer

You can create a custom import/export handler for common content item properties by implementing the `IContentImportHandler` interface. The following illustrates an example of defining the common properties importer.

```csharp
public sealed class CommonContentImportHandler : ContentImportHandlerBase, IContentImportHandler
{
    private readonly IContentItemIdGenerator _contentItemIdGenerator;
    private readonly IStringLocalizer S;

    public CommonContentImportHandler(
        IStringLocalizer<TitlePartContentImportHandler> stringLocalizer,
        IContentItemIdGenerator contentItemIdGenerator)
    {
        _contentItemIdGenerator = contentItemIdGenerator;
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentContext context)
    {
        return new[]
        {
            new ImportColumn()
            {
                Name = nameof(ContentItem.ContentItemId),
                Description = S["The id for the {0}", context.ContentTypeDefinition.DisplayName],
            },
            new ImportColumn()
            {
                Name = nameof(ContentItem.CreatedUtc),
                Description = S["The UTC created datetime value {0}", context.ContentTypeDefinition.DisplayName],
                Type = ImportColumnType.ExportOnly,
            },
            new ImportColumn()
            {
                Name = nameof(ContentItem.ModifiedUtc),
                Description = S["The UTC last modified datetime value {0}", context.ContentTypeDefinition.DisplayName],
                Type = ImportColumnType.ExportOnly,
            },
        };
    }

    public Task ImportAsync(ContentImportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (DataColumn column in context.Columns)
        {
            if (Is(column.ColumnName, nameof(ContentItem.ContentItemId)))
            {
                var contentItemId = context.Row[column]?.ToString();

                if (string.IsNullOrWhiteSpace(contentItemId))
                {
                    continue;
                }

                var fakeId = _contentItemIdGenerator.GenerateUniqueId(new ContentItem());

                if (fakeId.Length == contentItemId.Length)
                {
                    context.ContentItem.ContentItemId = contentItemId;
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentExportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Row[nameof(ContentItem.ContentItemId)] = context.ContentItem.ContentItemId;
        context.Row[nameof(ContentItem.CreatedUtc)] = context.ContentItem.CreatedUtc;
        context.Row[nameof(ContentItem.ModifiedUtc)] = context.ContentItem.ModifiedUtc;

        return Task.CompletedTask;
    }
}
```

Register the custom implementation in your module's `Startup`:

```csharp
services.AddScoped<IContentImportHandler, CommonContentImportHandler>();
```

## Built-in Import/Export Handlers

The following handlers are provided out of the box:

| Handler | Type | Description |
|---------|------|-------------|
| `CommonContentImportHandler` | Content Item | Handles `ContentItemId`, `CreatedUtc`, `ModifiedUtc` properties. |
| `TitlePartContentImportHandler` | Content Part | Handles `TitlePart.Title`. |
| `HtmlBodyPartContentImportHandler` | Content Part | Handles `HtmlBodyPart.Html`. |
| `AutoroutePartContentImportHandler` | Content Part | Handles `AutoroutePart.Path`. |
| `TextFieldImportHandler` | Content Field | Handles `TextField.Text`. |
| `BooleanFieldImportHandler` | Content Field | Handles `BooleanField.Value`. |
| `NumberFieldImportHandler` | Content Field | Handles `NumericField.Value`. |
| `DateFieldImportHandler` | Content Field | Handles `DateField.Value`. |
| `DateTimeFieldImportHandler` | Content Field | Handles `DateTimeField.Value`. |
| `TimeFieldImportHandler` | Content Field | Handles `TimeField.Value`. |
| `ContentPickerFieldImportHandler` | Content Field | Handles `ContentPickerField.ContentItemIds`. |
