using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers;

public sealed class TextSecretDisplayDriver : DisplayDriver<ISecret, TextSecret>
{
    internal readonly IStringLocalizer S;

    public TextSecretDisplayDriver(IStringLocalizer<TextSecretDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(TextSecret secret, BuildDisplayContext context)
    {
        return CombineAsync(
            View("TextSecret_Fields_Summary", secret).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("TextSecret_Fields_Thumbnail", secret).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(TextSecret secret, BuildEditorContext context)
    {
        return Initialize<TextSecretViewModel>("TextSecret_Fields_Edit", model =>
        {
            // The text value is never returned to the view for security.
            model.Text = string.Empty;
            model.IsNew = context.IsNew;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(TextSecret secret, UpdateEditorContext context)
    {
        var model = new TextSecretViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (context.IsNew && string.IsNullOrEmpty(model.Text))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Text), S["The secret value is required."]);
        }

        // The text value is only updated when a new value has been provided.
        if (!string.IsNullOrEmpty(model.Text))
        {
            secret.Text = model.Text;
        }

        return Edit(secret, context);
    }
}
