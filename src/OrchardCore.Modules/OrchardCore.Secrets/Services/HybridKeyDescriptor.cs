namespace OrchardCore.Secrets.Services
{
    internal class HybridKeyDescriptor
    {
        public string SecretName { get; set; }
        public string Key { get; set; }
        public string Iv { get; set; }
        public string ProtectedData { get; set; }
    }
}
