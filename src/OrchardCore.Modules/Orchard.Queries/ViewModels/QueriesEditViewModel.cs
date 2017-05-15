using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.Queries.ViewModels
{
    public class QueriesEditViewModel : QueriesCreateViewModel
    {
        public string Name { get; set; }
    }
}
