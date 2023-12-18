using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.ViewModels;

public class SecretInfoViewModel
{
    public string Name { get; set; }
    public string SelectedStore { get; set; }
    public string Description { get; set; }
    public dynamic Editor { get; set; }
    public string Type { get; set; }

    [BindNever]
    public IReadOnlyCollection<SecretStoreInfo> StoreInfos { get; set; }
}
