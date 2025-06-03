using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core.Handlers;

internal sealed class DefaultIndexEntityHandler : IndexEntityHandlerBase
{
    private readonly IIndexEntityStore _indexEntityStore;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly IndexingOptions _indexingOptions;
    private readonly IClock _clock;

    internal readonly IStringLocalizer S;

    public DefaultIndexEntityHandler(
        IIndexEntityStore indexEntityStore,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        IOptions<IndexingOptions> indexingOptions,
        IClock clock,
        IStringLocalizer<DefaultIndexEntityHandler> stringLocalizer)
    {
        _indexEntityStore = indexEntityStore;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _indexingOptions = indexingOptions.Value;
        _clock = clock;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
        => PopulateAsync(context.Model, context.Data);

    public override Task UpdatingAsync(UpdatingContext<IndexEntity> context)
        => PopulateAsync(context.Model, context.Data);

    public override async Task ValidatingAsync(ValidatingContext<IndexEntity> context)
    {
        if (string.IsNullOrWhiteSpace(context.Model.Name))
        {
            context.Result.Fail(new ValidationResult(S["Index name is required."], [nameof(IndexEntity.Name)]));
        }

        var hasIndexName = !string.IsNullOrWhiteSpace(context.Model.IndexName);

        if (!hasIndexName)
        {
            context.Result.Fail(new ValidationResult(S["The index name is required."]));
        }
        else
        {
            var existing = await _indexEntityStore.FindByNameAsync(context.Model.Name);

            if (existing is not null && existing.Id != context.Model.Id)
            {
                context.Result.Fail(new ValidationResult(S["There is already another index with the same name."], [nameof(IndexEntity.Name)]));
            }
        }

        if (string.IsNullOrWhiteSpace(context.Model.IndexFullName))
        {
            context.Result.Fail(new ValidationResult(S["The index full name is required."]));
        }

        var hasProviderName = !string.IsNullOrWhiteSpace(context.Model.ProviderName);
        var hasType = !string.IsNullOrWhiteSpace(context.Model.Type);

        if (!hasProviderName)
        {
            context.Result.Fail(new ValidationResult(S["Index profile-source is required."], [nameof(IndexEntity.ProviderName)]));
        }

        if (!hasType)
        {
            context.Result.Fail(new ValidationResult(S["Index type is required."], [nameof(IndexEntity.Type)]));
        }

        if (hasProviderName && hasType && !_indexingOptions.Sources.TryGetValue(new IndexEntityKey(context.Model.ProviderName, context.Model.Type), out _))
        {
            context.Result.Fail(new ValidationResult(S["Unable to find a provider named '{0}' with the type '{1}'.", context.Model.ProviderName, context.Model.Type], [nameof(IndexEntity.Type)]));
        }

        if (string.IsNullOrEmpty(context.Model.IndexFullName))
        {
            if (!hasProviderName || !hasIndexName)
            {
                context.Result.Fail(new ValidationResult(S["The index full name is required."]));
            }
            else
            {
                var nameProvider = _serviceProvider.GetKeyedService<IIndexNameProvider>(context.Model.ProviderName);

                if (nameProvider is null)
                {
                    context.Result.Fail(new ValidationResult(S["The index full name is required. Unable to find a index name provider that with the provider name '{0}'.", context.Model.ProviderName, context.Model.Type], [nameof(IndexEntity.Type)]));
                }
                else
                {
                    // Set the full name of the index.
                    context.Model.IndexFullName = nameProvider.GetFullIndexName(context.Model.IndexName);
                }
            }
        }
    }

    public override Task CreatingAsync(CreatingContext<IndexEntity> context)
    {
        SetIndexFullName(context.Model);

        if (string.IsNullOrEmpty(context.Model.IndexName) || string.IsNullOrEmpty(context.Model.IndexFullName))
        {
            throw new InvalidOperationException("Both the index-name and index full-name must be set.");
        }

        return Task.CompletedTask;
    }

    public override Task InitializedAsync(InitializedContext<IndexEntity> context)
    {
        context.Model.CreatedUtc = _clock.UtcNow;
        var user = _httpContextAccessor.HttpContext?.User;

        if (user != null)
        {
            context.Model.OwnerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            context.Model.Author = user.Identity.Name;
        }

        return Task.CompletedTask;
    }

    private Task PopulateAsync(IndexEntity index, JsonNode data)
    {
        var name = data[nameof(index.Name)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(name))
        {
            index.Name = name;
        }

        var indexName = data[nameof(index.IndexName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(indexName))
        {
            index.IndexName = indexName;
        }

        var providerName = data[nameof(index.ProviderName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(providerName))
        {
            index.ProviderName = providerName;
        }

        var type = data[nameof(index.Type)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(type))
        {
            index.Type = type;
        }

        var properties = data[nameof(index.Properties)]?.AsObject();

        if (properties != null)
        {
            index.Properties = properties.Clone();
        }

        if (string.IsNullOrWhiteSpace(index.Name))
        {
            index.Name = index.IndexName;
        }

        SetIndexFullName(index);

        return Task.CompletedTask;
    }

    private void SetIndexFullName(IndexEntity index)
    {
        if (!string.IsNullOrEmpty(index.IndexFullName))
        {
            return;
        }

        var nameProvider = _serviceProvider.GetKeyedService<IIndexNameProvider>(index.ProviderName);

        if (nameProvider is not null)
        {
            // Set the full name of the index.
            index.IndexFullName = nameProvider.GetFullIndexName(index.IndexName);
        }
    }
}

