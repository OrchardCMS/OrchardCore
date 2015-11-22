using Orchard.Setup.Annotations;

namespace Orchard.Setup.ViewModels
{
    public class SetupViewModel
    {
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }
        public string DatabaseProvider { get; set; }
        public string ConnectionString { get; set; }
        public string TablePrefix { get; set; }
    }
}