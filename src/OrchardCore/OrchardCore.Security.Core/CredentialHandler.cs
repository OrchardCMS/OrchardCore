using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Catalogs;
using OrchardCore.Catalogs.Models;
using OrchardCore.Modules;

namespace OrchardCore.Security.Core;

public sealed class CredentialHandler : CatalogEntryHandlerBase<Credential>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SecurityOptions _securityOptions;
    private readonly INamedCatalog<Credential> _catalog;
    private readonly IClock _clock;

    internal readonly IStringLocalizer S;

    public CredentialHandler(
        IHttpContextAccessor httpContextAccessor,
        IOptions<SecurityOptions> securityOptions,
        INamedCatalog<Credential> catalog,
        IClock clock,
        IStringLocalizer<CredentialHandler> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _securityOptions = securityOptions.Value;
        _catalog = catalog;
        _clock = clock;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingContext<Credential> context)
        => PopulateAsync(context.Model, context.Data, true);

    public override Task UpdatingAsync(UpdatingContext<Credential> context)
        => PopulateAsync(context.Model, context.Data, false);

    public override async Task ValidatingAsync(ValidatingContext<Credential> context)
    {
        if (string.IsNullOrWhiteSpace(context.Model.Name))
        {
            context.Result.Fail(new ValidationResult(S["Credential Name is required."], [nameof(Credential.Name)]));
        }
        else
        {
            var connection = await _catalog.FindByNameAsync(context.Model.Name);

            if (connection is not null && connection.ItemId != context.Model.ItemId)
            {
                context.Result.Fail(new ValidationResult(S["A connection with this name already exists. The name must be unique."], [nameof(Credential.Name)]));
            }
        }

        if (string.IsNullOrWhiteSpace(context.Model.Source))
        {
            context.Result.Fail(new ValidationResult(S["Source is required."], [nameof(Credential.Source)]));
        }
        else if (!_securityOptions.CredentialProviders.TryGetValue(context.Model.Source, out _))
        {
            context.Result.Fail(new ValidationResult(S["Invalid source."], [nameof(Credential.Source)]));
        }
    }

    public override Task InitializedAsync(InitializedContext<Credential> context)
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

    private static Task PopulateAsync(Credential credential, JsonNode data, bool isNew)
    {
        if (isNew)
        {
            var name = data[nameof(Credential.Name)]?.GetValue<string>()?.Trim();

            if (!string.IsNullOrEmpty(name))
            {
                credential.Name = name;
            }
        }

        var displayText = data[nameof(Credential.DisplayText)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(displayText))
        {
            credential.DisplayText = displayText;
        }

        var source = data[nameof(Credential.Source)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(source))
        {
            credential.Source = source;
        }

        var ownerId = data[nameof(Credential.OwnerId)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(ownerId))
        {
            credential.OwnerId = ownerId;
        }

        var author = data[nameof(Credential.Author)]?.GetValue<string>()?.Trim();

        if (!string.IsNullOrEmpty(author))
        {
            credential.Author = author;
        }

        var createdUtc = data[nameof(Credential.CreatedUtc)]?.GetValue<DateTime?>();

        if (createdUtc.HasValue)
        {
            credential.CreatedUtc = createdUtc.Value;
        }

        var properties = data[nameof(Credential.Properties)]?.AsObject();

        if (properties != null)
        {
            credential.Properties ??= [];
            credential.Properties.Merge(properties);
        }

        return Task.CompletedTask;
    }
}
