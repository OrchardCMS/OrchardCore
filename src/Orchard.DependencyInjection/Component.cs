using Orchard.Localization;

namespace Orchard.DependencyInjection
{
    public abstract class Component : IDependency
    {
        protected Component()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}