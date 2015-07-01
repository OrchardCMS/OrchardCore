using OrchardVNext.Setup.Annotations;

namespace OrchardVNext.Setup.ViewModels {
    public class SetupViewModel {
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }
    }
}
