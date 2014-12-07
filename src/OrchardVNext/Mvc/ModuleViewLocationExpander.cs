using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;

namespace OrchardVNext.Mvc {
        /// <summary>
        /// A <see cref="IViewLocationExpander"/> that replaces adds the language as an extension prefix to view names.
        /// </summary>
        /// <example>
        /// For the default case with no areas, views are generated with the following patterns (assuming controller is
        /// "Home", action is "Index" and language is "en")
        /// Views/Home/en/Action
        /// Views/Home/Action
        /// Views/Shared/en/Action
        /// Views/Shared/Action
        /// </example>
        public class ModuleViewLocationExpander : IViewLocationExpander {
            private const string ValueKey = "modules";

            /// <inheritdoc />
            public void PopulateValues(ViewLocationExpanderContext context) {
                //var value = _valueFactory(context.ActionContext);
                //if (!string.IsNullOrEmpty(value)) {
                //    context.Values[ValueKey] = value;
                //}
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