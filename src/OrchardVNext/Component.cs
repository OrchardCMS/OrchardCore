using OrchardVNext.Abstractions.Localization;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext
{
    public abstract class Component : IDependency {
        protected Component() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}
