using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Secrets.ViewModels;

public class RsaSecretViewModel
{
    public string PublicKey { get; set; }

    public string PrivateKey { get; set; }

    public string NewPublicKey { get; set; }

    public string NewPrivateKey { get; set; }

    public bool IncludesPrivateKey { get; set; }

    public bool HasNewKeys { get; set; }

    public bool IsNew { get; set; }

    public List<SelectListItem> KeyTypes { get; set; } = [];
}
