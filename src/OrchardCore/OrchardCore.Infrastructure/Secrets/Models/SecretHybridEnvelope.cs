using System;

namespace OrchardCore.Secrets.Models;

public class SecretHybridEnvelope
{
    public string Key { get; set; }
    public string Iv { get; set; }
    public string ProtectedData { get; set; }
    public string Signature { get; set; }
    public string EncryptionSecret { get; set; }
    public string SigningSecret { get; set; }
    public DateTime? ExpirationUtc { get; set; }
}
