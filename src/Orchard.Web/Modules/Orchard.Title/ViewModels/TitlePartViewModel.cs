using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Title.Model;

namespace Orchard.Title.ViewModels
{
    public class TitlePartViewModel
    {
        public string Title { get; set; }

        [BindNever]
        public TitlePart TitlePart { get; set; }
    }
}
