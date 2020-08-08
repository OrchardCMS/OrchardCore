
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Secrets.ViewModels
{
    public class AuthorizationSecretViewModel
    {
        public string AuthenticationString { get; set; }
        [BindNever]
        public BuildEditorContext Context { get; set; }
    }
}
