namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="SaveContentItemsStep"/>.
/// </summary>
public sealed class SaveContentItemsStepSettings
{
    /// <summary>
    /// Gets or sets the content type of the items.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets whether rows create items, update them, or both.
    /// </summary>
    public ContentItemSaveMode Mode { get; set; } = ContentItemSaveMode.CreateOrUpdate;

    /// <summary>
    /// Gets or sets how a row finds the item it updates.
    /// </summary>
    public ContentItemMatch MatchBy { get; set; } = ContentItemMatch.ContentItemId;

    /// <summary>
    /// Gets or sets the field of the rows that holds the content item id or the display text of the item to update.
    /// </summary>
    public string KeyField { get; set; }

    /// <summary>
    /// Gets or sets the fields of the rows written to the parts and fields of the items.
    /// </summary>
    public List<ContentFieldMapping> Mappings { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the items are published, rather than saved as drafts.
    /// </summary>
    public bool Publish { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of items saved to the database at once.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}

/// <summary>
/// Identifies whether rows create content items, update them, or both.
/// </summary>
public enum ContentItemSaveMode
{
    /// <summary>
    /// A row updates the item it matches, or creates one.
    /// </summary>
    CreateOrUpdate,

    /// <summary>
    /// A row creates an item, unless it matches one, which it then skips.
    /// </summary>
    CreateOnly,

    /// <summary>
    /// A row updates the item it matches; a row that matches none is rejected.
    /// </summary>
    UpdateOnly,
}

/// <summary>
/// Identifies how a row finds the content item it updates.
/// </summary>
public enum ContentItemMatch
{
    /// <summary>
    /// By its content item id.
    /// </summary>
    ContentItemId,

    /// <summary>
    /// By its display text, such as its title.
    /// </summary>
    DisplayText,
}

/// <summary>
/// Writes a field of the rows to a part or field of the content items.
/// </summary>
public sealed class ContentFieldMapping
{
    /// <summary>
    /// Gets or sets the column of the content type written, such as <c>TitlePart.Title</c> or <c>Product.Price</c>.
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// Gets or sets the field of the rows that holds the value.
    /// </summary>
    public string Source { get; set; }
}
