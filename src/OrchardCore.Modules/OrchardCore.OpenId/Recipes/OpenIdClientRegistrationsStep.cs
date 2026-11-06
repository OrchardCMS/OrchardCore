using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

/// <summary>
/// This recipe step adds or updates the additional OpenID Connect client registrations.
/// </summary>
public sealed class OpenIdClientRegistrationsStep : NamedRecipeStepHandler
{
    public const string RecipeStepName = "OpenIdClientRegistrations";

    private readonly IOpenIdClientRegistrationService _registrationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;

    public OpenIdClientRegistrationsStep(
        IOpenIdClientRegistrationService registrationService,
        IDataProtectionProvider dataProtectionProvider,
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<OpenIdClientRegistrationsStep> stringLocalizer)
        : base(RecipeStepName)
    {
        _registrationService = registrationService;
        _dataProtectionProvider = dataProtectionProvider;
        _shellReleaseManager = shellReleaseManager;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<OpenIdClientRegistrationsStepModel>();

        if (model?.Registrations is null)
        {
            return;
        }

        foreach (var item in model.Registrations)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                context.Errors.Add(S["A client registration of the {0} step has no name.", RecipeStepName]);

                continue;
            }

            // A new instance is built, so an invalid registration doesn't change the stored one. All the
            // settings come from the step, except the protected client secret when the step doesn't set one.
            var existing = await _registrationService.GetRegistrationAsync(item.Name);
            var registration = new OpenIdClientRegistration
            {
                Name = existing?.Name ?? item.Name,
                ClientSecret = existing?.ClientSecret,
            };

            item.ApplyTo(registration, _dataProtectionProvider);

            var errors = (await _registrationService.ValidateRegistrationAsync(registration, existing?.Name))
                .Where(result => result != ValidationResult.Success)
                .Select(result => result.ErrorMessage)
                .ToArray();

            if (errors.Length > 0)
            {
                context.Errors.Add(S["The client registration '{0}' is invalid: {1}", item.Name, string.Join(" ", errors)]);

                continue;
            }

            await _registrationService.UpdateRegistrationAsync(registration);
        }

        // The client registrations are read when the tenant builds its options.
        _shellReleaseManager.RequestRelease();
    }
}

public sealed class OpenIdClientRegistrationsStepModel
{
    public OpenIdClientRegistrationStepModel[] Registrations { get; set; }
}

public sealed class OpenIdClientRegistrationStepModel : OpenIdClientSettingsStepModel
{
    /// <summary>
    /// Gets or sets the unique name of the registration, also its authentication scheme.
    /// </summary>
    public string Name { get; set; }
}
