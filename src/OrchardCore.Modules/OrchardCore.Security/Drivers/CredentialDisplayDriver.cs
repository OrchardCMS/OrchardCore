using Microsoft.Extensions.Localization;
using OrchardCore.Catalogs;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Security.Core;
using OrchardCore.Security.ViewModels;

namespace CrestApps.OrchardCore.AI.Drivers;

internal sealed class CredentialDisplayDriver : DisplayDriver<Credential>
{
    private readonly INamedCatalog<Credential> _catalog;

    internal readonly IStringLocalizer S;

    public CredentialDisplayDriver(
        INamedCatalog<Credential> catalog,
        IStringLocalizer<CredentialDisplayDriver> stringLocalizer)
    {
        _catalog = catalog;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(Credential credential, BuildDisplayContext context)
    {
        return CombineAsync(
            View("Credential_Fields_SummaryAdmin", credential).Location("Content:1"),
            View("Credential_Buttons_SummaryAdmin", credential).Location("Actions:5"),
            View("Credential_DefaultTags_SummaryAdmin", credential).Location("Tags:5"),
            View("Credential_DefaultMeta_SummaryAdmin", credential).Location("Meta:5")
        );
    }

    public override IDisplayResult Edit(Credential credential, BuildEditorContext context)
    {
        return Initialize<CredentialFieldsViewModel>("CredentialFields_Edit", model =>
        {
            model.DisplayText = credential.DisplayText;
            model.Name = credential.Name;
            model.IsNew = context.IsNew;
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(Credential credential, UpdateEditorContext context)
    {
        var model = new CredentialFieldsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.IsNew)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["Name is required."]);
            }
            else if (await _catalog.FindByNameAsync(model.Name) is not null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["Another credential with the same name exists."]);
            }

            credential.Name = model.Name;
        }

        if (string.IsNullOrWhiteSpace(model.DisplayText))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.DisplayText), S["The Display text is required."]);
        }

        credential.DisplayText = model.DisplayText;

        return Edit(credential, context);
    }
}
