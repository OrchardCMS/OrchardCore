using OrchardVNext.Abstractions.Localization;

namespace OrchardVNext.DependencyInjection
{
    public abstract class Component : IDependency {
        protected Component() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}
