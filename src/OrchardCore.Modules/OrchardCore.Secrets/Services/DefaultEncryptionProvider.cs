using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrchardCore.Secrets.Services
{
    public class DefaultEncryptionProvider : IEncryptionProvider
    {
        private readonly ISecretService<RsaSecret> _rsaSecretService;

        public DefaultEncryptionProvider(ISecretService<RsaSecret> rsaSecretService)
        {
            _rsaSecretService = rsaSecretService;
        }

        public async Task<IEncryptor> CreateAsync(string secretName)
        {
            var secret = await _rsaSecretService.GetSecretAsync(secretName);
            if (secret == null)
            {
                throw new InvalidOperationException("Secret not found " + secretName);
            }

            if (secret.KeyType != RsaSecretType.PublicPrivatePair)
            {
                throw new InvalidOperationException("Secret provides decryption only and cannot be used for encryption");
            }

            using var rsa = RSA.Create();
            rsa.KeySize = 2048;

            rsa.ImportSubjectPublicKeyInfo(secret.PublicKeyAsBytes(), out _);
            // TODO  compute a HMAC of the encrypted payload, append it to the resulting string and use it when decrypting the payload to ensure the ciphertext was not tampered with)
            using var aes = Aes.Create();

            // TODO apparently we should create an inv for every encryption.
            // with var iv = aes.GenerateId();

            // Create an encryptor to perform the stream transform.
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            var key = rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
            var iv = rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1);

            return new DefaultEncryptor(encryptor, secretName, key, iv);
        }
    }
}
