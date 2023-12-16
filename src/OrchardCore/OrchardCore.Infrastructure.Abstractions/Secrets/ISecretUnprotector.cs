using System;

namespace OrchardCore.Secrets;

public interface ISecretUnprotector
{
    string Unprotect();
    string Unprotect(out DateTimeOffset expiration);
}
