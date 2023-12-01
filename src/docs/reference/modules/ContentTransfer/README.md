# Content Transfer (`OrchardCore.ContentTransfer`)

This feature provides you a way to bulk import and export content using Excel files. One the feature is enabled, you can navigate to `Content` > `Bulk Transfers` to import/export files.

## Adding a Custom Part Importer

You have the option to establish a field importer for a custom field by either implementing the `IContentPartImportHandler` interface. The following illustrates an example of defining the title part importer.

```
public class TitlePartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    protected readonly IStringLocalizer S;
    private ImportColumn _column;

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

Finally, you can register the custom implementation like this

```
services.AddContentPartImportHandler<TitlePart, TitlePartContentImportHandler>();
```

## Adding a Custom Field Importer

You have the option to establish a field importer for a custom field by either implementing the `IContentFieldImportHandler` interface or inheriting from the `StandardFieldImportHandler` class. The following illustrates an example of defining a text field importer.

```
public class TextFieldImportHandler : StandardFieldImportHandler
{
    protected readonly IStringLocalizer S;

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

Finally, you can register the custom implementation like this

```
services.AddContentFieldImportHandler<TextField, TextFieldImportHandler>();
```