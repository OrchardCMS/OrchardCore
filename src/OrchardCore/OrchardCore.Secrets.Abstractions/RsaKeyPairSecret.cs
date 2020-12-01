using System;

namespace OrchardCore.Secrets
{
    public class RsaKeyPairSecret : RsaPublicKeySecret
    {
        public string PrivateKey { get; set; }
    }

    public static class RsaKeyPairSecretExtensions
    {

        public static byte[] PrivateKeyAsBytes(this RsaKeyPairSecret rsaKeyPairSecret)
        {
            if (!String.IsNullOrEmpty(rsaKeyPairSecret.PrivateKey))
            {
                return Convert.FromBase64String(rsaKeyPairSecret.PrivateKey);
            }

            return Array.Empty<byte>();
        }
    }
}
