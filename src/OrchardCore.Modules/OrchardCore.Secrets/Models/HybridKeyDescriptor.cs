namespace OrchardCore.Secrets.Models;

public class HybridKeyDescriptor
{
    public string Key { get; set; }
    public string Iv { get; set; }
    public string ProtectedData { get; set; }
    public string Signature { get; set; }
    public string EncryptionSecret { get; set; }
    public string SigningSecret { get; set; }
}
