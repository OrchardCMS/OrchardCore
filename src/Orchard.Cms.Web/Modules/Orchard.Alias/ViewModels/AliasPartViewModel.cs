using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Alias.Models;
using Orchard.Alias.Settings;

namespace Orchard.Alias.ViewModels
{
    public class AliasPartViewModel
    {
        public string Alias { get; set; }

        [BindNever]
        public AliasPart AliasPart { get; set; }

        [BindNever]
        public AliasPartSettings Settings { get; set; }
    }
}
