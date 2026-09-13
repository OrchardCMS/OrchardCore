using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

/// <summary>Validates profile and provider names for domain handlers and the administration editor.</summary>
public sealed class IndexProfileIdentityValidator
{
    private readonly IIndexProfileStore _store;
    private readonly IStringLocalizer S;

    /// <summary>Creates the validator using the tenant's profile store.</summary>
    public IndexProfileIdentityValidator(IIndexProfileStore store, IStringLocalizer<IndexProfileIdentityValidator> localizer)
    {
        _store = store;
        S = localizer;
    }

    /// <summary>Validates required names, length limits and uniqueness, preserving an existing profile's identity.</summary>
    public async Task<IReadOnlyList<ValidationResult>> ValidateAsync(IndexProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var errors = new List<ValidationResult>();
        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            errors.Add(new ValidationResult(S["Index name is required."], [nameof(profile.Name)]));
        }
        else if (profile.Name.Length > 255)
        {
            errors.Add(new ValidationResult(S["The index name cannot be longer than 255 characters."], [nameof(profile.Name)]));
        }
        else
        {
            var existing = await _store.FindByNameAsync(profile.Name);
            if (existing is not null && existing.Id != profile.Id)
            {
                errors.Add(new ValidationResult(S["There is already another index with the same name."], [nameof(profile.Name)]));
            }
        }

        if (string.IsNullOrWhiteSpace(profile.IndexName))
        {
            errors.Add(new ValidationResult(S["The index name is required."], [nameof(profile.IndexName)]));
        }
        else if (profile.IndexName.Length > 255)
        {
            errors.Add(new ValidationResult(S["The index name cannot be longer than 255 characters."], [nameof(profile.IndexName)]));
        }
        else if (!string.IsNullOrWhiteSpace(profile.ProviderName))
        {
            var existing = await _store.FindByIndexNameAndProviderAsync(profile.IndexName, profile.ProviderName);
            if (existing is not null && existing.Id != profile.Id)
            {
                errors.Add(new ValidationResult(S["There is already another index with the same name."], [nameof(profile.IndexName)]));
            }
        }

        return errors;
    }
}
