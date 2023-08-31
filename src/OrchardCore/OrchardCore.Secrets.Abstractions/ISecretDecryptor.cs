namespace OrchardCore.Secrets;

public interface ISecretDecryptor
{
    string Decrypt(string protectedData);
}
