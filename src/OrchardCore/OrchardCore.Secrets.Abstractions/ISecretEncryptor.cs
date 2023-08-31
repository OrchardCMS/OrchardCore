namespace OrchardCore.Secrets;

public interface ISecretEncryptor
{
    string Encrypt(string plainText);
}
