namespace OrchardCore.Facebook.ViewModels;

public class FacebookPixelSettingsViewModel
{
    public string PixelId { get; set; }

    public string ConversionsApiAccessToken { get; set; }

    public string ConversionsApiTestEventCode { get; set; }

    public bool HasDecryptionError { get; set; }
}
