using System.Security.Cryptography;
using System.Text;
using OrchardCore.Documents;
using OrchardCore.Modules;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Lists the templates of the template sources with the state of their local overrides, and clones, resets
/// and deletes these overrides.
/// </summary>
public sealed class SourceTemplatesService
{
    private readonly IEnumerable<ITemplateSource> _sources;
    private readonly ShapeTemplateFiles _shapeTemplateFiles;
    private readonly TemplatesManager _templatesManager;
    private readonly AdminTemplatesManager _adminTemplatesManager;
    private readonly IDocumentManager<TemplateOverridesDocument> _overridesManager;
    private readonly IDocumentManager<AdminTemplateOverridesDocument> _adminOverridesManager;
    private readonly IClock _clock;

    public SourceTemplatesService(
        IEnumerable<ITemplateSource> sources,
        ShapeTemplateFiles shapeTemplateFiles,
        TemplatesManager templatesManager,
        AdminTemplatesManager adminTemplatesManager,
        IDocumentManager<TemplateOverridesDocument> overridesManager,
        IDocumentManager<AdminTemplateOverridesDocument> adminOverridesManager,
        IClock clock)
    {
        _sources = sources;
        _shapeTemplateFiles = shapeTemplateFiles;
        _templatesManager = templatesManager;
        _adminTemplatesManager = adminTemplatesManager;
        _overridesManager = overridesManager;
        _adminOverridesManager = adminOverridesManager;
        _clock = clock;
    }

    /// <summary>
    /// Gets the registered template sources.
    /// </summary>
    public IEnumerable<ITemplateSource> Sources => _sources;

    /// <summary>
    /// Gets a template source by its name.
    /// </summary>
    public ITemplateSource GetSource(string name)
        => _sources.LastOrDefault(source => string.Equals(source.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Lists the templates of a source, sorted by name, with the state of their local overrides.
    /// </summary>
    public async Task<IReadOnlyList<SourceTemplateEntry>> GetEntriesAsync(ITemplateSource source, bool adminTemplates)
    {
        var templates = await source.GetTemplatesAsync(adminTemplates);
        var localTemplates = await GetTemplatesDocumentAsync(adminTemplates);
        var overrides = await GetOverridesDocumentAsync(adminTemplates);

        var entries = new List<SourceTemplateEntry>();

        foreach (var template in templates.OrderBy(template => template.Name, StringComparer.OrdinalIgnoreCase))
        {
            var entry = new SourceTemplateEntry
            {
                Source = source.Name,
                Template = template,
                AdminTemplates = adminTemplates,
            };

            if (localTemplates.Templates.TryGetValue(template.Name, out var localTemplate))
            {
                entry.IsOverridden = true;

                if (overrides.Overrides.TryGetValue(template.Name, out var record))
                {
                    entry.IsCloned = true;
                    entry.IsModified = !string.Equals(ComputeHash(localTemplate.Content), record.Hash, StringComparison.Ordinal);

                    var content = await _shapeTemplateFiles.ReadAsync(template);
                    entry.IsSourceChanged = content != null && !string.Equals(ComputeHash(content), record.Hash, StringComparison.Ordinal);
                }
            }

            entries.Add(entry);
        }

        return entries;
    }

    /// <summary>
    /// Finds a template of a source by its name.
    /// </summary>
    /// <returns>The template, or <see langword="null"/> when the source has no such template.</returns>
    public static async Task<SourceTemplate> FindTemplateAsync(ITemplateSource source, string name, bool adminTemplates)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return (await source.GetTemplatesAsync(adminTemplates))
            .FirstOrDefault(template => string.Equals(template.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Reads the content of a template of a source.
    /// </summary>
    public Task<string> ReadAsync(SourceTemplate template) => _shapeTemplateFiles.ReadAsync(template);

    /// <summary>
    /// Clones a Liquid template of a source as a local template, and records where it comes from to monitor it.
    /// </summary>
    /// <param name="source">The source of the template.</param>
    /// <param name="template">The template to clone.</param>
    /// <param name="adminTemplates">Whether to clone it as an admin template.</param>
    /// <param name="overwrite">Whether to replace a local template with the same name, which resets it to the source.</param>
    /// <returns>Whether the template was cloned.</returns>
    public async Task<bool> CloneAsync(ITemplateSource source, SourceTemplate template, bool adminTemplates, bool overwrite)
    {
        if (!template.IsLiquid)
        {
            return false;
        }

        var content = await _shapeTemplateFiles.ReadAsync(template);

        if (string.IsNullOrEmpty(content))
        {
            return false;
        }

        var localTemplates = adminTemplates
            ? await _adminTemplatesManager.LoadTemplatesDocumentAsync()
            : await _templatesManager.LoadTemplatesDocumentAsync();

        localTemplates.Templates.TryGetValue(template.Name, out var localTemplate);

        if (localTemplate != null && !overwrite)
        {
            return false;
        }

        var clone = new Template
        {
            Content = content,

            // A reset keeps the description the template was given.
            Description = localTemplate?.Description ?? $"Cloned from {template.ExtensionId} ({template.RelativePath}).",
        };

        await (adminTemplates
            ? _adminTemplatesManager.UpdateTemplateAsync(template.Name, clone)
            : _templatesManager.UpdateTemplateAsync(template.Name, clone));

        var overrides = await LoadOverridesDocumentAsync(adminTemplates);

        overrides.Overrides[template.Name] = new TemplateOverride
        {
            Source = source.Name,
            ExtensionId = template.ExtensionId,
            RelativePath = template.RelativePath,
            Hash = ComputeHash(content),
            ClonedUtc = _clock.UtcNow,
        };

        await UpdateOverridesDocumentAsync(overrides, adminTemplates);

        return true;
    }

    /// <summary>
    /// Deletes the local template overriding a template, so that the template of the source is used again.
    /// </summary>
    /// <returns>Whether a local template was deleted.</returns>
    public async Task<bool> DeleteAsync(string name, bool adminTemplates)
    {
        var localTemplates = adminTemplates
            ? await _adminTemplatesManager.LoadTemplatesDocumentAsync()
            : await _templatesManager.LoadTemplatesDocumentAsync();

        if (!localTemplates.Templates.ContainsKey(name))
        {
            return false;
        }

        await (adminTemplates
            ? _adminTemplatesManager.RemoveTemplateAsync(name)
            : _templatesManager.RemoveTemplateAsync(name));

        var overrides = await LoadOverridesDocumentAsync(adminTemplates);

        if (overrides.Overrides.Remove(name))
        {
            await UpdateOverridesDocumentAsync(overrides, adminTemplates);
        }

        return true;
    }

    /// <summary>
    /// Computes the hash of a template content, ignoring the line endings, which can change when a file is checked out
    /// or a template is posted from a browser.
    /// </summary>
    public static string ComputeHash(string content)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes((content ?? string.Empty).ReplaceLineEndings("\n"))));

    private async Task<TemplatesDocument> GetTemplatesDocumentAsync(bool adminTemplates)
        => adminTemplates
            ? await _adminTemplatesManager.GetTemplatesDocumentAsync()
            : await _templatesManager.GetTemplatesDocumentAsync();

    private async Task<TemplateOverridesDocument> GetOverridesDocumentAsync(bool adminTemplates)
        => adminTemplates
            ? await _adminOverridesManager.GetOrCreateImmutableAsync()
            : await _overridesManager.GetOrCreateImmutableAsync();

    private async Task<TemplateOverridesDocument> LoadOverridesDocumentAsync(bool adminTemplates)
        => adminTemplates
            ? await _adminOverridesManager.GetOrCreateMutableAsync()
            : await _overridesManager.GetOrCreateMutableAsync();

    private Task UpdateOverridesDocumentAsync(TemplateOverridesDocument document, bool adminTemplates)
        => adminTemplates
            ? _adminOverridesManager.UpdateAsync((AdminTemplateOverridesDocument)document)
            : _overridesManager.UpdateAsync(document);
}
