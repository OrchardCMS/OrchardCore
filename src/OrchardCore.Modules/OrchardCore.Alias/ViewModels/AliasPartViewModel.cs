using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Settings;
using OrchardCore.ContentManagement;

namespace OrchardCore.Alias.ViewModels
{
    public class AliasPartViewModel
    {
        public string Alias { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public AliasPart AliasPart { get; set; }

        [BindNever]
        public AliasPartSettings Settings { get; set; }
    }
}
