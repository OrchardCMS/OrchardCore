namespace OrchardCore.Facebook.Settings
{
    public class FacebookSettings
    {
        public const string DEFAULT_VERSION = "v3.2";
        public string AppId { get; set; }
        public string AppSecret{ get; set; }
        public bool FBInit { get; set; }
        public string Version { get; set; }
        public string InitParams { get; set; }
    }
}
