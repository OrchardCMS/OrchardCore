using System.Collections.Generic;

namespace OrchardCore.Media
{
    /// <summary>
    /// The media token service adds a digital signature to a query string.
    /// This allows the media processor to validate that commands were issued by this host.
    /// </summary>
    public interface IMediaTokenService
    {
        string AddTokenToPath(string path);
        bool TryValidateToken(IDictionary<string, string> commands, string token);
    }
}
