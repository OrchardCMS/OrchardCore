using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Razor;

namespace OrchardVNext.Mvc {
    public class ModuleViewLocationExpander : IViewLocationExpander {
        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context) {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations) {
            Logger.Debug("Expanding search paths");
            return ExpandViewLocationsCore(viewLocations);
        }

        private IEnumerable<string> ExpandViewLocationsCore(IEnumerable<string> viewLocations) {
            foreach (var location in viewLocations) {
                yield return location.Replace("/Areas/", "/Modules/");
                yield return location;
            }
        }
    }
}