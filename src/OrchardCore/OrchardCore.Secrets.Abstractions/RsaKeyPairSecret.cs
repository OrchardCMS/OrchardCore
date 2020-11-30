using System;

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
                return Convert.FromBase64String(rsaKeyPair.PrivateKey);
            }

            return Array.Empty<byte>();
        }
    }
}
