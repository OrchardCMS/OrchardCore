using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrchardCore.DisplayManagement.TagHelpers
{
        public interface ITagHelpersProvider
    {
        IEnumerable<TypeInfo> Types { get; }
    }
}
