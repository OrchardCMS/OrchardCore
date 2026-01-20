using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.ViewModels;

public class AzureAISearchDefaultSettingsViewModel
{
    [Required]
    public AzureAIAuthenticationType? AuthenticationType { get; set; }

    public string Endpoint { get; set; }

    public string ApiKey { get; set; }

    public string IdentityClientId { get; set; }

    public bool UseCustomConfiguration { get; set; }

    [BindNever]
    public IList<SelectListItem> AuthenticationTypes { get; set; }

    [BindNever]
    public bool ConfigurationsAreOptional { get; set; }

    [BindNever]
    public bool ApiKeyExists { get; set; }
}
