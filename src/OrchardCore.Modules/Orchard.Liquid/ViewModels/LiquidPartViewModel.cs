using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Liquid.Model;

namespace Orchard.Liquid.ViewModels
{
    public class LiquidPartViewModel
    {
        public string Liquid { get; set; }

        [BindNever]
        public LiquidPart LiquidPart { get; set; }        
    }
}
