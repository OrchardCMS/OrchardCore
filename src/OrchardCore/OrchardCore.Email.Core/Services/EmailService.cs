using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure;

namespace OrchardCore.Email.Services;

public class EmailService : IEmailService
{
    private readonly IEmailProviderFactory _factory;
    private readonly DefaultProviderOptions _defaultProvider;

    public EmailService(IEmailProviderFactory factory, IOptions<DefaultProviderOptions> defaultProvider)
    {
        _factory = factory;
        _defaultProvider = defaultProvider.Value;
    }

    public async Task<Result> SendAsync(MailMessage message, string providerName = null)
    {
        providerName ??= _defaultProvider.ProviderName;

        var provider = _factory.GetProvider(providerName);

        return await provider.SendAsync(message);
    }
}
