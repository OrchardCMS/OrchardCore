using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Email.Azure.ViewModels;

public class AzureEmailSettingsViewModel
{
    public bool IsEnabled { get; set; }

    [Required(AllowEmptyStrings = false), EmailAddress]
    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    public bool ConfigurationExists { get; set; }

    public bool FileConfigurationExists { get; set; }

    public bool PreventUIConnectionChange { get; set; }
}
