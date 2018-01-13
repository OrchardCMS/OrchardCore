namespace OrchardCore.Workflows.Services
{
    public interface ISignalService
    {
        /// <summary>
        /// Creates a SAS (Shared Access Signature) token containing the specified correlation ID and signal name.
        /// </summary>
        string CreateToken(string correlationId, string signal);

        /// <summary>
        /// Decrypts the specified SAS token.
        /// </summary>
        bool DecryptToken(string token, out string correlationId, out string signal);
    }
}