using System.ComponentModel.DataAnnotations;
using Lucene.Net.Util;
using System.Text.Json.Serialization;
using OrchardCore.Contents.Indexing;

namespace OrchardCore.Lucene.Endpoints.Management;

/// <summary>Editable settings of a Lucene content index, excluding private extension properties.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LuceneIndexDefinition
{
    /// <summary>Gets or sets the unique administrative display name.</summary>
    [Required, StringLength(255)]
    public string Name { get; set; }

    /// <summary>Gets or sets the provider index name, immutable after creation.</summary>
    [Required, StringLength(255)]
    public string IndexName { get; set; }

    /// <summary>Gets or sets whether to index the latest version, including drafts.</summary>
    public bool IndexLatest { get; set; }

    /// <summary>Gets or sets the content types included in this index.</summary>
    [Required, MinLength(1), MaxLength(200)]
    public string[] IndexedContentTypes { get; set; } = [];

    /// <summary>Gets or sets the content culture, or any for all cultures.</summary>
    [StringLength(100)]
    public string Culture { get; set; } = "any";

    /// <summary>Gets or sets a registered indexing analyzer.</summary>
    [StringLength(255)]
    public string AnalyzerName { get; set; } = LuceneConstants.DefaultAnalyzer;

    /// <summary>Gets or sets whether to store source document data.</summary>
    public bool StoreSourceData { get; set; }

    /// <summary>Gets or sets a registered default query analyzer.</summary>
    [StringLength(255)]
    public string QueryAnalyzerName { get; set; } = LuceneConstants.DefaultAnalyzer;

    /// <summary>Gets or sets whether the default query permits Lucene query syntax.</summary>
    public bool AllowLuceneQueries { get; set; }

    /// <summary>Gets or sets the Lucene compatibility version for the default query.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<LuceneVersion>))]
    public LuceneVersion DefaultVersion { get; set; } = LuceneConstants.DefaultVersion;

    /// <summary>Gets or sets the fields searched by the default query.</summary>
    [MaxLength(200)]
    public string[] DefaultSearchFields { get; set; } = [ContentIndexingConstants.FullTextKey];
}

/// <summary>A stored Lucene content index's administrative identifier and editable definition.</summary>
public sealed class LuceneIndexDefinitionResponse
{
    /// <summary>Gets or sets the stable profile identifier.</summary>
    public string Id { get; set; }

    /// <summary>Gets or sets the public editable settings.</summary>
    public LuceneIndexDefinition Definition { get; set; }
}
