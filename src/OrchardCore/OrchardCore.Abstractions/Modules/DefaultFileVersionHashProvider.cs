using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace OrchardCore.Abstractions.Modules
{
    public class DefaultFileVersionHashProvider : IFileVersionHashProvider
    {
        public string GetFileVersionHash(Stream fileStream)
        {
            using (var sha256 = CreateSHA256())
            {
                var hash = sha256.ComputeHash(fileStream);
                return WebEncoders.Base64UrlEncode(hash);
            }
        }

        private static SHA256 CreateSHA256()
        {
            try
            {
                return SHA256.Create();
            }
            // SHA256.Create is documented to throw this exception on FIPS compliant machines.
            // See: https://msdn.microsoft.com/en-us/library/z08hz7ad%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
            catch (System.Reflection.TargetInvocationException)
            {
                // Fallback to a FIPS compliant SHA256 algorithm.
                return new SHA256CryptoServiceProvider();
            }
        }
    }
}
