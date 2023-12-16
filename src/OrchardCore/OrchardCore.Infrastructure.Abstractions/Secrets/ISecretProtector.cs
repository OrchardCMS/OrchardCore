using System;

namespace OrchardCore.Secrets;

public interface ISecretProtector
{
    string Protect(string plaintext);
    string Protect(string plaintext, DateTimeOffset expiration);
}
