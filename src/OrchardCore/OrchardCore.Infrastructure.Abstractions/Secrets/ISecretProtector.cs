using System;

namespace OrchardCore.Secrets;

public interface ISecretProtector
{
    string Protect(string plainText);
    string Protect(string plainText, DateTime? expirationUtc);
}
