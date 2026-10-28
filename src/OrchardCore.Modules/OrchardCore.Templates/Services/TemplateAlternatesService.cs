using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Gathers the alternates suggested by the <see cref="ITemplateAlternatesProvider"/> services, to find those of a template.
/// </summary>
public sealed class TemplateAlternatesService
{
    private readonly IEnumerable<ITemplateAlternatesProvider> _providers;
    private readonly Dictionary<bool, IReadOnlyList<TemplateAlternate>> _alternates = [];

    public TemplateAlternatesService(IEnumerable<ITemplateAlternatesProvider> providers)
    {
        _providers = providers;
    }

    /// <summary>
    /// Lists the alternates of a template: the more specific names of the shape it renders, e.g. <c>Content__BlogPost</c>
    /// and <c>Content_Summary__BlogPost</c> for <c>Content</c>, or only the latter for <c>Content_Summary</c>.
    /// An alternate several content types share is listed once, for none of them.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="adminTemplates">Whether to list the alternates rendered by the admin.</param>
    public async Task<IReadOnlyList<TemplateAlternate>> GetAlternatesAsync(string templateName, bool adminTemplates)
        => (await GetAllAsync(adminTemplates))
            .Where(alternate => IsAlternateOf(alternate, templateName))
            .ToList();

    /// <summary>
    /// Gets the names of the templates that have alternates, e.g. <c>Content</c>, <c>Content_Summary</c> or <c>TitlePart</c>.
    /// </summary>
    public async Task<ISet<string>> GetTemplatesWithAlternatesAsync(bool adminTemplates)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var alternate in await GetAllAsync(adminTemplates))
        {
            names.Add(alternate.ShapeType);
            names.Add($"{alternate.ShapeType}_{alternate.DisplayType}");
        }

        return names;
    }

    // The alternate renders the shape of the template, in any display type, or in the display type of the template.
    private static bool IsAlternateOf(TemplateAlternate alternate, string templateName)
        => !string.Equals(alternate.Name, templateName, StringComparison.OrdinalIgnoreCase)
            && (string.Equals(alternate.ShapeType, templateName, StringComparison.OrdinalIgnoreCase)
                || string.Equals($"{alternate.ShapeType}_{alternate.DisplayType}", templateName, StringComparison.OrdinalIgnoreCase));

    private async Task<IReadOnlyList<TemplateAlternate>> GetAllAsync(bool adminTemplates)
    {
        if (_alternates.TryGetValue(adminTemplates, out var alternates))
        {
            return alternates;
        }

        List<TemplateAlternate> all = [];

        foreach (var provider in _providers)
        {
            all.AddRange(await provider.GetAlternatesAsync(adminTemplates));
        }

        // The same name for several content types, e.g. TextField_Summary, applies to all of them.
        alternates = all
            .GroupBy(alternate => alternate.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var first = group.First();
                var sameDisplayType = group.All(alternate => string.Equals(alternate.DisplayType, first.DisplayType, StringComparison.OrdinalIgnoreCase));
                var sameContentType = group.All(alternate => string.Equals(alternate.ContentType, first.ContentType, StringComparison.OrdinalIgnoreCase));

                // E.g. two text fields of a content type share [ContentType]__TextField.
                var sameElement = sameContentType && group.All(alternate => string.Equals(alternate.Element, first.Element, StringComparison.OrdinalIgnoreCase));

                return new TemplateAlternate
                {
                    Name = first.Name,
                    ShapeType = first.ShapeType,
                    // E.g. [ContentType]__HtmlBodyPart, generated for each display type, applies to all of them.
                    DisplayType = sameDisplayType ? first.DisplayType : null,
                    ContentType = sameContentType ? first.ContentType : null,
                    Element = sameElement ? first.Element : null,
                };
            })
            .ToList();

        _alternates[adminTemplates] = alternates;

        return alternates;
    }
}
