namespace OrchardCore.Facebook.Recipes;

public sealed class FacebookCoreSettingsStepModel
{
    public string AppId { get; set; }
    public string AppSecret { get; set; }
    public string SdkJs { get; set; }
    public bool FBInit { get; set; }
    public string FBInitParams { get; set; }
    public string Version { get; set; }
}
