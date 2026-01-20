namespace OrchardCore.Facebook.Settings
{
    public class FacebookSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public bool FBInit { get; set; } = false;

        public string FBInitParams { get; set; } = @"status:true,
xfbml:true,
autoLogAppEvents:true";

        public string SdkJs { get; set; } = "sdk.js";
        public string Version { get; set; } = "v3.2";
    }
}
