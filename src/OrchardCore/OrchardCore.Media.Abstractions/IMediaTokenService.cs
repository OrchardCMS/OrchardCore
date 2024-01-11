using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OrchardCore.Media
{
    /// <summary>
    /// The media token service adds a digital signature to a query string.
    /// This allows the media processor to validate that commands were issued by this host.
    /// </summary>
    public interface IMediaTokenService
    {
        string AddTokenToPath(string path);
        bool TryValidateToken(KeyedCollection<string, KeyValuePair<string, string>> commands, string token);
    }
}
