using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure;

namespace OrchardCore.Email.Services;

public class EmailService : IEmailService
{
    private readonly IEmailProviderFactory _factory;
    private readonly EmailOptions _emailOptions;

    public EmailService(IEmailProviderFactory factory, IOptions<EmailOptions> emailOptions)
    {
        _factory = factory;
        _emailOptions = emailOptions.Value;
    }

    public async Task<Result> SendAsync(MailMessage message, string providerName = null)
    {
        providerName ??= _emailOptions.DefaultProviderName;

        var provider = _factory.GetProvider(providerName);

        return await provider.SendAsync(message);
    }
}
