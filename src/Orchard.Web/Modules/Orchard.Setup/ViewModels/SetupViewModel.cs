using Orchard.Setup.Annotations;

namespace Orchard.Setup.ViewModels {
    public class SetupViewModel {
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }
    }
}
