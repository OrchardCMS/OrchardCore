using System;
using Microsoft.AspNetCore.WebUtilities;

namespace OrchardCore.Secrets
{
    public class RsaSecret : Secret
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }

    public static class RsaSecretExtensions
    {
        public static byte[] PublicKeyAsBytes(this RsaSecret rsaSecret)
        {
            if (!String.IsNullOrEmpty(rsaSecret.PublicKey))
            {
                return WebEncoders.Base64UrlDecode(rsaSecret.PublicKey);
            }

            return Array.Empty<byte>();
        }

        public static byte[] PrivateKeyAsBytes(this RsaSecret rsaSecret)
        {
            if (!String.IsNullOrEmpty(rsaSecret.PrivateKey))
            {
                return WebEncoders.Base64UrlDecode(rsaSecret.PrivateKey);
            }

            return Array.Empty<byte>();
        }
    }
}
