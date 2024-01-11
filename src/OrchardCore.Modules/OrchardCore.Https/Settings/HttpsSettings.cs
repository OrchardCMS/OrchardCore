namespace OrchardCore.Https.Settings
{
    public class HttpsSettings
    {
        public bool EnableStrictTransportSecurity { get; set; }
        public bool RequireHttps { get; set; }
        public bool RequireHttpsPermanent { get; set; }
        public int? SslPort { get; set; }
    }
}
