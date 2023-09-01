
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.ViewModels;

public class RsaSecretViewModel
{
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public RSAKeyType KeyType { get; set; }
    public bool HasNewKeys { get; set; }
    public string NewPublicKey { get; set; }
    public string NewPrivateKey { get; set; }

    [BindNever]
    public List<SelectListItem> KeyTypes { get; set; }

    [BindNever]
    public BuildEditorContext Context { get; set; }
}
