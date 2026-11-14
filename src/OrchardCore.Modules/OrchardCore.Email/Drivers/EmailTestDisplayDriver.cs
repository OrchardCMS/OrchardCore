using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.ViewModels;

namespace OrchardCore.Email.Drivers;

/// <summary>
/// Builds the editor of the test email page, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class EmailTestDisplayDriver : DisplayDriver<EmailTestViewModel>
{
    internal readonly IStringLocalizer S;

    public EmailTestDisplayDriver(IStringLocalizer<EmailTestDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EmailTestViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<EmailTestViewModel>("EmailTestProvider_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["Provider"], "10")),
            Initialize<EmailTestViewModel>("EmailTestAddresses_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Addresses"], "20")),
            Initialize<EmailTestViewModel>("EmailTestMessage_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "3").Card(S["Message"], "30")));
    }

    public override async Task<IDisplayResult> UpdateAsync(EmailTestViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Provider,
            m => m.To,
            m => m.From,
            m => m.Cc,
            m => m.Bcc,
            m => m.ReplyTo,
            m => m.Subject,
            m => m.Body);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(EmailTestViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(EmailTestViewModel source, EmailTestViewModel model)
    {
        model.Provider = source.Provider;
        model.To = source.To;
        model.From = source.From;
        model.Cc = source.Cc;
        model.Bcc = source.Bcc;
        model.ReplyTo = source.ReplyTo;
        model.Subject = source.Subject;
        model.Body = source.Body;
        model.Providers = source.Providers;
    }
}
