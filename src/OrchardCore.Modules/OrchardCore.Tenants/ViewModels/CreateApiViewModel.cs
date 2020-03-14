using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Tenants.ViewModels
{
    public class CreateApiViewModel
    {
        public string Description { get; set; }

        [Required]
        public string Name { get; set; }

        public string DatabaseProvider { get; set; }

        public string RequestUrlPrefix { get; set; }

        public string RequestUrlHost { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        public string RecipeName { get; set; }
    }
}
