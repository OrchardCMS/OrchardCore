using System;

namespace OrchardCore.Mvc.Routing
{
    public interface IAreaControllerRoutePrefixProvider
    {
        public string Prefix { get; }
        public string ControllerName { get; }
        public Type AttributeType { get; }
    }
}
