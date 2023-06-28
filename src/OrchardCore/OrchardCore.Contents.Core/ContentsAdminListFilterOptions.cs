using System;
using System.Collections.Generic;

namespace OrchardCore.Contents;

public class ContentsAdminListFilterOptions
{
    public const string DefaultTermName = "text";

    private readonly Dictionary<string, string> _defaultTermNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the term-name to use by the given content type.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="termName"></param>
    /// <returns></returns>
    public bool TryGetDefaultTermName(string contentType, out string termName)
    {
        return _defaultTermNames.TryGetValue(contentType, out termName);
    }

    /// <summary>
    /// Adds a custom term name for the given content type if one does not exists.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="termName"></param>
    public void AddDefaultTermName(string contentType, string termName)
    {
        ArgumentNullException.ThrowIfNull(contentType, nameof(contentType));

        _defaultTermNames.TryAdd(contentType, termName);
    }

    /// <summary>
    /// Removes the content type from from the options if exists.
    /// </summary>
    /// <param name="contentType"></param>
    public void RemoveDefaultTermName(string contentType)
    {
        ArgumentNullException.ThrowIfNull(contentType, nameof(contentType));

        _defaultTermNames.Remove(contentType);
    }
}
