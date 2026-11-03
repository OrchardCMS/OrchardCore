using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.Services;

namespace OrchardCore.AzureAI.Endpoints.Management;

/// <summary>Editable settings of an Azure AI Search content index, excluding private extension properties and field mappings.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class AzureAISearchIndexDefinition
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

    /// <summary>Gets or sets a configured indexing analyzer.</summary>
    [StringLength(255)]
    public string AnalyzerName { get; set; } = AzureAISearchDefaultOptions.DefaultAnalyzer;


    /// <summary>Gets or sets a configured default query analyzer.</summary>
    [StringLength(255)]
    public string QueryAnalyzerName { get; set; } = AzureAISearchDefaultOptions.DefaultAnalyzer;

    /// <summary>Gets or sets the fields searched by the default query.</summary>
    [MaxLength(200)]
    public string[] DefaultSearchFields { get; set; } = [AzureAISearchIndexManager.FullTextKey];
}

/// <summary>A stored Azure AI Search content index's administrative identifier and editable definition.</summary>
public sealed class AzureAISearchIndexDefinitionResponse
{
    /// <summary>Gets or sets the stable profile identifier.</summary>
    public string Id { get; set; }

    /// <summary>Gets or sets the public editable settings.</summary>
    public AzureAISearchIndexDefinition Definition { get; set; }
}
