using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Email.Services;

public class EmailProviderFactory : IEmailProviderFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EmailProviderFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public IEmailProvider GetProvider(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        using var scope = _serviceScopeFactory.CreateScope();
        var providers = scope.ServiceProvider.GetServices<IEmailProvider>();

        var provider = providers.FirstOrDefault(p =>
            string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        return provider ?? throw new InvalidOperationException($"Email provider '{name}' not found.");
    }
}

