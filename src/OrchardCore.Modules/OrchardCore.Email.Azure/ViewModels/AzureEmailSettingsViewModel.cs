using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Email.Azure.ViewModels;

public class AzureEmailSettingsViewModel
{
    public bool IsEnabled { get; set; }

    [EmailAddress]
    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    [BindNever]
    public bool HasConnectionString { get; set; }
}
