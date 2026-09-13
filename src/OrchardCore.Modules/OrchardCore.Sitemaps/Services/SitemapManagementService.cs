using System.Buffers;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

internal sealed class SitemapManagementService
{
    private static readonly SearchValues<char> s_invalidNameCharacters = SearchValues.Create("/\\<>:\"|?*");
    private readonly ISitemapManager _manager;
    private readonly ISitemapHelperService _helper;
    private readonly ISitemapIdGenerator _ids;

    public SitemapManagementService(ISitemapManager manager, ISitemapHelperService helper, ISitemapIdGenerator ids)
    {
        _manager = manager;
        _helper = helper;
        _ids = ids;
    }

    public async Task<SitemapMutationResult> SaveAsync(SitemapDefinition input, string id = null)
    {
        var errors = new Dictionary<string, string[]>();
        if (input is null || string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 128 || input.Name.Any(char.IsControl) || input.Name.AsSpan().ContainsAny(s_invalidNameCharacters))
        {
            return new() { Errors = new() { ["name"] = ["A name of at most 128 characters without directory separators is required."] } };
        }
        if (input.Kind is not ("Sitemap" or "SitemapIndex")) { errors["kind"] = ["Select Sitemap or SitemapIndex."]; }
        var existing = id is null ? null : await _manager.LoadSitemapAsync(id);
        if (id is not null && existing is null) { return new() { NotFound = true }; }
        if (existing is not null && existing.GetType().Name != input.Kind) { errors["kind"] = ["An existing sitemap's kind cannot change."]; }
        var path = string.IsNullOrWhiteSpace(input.Path) ? _helper.GetSitemapSlug(input.Name) : input.Path;
        var validation = new ValidationModel();
        await _helper.ValidatePathAsync(path, validation, id);
        foreach (var entry in validation.ModelState.Where(entry => entry.Value.Errors.Count > 0))
        {
            errors[entry.Key] = entry.Value.Errors.Select(error => error.ErrorMessage).ToArray();
        }
        var contained = input.ContainedSitemapIds ?? [];
        if (input.Kind == "Sitemap" && contained.Length > 0) { errors["containedSitemapIds"] = ["Only sitemap indexes can contain other sitemaps."]; }
        if (input.Kind == "SitemapIndex")
        {
            var available = (await _manager.GetSitemapsAsync()).OfType<Sitemap>().Select(sitemap => sitemap.SitemapId).ToHashSet(StringComparer.Ordinal);
            if (contained.Any(value => value is null || !available.Contains(value)) || contained.Distinct(StringComparer.Ordinal).Count() != contained.Length)
            {
                errors["containedSitemapIds"] = ["Select distinct existing regular sitemaps."];
            }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }
        SitemapType sitemap = existing ?? (input.Kind == "SitemapIndex" ? new SitemapIndex() : new Sitemap());
        var changed = SitemapMutations.Apply(sitemap, input.Name, path, input.Enabled);
        if (existing is null) { sitemap.SitemapId = _ids.GenerateUniqueId(); changed = true; }
        if (sitemap is SitemapIndex)
        {
            var source = sitemap.SitemapSources.OfType<SitemapIndexSource>().SingleOrDefault();
            if (source is null)
            {
                source = new SitemapIndexSource { Id = _ids.GenerateUniqueId() };
                sitemap.SitemapSources.Add(source);
                changed = true;
            }
            if (!source.ContainedSitemapIds.SequenceEqual(contained)) { source.ContainedSitemapIds = contained.ToArray(); changed = true; }
        }
        if (changed) { await _manager.UpdateSitemapAsync(sitemap); }
        return new() { Sitemap = sitemap, Changed = changed };
    }

    private sealed class ValidationModel : IUpdateModel
    {
        public ModelStateDictionary ModelState { get; } = new();
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class => Task.FromResult(false);
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class => Task.FromResult(false);
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class => Task.FromResult(false);
        public bool TryValidateModel(object model) => true;
        public bool TryValidateModel(object model, string prefix) => true;
    }
}

internal sealed class SitemapMutationResult
{
    public SitemapType Sitemap { get; init; }
    public bool Changed { get; init; }
    public bool NotFound { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}

/// <summary>A complete sitemap or sitemap-index definition. Source definitions are managed separately.</summary>
public sealed class SitemapDefinition
{
    /// <summary>Gets or sets the administrative name.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the relative .xml path; omission derives it from the name.</summary>
    public string Path { get; set; }
    /// <summary>Gets or sets Sitemap or SitemapIndex.</summary>
    public string Kind { get; set; } = "Sitemap";
    /// <summary>Gets or sets whether public routing is enabled.</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Gets or sets the distinct regular sitemap IDs included by an index.</summary>
    public string[] ContainedSitemapIds { get; set; } = [];
}
