using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

/// <summary>
/// Builds the editor of the new password of a user, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class EditPasswordDisplayDriver : DisplayDriver<EditPasswordViewModel>
{
    internal readonly IStringLocalizer S;

    public EditPasswordDisplayDriver(IStringLocalizer<EditPasswordDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditPasswordViewModel model, BuildEditorContext context)
    {
        return Initialize<EditPasswordViewModel>("EditPasswordFields_Edit", m =>
        {
            m.UsernameOrEmail = model.UsernameOrEmail;
            m.CurrentPassword = model.CurrentPassword;
            m.NewPassword = model.NewPassword;
            m.PasswordConfirmation = model.PasswordConfirmation;
        }).Location(l => l.Zone("Content", "1").Card(S["Password"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditPasswordViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.CurrentPassword, m => m.NewPassword, m => m.PasswordConfirmation);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(EditPasswordViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
