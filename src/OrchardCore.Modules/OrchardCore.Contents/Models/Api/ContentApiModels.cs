using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Models;

public sealed class ContentItemsResponse
{
    [JsonPropertyName("skip")]
    public int Skip { get; set; }

    [JsonPropertyName("take")]
    public int Take { get; set; }

    /// <summary>
    /// Gets or sets the number of matching items the caller can view. When <see cref="TotalCountIsEstimate"/>
    /// is <see langword="true"/>, this is a lower bound and more items may exist.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="TotalCount"/> only counts the items read to build the page, which
    /// happens when access has to be checked on each item. The property is omitted when the count is exact.
    /// </summary>
    [JsonPropertyName("totalCountIsEstimate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool TotalCountIsEstimate { get; set; }

    [JsonPropertyName("items")]
    public List<ContentItem> Items { get; set; } = [];
}

public sealed class ContentItemValidationResponse
{
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("errors")]
    public Dictionary<string, string[]> Errors { get; set; } = [];
}

public sealed class ContentItemRenderResponse
{
    /// <summary>
    /// Gets or sets the identifier of the content item version that was rendered.
    /// </summary>
    [JsonPropertyName("contentItemVersionId")]
    public string ContentItemVersionId { get; set; }

    [JsonPropertyName("contentItemId")]
    public string ContentItemId { get; set; }

    [JsonPropertyName("displayType")]
    public string DisplayType { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }
}

public sealed class ContentItemSchemaResponse
{
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; }

    [JsonPropertyName("schema")]
    public JsonObject Schema { get; set; } = [];
}
