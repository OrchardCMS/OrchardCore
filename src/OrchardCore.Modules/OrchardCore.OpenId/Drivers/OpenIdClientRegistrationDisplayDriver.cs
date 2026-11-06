using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers;

/// <summary>
/// Builds the row of the <c>OpenIdClientRegistrations</c> admin list and the editor of an additional OpenID Connect
/// client registration, which reuses the editor of the client settings of the site.
/// </summary>
public sealed class OpenIdClientRegistrationDisplayDriver : DisplayDriver<OpenIdClientRegistration>
{
    private readonly IOpenIdClientRegistrationService _registrationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    public OpenIdClientRegistrationDisplayDriver(
        IOpenIdClientRegistrationService registrationService,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        IStringLocalizer<OpenIdClientRegistrationDisplayDriver> stringLocalizer)
    {
        _registrationService = registrationService;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(OpenIdClientRegistration registration, BuildDisplayContext context)
    {
        return CombineAsync(
            View("OpenIdClientRegistration_Fields_SummaryAdmin", registration)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "DisplayName:1"),
            View("OpenIdClientRegistration_DefaultTags_SummaryAdmin", registration)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5"),
            View("OpenIdClientRegistration_DefaultMeta_SummaryAdmin", registration)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Authority:5"),
            View("OpenIdClientRegistration_Buttons_SummaryAdmin", registration)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }

    public override IDisplayResult Edit(OpenIdClientRegistration registration, BuildEditorContext context)
    {
        return Combine(
            Initialize<OpenIdClientRegistrationNameViewModel>("OpenIdClientRegistrationName_Edit", model =>
            {
                model.Name = registration.Name;
                model.IsNew = context.IsNew;
            }).Location("Content:1"),
            Initialize<OpenIdClientSettingsViewModel>("OpenIdClientSettings_Edit", model =>
            {
                OpenIdClientSettingsEditor.Populate(model, registration, _serviceProvider);
            }).Location("Content:2")
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(OpenIdClientRegistration registration, UpdateEditorContext context)
    {
        // The name is the authentication scheme of the registration and the login provider of the linked
        // user accounts, so it can only be set when the registration is created.
        string originalName = null;

        if (context.IsNew)
        {
            var nameModel = new OpenIdClientRegistrationNameViewModel();
            await context.Updater.TryUpdateModelAsync(nameModel, Prefix);
            registration.Name = nameModel.Name?.Trim();
        }
        else
        {
            originalName = registration.Name;
        }

        var model = new OpenIdClientSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        await OpenIdClientSettingsEditor.UpdateAsync(
            model,
            registration,
            context.Updater.ModelState,
            Prefix,
            _serviceProvider,
            _dataProtectionProvider,
            S);

        OpenIdClientSettingsEditor.AddErrors(
            await _registrationService.ValidateRegistrationAsync(registration, originalName),
            context.Updater.ModelState,
            Prefix);

        return Edit(registration, context);
    }
}
