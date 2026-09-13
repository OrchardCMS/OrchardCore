using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Tenants.Models;

namespace OrchardCore.Tenants.Services;

public class FeatureProfilesManager
{
    private readonly IDocumentManager<FeatureProfilesDocument> _documentManager;

    private readonly FeatureProfilesRuleOptions _rules;
    private readonly IStringLocalizer S;

    /// <summary>Creates a manager using the registered feature-rule providers.</summary>
    public FeatureProfilesManager(IDocumentManager<FeatureProfilesDocument> documentManager,
        IOptions<FeatureProfilesRuleOptions> rules, IStringLocalizer<FeatureProfilesManager> localizer)
    {
        _documentManager = documentManager;
        _rules = rules.Value;
        S = localizer;
    }

    /// <summary>Validates a definition using the same rules as admin and recipe updates.</summary>
    public async Task<IDictionary<string, string[]>> ValidateFeatureProfileAsync(string id, FeatureProfile profile, bool isNew = false)
        => Validate(id, profile, await GetFeatureProfilesDocumentAsync(), isNew);

    private Dictionary<string, string[]> Validate(string id, FeatureProfile profile, FeatureProfilesDocument document, bool isNew)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(id) || id.Contains(',') || id != id.Trim())
        {
            errors["Id"] = [S["A feature profile identifier is required and cannot contain a comma or surrounding whitespace."]];
        }
        if (profile is null)
        {
            errors["FeatureRules"] = [S["A feature profile definition is required."]];
            return errors;
        }
        if (profile.Id is not null && !string.Equals(profile.Id, id, StringComparison.OrdinalIgnoreCase))
        {
            errors["Id"] = [S["The profile identifier must match its stored key."]];
        }
        var name = profile.Name ?? id;
        if (string.IsNullOrWhiteSpace(name))
        {
            errors["Name"] = [S["A feature profile name is required."]];
        }
        else if (document.FeatureProfiles.Any(entry => (isNew || !string.Equals(entry.Key, id, StringComparison.OrdinalIgnoreCase)) &&
            string.Equals(entry.Value?.Name ?? entry.Key, name, StringComparison.Ordinal)))
        {
            errors["Name"] = [S["A feature profile with the same name already exists."]];
        }
        if (isNew && id is not null && document.FeatureProfiles.ContainsKey(id))
        {
            errors["Id"] = [S["A feature profile with the same identifier already exists."]];
        }
        if (profile.FeatureRules is null || profile.FeatureRules.Any(rule => rule is null ||
            string.IsNullOrWhiteSpace(rule.Rule) || !_rules.Rules.ContainsKey(rule.Rule) || string.IsNullOrWhiteSpace(rule.Expression)))
        {
            errors["FeatureRules"] = [S["Feature rules must contain registered rule names and nonempty expressions."]];
        }
        return errors;
    }

    /// <summary>
    /// Loads the feature profiles document from the store for updating and that should not be cached.
    /// </summary>
    public Task<FeatureProfilesDocument> LoadFeatureProfilesDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the feature profiles document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<FeatureProfilesDocument> GetFeatureProfilesDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

    /// <summary>Removes a stored profile, without saving when it is already absent.</summary>
    public async Task RemoveFeatureProfileAsync(string id)
    {
        var document = await LoadFeatureProfilesDocumentAsync();
        if (document.FeatureProfiles.Remove(id))
        {
            await _documentManager.UpdateAsync(document);
        }
    }

    /// <summary>Updates a profile, preserving rule order and skipping equivalent definitions.</summary>
    public async Task UpdateFeatureProfileAsync(string id, FeatureProfile profile)
    {
        var document = await LoadFeatureProfilesDocumentAsync();
        var errors = Validate(id, profile, document, isNew: false);
        if (errors.Count > 0)
        {
            throw new ValidationException(string.Join(" ", errors.Values.SelectMany(messages => messages)));
        }
        if (document.FeatureProfiles.TryGetValue(id, out var current) && AreEquivalent(id, current, profile))
        {
            return;
        }

        document.FeatureProfiles[id] = profile;
        await _documentManager.UpdateAsync(document);
    }

    internal static bool AreEquivalent(string id, FeatureProfile left, FeatureProfile right) =>
        left is not null && right is not null &&
        string.Equals(left.Id ?? id, right.Id ?? id, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(left.Name ?? id, right.Name ?? id, StringComparison.Ordinal) &&
        left.FeatureRules is not null && right.FeatureRules is not null &&
        left.FeatureRules.Count == right.FeatureRules.Count &&
        left.FeatureRules.Zip(right.FeatureRules).All(pair =>
            pair.First is not null && pair.Second is not null &&
            string.Equals(pair.First.Rule, pair.Second.Rule, StringComparison.Ordinal) &&
            string.Equals(pair.First.Expression, pair.Second.Expression, StringComparison.Ordinal));
}
