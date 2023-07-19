using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace OrchardCore.Scripting
{
    public class CommonGeneratorMethods : IGlobalMethodProvider
    {
        private static readonly GlobalMethod _base64 = new()
        {
            Name = "base64",
            Method = serviceProvider => (Func<string, string>)(encoded =>
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            }),
        };

        private static readonly GlobalMethod _html = new()
        {
            Name = "html",
            Method = serviceProvider => (Func<string, string>)(encoded =>
            {
                return WebUtility.HtmlDecode(encoded);
            }),
        };

        /// <summary>
        /// Converts a Base64 encoded gzip stream to an uncompressed Base64 string.
        /// See http://www.txtwizard.net/compression
        /// </summary>
        private static readonly GlobalMethod _gZip = new()
        {
            Name = "gzip",
            Method = serviceProvider => (Func<string, string>)(encoded =>
            {
                var bytes = Convert.FromBase64String(encoded);
                using var gzip = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);

                var decompressed = new MemoryStream();
                var buffer = new byte[1024];
                int nRead;

                while ((nRead = gzip.Read(buffer, 0, buffer.Length)) > 0)
                {
                    decompressed.Write(buffer, 0, nRead);
                }

                return Convert.ToBase64String(decompressed.ToArray());
            }),
        };

        public IEnumerable<GlobalMethod> GetMethods() => new[] { _base64, _html, _gZip };
    }
}
