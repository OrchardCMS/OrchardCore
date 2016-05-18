using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Core.Title.Model;

namespace Orchard.Core.Title.ViewModels
{
    public class TitlePartViewModel
    {
        public string Title { get; set; }

        [BindNever]
        public TitlePart TitlePart { get; set; }
    }
}
