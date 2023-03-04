using System;
using System.Collections.Generic;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Testing;

public class ShapeBindingsDictionary : Dictionary<string, ShapeBinding>
{
    public ShapeBindingsDictionary()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }
}
