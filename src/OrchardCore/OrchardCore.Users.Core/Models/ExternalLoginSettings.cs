namespace OrchardCore.Users.Models;

public class ExternalLoginSettings
{
    public bool UseExternalProviderIfOnlyOneDefined { get; set; }

    public bool UseScriptToSyncProperties { get; set; }

    public string SyncPropertiesScript { get; set; }
}
