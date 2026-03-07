using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Email.Azure.ViewModels;

public class AzureEmailSettingsViewModel
{
    [EmailAddress]
    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    [BindNever]
    public bool HasConnectionString { get; set; }
}
