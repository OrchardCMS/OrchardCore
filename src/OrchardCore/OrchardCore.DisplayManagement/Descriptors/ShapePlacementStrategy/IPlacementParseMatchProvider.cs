using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy
{
    public interface IPlacementParseMatchProvider
    {
        string Key { get; }
        bool Match(ShapePlacementContext context, string expression);
    }
}
