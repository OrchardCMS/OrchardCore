using System;
using Microsoft.AspNetCore.WebUtilities;

namespace OrchardCore.Secrets
{
    public class RsaKeyPair : RsaPublicKeySecret
    {
        public string PrivateKey { get; set; }
    }

    public static class RsaKeyPairSecretExtensions
    {

        public static byte[] PrivateKeyAsBytes(this RsaKeyPair rsaKeyPair)
        {
            if (!String.IsNullOrEmpty(rsaKeyPair.PrivateKey))
            {
                return WebEncoders.Base64UrlDecode(rsaKeyPair.PrivateKey);
            }

            return Array.Empty<byte>();
        }
    }
}
