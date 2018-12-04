using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Queries.ViewModels
{
    public class EditQueryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Schema { get; set; }

        [BindNever]
        public Query Query { get; set; }
    }
}
