namespace OrchardCore.Email.Services;

public interface IEmailProviderFactory
{
    IEmailProvider GetProvider(string name);
}
