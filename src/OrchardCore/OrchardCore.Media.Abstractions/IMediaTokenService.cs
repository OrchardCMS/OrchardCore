using System.Collections.Generic;

namespace OrchardCore.Media
{
    /// <summary>
    /// The media token service encrypts and decrypts a query string with image processing commands.
    /// </summary>
    public interface IMediaTokenService
    {
        string TokenizePath(string path);
        IDictionary<string, string> GetTokenizedCommands(string token);
    }
}
