using Microsoft.Extensions.Localization;
using OrchardCore.Documents;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Models;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services;

public class ShortcodeTemplatesManager
{
    private readonly IDocumentManager<ShortcodeTemplatesDocument> _documentManager;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    internal readonly IStringLocalizer S;

    public ShortcodeTemplatesManager(IDocumentManager<ShortcodeTemplatesDocument> documentManager,
        ILiquidTemplateManager liquidTemplateManager, IHtmlSanitizerService htmlSanitizerService,
        IStringLocalizer<ShortcodeTemplatesManager> localizer)
    {
        _documentManager = documentManager;
        _liquidTemplateManager = liquidTemplateManager;
        _htmlSanitizerService = htmlSanitizerService;
        S = localizer;
    }

    /// <summary>
    /// Loads the shortcode templates document from the store for updating and that should not be cached.
    /// </summary>
    public Task<ShortcodeTemplatesDocument> LoadShortcodeTemplatesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the shortcode templates document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<ShortcodeTemplatesDocument> GetShortcodeTemplatesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

    /// <summary>Removes a template if present, invalidating the document cache only when it changes.</summary>
    public async Task RemoveShortcodeTemplateAsync(string name) => await RemoveIfExistsAsync(name);

    /// <summary>
    /// Upserts a template for recipe and programmatic callers, sanitizing its usage HTML.
    /// This compatibility path does not impose the admin editor's name and Liquid validation.
    /// </summary>
    public async Task UpdateShortcodeTemplateAsync(string name, ShortcodeTemplate template)
    {
        var document = await LoadShortcodeTemplatesDocumentAsync();
        document.ShortcodeTemplates[name.ToLowerInvariant()] = Normalize(template);
        await _documentManager.UpdateAsync(document);
    }

    internal Dictionary<string, string[]> Validate(string name, ShortcodeTemplate template)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = [S["The name is mandatory."]];
        }
        else if (name.Length > 256 || !IsValidShortcodeName(name))
        {
            errors["name"] = [S["The name must be a valid shortcode identifier of at most 256 characters."]];
        }
        if (string.IsNullOrEmpty(template?.Content))
        {
            errors["content"] = [S["The template content is mandatory."]];
        }
        else if (!_liquidTemplateManager.Validate(template.Content, out var liquidErrors))
        {
            errors["content"] = [S["The template doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", liquidErrors)]];
        }
        return errors;
    }

    internal async Task<ShortcodeTemplateMutationResult> SaveAsync(string name, ShortcodeTemplate template, string sourceName = null)
    {
        var errors = Validate(name, template);
        if (errors.Count > 0)
        {
            return new() { Status = ShortcodeTemplateMutationStatus.Invalid, Errors = errors };
        }
        var document = await LoadShortcodeTemplatesDocumentAsync();
        if (sourceName is not null && !document.ShortcodeTemplates.ContainsKey(sourceName))
        {
            return new() { Status = ShortcodeTemplateMutationStatus.NotFound };
        }
        var normalized = Normalize(template);
        var target = document.ShortcodeTemplates.FirstOrDefault(entry => string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase));
        if (target.Key is not null)
        {
            var equivalent = Equivalent(target.Value, normalized);
            if (sourceName is null || !string.Equals(sourceName, name, StringComparison.OrdinalIgnoreCase))
            {
                return new()
                {
                    Status = sourceName is null && equivalent ? ShortcodeTemplateMutationStatus.Existing : ShortcodeTemplateMutationStatus.Conflict,
                    Name = target.Key, Template = target.Value,
                };
            }
            if (equivalent)
            {
                return new() { Status = ShortcodeTemplateMutationStatus.Existing, Name = target.Key, Template = target.Value };
            }
        }
        if (sourceName is not null)
        {
            document.ShortcodeTemplates.Remove(sourceName);
        }
        var storedName = name.ToLowerInvariant();
        document.ShortcodeTemplates[storedName] = normalized;
        await _documentManager.UpdateAsync(document);
        return new() { Status = ShortcodeTemplateMutationStatus.Saved, Name = storedName, Template = normalized };
    }

    internal async Task<bool> RemoveIfExistsAsync(string name)
    {
        var document = await LoadShortcodeTemplatesDocumentAsync();
        if (!document.ShortcodeTemplates.Remove(name))
        {
            return false;
        }
        await _documentManager.UpdateAsync(document);
        return true;
    }

    private ShortcodeTemplate Normalize(ShortcodeTemplate template) => new()
    {
        Content = template.Content,
        Hint = template.Hint,
        Usage = _htmlSanitizerService.Sanitize(template.Usage),
        DefaultValue = template.DefaultValue,
        Categories = template.Categories?.ToArray() ?? [],
    };

    private static bool Equivalent(ShortcodeTemplate left, ShortcodeTemplate right) => left.Content == right.Content
        && left.Hint == right.Hint && left.Usage == right.Usage && left.DefaultValue == right.DefaultValue
        && (left.Categories ?? []).SequenceEqual(right.Categories ?? [], StringComparer.Ordinal);

    private static bool IsValidShortcodeName(string name)
    {
        try
        {
            var nodes = new ShortcodesParser().Parse($"[{name}]");
            return nodes.Count == 1 && nodes[0] is Shortcode shortcode
                && shortcode.Identifier.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
}

internal enum ShortcodeTemplateMutationStatus
{
    Saved,
    Existing,
    Conflict,
    Invalid,
    NotFound,
}

internal sealed class ShortcodeTemplateMutationResult
{
    public ShortcodeTemplateMutationStatus Status { get; init; }
    public string Name { get; init; }
    public ShortcodeTemplate Template { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
