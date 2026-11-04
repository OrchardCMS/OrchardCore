using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Templates.Endpoints.Management;

/// <summary>
/// Describes filters and paging for stored templates.
/// </summary>
public sealed class TemplateListRequest
{
    /// <summary>
    /// Gets or sets case-insensitive text matched against template names and descriptions.
    /// </summary>
    public string Search { get; init; }

    /// <summary>
    /// Gets or sets the zero-based number of matching templates to skip. The default is 0.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int? Skip { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of templates to return. The default is 50 and the maximum is 200.
    /// </summary>
    [Range(1, 200)]
    public int? Take { get; init; }
}

/// <summary>
/// Describes a complete stored custom Liquid template definition.
/// </summary>
public sealed class TemplateDefinitionDto
{
    /// <summary>
    /// Gets or sets the stable, case-insensitive template name, such as <c>Content__Article</c>.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the non-empty Liquid template source.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional editor-facing description.
    /// </summary>
    public string Description { get; init; }
}

/// <summary>
/// Describes a page of stored custom templates.
/// </summary>
public sealed class TemplateListResponse
{
    /// <summary>
    /// Gets the zero-based number of matching templates skipped.
    /// </summary>
    public int Skip { get; init; }

    /// <summary>
    /// Gets the maximum number of templates requested.
    /// </summary>
    public int Take { get; init; }

    /// <summary>
    /// Gets the total number of templates matching the filter before paging.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the templates in the current page.
    /// </summary>
    public IReadOnlyList<TemplateDefinitionDto> Items { get; init; } = [];
}
