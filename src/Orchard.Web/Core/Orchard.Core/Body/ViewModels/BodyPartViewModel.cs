using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Core.Body.Model;

namespace Orchard.Core.Body.ViewModels
{
    public class BodyPartViewModel
    {
        public string Body { get; set; }

        [BindNever]
        public BodyPart BodyPart { get; set; }
    }
}
