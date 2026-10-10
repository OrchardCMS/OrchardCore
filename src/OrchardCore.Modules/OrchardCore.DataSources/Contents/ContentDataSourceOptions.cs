namespace OrchardCore.DataSources.Contents;

/// <summary>
/// Describes how the content items data source reads the values of content parts and content fields. Modules that add
/// a part or a field type register its values here, so its data shows up as columns of the content type data sets:
/// <c>services.Configure&lt;ContentDataSourceOptions&gt;(options => options.Fields["MyField"] = [new("Value", DataFieldType.Text)]);</c>.
/// </summary>
public sealed class ContentDataSourceOptions
{
    /// <summary>
    /// Gets the values of each content field type, by field type name. A field with one value becomes the column
    /// <c>{PartName}.{FieldName}</c>; a field with several values becomes one <c>{PartName}.{FieldName}.{Value}</c>
    /// column per value.
    /// </summary>
    public Dictionary<string, ContentDataValue[]> Fields { get; } = new(StringComparer.Ordinal)
    {
        ["TextField"] = [new("Text", DataFieldType.Text)],
        ["NumericField"] = [new("Value", DataFieldType.Decimal)],
        ["BooleanField"] = [new("Value", DataFieldType.Boolean)],
        ["DateField"] = [new("Value", DataFieldType.Date)],
        ["DateTimeField"] = [new("Value", DataFieldType.DateTime)],
        ["TimeField"] = [new("Value", DataFieldType.Text)],
        ["HtmlField"] = [new("Html", DataFieldType.Text)],
        ["MarkdownField"] = [new("Markdown", DataFieldType.Text)],
        ["LinkField"] = [new("Url", DataFieldType.Text), new("Text", DataFieldType.Text)],
        ["ContentPickerField"] = [new("ContentItemIds", DataFieldType.Text) { IsList = true }],
        ["UserPickerField"] = [new("UserIds", DataFieldType.Text) { IsList = true }],
        ["TaxonomyField"] = [new("TermContentItemIds", DataFieldType.Text) { IsList = true }],
        ["MediaField"] = [new("Paths", DataFieldType.Text) { IsList = true }],
        ["MultiTextField"] = [new("Values", DataFieldType.Text) { IsList = true }],
        ["YoutubeField"] = [new("RawAddress", DataFieldType.Text)],
        ["LocalizationSetContentPickerField"] = [new("LocalizationSets", DataFieldType.Text) { IsList = true }],
    };

    /// <summary>
    /// Gets the values of each content part, by part name. Each value becomes the column <c>{PartName}.{Value}</c>.
    /// </summary>
    public Dictionary<string, ContentDataValue[]> Parts { get; } = new(StringComparer.Ordinal)
    {
        ["TitlePart"] = [new("Title", DataFieldType.Text)],
        ["AutoroutePart"] = [new("Path", DataFieldType.Text)],
        ["AliasPart"] = [new("Alias", DataFieldType.Text)],
        ["HtmlBodyPart"] = [new("Html", DataFieldType.Text)],
        ["MarkdownBodyPart"] = [new("Markdown", DataFieldType.Text)],
        ["ContainedPart"] = [new("ListContentItemId", DataFieldType.Text), new("Order", DataFieldType.Integer)],
        ["LocalizationPart"] = [new("Culture", DataFieldType.Text), new("LocalizationSet", DataFieldType.Text)],
        ["PublishLaterPart"] = [new("ScheduledPublishUtc", DataFieldType.DateTime)],
        ["ArchiveLaterPart"] = [new("ScheduledArchiveUtc", DataFieldType.DateTime)],
    };
}
