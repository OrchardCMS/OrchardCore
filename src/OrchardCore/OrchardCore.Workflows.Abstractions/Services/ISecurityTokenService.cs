namespace OrchardCore.Workflows.Services
{
    public interface ISecurityTokenService
    {
        /// <summary>
        /// Creates a SAS (Shared Access Signature) token containing the specified data.
        /// </summary>
        string CreateToken<T>(T payload);

        /// <summary>
        /// Decrypts the specified SAS token.
        /// </summary>
        Result<T> DecryptToken<T>(string token);
    }
}