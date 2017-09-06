using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Queries.Sql.ViewModels
{
    public class SqlQueryViewModel
    {
        [Required]
        public string Query { get; set; }

        public bool ReturnDocuments { get; set; }
    }
}
