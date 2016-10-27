using System.ComponentModel.DataAnnotations;

namespace Orchard.Tenants.ViewModels
{
    public class EditTenantViewModel
    {
        public string Name { get; set; }

        public string DatabaseProvider { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        public string RequestUrlPrefix { get; set; }
        public string RequestUrlHost { get; set; }

    }
}
