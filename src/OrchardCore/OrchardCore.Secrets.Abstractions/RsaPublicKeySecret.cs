using System;

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
                return Convert.FromBase64String(rsaPublickKeySecret.PublicKey);
            }

            return Array.Empty<byte>();
        }
    }
}
