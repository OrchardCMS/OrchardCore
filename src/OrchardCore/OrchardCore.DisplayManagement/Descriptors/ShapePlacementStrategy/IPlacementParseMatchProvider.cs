using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public interface IPlacementParseMatchProvider
    {
        string Key { get; }
        bool Match(ShapePlacementContext context, JToken expression);
    }
}
