using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAISettingsViewModel : IValidatableObject
{
    public string IndexName { get; set; }

    public string AnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string Culture { get; set; }

    public string[] IndexedContentTypes { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Analyzers { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Cultures { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var S = validationContext.GetRequiredService<IStringLocalizer<AzureAISettingsViewModel>>();

        if (IndexedContentTypes == null || IndexedContentTypes.Length == 0)
        {
            yield return new ValidationResult(S["At least one content type is required."], [nameof(IndexedContentTypes)]);
        }

        if (string.IsNullOrWhiteSpace(IndexName))
        {
            yield return new ValidationResult(S["The index name is required."], [nameof(IndexName)]);
        }
        else if (!AzureAISearchIndexNamingHelper.TryGetSafeIndexName(IndexName, out var indexName) || indexName != IndexName)
        {
            yield return new ValidationResult(S["The index name contains forbidden characters."], [nameof(IndexName)]);
        }
    }
}
