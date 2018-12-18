using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public interface IPlacementParseFilterProvider
    {
        string Key { get; }
        bool IsMatch(ShapePlacementContext context, JToken expression);
    }
}
