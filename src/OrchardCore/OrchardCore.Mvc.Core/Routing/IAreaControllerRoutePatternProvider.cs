using System;

namespace OrchardCore.Mvc.Routing
{
    public interface IAreaControllerRoutePatternProvider
    {
        public string Pattern { get; }
        public string ControllerName { get; }
        public Type AttributeType { get; }
    }
}
