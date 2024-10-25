namespace OrchardCore.Users.Models;

public class UsersAdminListFilterOptions
{
    /// <summary>
    /// The default term name to use if not defined in <see cref="TermName"/>.
    /// </summary>
    public const string DefaultTermName = "name";

    /// <summary>
    /// The term name to use when performing text search.
    /// </summary>
    public string TermName { get; set; } = DefaultTermName;

    /// <summary>
    /// Whether or not the entire text should be parsed as a single term, enabling an exact match search.
    /// This means that search engines will treat the text as a whole rather than individual words.
    /// </summary>
    public bool UseExactMatch { get; set; }
}
