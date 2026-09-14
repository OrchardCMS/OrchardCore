using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Email;

/// <summary>
/// The name of the breadcrumb rendered by the email screen.
/// </summary>
public static class EmailConstants
{
    /// <summary>
    /// The breadcrumb of the email screen.
    /// </summary>
    public const string Settings = "EmailSettings";
}

/// <summary>
/// Describes the breadcrumb trail of the email screen.
/// </summary>
public sealed class EmailBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public EmailBreadcrumbProvider(IStringLocalizer<EmailBreadcrumbProvider> stringLocalizer)
        : base(EmailConstants.Settings)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Email"], item => item.Id("Email"));

        return ValueTask.CompletedTask;
    }
}
