using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Title.Models;

namespace OrchardCore.Title.ViewModels
{
    public class TitlePartViewModel
    {
        public string Title { get; set; }

        [BindNever]
        public TitlePart TitlePart { get; set; }
    }
}
