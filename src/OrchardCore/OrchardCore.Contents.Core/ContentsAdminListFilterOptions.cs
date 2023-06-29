using System;
using System.Collections.Generic;

namespace OrchardCore.Contents;

public class ContentsAdminListFilterOptions
{
    private readonly Dictionary<string, string> _defaultTermNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The default term name to use when no term is defined in the <see cref="_defaultTermNames"/>.
    /// </summary>
    public const string DefaultTermName = "text";

    /// <summary>
    /// When true, the entire searched terms will be enclose within quotation marks, it creates an exact match search.
    /// This means that search engines will treat the entire term as a single keyword or phrase, rather than individual words.
    /// </summary>
    public bool UseExactMatch { get; set; }

    /// <summary>
    /// Gets the default term name to use for the given content type.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="termName"></param>
    /// <returns></returns>
    public bool TryGetDefaultTermName(string contentType, out string termName)
        => _defaultTermNames.TryGetValue(contentType, out termName);

    /// <summary>
    /// Adds a default term name for the given content type if one does not exists.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="termName"></param>
    public void AddDefaultTermName(string contentType, string termName)
        => _defaultTermNames.TryAdd(contentType, termName);

    /// <summary>
    /// Removes the default term name for the given content type.
    /// </summary>
    /// <param name="contentType"></param>
    public void RemoveDefaultTermName(string contentType)
        => _defaultTermNames.Remove(contentType);
}
