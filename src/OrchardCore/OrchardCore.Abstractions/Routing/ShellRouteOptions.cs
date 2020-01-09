using System.Collections.Generic;

namespace OrchardCore.Abstractions.Routing
{
    public class ShellRouteOptions
    {
        public IDictionary<string, string> AdminAreaRouteMap { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> AreaRouteMap { get; set; } = new Dictionary<string, string>();
    }
}
