using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Queries.ViewModels
{
    public class QueriesEditViewModel : QueriesCreateViewModel
    {
        public string Name { get; set; }
    }
}
