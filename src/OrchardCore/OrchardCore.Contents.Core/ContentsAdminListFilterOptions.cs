using System;
using System.Collections.Generic;

namespace OrchardCore.Contents;

public class ContentsAdminListFilterOptions
{
    /// <summary>
    /// The default term name to use if not defined for a given content type in the <see cref="DefaultTermNames"/>.
    /// </summary>
    public const string DefaultTermName = "text";

    /// <summary>
    /// This dictionary enables you to associate a content type or stereotype with a default term name.
    /// The dictionary's keys should encompass content types or stereotypes, the dictionary's values
    /// should indicate the default term names per content type to utilize during text searches.
    /// </summary>
    public readonly Dictionary<string, string> DefaultTermNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether or not the entire text should be parsed as a single term, enabling an exact match search.
    /// This means that search engines will treat the text as a whole rather than individual words.
    /// </summary>
    public bool UseExactMatch { get; set; }
}
