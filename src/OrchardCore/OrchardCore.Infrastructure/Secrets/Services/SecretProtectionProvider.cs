namespace OrchardCore.Secrets.Services;

public class SecretProtectionProvider : ISecretProtectionProvider
{
    private readonly ISecretService _secretService;

    public SecretProtectionProvider(ISecretService secretService) => _secretService = secretService;

    public ISecretProtector CreateProtector(string purpose = null) => new SecretHybridProtector(_secretService, purpose);
}
