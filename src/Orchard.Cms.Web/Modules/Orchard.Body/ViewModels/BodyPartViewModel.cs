using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Body.Model;
using Orchard.Body.Settings;

namespace Orchard.Body.ViewModels
{
    public class BodyPartViewModel
    {
        public string Body { get; set; }

        [BindNever]
        public BodyPart BodyPart { get; set; }

        [BindNever]
        public BodyPartSettings TypePartSettings { get; set; }
    }
}
