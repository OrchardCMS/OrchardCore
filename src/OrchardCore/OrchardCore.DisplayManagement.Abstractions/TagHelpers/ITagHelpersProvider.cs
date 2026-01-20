using System;
using System.Collections.Generic;

namespace OrchardCore.DisplayManagement.TagHelpers;

public interface ITagHelpersProvider
{
    IEnumerable<Type> GetTypes();
}
