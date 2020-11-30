using System;
using Microsoft.AspNetCore.WebUtilities;

namespace OrchardCore.Secrets
{
    public class RsaPublicKeySecret : Secret
    {
        public string PublicKey { get; set; }
    }

    public static class RsaPublicKeySecretExtensions
    {
        public static byte[] PublicKeyAsBytes(this RsaPublicKeySecret rsaPublickKeySecret)
        {
            if (!String.IsNullOrEmpty(rsaPublickKeySecret.PublicKey))
            {
                return WebEncoders.Base64UrlDecode(rsaPublickKeySecret.PublicKey);
            }

            return Array.Empty<byte>();
        }
    }
}
