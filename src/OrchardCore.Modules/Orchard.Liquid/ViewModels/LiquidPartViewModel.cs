using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.ContentManagement;
using Orchard.Liquid.Model;

namespace Orchard.Liquid.ViewModels
{
    public class LiquidPartViewModel
    {
        public string Liquid { get; set; }
        public string Html { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public LiquidPart LiquidPart { get; set; }
    }
}
