using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.AzureAI.Models;
using OrchardCore.Secrets;

namespace OrchardCore.AzureAI.ViewModels;

public class AzureAISearchDefaultSettingsViewModel
{
    [Required]
    public AzureAIAuthenticationType? AuthenticationType { get; set; }

    public string Endpoint { get; set; }

    public SecretInputViewModel ApiKey { get; set; } = new();

    public string IdentityClientId { get; set; }

    public bool UseCustomConfiguration { get; set; }

    [BindNever]
    public IList<SelectListItem> AuthenticationTypes { get; set; }

    [BindNever]
    public bool ConfigurationsAreOptional { get; set; }
}
