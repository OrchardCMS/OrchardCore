using OrchardVNext.DependencyInjection;
using OrchardVNext.Localization;

namespace OrchardVNext
{
    public abstract class Component : IDependency {
        protected Component() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}
