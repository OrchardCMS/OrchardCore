using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenIddict.Client.AspNetCore;
using OrchardCore.Documents;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Services;

public sealed partial class OpenIdClientRegistrationService : IOpenIdClientRegistrationService
{
    private readonly IDocumentManager<OpenIdClientRegistrationsDocument> _documentManager;
    private readonly IOpenIdClientService _clientService;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    public OpenIdClientRegistrationService(
        IDocumentManager<OpenIdClientRegistrationsDocument> documentManager,
        IOpenIdClientService clientService,
        IServiceProvider serviceProvider,
        IStringLocalizer<OpenIdClientRegistrationService> stringLocalizer)
    {
        _documentManager = documentManager;
        _clientService = clientService;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public async Task<IReadOnlyCollection<OpenIdClientRegistration>> GetRegistrationsAsync()
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Registrations.Values;
    }

    public async Task<OpenIdClientRegistration> GetRegistrationAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Registrations.GetValueOrDefault(name);
    }

    public async Task<OpenIdClientRegistration> LoadRegistrationAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateMutableAsync();

        return document.Registrations.GetValueOrDefault(name);
    }

    public async Task UpdateRegistrationAsync(OpenIdClientRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentException.ThrowIfNullOrEmpty(registration.Name);

        var document = await _documentManager.GetOrCreateMutableAsync();
        document.Registrations[registration.Name] = registration;

        await _documentManager.UpdateAsync(document);
    }

    public async Task<bool> DeleteRegistrationAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateMutableAsync();

        if (!document.Registrations.Remove(name))
        {
            return false;
        }

        await _documentManager.UpdateAsync(document);

        return true;
    }

    public async Task<ImmutableArray<ValidationResult>> ValidateRegistrationAsync(OpenIdClientRegistration registration, string originalName = null)
    {
        ArgumentNullException.ThrowIfNull(registration);

        var results = ImmutableArray.CreateBuilder<ValidationResult>();

        if (string.IsNullOrWhiteSpace(registration.Name))
        {
            results.Add(new ValidationResult(S["The name is required."], [nameof(registration.Name)]));
        }
        else if (!NameRegex().IsMatch(registration.Name))
        {
            results.Add(new ValidationResult(
                S["The name must start with a letter and contain only letters, digits, dashes and underscores."],
                [nameof(registration.Name)]));
        }
        else if (!string.Equals(registration.Name, originalName, StringComparison.OrdinalIgnoreCase))
        {
            // The name is the authentication scheme of the registration, so it can't be the scheme of another
            // registration, of the registration of the site settings or of another authentication handler.
            if (string.Equals(registration.Name, OpenIdConstants.Client.DefaultProviderName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(registration.Name, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase) ||
                await GetRegistrationAsync(registration.Name) is not null ||
                (await GetSchemesAsync()).Any(scheme => string.Equals(scheme.Name, registration.Name, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(new ValidationResult(S["The name is already used by another authentication provider."], [nameof(registration.Name)]));
            }
        }

        if (string.IsNullOrWhiteSpace(registration.DisplayName))
        {
            results.Add(new ValidationResult(S["The display name is required."], [nameof(registration.DisplayName)]));
        }

        if (string.IsNullOrWhiteSpace(registration.ClientId))
        {
            results.Add(new ValidationResult(S["The client identifier is required."], [nameof(registration.ClientId)]));
        }

        results.AddRange(await _clientService.ValidateSettingsAsync(registration));

        return results.ToImmutable();
    }

    // Note: the scheme provider is resolved when a registration is validated, as it depends on the authentication
    // options, which depend on the client registrations and so on this service.
    private Task<IEnumerable<AuthenticationScheme>> GetSchemesAsync()
        => _serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>().GetAllSchemesAsync();

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_-]*$")]
    private static partial Regex NameRegex();
}
