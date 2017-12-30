namespace OrchardCore.Workflows.Services
{
    public interface ISignalService
    {
        string CreateNonce(string correlationId, string signal);
        bool DecryptNonce(string nonce, out string correlationId, out string signal);
    }
}