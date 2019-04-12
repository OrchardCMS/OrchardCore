namespace OrchardCore.Facebook.Settings
{
    public class FacebookSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public bool FBInit { get; set; } = false;
        public string FBInitParams { get; set; } = "status:true/r/n,xfbml:true";
        public string Version { get; set; } = "v3.2";
    }
}
