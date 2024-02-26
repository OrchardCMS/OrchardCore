using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment
{
    public class DeploymentTarget
    {
        public DeploymentTarget(LocalizedString name, LocalizedString description, RouteValueDictionary route)
        {
            Name = name;
            Description = description;
            Route = route;
        }

        public IEnumerable<SelectListItem> Formats { get; set; }

        public LocalizedString Name { get; }
        public LocalizedString Description { get; }
        public RouteValueDictionary Route { get; }
    }
}
