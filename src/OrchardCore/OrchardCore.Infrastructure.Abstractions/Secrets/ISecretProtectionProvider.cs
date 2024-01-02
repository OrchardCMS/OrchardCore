namespace OrchardCore.Secrets;

public interface ISecretProtectionProvider
{
    ISecretProtector CreateProtector(string purpose = null);
}
