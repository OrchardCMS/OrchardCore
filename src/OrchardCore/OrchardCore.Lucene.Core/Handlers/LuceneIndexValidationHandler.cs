using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Lucene.Models;
using OrchardCore.Lucene.Services;

namespace OrchardCore.Lucene.Core.Handlers;

/// <summary>
/// Validates Lucene definitions shared by administration, recipes and remote management.
/// </summary>
public sealed class LuceneIndexValidationHandler : IndexProfileHandlerBase
{
    private readonly LuceneAnalyzerManager _analyzers;
    private readonly IStringLocalizer S;

    /// <summary>Creates validation using the tenant's registered analyzers.</summary>
    public LuceneIndexValidationHandler(LuceneAnalyzerManager analyzers, IStringLocalizer<LuceneIndexValidationHandler> localizer)
    {
        _analyzers = analyzers;
        S = localizer;
    }

    /// <inheritdoc />
    public override Task ValidatingAsync(ValidatingContext<IndexProfile> context)
    {
        var profile = context.Model;
        if (!string.Equals(profile.ProviderName, LuceneConstants.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (!LuceneIndexNameValidator.IsValid(profile.IndexName) || !LuceneIndexNameValidator.IsValid(profile.IndexFullName))
        {
            context.Result.Fail(new ValidationResult(S["The Lucene index name must be a single file name without path separators or reserved filename characters."], [nameof(IndexProfile.IndexName)]));
        }

        var metadata = profile.GetOrCreate<LuceneIndexMetadata>();
        var query = profile.GetOrCreate<LuceneIndexDefaultQueryMetadata>();
        ValidateAnalyzer(metadata.AnalyzerName, nameof(metadata.AnalyzerName), context);
        ValidateAnalyzer(query.QueryAnalyzerName, nameof(query.QueryAnalyzerName), context);
        if (!Enum.IsDefined(query.DefaultVersion))
        {
            context.Result.Fail(new ValidationResult(S["The Lucene version is not supported."], [nameof(query.DefaultVersion)]));
        }

        return Task.CompletedTask;
    }

    private void ValidateAnalyzer(string name, string member, ValidatingContext<IndexProfile> context)
    {
        var effectiveName = string.IsNullOrEmpty(name) ? LuceneConstants.DefaultAnalyzer : name;
        if (!_analyzers.GetAnalyzers().Any(analyzer => string.Equals(analyzer.Name, effectiveName, StringComparison.OrdinalIgnoreCase)))
        {
            context.Result.Fail(new ValidationResult(S["The Lucene analyzer '{0}' is not registered.", effectiveName], [member]));
        }
    }

}
