using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core.Handlers;

internal class IndexEntityHandler : ModelHandlerBase<IndexEntity>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IndexingOptions _indexingOptions;
    private readonly IClock _clock;

    internal readonly IStringLocalizer S;

    public IndexEntityHandler(
        IHttpContextAccessor httpContextAccessor,
        IOptions<IndexingOptions> indexingOptions,
        IClock clock,
        IStringLocalizer<IndexEntityHandler> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _indexingOptions = indexingOptions.Value;
        _clock = clock;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<IndexEntity> context)
        => PopulateAsync(context.Model, context.Data);
    public override Task UpdatingAsync(UpdatingContext<IndexEntity> context)
    => PopulateAsync(context.Model, context.Data);

    public override Task ValidatingAsync(ValidatingContext<IndexEntity> context)
    {
        if (string.IsNullOrWhiteSpace(context.Model.DisplayText))
        {
            context.Result.Fail(new ValidationResult(S["Index display-text is required."], [nameof(IndexEntity.DisplayText)]));
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

    private static Task PopulateAsync(IndexEntity dataSource, JsonNode data)
    {
        var displayText = data[nameof(IndexEntity.DisplayText)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(displayText))
        {
            dataSource.DisplayText = displayText;
        }

        var providerName = data[nameof(IndexEntity.ProviderName)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(providerName))
        {
            dataSource.ProviderName = providerName;
        }

        var type = data[nameof(IndexEntity.Type)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(type))
        {
            dataSource.Type = type;
        }

        var properties = data[nameof(IndexEntity.Properties)]?.AsObject();

        if (properties != null)
        {
            dataSource.Properties = properties.Clone();
        }

        return Task.CompletedTask;
    }
}

