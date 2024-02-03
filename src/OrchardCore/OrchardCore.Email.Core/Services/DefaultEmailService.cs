using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email.Core.Services;

public class DefaultEmailService : IEmailService
{
    private readonly IEmailProviderResolver _providerResolver;
    private IEmailProvider _provider;

    protected readonly IStringLocalizer S;

    public DefaultEmailService(
        IEmailProviderResolver providerResolver,
        IStringLocalizer<DefaultEmailService> stringLocalizer)
    {
        _providerResolver = providerResolver;
        S = stringLocalizer;
    }

    public async Task<EmailResult> SendAsync(MailMessage message)
    {
        _provider ??= await _providerResolver.GetAsync();

        if (_provider is null)
        {
            return EmailResult.Failed(S["SMS settings must be configured before an SMS message can be sent."]);
        }

        return await _provider.SendAsync(message);
    }
}
