using System;
using System.Collections.Generic;

namespace OrchardCore.Contents;

public class ContentsAdminListFilterOptions
{
    /// <summary>
    /// The default term name to use when no term is defined in the <see cref="DefaultTermNames"/>.
    /// </summary>
    public const string DefaultTermName = "text";

    /// <summary>
    /// This dictionary enables you to associate a content type or stereotype with a default term.
    /// The dictionary's keys should encompass content types or stereotypes.
    /// The dictionary's value should indicate the custom term name to utilize during text searches.
    /// </summary>
    public readonly Dictionary<string, string> DefaultTermNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// When true, the entire text will be treated as a single term enabling an exact match search.
    /// This means that search engines will treat the entire term as a single keyword or phrase, rather than individual words.
    /// </summary>
    public bool UseExactMatch { get; set; }
}
