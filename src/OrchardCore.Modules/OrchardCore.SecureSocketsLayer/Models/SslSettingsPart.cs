namespace OrchardCore.SecureSocketsLayer.Models {
    public class SslSettings {
        public bool RequireHttps { get; set; }
        //public string Urls { get; set; }
        //public bool SecureEverything { get; set; }
        //public bool CustomEnabled { get; set; }
        //public string SecureHostName { get; set; }
        //public string InsecureHostName { get; set; }
        public int? SslPort { get; set; }
        public bool RequireHttpsPermanent { get; set; }
    }
}