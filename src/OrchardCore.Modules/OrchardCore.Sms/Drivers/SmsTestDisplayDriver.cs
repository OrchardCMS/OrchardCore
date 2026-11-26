using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Drivers;

/// <summary>
/// Builds the editor of the test SMS page, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class SmsTestDisplayDriver : DisplayDriver<SmsTestViewModel>
{
    internal readonly IStringLocalizer S;

    public SmsTestDisplayDriver(IStringLocalizer<SmsTestDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(SmsTestViewModel model, BuildEditorContext context)
    {
        return Initialize<SmsTestViewModel>("SmsTestGeneral_Edit", m => Populate(model, m))
            .Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(SmsTestViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Provider, m => m.PhoneNumber);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(SmsTestViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(SmsTestViewModel source, SmsTestViewModel model)
    {
        model.Provider = source.Provider;
        model.PhoneNumber = source.PhoneNumber;
        model.Providers = source.Providers;
    }
}
